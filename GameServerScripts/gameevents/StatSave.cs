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
		
		private static long m_lastBytesIn = 0;
		private static long m_lastBytesOut = 0;
		private static long m_lastMeasureTick = DateTime.Now.Ticks;
		private static int m_statFrequency = 60 * 1000; // 1 minute
		private static PerformanceCounter m_systemCpuUsedCounter;
		private static volatile Timer m_timer = null;
		
		[GameServerStartedEvent]
		public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
		{
			// Desactivated
			if (ServerProperties.Properties.STATSAVE_INTERVAL == -1)
				return;
			
			m_systemCpuUsedCounter = new PerformanceCounter();

			m_systemCpuUsedCounter.CategoryName = "Processor";
			m_systemCpuUsedCounter.CounterName = "% Processor Time";
			m_systemCpuUsedCounter.InstanceName = "_Total";
			// 1 min * INTERVAL
			m_statFrequency *= ServerProperties.Properties.STATSAVE_INTERVAL;
			m_timer = new Timer(new TimerCallback(SaveStats), null, m_statFrequency, m_statFrequency);
		}

		public static void SaveStats(object state)
		{
			long time = System.DateTime.Now.Ticks - m_lastMeasureTick;
			time /= 10000000L;
			long inRate = (Statistics.BytesIn - m_lastBytesIn) / time;
			long outRate = (Statistics.BytesOut - m_lastBytesOut) / time;

			m_lastBytesIn = Statistics.BytesIn;
			m_lastBytesOut = Statistics.BytesOut;

			int clients = WorldMgr.GetAllPlayingClientsCount();

			float cpu = m_systemCpuUsedCounter.NextValue();

			DBServerStats newstat = new DBServerStats();
			newstat.CPU = cpu;
			newstat.Clients = clients;
			newstat.Upload = (int)outRate/1024;
			newstat.Download = (int)inRate / 1024;
			GameServer.Database.AddNewObject(newstat);
			GameServer.Database.SaveObject(newstat);
			
			m_lastMeasureTick = DateTime.Now.Ticks;
		}
	}
}


namespace DOL.Database
{
	/// <summary>
	/// Database Storage of ServerStats
	/// </summary>
	[DataTable(TableName = "serverstats")]
	public class DBServerStats : DataObject
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected DateTime m_statdate;
		protected int m_clients;
		protected float m_cpu;
		protected int m_upload;
		protected int m_download;


		static bool m_autoSave;
		static bool m_init;

		public DBServerStats()
		{
			m_statdate = DateTime.Now;
			m_clients = 0;
			m_cpu = 0;
			m_upload = 0;
			m_download = 0;
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


		[GameServerStartedEvent]
		public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
		{
			if (!m_init && (GameServer.Database != null))
			{
				log.Info("DATABASE ServerStats LOADED");
				GameServer.Database.RegisterDataObject(typeof(DBServerStats));
				GameServer.Database.LoadDatabaseTable(typeof(DBServerStats));
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