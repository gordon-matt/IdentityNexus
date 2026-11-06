namespace IdentityNexus;

/// <summary>
/// A tenant id for the current request. <see cref="None"/> means the host has no tenant
/// in this request, and IdentityNexus does not filter or stamp.
/// </summary>
public readonly struct TenantKey<TKey>
{
    /// <summary>
    /// Gets or sets a value indicating whether this instance has value.
    /// </summary>
    public bool HasValue { get; private init; }

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    public TKey Value { get; private init; }

    /// <summary>
    /// Gets the none.
    /// </summary>
    public static TenantKey<TKey> None => default;

    /// <summary>
    /// Gets the of.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The of.</returns>
    public static TenantKey<TKey> Of(TKey value) => new() { HasValue = true, Value = value };
}

/// <summary>
/// Provides tenant key.
/// </summary>
public static class TenantKey
{
    /// <summary>
    /// Gets the of nullable.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <param name="value">The value.</param>
    /// <returns>The of nullable.</returns>
    public static TenantKey<TKey?> OfNullable<TKey>(TKey? value)
        where TKey : struct
        => value is null ? default : TenantKey<TKey?>.Of(value);
}