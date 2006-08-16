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
using System.Collections.Generic;
using DOL.GS.Database;
using DOL.Database.DataTransferObjects;
using DOL.GS.PacketHandler;

namespace DOL.GS
{
	/// <summary>
	/// Faction of mob
	/// </summary>
	public class Faction
	{

		public Faction(FactionEntity dbfaction)
		{
			this.FactionID = dbfaction.FactionId;
			this.Name = dbfaction.Name;
		}

		/// <summary>
		/// Store the unique db id of the faction
		/// </summary>
		private int m_id;

		/// <summary>
		/// Get and set the unique db id of the faction
		/// </summary>
		public int FactionID
		{
			get { return m_id; }
			set { m_id = value; }
		}
		
		/// <summary>
		/// Store the name of faction
		/// </summary>
		private string m_name;

		/// <summary>
		/// Get and set the name of faction
		/// </summary>
		public string Name
		{
			get { return m_name; }
			set { m_name = value; }
		}

		/// <summary>
		/// The list of all friend faction with this one
		/// </summary>
		private List<Faction> m_friendFactions;

		/// <summary>
		/// Gets or sets the list of all friend faction with this one
		/// </summary>
		public List<Faction> FriendFactions
		{
			get { return m_friendFactions; }
			set	{ m_friendFactions = value; }
		}

		/// <summary>
		/// The list of all enemy faction with this one
		/// </summary>
		private List<Faction> m_enemyFactions;
		
		/// <summary>
		/// Gets or sets the list of all friend faction with this one
		/// </summary>
		public List<Faction> EnemyFactions
		{
			get { return m_enemyFactions; }
			set	{ m_enemyFactions = value; }
		}

		public void AddFriendFaction(Faction faction)
		{
			if ( !this.FriendFactions.Contains(faction) ) 
				this.FriendFactions.Add(faction);
		}

		public void RemoveFriendFaction(Faction faction)
		{
			this.FriendFactions.Remove(faction);
		}

		public void AddEnemyFaction(Faction faction)
		{
			if ( !this.EnemyFactions.Contains(faction) )
				this.EnemyFactions.Add(faction);
		}

		public void RemoveEnemyFaction(Faction faction)
		{
			this.EnemyFactions.Remove(faction);
		}
	}
}
