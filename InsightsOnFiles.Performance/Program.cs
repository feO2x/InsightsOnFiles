using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace InsightsOnFiles.Performance
{
    public static class Program
    {
        private static IConfig CreateDefaultConfig() => DefaultConfig.Instance
                                                                     .With(Job.Core, Job.Clr)
                                                                     .With(MemoryDiagnoser.Default);

        public static void Main(string[] args) =>
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly)
                             .Run(args, CreateDefaultConfig());
    }
}