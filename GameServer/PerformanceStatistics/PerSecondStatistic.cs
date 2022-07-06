using System;
using System.Reflection;
using log4net;

namespace DOL.PerformanceStatistics
{
    internal class PerSecondStatistic : IPerformanceStatistic
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        DateTime lastMeasurementTime;
        float lastTotal;
        IPerformanceStatistic totalValueStatistic;
        float cachedLastStatisticValue = -1;

        public PerSecondStatistic(IPerformanceStatistic totalValueStatistic)
        {
            try
            {
                lastMeasurementTime = DateTime.UtcNow;
                this.totalValueStatistic = totalValueStatistic;
                lastTotal = totalValueStatistic.GetNextValue();
            }
            catch (Exception ex)
            {
                if (log.IsWarnEnabled)
                    log.Warn($"{ex.GetType().Name}: '{totalValueStatistic.GetType().Name}' counter won't be available: {ex.Message}");
            }
        }

        public float GetNextValue()
        {
            if (totalValueStatistic == null) return -1;
            var currentTime = DateTime.UtcNow;
            var secondsPassed = (currentTime - lastMeasurementTime).TotalSeconds;
            if (secondsPassed < 1) return cachedLastStatisticValue;

            var currentTotal = totalValueStatistic.GetNextValue();
            var valuePerSecond = (currentTotal - lastTotal) / secondsPassed;

            lastMeasurementTime = currentTime;
            lastTotal = currentTotal;
            cachedLastStatisticValue = (float)valuePerSecond;
            return cachedLastStatisticValue;
        }
    }

}