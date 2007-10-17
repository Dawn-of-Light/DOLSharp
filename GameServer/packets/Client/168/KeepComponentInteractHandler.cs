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

using DOL.GS.Keeps;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandler(PacketHandlerType.TCP, 0xC7 ^ 168, "Keep component interact")]
	public class KeepComponentInteractHandler : IPacketHandler
	{
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			ushort keepId = packet.ReadShort();
			ushort wallId = packet.ReadShort();
			ushort responce = packet.ReadShort();
			int HPindex = packet.ReadShort();

			AbstractGameKeep keep = KeepMgr.getKeepByID(keepId);

			if (keep == null || !(GameServer.ServerRules.IsSameRealm(client.Player, (GameKeepComponent)keep.KeepComponents[wallId], true) || client.Account.PrivLevel > 1))
				return 0;

			if (responce == 0x00)//show info
				client.Out.SendKeepComponentInteract(((GameKeepComponent)keep.KeepComponents[wallId]));
			else if (responce == 0x01)// click on hookpoint button
				client.Out.SendKeepComponentHookPoint(((GameKeepComponent)keep.KeepComponents[wallId]), HPindex);
			else if (responce == 0x02)//select an hookpoint
			{
				if (client.Account.PrivLevel > 1)
					client.Out.SendMessage("DEBUG : selected hookpoint id " + HPindex, eChatType.CT_Say, eChatLoc.CL_SystemWindow);

				GameKeepComponent hp = keep.KeepComponents[wallId] as GameKeepComponent;
				client.Out.SendClearKeepComponentHookPoint(hp, HPindex);
				client.Out.SendHookPointStore(hp.HookPoints[HPindex] as GameKeepHookPoint);
			}
			return 1;
		}
	}
}
