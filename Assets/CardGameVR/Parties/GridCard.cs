using System;
using CardGameVR.Cards;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace CardGameVR.Parties
{
    public struct GridCard : INetworkSerializable, IEquatable<GridCard>
    {
        public int Id;
        public FixedString32Bytes CardType;
        public int Index;
        public ulong OwnerId;
        public bool IsBoosted;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Id);
            serializer.SerializeValue(ref CardType);
            serializer.SerializeValue(ref Index);
            serializer.SerializeValue(ref OwnerId);
            serializer.SerializeValue(ref IsBoosted);
        }

        public bool Equals(GridCard other)
            => Id.Equals(other.Id)
               && CardType.Equals(other.CardType)
               && Index.Equals(other.Index)
               && OwnerId.Equals(other.OwnerId)
               && IsBoosted.Equals(other.IsBoosted);

        public override int GetHashCode()
            => HashCode.Combine(Id, CardType, Index, OwnerId);
    }
}