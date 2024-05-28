namespace ProtoHackersDotNet.Servers.Helpers;

public static class CTSHelper
{
    /// <summary>Links <paramref name="cancellationSource"/> with <paramref name="newToken"/>, producing a new
    /// <see cref="CancellationTokenSource"/> and storing it in <paramref name="cancellationSource"/> and disposing of the old one.
    /// </summary>
    /// <param name="cancellationSource">The reference to the cancellation source that will be linked.</param>
    /// <param name="newToken">The token to link the new cancellation source.</param>
    /// <returns>A token from the new <paramref name="cancellationSource"/>.</returns>
    public static CancellationToken LinkTokenSource(ref CancellationTokenSource cancellationSource, CancellationToken newToken)
    {
        var oldCancellationSource = cancellationSource;
        cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(newToken, cancellationSource.Token);
        oldCancellationSource.Dispose();
        return cancellationSource.Token;
    }
}