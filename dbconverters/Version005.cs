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
using log4net;
using DOL.Database;

namespace DOL.GS.DatabaseConverters
{
	/// <summary>
	/// Converts the database format to the version 5
	/// </summary>
	[DatabaseConverter(5)]
	public class Version005 : IDatabaseConverter
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// we need to make use of the new poison fields
		/// </summary>
		public void ConvertDatabase()
		{
			log.Info("Database Version 5 Convert Started");
			log.Info("This fixes some errors with the area classtypes");

			var objs = GameServer.Database.SelectAllObjects<DBArea>();
			int count = 0;
			foreach (DBArea area in objs)
			{
				string orig = area.ClassType;
				if (area.ClassType == "DOL.GS.Area.Circle")
					area.ClassType = "DOL.GS.Area+Circle";
				else if (area.ClassType == "DOL.GS.Area.Square")
					area.ClassType = "DOL.GS.Area+Square";
				else if (area.ClassType == "DOL.GS.Area.BindArea")
					area.ClassType = "DOL.GS.Area+BindArea";
				if (area.ClassType != orig)
				{
					count++;
					GameServer.Database.SaveObject(area);
				}
			}

			log.Info("Converted " + count + " areas");

			log.Info("Database Version 5 Convert Finished");
		}
	}
}
