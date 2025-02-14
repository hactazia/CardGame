using Steamworks;
using UnityEngine;

namespace CardGameVR.API
{
    public static class Steam
    {
        private static bool isSteamRunning => SteamAPI.IsSteamRunning();
        private static bool isSteamUserLoggedOn => isSteamRunning && SteamUser.BLoggedOn();
        public static bool CanUse() => isSteamUserLoggedOn;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            if (!GameManager.StartGameFlag) return;
            if (!isSteamRunning)
            {
                Debug.LogError("Steam is not running.");
                return;
            }

            if (SteamAPI.Init()) return;
            Debug.LogError("SteamAPI.Init() failed.");
        }

        public static bool TryGetDisplayName(out string displayName)
        {
            if (!isSteamUserLoggedOn)
            {
                displayName = null;
                return false;
            }

            displayName = SteamFriends.GetPersonaName();
            return true;
        }

        public static bool TryGetId(out string id)
        {
            if (!isSteamUserLoggedOn)
            {
                id = null;
                return false;
            }

            id = SteamUser.GetSteamID().ToString();
            return true;
        }
    }
}