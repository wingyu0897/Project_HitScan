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
	}

	private void HandleConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest req, NetworkManager.ConnectionApprovalResponse res)
	{
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

	public void RespawnPlayer(ulong clientId, Vector3 pos = default(Vector3))
	{
		if (NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject != null) return;

		NetworkObject player = GameObject.Instantiate(_playerPrefab, pos, Quaternion.identity);
		player.SpawnAsPlayerObject(clientId);
		_spawnedPlayerList.Add(player.GetComponent<PlayerAgent>());
	}

	public void KillAllPlayer()
	{
		foreach (PlayerAgent player in _spawnedPlayerList)
		{
			player.Kill();
		}
	}

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
