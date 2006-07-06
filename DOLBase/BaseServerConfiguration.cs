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
using DOL.Config;

namespace DOL
{
	/// <summary>
	/// This is a server configuration
	/// </summary>
	public class BaseServerConfiguration
	{
		/// <summary>
		/// The port the server should listen to
		/// </summary>
		protected ushort m_port;
		/// <summary>
		/// The ip address the server should use for listening
		/// </summary>
		protected IPAddress m_ip;

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

		/// <summary>
		/// Enable uPnP features
		/// </summary>
		protected bool m_enableUPnP;

		/// <summary>
		/// Auto Detect the RegionIP
		/// </summary>
		protected bool m_detectRegionIP;

		/// <summary>
		/// Loads the config values from a specific config element
		/// </summary>
		/// <param name="root">the root config element</param>
		protected virtual void LoadFromConfig(ConfigElement root)
		{
			string ip = root["Server"]["IP"].GetString("any");
			if(ip == "any")
				m_ip = IPAddress.Any;
			else
				m_ip = IPAddress.Parse(ip);
			m_port = (ushort)root["Server"]["Port"].GetInt(m_port);
			
			ip = root["Server"]["RegionIP"].GetString("any");
			if(ip == "any")
				m_regionIP = IPAddress.Any;
			else
				m_regionIP = IPAddress.Parse(ip);
			m_regionPort = (ushort)root["Server"]["RegionPort"].GetInt(m_regionPort);

			ip = root["Server"]["UdpIP"].GetString("any");
			if(ip == "any")
				m_udpIP = IPAddress.Any;
			else
				m_udpIP = IPAddress.Parse(ip);
			m_udpPort = (ushort)root["Server"]["UdpPort"].GetInt(m_udpPort);

			m_enableUPnP = (bool)root["Server"]["EnableUPnP"].GetBoolean(m_enableUPnP);
			m_detectRegionIP = (bool)root["Server"]["DetectRegionIP"].GetBoolean(m_detectRegionIP);
		}

		/// <summary>
		/// Load the configuration from a XML source file
		/// </summary>
		/// <param name="configFile">The file to load from</param>
		public void LoadFromXMLFile(FileInfo configFile)
		{
			XMLConfigFile xmlConfig = XMLConfigFile.ParseXMLFile(configFile);
			LoadFromConfig(xmlConfig);
		}

		/// <summary>
		/// Saves the values into a specific config element
		/// </summary>
		/// <param name="root">the root config element</param>
		protected virtual void SaveToConfig(ConfigElement root)
		{
			root["Server"]["Port"].Set(m_port);
			root["Server"]["IP"].Set(m_ip);
			root["Server"]["RegionIP"].Set(m_regionIP);
			root["Server"]["RegionPort"].Set(m_regionPort);
			root["Server"]["UdpIP"].Set(m_udpIP);
			root["Server"]["UdpPort"].Set(m_udpPort);
			root["Server"]["EnableUPnP"].Set(m_enableUPnP);
			root["Server"]["DetectRegionIP"].Set(m_detectRegionIP);
		}

		/// <summary>
		/// Save the configuration to a XML file
		/// </summary>
		/// <param name="configFile">The file to save</param>
		public void SaveToXMLFile(FileInfo configFile)
		{
			if(configFile == null)
				throw new ArgumentNullException("configFile");

			XMLConfigFile config = new XMLConfigFile();
			SaveToConfig(config);
			config.Save(configFile);
		}

		/// <summary>
		/// Constructs a server configuration with default values
		/// </summary>
		public BaseServerConfiguration()
		{
			m_port = 10300;
			m_ip = IPAddress.Any;
			m_regionIP = IPAddress.Any;
			m_regionPort = 10400;
			m_udpIP = IPAddress.Any;
			m_udpPort = 10400;
			m_detectRegionIP = true;
			m_enableUPnP = true;
		}

		/// <summary>
		/// Sets or gets the port for the server
		/// </summary>
		public ushort Port
		{
			get { return m_port; }
			set { m_port = value; }
		}

		/// <summary>
		/// Sets or gets the IP address for the server
		/// </summary>
		public IPAddress Ip
		{
			get { return m_ip; }
			set { m_ip = value; }
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
		public bool EnableUPnP
		{
			get { return m_enableUPnP; }
			set { m_enableUPnP = value; }
		}

		/// <summary>
		/// Detects the RegionIP for servers that are behind a supported IGD
		/// </summary>
		public bool DetectRegionIP
		{
			get { return m_detectRegionIP; }
			set { m_detectRegionIP = value; }
		}
	}
}
