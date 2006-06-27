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
using System.Collections;
using DOL.GS.Database;
using NHibernate.Expression;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// LootGeneratorRandom
	/// This implementation uses ItemTemplates to fetch a random item in the range of LEVEL_RANGE to moblevel	
	/// </summary>
	public class LootGeneratorRandom : LootGeneratorBase
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Map holding the corresponding lootTemplateName for each Moblevel group
		/// groups are rounded down to 1-5, 5-10, 10-15, 15-20, 20-25, etc...
		/// 1:n Mapping between Moblevel and LootTemplate
		/// </summary>
		protected static GenericItemTemplate[][] m_itemTemplates=null;

		protected const int LEVEL_RANGE = 5; // 
		protected const int LEVEL_SIZE = 10; // 10*LEVEL_RANGE = up to level 50

		/// <summary>
		/// Constrcut a new templategenerate and load it's values from database.
		/// </summary>
		public LootGeneratorRandom()
		{
			PreloadItemTemplates();
		}

		/// <summary>
		/// Loads the loottemplates
		/// </summary>
		/// <returns></returns>
		protected static bool PreloadItemTemplates()
		{
			if (m_itemTemplates==null)
			{
				m_itemTemplates = new GenericItemTemplate[LEVEL_SIZE+1][];

				lock(m_itemTemplates.SyncRoot)
				{
					// ** find our loot template **
					IList itemTemplates=null;
					for (int i=0;i <= LEVEL_SIZE; i++) 
					{
						try
						{
							itemTemplates = GameServer.Database.SelectObjects(typeof(GenericItemTemplate), Expression.And(Expression.Ge("Level", i*LEVEL_RANGE), Expression.Le("Level", (i+1)*LEVEL_RANGE)));
						}
						catch(Exception e)
						{
							if (log.IsErrorEnabled)
								log.Error("LootGeneratorRandom: ItemTemplates could not be loaded", e);
							return false;
						}

						if(itemTemplates != null) // did we find a loot template
						{
							m_itemTemplates[i] = (GenericItemTemplate[]) itemTemplates;
						}
					}
				}
			}
			return true;
		}

		public override LootList GenerateLoot(GameMob mob, GameObject killer)
		{
			LootList loot = base.GenerateLoot(mob, killer);

			if (Util.Chance(25)) 
			{
				GenericItemTemplate[] itemTemplates = m_itemTemplates[Math.Min(m_itemTemplates.Length-1, mob.Level/LEVEL_RANGE)];
				if (itemTemplates!=null && itemTemplates.Length>0)
				{
					loot.AddRandom(100, itemTemplates[Util.Random(itemTemplates.Length-1)]);
				}
			}

			return loot;
		}
	}
}
