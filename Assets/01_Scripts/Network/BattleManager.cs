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

		if (IsOwner) // 주인일 경우에만 스폰을 요청할 수 있다.
		{
			WeaponManager = GetComponent<WeaponManager>();
			UIManager.UIViewManager.GetView<GameReadyView>().OnPlayButtonClick += SpawnPlayer;
		}

		UIManager.SceneUIViewManager.HideView<LoadingView>(); // OnNetworkSpawn이 실행될 때 Game 씬의 모든 요소가 활성화되기 때문에 LoadingView를 내려주는 것 또한 여기서 실행
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
	/// 게임 모드를 지정한다.
	/// </summary>
	public void SelectGameMode()
	{
		_gameMode = GAME_MODE.TeamDeathMatch;
		_gameDurationSec = 30;
	}

	public void SelectMap()
	{
		// 맵 선택 만들어 줘
	}

	public void StartGame()
	{
		// Player가 남아있지 않도록 모두 제거
		GameManager.Instance.NetworkServer.KillAllPlayer(true);

		_gameStartTime = (int)Time.unscaledTime;

		SelectGameMode();
		SelectMap();
		InitializeGame();

		_isSpawnable = true;
		_isGameRunning = true;
	}

	/// <summary>
	/// 게임 초기화
	/// </summary>
	private void InitializeGame()
	{
		// 점수 초기화
		switch (_gameMode)
		{
			case GAME_MODE.TeamDeathMatch:
				_redScore.Value = 0;
				_blueScore.Value = 0;
				break;
			default:
				throw new System.Exception("AAAAh 게임 모드 초기화 에러!");
		}

		// 클라이언트에서 초기화할 것들 (UI 같은 거)
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
			// 남은 시간 업데이트
			int leftTime = _gameDurationSec - (int)Time.unscaledTime + _gameStartTime;
			if (leftTime < 0) // 시간이 다 되었다면 게임 종료
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
	/// 게임 종료시키는 함수
	/// </summary>
	public void EndGame()
	{
		if (!IsHost) return;

		_isSpawnable = false; // 스폰 불가
		_isGameRunning = false; // 게임 업데이트 중단(제한 시간, 점수 등)
		GameManager.Instance.NetworkServer.KillAllPlayer(true); // 남아있는 모든 플레이어 처리

		StopAllCoroutines();
		StartCoroutine(IntermissionCo());
	}

	/// <summary>
	/// 다음 게임을 시작하기까지 대기
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

		_rankBoard.ResetRankboard(); // 랭크보드 초기화
		StartGame(); // 대기 시간이 끝나면 새로운 게임 진행
	}

	[ClientRpc(RequireOwnership = false)]
	private void SetIntermissionTextClientRpc(bool active, int time)
	{
		UIManager.UIViewManager.GetView<GameReadyView>().SetIntermission(active, time);
	}

	#endregion


	#region Player
	/// <summary>
	/// 플레이어 스폰
	/// </summary>
	private void SpawnPlayer()
	{
		// 요청은 클라이언트에서, 생성은 서버에서 한다
		// 오너 클리이언트에서 보내야하는 정보는 오너클라이언트의 Id, 선택된 무기의 이름
		SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId, WeaponManager.WeaponName);
	}

	[ServerRpc(RequireOwnership = false)]
	private void SpawnPlayerServerRpc(ulong clientId, string weaponName)
	{
		if (!_isSpawnable) return; // 게임 종료 등의 상황에는 스폰이 불가함

		Transform spawnPos = GameManager.Instance.GetTeam(clientId) == TEAM_TYPE.Red ? _redTeamSpawn : _blueTeamSpawn;
		GameManager.Instance.NetworkServer.RespawnPlayer(clientId, spawnPos.position, weaponName);
	}
	
	/// <summary>
	/// 다른 플레이어를 죽였을 때
	/// </summary>
	private void OnPlayerKilled(string enemyName)
	{
		// 킬 메시지를 띄운다
		UIManager.UIViewManager.GetView<GamePlayView>().MessageManager.ShowMessage(MESSAGE_TYPE.Kill, enemyName);
	}

	private void HandlePlayerDie(object sender, PlayerAgent.PlayerEventArgs e)
	{
		if (e.Player.Health.LastHitClientId == NetworkManager.LocalClientId) // 로컬 클라이언트가 죽인 것인가?
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
	/// 블루, 레드 중 하나라도 점수가 변경 되었을 시, 점수 UI를 새로고침함
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
	/// 만약 플레이어가 죽었다면
	/// </summary>
	private void HandlePlayerDespawn(object sender, PlayerAgent.PlayerEventArgs e)
	{
		if (IsHost)
		{
			GameManager.Instance.NetworkServer.PlayerDespawned(e.Player.OwnerClientId); // NetworkServer에 알린다
		}
	}

	/// <summary>
	/// 새로운 플레이어가 접속했을 때
	/// </summary>
	private void HandleClientConnected(ulong clientId)
	{
		InitializeGameClientRpc(_gameMode);
		UpdateScoreClientRpc(_redScore.Value, _blueScore.Value); // 새로훈 플레이어의 UI에 현재 점수를 반영

		Debug.Log("New player Connected");
	}
	#endregion
}
