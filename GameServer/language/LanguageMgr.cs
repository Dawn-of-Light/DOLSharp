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
using DOL.GS.Quests;
using log4net;

namespace DOL.Language
{
	public class LanguageMgr
    {
        /// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
		/// All the sentences [TranslationID] [Language] = [Sentence]
		/// </summary>
		public static Dictionary<string, Dictionary<string, string>> IDSentences;

		// All the sentence ID's found in any language files
		private static IList<string> m_listSentenceIDsFromFiles;

        private enum eObjColKey
        {
            eNULL,
            eArea,
            eDoor,
            eItem,
            eMission,
            eNPC,
            eQuest,
            eStaticObject,
            eTask,
            eZone
        }

        /// <summary>
        /// Holds all object translations.
        /// </summary>
        private static Dictionary<eObjColKey, Dictionary<string, Dictionary<string, DataObject>>> m_objectTranslations;
        //                                              language      translation id, translation

        /// <summary>
        /// Give a way to change or relocate the lang files
        /// </summary>
        private static string LangPath = Path.Combine(GameServer.Instance.Configuration.RootDirectory, "languages");
        public static void SetLangPath(string path)
		{
			LangPath = path;
		}
		
		/// <summary>
		/// Count files in a language directory
		/// </summary>
		/// <param name="abrev"></param>
		/// <returns></returns>
		private static int CountLanguageFiles(string abrev)
		{
			int count = 0;
			string langPath = Path.Combine(LangPath , abrev );
			
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

		public static bool Refresh(string TranslationID)
		{
			if (!LanguageMgr.IDSentences.ContainsKey(TranslationID)) return false;
			DBLanguage obj = GameServer.Database.SelectObject<DBLanguage>("`TranslationID` = '" + GameServer.Database.Escape(TranslationID) + "'");
			if (obj == null) return false;
			IDSentences[obj.TranslationID]["EN"] = obj.EN;
			IDSentences[obj.TranslationID]["DE"] = obj.DE != null && obj.DE != "" ? obj.DE : obj.EN;
			IDSentences[obj.TranslationID]["FR"] = obj.FR != null && obj.FR != "" ? obj.FR : obj.EN;
			IDSentences[obj.TranslationID]["IT"] = obj.IT != null && obj.IT != "" ? obj.IT : obj.EN;
			IDSentences[obj.TranslationID]["CU"] = obj.CU != null && obj.CU != "" ? obj.CU : obj.EN;
			return true;
		}

		/// <summary>
		/// Load language database and files
		/// </summary>
		/// <param name="lng"></param>
		/// <param name="abrev"></param>
		/// <param name="longname"></param>
		/// <returns></returns>
		private static bool LoadLanguages()
		{
			if (log.IsInfoEnabled)
				log.Info("[LanguageMgr] Loading translations ID...");
			
			if (DOL.GS.ServerProperties.Properties.USE_DBLANGUAGE)
			{
				IList<DBLanguage> objs = GameServer.Database.SelectAllObjects<DBLanguage>();
				foreach (DBLanguage obj in objs)
				{
					if (!IDSentences.ContainsKey(obj.TranslationID))
					{
						IDSentences.Add(obj.TranslationID, new Dictionary<string, string>());
						IDSentences[obj.TranslationID].Add("EN", obj.EN);
						IDSentences[obj.TranslationID].Add("DE", obj.DE != null && obj.DE != "" ? obj.DE : obj.EN);
						IDSentences[obj.TranslationID].Add("FR", obj.FR != null && obj.FR != "" ? obj.FR : obj.EN);
						IDSentences[obj.TranslationID].Add("IT", obj.IT != null && obj.IT != "" ? obj.IT : obj.EN);
						IDSentences[obj.TranslationID].Add("CU", obj.CU != null && obj.CU != "" ? obj.CU : obj.EN);
					}
				}
			}

			// dual usage for the same dictionary makes ones head hurt!
			// In this case we have entries for all the language short and long names
			// [SHORT_NAME or LONG_NAME] [Language Key] = [Short or Long name]

			if (!IDSentences.ContainsKey("SHORT_NAME"))
			{
				IDSentences.Add("SHORT_NAME", new Dictionary<string, string>());
				IDSentences["SHORT_NAME"].Add("EN", "EN");
				IDSentences["SHORT_NAME"].Add("DE", "DE");
				IDSentences["SHORT_NAME"].Add("FR", "FR");
				IDSentences["SHORT_NAME"].Add("IT", "IT");
				IDSentences["SHORT_NAME"].Add("CU", "CU");
			}

			if (!IDSentences.ContainsKey("LONG_NAME"))
			{
				IDSentences.Add("LONG_NAME", new Dictionary<string, string>());
				IDSentences["LONG_NAME"].Add("EN", "English");
				IDSentences["LONG_NAME"].Add("DE", "Deutsch");
				IDSentences["LONG_NAME"].Add("FR", "Français");
				IDSentences["LONG_NAME"].Add("IT", "Italian");
				IDSentences["LONG_NAME"].Add("CU", "Custom");
			}

			m_listSentenceIDsFromFiles = new List<string>();
			foreach (string langShortName in IDSentences["SHORT_NAME"].Keys)
			{
				CheckFromFiles(langShortName);
			}

			if (DOL.GS.ServerProperties.Properties.USE_DBLANGUAGE)
			{
				if (m_listSentenceIDsFromFiles.Count > 0)
				{
					foreach (string id in m_listSentenceIDsFromFiles)
					{
						bool create = false;
						if (IDSentences.ContainsKey(id))
						{
							DBLanguage obj = (DBLanguage)GameServer.Database.SelectObject<DBLanguage>("`TranslationID` = '" + GameServer.Database.Escape(id) + "'");
							if (obj == null)
							{
								obj = new DBLanguage();
								obj.TranslationID = id;
								create = true;
							}
							obj.EN = IDSentences[id]["EN"];
							obj.DE = IDSentences[id]["DE"];
							obj.FR = IDSentences[id]["FR"];
							obj.IT = IDSentences[id]["IT"];
							obj.CU = IDSentences[id]["CU"];
							if (create) GameServer.Database.AddObject(obj);
							else GameServer.Database.SaveObject(obj);
						}

						if (log.IsWarnEnabled)
							log.Warn("[LanguageMgr] TranslationID <" + id + "> " + (create ? "created" : "updated") + " in database!");
					}

					if (log.IsWarnEnabled)
						log.Warn("[LanguageMgr] Loaded " + m_listSentenceIDsFromFiles.Count + " translations ID from files to database!");
				}
			}
			m_listSentenceIDsFromFiles.Clear();

			if (log.IsInfoEnabled)
				log.Info("[LanguageMgr] Translations ID loaded.");
			
			return true;
		}

        #region AddObjectTranslation
        private static void AddObjectTranslation(eObjColKey key, ILanguageTable dbo)
        {
            if (dbo == null || key == eObjColKey.eNULL || Util.IsEmpty(dbo.Language) || Util.IsEmpty(dbo.TranslationId) || !(dbo is DataObject))
                return;

            if (!m_objectTranslations.ContainsKey(key))
            {
                Dictionary<string, Dictionary<string, DataObject>> lngCol = new Dictionary<string, Dictionary<string, DataObject>>();
                Dictionary<string, DataObject> objCol = new Dictionary<string, DataObject>();
                objCol.Add(dbo.TranslationId, (DataObject)dbo);
                lngCol.Add(dbo.Language.ToUpper(), objCol);
                m_objectTranslations.Add(key, lngCol);
                return;
            }

            if (!m_objectTranslations[key].ContainsKey(dbo.Language.ToUpper()))
            {
                Dictionary<string, DataObject> objCol = new Dictionary<string, DataObject>();
                objCol.Add(dbo.TranslationId, (DataObject)dbo);
                m_objectTranslations[key].Add(dbo.Language.ToUpper(), objCol);
                return;
            }

            if (!m_objectTranslations[key][dbo.Language.ToUpper()].ContainsKey(dbo.TranslationId))
            {
                m_objectTranslations[key][dbo.Language.ToUpper()].Add(dbo.TranslationId, (DataObject)dbo);
                return;
            }
        }

        private static void AddObjectTranslation(eObjColKey key, IList<DataObject> dbos)
        {
            if (dbos == null || dbos.Count < 1 || key == eObjColKey.eNULL)
                return;

            foreach (DataObject obj in dbos)
            {
                if (obj.GetType().GetInterface(typeof(ILanguageTable).Name, false) == null)
                    continue;

                AddObjectTranslation(key, (ILanguageTable)obj);
            }
        }
        #endregion AddObjectTranslation

        private static void LoadObjectTranslations()
        {
            List<DataObject> dbos = new List<DataObject>();
            dbos.AddRange(GameServer.Database.SelectAllObjects<DBLanguageArea>());
            AddObjectTranslation(eObjColKey.eArea, dbos);

            dbos.Clear();
            dbos.AddRange(GameServer.Database.SelectAllObjects<DBLanguageGameObject>());
            AddObjectTranslation(eObjColKey.eStaticObject, dbos);

            dbos.Clear();
            dbos.AddRange(GameServer.Database.SelectAllObjects<DBLanguageNPC>());
            AddObjectTranslation(eObjColKey.eNPC, dbos);

            dbos.Clear();
            dbos.AddRange(GameServer.Database.SelectAllObjects<DBLanguageZone>());
            AddObjectTranslation(eObjColKey.eZone, dbos);
        }

        /// <summary>
        /// Translation ID for the sentence, array position 0
        /// </summary>
        private const int ID = 0;

        /// <summary>
        /// The translated sentence, array position 1
        /// </summary>
        private const int TEXT = 1;

		private static void CheckFromFiles(string languageShortName)
		{
			if (CountLanguageFiles(languageShortName) == 0)
			{
				log.Error(languageShortName + " language not found!");
				
				if (DOL.GS.ServerProperties.Properties.SERV_LANGUAGE == languageShortName)
				{
					log.Error("Default " + languageShortName + " language files missing!! Server can't start without!");
					if (GameServer.Instance != null)
						GameServer.Instance.Stop();
					return;
				}
			}

			string langPath = Path.Combine(LangPath , languageShortName );
			foreach (string languageFile in Directory.GetFiles(langPath, "*", SearchOption.AllDirectories))
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

					string[] sentence = new string[2];

					// 0 is the identifier for the sentence
					sentence[ID] = line.Substring(0, line.IndexOf(':'));
					sentence[TEXT] = line.Substring(line.IndexOf(':') + 1);
					
					// 1 is the sentence with any tabs (used for readability in language file) removed
					sentence[TEXT] = sentence[TEXT].Replace("\t", " ");
					sentence[TEXT] = sentence[TEXT].Trim();

					// Makes no sense if DB languages are on.  We read from files anyway, add any new
					// sentences, but never check for changes. New ID's are added to DB but for what reason
					// if everything continues to be based off of files and only files are maintained 
					// I recommend leaving DB support off for now  -- tolakram

					if (IDSentences.ContainsKey(sentence[ID]) == false)
					{
						// This assumes English is first language checked
						IDSentences.Add(sentence[ID], new Dictionary<string, string>());
						IDSentences[sentence[ID]].Add("EN", sentence[TEXT]);
						IDSentences[sentence[ID]].Add("DE", "");
						IDSentences[sentence[ID]].Add("FR", "");
						IDSentences[sentence[ID]].Add("IT", "");
						IDSentences[sentence[ID]].Add("CU", "");

						// make sure this ID is in our list of all ID's from the language files
						if (!m_listSentenceIDsFromFiles.Contains(sentence[ID]))
						{
							m_listSentenceIDsFromFiles.Add(sentence[ID]);
						}
					}
					else if(m_listSentenceIDsFromFiles.Contains(sentence[ID]))
					{
						// else clause with a different check might not be working as intended

						if (!IDSentences[sentence[ID]].ContainsKey(languageShortName))
						{
							// make sure then translation ID exists for the language we are checking
							IDSentences[sentence[ID]].Add(languageShortName, "");
						}

						IDSentences[sentence[ID]][languageShortName] = sentence[TEXT];
					}
				}
			}
		}


		/// <summary>
		/// Initial function
		/// </summary>
		/// <returns></returns>
		public static bool Init()
		{
			IDSentences = new Dictionary<string, Dictionary<string, string>>();
            m_objectTranslations = new Dictionary<eObjColKey, Dictionary<string, Dictionary<string, DataObject>>>();
			
			LoadLanguages();
            LoadObjectTranslations();

			return true;
		}


		/// <summary>
		/// Convert the shortname into enumerator
		/// </summary>
		/// <param name="abrev"></param>
		/// <returns></returns>
		public static string NameToLangs(string abrev)
		{
			if (!IDSentences.ContainsKey("SHORT_NAME")
			    || !IDSentences["SHORT_NAME"].ContainsKey(abrev)) return "EN";

			return IDSentences["SHORT_NAME"][abrev];
		}


		/// <summary>
		/// Convert the enumerator into shortname
		/// </summary>
		/// <param name="l"></param>
		/// <returns></returns>
		public static string LangsToName(string lng)
		{
			if (!IDSentences.ContainsKey("SHORT_NAME")
			    || !IDSentences["SHORT_NAME"].ContainsKey(lng)) return "EN";

			return IDSentences["SHORT_NAME"][lng];
		}


		/// <summary>
		/// Convert the enumerator into longname
		/// </summary>
		/// <param name="c"></param>
		/// <param name="l"></param>
		/// <returns></returns>
		public static string LangsToCompleteName(GameClient client, string lng)
		{
			if (!IDSentences.ContainsKey("LONG_NAME")
			    || !IDSentences["LONG_NAME"].ContainsKey(lng)) return "English";

			return IDSentences["LONG_NAME"][lng];
		}


		/// <summary>
		/// This returns the last part of the translation text id if actual translation fails
		/// This helps to avoid returning strings that are too long and overflow the client
		/// In addition, later version clients seem to reject names with special characters in them
		/// </summary>
		/// <param name="TranslationID"></param>
		/// <returns></returns>
		public static string GetTranslationErrorText(string lang, string TranslationID)
		{
			try
			{
				string str = TranslationID;

				// trying to find entries like "GamePlayer.Title" and not "This is an untranslated sentence"
				if (str.Contains(".") && str.Contains(" ") == false && str.EndsWith(".") == false)
					str = TranslationID.Substring(TranslationID.LastIndexOf(".") + 1);

				if (str.Length > 0)
					return str;
				else
					return lang + " no text found";
			}
			catch
			{
			}

			return lang + " Error";
		}

        /// <summary>
        /// Returns a translation for the given client of the given translatable object.
        /// </summary>
        /// <param name="obj">The object you request a translation for.</param>
        /// <param name="client">The client you want the translation for.</param>
        /// <returns>DataObject or 'null' if nothing was found.</returns>
        public static DataObject GetTranslation(GameClient client, ITranslatableObject obj)
        {

            if (client == null || obj == null)
                return null;

            DataObject result = null;
            if (!Util.IsEmpty(obj.TranslationId))
            {
                eObjColKey key = eObjColKey.eNULL;

                if (obj is AbstractArea)
                    key = eObjColKey.eArea;
                else if (obj is GameStaticItem)
                {
                    if (obj is WorldInventoryItem)
                        return result; // Not supported yet
                    else
                        key = eObjColKey.eStaticObject;
                }
                else if (obj is GameNPC)
                    key = eObjColKey.eNPC;
                else if (obj is Zone)
                    key = eObjColKey.eZone;

                // No more checks, get the result as quick as possible - if exist
                // and all parameters / variables are valid!
                try
                {
                    result = m_objectTranslations[key][client.Account.Language.ToUpper()][obj.TranslationId];
                }
                catch
                {
                }
            }
            return result;
        }

		/// <summary>
		/// Translate the sentence
		/// </summary>
		/// <param name="lang"></param>
		/// <param name="TranslationID"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static string GetTranslation(string lang, string TranslationID, params object [] args)
		{
			// in case args are null, set them to an empty array so we don't catch any null refs
			if(args == null)
			{
				args = new object[0];
			}

			string translated = TranslationID;

			if (IDSentences.ContainsKey(TranslationID) == false)
			{
				return GetTranslationErrorText(lang, translated);
			}

			if (IDSentences[TranslationID].ContainsKey(lang) == false || IDSentences[TranslationID][lang].Length == 0)
			{
				// if language is invalid, not found, or the returned string is empty then give english a try
				lang = "EN";

				if (IDSentences[TranslationID].ContainsKey(lang) == false)
				{
					log.ErrorFormat("LanguageMGR: No default EN entry found for {0}!", TranslationID);
					return GetTranslationErrorText(lang, translated);
				}
			}

			translated = IDSentences[TranslationID][lang];

			if (translated.Length == 0)
			{
				// never allow the return of an empty string
				log.ErrorFormat("LanguageMGR: String empty on translation of {0} to language {1}!", TranslationID, lang);
				return lang + " no text found";
			}

			try
			{
				if (args.Length > 0)
					translated = string.Format(translated, args);
			}
			catch
			{
				log.ErrorFormat("LanguageMGR: Parameter number incorrect: {0} for language {1}, Arg count = {2}, sentence = '{3}', args[0] = '{4}'", TranslationID, lang, args.Length, translated, args.Length > 0 ? args[0] : "null");
			}

			return translated;
		}


		/// <summary>
		/// Translate the sentence
		/// </summary>
		/// <param name="client"></param>
		/// <param name="TranslationID"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static string GetTranslation(GameClient client, string TranslationID, params object[] args)
		{
			if (client == null || client.Account == null)
				return GetTranslation(DOL.GS.ServerProperties.Properties.SERV_LANGUAGE, TranslationID, args);

			if(client.Player != null && client.Account.PrivLevel > 1)
			{
				bool debug = client.Player.TempProperties.getProperty("LANGUAGEMGR-DEBUG", false);
				if (debug && IDSentences.ContainsKey(TranslationID))
					return "[" + TranslationID + "]=<" + IDSentences[TranslationID][client.Account.Language] + ">";
			}

			return GetTranslation(client.Account.Language, TranslationID, args);
		}
	}
}