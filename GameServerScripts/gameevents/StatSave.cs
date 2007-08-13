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
using DOL.Database;
using DOL.Database.Attributes;
using DOL.Events;
using DOL.GS;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.GameEvents
{
	class StatSave
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly int INITIAL_DELAY = 60000;
		
		private static long m_lastBytesIn = 0;
		private static long m_lastBytesOut = 0;
		private static long m_lastMeasureTick = DateTime.Now.Ticks;
		private static int m_statFrequency = 60 * 1000; // 1 minute
		private static PerformanceCounter m_systemCpuUsedCounter = null;
		private static PerformanceCounter m_processCpuUsedCounter = null;
		
		private static volatile Timer m_timer = null;
		
		[GameServerStartedEvent]
		public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
		{
			// Desactivated
			if (ServerProperties.Properties.STATSAVE_INTERVAL == -1)
				return;
			
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
			// 1 min * INTERVAL
			m_statFrequency *= ServerProperties.Properties.STATSAVE_INTERVAL;
			lock (typeof(StatSave))
			{
				m_timer = new Timer(new TimerCallback(SaveStats), null, INITIAL_DELAY, Timeout.Infinite);
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

		/// <summary>
		/// Find the process counter name
		/// </summary>
		/// <returns></returns>
		public static string GetProcessCounterName()
		{
			Process process = Process.GetCurrentProcess();
			int id = process.Id;
			PerformanceCounterCategory perfCounterCat = new PerformanceCounterCategory("Process");
			foreach (DictionaryEntry entry in perfCounterCat.ReadCategory()["id process"])
			{
				string processCounterName = (string)entry.Key;
				if (((InstanceData)entry.Value).RawValue == id)
					return processCounterName;
			}
			return "";
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

				float cpu = 0;
				if (m_systemCpuUsedCounter != null)
					cpu = m_systemCpuUsedCounter.NextValue(); 
				if (m_processCpuUsedCounter != null)
					cpu = m_processCpuUsedCounter.NextValue();

				long totalmem = GC.GetTotalMemory(false);
			
				DBServerStats newstat = new DBServerStats();
				newstat.CPU = cpu;
				newstat.Clients = clients;
				newstat.Upload = (int)outRate/1024;
				newstat.Download = (int)inRate / 1024;
				newstat.Memory = totalmem / 1024;
				GameServer.Database.AddNewObject(newstat);
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


namespace DOL.Database
{
	/// <summary>
	/// Database Storage of ServerStats
	/// </summary>
	[DataTable(TableName = "server_stats")]
	public class DBServerStats : DataObject
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected DateTime m_statdate;
		protected int m_clients;
		protected float m_cpu;
		protected int m_upload;
		protected int m_download;
		protected long m_memory;

		static bool m_autoSave;
		static bool m_init;

		public DBServerStats()
		{
			m_statdate = DateTime.Now;
			m_clients = 0;
			m_cpu = 0;
			m_upload = 0;
			m_download = 0;
			m_memory = 0;
			m_autoSave = false;
		}

		override public bool AutoSave
		{
			get { return m_autoSave; }
			set { m_autoSave = value; }
		}

		[DataElement(AllowDbNull = false)]
		public DateTime StatDate
		{
			get { return m_statdate; }
			set
			{
				Dirty = true;
				m_statdate = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public int Clients
		{
			get { return m_clients; }
			set
			{
				Dirty = true;
				m_clients = value;
			}
		}
		[DataElement(AllowDbNull = false)]
		public float CPU
		{
			get { return m_cpu; }
			set
			{
				Dirty = true;
				m_cpu = value;
			}
		}
		[DataElement(AllowDbNull = false)]
		public int Upload
		{
			get { return m_upload; }
			set
			{
				Dirty = true;
				m_upload = value;
			}
		}
		[DataElement(AllowDbNull = false)]
		public int Download
		{
			get { return m_download; }
			set
			{
				Dirty = true;
				m_download = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public long Memory
		{
			get { return m_memory; }
			set
			{
				Dirty = true;
				m_memory = value;
			}
		}


		[GameServerStartedEvent]
		public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
		{
			if (!m_init && (GameServer.Database != null))
			{
				log.Info("DATABASE ServerStats LOADED");
				GameServer.Database.RegisterDataObject(typeof(DBServerStats));
#warning it doesn't do anything with mysql database
				//GameServer.Database.LoadDatabaseTable(typeof(DBServerStats));
				m_init = true;
			}
		}
		[ScriptUnloadedEvent]
		public static void OnScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			//unregister tables
		}
	}
}