using System;
using System.Diagnostics;
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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) performanceStatistic = new WindowsPageFaultsPerSecondStatistic();
            else performanceStatistic = new LinuxPageFaultsPerSecondStatistic();
        }

        public float GetNextValue() => performanceStatistic.GetNextValue();
    }

#if NET
    [SupportedOSPlatform("Windows")]
#endif
    internal class WindowsPageFaultsPerSecondStatistic : IPerformanceStatistic
    {
        PerformanceCounter performanceCounter;

        public WindowsPageFaultsPerSecondStatistic()
        {
            performanceCounter = new PerformanceCounter("Memory", "Pages/sec", null);
        }

        public float GetNextValue()
        {
            return performanceCounter.NextValue();
        }
    }

#if NET
    [UnsupportedOSPlatform("Windows")]
#endif
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