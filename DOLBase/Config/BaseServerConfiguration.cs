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
	/// Base configuration for the server.
	/// </summary>
	public class BaseServerConfiguration
	{
		/// <summary>
		/// Whether or not to try and auto-detect the external IP of the server.
		/// </summary>
		private bool _detectRegionIP;

		/// <summary>
		/// Whether or not to enable UPnP mapping.
		/// </summary>
		private bool _enableUPnP;

		/// <summary>
		/// The listening address of the server.
		/// </summary>
		private IPAddress _ip;

		/// <summary>
		/// The listening port of the server.
		/// </summary>
		private ushort _port;

		/// <summary>
		/// The region (external) address of the server.
		/// </summary>
		private IPAddress _regionIP;

		/// <summary>
		/// The region (external) port of the server.
		/// </summary>
		private ushort _regionPort;

		/// <summary>
		/// The UDP listening address of the server.
		/// </summary>
		private IPAddress _udpIP;

		/// <summary>
		/// The UDP listening port of the server.
		/// </summary>
		private ushort _udpPort;

		/// <summary>
		/// Constructs a server configuration with default values.
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
		/// Gets/sets the listening port for the server.
		/// </summary>
		public ushort Port
		{
			get { return _port; }
			set { _port = value; }
		}

		/// <summary>
		/// Gets/sets the listening address for the server.
		/// </summary>
		public IPAddress IP
		{
			get { return _ip; }
			set { _ip = value; }
		}

		/// <summary>p
		/// Gets/sets the region (external) address for the server.
		/// </summary>
		public IPAddress RegionIP
		{
			get { return _regionIP; }
			set { _regionIP = value; }
		}

		/// <summary>
		/// Gets/sets the region (external) port for the server.
		/// </summary>
		public ushort RegionPort
		{
			get { return _regionPort; }
			set { _regionPort = value; }
		}

		/// <summary>
		/// Gets/sets the UDP listening address for the server.
		/// </summary>
		public IPAddress UDPIP
		{
			get { return _udpIP; }
			set { _udpIP = value; }
		}

		/// <summary>
		/// Gets/sets the UDP listening port for the server.
		/// </summary>
		public ushort UDPPort
		{
			get { return _udpPort; }
			set { _udpPort = value; }
		}

		/// <summary>
		/// Whether or not to enable UPnP mapping.
		/// </summary>
		public bool EnableUPnP
		{
			get { return _enableUPnP; }
			set { _enableUPnP = value; }
		}

		/// <summary>
		/// Whether or not to try and auto-detect the external IP of the server.
		/// </summary>
		public bool DetectRegionIP
		{
			get { return _detectRegionIP; }
			set { _detectRegionIP = value; }
		}

		/// <summary>
		/// Loads the configuration values from the given configuration element.
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
		/// Load the configuration from an XML source file.
		/// </summary>
		/// <param name="configFile">the file to load from</param>
		public void LoadFromXMLFile(FileInfo configFile)
		{
			if (configFile == null)
				throw new ArgumentNullException("configFile");

			XMLConfigFile xmlConfig = XMLConfigFile.ParseXMLFile(configFile);
			LoadFromConfig(xmlConfig);
		}

		/// <summary>
		/// Saves the values to the given configuration element.
		/// </summary>
		/// <param name="root">the configuration element to save to</param>
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
		/// Saves the values to the given XML configuration file.
		/// </summary>
		/// <param name="configFile">the file to save to</param>
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