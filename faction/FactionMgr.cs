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
using System.Collections.Generic;
using DOL.GS.Database;
using DOL.Database.DataAccessInterfaces;
using DOL.Database.DataTransferObjects;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// FactionMgr manage all the faction system
	/// </summary>
	public class FactionMgr
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public const int MAX_FACTION_AGGROLEVEL_VALUE = 1000;
		public const int MIN_FACTION_AGGROLEVEL_VALUE = -1500;

		/// <summary>
		/// This hash store all Factions of the game by unique id
		/// </summary>
		private static IDictionary<int, Faction> m_factions = new Dictionary<int, Faction>();

		/// <summary>
		/// This method load all the factions from the DB
		/// </summary>	
		public static bool LoadAllFactions()
		{
			IList<FactionEntity> allFactions = GameServer.DatabaseNew.Using<IFactionDao>().SelectAll();

			foreach (FactionEntity currentFaction in allFactions)
			{
				m_factions.Add(currentFaction.FactionId, new Faction(currentFaction));
			}

			IList<FriendFactionEntity> friendFactions = GameServer.DatabaseNew.Using<IFriendFactionDao>().SelectAll();
			foreach (FriendFactionEntity currentFriendFaction in friendFactions)
			{
				Faction myFaction = GetFaction(currentFriendFaction.FactionId);
				Faction myFriendFaction = GetFaction(currentFriendFaction.FriendFactionId);
				myFaction.AddFriendFaction(myFriendFaction);
			}

			IList<EnemyFactionEntity> enemyFactions = GameServer.DatabaseNew.Using<IEnemyFactionDao>().SelectAll();
			foreach (EnemyFactionEntity currentEnemyFaction in enemyFactions)
			{
				Faction myFaction = GetFaction(currentEnemyFaction.FactionId);
				Faction myEnemyFaction = GetFaction(currentEnemyFaction.EnemyFactionId);
				myFaction.AddEnemyFaction(myEnemyFaction);
			}

			return true;
		}

		/// <summary>
		/// Get the faction with the given id
		/// </summary>
		/// <param name="uniqueID">The id of the jump point to get</param>
		public static Faction GetFaction(int uniqueID)
		{
			return m_factions[uniqueID] ;
		}
	}
}
