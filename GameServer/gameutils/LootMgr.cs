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
using System.Collections.Specialized;
using System.Reflection;
using DOL.Database;
using DOL.GS.Scripts;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// the LootMgr holds pointers to all LootGenerators at 
	/// associates the correct LootGenerator with a given Mob
	/// </summary>
	public sealed class LootMgr
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Map holding one generator for each different class, to reuse similar generators...
		/// </summary>
		static readonly HybridDictionary m_ClassGenerators = new HybridDictionary();

		/// <summary>
		/// List of global Lootgenerators 
		/// </summary>
		static readonly IList m_globalGenerators = new ArrayList();

		/// <summary>
		/// List of Lootgenerators related by mobname
		/// </summary>
		static readonly HybridDictionary m_mobNameGenerators = new HybridDictionary();

		/// <summary>
		/// List of Lootgenerators related by mobguild
		/// </summary>
		static readonly HybridDictionary m_mobGuildGenerators = new HybridDictionary();

		/// <summary>
		/// List of Lootgenerators related by region ID
		/// </summary>
		static readonly HybridDictionary m_mobRegionGenerators = new HybridDictionary();

		/// <summary>
		/// List of Lootgenerators related by mobfaction
		/// </summary>
		//static readonly HybridDictionary m_mobFactionGenerators = new HybridDictionary();		

		/// <summary>
		/// Initializes the LootMgr. This function must be called
		/// before the LootMgr can be used!
		/// </summary>
		public static bool Init()
		{
			if (log.IsInfoEnabled)
				log.Info("Loading LootGenerators...");

			DataObject[] m_lootGenerators;
			try
			{
				m_lootGenerators = GameServer.Database.SelectAllObjects(typeof(DBLootGenerator));
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("LootMgr: LootGenerators could not be loaded", e);
				return false;
			}

			if (m_lootGenerators != null) // did we find any loot generators
			{
				foreach (DBLootGenerator dbGenerator in m_lootGenerators)
				{
					ILootGenerator generator = GetGeneratorInCache(dbGenerator);
					if (generator == null)
					{
						Type generatorType = null;
						foreach (Assembly asm in ScriptMgr.Scripts)
						{
							generatorType = asm.GetType(dbGenerator.LootGeneratorClass);
							if (generatorType != null)
								break;
						}
						if (generatorType == null)
						{
							generatorType = Assembly.GetAssembly(typeof(GameServer)).GetType(dbGenerator.LootGeneratorClass);
						}

						if (generatorType == null)
						{
							if (log.IsErrorEnabled)
								log.Error("Could not find LootGenerator: " + dbGenerator.LootGeneratorClass + "!!!");
							continue;
						}
						generator = (ILootGenerator)Activator.CreateInstance(generatorType);

						PutGeneratorInCache(dbGenerator, generator);
					}
					RegisterLootGenerator(generator, dbGenerator.MobName, dbGenerator.MobGuild, dbGenerator.MobFaction, dbGenerator.RegionID);
				}
			}
			if (log.IsDebugEnabled)
			{
				log.Debug("Found " + m_globalGenerators.Count + " Global LootGenerators");
				log.Debug("Found " + m_mobNameGenerators.Count + " Mobnames registered by LootGenerators");
				log.Debug("Found " + m_mobGuildGenerators.Count + " Guildnames registered by LootGenerators");
			}

			// no loot generators loaded...
			if (m_globalGenerators.Count == 0 && m_mobNameGenerators.Count == 0 && m_globalGenerators.Count == 0)
			{
				ILootGenerator baseGenerator = new LootGeneratorMoney();
				RegisterLootGenerator(baseGenerator, null, null, null, 0);
				if (log.IsInfoEnabled)
					log.Info("No LootGenerator found, adding LootGeneratorMoney for all mobs as default.");
			}

			if (log.IsInfoEnabled)
				log.Info("LootGenerator initialized: true");
			return true;
		}

		/// <summary>
		/// Stores a generator in a cache to reused the same generators multiple times
		/// </summary>
		/// <param name="dbGenerator"></param>
		/// <param name="generator"></param>
		private static void PutGeneratorInCache(DBLootGenerator dbGenerator, ILootGenerator generator)
		{
			m_ClassGenerators[dbGenerator.LootGeneratorClass + dbGenerator.ExclusivePriority] = generator;
		}

		/// <summary>
		///  Returns a generator from cache
		/// </summary>
		/// <param name="dbGenerator"></param>
		/// <returns></returns>
		private static ILootGenerator GetGeneratorInCache(DBLootGenerator dbGenerator)
		{
			if (m_ClassGenerators[dbGenerator.LootGeneratorClass + dbGenerator.ExclusivePriority] != null)
			{
				return (ILootGenerator)m_ClassGenerators[dbGenerator.LootGeneratorClass + dbGenerator.ExclusivePriority];
			}
			return null;
		}

		/// <summary>
		/// Unregister a generator for the given parameters		
		/// </summary>
		/// <param name="generator"></param>
		/// <param name="mobname"></param>
		/// <param name="mobguild"></param>
		/// <param name="mobfaction"></param>
		public static void UnRegisterLootGenerator(ILootGenerator generator, string mobname, string mobguild, string mobfaction)
		{
			if (generator == null)
				return;

			if (!Util.IsEmpty(mobname))
			{
				IList nameList = (IList)m_mobNameGenerators[mobname];
				if (nameList != null)
				{
					nameList.Remove(generator);
				}
			}

			if (!Util.IsEmpty(mobguild))
			{
				IList guildList = (IList)m_mobGuildGenerators[mobguild];
				if (guildList != null)
				{
					guildList.Remove(generator);
				}
			}

			if (Util.IsEmpty(mobname) && Util.IsEmpty(mobguild) && Util.IsEmpty(mobfaction))
			{
				m_globalGenerators.Remove(generator);
			}
		}


		/// <summary>
		/// Register a generator for the given parameters,
		/// If all parameters are null a global generaotr for all mobs will be registered
		/// </summary>
		/// <param name="generator"></param>
		/// <param name="mobname"></param>
		/// <param name="mobguild"></param>
		/// <param name="mobfaction"></param>
		public static void RegisterLootGenerator(ILootGenerator generator, string mobname, string mobguild, string mobfaction, int mobregion)
		{
			if (generator == null)
				return;

			if (!Util.IsEmpty(mobname))
			{
				IList nameList = (IList)m_mobNameGenerators[mobname];
				if (nameList == null)
				{
					nameList = new ArrayList();
					m_mobNameGenerators[mobname] = nameList;
				}
				nameList.Add(generator);
			}

			if (!Util.IsEmpty(mobguild))
			{
				IList guildList = (IList)m_mobGuildGenerators[mobguild];
				if (guildList == null)
				{
					guildList = new ArrayList();
					m_mobGuildGenerators[mobname] = guildList;
				}
				guildList.Add(generator);
			}

			if (mobregion > 0)
			{
				IList regionList = (IList)m_mobRegionGenerators[mobregion];
				if (regionList == null)
				{
					regionList = new ArrayList();
					m_mobRegionGenerators[mobregion] = regionList;
				}
				regionList.Add(generator);
			}

			if (Util.IsEmpty(mobname) && Util.IsEmpty(mobguild) && Util.IsEmpty(mobfaction) && mobregion == 0)
			{
				m_globalGenerators.Add(generator);
			}
		}


		/// <summary>
		/// Returns the lot for the given Mob
		/// </summary>		
		/// <param name="mob"></param>
		/// <param name="killer"></param>
		/// <returns></returns>
		public static ItemTemplate[] GetLoot(GameNPC mob, GameObject killer)
		{
			LootList lootList = null;
			IList generators = GetLootGenerators(mob);
			foreach (ILootGenerator generator in generators)
			{
				try
				{
					if (lootList == null)
						lootList = generator.GenerateLoot(mob, killer);
					else
						lootList.AddAll(generator.GenerateLoot(mob, killer));
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error("GetLoot", e);
				}
			}
			if (lootList != null)
				return lootList.GetLoot();
			else
				return new ItemTemplate[0];
		}

		/// <summary>
		/// Returns the ILootGenerators for the given mobs
		/// </summary>
		/// <param name="mob"></param>
		/// <returns></returns>
		private static IList GetLootGenerators(GameNPC mob)
		{
			IList filteredGenerators = new ArrayList();
			ILootGenerator exclusiveGenerator = null;

			IList nameGenerators = (IList)m_mobNameGenerators[mob.Name];
			IList guildGenerators = (IList)m_mobGuildGenerators[mob.GuildName];
			IList regionGenerators = (IList)m_mobRegionGenerators[mob.CurrentRegionID];
			//IList factionGenerators = m_mobFactionGenerators[mob.Faction]; not implemented

			ArrayList allGenerators = new ArrayList();

			allGenerators.AddRange(m_globalGenerators);
			if (nameGenerators != null)
				allGenerators.AddRange(nameGenerators);
			if (guildGenerators != null)
				allGenerators.AddRange(guildGenerators);
			if (regionGenerators != null)
				allGenerators.AddRange(regionGenerators);

			foreach (ILootGenerator generator in allGenerators)
			{
				if (generator.ExclusivePriority > 0)
				{
					if (exclusiveGenerator == null || exclusiveGenerator.ExclusivePriority < generator.ExclusivePriority)
						exclusiveGenerator = generator;
				}

				// if we found a exclusive generator skip adding other generators, since list will only contain exclusive generator.
				if (exclusiveGenerator != null)
					continue;

				if (!filteredGenerators.Contains(generator))
					filteredGenerators.Add(generator);
			}

			// if an exclusivegenerator is found only this one is used.
			if (exclusiveGenerator != null)
			{
				filteredGenerators.Clear();
				filteredGenerators.Add(exclusiveGenerator);
			}

			return filteredGenerators;
		}
	}
}
