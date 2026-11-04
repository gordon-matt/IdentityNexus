namespace IdentityNexus.Services.Extensibility;

/// <summary>
/// A host component shown as an extra tab in the user or role editor.
/// Register one with <c>AddUserSection</c> or <c>AddRoleSection</c> from the IdentityNexus package.
/// </summary>
public sealed record EditorSection(string Title, Type ComponentType);