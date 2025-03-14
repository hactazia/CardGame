using System.Collections.Generic;
using System.Linq;
using CardGameVR.Arenas;
using CardGameVR.Cards.Types;
using CardGameVR.Players;
using Cysharp.Threading.Tasks;

namespace CardGameVR.Cards
{
    public static class CardTypeManager
    {
        public static bool HasType(string type)
            => GetTypes().Contains(type);

        public static string[] GetTypes()
            => new[]
            {
                TankCard.GetTypeName(),
                JumperCard.GetTypeName()
            };

        public static Dictionary<string, float> GetDrawChances()
            => new()
            {
                { TankCard.GetTypeName(), TankCard.DrawChances },
                { JumperCard.GetTypeName(), JumperCard.DrawChances }
            };

        public static Dictionary<string, uint> GetMaxPresences()
            => new()
            {
                { TankCard.GetTypeName(), TankCard.MaxPresence },
                { JumperCard.GetTypeName(), JumperCard.MaxPresence }
            };

        public static async UniTask<ICard> SpawnType(string type)
        {
            if (type == TankCard.GetTypeName())
                return await TankCard.SpawnType();
            if (type == JumperCard.GetTypeName())
                return await JumperCard.SpawnType();
            return null;
        }

        public static string[] GetDrawableTypes()
        {
            var players = PlayerNetwork.Players;
            var counter = new Dictionary<string, uint>();
            foreach (var type in GetTypes())
                counter[type] = 0;
            foreach (var card in players.SelectMany(player => player.GetHandCards()))
                if (counter.ContainsKey(card.GetCardType()))
                    counter[card.GetCardType()]++;
                else counter[card.GetCardType()] = 1;
            foreach (var card in NetworkParty.Instance.GetBoardCards())
                if (counter.ContainsKey(card.GetCardType()))
                    counter[card.GetCardType()]++;
                else counter[card.GetCardType()] = 1;
            return counter.Keys
                .Where(type => counter[type] < GetMaxPresences()[type])
                .ToArray();
        }

        public static string DrawType()
        {
            var types = GetDrawableTypes();
            var chances = GetDrawChances();
            var total = types.Sum(type => chances[type]);
            var random = UnityEngine.Random.Range(0f, total);
            foreach (var type in types)
            {
                random -= chances[type];
                if (random <= 0)
                    return type;
            }

            return null;
        }


        public static int GetNextId()
        {
            var players = PlayerNetwork.Players;
            var ids = new HashSet<int>();
            foreach (var card in players.SelectMany(player => player.GetHandCards()))
                ids.Add(card.GetId());
            foreach (var card in NetworkParty.Instance.GetBoardCards())
                ids.Add(card.GetId());
            return ids.Prepend(0).Max() + 1;
        }
    }
}