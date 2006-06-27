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
using DOL.GS;
using DOL.GS.Database;
using DOL.GS.PacketHandler;
using System.Collections;

namespace DOL.GS.Scripts
{
	[CmdAttribute("&makeleader",
		 (uint)ePrivLevel.Player,
		 "Set a new group leader (can be used by current leader).",
		 "/makeleader <playerName>")]

	public class MakeLeaderCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (client.Player.PlayerGroup == null || client.Player.PlayerGroup.PlayerCount < 2)
			{
				client.Out.SendMessage("You are not part of a group.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				return 0;
			}
			if(client.Player.PlayerGroup.Leader != client.Player)
			{
				client.Out.SendMessage("You are not the leader of your group.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				return 0;
			}

			GamePlayer target;

			if(args.Length<2) // Setting by target
			{
				if(client.Player.TargetObject == null || client.Player.TargetObject == client.Player)
				{
					client.Out.SendMessage("You have not selected a valid player as your target.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					return 1;
				}

				if(!(client.Player.TargetObject is GamePlayer))
				{
					client.Out.SendMessage("You have not selected a valid player as your target.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					return 1;
				}
				target = (GamePlayer)client.Player.TargetObject;
				if(client.Player.PlayerGroup != target.PlayerGroup)
				{
					client.Out.SendMessage("You have not selected a valid player as your target.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					return 1;
				}
			}
			else //Setting by name
			{
				string targetName = args[1];
				GameClient targetClient = WorldMgr.GetClientByPlayerNameAndRealm(targetName, 0);
				if (targetClient == null)
					target = null;
				else target = targetClient.Player;
				if(target==null || client.Player.PlayerGroup != target.PlayerGroup)
				{ // Invalid target
					client.Out.SendMessage("No players in group with that name.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					return 1;
				}
				if(target==client.Player)
				{
					client.Out.SendMessage("You are the group leader already.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					return 1;
				}

			}

            client.Player.PlayerGroup.MakeLeader(target);
			client.Player.PlayerGroup.SendMessageToGroupMembers(target.Name + " is new group leader.", eChatType.CT_System, eChatLoc.CL_SystemWindow);

			return 0;
		}
	}
}