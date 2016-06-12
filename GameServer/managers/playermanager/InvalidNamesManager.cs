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
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;

using DOL.Config;

using log4net;

namespace DOL.GS
{
	/// <summary>
	/// InvalidNamesManager Check for In-Game Player Names/LastNames/GuildName/AllianceName restriction Policy.
	/// </summary>
	public sealed class InvalidNamesManager
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Invalid Names File Path
		/// </summary>
		private string InvalidNamesFile { get; set; }
		
		/// <summary>
		/// Invalid Patterns for Contains Match
		/// </summary>
		private string[] BadNamesContains { get; set; }
		
		/// <summary>
		/// Invalid Patterns for Regex Match
		/// </summary>
		private Regex[] BadNamesRegex { get; set; }
		
		/// <summary>
		/// Create a new Instance of <see cref="InvalidNamesManager"/>
		/// </summary>
		public InvalidNamesManager(string InvalidNamesFile)
		{
			this.InvalidNamesFile = InvalidNamesFile;
			
			BadNamesContains = new string[0];
			BadNamesRegex = new Regex[0];
			LoadInvalidNamesFromFile();
		}
		
		/// <summary>
		/// Check if this string Match any Invalid Pattern
		/// True if the string Match and is Invalid...
		/// </summary>
		public bool this[string match]
		{
			get
			{
				if (string.IsNullOrEmpty(match))
					return true;
				
				return BadNamesContains.Any(pattern => match.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0)
					|| BadNamesRegex.Any(pattern => pattern.IsMatch(match));
			}
		}
		
		/// <summary>
		/// Check if both string Match any Invalid Pattern, then check concatenated string. 
		/// True if the strings Match and are Invalid... 
		/// </summary>
		public bool this[string firstPart, string secondPart]
		{
			get
			{
				return this[firstPart] || this[secondPart]
					|| this[string.Concat(firstPart, secondPart)] || this[string.Concat(firstPart, " ", secondPart)];
			}
		}
		
		/// <summary>
		/// Load Invalid Names Patterns from File
		/// </summary>
		public void LoadInvalidNamesFromFile()
		{
			if (string.IsNullOrEmpty(InvalidNamesFile))
			{
				if (log.IsErrorEnabled)
					log.Error("Invalid Names File Configuration is null, not loading restriction...");
				return;
			}
			
			if (!File.Exists(InvalidNamesFile))
			{
				if (log.IsWarnEnabled)
					log.WarnFormat("Invalid Names File does not exists, trying to create default file: {0}", InvalidNamesFile);
				
				try
				{
					ResourceUtil.ExtractResource("invalidnames.txt", InvalidNamesFile);
				}
				catch (Exception ex)
				{
					if (log.IsErrorEnabled)
						log.Error("Default Invalid Names File could not be created, not loading restriction...", ex);
					return;
				}
			}
			
			try
			{
				using (StreamReader file = File.OpenText(InvalidNamesFile))
				{
					var lines = new List<string>();
					string line = null;
					
					while ((line = file.ReadLine()) != null)
						lines.Add(line);
					
					LoadFromLines(lines);
					
					file.Close();
				}
			}
			catch (Exception ex)
			{
				if (log.IsErrorEnabled)
					log.ErrorFormat("Error while loading Invalid Names File ({0}):\n{1}", InvalidNamesFile, ex);
			}
		}
		
		/// <summary>
		/// Load Restriction Policies from "Lines" Enumerable
		/// </summary>
		/// <param name="Lines">Enumeration of Config String</param>
		public void LoadFromLines(IEnumerable<string> Lines)
		{
			var contains = new List<string>();
			var regex = new List<Regex>();
			
			foreach (var line in Lines)
			{
				if (line.Length == 0 || line[0] == '#')
					continue;
				
				var pattern = line;
				var comment = line.IndexOf('#');
				
				if (comment >= 0)
					pattern = pattern.Substring(0, comment).Trim();
				
				// Regex or Contains Pattern
				if (pattern.StartsWith("/", StringComparison.OrdinalIgnoreCase) && pattern.EndsWith("/", StringComparison.OrdinalIgnoreCase))
					regex.Add(new Regex(pattern.Trim('/'), RegexOptions.IgnoreCase));
				else
					contains.Add(pattern);
			}
			
			BadNamesContains = contains.ToArray();
			BadNamesRegex = regex.ToArray();
		}
	}
}
