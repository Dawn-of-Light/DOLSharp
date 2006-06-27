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
using System.IO;
using System.Reflection;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// LanguageMgr take care about all support of multi language on server
	/// </summary>
	public sealed class LanguageMgr
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static Hashtable m_languageArray;

		/// <summary>
		/// This function load all language file into an hashtable
		/// </summary>
		public static bool Initialize()
		{
			m_languageArray = new Hashtable();

			try
			{
				if (!File.Exists(GameServer.Instance.Configuration.LanguageFile))
				{
					using(File.CreateText(GameServer.Instance.Configuration.LanguageFile))
					{
					}
				}

				ParseLanguageFile(GameServer.Instance.Configuration.LanguageFile);

				string path = GameServer.Instance.Configuration.RootDirectory +Path.DirectorySeparatorChar+ "scripts";
				ParseDirectory(path, "*.lng");
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("Language file exception", e);
			}
			return true;
		}

		/// <summary>
		/// This function get translated string in language file from key
		/// </summary>
		/// <returns>return the string get from language file</returns>
		public static string GetString(string key, string defaultString)
		{
			object res = m_languageArray[key];
			if (res == null)
				return defaultString;
			return (string)res;
		}

		/// <summary>
		/// Parses a directory for all source files
		/// </summary>
		/// <param name="path">The root directory to start the search in</param>
		/// <param name="filter">A filter representing the types of files to search for</param>
		private static void ParseDirectory(string path, string filter)
		{
			if (!Directory.Exists(path))
			{
				throw new Exception(String.Concat("Invalid path: ", path));
			}

			string[] dirs = Directory.GetDirectories(path);
			IEnumerator iter = dirs.GetEnumerator();
			while (iter.MoveNext())
			{
				string str = (string) (iter.Current);
				ParseDirectory(str, filter);
			}
			string[] files = Directory.GetFiles(path, filter);
			if (files.Length > 0)
			{
				foreach (string file in files)
					ParseLanguageFile(file);
			}
		}
		/// <summary>
		/// This function parse the language file to add the translated sentence in hashtable
		/// </summary>
		private static void ParseLanguageFile(string file)
		{
			if (!File.Exists(file))
			{
				throw new Exception(String.Concat("Invalid file: ", file));
			}
			using (StreamReader filereader = File.OpenText(file))
			{
				string line;
				int lineNumber = 1;
				while ((line = filereader.ReadLine()) != null)
				{
					if (line.StartsWith("["))
					{
						int lineindex = line.IndexOf(']');
						if (lineindex < 0)
						{
							if (log.IsWarnEnabled)
								log.Warn("Language file syntax error at line "+lineNumber);
							continue; //maybe throw an exception here
						}
						string key = line.Substring(1, lineindex - 1);

						lineindex = line.IndexOf('=');
						if (lineindex < 0)
						{
							if (log.IsWarnEnabled)
								log.Warn("Language file syntax error at line "+lineNumber);
							continue; //maybe throw an exception here
						}
						string strvalue = line.Substring(lineindex + 1, line.Length - lineindex - 1);
						if (!m_languageArray.ContainsKey(key))
						{
							m_languageArray.Add(key, strvalue);
						}
						else
						{
							if (log.IsWarnEnabled)
								log.Warn("Key ("+key+") defined twice in used language file");
						}
					}
					lineNumber++;
				}
				filereader.Close();
			}
		}
	}
}

//TODO : for moment all is load in the same array to have only one 
//language file but maybe later have multi language server so need 
//more array. but for that will need to parse name of language file