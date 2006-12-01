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
	/// Converts the database format to the version 3
	/// </summary>
	[DatabaseConverter(4)]
	public class Version004 : IDatabaseConverter
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
			log.Info("Database Version 4 Convert Started");

			if (GameServer.Instance.Configuration.DBType == DOL.Database.Connection.ConnectionType.DATABASE_XML)
			{
				log.Info("You have an XML database loaded, this converter will only work with MySQL, skipping");
				return;
			}

			Mob[] mobs = (Mob[])GameServer.Database.SelectObjects(typeof(Mob), "`ClassType` = 'DOL.GS.GameMob'");

			int count = 0;
			foreach (Mob mob in mobs)
			{
				mob.ClassType = "DOL.GS.GameNPC";
				GameServer.Database.SaveObject(mob);
				count++;
			}

			log.Info("Converted " + count + " mobs");

			log.Info("Database Version 4 Convert Finished");
		}
	}
}
