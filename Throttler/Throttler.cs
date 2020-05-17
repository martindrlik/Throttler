using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace github.com.martindrlik
{
    public class Throttler : IThrottler, IDisposable
    {
        readonly Func<DateTime> nowFunc;
        readonly IDictionary<IStructuralEquatable, Data> attempts;
        readonly SemaphoreSlim semaphore;
        readonly int maxCount;

        public Throttler(Func<DateTime> nowFunc, int maxCount)
        {
            if (nowFunc == null)
                throw new ArgumentNullException(nameof(nowFunc));
            attempts = new Dictionary<IStructuralEquatable, Data>(maxCount);
            semaphore = new SemaphoreSlim(1, 1);
            this.maxCount = maxCount;
            this.nowFunc = nowFunc;
        }

        public void Dispose()
        {
            semaphore.Dispose();
        }

        public async ValueTask<(int attemptCount, bool success)> TryThrottleAsync<T>(T key, TimeSpan duration)
            where T : IStructuralEquatable
        {
            if (await semaphore.WaitAsync(TimeSpan.FromSeconds(1)))
                return TryThrottle(key, duration);
            return (0, false);
        }

        public async ValueTask<(int attemptCount, bool success)> TryThrottleAsync<T>(T key, TimeSpan duration, CancellationToken cancellationToken)
            where T : IStructuralEquatable
        {
            await semaphore.WaitAsync(cancellationToken);
            return TryThrottle(key, duration);
        }

        (int attemptCount, bool success) TryThrottle<T>(T key, TimeSpan duration)
            where T : IStructuralEquatable
        {
            try
            {
                var now = nowFunc();
                if (!attempts.TryGetValue(key, out var data))
                {
                    if (attempts.Count == maxCount)
                        attempts.Clear();
                    attempts.Add(key, new Data { Deadline = now.Add(duration) });
                    return (0, true);
                }
                if (data.Deadline < now)
                {
                    data.AttemptCount = 0;
                    data.Deadline = now.Add(duration);
                    return (0, true);
                }
                data.AttemptCount++;
                return (data.AttemptCount, false);
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
