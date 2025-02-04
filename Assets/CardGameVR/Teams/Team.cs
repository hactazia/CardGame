using UnityEngine;

namespace CardGameVR.Teams
{
    public enum Team
    {
        None = 0,
        Red = 1,
        Blue = 2,
        Green = 3,
        Yellow = 4
    }

    public static class TeamExtensions
    {
        public static Color ToColor(this Team team) => team switch
        {
            Team.Red => Color.red,
            Team.Blue => Color.blue,
            Team.Green => Color.green,
            Team.Yellow => Color.yellow,
            _ => Color.white
        };

        public static Team ToTeam(Color color)
        {
            if (color == Color.red) return Team.Red;
            if (color == Color.blue) return Team.Blue;
            if (color == Color.green) return Team.Green;
            return color == Color.yellow
                ? Team.Yellow
                : Team.None;
        }
    }
}