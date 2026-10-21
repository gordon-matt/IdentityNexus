using System.Linq.Expressions;
using System.Reflection;

namespace IdentityNexus.Services.Tenancy;

internal readonly record struct TenantScopeValue<TKey>(bool HasValue, TKey Value);

internal static class IdentityTenantScopes
{
    public static IServiceCollection AddUser<TUser, TKey>(
        this IServiceCollection services,
        Expression<Func<TUser, TKey>> tenantId,
        Func<IServiceProvider, TenantScopeValue<TKey>> current)
        where TUser : class
    {
        services.AddSingleton(TenantRule<TUser, TKey>.Create(tenantId, current));
        return services;
    }

    public static IServiceCollection AddRole<TRole, TKey>(
        this IServiceCollection services,
        Expression<Func<TRole, TKey>> tenantId,
        Func<IServiceProvider, TenantScopeValue<TKey>> current)
        where TRole : class
    {
        services.AddSingleton(TenantRule<TRole, TKey>.Create(tenantId, current));
        return services;
    }
}

internal interface ITenantRule<T>
    where T : class
{
    IQueryable<T> Apply(IQueryable<T> source, IServiceProvider services);

    void Assign(T entity, IServiceProvider services);

    bool Matches(T entity, IServiceProvider services);
}

internal sealed class TenantRule<T, TKey> : ITenantRule<T>
    where T : class
{
    private readonly Expression<Func<T, TKey>> property;
    private readonly Func<T, TKey> read;
    private readonly Action<T, TKey> write;
    private readonly Func<IServiceProvider, TenantScopeValue<TKey>> current;

    private TenantRule(
        Expression<Func<T, TKey>> property,
        Func<T, TKey> read,
        Action<T, TKey> write,
        Func<IServiceProvider, TenantScopeValue<TKey>> current)
    {
        this.property = property;
        this.read = read;
        this.write = write;
        this.current = current;
    }

    public static ITenantRule<T> Create(Expression<Func<T, TKey>> property, Func<IServiceProvider, TenantScopeValue<TKey>> current)
    {
        if (property.Body is not MemberExpression { Member: PropertyInfo info } || !info.CanWrite)
        {
            throw new ArgumentException("The tenant id selector must be a writable property.", nameof(property));
        }

        var target = Expression.Parameter(typeof(T), "entity");
        var value = Expression.Parameter(typeof(TKey), "value");
        var assign = Expression.Assign(Expression.Property(target, info), value);
        var write = Expression.Lambda<Action<T, TKey>>(assign, target, value).Compile();

        return new TenantRule<T, TKey>(property, property.Compile(), write, current);
    }

    public IQueryable<T> Apply(IQueryable<T> source, IServiceProvider services)
    {
        var scope = current(services);
        if (!scope.HasValue)
        {
            return source;
        }

        var parameter = property.Parameters[0];
        var body = Expression.Equal(property.Body, Expression.Constant(scope.Value, typeof(TKey)));
        var predicate = Expression.Lambda<Func<T, bool>>(body, parameter);
        return source.Where(predicate);
    }

    public void Assign(T entity, IServiceProvider services)
    {
        var scope = current(services);
        if (scope.HasValue)
        {
            write(entity, scope.Value);
        }
    }

    public bool Matches(T entity, IServiceProvider services)
    {
        var scope = current(services);
        return !scope.HasValue || EqualityComparer<TKey>.Default.Equals(read(entity), scope.Value);
    }
}
