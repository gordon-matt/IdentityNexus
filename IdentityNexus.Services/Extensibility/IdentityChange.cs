namespace IdentityNexus.Services.Extensibility;

/// <summary>Whether a change, or an editor section, is about a user or a role.</summary>
public enum IdentitySubjectKind
{
    User,
    Role,
}

public enum IdentityChangeType
{
    Created,
    Updated,
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
    Task OnChangedAsync(IdentityChange change, CancellationToken cancellationToken = default);
}