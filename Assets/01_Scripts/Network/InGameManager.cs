using System;
using Unity.Netcode;
using UnityEngine;

public class InGameManager : NetworkBehaviour
{


    [SerializeField] private Transform _redTeamSpawn;
    [SerializeField] private Transform _blueTeamSpawn;

	private void Start()
	{
		UIViewManager.Instance.GetScene<GamePlayView>().OnPlayButtonClick += SpawnPlayer;
		PlayerAgent.OnPlayerDie += HandlePlayerDie;
		NetworkManager.Singleton.OnClientStarted += HandleClientConnected;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();

		if (NetworkManager.Singleton != null)
			NetworkManager.Singleton.OnClientStarted -= HandleClientConnected;
	}

	private void SpawnPlayer()
	{
		Debug.Log("Spawn");
		SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
	}

	[ServerRpc(RequireOwnership = false)]
	private void SpawnPlayerServerRpc(ulong clientId)
	{
		Transform spawnPos = GameManager.Instance.GetTeam(clientId) == TEAM_TYPE.Red ? _redTeamSpawn : _blueTeamSpawn;
		GameManager.Instance.NetworkServer.RespawnPlayer(clientId, spawnPos.position);
	}

	private void HandlePlayerDie(object sender, PlayerAgent.PlayerEventArgs e)
	{
		if (e.Player.IsOwner)
		{
			UIViewManager.Instance.ShowView<GamePlayView>();
		}
	}

	private void HandleClientConnected()
	{
		Debug.Log("Connected");
	}
}
