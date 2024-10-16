using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using Define;

public enum GAME_MODE
{
	TeamDeathMatch,
}

public class InGameManager : NetworkBehaviour
{
	[Header("GameMode")]
	private GAME_MODE _gameMode;
	private int _gameStartTime;
	private int _gameDurationSec;

	[SerializeField] private int _intermissionTime;

	[Header("Rankboard")]
	[SerializeField] private RankBoard _rankBoard;
    [SerializeField] private Transform _redTeamSpawn;
    [SerializeField] private Transform _blueTeamSpawn;

	// players
	private Players _players;

	// variables
	private NetworkVariable<int> _redScore = new NetworkVariable<int>();
	private NetworkVariable<int> _blueScore = new NetworkVariable<int>();

	// flags
	private bool _isSpawnable = false;
	private bool _isGameRunning = false;


	// events
	public event Action<int> OnGameTimerCount;
	public event Action<int/*Red*/, int/*Blue*/> OnScoreChanged;

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();

		if (IsClient)
		{
			_redScore.OnValueChanged += HandleScoreChanged;
			_blueScore.OnValueChanged += HandleScoreChanged;
		}

		if (IsHost)
		{
			NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
			StartGame();
		}

		print(NetworkManager.LocalClientId);
	}

	public override void OnNetworkDespawn()
	{
		base.OnNetworkDespawn();

		if (IsHost)
			NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
	}

	private void Start()
	{
		UIManager.UIViewManager.GetView<GameReadyView>().OnPlayButtonClick += SpawnPlayer;
		PlayerAgent.OnPlayerDie += HandlePlayerDie;
		PlayerAgent.OnPlayerDespawn += HandlePlayerDespawn;

		_players = new Players();
	}

	private void Update()
	{
		if (IsHost)
		{
			UpdateGame();
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();

		PlayerAgent.OnPlayerDie -= HandlePlayerDie;
		PlayerAgent.OnPlayerDespawn -= HandlePlayerDespawn;
	}

	#region Game

	/// <summary>
	/// ���� ��带 �����Ѵ�.
	/// </summary>
	public void SelectGameMode()
	{
		_gameMode = GAME_MODE.TeamDeathMatch;
		_gameDurationSec = 30;
	}

	public void SelectMap()
	{
		// �� ���� ����� ��
	}

	public void StartGame()
	{
		// Player�� �������� �ʵ��� ��� ����
		GameManager.Instance.NetworkServer.KillAllPlayer(true);

		_gameStartTime = (int)Time.unscaledTime;

		SelectGameMode();
		SelectMap();
		InitializeGame();

		_isSpawnable = true;
		_isGameRunning = true;
	}

	/// <summary>
	/// ���� �ʱ�ȭ
	/// </summary>
	private void InitializeGame()
	{
		switch (_gameMode)
		{
			case GAME_MODE.TeamDeathMatch:
				_redScore.Value = 0;
				_blueScore.Value = 0;
				break;
			default:
				throw new System.Exception("AAAAh ���� ��� �ʱ�ȭ ����!");
		}

		InitializeGameClientRpc(_gameMode);
		UpdateScoreClientRpc(_redScore.Value, _blueScore.Value);
	}

	[ClientRpc(RequireOwnership = false)]
	private void InitializeGameClientRpc(GAME_MODE gameMode)
	{
		UIManager.UIViewManager.GetView<GameReadyView>().SetIntermission(false, 0);
		UIManager.UIViewManager.GetView<GamePlayView>().SetGameModeUI(this, gameMode);
	}

	private void UpdateGame()
	{
		if (_isGameRunning)
		{
			int leftTime = _gameDurationSec - (int)Time.unscaledTime + _gameStartTime;
			if (leftTime < 0)
				EndGame();

			UpdateGameClientRpc(leftTime);
		}
	}

	[ClientRpc(RequireOwnership = false)]
	private void UpdateGameClientRpc(int leftTime)
	{
		OnGameTimerCount?.Invoke(leftTime);
	}

	public void EndGame()
	{
		_isSpawnable = false;
		_isGameRunning = false;
		GameManager.Instance.NetworkServer.KillAllPlayer(true);

		StopAllCoroutines();
		StartCoroutine(IntermissionCo());
	}

	IEnumerator IntermissionCo()
	{
		float timer = _intermissionTime;
		
		while (timer > 0)
		{
			timer -= Time.deltaTime;

			SetIntermissionTextClientRpc(true, Mathf.FloorToInt(timer));

			yield return null;
		}

		SetIntermissionTextClientRpc(true, 0);

		_rankBoard.ResetRankboard();
		StartGame();
	}

	[ClientRpc(RequireOwnership = false)]
	private void SetIntermissionTextClientRpc(bool active, int time)
	{
		UIManager.UIViewManager.GetView<GameReadyView>().SetIntermission(active, time);
	}

	#endregion


	#region Player
	private void SpawnPlayer()
	{
		SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
	}

	[ServerRpc(RequireOwnership = false)]
	private void SpawnPlayerServerRpc(ulong clientId)
	{
		if (!_isSpawnable) return;

		Transform spawnPos = GameManager.Instance.GetTeam(clientId) == TEAM_TYPE.Red ? _redTeamSpawn : _blueTeamSpawn;
		GameManager.Instance.NetworkServer.RespawnPlayer(clientId, spawnPos.position);
	}

	private void OnPlayerKilled(string enemyName)
	{
		UIManager.UIViewManager.GetView<GamePlayView>().MessageManager.ShowMessage(MESSAGE_TYPE.Kill, enemyName);
	}

	private void HandlePlayerDie(object sender, PlayerAgent.PlayerEventArgs e)
	{
		Debug.Log($"���� ��� ID : {e.Player.Health.LastHitClientId}, ���� ���̵� : {NetworkManager.LocalClientId}");
		if (e.Player.Health.LastHitClientId == NetworkManager.LocalClientId)
		{
			OnPlayerKilled(e.Player.UserName.Value.ToString());
		}

		if (IsHost)
		{
			if (!_isGameRunning) return;

			Debug.Log($"ClientId: {e.Player.Health.LastHitClientId}, UserName: {GameManager.Instance.NetworkServer.GetUserDataByClientID(e.Player.Health.LastHitClientId)?.UserName ?? "KillZone"}, Kill: 1");

			if (GameManager.Instance.GetTeam(e.Player.OwnerClientId) == TEAM_TYPE.Red)
				_blueScore.Value += 1;
			else
				_redScore.Value += 1;

			_rankBoard?.AddKills(e.Player.Health.LastHitClientId, 1);
		}
	}

	/// <summary>
	/// ���, ���� �� �ϳ��� ������ ���� �Ǿ��� ��, �̸� UI�� �ݿ��ϵ��� Rpc ����
	/// </summary>
	private void HandleScoreChanged(int previousValue, int newValue)
	{
		UpdateScoreClientRpc(_redScore.Value, _blueScore.Value);
	}

	[ClientRpc(RequireOwnership = false)]
	private void UpdateScoreClientRpc(int redScore, int blueScore)
	{
		OnScoreChanged?.Invoke(redScore, blueScore);
	}

	private void HandlePlayerDespawn(object sender, PlayerAgent.PlayerEventArgs e)
	{
		if (IsHost)
		{
			GameManager.Instance.NetworkServer.PlayerDespawned(e.Player.OwnerClientId);
		}
	}

	private void HandleClientConnected(ulong clientId)
	{
		InitializeGameClientRpc(_gameMode);
		UpdateScoreClientRpc(_redScore.Value, _blueScore.Value);

		Debug.Log("New player Connected");
	}
	#endregion
}
