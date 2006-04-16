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
		/// The listof all spawn generators used in the world (uniqueid => spawngenerator)
		/// </summary>
		protected static IDictionary m_spawnGenerators = new HybridDictionary(1);

		/// <summary>
		/// Lod all spawn generator from the db
		/// </summary>
		public static bool LoadAllSpawnGenerator()
		{
			lock(m_spawnGenerators.SyncRoot)
			{
				IList allSpawnGenerators = GameServer.Database.SelectAllObjects(typeof(SpawnGeneratorBase));
				foreach (SpawnGeneratorBase currentSpawnGenerator in allSpawnGenerators)
				{
					if(m_spawnGenerators.Contains(currentSpawnGenerator.SpawnGeneratorBaseID))
					{	
						if(log.IsWarnEnabled)
							log.Warn("SpawnGenerator unique id defined twice (JumpPointID : "+currentSpawnGenerator.SpawnGeneratorBaseID+")");	
					}
					else
					{
						m_spawnGenerators.Add(currentSpawnGenerator.SpawnGeneratorBaseID, currentSpawnGenerator);
						currentSpawnGenerator.Start();
					}
				}
				return true;
			}
		}
	}
}
