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

using System.Net;

namespace DOL.NatTraversal
{
	public class PortMappingInfo
	{

		#region Fields

		private bool enabled;
		private string description;
		private string internalHostName;
		private int internalPort;
		private IPAddress externalIPAddress;
		private int externalPort;
		private string protocol;

		#endregion

		#region Instance Management
		/// <summary>
		/// default
		/// </summary>
		/// <param name="description"></param>
		/// <param name="protocol"></param>
		/// <param name="internalHostNameOrIP"></param>
		/// <param name="internalPort"></param>
		/// <param name="externalIPAddress"></param>
		/// <param name="externalPort"></param>
		/// <param name="enabled"></param>
		public PortMappingInfo(string description, string protocol, string internalHostNameOrIP, int internalPort, IPAddress externalIPAddress, int externalPort, bool enabled)
		{

			// Initializes fields
			this.enabled = enabled;
			this.description = description;
			this.internalHostName = internalHostNameOrIP;
			this.internalPort = internalPort;
			this.externalIPAddress = externalIPAddress;
			this.externalPort = externalPort;
			this.protocol = protocol;
		}
		/// <summary>
		/// used for adding port mappings
		/// </summary>
		/// <param name="description"></param>
		/// <param name="protocol"></param>
		/// <param name="internalHostNameOrIP"></param>
		/// <param name="internalPort"></param>
		/// <param name="externalPort"></param>
		/// <param name="enabled"></param>
		public PortMappingInfo(string description, string protocol, string internalHostNameOrIP, int internalPort, int externalPort, bool enabled)
		{

			// Initializes fields
			this.enabled = enabled;
			this.description = description;
			this.internalHostName = internalHostNameOrIP;
			this.internalPort = internalPort;
			this.externalPort = externalPort;
			this.protocol = protocol;
		}
		/// <summary>
		/// used for removing port mappings
		/// </summary>
		/// <param name="protocol"></param>
		/// <param name="externalPort"></param>
		public PortMappingInfo(string protocol, int externalPort)
		{
			this.externalPort = externalPort;
			this.protocol = protocol;
		}


		#endregion

		#region Properties

		public string InternalHostName
		{
			get
			{
				return internalHostName;
			}
		}

		public int InternalPort
		{
			get
			{
				return internalPort;
			}
		}

		public IPAddress ExternalIPAddress
		{
			get
			{
				return externalIPAddress;
			}
		}

		public int ExternalPort
		{
			get
			{
				return externalPort;
			}
		}

		public string Protocol
		{
			get
			{
				return protocol;
			}
		}

		public bool Enabled
		{
			get
			{
				return enabled;
			}
		}

		public string Description
		{
			get
			{
				return description;
			}
		}

		#endregion
	}
}
