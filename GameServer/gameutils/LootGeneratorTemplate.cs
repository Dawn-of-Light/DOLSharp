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
using System.Collections.Specialized;
using System.Reflection;
using DOL.Database;
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
		/// Map holding a list of dbLootTemplates for each templateName
		/// 1:n mapping between loottemplateName and dbloottemplate entries
		/// </summary>
		protected static HybridDictionary m_templateNameXLootTemplate=null;

		/// <summary>
		/// Map holding the corresponding lootTemplateName for each Mob
		/// 1:n Mapping between Mob and LootTemplate
		/// </summary>
		protected static HybridDictionary m_mobXLootTemplates=null;

		// affecting difference between mob and item level where drop is still possible.		
		protected const double LEVEL_RANGE_FACTOR = 2.0;			

		/// <summary>
		/// Constrcut a new templategenerate and load it's values from database.
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
			if (m_templateNameXLootTemplate==null)
			{                
				m_templateNameXLootTemplate = new HybridDictionary(500);
				lock(m_templateNameXLootTemplate)
				{
					// ** find our loot template **
					DataObject[] m_lootTemplates=null;
					try
					{
						m_lootTemplates = GameServer.Database.SelectAllObjects(typeof(DBLootTemplate));
					}
					catch(Exception e)
					{
						if (log.IsErrorEnabled)
							log.Error("TemplateLootGenerator: LootTemplates could not be loaded:", e);
						return false;
					}
 				
					if(m_lootTemplates != null) // did we find a loot template
					{
					
						foreach(DBLootTemplate dbTemplate in m_lootTemplates) 
						{
							IList loot =(IList) m_templateNameXLootTemplate[dbTemplate.TemplateName];
							if (loot==null)
							{
								loot = new ArrayList();
								m_templateNameXLootTemplate[dbTemplate.TemplateName]=loot;
							}
														
							if (dbTemplate.ItemTemplate==null) 
							{
								if (log.IsWarnEnabled)
									log.Warn("No ItemTemplate found for id="+dbTemplate.ItemTemplateID+". Check loottemplat entry with id="+dbTemplate.ObjectId);
								continue;
							}

							loot.Add(dbTemplate);							
						}
					}
				}
			}
			if (m_mobXLootTemplates==null)
			{
				m_mobXLootTemplates = new HybridDictionary(1000);
				lock(m_mobXLootTemplates)
				{
					// ** find our mobs related with loot templates **
					DataObject[] m_mobLootTemplates=null;
					try
					{
						m_mobLootTemplates = GameServer.Database.SelectAllObjects(typeof(DBMobXLootTemplate));
					}
					catch(Exception e)
					{
						if (log.IsErrorEnabled)
							log.Error("TemplateLootGenerator: MobXLootTemplates could not be loaded", e);
						return false;
					}
			 				
					if(m_mobLootTemplates != null) // did we find a loot template
					{
						foreach(DBMobXLootTemplate dbMobXTemplate in m_mobLootTemplates) 
						{
                            if (m_mobXLootTemplates[dbMobXTemplate.MobName] == null)
                            {
                                ArrayList newMobXLootTemplates = new ArrayList();
								newMobXLootTemplates.Add( dbMobXTemplate );
								m_mobXLootTemplates[dbMobXTemplate.MobName] = newMobXLootTemplates;
                            }
                            else
                            {
                                ArrayList mobxLootTemplates = (ArrayList) m_mobXLootTemplates[dbMobXTemplate.MobName];
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
			}
			return true;
		}

		public override LootList GenerateLoot(GameMob mob, GameObject killer)
		{
			LootList loot = base.GenerateLoot(mob, killer);			
			
			ArrayList mobXLootTemplates = (ArrayList) m_mobXLootTemplates[mob.Name];
			//DBMobXLootTemplate[] mobXLootTemplates = ((DBMobXLootTemplate[]) m_mobXLootTemplates[mob.Name]);
			//string lootTemplateName = null;
            IList lootTemplates = null;
            
            //Build list of possible loottemplates...
            if (mobXLootTemplates == null)
            {
                // allow lazy relation between lootTemplate and mob if templateName == mob name.                
                lootTemplates = (IList)m_templateNameXLootTemplate[mob.Name];
            } else
			{
                lootTemplates = new ArrayList();
				foreach (DBMobXLootTemplate mobXLootTemplate in mobXLootTemplates) {                    
				    loot.DropCount=Math.Max(loot.DropCount,mobXLootTemplate.DropCount);

                    IList templateList = (IList)m_templateNameXLootTemplate[mobXLootTemplate.LootTemplateName];
                    if (templateList != null)
                    {
                        ((ArrayList)lootTemplates).AddRange(templateList);
                    }
                }
			}					

			if (lootTemplates!=null)
			{					
				int moblvl = mob.Level - 3;
				int itemlvl, chance, levelDiff;
				double levelBasedFactor =0.0;
				foreach(DBLootTemplate lootTemplate in lootTemplates)
				{
					// formula for chance of adding items to our loot based on item level and mob level **						
					itemlvl = lootTemplate.ItemTemplate.Level;
					chance = lootTemplate.Chance; // add in our chance based on 'Chance' of item
					levelDiff = FastMath.Abs(moblvl - itemlvl);
					if (levelDiff>LEVEL_RANGE_FACTOR) {
						levelBasedFactor = LEVEL_RANGE_FACTOR / (double)levelDiff;
						if (levelBasedFactor<1.0)
							chance =(int) (((double)chance)* levelBasedFactor); // get our chance based on level
					}
					//chance = chance >> 3; // if we are dropping multiple items we want to lower our chance to carry each item

					//Console.WriteLine("Chance for "+lootTemplate.ItemTemplate.Name+" is '"+chance+"/100'");
										
					// we will drop this item
					loot.AddRandom(chance, lootTemplate.ItemTemplate); // add this to our lootlist
				}
			}
			
			return loot;
		}
	}
}
