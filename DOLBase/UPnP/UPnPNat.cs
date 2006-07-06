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
using System.Net;
using DOL.NatTraversal.Interop;

namespace DOL.NatTraversal
{
	public class UPnPNat
	{
		#region Fields
		private UPnPNAT uPnpNat;
		#endregion

		#region Instance Management

		public UPnPNat()
		{
			try
			{
				UPnPNAT nat = new UPnPNAT();
				if(nat.NATEventManager != null && nat.StaticPortMappingCollection != null)
					uPnpNat = nat;
			}
			catch
			{
			}

			// No configurable UPNP NAT is available.
			if(uPnpNat == null)
				throw new NotSupportedException();
		}
		#endregion

		public PortMappingInfo[] PortMappings
		{
			get
			{
				// Builds port mappings list
				ArrayList portMappings = new ArrayList();

				// Enumerates the ports without using the foreach statement (causes the interop to fail).
				int count = uPnpNat.StaticPortMappingCollection.Count;
				IEnumerator enumerator = uPnpNat.StaticPortMappingCollection.GetEnumerator();
				enumerator.Reset();

				for(int i = 0; i <= count; i++)
				{
					IStaticPortMapping mapping = null;
					try
					{
						if(enumerator.MoveNext())
							mapping = (IStaticPortMapping)enumerator.Current;
					}
					catch
					{
					}

					if(mapping != null)
					{
						portMappings.Add(new PortMappingInfo(
						mapping.Description,
						mapping.Protocol.ToUpper(),
						mapping.InternalClient,
						mapping.InternalPort,
						IPAddress.Parse(mapping.ExternalIPAddress),
						mapping.ExternalPort,
						mapping.Enabled));
					}
				}

				// Now copies the ArrayList to an array of PortMappingInfo.
				PortMappingInfo[] portMappingInfos = new PortMappingInfo[portMappings.Count];
				portMappings.CopyTo(portMappingInfos);

				return portMappingInfos;
			}
		}
		public void AddPortMapping(PortMappingInfo portMapping)
		{
			uPnpNat.StaticPortMappingCollection.Add(portMapping.ExternalPort, portMapping.Protocol, portMapping.InternalPort, portMapping.InternalHostName, portMapping.Enabled, portMapping.Description);
		}

		public void RemovePortMapping(PortMappingInfo portMapping)
		{
			uPnpNat.StaticPortMappingCollection.Remove(portMapping.ExternalPort, portMapping.Protocol);
		} 

	}
}