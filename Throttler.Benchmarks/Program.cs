using BenchmarkDotNet.Running;

namespace github.com.martindrlik.ThrottlerBenchmarks
{
    class Program
    {
        static void Main(string[] args) =>
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
