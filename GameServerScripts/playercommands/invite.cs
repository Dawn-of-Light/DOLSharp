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
// TODO: Restrict inviting to own realm.

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&invite",
		(uint) ePrivLevel.Player,
		"Invite a specified or targeted player to join your group", "/invite <player>")]
	public class InviteCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (client.Player.PlayerGroup != null && client.Player.PlayerGroup.Leader != client.Player)
			{
				client.Out.SendMessage("You are not the leader of your group.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 0;
			}

			string targetName = string.Join(" ", args, 1, args.Length - 1);
			GamePlayer target;

			if (args.Length < 2)
			{ // Inviting by target
				if (client.Player.TargetObject == null || client.Player.TargetObject == client.Player)
				{
					client.Out.SendMessage("You have not selected a valid player as your target.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 1;
				}

				if (!(client.Player.TargetObject is GamePlayer))
				{
					client.Out.SendMessage("You have not selected a valid player as your target.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 1;
				}
				target = (GamePlayer) client.Player.TargetObject;
				if (!GameServer.ServerRules.IsAllowedToGroup(client.Player, target, false))
				{
					return 1;
				}
			}
			else
			{ // Inviting by name
				GameClient targetClient = WorldMgr.GetClientByPlayerNameAndRealm(targetName, 0, true);
				if (targetClient == null)
					target = null;
				else
					target = targetClient.Player;
				if (target == null || !GameServer.ServerRules.IsAllowedToGroup(client.Player, target, true))
				{ // Invalid target or realm restriction
					client.Out.SendMessage("No players online with that name.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 1;
				}
				if (target == client.Player)
				{
					client.Out.SendMessage("You can't invite yourself.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 1;
				}
			}

			if (target.PlayerGroup != null)
			{
				client.Out.SendMessage("The player is still in a group.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}

			client.Out.SendMessage("You have invited " + target.Name + " to join your group.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			target.Out.SendGroupInviteCommand(client.Player, client.Player.Name + " has invited you to join\n" + client.Player.GetPronoun(1, false) + " group. Do you wish to join?");
			target.Out.SendMessage(client.Player.Name + " has invited you to join " + client.Player.GetPronoun(1, false) + " group.", eChatType.CT_System, eChatLoc.CL_SystemWindow);

			return 1;
		}
	}
}