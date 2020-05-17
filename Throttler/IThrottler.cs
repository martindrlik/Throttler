using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace github.com.martindrlik
{
    public interface IThrottler
    {
        ValueTask<(int attemptCount, bool success)> TryThrottleAsync<T>(T key, TimeSpan duration) where T : IStructuralEquatable;
        ValueTask<(int attemptCount, bool success)> TryThrottleAsync<T>(T key, TimeSpan duration, CancellationToken cancellationToken) where T : IStructuralEquatable;
    }
}
