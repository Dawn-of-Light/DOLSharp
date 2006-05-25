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
using DOL.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&disband",
		(uint) ePrivLevel.Player,
		"Disband from a group", "/disband")]
	public class DisbandCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (client.Player.PlayerGroup == null)
			{
				client.Out.SendMessage("You are not in a group", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}

			if(args.Length < 2)//disband myslef
			{
				client.Player.PlayerGroup.RemovePlayer(client.Player);
				return 1;
			}
			else//disband by name
			{
				if(client.Player.PlayerGroup.Leader != client.Player)
				{
					client.Out.SendMessage("You are not the leader of your group.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					return 1;
				}

				GamePlayer target = null;
				foreach(GamePlayer player in client.Player.PlayerGroup.GetPlayersInTheGroup())
				{
					if(player.Name == args[1])
					{
						target = player;

						if(target == client.Player)
						{
							client.Out.SendMessage("Can't disband yourself in such way.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
							return 1;
						}
						break;
					}
				}

				if(target == null)
				{
					client.Out.SendMessage("No players in group with that name.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					return 1;
				}

				client.Player.PlayerGroup.SendMessageToGroupMembers(target.Name+" was disbanded from group.",eChatType.CT_Say,eChatLoc.CL_SystemWindow);
				target.PlayerGroup.RemovePlayer(target);
			}

			return 0;
		}
	}
}