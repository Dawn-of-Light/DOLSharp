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
using System.Text;
using System.IO;
using System.Collections;
using DOL.GS;
// Log
using System.Reflection;
using log4net;

namespace DOL.Language
{
	public class LanguageMgr
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		// All the sentences, by language
		private static Hashtable[] LangsSentences;
		
		// Optimisation, instead of read all the hastable to find the language, we have a table with ShortName->Langs
		private static Hashtable m_NameToLangs;
		
		public enum Langs : byte 
		{
			EN = 0,
			FR = 1,
			DE = 2,
			ES = 3,
			CZ = 4,
			IT = 5,
			Langs_size
		};

		private static bool LoadLanguage(Langs l, string abrev, string longname)
		{
			string[] lines;
			string FilePath = ".\\Languages\\Language-";
			FilePath += abrev;
			FilePath += ".txt";
			LangsSentences[(int)l] = new Hashtable();
			if (!File.Exists(FilePath))
			{
				log.Error("Language file : " + FilePath + " not found !");
				if (DOL.GS.ServerProperties.Properties.SERV_LANGUAGE == abrev)
				{
					log.Error("Default Language file : " + FilePath + " missing !! Server can't start without !");
					return false;
				}
				return false;
			}
			lines = File.ReadAllLines(FilePath, Encoding.GetEncoding(1252));

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

				splitted[1] = splitted[1].Replace("\t", "");
				log.Debug("Index : " + splitted[0] + " sentence : '" + splitted[1] + "'");
				LangsSentences[(int)l][splitted[0]] = splitted[1];
			}
			// Now we can set the short/long name
			LangsSentences[(int) l]["SHORT_NAME"] = abrev;
			// long name must be in english !
			LangsSentences[(int)l]["LONG_NAME"] = longname;
			m_NameToLangs[abrev] = l;
			return true;
		}
		
		public static bool Init()
		{
			LangsSentences = new Hashtable[(int)Langs.Langs_size];
			m_NameToLangs = new Hashtable();
			
			// Here you can add new languages, just add the Langs too.
			// Full name must be in english !
			LoadLanguage(Langs.EN, "EN", "English");
			LoadLanguage(Langs.FR, "FR", "French");
			LoadLanguage(Langs.DE, "DE", "German");
			LoadLanguage(Langs.ES, "ES", "Spanish");
			LoadLanguage(Langs.CZ, "CZ", "Czech");
			LoadLanguage(Langs.IT, "IT", "Italian");
			
			return true;
		}

		public static Langs NameToLangs(string abrev)
		{
			if (m_NameToLangs[abrev] == null)
				return (Langs) m_NameToLangs["EN"]; // English default
			return (Langs)m_NameToLangs[abrev];
		}

		public static string LangsToName(Langs l)
		{
			return (string)LangsSentences[(int)l]["SHORT_NAME"];
		}

		public static string LangsToCompleteName(GameClient c, Langs l)
		{
			string ID = (string)LangsSentences[(int) l]["LONG_NAME"];
			return GetTranslation(c, "System.LanguagesName." + ID);
		}
		
		public static string GetTranslation(string lang, string TranslationID, params object [] args)
		{
			Langs Language = NameToLangs(lang);
			//log.Debug("Translation requested: " + TranslationID);
			string translated = (string)LangsSentences[(int)Language][TranslationID];
			if (translated == null && lang != DOL.GS.ServerProperties.Properties.SERV_LANGUAGE)
			{
				translated = (string)LangsSentences[(int)NameToLangs(DOL.GS.ServerProperties.Properties.SERV_LANGUAGE)][TranslationID];
				log.Warn("Warning Tanslation ID : " + TranslationID + " doesn't exists in language : " + lang);
			}
			if (translated == null && lang != "EN")
			{
				translated = (string)LangsSentences[(int)NameToLangs("EN")][TranslationID];
				log.Warn("Warning Tanslation ID : " + TranslationID + " doesn't exists in language : English");
			}
			if (translated == null)
			{
				translated = "Error during translation, contact your shard admin with error name : '" + TranslationID + "'";
				log.Error("Error during translation with  ID : " + TranslationID);
			}
			else
			{
				translated = string.Format(translated, args);
			}
			return translated;
		}

		public static string GetTranslation(GameClient client, string TranslationID, params object[] args)
		{
			return GetTranslation(client.Account.Language, TranslationID, args);
		}
	}
}