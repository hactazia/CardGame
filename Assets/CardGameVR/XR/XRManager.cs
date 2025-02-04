using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using UnityEngine.XR.Management;

namespace CardGameVR.XR
{
    public class XRManager : MonoBehaviour
    {
#if UNITY_EDITOR
        private static bool noVRFlag
            => PlayerPrefs.GetInt("no-vr", 0) == 1;

        [UnityEditor.MenuItem("CardGameVR/VR/Disable VR")]
        public static void DisableVR() => PlayerPrefs.SetInt("no-vr", 1);

        [UnityEditor.MenuItem("CardGameVR/VR/Enable VR")]
        public static void EnableVR() => PlayerPrefs.SetInt("no-vr", 0);
#else
        private static bool noVRFlag
            => System.Array.Exists(
                System.Environment.GetCommandLineArgs(),
                arg => arg == "--no-vr"
            );
#endif

        private static bool _isXRActive;
        public static bool IsXRActive() => _isXRActive;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void OnSubsystemRegistration()
        {
            if (!GameManager.startGameFlag) return;
            GameManager.AddOperation(nameof(XRManager));
            Debug.Log("XRManager.OnSubsystemRegistration");

            if (noVRFlag)
            {
                Debug.LogWarning("VR disabled by flag.");
                return;
            }

            if (!XRGeneralSettings.Instance.Manager.isInitializationComplete)
            {
                Debug.Log("Initializing XR...");
                XRGeneralSettings.Instance.Manager.InitializeLoaderSync();
            }

            if (!XRGeneralSettings.Instance.Manager.activeLoader)
            {
                Debug.LogError("XR loader is not active.");
                return;
            }

            Debug.Log("XR initialized. Starting subsystems...");
            XRGeneralSettings.Instance.Manager.StartSubsystems();

            _isXRActive = XRSettings.isDeviceActive;
            OnXRHeadsetChange.Invoke(_isXRActive);

            if (!XRSettings.isDeviceActive)
            {
                Debug.LogWarning("XR device is not active.");
                return;
            }

            Debug.Log("XR present. Starting in VR mode.");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void OnAfterSceneLoad()
        {
            if (!GameManager.startGameFlag) return;
            Debug.Log("XRManager.OnAfterSceneLoad");
            var go = new GameObject($"[{nameof(XRManager)}]");
            go.AddComponent<XRManager>();
            DontDestroyOnLoad(go);
        }

        public static readonly UnityEvent<bool> OnXRHeadsetChange = new();

        public void Start()
        {
            Debug.Log("XRManager.Start");
            GameManager.RemoveOperation(nameof(XRManager));
        }

        private void OnApplicationQuit()
        {
            if (!XRGeneralSettings.Instance.Manager.activeLoader) return;
            Debug.Log("Stopping XR...");
            XRGeneralSettings.Instance.Manager.StopSubsystems();
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        }


        public void Update()
        {
            if (_isXRActive == XRSettings.isDeviceActive) return;
            _isXRActive = XRSettings.isDeviceActive;
            OnXRHeadsetChange.Invoke(_isXRActive);
        }
    }
}