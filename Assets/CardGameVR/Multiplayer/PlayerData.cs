using System;
using CardGameVR.Teams;
using Unity.Collections;
using Unity.Netcode;

namespace CardGameVR.Multiplayer
{
    public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
    {
        public ulong ClientId;
        public Team Team;
        public FixedString128Bytes PlayerId;
        public FixedString128Bytes PlayerName;

        public bool Equals(PlayerData other)
            => ClientId == other.ClientId
               && Team == other.Team
               && PlayerId == other.PlayerId
               && PlayerName == other.PlayerName;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref Team);
            serializer.SerializeValue(ref PlayerId);
            serializer.SerializeValue(ref PlayerName);
        }
    }
}