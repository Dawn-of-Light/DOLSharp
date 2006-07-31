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
using log4net;
using DOL.Database;

namespace DOL.GS.DatabaseConverters
{
	/// <summary>
	/// Converts the database format to the version 2
	/// </summary>
	[DatabaseConverter(2)]
	public class Version002 : IDatabaseConverter
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// style icon field added this should copy the ID value
		/// realm 6 should be peace flag and realm changed
		/// </summary>
		public void ConvertDatabase()
		{
			log.Info("Database Version 2 Convert Started");

			log.Info("Converting Styles");
			DBStyle[] styles = (DBStyle[])GameServer.Database.SelectAllObjects(typeof(DBStyle));
			foreach (DBStyle style in styles)
			{
				style.Icon = style.ID;

				GameServer.Database.SaveObject(style);
			}
			log.Info(styles.Length + " Styles Processed");

			log.Info("Converting Mobs");
			Mob[] mobs = (Mob[])GameServer.Database.SelectAllObjects(typeof(Mob));
			foreach (Mob mob in mobs)
			{
				if (mob.Realm == 6)
				{
					if ((mob.Flags & (uint)GameNPC.eFlags.PEACE) == 0)
					{
						mob.Flags ^= (uint)GameNPC.eFlags.PEACE;
					}

					Region region = WorldMgr.GetRegion(mob.Region);
					if (region != null)
					{
						Zone zone = region.GetZone(mob.X, mob.Y);
						if (zone != null)
						{
							mob.Realm = (byte)zone.GetRealm();
						}
					}

					GameServer.Database.SaveObject(mob);
				}
			}
			log.Info(mobs.Length + " Mobs Processed");

			log.Info("Database Version 2 Convert Finished");
		}
	}
}
