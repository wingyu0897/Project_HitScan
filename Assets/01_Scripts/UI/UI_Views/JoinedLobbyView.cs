using System;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class JoinedLobbyView : UIView
{
	[SerializeField] private Transform _playerPanelPrefab;
	[SerializeField] private Transform _playerPanelContainer;
	[SerializeField] private TextMeshProUGUI _serverTxt;
	[SerializeField] private Button _startGameBtn;
	[SerializeField] private Button _leaveLobbyBtn;

	protected override void Awake()
	{
		base.Awake();

		LobbyManager.Instance.OnJoinedLobby += OnJoinedLobbyHandler;
		LobbyManager.Instance.OnJoinedLobbyUpdate += OnJoinedLobbyUpdateHandler;
		LobbyManager.Instance.OnLeaveLobby += OnLeaveLobbyHandler;

		_startGameBtn.onClick.AddListener(OnStartGameClick);
		_leaveLobbyBtn.onClick.AddListener(OnLeaveLobbyClick);
	}

	private void OnDestroy()
	{
		if (LobbyManager.InstanceIsNull) return;
		LobbyManager.Instance.OnJoinedLobby -= OnJoinedLobbyHandler;
		LobbyManager.Instance.OnJoinedLobbyUpdate -= OnJoinedLobbyUpdateHandler;
		LobbyManager.Instance.OnLeaveLobby -= OnLeaveLobbyHandler;
	}

	private void OnJoinedLobbyHandler(object sender, LobbyManager.LobbyEventArgs args)
	{
		_serverTxt.text = $"Server - {args.Lobby.Name}";

		_startGameBtn.gameObject.SetActive(args.Lobby.HostId == AuthenticationService.Instance.PlayerId);

		UpdatePlayerList(args.Lobby.Players);
	}

	private void OnJoinedLobbyUpdateHandler(object sender, LobbyManager.LobbyEventArgs args)
	{
		UpdatePlayerList(args.Lobby.Players);
	}

	private void OnLeaveLobbyHandler(object sender, EventArgs args)
	{
		UIViewManager.Instance.HideView(this);
		UIViewManager.Instance.ShowView<LobbyView>();
	}

	private void OnStartGameClick()
	{
		LobbyManager.Instance.StartGame();
	}

	private void OnLeaveLobbyClick()
	{
		LobbyManager.Instance.LeaveLobby();
	}

	private void UpdatePlayerList(List<Player> players)
	{
		foreach (Transform child in _playerPanelContainer)
		{
			Destroy(child.gameObject);
		}

		foreach (Player player in players)
		{
			Transform playerPanel = Instantiate(_playerPanelPrefab, _playerPanelContainer);
			TextMeshProUGUI tmp = playerPanel.Find("PlayerNameText").GetComponent<TextMeshProUGUI>();
			tmp.text = player.Data[LobbyManager.KEY_PLAYER_NAME].Value;
		}
	}
}
