using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace DOL.PerformanceStatistics
{
    public class PageFaultsPerSecondStatistic : IPerformanceStatistic
    {
        IPerformanceStatistic performanceStatistic;

        public PageFaultsPerSecondStatistic()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                performanceStatistic = new PerformanceCounterStatistic("Memory", "Pages/sec", null);
            }
            else 
            {
                performanceStatistic = new LinuxPageFaultsPerSecondStatistic();
            }
        }

        public float GetNextValue() => performanceStatistic.GetNextValue();
    }

    [UnsupportedOSPlatform("Windows")]
    internal class LinuxPageFaultsPerSecondStatistic : IPerformanceStatistic
    {
        private IPerformanceStatistic memoryFaultsPerSecondStatistic;

        public LinuxPageFaultsPerSecondStatistic()
        {
            memoryFaultsPerSecondStatistic = new PerSecondStatistic(new LinuxTotalPageFaults());
        }

        public float GetNextValue() => memoryFaultsPerSecondStatistic.GetNextValue();
    }

    internal class LinuxTotalPageFaults : IPerformanceStatistic
    {
        public float GetNextValue()
        {
            return Convert.ToInt64(File.ReadAllText("/proc/vmstat").Split('\n')
                    .Where(s => s.StartsWith("pgfault"))
                    .First().Split(' ')[1]);
        }
    }
}