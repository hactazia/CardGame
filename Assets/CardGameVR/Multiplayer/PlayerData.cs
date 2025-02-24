using System;
using Unity.Collections;
using Unity.Netcode;

namespace CardGameVR.Multiplayer
{
    public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
    {
        public ulong ClientId;
        public FixedString128Bytes PlayerId;
        public FixedString128Bytes PlayerName;
        public byte Placement;

        public bool Equals(PlayerData other)
            => ClientId == other.ClientId
               && PlayerId == other.PlayerId
               && PlayerName == other.PlayerName
               && Placement == other.Placement;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref PlayerId);
            serializer.SerializeValue(ref PlayerName);
            serializer.SerializeValue(ref Placement);
        }
    }
}