using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CardGameVR
{
    public class NetworkParty : NetworkBehaviour
    {
        public static NetworkParty Instance { get; private set; }
        
        public static async UniTask<NetworkParty> Spawn()
        {
            if (!Multiplayer.MultiplayerManager.IsServer())
                throw new System.Exception("Only the server can spawn the NetworkParty");
            var asset = await Addressables.LoadAssetAsync<GameObject>("NetworkParty");
            var go = Instantiate(asset);
            go.name = $"[{nameof(NetworkParty)}]";
            DontDestroyOnLoad(go);
            var party = go.GetComponent<NetworkParty>();
            var networkObject = go.GetComponent<NetworkObject>();
            if (networkObject && !networkObject.IsSpawned)
                networkObject.Spawn();
            Instance = party;
            return party;
        }
        
        public static void Destroy()
        {
            if (!Multiplayer.MultiplayerManager.IsServer())
                throw new System.Exception("Only the server can spawn the NetworkParty");
            if (Instance)
                Destroy(Instance.gameObject);
            Instance = null;
        }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Instance = this;
        }
        
    }
}