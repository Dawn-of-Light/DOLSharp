using System;
using System.Diagnostics;

namespace DOL.PerformanceStatistics
{
    public class ProgramCpuUsagePercentStatistic : IPerformanceStatistic
    {
        private IPerformanceStatistic processorTimeRatioStatistic;

        public ProgramCpuUsagePercentStatistic()
        {
            processorTimeRatioStatistic = new PerSecondStatistic(new TotalProgramProcessorTimeInSeconds());
        }

        public float GetNextValue()
            => processorTimeRatioStatistic.GetNextValue() / Environment.ProcessorCount * 100;
    }

    internal class TotalProgramProcessorTimeInSeconds : IPerformanceStatistic
    {
        public float GetNextValue()
            => (float)Process.GetCurrentProcess().TotalProcessorTime.TotalSeconds;
    }
}