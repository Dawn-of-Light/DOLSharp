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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

using DOL.Database;
using DOL.GS;
using log4net;

namespace DOL.Language
{
    public class LanguageMgr
    {
        private static LanguageMgr soleInstance = new LanguageMgr();

        public static void LoadTestDouble(LanguageMgr testDouble) { soleInstance = testDouble; }

        protected virtual bool TryGetTranslationImpl(out string translation, ref string language, string translationId, ref object[] args)
        {
            translation = "";

            if (Util.IsEmpty(translationId))
            {
                translation = TRANSLATION_ID_EMPTY;
                return false;
            }

            if (Util.IsEmpty(language) || !m_translations.ContainsKey(language))
            {
                language = DefaultLanguage;
            }

            LanguageDataObject result = GetLanguageDataObject(language, translationId, LanguageDataObject.eTranslationIdentifier.eSystem);
            if (result == null)
            {
                translation = GetTranslationErrorText(language, translationId);
                return false;
            }
            else
            {
                if (!Util.IsEmpty(((DBLanguageSystem)result).Text))
                {
                    translation = ((DBLanguageSystem)result).Text;
                }
                else
                {
                    translation = GetTranslationErrorText(language, translationId);
                    return false;
                }
            }

            if (args == null)
            {
                args = new object[0];
            }

            try
            {
                if (args.Length > 0)
                    translation = string.Format(translation, args);
            }
            catch
            {
                log.ErrorFormat("[Language-Manager] Parameter number incorrect: {0} for language {1}, Arg count = {2}, sentence = '{3}', args[0] = '{4}'", translationId, language, args.Length, translation, args.Length > 0 ? args[0] : "null");
            }
            return true;
        }

        #region Variables
        private const string TRANSLATION_ID_EMPTY = "Empty translation id.";
        private const string TRANSLATION_NULL = "NULL";

        /// <summary>
        /// Translation ID for the sentence, array position 0
        /// </summary>
        private const int ID = 0;

        /// <summary>
        /// The translated sentence, array position 1
        /// </summary>
        private const int TEXT = 1;

        /// <summary>
        /// The sentence language, array position 2
        /// </summary>
        private const int LANGUAGE = 2;

        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Holds all translations (object translations and system sentence translations).
        /// </summary>
        private static IDictionary<string, IDictionary<LanguageDataObject.eTranslationIdentifier, IList<LanguageDataObject>>> m_translations;

        /// <summary>
        /// Give a way to change or relocate the lang files
        /// </summary>
        private static string LangPath { 
            get
            {
                if (soleInstance.LangPathImpl == "")
                {
                    soleInstance.LangPathImpl = Path.Combine(GameServer.Instance.Configuration.RootDirectory, "languages");
                }
                return soleInstance.LangPathImpl;
            }
        }
        protected string LangPathImpl = "";
        #endregion Variables

        #region Properties
        /// <summary>
        /// Returns the default language.
        /// </summary>
        public static string DefaultLanguage
        {
            get { return GS.ServerProperties.Properties.SERV_LANGUAGE; } // EN by default.
        }

        /// <summary>
        /// Returns all registered languages.
        /// </summary>
        public static IEnumerable<string> Languages
        {
            get
            {
                foreach (string language in m_translations.Keys)
                    yield return language;

                yield break;
            }
        }

        [Obsolete("Unused and will be deleted.")]
        public static void SetLangPath(string path)
        {
            soleInstance.LangPathImpl = path;
        }

        /// <summary>
        /// Returns the translations collection. MODIFY AT YOUR OWN RISK!!!
        /// </summary>
        public static IDictionary<string, IDictionary<LanguageDataObject.eTranslationIdentifier, IList<LanguageDataObject>>> Translations
        {
            get { return m_translations; }
        }
        #endregion Properties

        #region Initialization
        /// <summary>
        /// Initial function
        /// </summary>
        /// <returns></returns>
        public static bool Init()
        {
            m_translations = new Dictionary<string, IDictionary<LanguageDataObject.eTranslationIdentifier, IList<LanguageDataObject>>>();
            return LoadTranslations();
        }

        #region LoadTranslations
        private static bool LoadTranslations()
        {
            #region Load system translations
            if (log.IsDebugEnabled)
                log.Info("[Language-Manager] Loading system sentences...");

            ArrayList fileSentences = new ArrayList();
            bool defaultLanguageDirectoryFound = false;
            bool defaultLanguageFilesFound = false;
            foreach (string langDir in Directory.GetDirectories(LangPath, "*", SearchOption.TopDirectoryOnly))
            {
                string language = (langDir.Substring(langDir.LastIndexOf(Path.DirectorySeparatorChar) + 1)).ToUpper();
                if (language != DefaultLanguage)
                {
                    if (language != "CU") // Ignore the custom language folder. This check should be removed in the future! (code written: may 2012)
                        fileSentences.AddRange(ReadLanguageDirectory(Path.Combine(LangPath, language), language));
                }
                else
                {
                    defaultLanguageDirectoryFound = true;
                    ArrayList sentences = ReadLanguageDirectory(Path.Combine(LangPath, language), language);

                    if (sentences.Count < 1)
                        break;
                    else
                    {
                        fileSentences.AddRange(sentences);
                        defaultLanguageFilesFound = true;
                    }
                }
            }

            if (!defaultLanguageDirectoryFound)
            {
                log.Error("Could not find default '" + DefaultLanguage + "' language directory, server can't start without it!");
                return false;
            }

            if (!defaultLanguageFilesFound)
            {
                log.Error("Default '" + DefaultLanguage + "' language files missing, server can't start without those files!");
                return false;
            }

            if (DOL.GS.ServerProperties.Properties.USE_DBLANGUAGE)
            {
                int newEntries = 0;
                int updatedEntries = 0;

                IList<DBLanguageSystem> dbos = GameServer.Database.SelectAllObjects<DBLanguageSystem>();

                if (GS.ServerProperties.Properties.UPDATE_EXISTING_DB_SYSTEM_SENTENCES_FROM_FILES)
                {
                    foreach (string[] sentence in fileSentences)
                    {
                        bool found = false;
                        foreach (DBLanguageSystem dbo in dbos)
                        {
                            if (dbo.TranslationId != sentence[ID])
                                continue;

                            if (dbo.Language != sentence[LANGUAGE])
                                continue;

                            if (dbo.Text != sentence[TEXT])
                            {
                                dbo.Text = sentence[TEXT];
                                GameServer.Database.SaveObject(dbo); // Please be sure to use the UTF-8 format for your language files, otherwise
                                // some database rows will be updated on each server start, because one char
                                // differs from the one within the database.
                                updatedEntries++;

                                if (log.IsWarnEnabled)
                                    log.Warn("[Language-Manager] Language <" + sentence[LANGUAGE] + "> TranslationId <" + dbo.TranslationId + "> updated in database!");
                            }

                            found = true;
                            break;
                        }

                        if (!found)
                        {
                            DBLanguageSystem dbo = new DBLanguageSystem();
                            dbo.TranslationId = sentence[ID];
                            dbo.Text = sentence[TEXT];
                            dbo.Language = sentence[LANGUAGE];

                            GameServer.Database.AddObject(dbo);
                            RegisterLanguageDataObject(dbo);
                            newEntries++;

                            if (log.IsWarnEnabled)
                                log.Warn("[Language-Manager] Language <" + dbo.Language + "> TranslationId <" + dbo.TranslationId + "> added into the database.");
                        }
                    }
                }
                else // Add missing translations.
                {
                    foreach (string[] sentence in fileSentences)
                    {
                        bool found = false;
                        foreach (DBLanguageSystem lngObj in dbos)
                        {
                            if (lngObj.TranslationId != sentence[ID])
                                continue;

                            if (lngObj.Language != sentence[LANGUAGE])
                                continue;

                            found = true;
                            break;
                        }

                        if (!found)
                        {
                            DBLanguageSystem dbo = new DBLanguageSystem();
                            dbo.TranslationId = sentence[ID];
                            dbo.Text = sentence[TEXT];
                            dbo.Language = sentence[LANGUAGE];

                            GameServer.Database.AddObject(dbo);
                            RegisterLanguageDataObject(dbo);
                            newEntries++;

                            if (log.IsWarnEnabled)
                                log.Warn("[Language-Manager] Language <" + dbo.Language + "> TranslationId <" + dbo.TranslationId + "> added into the database.");
                        }
                    }
                }

                // Register all DBLanguageSystem rows. Must be done in this way to
                // register ALL database rows. The reason for this is simple:
                //
                // If a user adds new rows into the database without also adding those
                // data into the language files, the above foreach loop just adds the
                // sentences which have been added in the language files.
                foreach (DBLanguageSystem dbo in dbos)
                    RegisterLanguageDataObject(dbo);

                if (newEntries > 0)
                {
                    if (log.IsWarnEnabled)
                        log.Warn("[Language-Manager] Added <" + newEntries + "> new entries into the Database.");
                }

                if (updatedEntries > 0)
                {
                    if (log.IsWarnEnabled)
                        log.Warn("[Language-Manager] Updated <" + updatedEntries + "> entries in Database.");
                }
            }
            else
            {
                foreach (string[] sentence in fileSentences)
                {
                    DBLanguageSystem obj = new DBLanguageSystem();
                    obj.TranslationId = sentence[ID];
                    obj.Text = sentence[TEXT];
                    obj.Language = sentence[LANGUAGE];
                    RegisterLanguageDataObject(obj);
                }
            }

            fileSentences = null;
            #endregion Load system translations

            #region Load object translations
            if (log.IsDebugEnabled)
                log.Info("[Language-Manager] Loading object translations...");

            IList<LanguageDataObject> lngObjs = new List<LanguageDataObject>();
			Util.AddRange(lngObjs, (IList<LanguageDataObject>)GameServer.Database.SelectAllObjects<DBLanguageArea>());
			Util.AddRange(lngObjs, (IList<LanguageDataObject>)GameServer.Database.SelectAllObjects<DBLanguageGameObject>());
			Util.AddRange(lngObjs, (IList<LanguageDataObject>)GameServer.Database.SelectAllObjects<DBLanguageNPC>());
			Util.AddRange(lngObjs, (IList<LanguageDataObject>)GameServer.Database.SelectAllObjects<DBLanguageZone>());

            foreach (LanguageDataObject lngObj in lngObjs)
                RegisterLanguageDataObject(lngObj);

            lngObjs = null;
            #endregion Load object translations
            return true;
        }
        #endregion LoadTranslations

        #region CountLanguageFiles
        /// <summary>
        /// Count files in a language directory
        /// </summary>
        /// <param name="abrev"></param>
        /// <returns></returns>
        private static int CountLanguageFiles(string language)
        {
            int count = 0;
            string langPath = Path.Combine(LangPath, language);

            if (!Directory.Exists(langPath))
                return count;

            foreach (string file in Directory.GetFiles(langPath, "*", SearchOption.AllDirectories))
            {
                if (!file.EndsWith(".txt"))
                    continue;

                count++;
            }

            return count;
        }
        #endregion CountLanguageFiles

        #region ReadLanguageDirectory
        private static ArrayList ReadLanguageDirectory(string path, string language)
        {
		    ArrayList sentences = new ArrayList();
            foreach (string languageFile in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
            {
                if (!languageFile.EndsWith(".txt"))
                    continue;

                string[] lines = File.ReadAllLines(languageFile, Encoding.GetEncoding("utf-8"));
                IList textList = new ArrayList(lines);

                foreach (string line in textList)
                {
                    // do not read comments
                    if (line.StartsWith("#"))
                        continue;

                    // ignore any line that is not formatted  'identifier: sentence'
                    if (line.IndexOf(':') == -1)
                        continue;

                    string[] translation = new string[3];

                    // 0 is the identifier for the sentence
                    translation[ID] = line.Substring(0, line.IndexOf(':'));
                    translation[TEXT] = line.Substring(line.IndexOf(':') + 1);

                    // 1 is the sentence with any tabs (used for readability in language file) removed
                    translation[TEXT] = translation[TEXT].Replace("\t", " ");
                    translation[TEXT] = translation[TEXT].Trim();

                    // 2 is the language of the sentence
                    translation[LANGUAGE] = language;

                    // Ignore duplicates
                    bool ignore = false;
                    foreach (string[] sentence in sentences)
                    {
                        if (sentence[ID] != translation[ID])
                            continue;

                        if (sentence[LANGUAGE] != translation[LANGUAGE])
                            continue;

                        ignore = true;
                        break;
                    }

                    if (ignore)
                        continue;

                    sentences.Add(translation);
                }
            }
            return sentences;
        }
        #endregion ReadLanguageDirectory

        #endregion Initialization

        #region GetLanguageDataObject
        public static LanguageDataObject GetLanguageDataObject(string language, string translationId, LanguageDataObject.eTranslationIdentifier translationIdentifier)
        {
            if (Util.IsEmpty(language) || Util.IsEmpty(translationId))
                return null;

            if (!m_translations.ContainsKey(language))
                return null;

            if (m_translations[language] == null)
            {
                lock (m_translations)
                    m_translations.Remove(language);

                return null;
            }

            if (!m_translations[language].ContainsKey(translationIdentifier))
                return null;

            if (m_translations[language][translationIdentifier] == null)
            {
                lock (m_translations)
                    m_translations[language].Remove(translationIdentifier);

                return null;
            }

            LanguageDataObject result = null;
            foreach (LanguageDataObject colObj in m_translations[language][translationIdentifier])
            {
                if (colObj.TranslationIdentifier != translationIdentifier)
                    continue;

                if (colObj.TranslationId != translationId)
                    continue;

                if (colObj.Language != language)
                    continue;

                result = colObj;
                break;
            }

            return result;
        }
        #endregion GetLanguageDataObject

        #region GetTranslation / TryGetTranslation

        #region GetTranslation
        public static LanguageDataObject GetTranslation(GameClient client, ITranslatableObject obj)
        {
            LanguageDataObject translation;
			TryGetTranslation(out translation, client, obj);
            return translation;
        }
        
        public static LanguageDataObject GetTranslation(GamePlayer player, ITranslatableObject obj)
        {
        	return GetTranslation(player.Client, obj);
        }

        public static LanguageDataObject GetTranslation(string language, ITranslatableObject obj)
        {
            LanguageDataObject translation; 
			TryGetTranslation(out translation, language, obj);
            return translation;
        }

        public static string GetTranslation(GameClient client, string translationId, params object[] args)
        {
            string translation; 
			TryGetTranslation(out translation, client, translationId, args);
            return translation;
        }

        public static string GetTranslation(string language, string translationId, params object[] args)
        {
            string translation; 
			TryGetTranslation(out translation, language, translationId, args);
            return translation;
        }
        #endregion GetTranslation

        #region TryGetTranslation
        public static bool TryGetTranslation(out LanguageDataObject translation, GameClient client, ITranslatableObject obj)
        {
            if (client == null)
            {
                translation = null;
                return false;
            }

            return TryGetTranslation(out translation, (client.Account == null ? String.Empty : client.Account.Language), obj);
        }

        public static bool TryGetTranslation(out LanguageDataObject translation, string language, ITranslatableObject obj)
        {
            if (obj == null)
            {
                translation = null;
                return false;
            }

            if (Util.IsEmpty(language) || language == DefaultLanguage /*Use the objects base data (e.g. NPC.Name)*/)
            {
                translation = null;
                return false;
            }

            translation = GetLanguageDataObject(language, obj.TranslationId, obj.TranslationIdentifier);
            return (translation == null ? false : true);
        }

        public static bool TryGetTranslation(out string translation, GameClient client, string translationId, params object[] args)
        {
            if (client == null)
            {
                translation = TRANSLATION_NULL;
                return true;
            }

            bool result = TryGetTranslation(out translation, (client.Account == null ? DefaultLanguage : client.Account.Language), translationId, args);

            if (client.Account != null)
            {
                if (client.Account.PrivLevel > 1 && client.Player != null && result)
                {
                    if (client.ClientState == GameClient.eClientState.Playing)
                    {
                        bool debug = client.Player.TempProperties.getProperty("LANGUAGEMGR-DEBUG", false);
                        if (debug)
                            translation = ("Id is " + translationId + " " + translation);
                    }
                }
            }

            return result;
        }



		/// <summary>
		/// This returns the last part of the translation text id if actual translation fails
		/// This helps to avoid returning strings that are too long and overflow the client
		/// When the name overflows players my not be targetable or even visible!
		/// PLEASE DO NOT REMOVE THIS FUNCTIONALITY  - tolakram
		/// </summary>
		/// <param name="TranslationID"></param>
		/// <returns></returns>
		public static string GetTranslationErrorText(string lang, string TranslationID)
		{
			try
			{
				if (TranslationID.Contains(".") && TranslationID.TrimEnd().EndsWith(".") == false && TranslationID.StartsWith("'") == false)
				{
					return lang + " " + TranslationID.Substring(TranslationID.LastIndexOf(".") + 1);
				}
				else
				{
					// Odds are a literal string was passed with no translation, so just return the string unmodified
					return TranslationID;
				}
			}
			catch (Exception ex)
			{
				log.Error("Error Getting Translation Error Text for " + lang + ":" + TranslationID, ex);
			}

			return lang + " Translation Error!";
		}
		

        public static bool TryGetTranslation(out string translation, string language, string translationId, params object[] args)
        {
            return soleInstance.TryGetTranslationImpl(out translation, ref language, translationId, ref args);
        }
        #endregion TryGetTranslation

        #endregion GetTranslation / TryGetTranslation

        #region utils

        /// <summary>
        /// Try Translating some Sentence into Player target Language or Default to given String.
        /// </summary>
        /// <param name="player">Targeted player</param>
        /// <param name="missingDefault">Default String if Missing Translation</param>
        /// <param name="translationId">Translation Sentence ID</param>
        /// <param name="args">Translation Sentence Params</param>
        /// <returns>Translated Sentence or Default string.</returns>
        public static string TryTranslateOrDefault(GamePlayer player, string missingDefault, string translationId, params object[] args)
        {
        	string missing = missingDefault;
        	
        	if (args.Length > 0)
        	{
	        	try
	        	{
	        		missing = string.Format(missingDefault, args);
	        	}
	        	catch
	        	{
	        	}
        	}
        	
        	if (player == null || player.Client == null || player.Client.Account == null)
        		return missing;
        	
        	string retval;
        	if (TryGetTranslation(out retval, player.Client.Account.Language, translationId, args))
        	{
        		return retval;
        	}
        	
        	return missing;
        }
        
        #endregion

        #region RegisterLanguageDataObject / UnregisterLanguageDataObject

        #region RegisterLanguageDataObject
        public static bool RegisterLanguageDataObject(LanguageDataObject obj)
        {
            if (obj != null)
            {
                lock (m_translations)
                {
                    if (!m_translations.ContainsKey(obj.Language))
                    {
                        IDictionary<LanguageDataObject.eTranslationIdentifier, IList<LanguageDataObject>> col = new Dictionary<LanguageDataObject.eTranslationIdentifier, IList<LanguageDataObject>>();
                        IList<LanguageDataObject> objs = new List<LanguageDataObject>();
                        objs.Add(obj);
                        col.Add(obj.TranslationIdentifier, objs);
                        m_translations.Add(obj.Language, col);
                        return true;
                    }
                    else if (m_translations[obj.Language] == null)
                    {
                        IDictionary<LanguageDataObject.eTranslationIdentifier, IList<LanguageDataObject>> col = new Dictionary<LanguageDataObject.eTranslationIdentifier, IList<LanguageDataObject>>();
                        IList<LanguageDataObject> objs = new List<LanguageDataObject>();
                        objs.Add(obj);
                        col.Add(obj.TranslationIdentifier, objs);
                        m_translations[obj.Language] = col;
                        return true;
                    }
                    else if (!m_translations[obj.Language].ContainsKey(obj.TranslationIdentifier))
                    {
                        IDictionary<LanguageDataObject.eTranslationIdentifier, IList<LanguageDataObject>> col = new Dictionary<LanguageDataObject.eTranslationIdentifier, IList<LanguageDataObject>>();
                        IList<LanguageDataObject> objs = new List<LanguageDataObject>();
                        objs.Add(obj);
                        m_translations[obj.Language].Add(obj.TranslationIdentifier, objs);
                        return true;
                    }
                    else if (m_translations[obj.Language][obj.TranslationIdentifier] == null)
                    {
                        IList<LanguageDataObject> objs = new List<LanguageDataObject>();
                        objs.Add(obj);
                        m_translations[obj.Language][obj.TranslationIdentifier] = objs;
                    }
                    else if (!m_translations[obj.Language][obj.TranslationIdentifier].Contains(obj))
                    {
                        lock (m_translations[obj.Language][obj.TranslationIdentifier])
                        {
                            if (!m_translations[obj.Language][obj.TranslationIdentifier].Contains(obj))
                            {
                                m_translations[obj.Language][obj.TranslationIdentifier].Add(obj);
                                return true;
                            }
                        }
                    }
                }
            }
            return false; // Object is 'NULL' or already in list.
        }
        #endregion RegisterLanguageDataObject

        #region UnregisterLanguageDataObject
        public static void UnregisterLanguageDataObject(LanguageDataObject obj)
        {
            lock (m_translations)
            {
                if (!m_translations.ContainsKey(obj.Language))
                    return;

                if (m_translations[obj.Language] == null)
                {
                    lock (m_translations)
                        m_translations.Remove(obj.Language);

                    return;
                }

                if (!m_translations[obj.Language].ContainsKey(obj.TranslationIdentifier))
                {
                    if (m_translations[obj.Language].Count < 1)
                    {
                        lock (m_translations)
                            m_translations.Remove(obj.Language);
                    }

                    return;
                }

                if (m_translations[obj.Language][obj.TranslationIdentifier] == null)
                {
                    lock (m_translations)
                        m_translations[obj.Language].Remove(obj.TranslationIdentifier);

                    return;
                }

                if (!m_translations[obj.Language][obj.TranslationIdentifier].Contains(obj))
                {
                    if (m_translations[obj.Language][obj.TranslationIdentifier].Count < 1)
                    {
                        lock (m_translations)
                            m_translations[obj.Language].Remove(obj.TranslationIdentifier);
                    }

                    return;
                }

                lock (m_translations[obj.Language][obj.TranslationIdentifier])
                    m_translations[obj.Language][obj.TranslationIdentifier].Remove(obj);

                if (m_translations[obj.Language][obj.TranslationIdentifier].Count < 1)
                {
                    lock (m_translations)
                        m_translations[obj.Language].Remove(obj.TranslationIdentifier);

                    return;
                }

                if (m_translations[obj.Language].Count < 1)
                {
                    lock (m_translations)
                        m_translations.Remove(obj.Language);
                }
            }
        }
        #endregion UnregisterLanguageDataObject

        #endregion RegisterLanguageDataObject / UnregisterLanguageDataObject
    }
}