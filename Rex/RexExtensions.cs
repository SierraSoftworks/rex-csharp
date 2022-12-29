namespace Rex;

public static class RexExtensions
{
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