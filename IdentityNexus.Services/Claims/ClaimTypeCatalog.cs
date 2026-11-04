using System.Reflection;
using System.Security.Claims;

namespace IdentityNexus.Services.Claims;

/// <summary>Friendly names for the well-known <see cref="ClaimTypes"/> values. Custom types are left as entered.</summary>
public static class ClaimTypeCatalog
{
    private static readonly Dictionary<string, string> NamesByType = All
        .GroupBy(option => option.Type, StringComparer.Ordinal)
        .ToDictionary(group => group.Key, group => group.First().Name, StringComparer.Ordinal);

    public static IReadOnlyList<ClaimTypeOption> All { get; } = typeof(ClaimTypes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Select(field => new ClaimTypeOption(field.Name, (string)field.GetValue(null)!))
        .OrderBy(option => option.Name, StringComparer.Ordinal)
        .ToArray();

    public static string DisplayName(string type) =>
        NamesByType.TryGetValue(type, out string? name) ? name : type;
}