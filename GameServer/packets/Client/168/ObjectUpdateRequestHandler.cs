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
using DOL.GS.Housing;
using log4net;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandler(PacketHandlerType.TCP, 0x0D ^ 168, "Update all GameObjects in Playerrange")]
	public class ObjectUpdateRequestHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			foreach (GameStaticItem item in client.Player.GetItemsInRadius(WorldMgr.OBJ_UPDATE_DISTANCE))
			{
				client.Out.SendObjectCreate(item);
			}
			foreach (IDoor door in DoorMgr.getDoorsCloseToSpot(client.Player.CurrentRegionID, client.Player, WorldMgr.OBJ_UPDATE_DISTANCE))
			{
				client.Out.SendObjectCreate(door as GameObject);
				client.Out.SendDoorState(door);
				client.Out.SendObjectUpdate(door as GameObject);
			}

			//housing
			if (client.Player.CurrentRegion.HousingEnabled)
			{
				if (client.Player.HousingUpdateArray == null)
					client.Player.HousingUpdateArray = new BitArray(HouseMgr.MAXHOUSES, false);

				Hashtable houses = (Hashtable)HouseMgr.GetHouses(client.Player.CurrentRegionID);
				if (houses != null)
				{
					foreach (House house in HouseMgr.GetHouses(client.Player.CurrentRegionID).Values)
					{
						if (WorldMgr.GetDistance(client.Player, house.X, house.Y, house.Z) <= HouseMgr.HOUSE_DISTANCE)
						{
							if (!client.Player.HousingUpdateArray[house.UniqueID])
							{
								client.Out.SendHouse(house);
								client.Out.SendGarden(house);
								client.Player.HousingUpdateArray[house.UniqueID] = true;
							}
						}
						else
						{
							client.Player.HousingUpdateArray[house.UniqueID] = false;
						}
					}
				}
			}
			else if (client.Player.HousingUpdateArray != null)
				client.Player.HousingUpdateArray = null;

			return 1;
		}
	}
}
