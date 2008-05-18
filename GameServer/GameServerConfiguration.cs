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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using DOL.Config;
using DOL.Database.Connection;

namespace DOL.GS
{
	/// <summary>
	/// This is the game server configuration
	/// </summary>
	public class GameServerConfiguration : BaseServerConfiguration
	{
		#region Server

		/// <summary>
		/// holds the server root directory
		/// </summary>
		protected string m_rootDirectory;

		/// <summary>
		/// Holds the log configuration file path
		/// </summary>
		protected string m_logConfigFile;

		/// <summary>
		/// Holds the log configuration file path
		/// </summary>
		protected string m_regionConfigFile;
		
		/// <summary>
		/// Holds the log configuration file path
		/// </summary>
		protected string m_zoneConfigFile;

		/// <summary>
		/// Name of the scripts compilation target
		/// </summary>
		protected string m_scriptCompilationTarget;

		/// <summary>
		/// The assemblies to include when compiling the scripts
		/// </summary>
		protected string m_scriptAssemblies;

		/// <summary>
		/// True if the server shall automatically create accounts
		/// </summary>
		protected bool m_autoAccountCreation;

		/// <summary>
		/// The game server type
		/// </summary>
		protected eGameServerType m_serverType;

		/// <summary>
		/// The game server name
		/// </summary>
		protected string m_ServerName;

		/// <summary>
		/// The short server name, shown in /loc command
		/// </summary>
		protected string m_ServerNameShort;

		/// <summary>
		/// The location of the language file
		/// </summary>
		protected string m_languageFile;

		/// <summary>
		/// The count of server cpu
		/// </summary>
		protected int m_cpuCount;
		
		/// <summary>
		/// The max client count.
		/// </summary>
		protected int m_maxClientCount;

		/// <summary>
		/// The endpoint to send UDP packets from.
		/// </summary>
		protected IPEndPoint m_udpOutEndpoint;

		#endregion
		#region Logging
		/// <summary>
		/// The logger name where to log the gm+ commandos
		/// </summary>
		protected string m_gmActionsLoggerName;

		/// <summary>
		/// The logger name where to log cheat attempts
		/// </summary>
		protected string m_cheatLoggerName;

		/// <summary>
		/// The file name of the invalid names file
		/// </summary>
		protected string m_invalidNamesFile = "";

		#endregion
		#region Database

		/// <summary>
		/// The path to the XML database folder
		/// </summary>
		protected string m_dbConnectionString;

		/// <summary>
		/// Type database type
		/// </summary>
		protected ConnectionType m_dbType;

		/// <summary>
		/// True if the server shall autosave the db
		/// </summary>
		protected bool m_autoSave;

		/// <summary>
		/// The auto save interval in minutes
		/// </summary>
		protected int m_saveInterval;

		#endregion
		#region Load/Save
		
		/// <summary>
		/// Loads the config values from a specific config element
		/// </summary>
		/// <param name="root">the root config element</param>
		protected override void LoadFromConfig(ConfigElement root)
		{
			base.LoadFromConfig(root);

			// Removed to not confuse users
//			m_rootDirectory = root["Server"]["RootDirectory"].GetString(m_rootDirectory);

			m_logConfigFile = root["Server"]["LogConfigFile"].GetString(m_logConfigFile);
			m_regionConfigFile = root["Server"]["RegionConfigFile"].GetString(m_regionConfigFile);
			m_zoneConfigFile = root["Server"]["ZoneConfigFile"].GetString(m_zoneConfigFile);
			m_languageFile = root["Server"]["LanguageFile"].GetString(m_languageFile);

			m_scriptCompilationTarget = root["Server"]["ScriptCompilationTarget"].GetString(m_scriptCompilationTarget);
			m_scriptAssemblies = root["Server"]["ScriptAssemblies"].GetString(m_scriptAssemblies);
			m_autoAccountCreation = root["Server"]["AutoAccountCreation"].GetBoolean(m_autoAccountCreation);

			string serverType = root["Server"]["GameType"].GetString("Normal");
			switch (serverType.ToLower())
			{
				case "normal":
					m_serverType = eGameServerType.GST_Normal;
					break;
				case "casual":
					m_serverType = eGameServerType.GST_Casual;
					break;
				case "roleplay":
					m_serverType = eGameServerType.GST_Roleplay;
					break;
				case "pve":
					m_serverType = eGameServerType.GST_PvE;
					break;
				case "pvp":
					m_serverType = eGameServerType.GST_PvP;
					break;
				case "test":
					m_serverType = eGameServerType.GST_Test;
					break;
				default:
					m_serverType = eGameServerType.GST_Normal;
					break;
			}

			m_ServerName = root["Server"]["ServerName"].GetString(m_ServerName);
			m_ServerNameShort = root["Server"]["ServerNameShort"].GetString(m_ServerNameShort);

			m_cheatLoggerName = root["Server"]["CheatLoggerName"].GetString(m_cheatLoggerName);
			m_gmActionsLoggerName = root["Server"]["GMActionLoggerName"].GetString(m_gmActionsLoggerName);
			m_invalidNamesFile = root["Server"]["InvalidNamesFile"].GetString(m_invalidNamesFile);

			string db = root["Server"]["DBType"].GetString("XML");
			switch (db.ToLower())
			{
				case "xml":
					m_dbType = ConnectionType.DATABASE_XML;
					break;
				case "mysql":
					m_dbType = ConnectionType.DATABASE_MYSQL;
					break;
				case "mssql":
					m_dbType = ConnectionType.DATABASE_MSSQL;
					break;
				case "odbc":
					m_dbType = ConnectionType.DATABASE_ODBC;
					break;
				case "oledb":
					m_dbType = ConnectionType.DATABASE_OLEDB;
					break;
				default:
					m_dbType = ConnectionType.DATABASE_XML;
					break;
			}
			m_dbConnectionString = root["Server"]["DBConnectionString"].GetString(m_dbConnectionString);
			m_autoSave = root["Server"]["DBAutosave"].GetBoolean(m_autoSave);
			m_saveInterval = root["Server"]["DBAutosaveInterval"].GetInt(m_saveInterval);
			m_maxClientCount = root["Server"]["MaxClientCount"].GetInt(m_maxClientCount);
			m_cpuCount = root["Server"]["CpuCount"].GetInt(m_cpuCount);
			
			if (m_cpuCount < 1)
				m_cpuCount = 1;
				
			if (m_cpuCount > 1)
				m_cpuCount = 1;
				m_cpuCount = 2;
				
			m_cpuUse = root["Server"]["CpuUse"].GetInt(m_cpuUse);
			
			// Parse UDP out endpoint
			IPAddress	address = null;
			int			port = -1;
			string		addressStr = root["Server"]["UDPOutIP"].GetString(string.Empty);
			string		portStr = root["Server"]["UDPOutPort"].GetString(string.Empty);
			if (IPAddress.TryParse(addressStr, out address)
				&& int.TryParse(portStr, out port)
				&& IPEndPoint.MaxPort >= port
				&& IPEndPoint.MinPort <= port)
			{
				m_udpOutEndpoint = new IPEndPoint(address, port);
			}
		}

		/// <summary>
		/// Saves the values into a specific config element
		/// </summary>
		/// <param name="root">the root config element</param>
		protected override void SaveToConfig(ConfigElement root)
		{
			base.SaveToConfig(root);
			root["Server"]["ServerName"].Set(m_ServerName);
			root["Server"]["ServerNameShort"].Set(m_ServerNameShort);
			// Removed to not confuse users
//			root["Server"]["RootDirectory"].Set(m_rootDirectory);
			root["Server"]["LogConfigFile"].Set(m_logConfigFile);
			root["Server"]["RegionConfigFile"].Set(m_regionConfigFile);
			root["Server"]["ZoneConfigFile"].Set(m_zoneConfigFile);
			root["Server"]["LanguageFile"].Set(m_languageFile);

			root["Server"]["ScriptCompilationTarget"].Set(m_scriptCompilationTarget);
			root["Server"]["ScriptAssemblies"].Set(m_scriptAssemblies);
			root["Server"]["AutoAccountCreation"].Set(m_autoAccountCreation);

			string serverType = "Normal";

			switch (m_serverType)
			{
				case eGameServerType.GST_Normal:
					serverType = "Normal";
					break;
				case eGameServerType.GST_Casual:
					serverType = "Casual";
					break;
				case eGameServerType.GST_Roleplay:
					serverType = "Roleplay";
					break;
				case eGameServerType.GST_PvE:
					serverType = "PvE";
					break;
				case eGameServerType.GST_PvP:
					serverType = "PvP";
					break;
				case eGameServerType.GST_Test:
					serverType = "Test";
					break;
				default:
					serverType = "Normal";
					break;
			}
			root["Server"]["GameType"].Set(serverType);

			root["Server"]["CheatLoggerName"].Set(m_cheatLoggerName);
			root["Server"]["GMActionLoggerName"].Set(m_gmActionsLoggerName);
			root["Server"]["InvalidNamesFile"].Set(m_invalidNamesFile);

			string db = "XML";
			
			switch (m_dbType)
			{
			case ConnectionType.DATABASE_XML:
				db = "XML";
					break;
			case ConnectionType.DATABASE_MYSQL:
				db = "MYSQL";
					break;
			case ConnectionType.DATABASE_MSSQL:
				db = "MSSQL";
					break;
			case ConnectionType.DATABASE_ODBC:
				db = "ODBC";
					break;
			case ConnectionType.DATABASE_OLEDB:
				db = "OLEDB";
					break;
				default:
					m_dbType = ConnectionType.DATABASE_XML;
					break;
			}
			root["Server"]["DBType"].Set(db);
			root["Server"]["DBConnectionString"].Set(m_dbConnectionString);
			root["Server"]["DBAutosave"].Set(m_autoSave);
			root["Server"]["DBAutosaveInterval"].Set(m_saveInterval);
			root["Server"]["CpuUse"].Set(m_cpuUse);

			// Store UDP out endpoint
			if (m_udpOutEndpoint != null)
			{
				root["Server"]["UDPOutIP"].Set(m_udpOutEndpoint.Address.ToString());
				root["Server"]["UDPOutPort"].Set(m_udpOutEndpoint.Port.ToString());
			}
		}
		#endregion
		#region Constructors
		/// <summary>
		/// Constructs a server configuration with default values
		/// </summary>
		public GameServerConfiguration() : base()
		{
			m_ServerName = "Dawn Of Light";
			m_ServerNameShort = "DOLSERVER";
			if(Assembly.GetEntryAssembly()!=null)
				m_rootDirectory = new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName;
			else
				m_rootDirectory = new FileInfo(Assembly.GetAssembly(typeof(GameServer)).Location).DirectoryName;

			m_logConfigFile = "." + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar + "logconfig.xml";
			m_regionConfigFile = "." + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar + "regions.xml";
			m_zoneConfigFile = "." + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar + "zones.xml";
			m_languageFile = "." + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar + "GameServer.lng";

			m_scriptCompilationTarget = "."+Path.DirectorySeparatorChar+"lib"+Path.DirectorySeparatorChar+"GameServerScripts.dll";
			m_scriptAssemblies = "DOLBase.dll,GameServer.dll,DOLDatabase.dll,System.dll,log4net.dll,System.Xml.dll";
			m_autoAccountCreation = true;
			m_serverType = eGameServerType.GST_Normal;

			m_cheatLoggerName = "cheats";
			m_gmActionsLoggerName = "gmactions";
			m_invalidNamesFile = "." + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar + "invalidnames.txt";

			m_dbType = ConnectionType.DATABASE_XML;
			m_dbConnectionString = m_rootDirectory+Path.DirectorySeparatorChar+"xml_db";
			m_autoSave = true;
			m_saveInterval = 10;
			m_maxClientCount = 500;

			// Get count of CPUs
			m_cpuCount = Environment.ProcessorCount;
			if (m_cpuCount < 1)
			{
				try
				{
					m_cpuCount = int.Parse(Environment.GetEnvironmentVariable("NUMBER_OF_PROCESSORS"));
				}
				catch { m_cpuCount = -1; }
			}
			if (m_cpuCount < 1)
				m_cpuCount = 1;
				m_cpuUse = 1;
				
			if (m_cpuCount > 1)
				m_cpuCount = 2;
				m_cpuUse = 2;
		}

		#endregion

		/// <summary>
		/// Gets or sets the root directory of the server
		/// </summary>
		public string RootDirectory
		{
			get { return m_rootDirectory; }
			set { m_rootDirectory = value; }
		}

		/// <summary>
		/// Gets or sets the log configuration file of this server
		/// </summary>
		public string LogConfigFile
		{
			get
			{
				if(Path.IsPathRooted(m_logConfigFile))
					return m_logConfigFile;
				else
					return Path.Combine(m_rootDirectory, m_logConfigFile);
			}
			set { m_logConfigFile = value; }
		}

		/// <summary>
		/// Gets or sets the region configuration file of this server
		/// </summary>
		public string RegionConfigFile
		{
			get 
			{ 
				if(Path.IsPathRooted(m_regionConfigFile))
					return m_regionConfigFile;
				else
					return Path.Combine(m_rootDirectory, m_regionConfigFile);
			}
			set { m_regionConfigFile = value; }
		}

		/// <summary>
		/// Gets or sets the zone configuration file of this server
		/// </summary>
		public string ZoneConfigFile
		{
			get
			{
				if(Path.IsPathRooted(m_zoneConfigFile))
					return m_zoneConfigFile;
				else
					return Path.Combine(m_rootDirectory, m_zoneConfigFile);
			}
			set { m_zoneConfigFile = value; }
		}

		/// <summary>
		/// Gets or sets the language file
		/// </summary>
		public string LanguageFile
		{
			get
			{
				if(Path.IsPathRooted(m_languageFile))
					return m_languageFile;
				else
					return Path.Combine(m_rootDirectory, m_languageFile);
			}
			set { m_languageFile = value; }
		}

		/// <summary>
		/// Gets or sets the script compilation target
		/// </summary>
		public string ScriptCompilationTarget
		{
			get { return m_scriptCompilationTarget; }
			set { m_scriptCompilationTarget = value; }
		}

		/// <summary>
		/// Gets or sets the script assemblies to be included in the script compilation
		/// </summary>
		public string ScriptAssemblies
		{
			get { return m_scriptAssemblies; }
			set { m_scriptAssemblies = value; }
		}

		/// <summary>
		/// Gets or sets the auto account creation flag
		/// </summary>
		public bool AutoAccountCreation
		{
			get { return m_autoAccountCreation; }
			set { m_autoAccountCreation = value; }
		}

		/// <summary>
		/// Gets or sets the server type
		/// </summary>
		public eGameServerType ServerType
		{
			get { return m_serverType; }
			set { m_serverType = value; }
		}

		/// <summary>
		/// Gets or sets the server name
		/// </summary>
		public string ServerName
		{
			get { return m_ServerName; }
			set { m_ServerName = value; }
		}

		/// <summary>
		/// Gets or sets the short server name
		/// </summary>
		public string ServerNameShort
		{
			get { return m_ServerNameShort; }
			set { m_ServerNameShort = value; }
		}

		/// <summary>
		/// Gets or sets the GM action logger name
		/// </summary>
		public string GMActionsLoggerName
		{
			get { return m_gmActionsLoggerName; }
			set { m_gmActionsLoggerName = value; }
		}

		/// <summary>
		/// Gets or sets the cheat logger name
		/// </summary>
		public string CheatLoggerName
		{
			get { return m_cheatLoggerName; }
			set { m_cheatLoggerName = value; }
		}

		/// <summary>
		/// Gets or sets the invalid name filename
		/// </summary>
		public string InvalidNamesFile
		{
			get
			{
				if(Path.IsPathRooted(m_invalidNamesFile))
					return m_invalidNamesFile;
				else
					return Path.Combine(m_rootDirectory, m_invalidNamesFile);
			}
			set { m_invalidNamesFile = value; }
		}

		/// <summary>
		/// Gets or sets the xml database path
		/// </summary>
		public string DBConnectionString
		{
			get { return m_dbConnectionString; }
			set { m_dbConnectionString = value; }
		}

		/// <summary>
		/// Gets or sets the DB type
		/// </summary>
		public ConnectionType DBType
		{
			get { return m_dbType; }
			set { m_dbType = value; }
		}

		/// <summary>
		/// Gets or sets the autosave flag
		/// </summary>
		public bool AutoSave
		{
			get { return m_autoSave; }
			set { m_autoSave = value; }
		}

		/// <summary>
		/// Gets or sets the autosave interval
		/// </summary>
		public int SaveInterval
		{
			get { return m_saveInterval; }
			set { m_saveInterval = value; }
		}

		/// <summary>
		/// Gets or sets the server cpu count
		/// </summary>
		public int CpuCount
		{
			get { return m_cpuCount; }
			set { m_cpuCount = value; }
		}
		
		/// <summary>
		/// Gets or sets the max cout of clients allowed
		/// </summary>
		public int MaxClientCount
		{
			get { return m_maxClientCount; }
			set { m_maxClientCount = value; }
		}
		
		/// <summary>
		/// Gets or sets UDP address and port to send UDP packets from.
		/// If <code>null</code> then <see cref="Socket"/> decides where to bind.
		/// </summary>
		public IPEndPoint UDPOutEndpoint
		{
			get { return m_udpOutEndpoint; }
			set { m_udpOutEndpoint = value; }
		}

		private int m_cpuUse;
		public int CPUUse
		{
			get { return m_cpuUse; }
			set { m_cpuUse = value; }
		}
	}
}