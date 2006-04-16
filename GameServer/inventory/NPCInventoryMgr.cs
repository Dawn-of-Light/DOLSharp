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
using DOL.GS.Database;
using System.Collections;
using System;
using log4net;
using System.Reflection;

namespace DOL.GS
{
	public class NPCInventoryMgr
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Holds all the npc inventorys used in the world (id => npcinventory)
		/// </summary>
		public static IDictionary m_npcInventorys = new Hashtable(1);

		/// <summary>
		/// Lod all jump points from the db
		/// </summary>
		public static bool LoadAllNPCInventorys()
		{
			IList allNPCInventorys = GameServer.Database.SelectAllObjects(typeof(GameNpcInventory));
			foreach (GameNpcInventory currentInventory in allNPCInventorys)
			{
				if(m_npcInventorys.Contains(currentInventory.InventoryID))
				{	
					if(log.IsWarnEnabled)
						log.Warn("Npc inventory unique id defined twice (InventoryID : "+currentInventory.InventoryID+")");	
				}
				else
				{
					m_npcInventorys.Add(currentInventory.InventoryID, currentInventory);
				}
			}
			return true;
		}

		/// <summary>
		/// Add a new inventory to the NPCInventoryMgr
		/// </summary>
		/// <param name="newInventory">The new inventory to add</param>
		public static int AddNPCInventory(GameNpcInventory newInventory)
		{
			int freeInventoryId = 0;
			do
			{
				freeInventoryId = Util.Random(1, int.MaxValue);
			}
			while(m_npcInventorys.Contains(freeInventoryId));
			
			newInventory.InventoryID = freeInventoryId;
			m_npcInventorys.Add(freeInventoryId, newInventory);
					
			return freeInventoryId;
		}

		/// <summary>
		/// Get the npc inventory with the given id
		/// </summary>
		/// <param name="uniqueID">The id of the jump point to get</param>
		public static GameNpcInventory GetNPCInventory(int uniqueID)
		{
			return (m_npcInventorys[uniqueID] as GameNpcInventory);
		}
		
	}
}