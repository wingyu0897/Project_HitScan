using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Define;

public class RankBoard : NetworkBehaviour
{
	[SerializeField] private RankBoardRecordUI _rankboardUIPrefab;
    [SerializeField] private RectTransform _redRankListParent;
    [SerializeField] private RectTransform _blueRankListParent;

    private NetworkList<RankBoardEntityState> _rankList;

    private List<RankBoardRecordUI> _rankUIList = new List<RankBoardRecordUI>();

	private void Awake()
	{
		_rankList = new NetworkList<RankBoardEntityState>();
	}

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
					Debug.LogError($"{rbEntity.UserName} [ {rbEntity.ClientID} ] : 삭제중 오류 발생 {e.Message}");
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

	private void AdjustValueToUIList(RankBoardEntityState value)
	{
		RankBoardRecordUI ui = _rankUIList.Find(x => x.ClientId == value.ClientID);
		if (ui != null)
		{
			ui.UpdateValue(1, value.Kills);
		}

		_rankUIList.Sort((a, b) => b.Kills.CompareTo(a.Kills));
		for (int i = 0; i < _rankUIList.Count; ++i)
		{
			_rankUIList[i].UpdateValue(i + 1, _rankUIList[i].Kills);

			// 랭크 순서대로 정렬
			Transform uiParent = _rankUIList[i].Team == TEAM_TYPE.Red ? _redRankListParent : _blueRankListParent;
			_rankUIList[i].transform.SetParent(null);
			_rankUIList[i].transform.SetParent(uiParent);
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
			ui.UpdateValue(1, value.Kills);
			_rankUIList.Add(ui);
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
}
