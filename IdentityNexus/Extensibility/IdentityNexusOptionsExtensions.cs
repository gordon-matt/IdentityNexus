using IdentityNexus.Services.Extensibility;

namespace IdentityNexus.Extensibility;

public static class IdentityNexusOptionsExtensions
{
    extension(IdentityNexusOptions options)
    {
        /// <summary>Adds a tab to the user editor.</summary>
        /// <param name="managedClaimTypes">Claim types the section edits, hidden from the generic Claims list.</param>
        public IdentityNexusOptions AddUserSection<TSection>(string title, params string[] managedClaimTypes)
            where TSection : IdentityNexusSection
        {
            options.UserSections.Add(new EditorSection(RequireTitle(title), typeof(TSection)));
            ManageClaimTypes(options, managedClaimTypes);
            return options;
        }

        /// <summary>Adds a tab to the role editor.</summary>
        /// <param name="managedClaimTypes">Claim types the section edits, hidden from the generic Claims list.</param>
        public IdentityNexusOptions AddRoleSection<TSection>(string title, params string[] managedClaimTypes)
            where TSection : IdentityNexusSection
        {
            options.RoleSections.Add(new EditorSection(RequireTitle(title), typeof(TSection)));
            ManageClaimTypes(options, managedClaimTypes);
            return options;
        }
    }

    private static void ManageClaimTypes(IdentityNexusOptions options, string[] claimTypes)
    {
        foreach (string type in claimTypes.Where(type => !string.IsNullOrWhiteSpace(type)))
        {
            options.ManagedClaimTypes.Add(type.Trim());
        }
    }

    private static string RequireTitle(string title) =>
        string.IsNullOrWhiteSpace(title)
            ? throw new ArgumentException("A section needs a title for its tab.", nameof(title))
            : title.Trim();
}