using Ardalis.Result;
using IdentityNexus.Services.Extensibility;
using Microsoft.AspNetCore.Components;

namespace IdentityNexus.Extensibility;

/// <summary>The user or role an editor section is showing.</summary>
/// <param name="Id">Null while the user or role is being created. <see cref="IdentityNexusSection.SaveAsync"/> receives the new id.</param>
/// <param name="Name">The user name or role name as last saved.</param>
public sealed record IdentitySubject(IdentitySubjectKind Kind, string? Id, string? Name)
{
    public bool IsNew => Id is null;
}

/// <summary>
/// Base for a host component shown as a tab in the user or role editor. IdentityNexus knows nothing about
/// what the section stores: a profile table, claims, settings in another system. The section loads its own
/// data and saves it when the editor's Save button runs.
/// </summary>
/// <remarks>
/// The section stays rendered while other tabs are open, so edits survive switching tabs.
/// A new instance is created for each user or role, so loading in <see cref="ComponentBase.OnInitializedAsync"/> is enough.
/// </remarks>
public abstract class IdentityNexusSection : ComponentBase
{
    [Parameter, EditorRequired]
    public IdentitySubject Subject { get; set; } = default!;

    /// <summary>
    /// Runs after the editor has saved its own fields. For a new user or role, <paramref name="id"/> is the id it was just given.
    /// Return a failed result to show its messages in the editor. The editor's own changes are already saved.
    /// </summary>
    public abstract Task<Result> SaveAsync(string id, CancellationToken cancellationToken = default);
}