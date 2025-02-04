using Unity.Netcode;
using UnityEngine;

namespace CardGameVR.Grid
{
    public class Board : NetworkBehaviour
    {
        private NetworkVariable<Vector2Int> Size = new(new Vector2Int(10, 10));
        
        
        
    }
}