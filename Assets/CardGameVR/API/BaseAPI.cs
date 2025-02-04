using Unity.Services.Authentication;
using UnityEngine;

namespace CardGameVR.API
{
    public static class BaseAPI
    {
        public static readonly string DefaultUserName = $"Player-{Random.Range(1000, 10000)}";

        public static string GetDisplayName()
        {
            if (Steam.TryGetDisplayName(out var displayName))
                return displayName;
            // if(Epic.TryGetDisplayName(out var displayName))
            //    return displayName;
            return DefaultUserName;
        }

        public static string GetId()
        {
            if (Steam.TryGetId(out var userId))
                return userId;
            // if(Epic.TryGetId(out var userId))
            //    return userId;
            return AuthenticationService.Instance.PlayerId;
        }
    }
}