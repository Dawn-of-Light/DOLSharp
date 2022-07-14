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
using System.Reflection;
using System.Threading;
using DOL.Database;
using DOL.Events;
using DOL.PerformanceStatistics;
using log4net;

namespace DOL.GS.GameEvents
{
	class StatSave
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly int INITIAL_DELAY = 60000;
		
		private static long m_lastBytesIn = 0;
		private static long m_lastBytesOut = 0;
		private static long m_lastMeasureTick = DateTime.Now.Ticks;
		private static int m_statFrequency = 60 * 1000; // 1 minute
        private static IPerformanceStatistic programCpuUsagePercent;
		
		private static volatile Timer m_timer = null;
		
		[GameServerStartedEvent]
		public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
		{
			// Desactivated
			if (ServerProperties.Properties.STATSAVE_INTERVAL == -1)
				return;

			m_statFrequency *= ServerProperties.Properties.STATSAVE_INTERVAL;
			lock (typeof(StatSave))
			{
				m_timer = new Timer(new TimerCallback(SaveStats), null, INITIAL_DELAY, Timeout.Infinite);
			}

			programCpuUsagePercent = new CurrentProcessCpuUsagePercentStatistic();
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
		
		public static void SaveStats(object state)
		{
			try
			{
				long ticks = DateTime.Now.Ticks;
				long time = ticks - m_lastMeasureTick;
				m_lastMeasureTick = ticks;
				time /= 10000000L;
				if (time < 1)
				{
					log.Warn("Time has not changed since last call of SaveStats");
					time = 1; // prevent division by zero?
				}
				long inRate = (Statistics.BytesIn - m_lastBytesIn) / time;
				long outRate = (Statistics.BytesOut - m_lastBytesOut) / time;

				m_lastBytesIn = Statistics.BytesIn;
				m_lastBytesOut = Statistics.BytesOut;

				int clients = WorldMgr.GetAllPlayingClientsCount();
				var serverCpuUsage = programCpuUsagePercent.GetNextValue();
				
				long totalmem = GC.GetTotalMemory(false);
			
				ServerStats newstat = new ServerStats();
				newstat.CPU = serverCpuUsage >= 0 ? serverCpuUsage : 0;
				newstat.Clients = clients;
				newstat.Upload = (int)outRate/1024;
				newstat.Download = (int)inRate / 1024;
				newstat.Memory = totalmem / 1024;
				GameServer.Database.AddObject(newstat);
				GameServer.Database.SaveObject(newstat);
			}
			catch (Exception e)
			{
				log.Error("Updating server stats", e);
			}
			finally
			{
				lock (typeof(StatSave))
				{
					if (m_timer != null)
					{
						m_timer.Change(m_statFrequency, Timeout.Infinite);
					}
				}
			}
		}
	}
}