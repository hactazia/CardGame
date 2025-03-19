using System.Collections.Generic;
using System.Linq;
using CardGameVR.Arenas;
using CardGameVR.Cards.Types;
using CardGameVR.Parties;
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
                JumperCard.GetTypeName(),
                SprinterCard.GetTypeName(),
            };

        public static Dictionary<string, float> GetDrawChances()
            => new()
            {
                { TankCard.GetTypeName(), TankCard.GetGlobalConfiguration().drawChances },
                { JumperCard.GetTypeName(), JumperCard.GetGlobalConfiguration().drawChances },
                { SprinterCard.GetTypeName(), SprinterCard.GetGlobalConfiguration().drawChances },
            };

        public static Dictionary<string, float> GetMaxPresences()
            => new()
            {
                { TankCard.GetTypeName(), TankCard.GetGlobalConfiguration().drawChances },
                { JumperCard.GetTypeName(), JumperCard.GetGlobalConfiguration().maxPresence },
                { SprinterCard.GetTypeName(), SprinterCard.GetGlobalConfiguration().maxPresence },
            };

        public static async UniTask<ICard> SpawnType(string type)
        {
            if (type == TankCard.GetTypeName())
                return await TankCard.SpawnType();
            if (type == JumperCard.GetTypeName())
                return await JumperCard.SpawnType();
            if (type == SprinterCard.GetTypeName())
                return await SprinterCard.SpawnType();
            return null;
        }

        public static string[] GetDrawableTypes()
        {
            var players = NetworkPlayer.Players;
            var counter = new Dictionary<string, uint>();
            foreach (var type in GetTypes())
                counter[type] = 0;
            foreach (var card in players.SelectMany(player => player.Hand))
                if (counter.ContainsKey(card.CardType.ToString()))
                    counter[card.CardType.ToString()]++;
                else counter[card.CardType.ToString()] = 1;
            foreach (var card in NetworkParty.Instance.Board)
                if (counter.ContainsKey(card.CardType.ToString()))
                    counter[card.CardType.ToString()]++;
                else counter[card.CardType.ToString()] = 1;
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
            var ids = new HashSet<int>();
            foreach (var card in NetworkPlayer.Players.SelectMany(player => player.Hand))
                ids.Add(card.Id);
            foreach (var card in NetworkParty.Instance.Board)
                ids.Add(card.Id);
            return ids.Prepend(0).Max() + 1;
        }
    }
}