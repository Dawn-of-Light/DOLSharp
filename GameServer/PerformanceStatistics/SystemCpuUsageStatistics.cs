using System;
using System.Diagnostics;
using System.Linq;

namespace DOL.PerformanceStatistics
{
    public class SystemCpuUsagePercent : IPerformanceStatistic
    {
        private IPerformanceStatistic processorTimeRatioStatistic;

        public SystemCpuUsagePercent()
        {
            processorTimeRatioStatistic = new PerSecondStatistic(new TotalSystemProcessorTimeInSeconds());
        }

        public float GetNextValue()
            => processorTimeRatioStatistic.GetNextValue() / Environment.ProcessorCount * 100;
    }

    internal class TotalSystemProcessorTimeInSeconds : IPerformanceStatistic
    {
        public float GetNextValue()
        {
            return (float)Process.GetProcesses()
                .Select(p => p.TotalProcessorTime)
                .Aggregate(TimeSpan.Zero, (p1, p2) => p1 + p2).TotalSeconds;
        }
    }
}