using System;

namespace CardGameVR.Tiles
{
    public class ActionTileArgs : EventArgs
    {
        public int actionTokens;
        public Tile tile;
        public Cards.ICard card;
    }
}