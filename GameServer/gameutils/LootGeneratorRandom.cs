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
using DOL.Database;

namespace DOL.GS
{
	/// <summary>
	/// LootGeneratorRandom
	/// This implementation uses ItemTemplates to fetch a random item in the range of LEVEL_RANGE to moblevel   
	/// </summary>
	public class LootGeneratorRandom : LootGeneratorBase
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Map holding the corresponding lootTemplateName for each Moblevel group
		/// groups are rounded down to 1-5, 5-10, 10-15, 15-20, 20-25, etc...
		/// 1:n Mapping between Moblevel and LootTemplate
		/// </summary>

		protected static ItemTemplate[][] m_itemTemplatesAlb = new ItemTemplate[LEVEL_SIZE + 1][];
		protected static ItemTemplate[][] m_itemTemplatesMid = new ItemTemplate[LEVEL_SIZE + 1][];
		protected static ItemTemplate[][] m_itemTemplatesHib = new ItemTemplate[LEVEL_SIZE + 1][];

		protected const int LEVEL_RANGE = 5; // 
		protected const int LEVEL_SIZE = 10; // 10*LEVEL_RANGE = up to level 50

		public LootGeneratorRandom() : base()
		{ }

		/// <summary>
		/// Loads the loottemplates
		/// </summary>
		static LootGeneratorRandom()
		{
			// ** find our loot template **
			IList<ItemTemplate> itemTemplates = null;

			for (int i = 0; i <= LEVEL_SIZE; i++)
			{
				try
				{
					itemTemplates = GameServer.Database.SelectObjects<ItemTemplate>("`Level` >= @LevelMin AND `Level` <= @LevelMax AND `IsPickable` = @IsPickable AND `IsDropable` = @IsDropable AND `CanDropAsloot` = @CanDropAsloot AND `Item_Type` >= @MinSlot AND `Item_Type` <= @MaxSlot",
							                                                        new[] { new QueryParameter("@LevelMin", (i * LEVEL_RANGE)),
							                                                            new QueryParameter("@LevelMax", ((i + 1) * LEVEL_RANGE)),
							                                                            new QueryParameter("@IsPickable", 1),
							                                                            new QueryParameter("@IsDropable", 1),
																						new QueryParameter("@CanDropAsloot", 1),
																						new QueryParameter("@MinSlot", (int)eInventorySlot.MinEquipable),
																						new QueryParameter("@MaxSlot", (int)eInventorySlot.MaxEquipable) });
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error("LootGeneratorRandom: ItemTemplates could not be loaded", e);
					return;
				}

				if (itemTemplates != null) // did we find a loot template
				{
					List<ItemTemplate> templatesAlb = new List<ItemTemplate>();
					List<ItemTemplate> templatesHib = new List<ItemTemplate>();
					List<ItemTemplate> templatesMid = new List<ItemTemplate>();

					foreach (ItemTemplate itemTemplate in itemTemplates)
					{
						switch (itemTemplate.Realm)
						{
							case (int)eRealm.Albion:
								templatesAlb.Add(itemTemplate);
								break;
							case (int)eRealm.Hibernia:
								templatesHib.Add(itemTemplate);
								break;
							case (int)eRealm.Midgard:
								templatesMid.Add(itemTemplate);
								break;
							default:
									templatesAlb.Add(itemTemplate);
									templatesHib.Add(itemTemplate);
									templatesMid.Add(itemTemplate);
									break;
						}
					}

					m_itemTemplatesAlb[i] = templatesAlb.ToArray();
					m_itemTemplatesHib[i] = templatesHib.ToArray();
					m_itemTemplatesMid[i] = templatesMid.ToArray();
				}
			} // for
		}

		public override LootList GenerateLoot(GameNPC mob, GameObject killer)
		{
			LootList loot = base.GenerateLoot(mob, killer);

			if (Util.Chance(10))
			{
				ItemTemplate[] itemTemplates = null;

				eRealm realm = mob.CurrentZone.Realm;

				if (realm < eRealm._FirstPlayerRealm || realm > eRealm._LastPlayerRealm)
					realm = (eRealm)Util.Random((int)eRealm._FirstPlayerRealm, (int)eRealm._LastPlayerRealm);

				switch (realm)
				{
					case eRealm.Albion:
						{
							int index = Math.Min(m_itemTemplatesAlb.Length - 1, mob.Level / LEVEL_RANGE);
							itemTemplates = m_itemTemplatesAlb[index];
						}
						break;
					case eRealm.Hibernia:
						{
							int index = Math.Min(m_itemTemplatesHib.Length - 1, mob.Level / LEVEL_RANGE);
							itemTemplates = m_itemTemplatesHib[index];
							break;
						}
					case eRealm.Midgard:
						{
							int index = Math.Min(m_itemTemplatesHib.Length - 1, mob.Level / LEVEL_RANGE);
							itemTemplates = m_itemTemplatesMid[index];
							break;
						}
				}

				if (itemTemplates != null && itemTemplates.Length > 0)
				{
					ItemTemplate itemTemplate = itemTemplates[Util.Random(itemTemplates.Length - 1)];
					loot.AddFixed(itemTemplate,1);
				}
			}

			return loot;
		}
	}
}