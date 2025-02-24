using CardGameVR.XR;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CardGameVR.Controllers
{
    public static class ControllerManager
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        public static void OnBeforeSplashScreen()
        {
            if (!GameManager.StartGameFlag) return;
            GameManager.AddOperation(nameof(ControllerManager));
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void OnBeforeSceneLoad() => OnBeforeSceneLoadAsync().Forget();

        private static async UniTask OnBeforeSceneLoadAsync()
        {
            if (!GameManager.StartGameFlag) return;
            if (XRManager.IsXRActive())
                await SetXRPlayer();
            else await SetDesktopController();
            Controller.menu.Close();
            XRManager.OnXRHeadsetChange.AddListener(OnXRHeadsetChange);
            GameManager.RemoveOperation(nameof(ControllerManager));
        }

        public static Controller Controller { get; private set; }

        private static void OnXRHeadsetChange(bool active)
        {
            if (Controller && active == Controller.TryCast(out XRController _)) return;
            if (active) SetXRPlayer().Forget();
            else SetDesktopController().Forget();
        }

        private static async UniTask SetXRPlayer()
        {
            DestroyPreviousController();
            var prefab = await Addressables.LoadAssetAsync<GameObject>("XRPlayer").ToUniTask();
            var o = Object.Instantiate(prefab);
            o.name = $"[{nameof(XRController)}]";
            Object.DontDestroyOnLoad(o.gameObject);
            Controller = o.GetComponent<Controller>();
        }

        private static async UniTask SetDesktopController()
        {
            DestroyPreviousController();
            var prefab = await Addressables.LoadAssetAsync<GameObject>("DesktopPlayer").ToUniTask();
            var o = Object.Instantiate(prefab);
            o.name = $"[{nameof(DesktopController)}]";
            Object.DontDestroyOnLoad(o.gameObject);
            Controller = o.GetComponent<Controller>();
        }

        private static void DestroyPreviousController()
        {
            if (!Controller) return;
            Object.Destroy(Controller.gameObject);
            Controller = null;
        }
    }
}