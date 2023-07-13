using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResourceFrameWork
{
    internal static class AbObjectPool<T> where T : new()
    {
        // Object pool to avoid allocations.
        private static readonly ObjectPool<T> s_ListPool = new ObjectPool<T>(null, null);

        public static T Get()
        {
            return s_ListPool.Get();
        }

        public static void Release(T toRelease)
        {
            s_ListPool.Release(toRelease);
        }
    }
}
