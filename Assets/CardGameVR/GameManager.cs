using System.Collections.Generic;
using CardGameVR.Lobbies;
using CardGameVR.Multiplayer;
using CardGameVR.Players;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CardGameVR
{
    public class GameManager : MonoBehaviour
    {
        private const int LoadingSceneIndex = 0;
        private const int MainSceneIndex = 1;
        private static readonly List<string> OList = new();
        
        [Header("Managers")]
        public NetworkManager networkManager;
        public LobbyManager lobbyManager;
        public MultiplayerManager multiplayerManager;
        
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
        {
            Debug.Log($"GameManager.AddOperation: {operation}");
            OList.Add(operation);
        }

        public static void RemoveOperation(string operation)
        {
            Debug.Log($"GameManager.RemoveOperation: {operation}");
            OList.Remove(operation);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void OnAfterAssembliesLoaded()
        {
            if (!StartGameFlag) return;
            Debug.Log("GameManager.OnAfterAssembliesLoaded");
            OList.Clear();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void OnAfterSceneLoad()
        {
            if (!StartGameFlag) return;
            Debug.Log("GameManager.OnAfterSceneLoad");
            SceneManager.LoadScene(LoadingSceneIndex);
            var go = new GameObject($"[{nameof(GameManager)}]");
            var game = go.AddComponent<GameManager>();
            DontDestroyOnLoad(go);
            var netPrefab = Resources.Load<GameObject>("NetworkManager");
            game.networkManager = Instantiate(netPrefab).GetComponent<NetworkManager>();
            game.networkManager.gameObject.name = $"[{nameof(NetworkManager)}]";
            DontDestroyOnLoad(game.networkManager.gameObject);
            var lobPrefab = Resources.Load<GameObject>("LobbyManager");
            game.lobbyManager = Instantiate(lobPrefab).GetComponent<LobbyManager>();
            game.lobbyManager.gameObject.name = $"[{nameof(LobbyManager)}]";
            DontDestroyOnLoad(game.lobbyManager.gameObject);
            var mulPrefab = Resources.Load<GameObject>("MultiplayerManager");
            game.multiplayerManager = Instantiate(mulPrefab).GetComponent<MultiplayerManager>();
            game.multiplayerManager.gameObject.name = $"[{nameof(MultiplayerManager)}]";
            DontDestroyOnLoad(game.multiplayerManager.gameObject);
        }

        private void Start()
            => StartAsync().Forget();

        private async UniTask StartAsync()
        {
            Debug.Log("GameManager.StartAsync");
            await UniTask.WaitUntil(() => OList.Count == 0);
            await SceneManager.LoadSceneAsync(MainSceneIndex);
            PlayerManager.Player.Recenter();
        }
    }
}