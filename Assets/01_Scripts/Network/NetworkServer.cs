using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

public class UserData
{
	public string UserName;
}

public class NetworkServer : IDisposable
{
	public delegate void UserChanged(ulong clientId, UserData userData);

	public event UserChanged OnUserJoin;
	public event UserChanged OnUserLeft;

	private NetworkManager _networkManager;
	private NetworkObject _playerPrefab;

	private Dictionary<ulong, UserData> _clientIdToUserDataDictionary = new Dictionary<ulong, UserData>();

	private List<PlayerAgent> _spawnedPlayerList;

	public NetworkServer(NetworkObject playerPrefab)
	{
		_playerPrefab = playerPrefab;

		_networkManager = NetworkManager.Singleton;
		_networkManager.ConnectionApprovalCallback += HandleConnectionApprovalCallback;
		_networkManager.OnServerStarted += HandleServerStarted;

		_spawnedPlayerList = new List<PlayerAgent>();

		_clientIdToUserDataDictionary.Add(256, new UserData() { UserName = "Kill Zone" });
	}

	private void HandleConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest req, NetworkManager.ConnectionApprovalResponse res)
	{
		// 접속한 클라이언트의 정보를 가져온다
		string json = Encoding.UTF8.GetString(req.Payload);
		UserData userData = JsonUtility.FromJson<UserData>(json);

		_clientIdToUserDataDictionary[req.ClientNetworkId] = userData;

		res.Approved = true;
		res.CreatePlayerObject = false;
	}

	private void HandleServerStarted()
	{
		_networkManager.OnClientConnectedCallback += HandleClientConnect;
		_networkManager.OnClientDisconnectCallback += HandleClientDisconnect;
	}

	private void HandleClientConnect(ulong clientId)
	{
		GameManager.Instance.AddUser(clientId);

		if (_clientIdToUserDataDictionary.TryGetValue(clientId, out UserData data))
		{
			OnUserJoin?.Invoke(clientId, data);
		}
	}

	private void HandleClientDisconnect(ulong clientId)
	{
		GameManager.Instance.RemoveUser(clientId);

		if (_clientIdToUserDataDictionary.TryGetValue(clientId, out UserData data))
		{
			OnUserLeft?.Invoke(clientId, data);
			_clientIdToUserDataDictionary.Remove(clientId);
		}
	}

	/// <summary>
	/// 클라이언트 Id를 바탕으로 플레이어 오브젝트를 생성한다
	/// </summary>
	/// <param name="clientId">생성할 클라이언트의 Id</param>
	/// <param name="pos">생성할 위치</param>
	public void RespawnPlayer(ulong clientId, Vector3 pos = default(Vector3))
	{
		// 이미 생성된 플레이어 오브젝트가 있다면 생성하지 않음
		if (NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject != null) return;

		NetworkObject player = GameObject.Instantiate(_playerPrefab, pos, Quaternion.identity);
		player.SpawnAsPlayerObject(clientId, true);

		PlayerAgent agent = player.GetComponent<PlayerAgent>();
		agent.WeaponHolder.ChangeWeaponServerRpc(GameManager.Instance.Weapon);
		agent.UserName.Value = GetUserDataByClientID(clientId).UserName;
		_spawnedPlayerList.Add(agent);
	}

	/// <summary>
	/// 만약 플레이어가 죽었다면 스폰된 플레이어 리스트에서 제거한다
	/// </summary>
	/// <param name="clientId">제거할 클라이언트의 Id</param>
	public void PlayerDespawned(ulong clientId)
	{
		PlayerAgent player = _spawnedPlayerList.Find(x => x.OwnerClientId == clientId);

		if (player != null)
			_spawnedPlayerList.Remove(player);
	}

	/// <summary>
	/// 스폰되어 있는 모든 플레이어 죽이기. 게임 종료 등에 사용
	/// </summary>
	public void KillAllPlayer(bool immediately = false)
	{
		foreach (PlayerAgent player in _spawnedPlayerList)
		{
			player?.Health.SetDealer(256);

			if (immediately)
				player?.KillImmediately();
			else
				player?.Kill();
		}

		_spawnedPlayerList.Clear();
	}

	/// <summary>
	/// 클라이언트 Id를 통해 클라이언트의 정보를 가져온다
	/// </summary>
	public UserData GetUserDataByClientID(ulong clientID)
	{
		if (_clientIdToUserDataDictionary.TryGetValue(clientID, out UserData userData))
		{
			return userData;
		}

		return null;
	}

	public void Dispose()
	{
		if (_networkManager == null) return;
		_networkManager.ConnectionApprovalCallback -= HandleConnectionApprovalCallback;
		_networkManager.OnServerStarted -= HandleServerStarted;
		_networkManager.OnClientConnectedCallback -= HandleClientConnect;
		_networkManager.OnClientDisconnectCallback -= HandleClientDisconnect;

		if (_networkManager.IsListening)
		{
			_networkManager.Shutdown();
		}
	}
}
