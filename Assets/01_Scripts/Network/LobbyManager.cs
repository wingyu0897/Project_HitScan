using System;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoSingleton<LobbyManager>
{
	public const string KEY_START_GAME = "StartGame";
	public const string KEY_PLAYER_NAME = "PlayerName";

	private bool _isAuthenticating = false;
	private bool _isJoinedGame = false;
	private Lobby _hostLobby;
	private Lobby _joinedLobby;
	private float _heartbeatTimer;
	private float _lobbyPollTimer;
	private float _refreshTimer;
	private string _playerName;
	private string _lobbyName;

	public event EventHandler OnSignedIn;
	public event EventHandler OnSignInFailed;
	public event EventHandler OnGameStarted;
	public event EventHandler OnLeaveLobby;
	public event EventHandler<LobbyEventArgs> OnJoinedLobby;
	public event EventHandler<LobbyEventArgs> OnJoinedLobbyUpdate;
	public event EventHandler<LobbyListChangedEventArgs> OnLobbyListChanged;

	public class LobbyEventArgs : EventArgs {
		public Lobby Lobby;
	}

	public class LobbyListChangedEventArgs : EventArgs {
		public List<Lobby> LobbyList;
	}

	protected override void Awake()
	{
		base.Awake();

		DontDestroyOnLoad(gameObject);
	}

	private void Update()
	{
		HandleLobbyHeartbeat();
		HandleLobbyPolling();
	}

	/// <summary>
	/// 게임 접속 시도
	/// </summary>
	public async void Authenticate(string playerName)
	{
		if (_isAuthenticating) return;
		_isAuthenticating = true;

		_playerName = playerName;
		InitializationOptions initializationOptions = new InitializationOptions();

		try
		{
			initializationOptions.SetProfile(_playerName);
		}
		catch (AuthenticationException e)
		{
			Debug.Log(e);
			_isAuthenticating = false;
			return;
		}

		await UnityServices.InitializeAsync(initializationOptions);

		AuthenticationService.Instance.SignedIn += () => {
			RefreshLobbyList();
			OnSignedIn?.Invoke(this, EventArgs.Empty);
			GameManager.Instance.UserName = _playerName; // 로그인에 성공하면 GameManager에 유저 이름을 저장한다
		};

		AuthenticationService.Instance.SignInFailed += (e) => {
			OnSignInFailed?.Invoke(this, EventArgs.Empty);
		};

		await AuthenticationService.Instance.SignInAnonymouslyAsync();
	}

	private async void HandleLobbyHeartbeat()
	{
		if (_hostLobby != null)
		{
			_heartbeatTimer -= Time.deltaTime;
			if (_heartbeatTimer < 0f)
			{
				float heartbeatTimerMax = 15f;
				_heartbeatTimer = heartbeatTimerMax;

				await LobbyService.Instance.SendHeartbeatPingAsync(_hostLobby.Id);
			}
		}
	}

	private async void HandleLobbyPolling()
	{
		if (_joinedLobby != null && !_isJoinedGame)
		{
			_lobbyPollTimer -= Time.deltaTime;
			if (_lobbyPollTimer < 0f)
			{
				float lobbyUpdateTimerMax = 1.1f;
				_lobbyPollTimer = lobbyUpdateTimerMax;

				try
				{
					_joinedLobby = await LobbyService.Instance.GetLobbyAsync(_joinedLobby.Id);
					OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { Lobby = _joinedLobby });
				}
				catch (LobbyServiceException e)
				{
					Debug.Log(e);
				}

				//if (!IsPlayerInLobby()) // Kicked
				//{
				//
				//}

				if (_joinedLobby == null)
					print("null1");

				if (_joinedLobby.Data[KEY_START_GAME].Value != "0")
				{
					// Game Started
					if (!IsLobbyHost())
					{
						GameManager.Instance.JoinRelayGame(_joinedLobby.Data[KEY_START_GAME].Value, new UserData { UserName = _playerName });
					}

					_isJoinedGame = true;

					OnGameStarted?.Invoke(this, EventArgs.Empty);
				}
			}
		}
	}

	private bool IsLobbyHost()
	{
		return _joinedLobby != null && _joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
	}

	private Player GetPlayer()
	{
		return new Player(AuthenticationService.Instance.PlayerId, null, new Dictionary<string, PlayerDataObject> {
			{ KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, _playerName) }
		});
	}

	public async void CreateLobby(string lobbyName, int maxPlayer)
	{
		try
		{
			_lobbyName = lobbyName;

			CreateLobbyOptions options = new CreateLobbyOptions
			{
				IsPrivate = false,
				Player = GetPlayer(),
				Data = new Dictionary<string, DataObject>
				{
					{ KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, "0") }
				}
			};


			Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayer, options);

			_hostLobby = lobby;
			_joinedLobby = _hostLobby;

			OnJoinedLobby?.Invoke(this, new LobbyEventArgs { Lobby = lobby });
		}
		catch (LobbyServiceException e)
		{
			Debug.Log(e);
		}
	}

	public async void JoinLobby(string id)
	{
		try
		{
			JoinLobbyByIdOptions options = new JoinLobbyByIdOptions { Player = GetPlayer() };

			Lobby lobby = await Lobbies.Instance.JoinLobbyByIdAsync(id, options);
			
			if (_joinedLobby != null)
				LeaveLobby();
			_joinedLobby = lobby;

			OnJoinedLobby?.Invoke(this, new LobbyEventArgs { Lobby = lobby });
		}
		catch (LobbyServiceException e)
		{
			Debug.Log(e);
		}
	}

	public async void JoinLobbyByCode(string lobbyCode)
	{
		try
		{
			JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions { Player = GetPlayer() };

			Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode);

			if (_joinedLobby != null)
				LeaveLobby();
			_joinedLobby = lobby;

			OnJoinedLobby?.Invoke(this, new LobbyEventArgs { Lobby = lobby });
		}
		catch (LobbyServiceException e)
		{
			Debug.Log(e);
		}
	}

	public async void RefreshLobbyList()
	{
		if (Time.time - _refreshTimer < 1.1f) return;
		_refreshTimer = Time.time;

		try
		{
			QueryLobbiesOptions options = new QueryLobbiesOptions();
			options.Count = 25;

			options.Filters = new List<QueryFilter>()
			{
				new QueryFilter(
					field: QueryFilter.FieldOptions.AvailableSlots,
					op: QueryFilter.OpOptions.GT,
					value: "0")
			};

			options.Order = new List<QueryOrder>()
			{
				new QueryOrder(
					asc: false,
					field: QueryOrder.FieldOptions.Created)
			};

			QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
			OnLobbyListChanged?.Invoke(this, new LobbyListChangedEventArgs() { LobbyList = queryResponse.Results });
		}
		catch (LobbyServiceException e)
		{
			Debug.Log(e);
		}
	}

	private async void UpdatePlayerName(string newPlayerName)
	{
		try
		{
			_playerName = newPlayerName;
			await LobbyService.Instance.UpdatePlayerAsync(_joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions()
			{
				Data = new Dictionary<string, PlayerDataObject>
				{
					{ "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, _playerName) }
				}
			});
		}
		catch (LobbyServiceException e)
		{
			Debug.Log(e);
		}
	}
	
	public async void LeaveLobby()
	{
		if (_joinedLobby == null)
		{
			RefreshLobbyList();
			OnLeaveLobby?.Invoke(this, EventArgs.Empty);
			return;
		}

		try
		{
			if (IsLobbyHost())
			{
				if (_joinedLobby.Players.Count > 1)
					MigrateLobbyHost();
				else
					DeleteLobby();

				_hostLobby = null;
			}

			await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id, AuthenticationService.Instance.PlayerId);
		}
		catch (LobbyServiceException e)
		{
			Debug.Log(e);
		}
		finally
		{
			_isJoinedGame = false;
			_joinedLobby = null;
			RefreshLobbyList();

			OnLeaveLobby?.Invoke(this, EventArgs.Empty);
		}
	}

	public async void KickPlayer(string playerId)
	{
		if (IsLobbyHost())
		{
			try
			{
				await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id, playerId);
			}
			catch (LobbyServiceException e)
			{
				Debug.Log(e);
			}
		}
	}

	private async void MigrateLobbyHost()
	{
		try
		{
			_hostLobby = await Lobbies.Instance.UpdateLobbyAsync(_hostLobby.Id, new UpdateLobbyOptions
			{
				HostId = _hostLobby.Players[1].Id
			});
			_joinedLobby = _hostLobby;

			_hostLobby = null;
		}
		catch (LobbyServiceException e)
		{
			Debug.Log(e);
		}
	}

	private async void DeleteLobby()
	{
		if (_hostLobby == null) return;

		try
		{
			await LobbyService.Instance?.DeleteLobbyAsync(_hostLobby.Id);
			_hostLobby = null;
		}
		catch (LobbyServiceException e)
		{
			Debug.Log(e);
		}
	}

	public async void StartGame()
	{
		if (IsLobbyHost())
		{
			try
			{
				string relayCode = await GameManager.Instance.CreateRelayGame(new UserData{ UserName = _playerName });

				Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(_joinedLobby.Id, new UpdateLobbyOptions
				{
					Data = new Dictionary<string, DataObject>
					{
						{ KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
					}
				});

				_joinedLobby = lobby;
			}
			catch (LobbyServiceException e)
			{
				Debug.Log(e);
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		//if (_hostLobby != null)
		//{
		//	DeleteLobby();
		//}
		if (_joinedLobby != null)
		{
			LeaveLobby();
		}
	}
}
