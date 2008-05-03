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
using DOL.Database2;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// FactionMgr manage all the faction system
	/// </summary>
	public class FactionMgr
	{
		private FactionMgr(){}

		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static Hashtable m_factions;

		public static Hashtable Factions
		{
			get	{ return m_factions;}
		}
		/// <summary>
		/// this function load all faction from DB
		/// </summary>	
		public static bool Init()
		{
			m_factions = new Hashtable(1);

			DatabaseObject[] dbfactions =	GameServer.Database.SelectObjects(typeof(DBFaction));
			foreach(DBFaction dbfaction in dbfactions)
			{
				Faction myfaction = new Faction();
				myfaction.LoadFromDatabase(dbfaction);
				m_factions.Add(dbfaction.ID,myfaction);
			}

			DatabaseObject[] dblinkedfactions =	GameServer.Database.SelectObjects(typeof(DBLinkedFaction));
			foreach(DBLinkedFaction dblinkedfaction in dblinkedfactions)
			{
				Faction faction = GetFactionByID(dblinkedfaction.LinkedFactionID);
				Faction linkedFaction = GetFactionByID(dblinkedfaction.FactionID);
				if (faction == null || linkedFaction == null) 
				{
					log.Warn("Missing Faction or friend faction with Id :"+dblinkedfaction.LinkedFactionID+"/"+dblinkedfaction.FactionID);
					continue;
				}
				if (dblinkedfaction.IsFriend)
					faction.AddFriendFaction(linkedFaction);
				else
					faction.AddEnemyFaction(linkedFaction);
			}
			DatabaseObject[] dbfactionAggroLevels =	GameServer.Database.SelectObjects(typeof(DBFactionAggroLevel));
			foreach(DBFactionAggroLevel dbfactionAggroLevel in dbfactionAggroLevels)
			{
				Faction faction = GetFactionByID(dbfactionAggroLevel.FactionID);
				if (faction == null)
				{
					log.Warn("Missing Faction with Id :"+dbfactionAggroLevel.FactionID);
					continue;
				}
				faction.PlayerxFaction.Add(dbfactionAggroLevel.CharacterID,dbfactionAggroLevel.AggroLevel);
			}
			return true;
		}
		/// <summary>
		/// get the faction with id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static Faction GetFactionByID(int id)
		{
			if (m_factions.ContainsKey(id))
				return m_factions[id] as Faction;
			else
				return null;
		}

		/// <summary>
		/// save all faction aggro level of player who have change faction aggro level
		/// </summary>
		public static void SaveAllAggroToFaction()
		{
			if (m_factions == null) return; // nothing to save yet
			foreach(Faction faction in m_factions.Values)
				faction.SaveAggroToFaction();
		}
	}
}
