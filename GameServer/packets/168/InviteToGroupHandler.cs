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

namespace DOL.GS.PacketHandler.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP, 0x2F^168, "handle invite to group")]
	public class InviteToGroupHandler : IPacketHandler
	{
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			new HandleGroupInviteAction(client.Player).Start(1);

			return 1;
		}

		/// <summary>
		/// Handles group invlite actions
		/// </summary>
		protected class HandleGroupInviteAction : RegionAction
		{
			/// <summary>
			/// constructs a new HandleGroupInviteAction
			/// </summary>
			/// <param name="actionSource">The action source</param>
			public HandleGroupInviteAction(GamePlayer actionSource) : base(actionSource)
			{
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				GamePlayer player = (GamePlayer)m_actionSource;

				if(player.TargetObject == null || player.TargetObject == player)
				{
					player.Out.SendMessage("You have not selected a valid player as your target.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					return;
				}

				if(!(player.TargetObject is GamePlayer))
				{
					player.Out.SendMessage("You have not selected a valid player as your target.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					return;
				}

				GamePlayer target = (GamePlayer)player.TargetObject;

				if(player.PlayerGroup != null && player.PlayerGroup.Leader != player)
				{
					player.Out.SendMessage("You are not the leader of your group.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
				if(player.PlayerGroup != null && player.PlayerGroup.PlayerCount >= PlayerGroup.MAX_GROUP_SIZE)
				{
					player.Out.SendMessage("The group is full.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					return;
				}

				if(!GameServer.ServerRules.IsAllowedToGroup(player, target, false))
				{
					return;
				}

				if(target.PlayerGroup != null)
				{
					player.Out.SendMessage("The player is still in a group.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					return;
				}

				player.Out.SendMessage("You have invited " + target.Name + " to join your group.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				target.Out.SendDialogBox(eDialogCode.GroupInvite, (ushort)player.Client.SessionID, 0x00, 0x00, 0x00, eDialogType.YesNo, false, player.Name + " has invited you to join\n" + player.GetPronoun(1, false) + " group. Do you wish to join?");
				target.Out.SendMessage(player.Name + " has invited you to join "+ player.GetPronoun(1, false) +" group.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}
	}
}
