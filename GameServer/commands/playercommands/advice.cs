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
using DOL.Language;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&advice",
		ePrivLevel.Player,
		"Ask for advice from an advisor",
		"Advisors will reply via /send",
		"Please answer them via /send <Name of the Advisor>",
		"/advice - shows all advisors",
		"/advice <message>")]
	public class AdviceCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			string msg = "";
			if (args.Length >= 2)
			{
				for (int i = 1; i < args.Length; ++i)
				{
					msg += args[i] + " ";
				}
			}
			else
			{
				int total = 0;
				foreach (GameClient playerClient in WorldMgr.GetAllClients())
				{
					if (playerClient.Player == null) continue;
					if (playerClient.Player.Advisor &&
					   ((playerClient.Player.Realm == client.Player.Realm && playerClient.Player.IsAnonymous == false) ||
					   client.Account.PrivLevel > 1))
					{
						total++;
						client.Out.SendMessage(total + ")" + playerClient.Player.Name + (playerClient.Player.IsAnonymous ? " [ANON]" : ""), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}

				}
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Advice.AdvicersOn", total), eChatType.CT_System, eChatLoc.CL_PopupWindow);
				return;
			}
			foreach (GameClient playerClient in WorldMgr.GetAllClients())
			{
				if (playerClient.Player == null) continue;
				if ((playerClient.Player.Advisor &&
					playerClient.Player.Realm == client.Player.Realm) ||
					playerClient.Account.PrivLevel > 1)
					playerClient.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Advice.Advice", getRealmString(client.Player.Realm), client.Player.Name, msg), eChatType.CT_Staff, eChatLoc.CL_ChatWindow);

			}
		}

		public string getRealmString(eRealm Realm)
		{
			switch (Realm)
			{
				case eRealm.Albion: return " ALB";
				case eRealm.Midgard: return " MID";
				case eRealm.Hibernia: return " HIB";
				default: return " NONE";
			}
		}
	}
}