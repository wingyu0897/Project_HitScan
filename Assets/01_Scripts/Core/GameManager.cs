using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Define;

public class GameManager : MonoSingleton<GameManager>
{
	[SerializeField] private NetworkObject _playerPrefab;

	private List<ulong> _redTeam;
	private List<ulong> _blueTeam;
	private Dictionary<ulong, TEAM_TYPE> _clientId2Team;

	public NetworkServer NetworkServer;

	public string UserName { get; set; }
	public string Weapon { get; set; } = "Sample";
	private bool _joinAsClient = false;

	protected override void Awake()
	{
		base.Awake();

		_redTeam = new List<ulong>();
		_blueTeam = new List<ulong>();
		_clientId2Team = new Dictionary<ulong, TEAM_TYPE>();

		//NetworkManager.Singleton.OnClientStarted += HandleClientStart;
		//NetworkManager.Singleton.OnServerStarted += HandleServerStart;

		DontDestroyOnLoad(gameObject);
	}

	private void Start()
	{
		// 모든 게임 시스템 셋업이 끝난 후 Main 씬으로 이동
		SceneManager.LoadScene("Main");
	}

	private void HandleServerStart()
	{
		if (_joinAsClient) return;
		UIManager.Get<SceneUIViewManager>().HideView<LoadingView>();
	}

	private void HandleClientStart()
	{
		if (!_joinAsClient) return;
		UIManager.Get<SceneUIViewManager>().HideView<LoadingView>();
	}

	#region Relay

	public async Task<string> CreateRelayGame(UserData userData)
	{
		_joinAsClient = false;

		//UIManager.SceneUIViewManager.ShowView<LoadingView>();
		UIManager.Get<SceneUIViewManager>().ShowView<LoadingView>();
		SceneManager.LoadScene("Game");

		NetworkServer = new NetworkServer(_playerPrefab);
		string relayCode = await RelayManager.Instance.CreateRelay(userData);

		return relayCode;
	}

	public void JoinRelayGame(string code, UserData userData)
	{
		StartCoroutine(JoinRelayCo(code, userData));
	}

	IEnumerator JoinRelayCo(string code, UserData userData)
	{
		_joinAsClient = true;

		//UIManager.SceneUIViewManager.ShowView<LoadingView>();
		UIManager.Get<SceneUIViewManager>().ShowView<LoadingView>();
		AsyncOperation operation = SceneManager.LoadSceneAsync("Game");

		while (!operation.isDone)
			yield return null;

		RelayManager.Instance.JoinRelay(code, userData);
	}

	/// <summary>
	/// 게임 퇴장
	/// </summary>
	public void LeaveRelayGame()
	{
		_redTeam.Clear();
		_blueTeam.Clear();
		_clientId2Team.Clear();

		//UIManager.SceneUIViewManager.ShowView<LoadingView>();
		StartCoroutine(LeaveRelayCo());
		//UIManager.SceneUIViewManager.HideView<LoadingView>();
	}

	IEnumerator LeaveRelayCo()
	{
		RelayManager.Instance.LeaveRelay();
		LobbyManager.Instance.LeaveLobby();
		NetworkServer?.Dispose();

		AsyncOperation operation = SceneManager.LoadSceneAsync("Main");

		while (!operation.isDone)
			yield return null;

		// 게임에서 퇴장하는 경우에는 이미 인증을 마친 상황이니 인증 화면이 아닌 로비 화면을 보여준다
		UIManager.Get<UIViewManager>().HideView<AuthenticationView>();
		UIManager.Get<UIViewManager>().ShowView<LobbyView>();
	}

	#endregion

	#region Battle
	// 클라리언트에서는 아래의 함수를 실행할 수 없다. 호스트 전용

	public void AddUser(ulong clientId)
	{
		if (_redTeam.Count > _blueTeam.Count)
		{
			_blueTeam.Add(clientId);
			_clientId2Team[clientId] = TEAM_TYPE.Blue;
		}
		else
		{
			_redTeam.Add(clientId);
			_clientId2Team[clientId] = TEAM_TYPE.Red;
		}

		foreach (var pair in _clientId2Team)
		{
			Debug.Log($"Player: {NetworkServer?.GetUserDataByClientID(pair.Key).UserName}, Team: {pair.Value}");
		}
	}

	public void RemoveUser(ulong clientId)
	{
		if (_clientId2Team.ContainsKey(clientId))
		{
			if (_clientId2Team[clientId] == TEAM_TYPE.Red)
			{
				_redTeam.Remove(clientId);
			}
			else
			{
				_blueTeam.Remove(clientId);
			}
			_clientId2Team.Remove(clientId);
		}
	}

	public bool IsAttackable(ulong clientId1, ulong clientId2)
	{
		return _clientId2Team[clientId1] != _clientId2Team[clientId2];
	}

	public TEAM_TYPE GetTeam(ulong clientId)
	{
		if (!_clientId2Team.ContainsKey(clientId))
		{
			Debug.LogWarning("Is not Host or not contains key.");
			return default(TEAM_TYPE);
		}

		return _clientId2Team[clientId];
	}

	#endregion

	protected override void OnDestroy()
	{
		NetworkServer?.Dispose();

		base.OnDestroy();
	}
}
