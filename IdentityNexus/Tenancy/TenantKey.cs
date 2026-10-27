namespace IdentityNexus;

/// <summary>
/// A tenant id for the current request. <see cref="None"/> means the host has no tenant
/// in this request, and IdentityNexus does not filter or stamp.
/// </summary>
public readonly struct TenantKey<TKey>
{
    public bool HasValue { get; private init; }

    public TKey Value { get; private init; }

    public static TenantKey<TKey> None => default;

    public static TenantKey<TKey> Of(TKey value) => new() { HasValue = true, Value = value };
}

public static class TenantKey
{
    public static TenantKey<TKey?> OfNullable<TKey>(TKey? value)
        where TKey : struct
        => value is null ? default : TenantKey<TKey?>.Of(value);
}