using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Define;

public class RankBoard : NetworkBehaviour
{
	[Header("References")]
	[SerializeField] private RankBoardRecordUI _rankboardUIPrefab;
    [SerializeField] private RectTransform _redRankListParent;
    [SerializeField] private RectTransform _blueRankListParent;

	[Header("Input")]
	[SerializeField] private InputReaderUI _inputReader;

	[Header("Color")]
	[SerializeField] private Color32 _defaultColor;
	[SerializeField] private Color32 _myColor;

    private NetworkList<RankBoardEntityState> _rankList;

    private List<RankBoardRecordUI> _rankUIList = new List<RankBoardRecordUI>();

	private void Awake()
	{
		_rankList = new NetworkList<RankBoardEntityState>();

		_inputReader.OnTapDown += HandleTapDown;
		_inputReader.OnTapUp += HandleTapUp;
	}

	#region Input
	private void HandleTapUp()
	{
		UIManager.Get<UIViewManager>().GetView<GamePlayView>().ShowRankboard(false);
	}

	private void HandleTapDown()
	{
		UIManager.Get<UIViewManager>().GetView<GamePlayView>().ShowRankboard(true);
	}
	#endregion

	#region Network
	public override void OnNetworkSpawn()
	{
		if (IsClient)
		{
			_rankList.OnListChanged += HandleOnListChanged;
			foreach (RankBoardEntityState ui in _rankList)
			{
				HandleOnListChanged(new NetworkListEvent<RankBoardEntityState>
				{
					Type = NetworkListEvent<RankBoardEntityState>.EventType.Add,
					Value = ui
				});
			}
		}

		if (IsHost)
		{
			GameManager.Instance.NetworkServer.OnUserJoin += HandleUserJoin;
			GameManager.Instance.NetworkServer.OnUserLeft += HandleUserLeft;
		}
	}

	public override void OnNetworkDespawn()
	{
		if (IsClient)
		{
			_rankList.OnListChanged -= HandleOnListChanged;
		}

		if (IsHost)
		{
			GameManager.Instance.NetworkServer.OnUserJoin -= HandleUserJoin;
			GameManager.Instance.NetworkServer.OnUserLeft -= HandleUserLeft;
		}
	}

	private void HandleUserJoin(ulong clientId, UserData userData)
	{
		RankBoardEntityState rbEntity = new RankBoardEntityState {
			ClientID = clientId,
			UserName = userData.UserName,
			Kills = 0,
			Team = GameManager.Instance.GetTeam(clientId) };

		_rankList.Add(rbEntity);
	}

	private void HandleUserLeft(ulong clientId, UserData userData)
	{
		foreach (RankBoardEntityState rbEntity in _rankList)
		{
			if (rbEntity.ClientID == clientId)
			{
				try {
					_rankList.Remove(rbEntity);
				} 
				catch (Exception e) {
					Debug.Log($"{rbEntity.UserName} [ {rbEntity.ClientID} ] : ������ ���� �߻� {e.Message}");
				}
				break;
			}
		}
	}

	private void HandleOnListChanged(NetworkListEvent<RankBoardEntityState> evt)
	{
		switch (evt.Type)
		{
			case NetworkListEvent<RankBoardEntityState>.EventType.Add:
				AddToUIList(evt.Value);
				break;
			case NetworkListEvent<RankBoardEntityState>.EventType.Remove:
				RemoveFromUIList(evt.Value.ClientID);
				break;
			case NetworkListEvent<RankBoardEntityState>.EventType.Value:
				AdjustValueToUIList(evt.Value);
				break;
		}
	}
	#endregion

	#region ������
	// RankList�� ���� ����� �� UI�� �ݿ��ϴ� �Լ�
	private void AdjustValueToUIList(RankBoardEntityState value)
	{
		RankBoardRecordUI ui = _rankUIList.Find(x => x.ClientId == value.ClientID); // Ŭ���̾�Ʈ Id�� ���� UI�� ã�Ƴ���

		if (ui != null)
		{
			int rank = 1; // _rankUIList�� ����, ��� ���� ��� �����ֱ� ������ ��ũ�� ���� ����ؾ� ��
			Transform uiParent = ui.Team == TEAM_TYPE.Red ? _redRankListParent : _blueRankListParent;

			ui.UpdateValue(1, value.Kills); // UI�� �����Ѵٸ� ���� �����Ѵ�
			_rankUIList.Sort((a, b) => b.Kills.CompareTo(a.Kills));

			// ��ũ������ ���� ����Ǿ��� ������ ����� ���� �������� �ٽ� �����Ѵ�
			for (int i = 0; i < _rankUIList.Count; ++i)
			{
				if (_rankUIList[i].Team == ui.Team) // �ٸ� ���� ������ �ʿ䰡 �����Ƿ� ���� ���� ����
				{
					_rankUIList[i].UpdateValue(rank, _rankUIList[i].Kills);
					rank++;

					// ��ũ ������� ����
					_rankUIList[i].transform.SetParent(null);
					_rankUIList[i].transform.SetParent(uiParent);
				}
			}
		}
	}

	private void AddToUIList(RankBoardEntityState value)
	{
		RankBoardRecordUI ui = _rankUIList.Find(x => x.ClientId == value.ClientID);
		if (ui == null)
		{
			Transform uiParent = value.Team == TEAM_TYPE.Red ? _redRankListParent : _blueRankListParent;
			ui = Instantiate(_rankboardUIPrefab, uiParent);
			ui.Team = value.Team;
			ui.SetOwner(value.ClientID);
			ui.SetName(value.UserName.ToString());
			ui.SetColor(value.ClientID == NetworkManager.Singleton.LocalClientId ? _myColor : _defaultColor);
			
			_rankUIList.Add(ui);
			UpdateRank();

		}
	}

	private void RemoveFromUIList(ulong clientId)
	{
		RankBoardRecordUI ui = _rankUIList.Find(x => x.ClientId == clientId);
		if (ui != null)
		{
			_rankUIList.Remove(ui);
			Destroy(ui.gameObject);
		}
	}

	public void AddKills(ulong clientId, int kills)
	{
		if (_rankList == null) return;

		for (int i = 0; i < _rankList.Count; ++i)
		{
			if (_rankList[i].ClientID == clientId)
			{
				var oldItem = _rankList[i];

				_rankList[i] = new RankBoardEntityState
				{
					ClientID = oldItem.ClientID,
					UserName = oldItem.UserName,
					Kills = oldItem.Kills + kills,
					Team = oldItem.Team,
				};
				break;
			}
		}
	}

	private void UpdateRank()
	{
		int redRank = 1; // _rankUIList�� ����, ��� ���� ��� �����ֱ� ������ ��ũ�� ���� ����ؾ� ��
		int blueRank = 1;
		Transform uiParent;

		_rankUIList.Sort((a, b) => b.Kills.CompareTo(a.Kills));

		// ��ũ������ ���� ����Ǿ��� ������ ����� ���� �������� �ٽ� �����Ѵ�
		for (int i = 0; i < _rankUIList.Count; ++i)
		{
			if (_rankUIList[i].Team == TEAM_TYPE.Red) {
				uiParent = _redRankListParent;
				_rankUIList[i].UpdateValue(redRank, _rankUIList[i].Kills);
				redRank++;
			}
			else {
				uiParent = _blueRankListParent;
				_rankUIList[i].UpdateValue(blueRank, _rankUIList[i].Kills);
				blueRank++;
			}

			// ��ũ ������� ����
			_rankUIList[i].transform.SetParent(null);
			_rankUIList[i].transform.SetParent(uiParent);
		}
	}

	public void ResetRankboard()
	{
		for (int i = 0; i < _rankList.Count; ++i)
		{
			var oldItem = _rankList[i];

			_rankList[i] = new RankBoardEntityState
			{
				ClientID = oldItem.ClientID,
				UserName = oldItem.UserName,
				Kills = 0,
				Team = GameManager.Instance.GetTeam(oldItem.ClientID),
			};
		}
	}
	#endregion
}
