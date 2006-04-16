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
		/// Loads the config values from a specific config element
		/// </summary>
		/// <param name="root">the root config element</param>
		protected virtual void LoadFromConfig(ConfigElement root)
		{
			m_port = (ushort) root["Server"]["Port"].GetInt(10300);
			string ip = root["Server"]["IP"].GetString("any");
			if(ip == "any")
				m_ip = IPAddress.Any;
			else
				m_ip = IPAddress.Parse(ip);
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
	}
}
