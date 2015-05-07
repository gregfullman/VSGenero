using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero
{
    public static class Extensions
    {
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> rangeToAdd)
        {
            foreach (var item in rangeToAdd)
                set.Add(item);
        }
    }
}
