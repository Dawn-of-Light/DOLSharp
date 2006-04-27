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
using System.Reflection;
using DOL.Config;

namespace DOL.GS
{
	/// <summary>
	/// This is the game server configuration
	/// </summary>
	public class GameServerConfiguration : BaseServerConfiguration
	{
		#region Server

		/// <summary>
		/// Holds the log configuration file path
		/// </summary>
		protected string m_logConfigFile;
		
		/// <summary>
		/// Holds the database configuration file path
		/// </summary>
		protected string m_databaseConfigFile;

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
		/// Use the reflexion optimiser ?
		/// </summary>
		protected bool m_useReflectionOptimizer;

		#endregion
		#region Network

		/// <summary>
		/// The region IP
		/// </summary>
		protected IPAddress m_regionIP;

		/// <summary>
		/// The region port
		/// </summary>
		protected ushort m_regionPort;

		/// <summary>
		/// The UDP IP
		/// </summary>
		protected IPAddress m_udpIP;

		/// <summary>
		/// The UDP port
		/// </summary>
		protected ushort m_udpPort;

		#endregion
		#region Database

		/// <summary>
		/// The path to the XML database folder
		/// </summary>
		protected bool m_dbAutoCreate;

		/// <summary>
		/// The auto save interval in minutes
		/// </summary>
		protected int m_dbSaveInterval;

		#endregion
		#region Load/Save
		
		/// <summary>
		/// Loads the config values from a specific config element
		/// </summary>
		/// <param name="root">the root config element</param>
		protected override void LoadFromConfig(ConfigElement root)
		{
			base.LoadFromConfig(root);

			m_logConfigFile = root["Server"]["LogConfigFile"].GetString(m_logConfigFile);
			m_databaseConfigFile = root["Server"]["DatabaseConfigFile"].GetString(m_databaseConfigFile);
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

			string ip = root["Server"]["RegionIP"].GetString("any");
			if (ip == "any")
				m_regionIP = IPAddress.Any;
			else
				m_regionIP = IPAddress.Parse(ip);
			m_regionPort = (ushort) root["Server"]["RegionPort"].GetInt(m_regionPort);
			ip = root["Server"]["UdpIP"].GetString("any");
			if (ip == "any")
				m_udpIP = IPAddress.Any;
			else
				m_udpIP = IPAddress.Parse(ip);
			m_udpPort = (ushort) root["Server"]["UdpPort"].GetInt(m_udpPort);

			m_dbAutoCreate = root["Server"]["DBAutoCreate"].GetBoolean(m_dbAutoCreate);
			m_dbSaveInterval = root["Server"]["DBAutosaveInterval"].GetInt(m_dbSaveInterval);
			m_maxClientCount = root["Server"]["MaxClientCount"].GetInt(m_maxClientCount);
			m_useReflectionOptimizer =  root["Server"]["UseReflexionOptimiser"].GetBoolean(m_useReflectionOptimizer);
			m_cpuCount = root["Server"]["CpuCount"].GetInt(m_cpuCount);
			if (m_cpuCount < 1)
				m_cpuCount = 1;
		}

		/// <summary>
		/// Saves the values into a specific config element
		/// </summary>
		/// <param name="root">the root config element</param>
		protected override void SaveToConfig(ConfigElement root)
		{
			base.SaveToConfig(root);

			root["Server"]["LogConfigFile"].Set(m_logConfigFile);
			root["Server"]["DatabaseConfigFile"].Set(m_databaseConfigFile);
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

			root["Server"]["RegionIP"].Set(m_regionIP);
			root["Server"]["RegionPort"].Set(m_regionPort);
			root["Server"]["UdpIP"].Set(m_udpIP);
			root["Server"]["UdpPort"].Set(m_udpPort);

			root["Server"]["DBAutoCreate"].Set(m_dbAutoCreate);
			root["Server"]["DBAutosaveInterval"].Set(m_dbSaveInterval);
			root["Server"]["MaxClientCount"].Set(m_maxClientCount);
			root["Server"]["UseReflexionOptimiser"].Set(m_useReflectionOptimizer);
			root["Server"]["CpuCount"].Set(m_cpuCount);
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
			
			m_logConfigFile = "." + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar + "logconfig.xml";
			m_databaseConfigFile = "." + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar + "databaseconfig.xml";
			m_languageFile = "." + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar + "GameServer.lng";

			m_scriptCompilationTarget = "."+Path.DirectorySeparatorChar+"lib"+Path.DirectorySeparatorChar+"GameServerScripts.dll";
			m_scriptAssemblies = "DOLBase.dll,GameServer.dll,System.dll,log4net.dll,System.Xml.dll,NHibernate.dll,Iesi.Collections.dll,NHibernate.Mapping.Attributes.dll";
			m_autoAccountCreation = true;
			m_serverType = eGameServerType.GST_Normal;

			m_regionIP = IPAddress.Any;
			m_regionPort = 10400;
			m_udpIP = IPAddress.Any;
			m_udpPort = 10400;

			m_dbAutoCreate = false;
			m_dbSaveInterval = 10;
			m_maxClientCount = 500;

			m_useReflectionOptimizer = true;
			
			try
			{
				m_cpuCount = int.Parse(Environment.GetEnvironmentVariable("NUMBER_OF_PROCESSORS"));
			}
			catch
			{
			    m_cpuCount = -1;
			}
			if (m_cpuCount < 1)
				m_cpuCount = 1;
		}

		#endregion

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
                    return Path.Combine(Directory.GetCurrentDirectory(), m_logConfigFile);
			}
			set { m_logConfigFile = value; }
		}

		/// <summary>
		/// Gets or sets the database configuration file of this server
		/// </summary>
		public string DatabaseConfigFile
		{
			get
			{
				if(Path.IsPathRooted(m_databaseConfigFile))
					return m_databaseConfigFile;
				else
                    return Path.Combine(Directory.GetCurrentDirectory(), m_databaseConfigFile);
			}
			set { m_databaseConfigFile = value; }
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
                    return Path.Combine(Directory.GetCurrentDirectory(), m_languageFile);
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
		/// Gets or sets the region ip
		/// </summary>
		public IPAddress RegionIp
		{
			get { return m_regionIP; }
			set { m_regionIP = value; }
		}

		/// <summary>
		/// Gets or sets the region port
		/// </summary>
		public ushort RegionPort
		{
			get { return m_regionPort; }
			set { m_regionPort = value; }
		}
    
		/// <summary>
		/// Gets or sets the UDP ip
		/// </summary>
		public IPAddress UDPIp
		{
			get { return m_udpIP; }
			set { m_udpIP = value; }
		}

		/// <summary>
		/// Gets or sets the UDP port
		/// </summary>
		public ushort UDPPort
		{
			get { return m_udpPort; }
			set { m_udpPort = value; }
		}

		/// <summary>
		/// Gets or sets the autocreate flag
		/// </summary>
		public bool DBAutoCreate
		{
			get { return m_dbAutoCreate; }
			set { m_dbAutoCreate = value; }
		}

		/// <summary>
		/// Gets or sets the autosave interval
		/// </summary>
		public int DBSaveInterval
		{
			get { return m_dbSaveInterval; }
			set { m_dbSaveInterval = value; }
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
		/// Gets or sets if we use the reflection optimiser
		/// </summary>
		public bool UseReflectionOptimizer
		{
			get { return m_useReflectionOptimizer; }
			set { m_useReflectionOptimizer = value; }
		}
	}
}