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
using System.Reflection;
using DOL.Database;
using DOL.AI.Brain;
using DOL.GS.Utils;
using log4net;

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
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Map holding a list of ItemTemplateIDs for each TemplateName
		/// 1:n mapping between loottemplateName and dbloottemplate entries
		/// </summary>
		protected static Dictionary<string, Dictionary<string, DBLootTemplate>> m_LootTemplates = null;

		/// <summary>
		/// Map holding the corresponding LootTemplateName for each MobName
		/// 1:n Mapping between Mob and LootTemplate
		/// </summary>
		protected static HybridDictionary m_mobXLootTemplates = null;

		/// <summary>
		/// Construct a new templategenerate and load its values from database.
		/// </summary>
		public LootGeneratorTemplate()
		{
			PreloadLootTemplates();
		}

		/// <summary>
		/// Loads the loottemplates
		/// </summary>
		/// <returns></returns>
		protected static bool PreloadLootTemplates()
		{
			if (m_LootTemplates == null)
			{
				m_LootTemplates = new Dictionary<string, Dictionary<string, DBLootTemplate>>();

				lock (m_LootTemplates)
				{
					DataObject[] dbLootTemplates = null;

					try
					{
						// TemplateName (typically the mob name), ItemTemplateID, Chance
						dbLootTemplates = GameServer.Database.SelectAllObjects(typeof(DBLootTemplate));
					}
					catch (Exception e)
					{
						if (log.IsErrorEnabled)
							log.Error("TemplateLootGenerator: LootTemplates could not be loaded:", e);
						return false;
					}

					if (dbLootTemplates != null)
					{
						Dictionary<string, DBLootTemplate> loot = null;

						foreach (DBLootTemplate dbTemplate in dbLootTemplates)
						{
							try
							{
								loot = m_LootTemplates[dbTemplate.TemplateName.ToLower()];
							}
							catch (KeyNotFoundException)
							{
								loot = new Dictionary<string, DBLootTemplate>();
								m_LootTemplates[dbTemplate.TemplateName.ToLower()] = loot;
							}

							if (dbTemplate.ItemTemplate == null)
							{
								if (log.IsErrorEnabled)
									log.Error("ItemTemplate: " + dbTemplate.ItemTemplateID + " is not found, it is referenced from loottemplate_id: " + dbTemplate.ObjectId);
							}
							else
							{
								if (!loot.ContainsKey(dbTemplate.ItemTemplateID.ToLower()))
									loot.Add(dbTemplate.ItemTemplateID.ToLower(), dbTemplate);
							}
						}
					}
				}

				log.Info("LootTemplates pre-loaded.");
			}

			if (m_mobXLootTemplates == null)
			{
				m_mobXLootTemplates = new HybridDictionary(1000);

				lock (m_mobXLootTemplates)
				{
					DataObject[] dbMobLootTemplates = null;

					try
					{
						dbMobLootTemplates = GameServer.Database.SelectAllObjects(typeof(DBMobXLootTemplate));
					}
					catch (Exception e)
					{
						if (log.IsErrorEnabled)
							log.Error("TemplateLootGenerator: MobXLootTemplates could not be loaded", e);
						return false;
					}

					if (dbMobLootTemplates != null)
					{
						ArrayList newMobXLootTemplates;

						foreach (DBMobXLootTemplate dbMobXTemplate in dbMobLootTemplates)
						{
							if (m_mobXLootTemplates[dbMobXTemplate.MobName.ToLower()] == null)
							{
								newMobXLootTemplates = new ArrayList();
								newMobXLootTemplates.Add(dbMobXTemplate);
								m_mobXLootTemplates[dbMobXTemplate.MobName.ToLower()] = newMobXLootTemplates;
							}
							else
							{
								// ~~~ Kakuri Nov 29, 2008 ~~~
								// This whole 'else' clause looks not only broken (doesn't do anything), but pointless -
								// since each MobName has only ONE corresponding LootTemplateName, this 'else' clause
								// will never even be executed
								// ~~~ /Kakuri ~~~
								ArrayList mobxLootTemplates = (ArrayList)m_mobXLootTemplates[dbMobXTemplate.MobName.ToLower()];
								mobxLootTemplates.Add(dbMobXTemplate);

								//Array.Resize IS ONLY AVAILABLE IN .NET 2.0 ... which we don't use currently because
								//it is BETA!
								//DBMobXLootTemplate[] mobxLootTemplates =(DBMobXLootTemplate[]) m_mobXLootTemplates[dbMobXTemplate.MobName];
								//Array.Resize(ref mobxLootTemplates, mobxLootTemplates.Length + 1);
								//mobxLootTemplates[mobxLootTemplates.Length - 1] = dbMobXTemplate;
							}
						}
					}
				}

				log.Info("MobXLootTemplates pre-loaded.");
			}

			return true;
		}

		/// <summary>
		/// Reloads the specified loot template from the DB
		/// </summary>
		/// <param name="templateName"></param>
		public void RefreshLootTemplate(string templateName)
		{
			Dictionary<string, DBLootTemplate> loot = null;
			DataObject[] dbLootTemplates = GameServer.Database.SelectObjects(typeof(DBLootTemplate), "TemplateName = '" + templateName + "'");

			if (dbLootTemplates != null)
			{
				if (m_LootTemplates.ContainsKey(templateName.ToLower()))
					m_LootTemplates.Remove(templateName.ToLower());

				loot = new Dictionary<string, DBLootTemplate>();
				m_LootTemplates[templateName.ToLower()] = loot;

				foreach (DBLootTemplate lt in dbLootTemplates)
				{
					if (!loot.ContainsKey(lt.ItemTemplateID.ToLower()))
						loot.Add(lt.ItemTemplateID.ToLower(), lt);
				}
			}

			log.Debug("LootTemplate " + templateName + " refreshed.");
		}

		public override LootList GenerateLoot(GameNPC mob, GameObject killer)
		{
			LootList loot = base.GenerateLoot(mob, killer);
			ArrayList mobXLootTemplates = (ArrayList)m_mobXLootTemplates[mob.Name.ToLower()];
			Dictionary<string, DBLootTemplate> lootTemplates = null;

			// Create a list with all loot templates for this mob.

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
				if (mobXLootTemplates == null)
				{
					// allow lazy relation between lootTemplate and mob if templateName == mob name.                
					if (m_LootTemplates.ContainsKey(mob.Name.ToLower()))
						lootTemplates = m_LootTemplates[mob.Name.ToLower()];
				}
				else
				{
					// Look up all possible drops for each entry in MobXLoot.

					lootTemplates = new Dictionary<string, DBLootTemplate>();
					foreach (DBMobXLootTemplate mobXLootTemplate in mobXLootTemplates)
						GenerateLootFromTemplate(mobXLootTemplate, lootTemplates, loot, player);
				}

				// Add random drops to loot list.
				// Here we decide if the item can drop for the player - Tolakram

				if (lootTemplates != null)
				{
					foreach (DBLootTemplate lootTemplate in lootTemplates.Values)
					{
						if (lootTemplate.ItemTemplate.Realm == (int)player.Realm ||
							lootTemplate.ItemTemplate.Realm == 0 ||
							ServerProperties.Properties.ALLOW_CROSS_REALM_ITEMS)
						{
							loot.AddRandom(lootTemplate.Chance, lootTemplate.ItemTemplate);
						}
					}
				}
			}

			return loot;
		}

		/// <summary>
		/// Add all loot templates for an entry in MobXLoot to the list of templates;
		/// if the item has a 100% drop chance, add it as a fixed drop to the
		/// loot list.  
		/// </summary>
		/// <param name="mobXLootTemplate">Entry in MobXLoot.</param>
		/// <param name="lootTemplates">List of templates for random drops.</param>
		/// <param name="loot">List to hold loot.</param>
		private void GenerateLootFromTemplate(DBMobXLootTemplate mobXLootTemplate, Dictionary<string, DBLootTemplate> lootTemplates, LootList loot, GamePlayer player)
		{
			if (mobXLootTemplate == null || player == null)
				return;

			// Using Name + Realm (if ALLOW_CROSS_REALM_ITEMS) for the key to try and prevent duplicate drops

			Dictionary<string, DBLootTemplate> templateList = null;

			if (m_LootTemplates.ContainsKey(mobXLootTemplate.LootTemplateName.ToLower()))
				templateList = m_LootTemplates[mobXLootTemplate.LootTemplateName.ToLower()];

			if (templateList != null)
			{
				foreach (DBLootTemplate lootTemplate in templateList.Values)
				{
					if (lootTemplate.ItemTemplate.Realm == (int)player.Realm ||
						lootTemplate.ItemTemplate.Realm == 0 ||
						ServerProperties.Properties.ALLOW_CROSS_REALM_ITEMS)
					{
						if (lootTemplate.Chance == 100)
						{
							loot.AddFixed(lootTemplate.ItemTemplate, mobXLootTemplate.DropCount);
						}
						else
						{
							string key = lootTemplate.ItemTemplate.Name.ToLower();

							if (ServerProperties.Properties.ALLOW_CROSS_REALM_ITEMS)
								key += lootTemplate.ItemTemplate.Realm;

							if (!lootTemplates.ContainsKey(key))
							{
								loot.DropCount = Math.Max(loot.DropCount, mobXLootTemplate.DropCount);
								lootTemplates.Add(key, lootTemplate);
							}
						}
					}
				}
			}
		}
	}
}
