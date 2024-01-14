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
using DOL.Language;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&yell",
		new string[] { "&y" },
		ePrivLevel.Player,
		"Yell something to other players around you",
		"/yell <message>")]
	public class YellCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if( IsSpammingCommand( client.Player, "yell", 750 ) )
			{
                DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "GamePlayer.Spamming.Say"));
                return;
			}

            if (client.Player.IsMuted)
            {
                client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Yell.Muted"), eChatType.CT_Staff, eChatLoc.CL_SystemWindow);
                return;
            }

			if (args.Length < 2)
			{
				foreach (GamePlayer player in client.Player.GetPlayersInRadius(WorldMgr.YELL_DISTANCE))
				{
					if (player != client.Player)
					{
                        var directionToTarget = player.Coordinate.GetOrientationTo(client.Player.Coordinate);
                        var cardinalDirection = LanguageMgr.GetCardinalDirection(player.Client.Account.Language, directionToTarget);
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Scripts.Players.Yell.FromDirection", client.Player.Name, cardinalDirection), eChatType.CT_Help, eChatLoc.CL_SystemWindow);
                    }
					else
                        client.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Scripts.Players.Yell.YouYell"), eChatType.CT_Help, eChatLoc.CL_SystemWindow);
                }
				return;
			}

			string message = string.Join(" ", args, 1, args.Length - 1);
			client.Player.Yell(message);
		}
	}
}