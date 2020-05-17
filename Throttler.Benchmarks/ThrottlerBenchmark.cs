using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace github.com.martindrlik.ThrottlerBenchmarks
{
    public class ThrottlerBenchmark
    {
        readonly TimeSpan day = TimeSpan.FromDays(1);
        readonly Throttler throttler = new Throttler(() => DateTime.Now, 1);

        [Benchmark]
        public async ValueTask<(int, bool)> TryThrottleAsync() =>
            await throttler.TryThrottleAsync(("Feature String", 5670000), day);
    }
}
