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
using System.Collections.Generic;
using DOL.GS.Housing;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP, eClientPackets.HouseMenuRequest, "Handles housing menu requests", eClientStatus.PlayerInGame)]
	public class HousingMenuRequestHandler : IPacketHandler
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static Dictionary<int, eMerchantWindowType> _menu168 = new Dictionary<int, eMerchantWindowType>
		{
			{0, eMerchantWindowType.HousingOutsideShop},
			{1, eMerchantWindowType.HousingInsideShop},
			{2, eMerchantWindowType.HousingOutsideMenu},
			{3, eMerchantWindowType.HousingNPCHookpoint},
			{4, eMerchantWindowType.HousingVaultHookpoint},
			{5, eMerchantWindowType.HousingCraftingHookpoint},
			{6, eMerchantWindowType.HousingBindstoneHookpoint},
			{7, (eMerchantWindowType)0xFF}, // not the best but it's ok
			{8, eMerchantWindowType.HousingInsideMenu}, // Interior menu (flag = 0x00 - roof, 0xFF - floor or wall)
		};
		private static Dictionary<int, eMerchantWindowType> _menu1127 = new Dictionary<int, eMerchantWindowType>
		{
			{0, eMerchantWindowType.HousingOutsideShop},
			{1, eMerchantWindowType.HousingInsideShop},
			{2, eMerchantWindowType.HousingDeedMenu},
			{3, eMerchantWindowType.HousingOutsideMenu},
			{4, eMerchantWindowType.HousingNPCHookpoint},
			{5, eMerchantWindowType.HousingVaultHookpoint},
			{6, eMerchantWindowType.HousingCraftingHookpoint},
			{7, eMerchantWindowType.HousingBindstoneHookpoint},
			{8, eMerchantWindowType.HousingInsideMenu},
			{9, (eMerchantWindowType)0xFF}, // not the best but it's ok
		};

		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			int housenumber = packet.ReadShort();
			int menuid = packet.ReadByte();
			int flag = packet.ReadByte();

			var house = HouseMgr.GetHouse(client.Player.CurrentRegionID, housenumber);
			if (house == null)
				return;

			if (client.Player == null)
				return;

			client.Player.CurrentHouse = house;

			var menu = _menu168;
			if (client.Version >= GameClient.eClientVersion.Version1127)
				menu = _menu1127;

			if (menu.TryGetValue(menuid, out var type))
				OpenWindow(client, house, type);
			else
				client.Out.SendMessage("Invalid menu id " + menuid + " (hookpoint?).", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		private void OpenWindow(GameClient client, House house, eMerchantWindowType type)
		{
			switch (type)
			{
				case eMerchantWindowType.HousingOutsideShop:
				case eMerchantWindowType.HousingOutsideMenu:
					if (!house.CanChangeGarden(client.Player, DecorationPermissions.Add))
						return;
					HouseMgr.SendHousingMerchantWindow(client.Player, type);
					break;

				case eMerchantWindowType.HousingDeedMenu:
				case eMerchantWindowType.HousingVaultHookpoint:
				case eMerchantWindowType.HousingCraftingHookpoint:
				case eMerchantWindowType.HousingBindstoneHookpoint:
				case eMerchantWindowType.HousingNPCHookpoint:
				case eMerchantWindowType.HousingInsideShop:
				case eMerchantWindowType.HousingInsideMenu:
					if (!house.CanChangeInterior(client.Player, DecorationPermissions.Add))
						return;
					HouseMgr.SendHousingMerchantWindow(client.Player, type);
					break;

				case (eMerchantWindowType)0xFF:
					house.SendHouseInfo(client.Player);
					break;
			}
		}
	}
}