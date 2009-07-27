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
#define NOENCRYPTION
using System;
using System.Reflection;
using log4net;


namespace DOL.GS.PacketHandler
{
	[PacketLib(199, GameClient.eClientVersion.Version199)]
	public class PacketLib199 : PacketLib198
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Constructs a new PacketLib for Version 1.99 clients
		/// </summary>
		/// <param name="client">the gameclient this lib is associated with</param>
		public PacketLib199(GameClient client)
			: base(client)
		{
			//Dunnerholl 2009-07-28: no changes found in a quick standard analysis
		}
	}
}
