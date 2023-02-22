using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace DOL.PerformanceStatistics
{
    public class CurrentProcessCpuUsagePercentStatistic : IPerformanceStatistic
    {
        private IPerformanceStatistic processorTimeRatioStatistic;

        public CurrentProcessCpuUsagePercentStatistic()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                processorTimeRatioStatistic = new PerformanceCounterStatistic("Process", "% processor time", Process.GetCurrentProcess().ProcessName);
            }
            else
            {
                processorTimeRatioStatistic = new LinuxCurrentProcessUsagePercentStatistic();
            }
        }

        public float GetNextValue()
            => processorTimeRatioStatistic.GetNextValue();
    }

    [UnsupportedOSPlatform("Windows")]
    internal class LinuxCurrentProcessUsagePercentStatistic : IPerformanceStatistic
    {
        private IPerformanceStatistic idleProcessorTimeStatistic = new PerSecondStatistic(new LinuxSystemIdleProcessorTimeInSeconds());
        private IPerformanceStatistic totalProcessorTimeStatistic = new PerSecondStatistic(new LinuxTotalProcessorTimeInSeconds());
        private IPerformanceStatistic currentProcessProcessorTimeStatistic = new PerSecondStatistic(new LinuxCurrentProcessProcessorTimeInSeconds());

        public float GetNextValue()
        {
            var idleTime = idleProcessorTimeStatistic.GetNextValue();
            var totalTime = totalProcessorTimeStatistic.GetNextValue();
            var processTime = currentProcessProcessorTimeStatistic.GetNextValue();
            return processTime / totalTime * 100 * Environment.ProcessorCount;
        }
    }

    [UnsupportedOSPlatform("Windows")]
    internal class LinuxCurrentProcessProcessorTimeInSeconds : IPerformanceStatistic
    {
        public float GetNextValue()
        {
            var pid = Process.GetCurrentProcess().Id;
            var statArray = File.ReadAllText($"/proc/{pid}/stat").Split(' ');
            var processorTime = Convert.ToInt64(statArray[13]) + Convert.ToInt64(statArray[14]);
            return processorTime * 0.001f;
        }
    }
}