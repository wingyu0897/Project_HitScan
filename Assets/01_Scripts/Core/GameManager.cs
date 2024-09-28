using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum TEAM_TYPE
{
	Red = 0,
	Blue,
}

public class GameManager : MonoSingleton<GameManager>
{
	[SerializeField] private NetworkObject _playerPrefab;

	private List<ulong> _redTeam;
	private List<ulong> _blueTeam;

	private Dictionary<ulong, TEAM_TYPE> _clientId2Team;

	public NetworkServer NetworkServer;

	protected override void Awake()
	{
		base.Awake();

		_redTeam = new List<ulong>();
		_blueTeam = new List<ulong>();
		_clientId2Team = new Dictionary<ulong, TEAM_TYPE>();

		DontDestroyOnLoad(gameObject);
	}

	#region Relay

	public async Task<string> CreateRelayGame(UserData userData)
	{
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
		AsyncOperation operation = SceneManager.LoadSceneAsync("Game");

		while (!operation.isDone)
			yield return null;

		RelayManager.Instance.JoinRelay(code, userData);
	}

	#endregion

	#region Battle

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
			return default(TEAM_TYPE);
		return _clientId2Team[clientId];
	}

	#endregion

	protected override void OnDestroy()
	{
		NetworkServer?.Dispose();

		base.OnDestroy();
	}
}
