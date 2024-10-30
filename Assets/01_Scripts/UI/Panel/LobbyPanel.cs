using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPanel : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI _serverName;
	[SerializeField] private TextMeshProUGUI _playerCount;
	[SerializeField] private Button _joinButton;

	public void Init(Lobby lobby)
	{
		_serverName.text = lobby.Name;
		_playerCount.text = $"{lobby.Players.Count} / {lobby.MaxPlayers}";
		_joinButton.onClick.AddListener(() => LobbyManager.Instance.JoinLobby(lobby.Id));
	}
}
