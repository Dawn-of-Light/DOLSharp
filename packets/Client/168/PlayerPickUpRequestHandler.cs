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
using DOL.GS;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP,0x1D^168,"Handles Pick up object request")]
	public class PlayerPickUpRequestHandler : IPacketHandler
	{
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			if (client.Player == null)
				return 1;
			uint X = packet.ReadInt();
			uint Y = packet.ReadInt();
			ushort id =(ushort) packet.ReadShort();
			ushort obj=(ushort) packet.ReadShort();

			GameObject target = client.Player.TargetObject;
			if (target == null)
			{
				client.Out.SendMessage("You must have a target to get something!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}
			if (target.ObjectState != GameObject.eObjectState.Active)
			{
				client.Out.SendMessage("You have an invalid target!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}
			
			client.Player.PickupObject(target);
			return 1;
		}
	}
}
