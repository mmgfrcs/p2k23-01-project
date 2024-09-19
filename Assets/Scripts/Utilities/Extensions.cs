using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AdInfinitum.Utilities
{
    public static class Extensions
    {
        public static Vector3Int ToVector3Int(this Vector2Int v2)
        {
            return new Vector3Int(v2.x, v2.y, 0);
        }
    
        public static Vector2Int ToVector2Int(this Vector3Int v3)
        {
            return new Vector2Int(v3.x, v3.y);
        }
    
        public static Vector3 ToVector3(this Vector3Int v3)
        {
            return new Vector3(v3.x, v3.y, v3.z);
        }

        public static T PickOne<T>(this T[] arr)
        {
            if (arr.Length == 0) return default;
            return arr[Random.Range(0, arr.Length)];
        }
    
        public static T PickOne<T>(this IList<T> arr)
        {
            return arr[Random.Range(0, arr.Count)];
        }

        public static uint Sum<TSource>(
            this IEnumerable<TSource> source, Func<TSource, uint> summer)
        {
            uint total = 0;

            foreach(var item in source)
                total += summer(item);

            return total;
        }
    }
}
