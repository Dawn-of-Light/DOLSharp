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

using DOL.GS.Keeps;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandler(PacketHandlerType.TCP, 0xE4, "ship hookpoint interact")]
	public class ShipHookpointInteractHandler : IPacketHandler
	{
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			ushort unk1 = packet.ReadShort();
			ushort objectOid = packet.ReadShort();
			ushort unk2 = packet.ReadShort();
			int slot = packet.ReadByte();
			int flag = packet.ReadByte();
			int currency = packet.ReadByte();
			int unk3 = packet.ReadByte();
			ushort unk4 = packet.ReadShort();
			int type = packet.ReadByte();
			int unk5 = packet.ReadByte();
			int unk6 = packet.ReadShort();

			if (client.Player.Steed == null || client.Player.Steed is GameBoat == false)
				return 0;

			switch (flag)
			{
				case 0:
					{
						//siegeweapon
						break;
					}
				case 3:
					{
						//move
						GameBoat boat = client.Player.Steed as GameBoat;
						if (boat.Riders[slot] == null)
						{
							client.Player.SwitchSeat(slot);
						}
						else
						{
							client.Player.Out.SendMessage("That seat isn't empty!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						break;
					}
				default:
					{
						KeepMgr.Logger.Error(string.Format("Unhandled ShipHookpointInteract client to server packet unk1 {0} objectOid {1} unk2 {2} slot {3} flag {4} currency {5} unk3 {6} unk4 {7} type {8} unk5 {9} unk6 {10}", unk1, objectOid, unk2, slot, flag, currency, unk3, unk4, type, unk5, unk6));
						break;
					}
			}
			return 1;
		}
	}
}