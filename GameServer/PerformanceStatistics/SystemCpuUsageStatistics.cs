using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace DOL.PerformanceStatistics
{
    public class SystemCpuUsagePercent : IPerformanceStatistic
    {
        private IPerformanceStatistic processorTimeRatioStatistic;

        public SystemCpuUsagePercent()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                processorTimeRatioStatistic = new PerformanceCounterStatistic("Processor", "% processor time", "_total");
            }
            else
            {
                processorTimeRatioStatistic = new LinuxSystemCpuUsagePercent();
            }
        }

        public float GetNextValue()
            => processorTimeRatioStatistic.GetNextValue();
    }

    [UnsupportedOSPlatform("Windows")]
    internal class LinuxSystemCpuUsagePercent : IPerformanceStatistic
    {
        private IPerformanceStatistic processorTimeStatistic;
        private IPerformanceStatistic idleTimeStatistic;

        public LinuxSystemCpuUsagePercent()
        {
            processorTimeStatistic = new PerSecondStatistic(new LinuxTotalProcessorTimeInSeconds());
            idleTimeStatistic = new PerSecondStatistic(new LinuxSystemIdleProcessorTimeInSeconds());
        }

        public float GetNextValue()
        {
            var cpuUsage = (1 - (idleTimeStatistic.GetNextValue() / processorTimeStatistic.GetNextValue()));
            return cpuUsage * 100;
        }
    }

    [UnsupportedOSPlatform("Windows")]
    internal class LinuxTotalProcessorTimeInSeconds : IPerformanceStatistic
    {
        public float GetNextValue()
        {
            var cpuTimeInSeconds = File.ReadAllText("/proc/stat").Split('\n')
                .First().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Skip(1).Select(c => Convert.ToInt64(c)).Aggregate(0L, (a, b) => a + b) * 0.001f;
            return cpuTimeInSeconds;
        }
    }

    [UnsupportedOSPlatform("Windows")]
    internal class LinuxSystemIdleProcessorTimeInSeconds : IPerformanceStatistic
    {
        public float GetNextValue()
        {
            var cpuIdleTimeString = File.ReadAllText("/proc/stat").Split('\n')
                .First().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[4];
            var cpuIdleTimeInSeconds = Convert.ToInt64(cpuIdleTimeString) * 0.001f;
            return cpuIdleTimeInSeconds;
        }
    }
}