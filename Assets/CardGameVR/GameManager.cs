using System.Collections.Generic;
using CardGameVR.Lobbies;
using CardGameVR.Multiplayer;
using CardGameVR.Controllers;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace CardGameVR
{
    public class GameManager : MonoBehaviour
    {
        private const int LoadingSceneIndex = 0;
        private const int MainSceneIndex = 1;
        private static readonly List<string> OList = new();

        [Header("Managers")] public NetworkManager networkManager;
        public LobbyManager lobbyManager;

#if UNITY_EDITOR
        public static bool StartGameFlag
            => PlayerPrefs.GetInt("start-game", 1) == 1;

        [UnityEditor.MenuItem("CardGameVR/Game/Enable Auto Start")]
        public static void EnableGame() => PlayerPrefs.SetInt("start-game", 1);

        [UnityEditor.MenuItem("CardGameVR/Game/Disable Auto Start")]
        public static void DisableGame() => PlayerPrefs.SetInt("start-game", 0);

        private void OnGUI()
        {
            if (OList.Count == 0) return;
            GUILayout.BeginArea(new Rect(10, 10, 200, 200));
            GUILayout.Label("Operations:");
            foreach (var operation in OList)
                GUILayout.Label(operation);
            GUILayout.EndArea();
        }
#else
        public static bool StartGameFlag => true;
#endif


        public static void AddOperation(string operation) 
            => OList.Add(operation);

        public static void RemoveOperation(string operation) 
            => OList.Remove(operation);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void OnAfterAssembliesLoaded()
        {
            if (!StartGameFlag) return;
            OList.Clear();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void OnAfterSceneLoad() => OnAfterSceneLoadAsync().Forget();

        private static async UniTask OnAfterSceneLoadAsync()
        {
            if (!StartGameFlag) return;
            SceneManager.LoadScene(LoadingSceneIndex);
            var go = new GameObject($"[{nameof(GameManager)}]");
            var game = go.AddComponent<GameManager>();
            DontDestroyOnLoad(go);
            var netPrefab = await Addressables.LoadAssetAsync<GameObject>("NetworkManager").ToUniTask();
            game.networkManager = Instantiate(netPrefab).GetComponent<NetworkManager>();
            game.networkManager.gameObject.name = $"[{nameof(NetworkManager)}]";
            game.networkManager.AddNetworkPrefab(await Addressables.LoadAssetAsync<GameObject>("NetworkParty"));
            DontDestroyOnLoad(game.networkManager.gameObject);
            var lobPrefab = await Addressables.LoadAssetAsync<GameObject>("LobbyManager").ToUniTask();
            game.lobbyManager = Instantiate(lobPrefab).GetComponent<LobbyManager>();
            game.lobbyManager.gameObject.name = $"[{nameof(LobbyManager)}]";
            DontDestroyOnLoad(game.lobbyManager.gameObject);
        }

        private void Start()
            => StartAsync().Forget();

        private async UniTask StartAsync()
        {
            await UniTask.WaitUntil(() => OList.Count == 0);
            await SceneManager.LoadSceneAsync(MainSceneIndex);
            ControllerManager.Controller.Recenter();
            ControllerManager.Controller.menu.Open();
        }
    }
}