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
using System.Collections;
using System.Reflection;
using System.Text;
using System.IO;

using DOL.GS;
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
		/// All the sentences, by language
		/// </summary>
		private static Hashtable[] LangsSentences;
		

		/// <summary>
		/// Optimisation, instead of read all the hastable to find the language, we have a table with ShortName->Langs
		/// </summary>
		private static Hashtable m_NameToLangs = new Hashtable();
		

		/// <summary>
		/// All language enumerator
		/// </summary>
		public enum Langs : byte 
		{
			EN,
			IT,
			DE,
			FR,
			CZ,
			ES,
			Langs_size
		};


		/// <summary>
		/// Count files in a language directory
		/// </summary>
		/// <param name="abrev"></param>
		/// <returns></returns>
		private static int CountLanguageFiles(string abrev)
		{
			int count = 0;
			string LangPath = "." + Path.DirectorySeparatorChar + "languages" + Path.DirectorySeparatorChar + abrev + Path.DirectorySeparatorChar;
			
			if (!Directory.Exists(LangPath))
				return count;

			foreach (string file in Directory.GetFiles(LangPath, "*", SearchOption.AllDirectories))
			{
				if (!file.EndsWith(".txt"))
					continue;
				count++;
			}

			return count;
		}


		/// <summary>
		/// Load language files
		/// </summary>
		/// <param name="lng"></param>
		/// <param name="abrev"></param>
		/// <param name="longname"></param>
		/// <returns></returns>
		private static bool LoadLanguage(Langs lng, string abrev, string longname)
		{
			LangsSentences[(int)lng] = new Hashtable();

			if (CountLanguageFiles(abrev) == 0)
			{
				log.Error(longname + " language not found!");
				if (DOL.GS.ServerProperties.Properties.SERV_LANGUAGE == abrev)
				{
					log.Error("Default " + longname + " language files missing!! Server can't start without!");
					if (GameServer.Instance != null)
						GameServer.Instance.Stop();
				}
				return false;
			}

			string LangPath = "." + Path.DirectorySeparatorChar + "languages" + Path.DirectorySeparatorChar + abrev + Path.DirectorySeparatorChar;
			foreach (string FilePath in Directory.GetFiles(LangPath, "*", SearchOption.AllDirectories))
			{
				if (!FilePath.EndsWith(".txt"))
					continue;

				string[] lines = File.ReadAllLines(FilePath, Encoding.GetEncoding("utf-8"));
				IList textList = new ArrayList(lines);
				
				foreach (string line in textList)
				{
					if (line.StartsWith("#"))
						continue;

					if (line.IndexOf(':') == -1)
						continue;

					string[] splitted = new string[2];

					splitted[0] = line.Substring(0, line.IndexOf(':'));
					splitted[1] = line.Substring(line.IndexOf(':') + 1);
					
					splitted[1] = splitted[1].Replace("\t", " ");
					splitted[1] = splitted[1].Trim();

					LangsSentences[(int)lng][splitted[0]] = splitted[1];
				}
			}
			
			LangsSentences[(int)lng]["SHORT_NAME"] = abrev;
			LangsSentences[(int)lng]["LONG_NAME"] = longname;
			m_NameToLangs[abrev] = lng;
			
			return true;
		}


		/// <summary>
		/// Initial function
		/// </summary>
		/// <returns></returns>
		public static bool Init()
		{
			LangsSentences = new Hashtable[(int)Langs.Langs_size];
			
			LoadLanguage(Langs.EN, "EN", "English");
			LoadLanguage(Langs.IT, "IT", "Italian");
			LoadLanguage(Langs.FR, "FR", "French");	
			LoadLanguage(Langs.DE, "DE", "German");
			LoadLanguage(Langs.ES, "ES", "Spanish");
			LoadLanguage(Langs.CZ, "CZ", "Czech");
					
			return true;
		}


		/// <summary>
		/// Convert the shortname into enumerator
		/// </summary>
		/// <param name="abrev"></param>
		/// <returns></returns>
		public static Langs NameToLangs(string abrev)
		{
			if (m_NameToLangs[abrev] == null)
				return (Langs)m_NameToLangs["EN"];

			return (Langs)m_NameToLangs[abrev];
		}


		/// <summary>
		/// Convert the enumerator into shortname
		/// </summary>
		/// <param name="l"></param>
		/// <returns></returns>
		public static string LangsToName(Langs lng)
		{
			return (string)LangsSentences[(int)lng]["SHORT_NAME"];
		}


		/// <summary>
		/// Convert the enumerator into longname
		/// </summary>
		/// <param name="c"></param>
		/// <param name="l"></param>
		/// <returns></returns>
		public static string LangsToCompleteName(GameClient client, Langs lng)
		{
			string ID = (string)LangsSentences[(int)lng]["LONG_NAME"];
			return GetTranslation(client, "System.LanguagesName." + ID);
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
			Langs Language = NameToLangs(lang);

			string translated = (string)LangsSentences[(int)Language][TranslationID];

			if ((translated == null) && (lang != DOL.GS.ServerProperties.Properties.SERV_LANGUAGE))
			{
				translated = (string)LangsSentences[(int)NameToLangs(DOL.GS.ServerProperties.Properties.SERV_LANGUAGE)][TranslationID];
				//log.Warn("Tanslation ID: \"" + TranslationID + "\" doesn't exists in " + lang + " language.");
			}

			if ((translated == null) && (lang != "EN") && (DOL.GS.ServerProperties.Properties.SERV_LANGUAGE != "EN"))
			{
				translated = (string)LangsSentences[(int)NameToLangs("EN")][TranslationID];
				log.Warn("Tanslation ID: \"" + TranslationID + "\" doesn't exists in English language.");
			}

			if (translated == null)
			{
				translated = TranslationID;
				//log.Warn("Error during translation with ID: " + TranslationID);
			}
			else
			{
				try
				{
					if (args.Length > 0)
						translated = string.Format(translated, args);
				}
				catch
				{
					log.Warn("Parameters number error, ID: " + TranslationID + " (Arg count=" + args.Length + ").");
				}
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
            if (client == null)
                return GetTranslation(DOL.GS.ServerProperties.Properties.SERV_LANGUAGE, TranslationID, args);
			
			return GetTranslation(client.Account.Language, TranslationID, args);
		}
	}
}