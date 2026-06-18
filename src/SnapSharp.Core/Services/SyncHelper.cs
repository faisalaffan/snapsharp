namespace SnapSharp.Services;

internal static class SyncHelper
{
    internal static T Run<T>(Func<Task<T>> asyncFunc)
    {
        return Task.Run(async () => await asyncFunc().ConfigureAwait(false))
            .GetAwaiter().GetResult();
    }
}
