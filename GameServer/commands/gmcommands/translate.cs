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
using DOL.GS.Keeps;

namespace DOL.GS.Commands
{
	[CmdAttribute("&translate", ePrivLevel.GM,
	              "'/translate <TranslationID> <text in your actual language>' : translate the string in current language",
	              "'/translate showid <sentence>' : show the TranslationID related to the sentence",
	              "example : '/translate Effects.StaticEffect.YouCantRemoveThisEffect You can't remove this effect!'",
                  "'/translate name <language> <name>' to add or change the name of your target.",
                  "'/translate suffix <language> <suffix>' to add or change the suffix of your target.",
                  "'/translate guildname <language> <guild name> to add or change the guild name of your target.",
                  "'/translate examinearticle <language> <examine article>' to add or change the examine article of your target.",
                  "'/translate messagearticle <language> <message article>' to add or change the message article of your target.")]
	public class GMLanguageBaseCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (client == null || client.Player == null || args.Length < 3)
			{
				DisplaySyntax(client);
				return;
			}

            GameObject target = null;

            if (client.Player.TargetObject != null && client.Player.TargetObject is GameObject)
                target = (GameObject)client.Player.TargetObject;

            //switch (args[1].ToLower())
            //{
                //case "name":
                //    {
                //        if (target == null || target is IDoor)
                //            client.Out.SendMessage("You need a valid target.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                //        else
                //            name(client, target, args);

                //        return;
                //    }
                //case "suffix":
                //    {
                //        if (target == null || target is IDoor)
                //            client.Out.SendMessage("You need a valid target.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                //        else
                //            suffix(client, target, args);

                //        return;
                //    }
                //case "guildname":
                //    {
                //        if (target == null || target is IDoor)
                //            client.Out.SendMessage("You need a valid target.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                //        else
                //            guildname(client, target, args);

                //        return;
                //    }
                //case "examinearticle":
                //    {
                //        if (target == null || target is IDoor)
                //            client.Out.SendMessage("You need a valid target.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                //        else
                //            examinearticle(client, target, args);

                //        return;
                //    }
                //case "messagearticle":
                //    {
                //        if (target == null || target is IDoor)
                //            client.Out.SendMessage("You need a valid target.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                //        else
                //            messagearticle(client, target, args);

                //        return;
                //    }
            //    default:
            //        break;
            //}

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

        //private void name(GameClient client, GameObject target, string[] args)
        //{
        //    if (args.Length < 4)
        //    {
        //        DisplaySyntax(client, args[1]);
        //        return;
        //    }

        //    if (target is GameStaticItem)
        //    {
        //        client.Out.SendMessage("Not supported yet.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
        //        return;
        //    }

        //    if (target is GameNPC)
        //    {
        //        GameNPC npc = (GameNPC)target;
        //        if (Util.IsEmpty(npc.TranslationId))
        //        {
        //            client.Out.SendMessage("Your target doesn't have a translation id, please use '/mob translationid <translation id>' first.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
        //            return;
        //        }

        //        DBLanguageNPC result = GameServer.Database.SelectObject<DBLanguageNPC>("TranslationId = '" + GameServer.Database.Escape(npc.TranslationId) +
        //                                                                               "' and Language = '" + args[2].ToUpper() + "'");
        //        if (result != null)
        //            GameServer.Database.DeleteObject(result);
        //        else
        //        {
        //            result = new DBLanguageNPC();
        //            result.TranslationId = npc.TranslationId;
        //            result.Suffix = string.Empty;
        //            result.GuildName = string.Empty;
        //            result.ExamineArticle = string.Empty;
        //            result.MessageArticle = string.Empty;
        //            result.Language = args[2].ToUpper();
        //        }

        //        if (args.Length > 4)
        //            result.Name = string.Join(" ", args, 3, args.Length - 3);
        //        else
        //            result.Name = args[3];

        //        GameServer.Database.AddObject(result);

        //        if (!(npc is GameMovingObject) && !(npc is GamePet) && client.Account.Language.ToUpper() == args[2].ToUpper())
        //        {
        //            npc.RemoveFromWorld();
        //            GameNPC.RefreshTranslation(args[2].ToUpper(), result.TranslationId);
        //            npc.AddToWorld();
        //        }
        //        else
        //            GameNPC.RefreshTranslation(args[2].ToUpper(), result.TranslationId);

        //        client.Out.SendMessage("Name for language '" + args[2].ToUpper() + "' changed to: " + result.Name, eChatType.CT_System, eChatLoc.CL_SystemWindow);
        //        return;
        //    }

        //    DisplaySyntax(client, args[1]);
        //}

        //private void suffix(GameClient client, GameObject target, string[] args)
        //{
        //    if (args.Length < 4)
        //    {
        //        DisplaySyntax(client, args[1]);
        //        return;
        //    }

        //    if (target is GameStaticItem)
        //    {
        //        client.Out.SendMessage("GameStaticItems don't have a suffix!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
        //        return;
        //    }

        //    if (target is GameNPC)
        //    {
        //        GameNPC npc = (GameNPC)target;
        //        if (Util.IsEmpty(npc.TranslationId))
        //        {
        //            client.Out.SendMessage("Your target doesn't have a translation id, please use '/mob translationid <translation id>' first.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
        //            return;
        //        }

        //        if (target is GameMovingObject)
        //        {
        //            client.Out.SendMessage("You cannot set a suffix for GameMovingObjects.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
        //            return;
        //        }
        //        else // GameNPC, GamePet etc.
        //        {
        //            DBLanguageNPC result = GameServer.Database.SelectObject<DBLanguageNPC>("TranslationId = '" + GameServer.Database.Escape(npc.TranslationId) +
        //                                               "' and Language = '" + args[2].ToUpper() + "'");
        //            if (result != null)
        //                GameServer.Database.DeleteObject(result);
        //            else
        //            {
        //                result = new DBLanguageNPC();
        //                result.TranslationId = npc.TranslationId;
        //                result.Name = string.Empty;
        //                result.GuildName = string.Empty;
        //                result.ExamineArticle = string.Empty;
        //                result.MessageArticle = string.Empty;
        //                result.Language = args[2].ToUpper();
        //            }

        //            if (args.Length > 4)
        //                result.Suffix = string.Join(" ", args, 3, args.Length - 3);
        //            else
        //                result.Suffix = args[3];

        //            GameServer.Database.AddObject(result);
        //            GameNPC.RefreshTranslation(args[2].ToUpper(), result.TranslationId);
        //            client.Out.SendMessage("Suffix for language '" + args[2].ToUpper() + "' changed to: " + result.Suffix, eChatType.CT_System, eChatLoc.CL_SystemWindow);
        //            return;
        //        }
        //    }

        //    DisplaySyntax(client, args[1]);
        //}

        //private void guildname(GameClient client, GameObject target, string[] args)
        //{
        //    if (args.Length < 4)
        //    {
        //        DisplaySyntax(client, args[1]);
        //        return;
        //    }

        //    if (target is GameStaticItem)
        //    {
        //        client.Out.SendMessage("GameStaticObjects don't have a guild name!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
        //        return;
        //    }

        //    if (target is GameNPC)
        //    {
        //        if (target is GameKeepGuard)
        //        {
        //            client.Out.SendMessage("You cannot translate the guild name of a GameKeepGuard!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
        //            return;
        //        }

        //        if (target is GameMovingObject)
        //        {
        //            client.Out.SendMessage("GameMovingObjects don't have a guild name!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
        //            return;
        //        }

        //        // Not sure if we should allow (or not) guild name translations for GamePets.

        //        GameNPC npc = (GameNPC)target;
        //        if (Util.IsEmpty(npc.TranslationId))
        //        {
        //            client.Out.SendMessage("Your target doesn't have a translation id, please use '/mob translationid <translation id>' first.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
        //            return;
        //        }

        //        DBLanguageNPC result = GameServer.Database.SelectObject<DBLanguageNPC>("TranslationId = '" + GameServer.Database.Escape(npc.TranslationId) +
        //                                                                               "' and Language = '" + args[2].ToUpper() + "'");
        //        if (result != null)
        //            GameServer.Database.DeleteObject(result);
        //        else
        //        {
        //            result = new DBLanguageNPC();
        //            result.TranslationId = npc.TranslationId;
        //            result.Name = string.Empty;
        //            result.Suffix = string.Empty;
        //            result.ExamineArticle = string.Empty;
        //            result.MessageArticle = string.Empty;
        //            result.Language = args[2].ToUpper();
        //        }

        //        if (args.Length > 4)
        //            result.GuildName = string.Join(" ", args, 3, args.Length - 3);
        //        else
        //            result.GuildName = args[3];

        //        GameServer.Database.AddObject(result);

        //        if (!(npc is GameMovingObject) && !(npc is GamePet) && client.Account.Language.ToUpper() == args[2].ToUpper())
        //        {
        //            npc.RemoveFromWorld();
        //            GameNPC.RefreshTranslation(args[2].ToUpper(), result.TranslationId);
        //            npc.AddToWorld();
        //        }
        //        else
        //            GameNPC.RefreshTranslation(args[2].ToUpper(), result.TranslationId);

        //        client.Out.SendMessage("Guild name for language '" + args[2].ToUpper() + "' changed to: " + result.GuildName, eChatType.CT_System, eChatLoc.CL_SystemWindow);
        //        return;
        //    }

        //    DisplaySyntax(client, args[1]);
        //}

        //private void examinearticle(GameClient client, GameObject target, string[] args)
        //{
        //    if (args.Length < 4)
        //    {
        //        DisplaySyntax(client, args[1]);
        //        return;
        //    }

        //    if (target is GameStaticItem)
        //    {
        //        client.Out.SendMessage("Not supported yet.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
        //        return;
        //    }

        //    if (target is GameNPC)
        //    {
        //        GameNPC npc = (GameNPC)target;
        //        if (Util.IsEmpty(npc.TranslationId))
        //        {
        //            client.Out.SendMessage("Your target doesn't have a translation id, please use '/mob translationid <translation id>' first.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
        //            return;
        //        }

        //        DBLanguageNPC result = GameServer.Database.SelectObject<DBLanguageNPC>("TranslationId = '" + GameServer.Database.Escape(npc.TranslationId) +
        //                                                                               "' and Language = '" + args[2].ToUpper() + "'");
        //        if (result != null)
        //            GameServer.Database.DeleteObject(result);
        //        else
        //        {
        //            result = new DBLanguageNPC();
        //            result.TranslationId = npc.TranslationId;
        //            result.Name = string.Empty;
        //            result.Suffix = string.Empty;
        //            result.GuildName = string.Empty;
        //            result.MessageArticle = string.Empty;
        //            result.Language = args[2].ToUpper();
        //        }

        //        if (args.Length > 4)
        //            result.ExamineArticle = string.Join(" ", args, 3, args.Length - 3);
        //        else
        //            result.ExamineArticle = args[3];

        //        GameServer.Database.AddObject(result);
        //        GameNPC.RefreshTranslation(args[2].ToUpper(), result.TranslationId);
        //        client.Out.SendMessage("Examine article for language '" + args[2].ToUpper() + "' changed to: " + result.ExamineArticle, eChatType.CT_System, eChatLoc.CL_SystemWindow);
        //        return;
        //    }

        //    DisplaySyntax(client, args[1]);
        //}

        //private void messagearticle(GameClient client, GameObject target, string[] args)
        //{
        //    if (args.Length < 4)
        //    {
        //        DisplaySyntax(client, args[1]);
        //        return;
        //    }

        //    if (target is GameStaticItem)
        //    {
        //        client.Out.SendMessage("Not supported yet.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
        //        return;
        //    }

        //    if (target is GameNPC)
        //    {
        //        GameNPC npc = (GameNPC)target;
        //        if (Util.IsEmpty(npc.TranslationId))
        //        {
        //            client.Out.SendMessage("Your target doesn't have a translation id, please use '/mob translationid <translation id>' first.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
        //            return;
        //        }

        //        DBLanguageNPC result = GameServer.Database.SelectObject<DBLanguageNPC>("TranslationId = '" + GameServer.Database.Escape(npc.TranslationId) +
        //                                                                               "' and Language = '" + args[2].ToUpper() + "'");
        //        if (result != null)
        //            GameServer.Database.DeleteObject(result);
        //        else
        //        {
        //            result = new DBLanguageNPC();
        //            result.TranslationId = npc.TranslationId;
        //            result.Name = string.Empty;
        //            result.Suffix = string.Empty;
        //            result.GuildName = string.Empty;
        //            result.ExamineArticle = string.Empty;
        //            result.Language = args[2].ToUpper();
        //        }

        //        if (args.Length > 4)
        //            result.MessageArticle = string.Join(" ", args, 3, args.Length - 3);
        //        else
        //            result.MessageArticle = args[3];

        //        GameServer.Database.AddObject(result);
        //        GameNPC.RefreshTranslation(args[2].ToUpper(), result.TranslationId);
        //        client.Out.SendMessage("Message article for language '" + args[2].ToUpper() + "' changed to: " + result.MessageArticle, eChatType.CT_System, eChatLoc.CL_SystemWindow);
        //        return;
        //    }

        //    DisplaySyntax(client, args[1]);
        //}
	}
}