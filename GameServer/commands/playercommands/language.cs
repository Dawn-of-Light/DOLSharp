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
using DOL.Database;
using log4net;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&language",
		ePrivLevel.Player,
		"Change your server-language display",
		"/language <EN|IT|FR|DE|ES|CZ>")]
	public class LanguageCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public void OnCommand(GameClient client, string[] args)
		{
			if (DOL.GS.ServerProperties.Properties.ALLOW_CHANGE_LANGUAGE == false)
				return;
			if (args.Length == 1)
			{
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Language.Current", LanguageMgr.LangsToCompleteName(client, LanguageMgr.NameToLangs(client.Account.Language))), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else if (args.Length == 2)
			{
                if (args[1].ToLower() == "debug" && client.Account.PrivLevel > 1)
                {
                    bool debug = client.Player.TempProperties.getProperty("LANGUAGEMGR-DEBUG", false);
                    debug = !debug;
                    client.Player.TempProperties.setProperty("LANGUAGEMGR-DEBUG", debug);
                    client.Out.SendMessage("[LanguageMgr] Debug mode : " + (debug ? "ON" : "OFF"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return;
                }
                // Valid language -> English default
				client.Account.Language = LanguageMgr.LangsToName(LanguageMgr.NameToLangs(args[1].ToUpper()));
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Language.Set", LanguageMgr.LangsToCompleteName(client, LanguageMgr.NameToLangs(client.Account.Language))), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				GameServer.Database.SaveObject(client.Account);
				if (log.IsInfoEnabled)
					log.Info(client.Player.Name + " (" + client.Account.Name + ") changed language.");
			}
			else client.Out.SendMessage("Command help: /language <EN|IT|FR|DE|ES|CZ>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}
	}

    public class GMLanguageBaseCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        public void OnCommand(GameClient client, string[] args)
        {
            if (client == null
                || client.Player == null
                || client.Account.PrivLevel < 2
                || args.Length < 1
                || args[0].Length != 8) return;

            string lang = args[0].ToUpper().Substring(6, 2);
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
                case "ES": obj.ES = sentence; break;
                case "CZ": obj.CZ = sentence; break;
                default: break;
            }
            if (create) GameServer.Database.AddObject(obj);
            else GameServer.Database.SaveObject(obj);
            DisplayMessage(player.Client, "The Translation [" + id + "] [" + lang + "] is now <" + sentence + "> (" + (create ? "created" : "updated") + ") !");
        }
    }
    [CmdAttribute("&gmlanen", ePrivLevel.Admin,
        "'/gmlanen <TranslationID> <text in english>', example : '/lanen Effects.StaticEffect.YouCantRemoveThisEffect You can't remove this effect!' ")]
    public class GMLanguageENCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        public void OnCommand(GameClient client, string[] args)
        {
            if (args.Length < 3 || args[0].Length != 6)
            {
                DisplaySyntax(client);
                return;
            }
            GMLanguageBaseCommandHandler command = new GMLanguageBaseCommandHandler();
            command.OnCommand(client, args);
        }
    }
    [CmdAttribute("&gmlanfr", ePrivLevel.Admin,
        "'/gmlanfr <TranslationID> <text in french>', example : '/lanen Effects.StaticEffect.YouCantRemoveThisEffect You can't remove this effect!' ")]
    public class GMLanguageFRCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        public void OnCommand(GameClient client, string[] args)
        {
            if (args.Length < 3 || args[0].Length != 8)
            {
                DisplaySyntax(client);
                return;
            }
            GMLanguageBaseCommandHandler command = new GMLanguageBaseCommandHandler();
            command.OnCommand(client, args);
        }
    }
    [CmdAttribute("&gmlande", ePrivLevel.Admin,
        "'/gmlanfr <TranslationID> <text in german>', example : '/lanen Effects.StaticEffect.YouCantRemoveThisEffect You can't remove this effect!' ")]
    public class GMLanguageDECommandHandler : AbstractCommandHandler, ICommandHandler
    {
        public void OnCommand(GameClient client, string[] args)
        {
            if (args.Length < 3 || args[0].Length != 8)
            {
                DisplaySyntax(client);
                return;
            }
            GMLanguageBaseCommandHandler command = new GMLanguageBaseCommandHandler();
            command.OnCommand(client, args);
        }
    }
    [CmdAttribute("&gmlanit", ePrivLevel.Admin,
        "'/gmlanit <TranslationID> <text in italian>', example : '/lanen Effects.StaticEffect.YouCantRemoveThisEffect You can't remove this effect!' ")]
    public class GMLanguageITCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        public void OnCommand(GameClient client, string[] args)
        {
            if (args.Length < 3 || args[0].Length != 8)
            {
                DisplaySyntax(client);
                return;
            }
            GMLanguageBaseCommandHandler command = new GMLanguageBaseCommandHandler();
            command.OnCommand(client, args);
        }
    }
    [CmdAttribute("&gmlanes", ePrivLevel.Admin,
        "'/gmlanes <TranslationID> <text in spanish>', example : '/lanen Effects.StaticEffect.YouCantRemoveThisEffect You can't remove this effect!' ")]
    public class GMLanguageESCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        public void OnCommand(GameClient client, string[] args)
        {
            if (args.Length < 3 || args[0].Length != 8)
            {
                DisplaySyntax(client);
                return;
            }
            GMLanguageBaseCommandHandler command = new GMLanguageBaseCommandHandler();
            command.OnCommand(client, args);
        }
    }
    [CmdAttribute("&gmlancz", ePrivLevel.Admin,
        "'/gmlancz <TranslationID> <text in russian>', example : '/lanen Effects.StaticEffect.YouCantRemoveThisEffect You can't remove this effect!' ")]
    public class GMLanguageCZCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        public void OnCommand(GameClient client, string[] args)
        {
            if (args.Length < 3 || args[0].Length != 8)
            {
                DisplaySyntax(client);
                return;
            }
            GMLanguageBaseCommandHandler command = new GMLanguageBaseCommandHandler();
            command.OnCommand(client, args);
        }
    }
}