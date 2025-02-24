using CardGameVR.XR;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
        public static void OnBeforeSceneLoad() => OnBeforeSceneLoadAsync().Forget();

        private static async UniTask OnBeforeSceneLoadAsync()
        {
            if (!GameManager.StartGameFlag) return;
            Debug.Log("PlayerManager.OnBeforeSceneLoad()");
            if (XRManager.IsXRActive())
                await SetXRPlayer();
            else await SetDesktopPlayer();
            Player.menu.Close();
            XRManager.OnXRHeadsetChange.AddListener(OnXRHeadsetChange);
            GameManager.RemoveOperation(nameof(PlayerManager));
        }

        public static Player Player { get; private set; }

        private static void OnXRHeadsetChange(bool active)
        {
            if (Player && active == Player.TryCast(out XRPlayer _)) return;
            if (active) SetXRPlayer().Forget();
            else SetDesktopPlayer().Forget();
        }

        private static async UniTask SetXRPlayer()
        {
            DestroyPreviousPlayer();
            Debug.Log("Setup XR Player");
            var prefab = await Addressables.LoadAssetAsync<GameObject>("XRPlayer").ToUniTask();
            var o = Object.Instantiate(prefab);
            o.name = $"[{nameof(XRPlayer)}]";
            Object.DontDestroyOnLoad(o.gameObject);
            Player = o.GetComponent<Player>();
        }

        private static async UniTask SetDesktopPlayer()
        {
            DestroyPreviousPlayer();
            Debug.Log("Setup Desktop Player");
            var prefab = await Addressables.LoadAssetAsync<GameObject>("DesktopPlayer").ToUniTask();
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