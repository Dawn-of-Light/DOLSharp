using System;
using System.Collections;
using System.Reflection;
using System.Text;
using System.Threading;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.PerformanceStatistics;
using log4net;

namespace DOL.GS.GameEvents
{
    public class StatPrint
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static volatile Timer m_timer = null;
        private static long m_lastBytesIn = 0;
        private static long m_lastBytesOut = 0;
        private static long m_lastPacketsIn = 0;
        private static long m_lastPacketsOut = 0;
        private static long m_lastMeasureTick = DateTime.Now.Ticks;

        private static IPerformanceStatistic systemCpuUsagePercent;
        private static IPerformanceStatistic programCpuUsagePercent;
        private static IPerformanceStatistic pageFaultsPerSecond;
        private static IPerformanceStatistic diskTransfersPerSecond;

        private static Hashtable m_timerStatsByMgr;

        [GameServerStartedEvent]
        public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
        {
            lock (typeof(StatPrint))
            {
                m_timerStatsByMgr = new Hashtable();
                m_timer = new Timer(new TimerCallback(PrintStats), null, 10000, 0);
                systemCpuUsagePercent = TryToCreateStatistic(() => new SystemCpuUsagePercent());
                programCpuUsagePercent = TryToCreateStatistic(() => new CurrentProcessCpuUsagePercentStatistic());
                diskTransfersPerSecond = TryToCreateStatistic(() => new DiskTransfersPerSecondStatistic());
                pageFaultsPerSecond = TryToCreateStatistic(() => new PageFaultsPerSecondStatistic());
            }
        }

        [ScriptUnloadedEvent]
        public static void OnScriptUnloaded(DOLEvent e, object sender, EventArgs args)
        {
            lock (typeof(StatPrint))
            {
                if (m_timer != null)
                {
                    m_timer.Change(Timeout.Infinite, Timeout.Infinite);
                    m_timer.Dispose();
                    m_timer = null;
                }
            }
        }

        public static void PrintStats(object state)
        {
            try
            {
                long newTick = DateTime.Now.Ticks;
                long time = newTick - m_lastMeasureTick;
                m_lastMeasureTick = newTick;
                time /= 10000000L;
                if (time < 1)
                {
                    log.Warn("Time has not changed since last call of PrintStats");
                    time = 1; // prevent division by zero?
                }
                long inRate = (Statistics.BytesIn - m_lastBytesIn) / time;
                long outRate = (Statistics.BytesOut - m_lastBytesOut) / time;
                long inPckRate = (Statistics.PacketsIn - m_lastPacketsIn) / time;
                long outPckRate = (Statistics.PacketsOut - m_lastPacketsOut) / time;
                m_lastBytesIn = Statistics.BytesIn;
                m_lastBytesOut = Statistics.BytesOut;
                m_lastPacketsIn = Statistics.PacketsIn;
                m_lastPacketsOut = Statistics.PacketsOut;

                // Get threadpool info
                int iocpCurrent, iocpMin, iocpMax;
                int poolCurrent, poolMin, poolMax;
                ThreadPool.GetAvailableThreads(out poolCurrent, out iocpCurrent);
                ThreadPool.GetMinThreads(out poolMin, out iocpMin);
                ThreadPool.GetMaxThreads(out poolMax, out iocpMax);

                int globalHandlers = GameEventMgr.NumGlobalHandlers;
                int objectHandlers = GameEventMgr.NumObjectHandlers;

                if (log.IsInfoEnabled)
                {
                    StringBuilder stats = new StringBuilder(256)
                        .Append("-stats- Mem=").Append(GC.GetTotalMemory(false) / 1024 / 1024).Append("MB")
                        .Append("  Clients=").Append(GameServer.Instance.ClientCount)
                        .Append("  Down=").Append(inRate / 1024).Append("kb/s (").Append(Statistics.BytesIn / 1024 / 1024).Append("MB)")
                        .Append("  Up=").Append(outRate / 1024).Append("kb/s (").Append(Statistics.BytesOut / 1024 / 1024).Append("MB)")
                        .Append("  In=").Append(inPckRate).Append("pck/s (").Append(Statistics.PacketsIn / 1000).Append("K)")
                        .Append("  Out=").Append(outPckRate).Append("pck/s (").Append(Statistics.PacketsOut / 1000).Append("K)")
                        .AppendFormat("  Pool={0}/{1}({2})", poolCurrent, poolMax, poolMin)
                        .AppendFormat("  IOCP={0}/{1}({2})", iocpCurrent, iocpMax, iocpMin)
                        .AppendFormat("  GH/OH={0}/{1}", globalHandlers, objectHandlers);

                    lock (m_timerStatsByMgr.SyncRoot)
                    {
                        foreach (GameTimer.TimeManager mgr in WorldMgr.GetRegionTimeManagers())
                        {
                            TimerStats ts = (TimerStats)m_timerStatsByMgr[mgr];
                            if (ts == null)
                            {
                                ts = new TimerStats();
                                m_timerStatsByMgr.Add(mgr, ts);
                            }
                            long curInvoked = mgr.InvokedCount;
                            long invoked = curInvoked - ts.InvokedCount;
                            stats.Append("  ").Append(mgr.Name).Append('=').Append(invoked / time).Append("t/s (")
                                .Append(mgr.ActiveTimers).Append(')');
                            ts.InvokedCount = curInvoked;
                        }
                    }

                    AppendStatistic(stats, "CPU", systemCpuUsagePercent, "%");
                    AppendStatistic(stats, "DOL", programCpuUsagePercent, "%");
                    AppendStatistic(stats, "pg/s", pageFaultsPerSecond);
                    AppendStatistic(stats, "dsk/s", diskTransfersPerSecond);

                    log.Info(stats);
                }

                if (log.IsFatalEnabled)
                {
                    lock (m_timerStatsByMgr.SyncRoot)
                    {
                        foreach (GameTimer.TimeManager mgr in WorldMgr.GetRegionTimeManagers())
                        {
                            TimerStats ts = (TimerStats)m_timerStatsByMgr[mgr];
                            if (ts == null) continue;

                            long curTick = mgr.CurrentTime;
                            if (ts.Time == curTick)
                            {
                                log.FatalFormat("{0} stopped ticking; timer stacktrace:\n{1}\n", mgr.Name, mgr.GetFormattedStackTrace());
                                log.FatalFormat("NPC update stacktrace:\n{0}\n", WorldMgr.GetFormattedWorldUpdateStackTrace());
                                log.FatalFormat("Relocate() stacktrace:\n{0}\n", WorldMgr.GetFormattedRelocateRegionsStackTrace());
                                log.FatalFormat("Packethandlers stacktraces:\n{0}\n", PacketProcessor.GetConnectionThreadpoolStacks());
                            }

                            ts.Time = curTick;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Error("stats Log callback", e);
            }
            finally
            {
                lock (typeof(StatPrint))
                {
                    if (m_timer != null)
                    {
                        m_timer.Change(ServerProperties.Properties.STATPRINT_FREQUENCY, 0);
                    }
                }
            }
        }

        public class TimerStats
        {
            public long InvokedCount;
            public long Time = -1;
        }

        private static void AppendStatistic(StringBuilder stringBuilder, string shortName, IPerformanceStatistic statistic, string unit = "")
        {
            if (statistic == null) return;
            stringBuilder.Append($"  {shortName}={statistic.GetNextValue().ToString("0.0")}{unit}");
        }

        private static IPerformanceStatistic TryToCreateStatistic(Func<IPerformanceStatistic> createFunc)
        {
            try
            {
                return createFunc();
            }
            catch(Exception ex)
            {
                log.Error(ex);
                return null;
            }
        }
    }
}
