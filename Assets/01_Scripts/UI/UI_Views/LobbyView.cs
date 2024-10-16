using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyView : UIView
{
	[Header("Lobby List")]
	[SerializeField] private LobbyPanel _lobbyPanelPrefab;
	[SerializeField] private Button _refreshBtn;
	[SerializeField] private Transform _lobbyPanelContainer;

	[Header("Create Lobby")]
	[SerializeField] private TMP_InputField _serverName;
	[SerializeField] private TextMeshProUGUI _maxPlayerTxt;
	[SerializeField] private Button _createServerBtn;
	
	private int _maxPlayer = 10;
	public int MaxPlayer { get => _maxPlayer; set { 
			_maxPlayer = Mathf.Clamp(value / 2 * 2, 2, 10);
			_maxPlayerTxt.text = _maxPlayer.ToString();
		} }

	protected override void Awake()
	{
		base.Awake();

		BindEvents();
	}

	private void BindEvents()
	{
		LobbyManager.Instance.OnLobbyListChanged += OnLobbyListChangedHandler;
		LobbyManager.Instance.OnJoinedLobby += OnJoinedLobbyHandler;

		_createServerBtn.onClick.AddListener(OnCreateLobbyClick);
		_refreshBtn.onClick.AddListener(OnRefreshClick);
	}

	private void OnLobbyListChangedHandler(object sender, LobbyManager.LobbyListChangedEventArgs args)
	{
		UpdateLobbyList(args.LobbyList);
	}

	private void OnJoinedLobbyHandler(object sender, LobbyManager.LobbyEventArgs args)
	{
		UIManager.UIViewManager.HideView(this);
		UIManager.UIViewManager.ShowView<JoinedLobbyView>();
	}

	private void UpdateLobbyList(List<Lobby> lobbyList)
	{
		foreach (Transform child in _lobbyPanelContainer)
		{
			Destroy(child.gameObject);
		}

		foreach (Lobby lobby in lobbyList)
		{
			LobbyPanel lobbyPanelUI = Instantiate(_lobbyPanelPrefab, _lobbyPanelContainer);
			lobbyPanelUI.Init(lobby);
		}
	}

	private void OnCreateLobbyClick()
	{
		if (string.IsNullOrEmpty(_serverName.text)) return;

		LobbyManager.Instance.CreateLobby(_serverName.text, _maxPlayer);
		LobbyManager.Instance.RefreshLobbyList();
	}

	private void OnRefreshClick()
	{
		LobbyManager.Instance.RefreshLobbyList();
	}

	public void ChangeMaxPlayer(int add) => MaxPlayer += add;
}
