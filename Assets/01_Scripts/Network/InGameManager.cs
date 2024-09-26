using Unity.Netcode;
using UnityEngine;

public enum GAME_MODE
{
	TeamDeathMatch,
}

public class InGameManager : NetworkBehaviour
{
	private GAME_MODE _gameMode;

	[SerializeField] private RankBoard _rankBoard;
    [SerializeField] private Transform _redTeamSpawn;
    [SerializeField] private Transform _blueTeamSpawn;

	private bool _isSpawnable = false;

	private void Start()
	{
		UIViewManager.Instance.GetView<GameReadyView>().OnPlayButtonClick += SpawnPlayer;
		PlayerAgent.OnPlayerDie += HandlePlayerDie;
		PlayerAgent.OnPlayerDespawn += HandlePlayerDespawn;
		NetworkManager.Singleton.OnClientStarted += HandleClientConnected;

		SelectGameMode();
		InitializeGame();
	}

	public override void OnDestroy()
	{
		base.OnDestroy();

		if (NetworkManager.Singleton != null)
			NetworkManager.Singleton.OnClientStarted -= HandleClientConnected;
	}

	#region Game Mode

	public void SelectGameMode()
	{
		_gameMode = GAME_MODE.TeamDeathMatch;
	}

	public void InitializeGame()
	{
		GameManager.Instance.NetworkServer.KillAllPlayer();

		_isSpawnable = true;
	}

	public void SelectMap()
	{

	}

	#endregion


	#region Player
	private void SpawnPlayer()
	{
		if (!_isSpawnable) return;

		Debug.Log("Player Spawned");
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
		if (IsHost)
		{
			Debug.Log($"ClientId: {e.Player.Health.LastHitClientId}, UserName: {GameManager.Instance.NetworkServer.GetUserDataByClientID(e.Player.Health.LastHitClientId)?.UserName ?? "KillZone"}, Kill: 1");
			_rankBoard?.AddKills(e.Player.Health.LastHitClientId, 1);
		}
	}

	private void HandlePlayerDespawn(object sender, PlayerAgent.PlayerEventArgs e)
	{
		if (e.Player.IsOwner)
		{
			UIViewManager.Instance.ShowView<GameReadyView>();
		}
	}

	private void HandleClientConnected()
	{
		Debug.Log("Connected");
	}
	#endregion
}
