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
using DOL.GS.PacketHandler;

namespace DOL.GS.PacketHandler.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP, 0x00, "Handles housing menu requests")]
	public class HousingBuyRequestHandler : IPacketHandler
	{
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			int housenumber = packet.ReadShort();
			int menuid = packet.ReadByte();
			int unkown = packet.ReadByte();

			/*House house = (House)HouseMgr.GetHouse(client.Player.Region, housenumber);
			if(house==null) { return 1; }

			client.Player.CurrentHouse = house;

			switch(menuid)
			{
				case 0:
					//client.Player.Out.SendMerchantWindow(HouseTemplateMgr.HiberniaOutdoorShopItems,eMerchantWindowType.HousingOutsideShop);
					break;

				case 1:
					//client.Player.Out.SendMerchantWindow(HouseTemplateMgr.HiberniaIndoorMenuItems,eMerchantWindowType.HousingInsideMenu);
					break;

				case 2:
					client.Player.Out.SendMerchantWindow(HouseTemplateMgr.OutdoorMenuItems,eMerchantWindowType.HousingOutsideMenu);
					return 1;

				case 7:
					house.SendHouseInfo(client.Player);
					return 1;

				case 8:
					client.Player.Out.SendMerchantWindow(HouseTemplateMgr.IndoorMenuItems,eMerchantWindowType.HousingInsideShop);
					return 1;

				default:
					client.Out.SendMessage("Invalid menu id "+menuid+" (hookpoint?).",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					break;
			}*/
			return 1;
		}
	}
}
