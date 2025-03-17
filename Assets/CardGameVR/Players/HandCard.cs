using System;
using CardGameVR.Cards;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;

namespace CardGameVR.Players
{
    public struct PlayerHandCard : INetworkSerializable, IEquatable<PlayerHandCard>
    {
        public int Id;
        public FixedString32Bytes CardType;
        public bool IsVisibleForOtherPlayers;
        public bool IsVisibleForLocalPlayer;

        public bool HasType
            => CardTypeManager.HasType(CardType.ToString());

        public async UniTask<ICard> Spawn()
        {
            var spawned = await CardTypeManager.SpawnType(CardType.ToString());
            spawned.SetId(Id);
            return spawned;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Id);
            serializer.SerializeValue(ref CardType);
            serializer.SerializeValue(ref IsVisibleForOtherPlayers);
            serializer.SerializeValue(ref IsVisibleForLocalPlayer);
        }

        public bool Equals(PlayerHandCard other)
            => Id.Equals(other.Id)
               && CardType.Equals(other.CardType)
               && IsVisibleForOtherPlayers.Equals(other.IsVisibleForOtherPlayers);

        public override int GetHashCode()
            => HashCode.Combine(Id, CardType, IsVisibleForOtherPlayers);
    }
}