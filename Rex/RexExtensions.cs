using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SierraLib.API.Views;

namespace Rex
{
    public static class RexExtensions
    {
        public static T Random<T>(this IEnumerable<T> e)
            where T : notnull => e.RandomWith(new Random());

        public static T RandomWith<T>(this IEnumerable<T> e, Random rng)
            where T : notnull
        {
            if (e is null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            if (rng is null)
            {
                throw new ArgumentNullException(nameof(rng));
            }

            var current = e.First();
            var count = 0;

            foreach (var item in e)
                if (rng.Next(++count) == 0)
                    current = item;

            return current;
        }

        public static T? RandomOrDefault<T>(this IEnumerable<T> e, T? defaultValue)
            where T : class
        {
            return e.RandomOrDefaultWith(defaultValue, new Random());
        }

        [return: MaybeNull]
        public static T? RandomOrDefaultWith<T>(this IEnumerable<T> e, T? defaultValue, Random rng)
            where T : class
        {
            if (e is null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            if (rng is null)
            {
                throw new ArgumentNullException(nameof(rng));
            }

            var current = default(T);
            var count = 0;

            foreach (var item in e)
                if (rng.Next(++count) == 0)
                    current = item;

            if (count == 0)
                return defaultValue;

            return current;
        }

        public static async Task<T> Random<T>(this IAsyncEnumerable<T> e)
            where T : notnull => await e.RandomWith(new Random()).ConfigureAwait(false);

        public static async Task<T> RandomWith<T>(this IAsyncEnumerable<T> e, Random rng)
            where T : notnull
        {
            if (e is null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            if (rng is null)
            {
                throw new ArgumentNullException(nameof(rng));
            }

            var current = await e.FirstAsync().ConfigureAwait(false);
            var count = 0;

            await foreach (var item in e)
                if (rng.Next(++count) == 0)
                    current = item;

            return current;
        }

        public static async Task<T?> RandomOrDefault<T>(this IAsyncEnumerable<T> e, T? defaultValue)
            where T : class
        {
            return await e.RandomOrDefaultWith(defaultValue, new Random()).ConfigureAwait(false);
        }
        public static async Task<T?> RandomOrDefaultWith<T>(this IAsyncEnumerable<T> e, T? defaultValue, Random rng)
            where T : class
        {
            if (e is null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            if (rng is null)
            {
                throw new ArgumentNullException(nameof(rng));
            }

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
            if (e is null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            await foreach (var item in e)
            {
                if (predicate(item))
                    yield return item;
            }
        }

        public static async Task<T> FirstAsync<T>(this IAsyncEnumerable<T> e)
        {
            if (e is null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            await foreach (var item in e)
            {
                return item;
            }

            throw new InvalidOperationException("The source sequence is empty.");
        }

        public static async Task<IEnumerable<T>> ToEnumerable<T>(this IAsyncEnumerable<T> e)
        {
            if (e is null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            var ll = new LinkedList<T>();
            await foreach (var item in e)
            {
                ll.AddLast(item);
            }

            return ll.ToArray();
        }

        [return: MaybeNull]
        public static ActionResult<T>? ToActionResult<T>(this T? result)
            where T : class
        {
            if (result is null) return null;
            return new ActionResult<T>(result);
        }

        public static Guid GetOid(this ClaimsPrincipal user)
        {
            return Guid.ParseExact(user.GetClaimOrDefault("http://schemas.microsoft.com/identity/claims/objectidentifier") ?? Guid.NewGuid().ToString("D", CultureInfo.InvariantCulture), "D");
        }

        [SuppressMessage("Microsoft.Security", "CA5351", Justification = "MD5 is required for compatibility with Gravatar.")]
        [SuppressMessage("Microsoft.Globalization", "CA1308", Justification = "Lowercase is required for compatibility with Gravatar.")]
        public static string? GetEmailHash(this ClaimsPrincipal user)
        {
            var email = user.GetClaimOrDefault("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
            if (email is null)
            {
                return null;
            }

            using var md5hash = System.Security.Cryptography.MD5.Create();
            return string.Join("", Encoding.UTF8.GetBytes(email.ToLowerInvariant().Trim()).Select(b => b.ToString("x2", CultureInfo.InvariantCulture)));
        }

        public static string? GetClaimOrDefault(this ClaimsPrincipal user, string claim)
        {
            return user?.FindFirst(claim)?.Value;
        }

        public static TModel? ToModelOrDefault<TView, TModel>(this IRepresenter<TModel, TView> representer, TView? view)
            where TModel : class
            where TView : class, IView<TModel>
        {
            if (representer is null)
            {
                throw new ArgumentNullException(nameof(representer));
            }

            if (view is null) return null;

            return representer.ToModel(view);
        }

        public static TView? ToViewOrDefault<TView, TModel>(this IRepresenter<TModel, TView> representer, TModel? model)
            where TModel : class
            where TView : class, IView<TModel>
        {
            if (representer is null)
            {
                throw new ArgumentNullException(nameof(representer));
            }

            if (model is null) return null;

            return representer.ToView(model);
        }
    }
}