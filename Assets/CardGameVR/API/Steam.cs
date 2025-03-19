using Steamworks;
using UnityEngine;

namespace CardGameVR.API
{
    public static class Steam
    {
        private static bool IsSteamRunning()
        {
            try
            {
                return SteamAPI.IsSteamRunning();
            }catch
            {
                return false;
            }
        }

        private static bool IsLoggedOn()
        {
            try
            {
                return SteamUser.BLoggedOn();
            }
            catch
            {
                return false;
            }
        }

        public static bool CanUse() => IsSteamRunning() && IsLoggedOn();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            if (!GameManager.StartGameFlag) return;
            if (!IsSteamRunning())
            {
                Debug.LogError("Steam is not running.");
                return;
            }

            if (SteamAPI.Init()) return;
            Debug.LogError("SteamAPI.Init() failed.");
        }

        public static bool TryGetDisplayName(out string displayName)
        {
            if (!CanUse())
            {
                displayName = null;
                return false;
            }

            displayName = SteamFriends.GetPersonaName();
            return true;
        }

        public static bool TryGetId(out string id)
        {
            if (!CanUse())
            {
                id = null;
                return false;
            }

            id = SteamUser.GetSteamID().ToString();
            return true;
        }
    }
}