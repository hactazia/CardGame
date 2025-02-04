using CardGameVR.XR;
using UnityEngine;

namespace CardGameVR.Players
{
    public static class PlayerManager
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        public static void OnBeforeSplashScreen()
        {
            if (!GameManager.startGameFlag) return;
            Debug.Log("PlayerManager.OnBeforeSplashScreen()");
            GameManager.AddOperation(nameof(PlayerManager));
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void OnBeforeSceneLoad()
        {
            if (!GameManager.startGameFlag) return;
            Debug.Log("PlayerManager.OnBeforeSceneLoad()");
            if (XRManager.IsXRActive())
                SetXRPlayer();
            else SetDesktopPlayer();
            XRManager.OnXRHeadsetChange.AddListener(OnXRHeadsetChange);
            GameManager.RemoveOperation(nameof(PlayerManager));
        }

        public static Player player { get; private set; }
        
        private static void OnXRHeadsetChange(bool active)
        {
            if (player && active == player.TryCast(out VRPlayer _)) return;
            if (active) SetXRPlayer();
            else SetDesktopPlayer();
        }

        private static void SetXRPlayer()
        {
            DestroyPreviousPlayer();
            Debug.Log("Setup XR Player");
            var prefab = Resources.Load<GameObject>("XRPlayer");
            var player = Object.Instantiate(prefab);
            Object.DontDestroyOnLoad(player);
            PlayerManager.player = player.GetComponent<Player>();
        }

        private static void SetDesktopPlayer()
        {
            DestroyPreviousPlayer();
            Debug.Log("Setup Desktop Player");
            var prefab = Resources.Load<GameObject>("DesktopPlayer");
            var player = Object.Instantiate(prefab);
            Object.DontDestroyOnLoad(player);
            PlayerManager.player = player.GetComponent<Player>();
        }

        private static void DestroyPreviousPlayer()
        {
            if (!player) return;
            Debug.Log("Destroy Previous Player");
            Object.Destroy(player.gameObject);
            player = null;
        }
    }
}