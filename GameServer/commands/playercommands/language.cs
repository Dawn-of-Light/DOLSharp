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
using log4net;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&language",
		ePrivLevel.Player,
		"Change your server-language display",
		"/language <EN|FR|DE|ES|CZ>")]
	public class LanguageCommandHandler : ICommandHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public int OnCommand(GameClient client, string[] args)
		{
			if (DOL.GS.ServerProperties.Properties.ALLOW_CHANGE_LANGUAGE == false)
				return 0;
			if (args.Length == 1)
			{
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Language.Current", LanguageMgr.LangsToCompleteName(client, LanguageMgr.NameToLangs(client.Account.Language))), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 0;
			}
			else	if (args.Length == 2)
			{
				// Valid language -> English default
				client.Account.Language = LanguageMgr.LangsToName(LanguageMgr.NameToLangs(args[1].ToUpper()));
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Language.Set", LanguageMgr.LangsToCompleteName(client, LanguageMgr.NameToLangs(client.Account.Language))), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				GameServer.Database.SaveObject(client.Account);
				if (log.IsInfoEnabled)
					log.Info(client.Player.Name + " (" + client.Account.Name + ") changed language.");
				return 0;
			}
			else	client.Out.SendMessage("Command help: /language <EN|FR|DE|ES|CZ>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			return 1;
		}
	}
}