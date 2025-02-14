using CardGameVR.XR;
using UnityEngine;

namespace CardGameVR.Players
{
    public static class PlayerManager
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        public static void OnBeforeSplashScreen()
        {
            if (!GameManager.StartGameFlag) return;
            Debug.Log("PlayerManager.OnBeforeSplashScreen()");
            GameManager.AddOperation(nameof(PlayerManager));
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void OnBeforeSceneLoad()
        {
            if (!GameManager.StartGameFlag) return;
            Debug.Log("PlayerManager.OnBeforeSceneLoad()");
            if (XRManager.IsXRActive())
                SetXRPlayer();
            else SetDesktopPlayer();
            XRManager.OnXRHeadsetChange.AddListener(OnXRHeadsetChange);
            GameManager.RemoveOperation(nameof(PlayerManager));
        }

        public static Player Player { get; private set; }

        private static void OnXRHeadsetChange(bool active)
        {
            if (Player && active == Player.TryCast(out XRPlayer _)) return;
            if (active) SetXRPlayer();
            else SetDesktopPlayer();
        }

        private static void SetXRPlayer()
        {
            DestroyPreviousPlayer();
            Debug.Log("Setup XR Player");
            var prefab = Resources.Load<GameObject>("XRPlayer");
            var o = Object.Instantiate(prefab);
            o.name = $"[{nameof(XRPlayer)}]";
            Object.DontDestroyOnLoad(o.gameObject);
            Player = o.GetComponent<Player>();
        }

        private static void SetDesktopPlayer()
        {
            DestroyPreviousPlayer();
            Debug.Log("Setup Desktop Player");
            var prefab = Resources.Load<GameObject>("DesktopPlayer");
            var o = Object.Instantiate(prefab);
            o.name = $"[{nameof(DesktopPlayer)}]";
            Object.DontDestroyOnLoad(o.gameObject);
            Player = o.GetComponent<Player>();
        }

        private static void DestroyPreviousPlayer()
        {
            if (!Player) return;
            Debug.Log("Destroy Previous Player");
            Object.Destroy(Player.gameObject);
            Player = null;
        }
    }
}