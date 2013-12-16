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
using System.Collections.Generic;
using System.Linq;

using DOL.Database;
using DOL.Language;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
    [Cmd("&translate", ePrivLevel.GM,
         "Use '/translate add [Language] [TranslationId] [Text]' to add a new translation.",
         "Use '/translate debug' to activate / deactivate the LanguageMgr debug mode for you and to receive extended messages or not.",
         "Use '/translate memadd [Language] [TranslationId]' to add a language object to your temporary properties. Use this sub command if the combination of a translation id and text is longer than the DAoC chat allows for one \"line\".",
         "Use '/translate memclear' to remove the previously added language object from your temporary properties.",
         "Use '/translate memsave [Text]' to add a text to your language object and to save it into the database. This command will also register your new translation in the LanguageMgr.",
         "Use '/translate memshow' to show the translation id and the language of your language object.",
         "Use '/translate refresh [Language] [TranslationId] [Text]' to refresh a existing translation.",
         "Use '/translate select [Language] [TranslationId]' to select a existing translation and to add it's language object into your temporary properties. Use this sub command if the combination of a translation id and text is longer than the DAoC chat allows for one \"line\".",
         "Use '/translate selectclear' to remove the previously selected language object from your temporary properties.",
         "Use '/translate selectsave [Text]' to refresh the text of the selected translation and to save it's language object into the database.",
         "Use '/translate selectshow' to show the language, translation id and the text of your selected language object.",
         "Use '/translate show [Language] [TranslationId]' to show the translated text of the given language and translation id.")]
    //"Use '/translate showlist [showall or Language]' to show a sorted list of all registered translations or to show a list of all translations of a language.")]
    public class TranslateCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        private const string LANGUAGEMGR_MEM_LNG_OBJ = "LANGUAGEMGR_MEM_LNG_OBJ";
        private const string LANGUAGEMGR_SEL_LNG_OBJ = "LANGUAGEMGR_SEL_LNG_OBJ";

        public void OnCommand(GameClient client, string[] args)
        {
            if (IsSpammingCommand(client.Player, "translate"))
                return;

            if (args.Length < 2)
            {
                DisplaySyntax(client);
                return;
            }

            switch (args[1].ToLower())
            {
                #region add
                case "add":
                    {
                        if (client.Account.PrivLevel != (uint)ePrivLevel.Player)
                        {
                            if (args.Length < 5)
                            {
                                DisplayMessage(client, "[Language-Manager] Usage: '/translate add [Language] [TranslationId] [Text]'");
                                return;
                            }

                            LanguageDataObject translation = LanguageMgr.GetLanguageDataObject(args[2].ToUpper(), args[3], LanguageDataObject.eTranslationIdentifier.eSystem);
                            if (translation != null)
                            {
                                DisplayMessage(client, "[Language-Manager] This translation id is already in use by the given language! ( Language <" + args[2].ToUpper() + "> - TranslationId <" + args[3] + "> )");
                                return;
                            }

                            translation = new DBLanguageSystem();
                            ((DBLanguageSystem)translation).TranslationId = args[3];
                            ((DBLanguageSystem)translation).Text = args[4];
                            ((DBLanguageSystem)translation).Language = args[2];

                            GameServer.Database.AddObject(translation);
                            LanguageMgr.RegisterLanguageDataObject(translation);
                            DisplayMessage(client, "[Language-Manager] Translation successfully added! (Language <" + args[2].ToUpper() + "> - TranslationId <" + args[3] + "> )");
                            return;
                        }

                        return;
                    }
                #endregion add

                #region debug
                case "debug":
                    {
                        bool debug = client.Player.TempProperties.getProperty("LANGUAGEMGR-DEBUG", false);
                        debug = !debug;
                        client.Player.TempProperties.setProperty("LANGUAGEMGR-DEBUG", debug);
                        DisplayMessage(client, "[Language-Manager] Debug mode: " + (debug ? "ON" : "OFF"));
                        return;
                    }
                #endregion debug

                #region memadd
                case "memadd":
                    {
                        // This sub command adds a new language object to your temp properties which will "pre save" the given translation id
                        // and language. Use this sub command if the translation id or text of your new translation requires more room
                        // as the DAoC chat allows you to use in one line. Use the memsave sub command to add a text to this language object
                        // and to save it into the database - or use "memclear" to remove the language object from your temp properties.

                        if (args.Length < 4)
                            DisplayMessage(client, "[Language-Manager] Usage: '/translate memadd [Language] [TranslationId]'");
                        else
                        {
                            LanguageDataObject lngObj = (LanguageDataObject)client.Player.TempProperties.getProperty<object>(LANGUAGEMGR_MEM_LNG_OBJ, null);

                            if (lngObj != null)
                                DisplayMessage(client, "[Language-Manager] Can't add language object, there is already another one!");
                            else
                            {
                                lngObj = LanguageMgr.GetLanguageDataObject(args[2].ToUpper(), args[3], LanguageDataObject.eTranslationIdentifier.eSystem);

                                if (lngObj != null)
                                    DisplayMessage(client, "[Language-Manager] The combination of the given TranslationId <" + args[3] + "> and Language <" + args[2].ToUpper() + "> is already in use!");
                                else
                                {
                                    lngObj = new DBLanguageSystem();
                                    ((DBLanguageSystem)lngObj).TranslationId = args[3];
                                    ((DBLanguageSystem)lngObj).Language = args[2];

                                    client.Player.TempProperties.setProperty(LANGUAGEMGR_MEM_LNG_OBJ, lngObj);
                                    DisplayMessage(client, "[Language-Manager] Language object successfully added to your temporary properties! ( Language <" + args[2].ToUpper() + "> TranslationId <" + args[3] + "> )");
                                }
                            }
                        }

                        return;
                    }
                #endregion memadd

                #region memclear
                case "memclear":
                    {
                        // Removes the language object from your temp properties you've previously added with the "memadd" sub command.
                        LanguageDataObject lngObj = (LanguageDataObject)client.Player.TempProperties.getProperty<object>(LANGUAGEMGR_MEM_LNG_OBJ, null);

                        if (lngObj == null)
                            DisplayMessage(client, "[Language-Manager] No language object found.");
                        else
                        {
                            client.Player.TempProperties.removeProperty(LANGUAGEMGR_MEM_LNG_OBJ);
                            DisplayMessage(client, "[Language-Manager] Language object successfully removed.");
                        }

                        return;
                    }
                #endregion memclear

                #region memsave
                case "memsave":
                    {
                        // See "memadd" sub command for a description.
                        if (args.Length < 3)
                            DisplayMessage(client, "[Language-Manager] Usage: '/translate memsave [Text]'");
                        else
                        {
                            LanguageDataObject lngObj = (LanguageDataObject)client.Player.TempProperties.getProperty<object>(LANGUAGEMGR_MEM_LNG_OBJ, null);

                            if (lngObj == null)
                                DisplayMessage(client, "[Language-Manager] No language object found.");
                            else
                            {
                                if (args.Length > 3)
                                    ((DBLanguageSystem)lngObj).Text = string.Join(" ", args, 2, args.Length - 2);
                                else
                                    ((DBLanguageSystem)lngObj).Text = args[2];

                                if (!LanguageMgr.RegisterLanguageDataObject(lngObj))
                                    DisplayMessage(client, "[Language-Manager] Can't register language object in LanguageMgr, there is already another one!");
                                else
                                {
                                    GameServer.Database.AddObject(lngObj);
                                    client.Player.TempProperties.removeProperty(LANGUAGEMGR_MEM_LNG_OBJ);
                                    DisplayMessage(client, "[Language-Manager] Translation successfully added into the database and registered in LanguageMgr.");
                                }
                            }
                        }

                        return;
                    }
                #endregion memsave

                #region memshow
                case "memshow":
                    {
                        LanguageDataObject lngObj = (LanguageDataObject)client.Player.TempProperties.getProperty<object>(LANGUAGEMGR_MEM_LNG_OBJ, null);

                        if (lngObj == null)
                            DisplayMessage(client, "[Language-Manager] No language object found.");
                        else
                            DisplayMessage(client, "[Language-Manager] Language object info: Language <" + lngObj.Language + "> TranslationId <" + lngObj.TranslationId + ">");

                        return;
                    }
                #endregion memshow

                #region refresh
                case "refresh":
                    {
                        if (args.Length < 5)
                            DisplayMessage(client, "[Language-Manager] Usage: '/translate refresh [Language] [TranslationId] [Text]'");
                        else
                        {
                            LanguageDataObject lngObj = LanguageMgr.GetLanguageDataObject(args[2].ToUpper(), args[3], LanguageDataObject.eTranslationIdentifier.eSystem);

                            if (lngObj == null)
                                DisplayMessage(client, "[Language-Manager] Can't find TranslationId <" + args[3] + "> (Language <" + args[2].ToUpper() + "> !");
                            else
                            {
                                ((DBLanguageSystem)lngObj).Text = args[3];
                                GameServer.Database.SaveObject(lngObj);
                                DisplayMessage(client, "[Language-Manager] TranslationId <" + args[3] + "> (Language: " + args[2].ToUpper() + " ) successfully updated in database!");
                            }
                        }

                        return;
                    }
                #endregion refresh

                #region select
                case "select":
                    {
                        if (args.Length < 4)
                            DisplayMessage(client, "[Language-Manager] Usage: '/translate select [Language] [TranslationId]'");
                        else
                        {
                            LanguageDataObject lngObj = (LanguageDataObject)client.Player.TempProperties.getProperty<object>(LANGUAGEMGR_SEL_LNG_OBJ, null);

                            if (lngObj != null)
                            {
                                DisplayMessage(client, "[Language-Manager] You already have selected a language object! ( Language <" + ((DBLanguageSystem)lngObj).Language +
                                                       "> - TranslationId <" + ((DBLanguageSystem)lngObj).TranslationId + "> )");
                            }
                            else
                            {
                                lngObj = LanguageMgr.GetLanguageDataObject(args[2].ToUpper(), args[3], LanguageDataObject.eTranslationIdentifier.eSystem);

                                if (lngObj == null)
                                {
                                    DisplayMessage(client, "[Language-Manager] Can't find language object. ( Language <" + args[2].ToUpper() +
                                                           "> - TranslationId <" + args[3] + "> )");
                                }
                                else
                                {
                                    client.Player.TempProperties.setProperty(LANGUAGEMGR_SEL_LNG_OBJ, lngObj);
                                    DisplayMessage(client, "[Language-Manager] Language object found and added to your temporary properties! ( Language <" + args[2].ToUpper() +
                                                           "> - TranslationId <" + args[3] + "> )");
                                }
                            }
                        }

                        return;
                    }
                #endregion select

                #region selectclear
                case "selectclear":
                    {
                        // Removes the language object from your temp properties you've previously selected with the "select" sub command.
                        LanguageDataObject lngObj = (LanguageDataObject)client.Player.TempProperties.getProperty<object>(LANGUAGEMGR_SEL_LNG_OBJ, null);

                        if (lngObj == null)
                            DisplayMessage(client, "[Language-Manager] No language object selected!");
                        else
                        {
                            client.Player.TempProperties.removeProperty(LANGUAGEMGR_SEL_LNG_OBJ);
                            DisplayMessage(client, "[Language-Manager] Language object successfully removed from your temporary properties." +
                                                   "( Language <" + ((DBLanguageSystem)lngObj).Language +
                                                   "> - TranslationId <" + ((DBLanguageSystem)lngObj).TranslationId + "> )");
                        }

                        return;
                    }
                #endregion selectclear

                #region selectsave
                case "selectsave":
                    {
                        if (args.Length < 3)
                            DisplayMessage(client, "[Language-Manager] Usage: '/translate selectsave [Text]'");
                        else
                        {
                            LanguageDataObject lngObj = (LanguageDataObject)client.Player.TempProperties.getProperty<object>(LANGUAGEMGR_SEL_LNG_OBJ, null);

                            if (lngObj == null)
                                DisplayMessage(client, "[Language-Manager] No language object selected!");
                            else
                            {
                                if (args.Length > 3)
                                    ((DBLanguageSystem)lngObj).Text = string.Join(" ", args, 2, args.Length - 2);
                                else
                                    ((DBLanguageSystem)lngObj).Text = args[2];

                                GameServer.Database.SaveObject(lngObj);
                                client.Player.TempProperties.removeProperty(LANGUAGEMGR_SEL_LNG_OBJ);
                                DisplayMessage(client, "[Language-Manager] Language object successfully changed and saved in database." +
                                                       "( Language <" + ((DBLanguageSystem)lngObj).Language +
                                                       "> - TranslationId <" + ((DBLanguageSystem)lngObj).TranslationId +
                                                       "> - Text <" + ((DBLanguageSystem)lngObj).Text + "> )");
                            }
                        }

                        return;
                    }
                #endregion selectsave

                #region selectshow
                case "selectshow":
                    {
                        LanguageDataObject lngObj = (LanguageDataObject)client.Player.TempProperties.getProperty<object>(LANGUAGEMGR_SEL_LNG_OBJ, null);

                        if (lngObj == null)
                            DisplayMessage(client, "[Language-Manager] No language object selected!");
                        else
                            DisplayMessage(client, "[Language-Manager] Language object info: Language <" + lngObj.Language + "> - TranslationId <" + lngObj.TranslationId +
                                                   "> - Text <" + ((DBLanguageSystem)lngObj).Text + ">");
                        return;
                    }
                #endregion selectshow

                #region show
                case "show":
                    {
                        if (args.Length < 4)
                            DisplayMessage(client, "[Language-Manager] Usage: '/translate show [Language] [TranslationId]'");
                        else
                        {
                            LanguageDataObject lngObj = LanguageMgr.GetLanguageDataObject(args[2].ToUpper(), args[3], LanguageDataObject.eTranslationIdentifier.eSystem);

                            if (lngObj == null)
                                DisplayMessage(client, "[Language-Manager] Can't find language object. ( Language <" + args[2].ToUpper() +
                                                       "> - TranslationId <" + args[3] + "> )");
                            else
                                DisplayMessage(client, "[Language-Manager] " + ((DBLanguageSystem)lngObj).Text);
                        }

                        return;
                    }
                #endregion show

                #region showlist
                /*
                 * The code works fine, but DAoC does not support a such huge list.
                 * 
                 * case "showlist":
                    {
                        if (args.Length < 3)
                            DisplayMessage(client, "aaa");
                        else
                        {
                            #region showall
                            if (args[2].ToLower() == "showall")
                            {
                                IDictionary<string, IList<string>> idLangs = new Dictionary<string, IList<string>>();
                                List<string> languages = new List<string>();
                                languages.AddRange(LanguageMgr.Languages);
                                IList<string> data = new List<string>();

                                foreach (string language in LanguageMgr.Translations.Keys)
                                {
                                    if (!LanguageMgr.Translations[language].ContainsKey(LanguageDataObject.eTranslationIdentifier.eSystem))
                                        continue;

                                    data.Add("======== Language <" + language + "> ========\n\n");

                                    foreach (LanguageDataObject lngObj in LanguageMgr.Translations[language][LanguageDataObject.eTranslationIdentifier.eSystem])
                                    {
                                        data.Add("TranslationId: " + lngObj.TranslationId + "\nText: " + ((DBLanguageSystem)lngObj).Text + "\n\n");

                                        if (!idLangs.ContainsKey(lngObj.TranslationId))
                                        {
                                            IList<string> langs = new List<string>();
                                            langs.Add(lngObj.Language);
                                            idLangs.Add(lngObj.TranslationId, langs);
                                            continue;
                                        }

                                        if (!idLangs[lngObj.TranslationId].Contains(lngObj.Language))
                                            idLangs[lngObj.TranslationId].Add(lngObj.Language);

                                        continue;
                                    }
                                }

                                IDictionary<string, IList<string>> missingLanguageTranslations = new Dictionary<string, IList<string>>();

                                foreach (string translationId in idLangs.Keys)
                                {
                                    foreach (string language in languages)
                                    {
                                        if (idLangs[translationId].Contains(language))
                                            continue;

                                        if (!missingLanguageTranslations.ContainsKey(translationId))
                                        {
                                            IList<string> langs = new List<string>();
                                            langs.Add(language);
                                            missingLanguageTranslations.Add(translationId, langs);
                                            continue;
                                        }

                                        if (!missingLanguageTranslations[translationId].Contains(language))
                                            missingLanguageTranslations[translationId].Add(language);

                                        continue;
                                    }
                                }

                                if (missingLanguageTranslations.Count > 0)
                                {
                                    data.Add("======== Missing language translations ========\n\n");

                                    foreach (string translationId in missingLanguageTranslations.Keys)
                                    {
                                        string str = ("TranslationId: " + translationId + "\nLanguages: ");

                                        foreach (string language in missingLanguageTranslations[translationId])
                                            str += (language + ",");

                                        if (str[(str.Length - 1)] == ',')
                                            str = str.Remove(str.Length - 1);

                                        data.Add(str);
                                    }
                                }

                                client.Out.SendCustomTextWindow("[Language-Manager] Translations", data); // I wish you a merry christmas and a happy new year (2112)! :-)
                            }
                            #endregion  showall

                            #region language
                            else
                            {
                                if (!LanguageMgr.Languages.Contains(args[2].ToUpper()))
                                    DisplayMessage(client, "aaa");
                                else
                                {
                                    if (!LanguageMgr.Translations[args[2].ToUpper()].ContainsKey(LanguageDataObject.eTranslationIdentifier.eSystem))
                                        DisplayMessage(client, "aaa");
                                    else
                                    {
                                        IList<string> data = new List<string>();

                                        foreach (LanguageDataObject lngObj in LanguageMgr.Translations[args[2].ToUpper()][LanguageDataObject.eTranslationIdentifier.eSystem])
                                            data.Add("TranslationId: " + lngObj.TranslationId + "\nText: " + ((DBLanguageSystem)lngObj).Text + "\n\n");

                                        client.Out.SendCustomTextWindow("[Language-Manager] Language translations <" + args[2].ToUpper() + ">", data);
                                    }
                                }
                            }
                            #endregion language
                        }

                        return;
                    }*/
                #endregion showlist

                default:
                    {
                        DisplaySyntax(client);
                        return;
                    }
            }
        }
    }
}