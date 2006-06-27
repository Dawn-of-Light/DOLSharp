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

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&emote", new string[] {"&em", "&e"},
		(uint) ePrivLevel.Player,
		"Roleplay an action or emotion", "/emote <text>")]
	public class CustomEmoteCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			// no emotes if dead
			if (!client.Player.Alive)
			{
				client.Out.SendMessage("You can't emote while dead!", eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
				return 1;
			}

			if (args.Length < 2)
			{
				client.Out.SendMessage("You need something to emote.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}

			string ownRealm = string.Join(" ", args, 1, args.Length - 1);
			ownRealm = "<" + client.Player.Name + " " + ownRealm + " >";

			string diffRealm = "<" + client.Player.Name + " makes strange motions.>";

			foreach (GamePlayer player in client.Player.GetPlayersInRadius(WorldMgr.SAY_DISTANCE))
				if (GameServer.ServerRules.IsAllowedToUnderstand(client.Player, player))
				{
					player.Out.SendMessage(ownRealm, eChatType.CT_Emote, eChatLoc.CL_ChatWindow);
				}
				else
				{
					player.Out.SendMessage(diffRealm, eChatType.CT_Emote, eChatLoc.CL_ChatWindow);
				}

			return 1;
		}
	}
}