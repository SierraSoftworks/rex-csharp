using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Rex
{
    public static class Extensions
    {
        public static T Random<T>(this IEnumerable<T> e) => e.RandomWith(new Random());

        public static T RandomWith<T>(this IEnumerable<T> e, Random rng)
        {
            var defaultValue = default(T);
            var result = e.RandomOrDefaultWith(defaultValue, rng);
            if (object.ReferenceEquals(result, defaultValue))
                throw new InvalidOperationException("Sequence was empty");
            return result;
        }

        public static T RandomOrDefault<T>(this IEnumerable<T> e, T defaultValue)
        {
            return e.RandomOrDefaultWith(defaultValue, new Random());
        }

        public static T RandomOrDefaultWith<T>(this IEnumerable<T> e, T defaultValue, Random rng)
        {
            var current = default(T);
            var count = 0;

            foreach (var item in e)
                if (rng.Next(++count) == 0)
                    current = item;

            if (count == 0)
                return defaultValue;

            return current;
        }

        public static async Task<T> Random<T>(this IAsyncEnumerable<T> e) => await e.RandomWith(new Random());

        public static async Task<T> RandomWith<T>(this IAsyncEnumerable<T> e, Random rng)
        {
            var defaultValue = default(T);
            var result = await e.RandomOrDefaultWith(defaultValue, rng);
            if (object.ReferenceEquals(result, defaultValue))
                throw new InvalidOperationException("Sequence was empty");
            return result;
        }

        public static async Task<T> RandomOrDefault<T>(this IAsyncEnumerable<T> e, T defaultValue)
        {
            return await e.RandomOrDefaultWith(defaultValue, new Random());
        }
        public static async Task<T> RandomOrDefaultWith<T>(this IAsyncEnumerable<T> e, T defaultValue, Random rng)
        {
            var current = default(T);
            var count = 0;

            await foreach (var item in e)
                if (rng.Next(++count) == 0)
                    current = item;

            if (count == 0)
                return defaultValue;

            return current;
        }

        public static async IAsyncEnumerable<T> Where<T>(this IAsyncEnumerable<T> e, Func<T, bool> predicate)
        {
            await foreach (var item in e)
            {
                if (predicate(item))
                    yield return item;
            }
        }

        public static async Task<IEnumerable<T>> ToEnumerable<T>(this IAsyncEnumerable<T> e)
        {
            var ll = new LinkedList<T>();
            await foreach (var item in e)
            {
                ll.AddLast(item);
            }

            return ll.ToArray();
        }

        public static ActionResult<T> ToActionResult<T>(this T result)
        {
            return new ActionResult<T>(result);
        }
    }
}