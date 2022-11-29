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
						ushort headingtotarget = player.GetHeading(client.Player);
						if( headingtotarget < 0 )
							headingtotarget += 4096;
                        
                        string direction = "";
                        if( headingtotarget >= 3840 || headingtotarget <= 256  ) direction = LanguageMgr.GetTranslation(player.Client.Account.Language, "Scripts.Players.Yell.South");
                        else if( headingtotarget > 256   && headingtotarget <= 768  ) direction = LanguageMgr.GetTranslation(player.Client.Account.Language, "Scripts.Players.Yell.SouthWest");
                        else if( headingtotarget > 768   && headingtotarget <= 1280 ) direction = LanguageMgr.GetTranslation(player.Client.Account.Language, "Scripts.Players.Yell.West");
                        else if( headingtotarget > 1280  && headingtotarget <= 1792 ) direction = LanguageMgr.GetTranslation(player.Client.Account.Language, "Scripts.Players.Yell.NorthWest");
                        else if( headingtotarget > 1792  && headingtotarget <= 2304 ) direction = LanguageMgr.GetTranslation(player.Client.Account.Language, "Scripts.Players.Yell.North");
                        else if( headingtotarget > 2304  && headingtotarget <= 2816 ) direction = LanguageMgr.GetTranslation(player.Client.Account.Language, "Scripts.Players.Yell.NorthEast");
                        else if( headingtotarget > 2816  && headingtotarget <= 3328 ) direction = LanguageMgr.GetTranslation(player.Client.Account.Language, "Scripts.Players.Yell.East");
                        else if( headingtotarget > 3328  && headingtotarget <= 3840 ) direction = LanguageMgr.GetTranslation(player.Client.Account.Language, "Scripts.Players.Yell.SouthEast");

                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Scripts.Players.Yell.FromDirection", client.Player.Name, direction), eChatType.CT_Help, eChatLoc.CL_SystemWindow);
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