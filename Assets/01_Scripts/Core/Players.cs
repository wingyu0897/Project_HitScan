using System.Collections.Generic;

/// <summary>
/// 각 클라이언트에서 현재 스폰된 Player Object를 관리하는 클래스. InGameManager에서 인스턴스를 생성하며, 모든 클라이언트에서 동작한다.
/// </summary>
public class Players
{
	static Players()
	{
		PlayerAgent.OnPlayerSpawn += HandlePlayerSpawn;
		PlayerAgent.OnPlayerDespawn += HandlePlayerDespawn;
	}

	private static Dictionary<ulong, PlayerAgent> _playerList = new();

	/// <summary>
	/// 플레이어가 스폰하면 리스트에 추가한다
	/// </summary>
	private static void HandlePlayerSpawn(object sender, PlayerAgent.PlayerEventArgs args)
	{
		if (_playerList.ContainsKey(args.ClientID))
		{
			UnityEngine.Debug.Log($"ClientID: {args.ClientID}의 플레이어 오브젝트가 이미 존재합니다.");
			return;
		}
		
		_playerList.Add(args.ClientID, args.Player);
		UnityEngine.Debug.Log($"ClientID: {args.ClientID}의 플레이어 오브젝트가 생성되었습니다.");
	}

	/// <summary>
	/// 플레이어가 디스폰하면 리스트에서 제거한다
	/// </summary>
	private static void HandlePlayerDespawn(object sender, PlayerAgent.PlayerEventArgs args)
	{
		if (!_playerList.ContainsKey(args.ClientID))
		{
			UnityEngine.Debug.Log($"ClientID: {args.ClientID}의 플레이어 오브젝트가 존재하지 않습니다.");
			return;
		}

		_playerList.Remove(args.ClientID);
		UnityEngine.Debug.Log($"ClientID: {args.ClientID}의 플레이어 오브젝트가 제거되었습니다.");
	}

	/// <summary>
	/// 클라이언트 ID를 통해 Player Object를 반환
	/// </summary>
	public static PlayerAgent GetPlayerObjectByClientID(ulong clientId)
	{
		if (!_playerList.ContainsKey(clientId))
		{
			UnityEngine.Debug.Log($"<GetPlayerObjectByClientID> 클라이언트 ID: {clientId}의 플레이어 오브젝트가 존재하지 않습니다.");
			return null;
		}

		return _playerList[clientId];
	}
}
