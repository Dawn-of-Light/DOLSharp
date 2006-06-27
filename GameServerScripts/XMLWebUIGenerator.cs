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
using System.IO;
using DOL.Database;
using DOL.Database.Attributes;
using DOL.Database.Connection;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Scripts
{
	/// <summary>
	/// Generates an XML version of the web ui
	/// </summary>
	public class XMLWebUIGenerator
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		[ScriptLoadedEvent]
		public static void OnScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			//Uncomment the following line to enable the WebUI
			//Start();
		}

		[ScriptUnloadedEvent]
		public static void OnScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			//Uncomment the following line to enable the WebUI
			//Stop();
		}


		private static System.Timers.Timer m_timer = null;

		[DataTable(TableName="ServerInfo")]
		public class ServerInfo : DataObject
		{
			private string m_dateTime = "NA";
			private string m_srvrName = "NA";
			private string m_aac = "NA";
			private string m_srvrType = "NA";
			private string m_srvrStatus = "NA";
			private int m_numClients = 0;
			private int m_numAccts = 0;
			private int m_numMobs = 0;
			private int m_numInvItems = 0;
			private int m_numPlrChars = 0;
			private int m_numMerchantItems = 0;
			private int m_numItemTemplates = 0;
			private int m_numWorldObj = 0;
			private static bool m_autoSave = true;

			public override bool AutoSave
			{
				get { return m_autoSave; }
				set { m_autoSave = value; }
			}

			[DataElement(AllowDbNull=true)]
			public string Time
			{
				get { return m_dateTime; }
				set { m_dateTime = value; }
			}

			[DataElement(AllowDbNull=true)]
			public string ServerName
			{
				get { return m_srvrName; }
				set { m_srvrName = value; }
			}

			[DataElement(AllowDbNull=true)]
			public string AAC
			{
				get { return m_aac; }
				set { m_aac = value; }
			}

			[DataElement(AllowDbNull=true)]
			public string ServerType
			{
				get { return m_srvrType; }
				set { m_srvrType = value; }
			}

			[DataElement(AllowDbNull=true)]
			public string ServerStatus
			{
				get { return m_srvrStatus; }
				set { m_srvrStatus = value; }
			}

			[DataElement(AllowDbNull=true)]
			public int NumClients
			{
				get { return m_numClients; }
				set { m_numClients = value; }
			}

			[DataElement(AllowDbNull=true)]
			public int NumAccounts
			{
				get { return m_numAccts; }
				set { m_numAccts = value; }
			}

			[DataElement(AllowDbNull=true)]
			public int NumMobs
			{
				get { return m_numMobs; }
				set { m_numMobs = value; }
			}

			[DataElement(AllowDbNull=true)]
			public int NumInventoryItems
			{
				get { return m_numInvItems; }
				set { m_numInvItems = value; }
			}

			[DataElement(AllowDbNull=true)]
			public int NumPlayerChars
			{
				get { return m_numPlrChars; }
				set { m_numPlrChars = value; }
			}

			[DataElement(AllowDbNull=true)]
			public int NumMerchantItems
			{
				get { return m_numMerchantItems; }
				set { m_numMerchantItems = value; }
			}

			[DataElement(AllowDbNull=true)]
			public int NumItemTemplates
			{
				get { return m_numItemTemplates; }
				set { m_numItemTemplates = value; }
			}

			[DataElement(AllowDbNull=true)]
			public int NumWorldObjects
			{
				get { return m_numWorldObj; }
				set { m_numWorldObj = value; }
			}
		}

		[DataTable(TableName="PlayerInfo")]
		public class PlayerInfo : DataObject
		{
			private string m_name = "NA";
			private string m_lastName = "NA";
			private string m_guild = "NA";
			private string m_race = "NA";
			private string m_class = "NA";
			private string m_alive = "NA";
			private string m_realm = "NA";
			private string m_region = "NA";
			private int m_lvl = 0;
			private int m_x = 0;
			private int m_y = 0;

			private static bool m_autoSave = true;

			public override bool AutoSave
			{
				get { return m_autoSave; }
				set { m_autoSave = value; }
			}

			[DataElement(AllowDbNull=true)]
			public string Name
			{
				get { return m_name; }
				set { m_name = value; }
			}

			[DataElement(AllowDbNull=true)]
			public string LastName
			{
				get { return m_lastName; }
				set { m_lastName = value; }
			}

			[DataElement(AllowDbNull=true)]
			public string Guild
			{
				get { return m_guild; }
				set { m_guild = value; }
			}

			[DataElement(AllowDbNull=true)]
			public string Race
			{
				get { return m_race; }
				set { m_race = value; }
			}

			[DataElement(AllowDbNull=true)]
			public string Class
			{
				get { return m_class; }
				set { m_class = value; }
			}

			[DataElement(AllowDbNull=true)]
			public string Alive
			{
				get { return m_alive; }
				set { m_alive = value; }
			}

			[DataElement(AllowDbNull=true)]
			public string Realm
			{
				get { return m_realm; }
				set { m_realm = value; }
			}

			[DataElement(AllowDbNull=true)]
			public string Region
			{
				get { return m_region; }
				set { m_region = value; }
			}

			[DataElement(AllowDbNull=true)]
			public int Level
			{
				get { return m_lvl; }
				set { m_lvl = value; }
			}

			[DataElement(AllowDbNull=true)]
			public int X
			{
				get { return m_x; }
				set { m_x = value; }
			}

			[DataElement(AllowDbNull=true)]
			public int Y
			{
				get { return m_y; }
				set { m_y = value; }
			}
		}

		/// <summary>
		/// Reads in the template and generates the appropriate html
		/// </summary>
		public static void Generate()
		{
			try
			{
				DataConnection con = new DataConnection(ConnectionType.DATABASE_XML, "."+Path.DirectorySeparatorChar+"webui"+Path.DirectorySeparatorChar+"generated");
				ObjectDatabase db = new ObjectDatabase(con);

				db.RegisterDataObject(typeof (ServerInfo));
				db.RegisterDataObject(typeof (PlayerInfo));

				ServerInfo si = new ServerInfo();

				si.Time = DateTime.Now.ToString();
				si.ServerName = GameServer.Instance.Configuration.ServerName;
				si.NumClients = GameServer.Instance.ClientCount;
				si.NumAccounts = GameServer.Database.GetObjectCount(typeof (DOL.Database.Account));
				si.NumMobs = GameServer.Database.GetObjectCount(typeof (DOL.Database.Mob));
				si.NumInventoryItems = GameServer.Database.GetObjectCount(typeof (DOL.Database.InventoryItem));
				si.NumPlayerChars = GameServer.Database.GetObjectCount(typeof (DOL.Database.Character));
				si.NumMerchantItems = GameServer.Database.GetObjectCount(typeof (DOL.Database.MerchantItem));
				si.NumItemTemplates = GameServer.Database.GetObjectCount(typeof (DOL.Database.ItemTemplate));
				si.NumWorldObjects = GameServer.Database.GetObjectCount(typeof (DOL.Database.WorldObject));
				si.ServerType = GameServer.Instance.Configuration.ServerType.ToString();
				si.ServerStatus = GameServer.Instance.ServerStatus.ToString();
				si.AAC = GameServer.Instance.Configuration.AutoAccountCreation ? "enabled" : "disabled";

				db.AddNewObject(si);

				PlayerInfo pi = new PlayerInfo();

				foreach (GameClient client in WorldMgr.GetAllPlayingClients())
				{
					GamePlayer plr = client.Player;

					pi.Name = plr.Name;
					pi.LastName = plr.LastName;
					pi.Class = plr.CharacterClass.Name;
					pi.Race = plr.RaceName;
					pi.Guild = plr.GuildName;
					pi.Level = plr.Level;
					pi.Alive = plr.Alive ? "yes" : "no";
					pi.Realm = ((eRealm) plr.Realm).ToString();
					pi.Region = plr.CurrentRegion.Name;
					pi.X = plr.X;
					pi.Y = plr.Y;
				}

				db.WriteDatabaseTables();
				con = null;
				db = null;

				if (log.IsInfoEnabled)
					log.Info("WebUI Generation initialized");
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("WebUI Generation: ", e);
			}
		}

		/// <summary>
		/// Starts the timer to generate the web ui
		/// </summary>
		public static void Start()
		{
			if (m_timer != null)
			{
				Stop();
			}

			m_timer = new System.Timers.Timer(60000.0); //1 minute
			m_timer.Elapsed += new System.Timers.ElapsedEventHandler(m_timer_Elapsed);
			m_timer.AutoReset = true;
			m_timer.Start();

			if (log.IsInfoEnabled)
				log.Info("Web UI generation started...");
		}

		/// <summary>
		/// Stops the timer that generates the web ui
		/// </summary>
		public static void Stop()
		{
			if (m_timer != null)
			{
				m_timer.Stop();
				m_timer.Close();
				m_timer.Elapsed -= new System.Timers.ElapsedEventHandler(m_timer_Elapsed);
				m_timer = null;
			}

			Generate();

			if (log.IsInfoEnabled)
				log.Info("Web UI generation stopped...");
		}

		/// <summary>
		/// The timer proc that generates the web ui every X milliseconds
		/// </summary>
		/// <param name="sender">Caller of this function</param>
		/// <param name="e">Info about the timer</param>
		private static void m_timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			Generate();
		}
	}
}