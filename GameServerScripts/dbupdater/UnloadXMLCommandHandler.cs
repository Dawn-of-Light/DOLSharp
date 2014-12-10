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
using System.Reflection;
using System.IO;

using DOL.Database.Attributes;
using DOL.GS.Commands;
using DOL.GS.ServerProperties;

using log4net;

namespace DOL.GS.DatabaseUpdate
{
	/// <summary>
	/// UnloadXMLCommandHandler is used to Fully unload DataBase or DataTable to a Local XML file.
	/// </summary>
	[CmdAttribute(
		"&unloadxmldb",
		ePrivLevel.Admin,
		"Unload your Database Tables to a local XML File Repository.",
		"Usage: /unloadxmldb [FULL|TableName]")]
	public class UnloadXMLCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		#region ServerProperties
		/// <summary>
		/// Set Default Path for Unloading XML Package Directory
		/// </summary>
		[ServerProperty("xmlautoload", "xml_unload_db_directory", "Enforce directory path where the XML Packages are Unloaded From Database (Relative to Scripts or Absolute...)", "dbupdater/unload")]
		public static string XML_UNLOAD_DB_DIRECTORY;
		#endregion

		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Check For UnloadXML Args
		/// </summary>
		/// <param name="client"></param>
		/// <param name="args"></param>
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 2)
			{
				DisplaySyntax(client);
				return;
			}
			
			IList<Type> types = LoaderUnloaderXML.GetAllDataTableTypes();
			
			string argTable = args[1].ToLower();
			
			// Prepare Write Path
			string directory = Path.IsPathRooted(XML_UNLOAD_DB_DIRECTORY) ? XML_UNLOAD_DB_DIRECTORY : string.Format("{0}{1}scripts{1}{2}", GameServer.Instance.Configuration.RootDirectory, Path.DirectorySeparatorChar, XML_UNLOAD_DB_DIRECTORY);

			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);


			switch (argTable)
			{
				case "full":
					foreach (Type table in types)
					{
						string dir = directory;
						Type workingType = table;
						System.Threading.Tasks.Task.Factory.StartNew(() => LoaderUnloaderXML.UnloadXMLTable(workingType, dir));
					}
				break;
				default:
				
					Type findType = types.Where(t => t.Name.ToLower().Equals(argTable)
				                               || t.GetCustomAttributes(typeof(DataTable), false).Cast<DataTable>()
				                               .Where(dt => dt.TableName.ToLower().Equals(argTable)).Any())
						                     .FirstOrDefault();
					if (findType == null)
					{
						DisplaySyntax(client);
						if (log.IsInfoEnabled)
							log.InfoFormat("Could not find table to unload with search string : {0}", argTable);
						return;
					}
					
					System.Threading.Tasks.Task.Factory.StartNew(() => LoaderUnloaderXML.UnloadXMLTable(findType, directory));
				break;
			}
		}
	}
}
