namespace IdentityNexus.Services.Extensibility;

/// <summary>Passes a change to every registered <see cref="IIdentityChangeObserver"/>.</summary>
public sealed class IdentityChangeNotifier(
    IEnumerable<IIdentityChangeObserver> observers,
    ILogger<IdentityChangeNotifier> logger)
{
    /// <summary>
    /// Notifies the value.
    /// </summary>
    /// <param name="kind">The kind.</param>
    /// <param name="id">The identifier.</param>
    /// <param name="type">The type.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
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