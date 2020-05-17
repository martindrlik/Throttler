using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using github.com.martindrlik;

namespace github.com.martindrlik.ThrottlerTests
{
    public class ThrottlerTest
    {
        [Fact]
        public async Task TryThrottleAsync_IsNewKey_Return0_true()
        {
            var throttler = new Throttler(() => DateTime.Now, 1);
            var (attemptCount, success) = await throttler.TryThrottleAsync(("x", 1), TimeSpan.FromDays(1));
            Assert.Equal(0, attemptCount);
            Assert.True(success);
        }

        [Fact]
        public async Task TryThrottleAsync_IsSameKey_Return1_false()
        {
            var throttler = new Throttler(() => DateTime.Now, 1);
            await throttler.TryThrottleAsync(("x", 1), TimeSpan.FromDays(1));
            var (attemptCount, success) = await throttler.TryThrottleAsync(("x", 1), TimeSpan.FromDays(1));
            Assert.Equal(1, attemptCount);
            Assert.False(success);
        }

        [Fact]
        public async Task TryThrottleAsync_IsDifferentKey_Cleared_Return0_true()
        {
            var throttler = new Throttler(() => DateTime.Now, 1);
            await throttler.TryThrottleAsync(("x", 1), TimeSpan.FromDays(1));
            var (attemptCount, success) = await throttler.TryThrottleAsync(("y", 1), TimeSpan.FromDays(1));
            Assert.Equal(0, attemptCount);
            Assert.True(success);
        }

        [Fact]
        public async Task TryThrottleAsync_IsCancelled_Throw()
        {
            var throttler = new Throttler(() => DateTime.Now, 1);
            await Assert.ThrowsAsync<TaskCanceledException>(async () => await throttler.TryThrottleAsync(
                ("x", 1), TimeSpan.FromDays(1), new CancellationToken(true)));
        }

        [Fact]
        public async Task TryThrottleAsync_DurationUpdated()
        {
            var nextDate = 0;
            DateTime NextDate()
            {
                nextDate++;
                return new DateTime(2049, 1, nextDate);
            }
            var throttler = new Throttler(NextDate, 1);
            var (attempt, success) = await throttler.TryThrottleAsync(("x", 1), TimeSpan.FromDays(1));
            Assert.Equal(0, attempt);
            Assert.True(success);
            (attempt, success) = await throttler.TryThrottleAsync(("x", 1), TimeSpan.FromDays(1));
            Assert.Equal(1, attempt);
            Assert.False(success);
            (attempt, success) = await throttler.TryThrottleAsync(("x", 1), TimeSpan.FromDays(1));
            Assert.Equal(0, attempt);
            Assert.True(success);
        }
    }
}
