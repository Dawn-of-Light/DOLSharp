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
using System.Collections.Generic;
using System.Collections.Concurrent;

using DOL.Database;

namespace DOL.GS
{
	/// <summary>
	/// NPCManager is a Static Class meant to handle a "datastore" of NPC records, which includes all Mobs, Merchants, Guards, Ambients
	/// it's purpose is to initialize a memory datastore for all these, and allows to reference respawning mobs or temporary living...
	/// NPC data is pretty much used during server startup by multiple thread, NPC instances are mostly used in a single thread
	/// NPC data shouldn't be needed after startup, but some special admin tools could query them during running, foreign tools could need periodic refresh too !
	/// </summary>
	public static class NPCManager
	{
		/// <summary>
		/// Dictionnary holding NPC table cache
		/// </summary>
		private static ConcurrentDictionary<long, Npc> m_NpcData = new ConcurrentDictionary<long, Npc>();
		
		public static bool Init()
		{
			try
			{
				IList<Npc> npcTable = GameServer.Database.SelectAllObjects<Npc>();
				
				foreach(Npc npcTuple in npcTable)
				{
					if (!m_NpcData.TryAdd(npcTuple.NpcID, npcTuple))
						return false;
				}
				
				return true;
			}
			catch
			{
				throw;
			}
			
		}
		
	}
}
