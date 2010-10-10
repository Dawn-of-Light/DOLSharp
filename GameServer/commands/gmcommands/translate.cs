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
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.Language;
using System.Linq;

namespace DOL.GS.Commands
{
	[CmdAttribute("&translate", ePrivLevel.GM,
	              "'/translate <TranslationID> <text in your actual language>' : translate the string in current language",
	              "'/translate showid <sentence>' : show the TranslationID related to the sentence",
	              "example : '/translate Effects.StaticEffect.YouCantRemoveThisEffect You can't remove this effect!'")]
	public class GMLanguageBaseCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (client == null || client.Player == null || args.Length < 3)
			{
				DisplaySyntax(client);
				return;
			}

			string lang = Language.LanguageMgr.LangsToName(client.Account.Language);
			
			// ShowId displays the related translation.
			// If a GM find an incorrect sentence, he could 
			// grab its TranslationID with part of the translated text
			// ie: /translate showid Can't Mount
			// will list every translation in its language containing 'Can't Mount'
			if (args[1].ToLower().Contains("showid"))
			{
				var sentences = GameServer.Database.SelectObjects<DBLanguage>("`" + lang + "` LIKE '%" + GameServer.Database.Escape(String.Join(" ", args, 2, args.Length - 2)) + "%'");
				foreach (var text in sentences)
					DisplayMessage(client.Player, string.Format("{0} -> {1}", text.TranslationID, LanguageMgr.GetTranslation(client, text.TranslationID)));
				return;
			}

			string id = args[1];
			if (!LanguageMgr.IDSentences.ContainsKey(id))
			{
				DisplayMessage(client, "The TranslationID <" + id + "> does not exist!");
				return;
			}
			if (!LanguageMgr.IDSentences[id].ContainsKey(lang))
			{
				DisplayMessage(client, "The TranslationID <" + id + "> does not contain the language <" + lang + "> !");
				return;
			}
			string sentence = String.Join(" ", args, 2, args.Length - 2);
			client.Player.TempProperties.setProperty("LANGUAGEMGR-ID", id);
			client.Player.TempProperties.setProperty("LANGUAGEMGR-LANG", lang);
			client.Player.TempProperties.setProperty("LANGUAGEMGR-TEXT", sentence);
			client.Out.SendCustomDialog("Please confirm the change : \n[" + id + "][" + lang + "] = \n\"" + sentence + "\"", new CustomDialogResponse(Confirm));
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="player"></param>
		/// <param name="response"></param>
		private void Confirm(GamePlayer player, byte response)
		{
			if (player == null || response != 0x01) return;
			string id = player.TempProperties.getProperty<string>("LANGUAGEMGR-ID", "");
			string lang = player.TempProperties.getProperty<string>("LANGUAGEMGR-LANG", "");
			string sentence = player.TempProperties.getProperty<string>("LANGUAGEMGR-TEXT", "");
			if (id == "" || lang == "" || sentence == "") return;
			player.TempProperties.removeProperty("LANGUAGEMGR-ID");
			player.TempProperties.removeProperty("LANGUAGEMGR-LANG");
			player.TempProperties.removeProperty("LANGUAGEMGR-TEXT");
			if (!LanguageMgr.IDSentences.ContainsKey(id))
			{
				DisplayMessage(player.Client, "The TranslationID <" + id + "> does not exist!");
				return;
			}
			if (!LanguageMgr.IDSentences[id].ContainsKey(lang))
			{
				DisplayMessage(player.Client, "The TranslationID <" + id + "> does not contain the language <" + lang + "> !");
				return;
			}
			LanguageMgr.IDSentences[id][lang] = sentence;
			bool create = false;
			DBLanguage obj = GameServer.Database.SelectObject<DBLanguage>("`TranslationID` = '" + GameServer.Database.Escape(id) + "'");
			if (obj == null)
			{
				obj = new DBLanguage();
				obj.TranslationID = id;
				create = true;
			}
			switch (lang)
			{
					case "EN": obj.EN = sentence; break;
					case "DE": obj.DE = sentence; break;
					case "FR": obj.FR = sentence; break;
					case "IT": obj.IT = sentence; break;
					case "CU": obj.CU = sentence; break;
					default: break;
			}
			if (create) GameServer.Database.AddObject(obj);
			else GameServer.Database.SaveObject(obj);
			DisplayMessage(player.Client, "The Translation [" + id + "] [" + lang + "] is now <" + sentence + "> (" + (create ? "created" : "updated") + ") !");
			LanguageMgr.Refresh(id);
		}
	}
}