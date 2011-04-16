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
	[CmdAttribute(
		"&language",
		ePrivLevel.Player,
		"Change your server-language display. Custom 'CU' is what your administrator decides.",
		"/language <EN|IT|FR|DE|CU>")]
	public class LanguageCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public void OnCommand(GameClient client, string[] args)
		{
			if (IsSpammingCommand(client.Player, "language"))
				return;

			if (client.Account.PrivLevel == (uint) ePrivLevel.Player &&
			    DOL.GS.ServerProperties.Properties.ALLOW_CHANGE_LANGUAGE == false)
			{
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Language.Current", LanguageMgr.LangsToCompleteName(client, LanguageMgr.NameToLangs(client.Account.Language))), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				DisplayMessage(client, "This server does not support changing languages");
				return;
			}


			if (args.Length == 1)
			{
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Language.Current", LanguageMgr.LangsToCompleteName(client, LanguageMgr.NameToLangs(client.Account.Language))), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				DisplaySyntax(client);
			}
			else
			{
				if (client.Account.PrivLevel != (uint)ePrivLevel.Player)
				{
					switch (args[1].ToLower())
					{
						case "debug": //receive extended messages
							{
								bool debug = client.Player.TempProperties.getProperty("LANGUAGEMGR-DEBUG", false);
								debug = !debug;
								client.Player.TempProperties.setProperty("LANGUAGEMGR-DEBUG", debug);
								client.Out.SendMessage("[LanguageMgr] Debug mode : " + (debug ? "ON" : "OFF"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return;
							}
						case "load": //refresh from database
							{
								if (args.Length != 3)
								{
									DisplayMessage(client, "[LanguageMgr] Usage : '/lang load GamePlayer.AddAbility.YouLearn'");
									return;
								}
								if (!LanguageMgr.IDSentences.ContainsKey(args[2]))
								{
									DisplayMessage(client, "[LanguageMgr] Can't find TranslationID <" + args[2] + "> !");
									return;
								}
								if (LanguageMgr.Refresh(args[2]))
								{
									DisplayMessage(client, "[LanguageMgr] TranslationID <" + args[2] + "> updated successfully !");
									return;
								}
								DisplayMessage(client, "[LanguageMgr] An error occured.");
								return;
							}
					}
				}

				// Valid language -> English default
				client.Account.Language = LanguageMgr.LangsToName(LanguageMgr.NameToLangs(args[1].ToUpper()));
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Language.Set", LanguageMgr.LangsToCompleteName(client, LanguageMgr.NameToLangs(client.Account.Language))), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				GameServer.Database.SaveObject(client.Account);

				if (log.IsInfoEnabled)
				{
					log.Info(client.Player.Name + " (" + client.Account.Name + ") changed language.");
				}
			}
		}
	}
}