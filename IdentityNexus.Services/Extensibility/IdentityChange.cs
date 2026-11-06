namespace IdentityNexus.Services.Extensibility;

/// <summary>Whether a change, or an editor section, is about a user or a role.</summary>
public enum IdentitySubjectKind
{
    /// <summary>
    /// The user value.
    /// </summary>
    User,
    /// <summary>
    /// The role value.
    /// </summary>
    Role,
}

/// <summary>
/// Specifies the identity change type.
/// </summary>
public enum IdentityChangeType
{
    /// <summary>
    /// The created value.
    /// </summary>
    Created,
    /// <summary>
    /// The updated value.
    /// </summary>
    Updated,
    /// <summary>
    /// The deleted value.
    /// </summary>
    Deleted,
}

/// <summary>A user or role that IdentityNexus has just written.</summary>
public sealed record IdentityChange(IdentitySubjectKind Kind, string Id, IdentityChangeType Type);

/// <summary>
/// Told about every user or role IdentityNexus creates, updates or deletes, after the change is saved.
/// Lets a host react without IdentityNexus knowing why: clear a cache, write an audit entry, sync another system.
/// A failure is logged and does not undo the change.
/// </summary>
public interface IIdentityChangeObserver
{
    /// <summary>
    /// Handles the changed event.
    /// </summary>
    /// <param name="change">The change.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task OnChangedAsync(IdentityChange change, CancellationToken cancellationToken = default);
}