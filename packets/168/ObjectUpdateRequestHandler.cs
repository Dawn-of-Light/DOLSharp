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
using System.Reflection;

using log4net;

namespace DOL.GS.PacketHandler.v168
{
	[PacketHandler(PacketHandlerType.TCP,0x0D^168,"Update all GameObjects in Playerrange")]
	public class ObjectUpdateRequestHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			foreach(GameObject obj in client.Player.GetInRadius(typeof(GameObject), WorldMgr.OBJ_UPDATE_DISTANCE))
			{
				if(obj is GameGravestone || obj is GameObjectTimed || obj is GameStaticItem || obj is GameCraftTool)
				{
					// TODO :Add GameKeepBanner/House
					client.Out.SendItemCreate(obj);
				}
				else if(obj is IDoor)
				{
					client.Out.SendItemCreate(obj);
					client.Out.SendDoorState((IDoor)obj);
				}
			}

			return 1;
		}
	}
}
