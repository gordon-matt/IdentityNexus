namespace IdentityNexus.Services.Extensibility;

/// <summary>Passes a change to every registered <see cref="IIdentityChangeObserver"/>.</summary>
public sealed class IdentityChangeNotifier(
    IEnumerable<IIdentityChangeObserver> observers,
    ILogger<IdentityChangeNotifier> logger)
{
    public async Task NotifyAsync(IdentitySubjectKind kind, string? id, IdentityChangeType type, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        var change = new IdentityChange(kind, id, type);
        foreach (var observer in observers)
        {
            try
            {
                await observer.OnChangedAsync(change, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "{Observer} failed on {Kind} {Id} ({ChangeType}).", observer.GetType().Name, kind, id, type);
            }
        }
    }
}