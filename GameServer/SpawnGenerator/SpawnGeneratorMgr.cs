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
using System.Collections.Specialized;
using DOL.GS.Database;
using DOL.GS.Scripts;
using log4net;
using NHibernate.Expression;

namespace DOL.GS.SpawnGenerators
{
	/// <summary>
	/// Description résumée de SpawnGeneratorMgr.
	/// </summary>
	public class SpawnGeneratorMgr
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// not local in init because can be usefull in futur
        /// </summary>
        private static HybridDictionary m_spawnGenerators = new HybridDictionary(1);

		public static bool Init(Region region)
		{
			if (log.IsInfoEnabled)
				log.Info("Loading Spawn Generator...");
			
			IList m_spawnAreas;
			try
			{
                m_spawnAreas = GameServer.Database.SelectObjects(typeof(DBSpawnArea), Expression.Eq("RegionID", region.RegionID));
			}
			catch(Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("SpawnGeneratorMgr : spawn areas could not be loaded", e);
				return false;
			}
 			
			if(m_spawnAreas != null)
			{
				foreach( DBSpawnArea dbspawnArea in m_spawnAreas) 
				{
					ISpawnArea spawnArea;
					if (dbspawnArea.AreaType == null || dbspawnArea.AreaType == "")
						spawnArea= new SquareArea(region);//default
					else
                        spawnArea = ScriptMgr.GetInstance(dbspawnArea.AreaType, region) as ISpawnArea;

					if (spawnArea == null)
					{
						if (log.IsErrorEnabled)
							log.Error("SpawnGeneratorMgr : spawnArea of type: "+dbspawnArea.AreaType+" does not exist or does not inherit from ISpawnArea.");
						continue;
					}
					spawnArea.ParseParams(dbspawnArea.AreaParams);
					
					ISpawnGenerator spawnGenerator;
					if (dbspawnArea.SpawnGenerator == null || dbspawnArea.SpawnGenerator == "")
						spawnGenerator= new StandardSpawnGenerator();//default
					else
						spawnGenerator = ScriptMgr.GetInstance(dbspawnArea.SpawnGenerator) as ISpawnGenerator;
					if (spawnGenerator == null)
					{
						if (log.IsErrorEnabled)
							log.Error("SpawnGeneratorMgr : spawnGenerator of type: "+dbspawnArea.SpawnGenerator+" does not exist or does not inherit from ISpawnGenerator.");
						continue;
					}
                    spawnGenerator.ParseParams(dbspawnArea.SpawnGeneratorParams);				
					spawnGenerator.Area = spawnArea;
                    m_spawnGenerators.Add(dbspawnArea.SpawnAreaId,spawnGenerator);
				}
			}
            IList m_mobSpawners;
            try
            {
                m_mobSpawners = GameServer.Database.SelectAllObjects(typeof(DBMobSpawner));
            }
            catch (Exception e)
            {
                if (log.IsErrorEnabled)
                    log.Error("SpawnGeneratorMgr : mob Spawners could not be loaded", e);
                return false;
            }
            if (m_mobSpawners != null)
            {
                foreach (DBMobSpawner dbmobSpawner in m_mobSpawners)
                {
                    IMobSpawner mobSpawner;
                    if (dbmobSpawner.MobSpawnerType == null || dbmobSpawner.MobSpawnerType == "")
                    {
                        if (log.IsErrorEnabled)
                            mobSpawner = new StandardMobSpawner();//default
                            log.Warn("SpawnGeneratorMgr : default mob Spawner of typeused.");
                        continue;
                    }
                    else
                        mobSpawner = ScriptMgr.GetInstance(dbmobSpawner.MobSpawnerType) as IMobSpawner;

                    if (mobSpawner == null)
					{
						if (log.IsErrorEnabled)
                            log.Error("SpawnGeneratorMgr : mob Spawner of type: " + dbmobSpawner.MobSpawnerType + " does not exist or does not inherit from IMobSpawner.");
						continue;
					}
                    mobSpawner.ParseParams(dbmobSpawner.MobSpawnerParams);

                    ISpawnGenerator spawnGenerator = m_spawnGenerators[dbmobSpawner.SpawnAreaID] as ISpawnGenerator;
                    spawnGenerator.MobSpawners.Add(mobSpawner);
                }
            }
            foreach (ISpawnGenerator spawnGenerator in m_spawnGenerators.Values)
            {
                spawnGenerator.Init();
            }
            
			if (log.IsInfoEnabled)
			{
				log.Info("Create "+m_spawnAreas.Count+" spawn areas in Region "+ region.Description);
			}
			return true;
		}
	}
}
