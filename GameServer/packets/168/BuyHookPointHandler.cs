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

namespace DOL.GS.PacketHandler.v168
{
	[PacketHandler(PacketHandlerType.TCP,0xCC^168,"buy hookpoint siege weapon/mob")]
	public class BuyHookPointHandler : IPacketHandler
	{
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			ushort keepId = packet.ReadShort();
			ushort wallId = packet.ReadShort();
			int hookpointID = packet.ReadShort();
            ushort itemslot = packet.ReadShort();
			int payType = packet.ReadByte();//gold RP BP contrat???
			int unk2 = packet.ReadByte();
			int unk3 = packet.ReadByte();
			int unk4 = packet.ReadByte();
//			client.Player.Out.SendMessage("x="+unk2+"y="+unk3+"z="+unk4,eChatType.CT_Say,eChatLoc.CL_SystemWindow);
			AbstractGameKeep keep = KeepMgr.getKeepByID(keepId);
			if (keep == null) return 1;
			GameKeepComponent component = keep.KeepComponents[wallId] as GameKeepComponent;
			if (component == null) return 1;
			/*GameKeepHookPoint hookpoint = component.HookPoints[hookpointID] as GameKeepHookPoint;
			if (hookpoint == null) return 1;
			*/
			HookPointInventory inventory = null;
			if(hookpointID > 0x80) inventory = HookPointInventory.YellowHPInventory; //oil
			else if(hookpointID > 0x60) inventory = HookPointInventory.GreenHPInventory;//big siege
			else if(hookpointID > 0x40) inventory = HookPointInventory.LightGreenHPInventory; //small siege
			else if (hookpointID > 0x20) inventory = HookPointInventory.BlueHPInventory;//npc
			else inventory = HookPointInventory.RedHPInventory;//guard

			if (inventory != null)
			{
				HookPointItem item = inventory.GetItem(itemslot);
				if (item != null)
					item.Invoke(client.Player, payType, component.HookPoints[hookpointID] as GameKeepHookPoint, component);
			}

			return 1;
		}
	}
}