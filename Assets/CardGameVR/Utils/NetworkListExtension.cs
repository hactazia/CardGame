using System;
using System.Collections.Generic;
using Unity.Netcode;

namespace CardGameVR.Utils
{
    public class NetworkListExtension
    {
        public static List<T> ToList<T>(NetworkList<T> networkList) where T : unmanaged, IEquatable<T>
        {
            var list = new List<T>();
            foreach (var item in networkList)
                list.Add(item);
            return list;
        }

        public static T[] ToArray<T>(NetworkList<T> networkList) where T : unmanaged, IEquatable<T>
            => ToList(networkList).ToArray();
    }
}