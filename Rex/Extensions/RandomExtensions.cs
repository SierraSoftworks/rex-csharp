namespace Rex.Extensions;

public static class RandomExtensions
{
    public static T? Random<T>(this IEnumerable<T> e)
        where T : notnull => e.RandomWith(new Random());

    [SuppressMessage("Microsoft.Security", "CA5394", Justification = "We do not require cryptographically secure random generation for this.")]
    public static T? RandomWith<T>(this IEnumerable<T> e, Random rng)
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

        var current = default(T);
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

    [SuppressMessage("Microsoft.Security", "CA5394", Justification = "We do not require cryptographically secure random generation for this.")]
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

    [SuppressMessage("Microsoft.Security", "CA5394", Justification = "We do not require cryptographically secure random generation for this.")]
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

    [SuppressMessage("Microsoft.Security", "CA5394", Justification = "We do not require cryptographically secure random generation for this.")]
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
}