using System.Collections.Generic;
using System.Runtime.CompilerServices;
// ReSharper disable UnusedMethodReturnValue.Global

namespace DefaultECS.EntityFactory
{
    public static class Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (dict.ContainsKey(key))
                return false;

            dict.Add(key, value);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnqueueAll<TData>(this Queue<TData> queue, IEnumerable<TData> newData)
        {
            foreach (var dataItem in newData) 
                queue.Enqueue(dataItem);
        }
    }
}
