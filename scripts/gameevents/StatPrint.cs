/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.GameEvents
{
	/// <summary>
	/// 
	/// </summary>
	public class StatPrint
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static volatile Timer m_timer = null;
		private static long m_lastBytesIn = 0;
		private static long m_lastBytesOut = 0;
		private static long m_lastPacketsIn = 0;
		private static long m_lastPacketsOut = 0;
		private static long m_lastMeasureTick = DateTime.Now.Ticks;
		private static int m_statFrequency = 30000; // 30s
		private static PerformanceCounter m_systemCpuUsedCounter;
		private static PerformanceCounter m_processCpuUsedCounter;
		private static Hashtable m_timerStatsByMgr;

		public StatPrint()
		{
		}

		[GameServerStartedEvent]
		public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
		{
			m_timerStatsByMgr = new Hashtable();
			m_timer = new Timer(new TimerCallback(PrintStats), null, 10000, 0);
			try
			{
				m_systemCpuUsedCounter = new PerformanceCounter("Processor", "% processor time", "_total");
				m_systemCpuUsedCounter.NextValue();
			}
			catch (Exception ex)
			{
				m_systemCpuUsedCounter = null;
				if (log.IsWarnEnabled)
					log.Warn(ex.GetType().Name + " SystemCpuUsedCounter won't be available: " + ex.Message);
			}
			try
			{
				m_processCpuUsedCounter = new PerformanceCounter("Process", "% processor time", GetProcessCounterName());
				m_processCpuUsedCounter.NextValue();
			}
			catch (Exception ex)
			{
				m_processCpuUsedCounter = null;
				if (log.IsWarnEnabled)
					log.Warn(ex.GetType().Name + " ProcessCpuUsedCounter won't be available: " + ex.Message);
			}
		}
		
		/// <summary>
		/// Find the process counter name
		/// </summary>
		/// <returns></returns>
		public static string GetProcessCounterName()
		{
			Process process = Process.GetCurrentProcess();
			int id = process.Id;
			PerformanceCounterCategory perfCounterCat = new PerformanceCounterCategory("Process");
			foreach(DictionaryEntry entry in perfCounterCat.ReadCategory()["id process"])
			{
				string processCounterName = (string)entry.Key;
				if (((InstanceData)entry.Value).RawValue == id)
					return processCounterName;
			}
			return "";
		}

		[ScriptUnloadedEvent]
		public static void OnScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			Timer t = m_timer;
			m_timer = null;
			if (t != null)
			{
				t.Change(Timeout.Infinite, Timeout.Infinite);
				t.Dispose();
			}
			if (m_systemCpuUsedCounter != null)
			{
				m_systemCpuUsedCounter.Close();
				m_systemCpuUsedCounter = null;
			}
			if (m_processCpuUsedCounter != null)
			{
				m_processCpuUsedCounter.Close();
				m_processCpuUsedCounter = null;
			}
		}

		/// <summary>
		/// print out some periodic information on server statistics
		/// </summary>
		/// <param name="state"></param>
		public static void PrintStats(object state)
		{
			try
			{
				//Don't enable this line unless you have memory issues and
				//need more details in memory usage
				//GC.Collect();

				long time = DateTime.Now.Ticks - m_lastMeasureTick;
				time /= 10000000L;
				if (time < 1)
				{
					log.Warn("Time has not changed since last call of PrintStats");
					time = 1; // prevent division by zero?
				}
				long inRate = (Statistics.BytesIn - m_lastBytesIn)/time;
				long outRate = (Statistics.BytesOut - m_lastBytesOut)/time;
				long inPckRate = (Statistics.PacketsIn - m_lastPacketsIn)/time;
				long outPckRate = (Statistics.PacketsOut - m_lastPacketsOut)/time;
				m_lastBytesIn = Statistics.BytesIn;
				m_lastBytesOut = Statistics.BytesOut;
				m_lastPacketsIn = Statistics.PacketsIn;
				m_lastPacketsOut = Statistics.PacketsOut;

				if (log.IsInfoEnabled)
				{
					StringBuilder stats = new StringBuilder(256)
						.Append("-stats- Mem=").Append(GC.GetTotalMemory(false)/1024).Append("kb")
						.Append("  Clients=").Append(GameServer.Instance.ClientCount)
						.Append("  Down=").Append(inRate/1024).Append( "kb/s (" ).Append(Statistics.BytesIn/1024/1024).Append( "MB)" )
						.Append("  Up=").Append(outRate/1024).Append( "kb/s (" ).Append(Statistics.BytesOut/1024/1024).Append( "MB)" )
						.Append("  In=").Append(inPckRate).Append( "pck/s (" ).Append(Statistics.PacketsIn/1000).Append( "K)" )
						.Append("  Out=").Append(outPckRate).Append( "pck/s (" ).Append(Statistics.PacketsOut/1000).Append( "K)" );

					lock (m_timerStatsByMgr.SyncRoot)
					{
						foreach (GameTimer.TimeManager mgr in WorldMgr.GetRegionTimeManagers())
						{
							TimerStats ts = (TimerStats) m_timerStatsByMgr[mgr];
							if (ts == null)
							{
								ts = new TimerStats();
								m_timerStatsByMgr.Add(mgr, ts);
							}
							long curInvoked = mgr.InvokedCount;
							long invoked = curInvoked - ts.InvokedCount;
							stats.Append("  ").Append(mgr.Name).Append('=').Append(invoked/time).Append("t/s (")
								.Append(mgr.ActiveTimers).Append(')');
							ts.InvokedCount = curInvoked;
						}
					}

					if (m_systemCpuUsedCounter != null)
						stats.Append("  CPU=").Append(m_systemCpuUsedCounter.NextValue().ToString("0.0")).Append('%');
					if (m_processCpuUsedCounter != null)
						stats.Append("  DOL=").Append(m_processCpuUsedCounter.NextValue().ToString("0.0")).Append('%');

					log.Info(stats);
				}

				if (log.IsFatalEnabled)
				{
					lock (m_timerStatsByMgr.SyncRoot)
					{
						foreach (GameTimer.TimeManager mgr in WorldMgr.GetRegionTimeManagers())
						{
							TimerStats ts = (TimerStats) m_timerStatsByMgr[mgr];
							if (ts == null) continue;

							long curTick = mgr.CurrentTime;
							if (ts.Time == curTick)
							{
								log.FatalFormat("{0} stopped ticking; timer stacktrace:\n{1}\n", mgr.Name, Util.FormatStackTrace(mgr.GetStacktrace()));
								log.FatalFormat("NPC update stacktrace:\n{0}\n", Util.FormatStackTrace(WorldMgr.GetNpcUpdateStacktrace()));
								log.FatalFormat("Relocate() stacktrace:\n{0}\n", Util.FormatStackTrace(WorldMgr.GetRelocateRegionsStacktrace()));
								log.FatalFormat("Packethandlers stacktraces:\n{0}\n", PacketProcessor.GetConnectionThreadpoolStacks());
							}

							ts.Time = curTick;
						}
					}
				}
			}
			catch (Exception e)
			{
				log.Error("stats log callback", e);
			}
			finally
			{
				m_lastMeasureTick = DateTime.Now.Ticks;
				m_timer.Change(m_statFrequency, Timeout.Infinite);
			}
		}
		
		public class TimerStats
		{
			public long InvokedCount;
			public long Time = -1;
		}
	}
}