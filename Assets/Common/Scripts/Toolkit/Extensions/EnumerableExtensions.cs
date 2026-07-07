using System.Collections.Generic;
using System.Linq;

namespace OctoberStudio.Extensions
{
    public static class EnumerableExtensions
    {
        public static T Random<T>(this IList<T> list)
        {
            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        public static T Random<T>(this IEnumerable<T> enumerable)
        {
            var count = enumerable.Count();
            var rand = UnityEngine.Random.Range(0, count);
            var i = 0;
            foreach (var element in enumerable)
            {
                if (rand == i)
                    return element;
                i++;
            }

            return default;
        }

        public static T PopRandom<T>(this IList<T> list)
        {
            var random = UnityEngine.Random.Range(0, list.Count);
            var result = list[random];
            list.RemoveAt(random);
            return result;
        }
    }
}