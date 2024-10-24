using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using Define;

public enum GAME_MODE
{
	TeamDeathMatch,
}

public class BattleManager : NetworkSingleton<BattleManager>
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

	// references
	public WeaponManager WeaponManager { get; private set; }

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
	public event Action<GAME_MODE> OnGameStarted;

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

		if (IsOwner) // ������ ��쿡�� ������ ��û�� �� �ִ�.
		{
			WeaponManager = GetComponent<WeaponManager>();
			UIManager.UIViewManager.GetView<GameReadyView>().OnPlayButtonClick += SpawnPlayer;
		}

		UIManager.SceneUIViewManager.HideView<LoadingView>(); // OnNetworkSpawn�� ����� �� Game ���� ��� ��Ұ� Ȱ��ȭ�Ǳ� ������ LoadingView�� �����ִ� �� ���� ���⼭ ����
	}

	public override void OnNetworkDespawn()
	{
		base.OnNetworkDespawn();

		if (IsHost)
			NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
	}

	private void Start()
	{
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
		// ���� �ʱ�ȭ
		switch (_gameMode)
		{
			case GAME_MODE.TeamDeathMatch:
				_redScore.Value = 0;
				_blueScore.Value = 0;
				break;
			default:
				throw new System.Exception("AAAAh ���� ��� �ʱ�ȭ ����!");
		}

		// Ŭ���̾�Ʈ���� �ʱ�ȭ�� �͵� (UI ���� ��)
		InitializeGameClientRpc(_gameMode);
		UpdateScoreClientRpc(_redScore.Value, _blueScore.Value);
	}

	[ClientRpc(RequireOwnership = false)]
	private void InitializeGameClientRpc(GAME_MODE gameMode)
	{
		UIManager.UIViewManager.GetView<GameReadyView>().SetIntermission(false, 0);
		//UIManager.UIViewManager.GetView<GamePlayView>().SetGameModeUI(this, gameMode);

		OnGameStarted?.Invoke(_gameMode);
	}

	private void UpdateGame()
	{
		if (_isGameRunning)
		{
			// ���� �ð� ������Ʈ
			int leftTime = _gameDurationSec - (int)Time.unscaledTime + _gameStartTime;
			if (leftTime < 0) // �ð��� �� �Ǿ��ٸ� ���� ����
			{
				leftTime = 0;
				EndGame();
			}
			UpdateGameClientRpc(leftTime);
		}
	}

	[ClientRpc(RequireOwnership = false)]
	private void UpdateGameClientRpc(int leftTime)
	{
		OnGameTimerCount?.Invoke(leftTime);
	}

	/// <summary>
	/// ���� �����Ű�� �Լ�
	/// </summary>
	public void EndGame()
	{
		if (!IsHost) return;

		_isSpawnable = false; // ���� �Ұ�
		_isGameRunning = false; // ���� ������Ʈ �ߴ�(���� �ð�, ���� ��)
		GameManager.Instance.NetworkServer.KillAllPlayer(true); // �����ִ� ��� �÷��̾� ó��

		StopAllCoroutines();
		StartCoroutine(IntermissionCo());
	}

	/// <summary>
	/// ���� ������ �����ϱ���� ���
	/// </summary>
	IEnumerator IntermissionCo()
	{
		float timer = _intermissionTime;
		
		while (timer > 0)
		{
			timer = Mathf.Clamp(timer - Time.deltaTime, 0, _intermissionTime);

			SetIntermissionTextClientRpc(true, Mathf.CeilToInt(timer));

			yield return null;
		}

		_rankBoard.ResetRankboard(); // ��ũ���� �ʱ�ȭ
		StartGame(); // ��� �ð��� ������ ���ο� ���� ����
	}

	[ClientRpc(RequireOwnership = false)]
	private void SetIntermissionTextClientRpc(bool active, int time)
	{
		UIManager.UIViewManager.GetView<GameReadyView>().SetIntermission(active, time);
	}

	#endregion


	#region Player
	/// <summary>
	/// �÷��̾� ����
	/// </summary>
	private void SpawnPlayer()
	{
		// ��û�� Ŭ���̾�Ʈ����, ������ �������� �Ѵ�
		// ���� Ŭ���̾�Ʈ���� �������ϴ� ������ ����Ŭ���̾�Ʈ�� Id, ���õ� ������ �̸�
		SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId, WeaponManager.WeaponName);
	}

	[ServerRpc(RequireOwnership = false)]
	private void SpawnPlayerServerRpc(ulong clientId, string weaponName)
	{
		if (!_isSpawnable) return; // ���� ���� ���� ��Ȳ���� ������ �Ұ���

		Transform spawnPos = GameManager.Instance.GetTeam(clientId) == TEAM_TYPE.Red ? _redTeamSpawn : _blueTeamSpawn;
		GameManager.Instance.NetworkServer.RespawnPlayer(clientId, spawnPos.position, weaponName);
	}
	
	/// <summary>
	/// �ٸ� �÷��̾ �׿��� ��
	/// </summary>
	private void OnPlayerKilled(string enemyName)
	{
		// ų �޽����� ����
		UIManager.UIViewManager.GetView<GamePlayView>().MessageManager.ShowMessage(MESSAGE_TYPE.Kill, enemyName);
	}

	private void HandlePlayerDie(object sender, PlayerAgent.PlayerEventArgs e)
	{
		if (e.Player.Health.LastHitClientId == NetworkManager.LocalClientId) // ���� Ŭ���̾�Ʈ�� ���� ���ΰ�?
		{
			OnPlayerKilled(e.Player.UserName.Value.ToString());
		}

		if (IsHost)
		{
			if (!_isGameRunning) return;

			if (GameManager.Instance.GetTeam(e.Player.OwnerClientId) == TEAM_TYPE.Red)
				_blueScore.Value += 1;
			else
				_redScore.Value += 1;

			_rankBoard?.AddKills(e.Player.Health.LastHitClientId, 1);
		}
	}

	/// <summary>
	/// ���, ���� �� �ϳ��� ������ ���� �Ǿ��� ��, ���� UI�� ���ΰ�ħ��
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

	/// <summary>
	/// ���� �÷��̾ �׾��ٸ�
	/// </summary>
	private void HandlePlayerDespawn(object sender, PlayerAgent.PlayerEventArgs e)
	{
		if (IsHost)
		{
			GameManager.Instance.NetworkServer.PlayerDespawned(e.Player.OwnerClientId); // NetworkServer�� �˸���
		}
	}

	/// <summary>
	/// ���ο� �÷��̾ �������� ��
	/// </summary>
	private void HandleClientConnected(ulong clientId)
	{
		InitializeGameClientRpc(_gameMode);
		UpdateScoreClientRpc(_redScore.Value, _blueScore.Value); // ������ �÷��̾��� UI�� ���� ������ �ݿ�

		Debug.Log("New player Connected");
	}
	#endregion
}
