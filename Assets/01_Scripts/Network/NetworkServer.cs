using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

public class UserData
{
	public string UserName;
}

public class NetworkServer
{
	private NetworkManager _networkManager;
	private NetworkObject _playerPrefab;

	private Dictionary<ulong, UserData> _clientIdToUserDataDictionary = new Dictionary<ulong, UserData>();

	public NetworkServer(NetworkObject playerPrefab)
	{
		_playerPrefab = playerPrefab;

		_networkManager = NetworkManager.Singleton;
		_networkManager.ConnectionApprovalCallback += HandleConnectionApprovalCallback;
		_networkManager.OnServerStarted += HandleServerStarted;
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
		RespawnPlayer(clientId);
		GameManager.Instance.AddUser(clientId);
	}

	private void HandleClientDisconnect(ulong clientId)
	{
		GameManager.Instance.RemoveUser(clientId);
	}

	private void RespawnPlayer(ulong clientId)
	{
		NetworkObject instance = GameObject.Instantiate(_playerPrefab, Vector3.zero, Quaternion.identity);

		instance.SpawnAsPlayerObject(clientId);
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
