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
using DOL.GS.PacketHandler;
using DOL.Language;


namespace DOL.GS.Commands
{
	[CmdAttribute("&freelevel", //command to handle
		ePrivLevel.Player, //minimum privelege level
		"Display state of FreeLevel", //command description
		"/freelevel")] //command usage
	public class FreelevelCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			//flag 1 = above level, 2 = elligable, 3= time until, 4 = level and time until, 5 = level until
			byte state = client.Player.FreeLevelState;
			string message = "";

			if (args.Length == 2 && args[1] == "decline")
			{
				if (state == 2)
				{
					// NOT SURE FOR THIS MESSAGE
                    message = LanguageMgr.GetTranslation(client, "PLCommands.FreeLevel.Removed");
					// we decline THIS ONE, but next level, we will gain another freelevel !!
					client.Player.DBCharacter.LastFreeLevel = client.Player.Level - 1;
					client.Player.Out.SendPlayerFreeLevelUpdate();
				}
				else
				{
					// NOT SURE FOR THIS MESSAGE
                    message = LanguageMgr.GetTranslation(client, "PLCommands.FreeLevel.NoFreeLevel");
                }
				client.Out.SendMessage(message, eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
            TimeSpan t = (client.Player.DBCharacter.LastFreeLeveled.AddDays(DOL.GS.ServerProperties.Properties.FREELEVEL_DAYS) - DateTime.Now);

			switch (state)
			{
				case 1:
                    message = LanguageMgr.GetTranslation(client, "PLCommands.FreeLevel.AboveMaximumLevel");
                    break;
				case 2:
                    message = LanguageMgr.GetTranslation(client, "PLCommands.FreeLevel.EligibleFreeLevel");
                    break;
				case 3:
                    // NOT SURE FOR THIS MESSAGE
                    message = LanguageMgr.GetTranslation(client, "PLCommands.FreeLevel.FreeLevelIn", t.Days, t.Hours, t.Minutes);
                    break;
				case 4:
					// NOT SURE FOR THIS MESSAGE
                    message = LanguageMgr.GetTranslation(client, "PLCommands.FreeLevel.FreeLevelIn2", t.Days, t.Hours, t.Minutes);
                    break;
				case 5:
                    message = LanguageMgr.GetTranslation(client, "PLCommands.FreeLevel.FreeLevelSoon");
                    break;

			}
			client.Out.SendMessage(message, eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}
	}
}