using System.Collections.Generic;

/// <summary>
/// �� Ŭ���̾�Ʈ���� ���� ������ Player Object�� �����ϴ� Ŭ����. InGameManager���� �ν��Ͻ��� �����ϸ�, ��� Ŭ���̾�Ʈ���� �����Ѵ�.
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
	/// �÷��̾ �����ϸ� ����Ʈ�� �߰��Ѵ�
	/// </summary>
	private static void HandlePlayerSpawn(object sender, PlayerAgent.PlayerEventArgs args)
	{
		if (_playerList.ContainsKey(args.ClientID))
		{
			UnityEngine.Debug.Log($"ClientID: {args.ClientID}�� �÷��̾� ������Ʈ�� �̹� �����մϴ�.");
			return;
		}
		
		_playerList.Add(args.ClientID, args.Player);
		UnityEngine.Debug.Log($"ClientID: {args.ClientID}�� �÷��̾� ������Ʈ�� �����Ǿ����ϴ�.");
	}

	/// <summary>
	/// �÷��̾ �����ϸ� ����Ʈ���� �����Ѵ�
	/// </summary>
	private static void HandlePlayerDespawn(object sender, PlayerAgent.PlayerEventArgs args)
	{
		if (!_playerList.ContainsKey(args.ClientID))
		{
			UnityEngine.Debug.Log($"ClientID: {args.ClientID}�� �÷��̾� ������Ʈ�� �������� �ʽ��ϴ�.");
			return;
		}

		_playerList.Remove(args.ClientID);
		UnityEngine.Debug.Log($"ClientID: {args.ClientID}�� �÷��̾� ������Ʈ�� ���ŵǾ����ϴ�.");
	}

	/// <summary>
	/// Ŭ���̾�Ʈ ID�� ���� Player Object�� ��ȯ
	/// </summary>
	public static PlayerAgent GetPlayerObjectByClientID(ulong clientId)
	{
		if (!_playerList.ContainsKey(clientId))
		{
			UnityEngine.Debug.Log($"<GetPlayerObjectByClientID> Ŭ���̾�Ʈ ID: {clientId}�� �÷��̾� ������Ʈ�� �������� �ʽ��ϴ�.");
			return null;
		}

		return _playerList[clientId];
	}
}
