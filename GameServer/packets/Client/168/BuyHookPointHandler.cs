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
	[PacketHandlerAttribute(PacketHandlerType.TCP, eClientPackets.BuyHookPoint, "buy hookpoint siege weapon/mob", eClientStatus.PlayerInGame)]
	public class BuyHookPointHandler : IPacketHandler
	{
		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			ushort keepId = packet.ReadShort();
			ushort wallId = packet.ReadShort();
			int hookpointID = packet.ReadShort();
			ushort itemslot = packet.ReadShort();
			int payType = packet.ReadByte();//gold RP BP contrat???
			packet.ReadByte();
			packet.ReadByte();
			packet.ReadByte();


			AbstractGameKeep keep = GameServer.KeepManager.GetKeepByID(keepId);
			if (keep == null)
				return;
			GameKeepComponent component = keep.KeepComponents[wallId] as GameKeepComponent;
			if (component == null)
				return;

			HookPointInventory inventory = null;
			if (hookpointID > 0x80) inventory = HookPointInventory.YellowHPInventory; // oil
			else if (hookpointID > 0x60) inventory = HookPointInventory.GreenHPInventory; // big siege
			else if (hookpointID > 0x40) inventory = HookPointInventory.LightGreenHPInventory; // small siege
			else if (hookpointID > 0x20) inventory = HookPointInventory.BlueHPInventory; // npc
			else inventory = HookPointInventory.RedHPInventory; // guard

			HookPointItem item = inventory?.GetItem(itemslot);
			item?.Invoke(client.Player, payType, component.KeepHookPoints[hookpointID] as GameKeepHookPoint, component);
		}
	}
}