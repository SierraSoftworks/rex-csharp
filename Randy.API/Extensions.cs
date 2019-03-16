using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Randy.API
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
                if (rng.Next(count++) == 0)
                    current = item;

            if (count == 0)
                return defaultValue;

            return current;
        }

        public static ActionResult<T> ToActionResult<T>(this T result)
        {
            return new ActionResult<T>(result);
        }
    }
}