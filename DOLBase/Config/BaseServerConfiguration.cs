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

namespace DOL.Config
{
	/// <summary>
	/// This is a server configuration
	/// </summary>
	public class BaseServerConfiguration
	{
		/// <summary>
		/// Auto Detect the RegionIP
		/// </summary>
		private bool _detectRegionIP;

		/// <summary>
		/// Enable uPnP features
		/// </summary>
		private bool _enableUPnP;

		/// <summary>
		/// The ip address the server should use for listening
		/// </summary>
		private IPAddress _ip;

		/// <summary>
		/// The port the server should listen to
		/// </summary>
		private ushort _port;

		/// <summary>
		/// The region IP
		/// </summary>
		private IPAddress _regionIP;

		/// <summary>
		/// The region port
		/// </summary>
		private ushort _regionPort;

		/// <summary>
		/// The UDP IP
		/// </summary>
		private IPAddress _udpIP;

		/// <summary>
		/// The UDP port
		/// </summary>
		private ushort _udpPort;

		/// <summary>
		/// Constructs a server configuration with default values
		/// </summary>
		protected BaseServerConfiguration()
		{
			_port = 10300;
			_ip = IPAddress.Any;
			_regionIP = IPAddress.Any;
			_regionPort = 10400;
			_udpIP = IPAddress.Any;
			_udpPort = 10400;
			_detectRegionIP = true;
			_enableUPnP = true;
		}

		/// <summary>
		/// Sets or gets the port for the server
		/// </summary>
		public ushort Port
		{
			get { return _port; }
			set { _port = value; }
		}

		/// <summary>
		/// Sets or gets the IP address for the server
		/// </summary>
		public IPAddress Ip
		{
			get { return _ip; }
			set { _ip = value; }
		}

		/// <summary>
		/// Gets or sets the region ip
		/// </summary>
		public IPAddress RegionIp
		{
			get { return _regionIP; }
			set { _regionIP = value; }
		}

		/// <summary>
		/// Gets or sets the region port
		/// </summary>
		public ushort RegionPort
		{
			get { return _regionPort; }
			set { _regionPort = value; }
		}

		/// <summary>
		/// Gets or sets the UDP ip
		/// </summary>
		public IPAddress UDPIp
		{
			get { return _udpIP; }
			set { _udpIP = value; }
		}

		/// <summary>
		/// Gets or sets the UDP port
		/// </summary>
		public ushort UDPPort
		{
			get { return _udpPort; }
			set { _udpPort = value; }
		}

		public bool EnableUpnp
		{
			get { return _enableUPnP; }
			set { _enableUPnP = value; }
		}

		/// <summary>
		/// Detects the RegionIP for servers that are behind a supported IGD
		/// </summary>
		public bool DetectRegionIP
		{
			get { return _detectRegionIP; }
			set { _detectRegionIP = value; }
		}

		/// <summary>
		/// Loads the config values from a specific config element
		/// </summary>
		/// <param name="root">the root config element</param>
		protected virtual void LoadFromConfig(ConfigElement root)
		{
			string ip = root["Server"]["IP"].GetString("any");
			_ip = ip == "any" ? IPAddress.Any : IPAddress.Parse(ip);
			_port = (ushort) root["Server"]["Port"].GetInt(_port);

			ip = root["Server"]["RegionIP"].GetString("any");
			_regionIP = ip == "any" ? IPAddress.Any : IPAddress.Parse(ip);
			_regionPort = (ushort) root["Server"]["RegionPort"].GetInt(_regionPort);

			ip = root["Server"]["UdpIP"].GetString("any");

			_udpIP = ip == "any" ? IPAddress.Any : IPAddress.Parse(ip);
			_udpPort = (ushort) root["Server"]["UdpPort"].GetInt(_udpPort);

			_enableUPnP = root["Server"]["EnableUPnP"].GetBoolean(_enableUPnP);
			_detectRegionIP = root["Server"]["DetectRegionIP"].GetBoolean(_detectRegionIP);
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
			root["Server"]["Port"].Set(_port);
			root["Server"]["IP"].Set(_ip);
			root["Server"]["RegionIP"].Set(_regionIP);
			root["Server"]["RegionPort"].Set(_regionPort);
			root["Server"]["UdpIP"].Set(_udpIP);
			root["Server"]["UdpPort"].Set(_udpPort);
			root["Server"]["EnableUPnP"].Set(_enableUPnP);
			root["Server"]["DetectRegionIP"].Set(_detectRegionIP);
		}

		/// <summary>
		/// Save the configuration to a XML file
		/// </summary>
		/// <param name="configFile">The file to save</param>
		public void SaveToXMLFile(FileInfo configFile)
		{
			if (configFile == null)
				throw new ArgumentNullException("configFile");

			var config = new XMLConfigFile();
			SaveToConfig(config);

			config.Save(configFile);
		}
	}
}
