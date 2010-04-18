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
/*
 * Credits go to: 
 * - Echostorm's Mob Drop Loot System
 * - Roach's modifications to add loottemplate base mobdrops  
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using DOL.Database;
using DOL.AI.Brain;
using DOL.GS.Utils;

namespace DOL.GS
{
	/// <summary>
	/// TemplateLootGenerator
	/// This implementation uses LootTemplates to relate loots to a specific mob type.
	/// Used DB Tables: 
	///				MobxLootTemplate  (Relation between Mob and loottemplate
	///				LootTemplate	(loottemplate containing possible loot items)
	/// </summary>
	public class LootGeneratorTemplate : LootGeneratorBase
	{
		/// <summary>
		/// Map holding a list of ItemTemplateIDs for each TemplateName
		/// 1:n mapping between loottemplateName and loottemplate entries
		/// </summary>
		protected static Dictionary<string, Dictionary<string, LootTemplate>> m_lootTemplates = null;

		/// <summary>
		/// Map holding the corresponding LootTemplateName for each MobName
		/// 1:n Mapping between Mob and LootTemplate
		/// </summary>
		protected static Dictionary<string, List<MobXLootTemplate>> m_mobXLootTemplates = null;

		/// <summary>
		/// Construct a new templategenerate and load its values from database.
		/// </summary>
		public LootGeneratorTemplate()
		{
			PreloadLootTemplates();
		}

		public static void ReloadLootTemplates()
		{
			m_lootTemplates = null;
			m_mobXLootTemplates = null;
			PreloadLootTemplates();
		}

		/// <summary>
		/// Loads the loottemplates
		/// </summary>
		/// <returns></returns>
		protected static bool PreloadLootTemplates()
		{
			if (m_lootTemplates == null)
			{
				m_lootTemplates = new Dictionary<string, Dictionary<string, LootTemplate>>();

				lock (m_lootTemplates)
				{
					IList<LootTemplate> dbLootTemplates = null;

					try
					{
						// TemplateName (typically the mob name), ItemTemplateID, Chance
						dbLootTemplates = GameServer.Database.SelectAllObjects<LootTemplate>();
					}
					catch (Exception e)
					{
						if (log.IsErrorEnabled)
						{
							log.Error("LootGeneratorTemplate: LootTemplates could not be loaded:", e);
						}
						return false;
					}

					if (dbLootTemplates != null)
					{
						Dictionary<string, LootTemplate> loot = null;

						foreach (LootTemplate dbTemplate in dbLootTemplates)
						{
							try
							{
								loot = m_lootTemplates[dbTemplate.TemplateName.ToLower()];
							}
							catch (KeyNotFoundException)
							{
								loot = new Dictionary<string, LootTemplate>();
								m_lootTemplates[dbTemplate.TemplateName.ToLower()] = loot;
							}

							ItemTemplate drop = GameServer.Database.FindObjectByKey<ItemTemplate>(dbTemplate.ItemTemplateID);

							if (drop == null)
							{
								if (log.IsErrorEnabled)
								{
									log.Error("ItemTemplate: " + dbTemplate.ItemTemplateID + " is not found, it is referenced from loottemplate: " + dbTemplate.TemplateName);
								}
							}
							else
							{
								if (!loot.ContainsKey(dbTemplate.ItemTemplateID.ToLower()))
								{
									loot.Add(dbTemplate.ItemTemplateID.ToLower(), dbTemplate);
								}
							}
						}
					}
				}

				log.Info("LootTemplates pre-loaded.");
			}

			if (m_mobXLootTemplates == null)
			{
				m_mobXLootTemplates = new Dictionary<string, List<MobXLootTemplate>>();

				lock (m_mobXLootTemplates)
				{
					IList<MobXLootTemplate> dbMobXLootTemplates = null;

					try
					{
						// MobName, LootTemplateName, DropCount
						dbMobXLootTemplates = GameServer.Database.SelectAllObjects<MobXLootTemplate>();
					}
					catch (Exception e)
					{
						if (log.IsErrorEnabled)
						{
							log.Error("LootGeneratorTemplate: MobXLootTemplates could not be loaded", e);
						}
						return false;
					}

					if (dbMobXLootTemplates != null)
					{
						foreach (MobXLootTemplate dbMobXTemplate in dbMobXLootTemplates)
						{
							if (m_mobXLootTemplates.ContainsKey(dbMobXTemplate.MobName.ToLower()))
							{
								// There can be multiple MobXLootTemplates for a mob, each pointing to a different loot template
								List<MobXLootTemplate> mobxLootTemplates = m_mobXLootTemplates[dbMobXTemplate.MobName.ToLower()];
								mobxLootTemplates.Add(dbMobXTemplate);
							}
							else
							{
								List<MobXLootTemplate> newMobXLootTemplates = new List<MobXLootTemplate>();
								newMobXLootTemplates.Add(dbMobXTemplate);
								m_mobXLootTemplates[dbMobXTemplate.MobName.ToLower()] = newMobXLootTemplates;
							}
						}
					}
				}

				log.Info("MobXLootTemplates pre-loaded.");
			}

			return true;
		}

		/// <summary>
		/// Reload the loot templates for this mob
		/// </summary>
		/// <param name="mob"></param>
		public override void Refresh(GameNPC mob)
		{
			if (mob == null)
				return;

			bool isDefaultLootTemplateRefreshed = false;

			// First see if there are any MobXLootTemplates associated with this mob

			IList<MobXLootTemplate> mxlts = GameServer.Database.SelectObjects<MobXLootTemplate>("MobName = '" + GameServer.Database.Escape(mob.Name) + "'");

			if (mxlts != null)
			{
				lock (m_mobXLootTemplates)
				{
					foreach (MobXLootTemplate mxlt in mxlts)
					{
						if (m_mobXLootTemplates.ContainsKey(mxlt.MobName.ToLower()))
						{
							List<MobXLootTemplate> mobxLootTemplates = m_mobXLootTemplates[mxlt.MobName.ToLower()];
							mobxLootTemplates.Add(mxlt);
						}
						else
						{
							List<MobXLootTemplate> newMobXLootTemplates = new List<MobXLootTemplate>();
							newMobXLootTemplates.Add(mxlt);
							m_mobXLootTemplates[mxlt.MobName.ToLower()] = newMobXLootTemplates;
						}

						RefreshLootTemplate(mxlt.LootTemplateName);


						if (mxlt.LootTemplateName.ToLower() == mob.Name.ToLower())
						{
							isDefaultLootTemplateRefreshed = true;
						}
					}
				}
			}

			// now force a refresh of the mobs default loot template

			if (isDefaultLootTemplateRefreshed == false)
			{
				RefreshLootTemplate(mob.Name);
			}
		}

		protected void RefreshLootTemplate(string templateName)
		{
			lock (m_lootTemplates)
			{
				if (m_lootTemplates.ContainsKey(templateName.ToLower()))
				{
					m_lootTemplates.Remove(templateName.ToLower());
				}
			}

			IList<LootTemplate> lootTemplates = GameServer.Database.SelectObjects<LootTemplate>("TemplateName = '" + GameServer.Database.Escape(templateName) + "'");

			if (lootTemplates != null)
			{
				lock (m_lootTemplates)
				{
					if (m_lootTemplates.ContainsKey(templateName.ToLower()))
					{
						m_lootTemplates.Remove(templateName.ToLower());
					}

					Dictionary<string, LootTemplate> lootList = new Dictionary<string, LootTemplate>();

					foreach (LootTemplate lt in lootTemplates)
					{
						if (lootList.ContainsKey(lt.ItemTemplateID.ToLower()) == false)
						{
							lootList.Add(lt.ItemTemplateID.ToLower(), lt);
						}
					}

					m_lootTemplates.Add(templateName.ToLower(), lootList);
				}
			}
		}

		public override LootList GenerateLoot(GameNPC mob, GameObject killer)
		{
			LootList loot = base.GenerateLoot(mob, killer);

			try
			{
				GamePlayer player = null;

				if (killer is GamePlayer)
				{
					player = killer as GamePlayer;
				}
				else if (killer is GameNPC && (killer as GameNPC).Brain is IControlledBrain)
				{
					player = ((killer as GameNPC).Brain as ControlledNpcBrain).GetPlayerOwner();
				}

				// allow the leader to decide the loot realm
				if (player != null && player.Group != null)
				{
					player = player.Group.Leader;
				}

				if (player != null)
				{
					List<MobXLootTemplate> killedMobXLootTemplates = null;
					if (m_mobXLootTemplates.ContainsKey(mob.Name.ToLower()))
					{
						killedMobXLootTemplates = m_mobXLootTemplates[mob.Name.ToLower()];
					}

					Dictionary<string, LootTemplate> lootTemplatesToDrop = null;

					// MobXLootTemplate contains a loot template name and the max number of drops allowed for that template.
					// We don't need an entry in MobXLootTemplate in order to drop loot, only to control the max number of drops.

					// LootTemplate contains a template name and an itemtemplateid (id_nb).
					// TemplateName usually equals Mob name, so if you want to know what drops for a mob:
					// select * from LootTemplate where templatename = 'mob name';
					// It is possible to have TemplateName != MobName but this works only if there is an entry in MobXLootTemplate for the MobName.

					if (killedMobXLootTemplates == null)
					{
						// If there is no MobXLootTemplate entry then every item in this mobs LootTemplate can drop.
						// In addition, we can use LootTemplate.Count to determine how many of a fixed (100% chance) item can drop
						if (m_lootTemplates.ContainsKey(mob.Name.ToLower()))
						{
							lootTemplatesToDrop = m_lootTemplates[mob.Name.ToLower()];

							if (lootTemplatesToDrop != null)
							{
								foreach (LootTemplate lootTemplate in lootTemplatesToDrop.Values)
								{
									ItemTemplate drop = GameServer.Database.FindObjectByKey<ItemTemplate>(lootTemplate.ItemTemplateID);

									if (drop.Realm == (int)player.Realm || drop.Realm == 0 || ServerProperties.Properties.ALLOW_CROSS_REALM_ITEMS)
									{
										if (lootTemplate.Chance == 100)
										{
											loot.AddFixed(drop, lootTemplate.Count);
										}

										loot.AddRandom(lootTemplate.Chance, drop);
									}
								}
							}
						}
					}
					else
					{
						// MobXLootTemplate exists and tells us the max number of items that can drop.
						// Because we are restricting the max number of items to drop we need to traverse the list
						// and add every 100% chance items to the loots Fixed list and add the rest to the Random list
						// due to the fact that 100% items always drop regardless of the drop limit

						lootTemplatesToDrop = new Dictionary<string, LootTemplate>();
						foreach (MobXLootTemplate mobXLootTemplate in killedMobXLootTemplates)
						{
							loot = GenerateLootFromMobXLootTemplates(mobXLootTemplate, lootTemplatesToDrop, loot, player);

							if (lootTemplatesToDrop != null)
							{
								foreach (LootTemplate lootTemplate in lootTemplatesToDrop.Values)
								{
									ItemTemplate drop = GameServer.Database.FindObjectByKey<ItemTemplate>(lootTemplate.ItemTemplateID);

									if (drop.Realm == (int)player.Realm || drop.Realm == 0 || ServerProperties.Properties.ALLOW_CROSS_REALM_ITEMS)
									{
										loot.AddRandom(lootTemplate.Chance, drop);
									}
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				log.ErrorFormat("Error in LootGeneratorTemplate for mob {0}.  Exception: {1}", mob.Name, ex.Message);
			}

			return loot;
		}

		/// <summary>
		/// Add all loot templates specified in MobXLootTemplate for an entry in LootTemplates
		/// If the item has a 100% drop chance add it as a fixed drop to the loot list.  
		/// </summary>
		/// <param name="mobXLootTemplate">Entry in MobXLootTemplate.</param>
		/// <param name="lootTemplates">List of all itemtemplates this mob can drop and the chance to drop</param>
		/// <param name="lootList">List to hold loot.</param>
		/// <param name="player">Player used to determine realm</param>
		/// <returns>lootList (for readability)</returns>
		private LootList GenerateLootFromMobXLootTemplates(MobXLootTemplate mobXLootTemplates, Dictionary<string, LootTemplate> lootTemplates, LootList lootList, GamePlayer player)
		{
			if (mobXLootTemplates == null || lootTemplates == null || player == null)
				return lootList;

			// Using Name + Realm (if ALLOW_CROSS_REALM_ITEMS) for the key to try and prevent duplicate drops

			Dictionary<string, LootTemplate> templateList = null;

			if (m_lootTemplates.ContainsKey(mobXLootTemplates.LootTemplateName.ToLower()))
			{
				templateList = m_lootTemplates[mobXLootTemplates.LootTemplateName.ToLower()];
			}

			if (templateList != null)
			{
				foreach (LootTemplate lootTemplate in templateList.Values)
				{
					ItemTemplate drop = GameServer.Database.FindObjectByKey<ItemTemplate>(lootTemplate.ItemTemplateID);

					if (drop.Realm == (int)player.Realm || drop.Realm == 0 || ServerProperties.Properties.ALLOW_CROSS_REALM_ITEMS)
					{
						if (lootTemplate.Chance == 100)
						{
							// Added support for specifying drop count in LootTemplate rather than relying on MobXLootTemplate DropCount
							if (lootTemplate.Count > 0)
							{
								lootList.AddFixed(drop, lootTemplate.Count);
							}
							else
							{
								lootList.AddFixed(drop, mobXLootTemplates.DropCount);
							}
						}
						else
						{
							string key = drop.Name.ToLower();

							if (ServerProperties.Properties.ALLOW_CROSS_REALM_ITEMS)
							{
								// make sure the key is unique by adding the realm number
								key += drop.Realm;
							}

							if (!lootTemplates.ContainsKey(key))
							{
								lootList.DropCount = Math.Max(lootList.DropCount, mobXLootTemplates.DropCount);
								lootTemplates.Add(key, lootTemplate);
							}
						}
					}
				}
			}

			return lootList;
		}

	}
}
