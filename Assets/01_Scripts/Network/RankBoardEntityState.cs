using System;
using Unity.Collections;
using Unity.Netcode;

public struct RankBoardEntityState : INetworkSerializable, IEquatable<RankBoardEntityState>
{
	public ulong ClientID;
	public FixedString32Bytes UserName;
	public int Kills;
	public Define.TEAM_TYPE Team;

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref ClientID);
		serializer.SerializeValue(ref UserName);
		serializer.SerializeValue(ref Kills);
		serializer.SerializeValue(ref Team);
	}

	public bool Equals(RankBoardEntityState other)
	{
		return ClientID == other.ClientID;
	}
}
