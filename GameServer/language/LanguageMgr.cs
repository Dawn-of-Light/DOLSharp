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
		public static string[] Langs_str;
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private static Hashtable[] LangsSentences;
		public enum Langs : byte
		{
			EN = 0,
			FR = 1,
			DE = 2,
			Langs_size
		};

		public static string LangsToCompleteName(GameClient client, Langs LangToConvert)
		{
			switch (LangToConvert)
			{
				case Langs.EN: return GetTranslation(client, "System.LanguagesName.English");
				case Langs.FR: return GetTranslation(client, "System.LanguagesName.French");
				case Langs.DE: return GetTranslation(client, "System.LanguagesName.German");
				default: return GetTranslation(client, "System.LanguagesName.English");
			}
		}
		public static string LangsToName(Langs l)
		{
			switch (l)
			{
				case Langs.EN: return "EN";
				case Langs.FR: return "FR";
				case Langs.DE: return "DE";
				default: return "EN";
			}
		}
		public static Langs NameToLangs(string l)
		{
			switch (l)
			{
				case "EN": return Langs.EN;
				case "FR": return Langs.FR;
				case "DE": return Langs.DE;
				default: return Langs.EN;
			}
		}
		public static bool Init()
		{
			string[] lines;
			LangsSentences = new Hashtable[(int)Langs.Langs_size];
			for (int i = 0; i < (int)Langs.Langs_size; i++)
			{
				string FilePath = ".\\Languages\\Language-";
				FilePath += LangsToName((Langs)i);
				FilePath += ".txt";
				LangsSentences[i] = new Hashtable();
				if (!File.Exists(FilePath))
				{
					log.Warn("Language file : " + FilePath + " not found !");
					if (DOL.GS.ServerProperties.Properties.SERV_LANGUAGE == LangsToName((Langs)i))
					{
						log.Error("Default Language file : " + FilePath + " missing !! Server can't start without !");
						return false;
					}
					continue;
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
					LangsSentences[i][splitted[0]] = splitted[1];
				}
			}
			return true;
		}

		public static string GetTranslation(GameClient client, string TranslationID)
		{
			Langs ServerLanguage = NameToLangs(client.Account.Language);

			//log.Debug("Translation requested: " + TranslationID);
			string translated = (string)LangsSentences[(int)ServerLanguage][TranslationID];
			if (translated == null)
			{
				translated = (string)LangsSentences[(int)NameToLangs(DOL.GS.ServerProperties.Properties.SERV_LANGUAGE)][TranslationID];
				log.Warn("Warning Tanslation ID : " + TranslationID + " doesn't exists in language " + client.Account.Language);
			} 
			if (translated == null)
			{
				translated = "Error during translation, contact your shard admin with error name : '" + TranslationID + "'";
				log.Error("Error during translation with  ID : " + TranslationID);
			}
			return translated;
		}
	}
}