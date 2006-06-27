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
using DOL.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS
{
	/// <summary>
	/// Faction of mob
	/// </summary>
	public class Faction
	{
		private const int DICREASE_VALUE = 1;
		private const int INCREASE_VALUE = 1;
		private const int MAX_VALUE = 100;
		private const int MIN_VALUE = -100;

		public Faction()
		{
			m_name = String.Empty;
			m_friendFactions = new ArrayList(1);
			m_enemyFactions = new ArrayList(1);
			m_playerxFaction = new Hashtable(1);
			m_updatePlayer = new ArrayList(1);
		}

		#region DB
		/// <summary>
		/// load faction from DB
		/// </summary>
		/// <param name="dbfaction"></param>
		public void LoadFromDatabase(DBFaction dbfaction)
		{
			m_name= dbfaction.Name;
			m_id=dbfaction.ID;
			m_baseFriendship=dbfaction.BaseFriendShip;
		}
		public void SaveAggroToFaction()
		{
			foreach (string charID in m_updatePlayer)
			{
				SaveAggroToFaction(charID);
			}
			m_updatePlayer.Clear();
		}
		public void SaveAggroToFaction(string charID)
		{
			DBFactionAggroLevel dbfactionAggroLevel = (DBFactionAggroLevel)GameServer.Database.SelectObject(typeof(DBFactionAggroLevel),"CharacterID = '" + charID + "' AND FactionID ="+this.ID);
			if (dbfactionAggroLevel == null)
			{
				dbfactionAggroLevel = new DBFactionAggroLevel();
				dbfactionAggroLevel.AggroLevel = (int)m_playerxFaction[charID];
				dbfactionAggroLevel.CharacterID = charID;
				dbfactionAggroLevel.FactionID = this.ID;
				GameServer.Database.AddNewObject(dbfactionAggroLevel);
			}
			else
			{
				dbfactionAggroLevel.AggroLevel = (int)m_playerxFaction[charID];
				GameServer.Database.SaveObject(dbfactionAggroLevel);
			}
		}
		#endregion

		#region Properties
		
		
		/// <summary>
		/// hold name of faction
		/// </summary>
		private string m_name;
		/// <summary>
		/// name of faction
		/// </summary>
		public string Name
		{
			get{ return m_name; }
		}

		/// <summary>
		/// hold friend factions
		/// </summary>
		private ArrayList m_friendFactions;
		/// <summary>
		/// friend factions
		/// </summary>
		public ArrayList FriendFactions
		{
			get{ return m_friendFactions; }
		}

		/// <summary>
		/// hold enemy factions
		/// </summary>
		private ArrayList m_enemyFactions;
		/// <summary>
		/// enemy factions
		/// </summary>
		public ArrayList EnemyFactions
		{
			get{ return m_enemyFactions; }
		}

		/// <summary>
		/// hold id of faction
		/// </summary>
		private int m_id;
		/// <summary>
		/// id of faction
		/// </summary>
		public int ID
		{
			get { return m_id; }
		}

		/// <summary>
		/// hold basefriendship
		/// </summary>
		private int m_baseFriendship;
		/// <summary>
		/// base friendship when player have never meet faction before
		/// </summary>
		private int BaseFriendShip
		{
			get { return m_baseFriendship; }
		}

		/// <summary>
		/// this is the table of player aggrolevel 
		/// </summary>
		private Hashtable m_playerxFaction;

		private ArrayList m_updatePlayer;

		/// <summary>
		/// table of player and aggrolevel (characterid/aggrolevel)
		/// </summary>
		public Hashtable PlayerxFaction
		{
			get { return m_playerxFaction; }
		}
		#endregion

		#region Friend/enemy Faction
		/// <summary>
		/// add friend faction to this faction
		/// </summary>
		/// <param name="faction"></param>
		public void AddFriendFaction(Faction faction)
		{
			if (!m_friendFactions.Contains(faction))
				m_friendFactions.Add(faction);
		}

		/// <summary>
		/// remove friend faction
		/// </summary>
		/// <param name="faction"></param>
		public void RemoveFriendFaction(Faction faction)
		{
			if (m_friendFactions.Contains(faction))
				m_friendFactions.Remove(faction);
		}

		/// <summary>
		/// add enemy faction
		/// </summary>
		/// <param name="faction"></param>
		public void AddEnemyFaction(Faction faction)
		{
			if (!m_enemyFactions.Contains(faction))
				m_enemyFactions.Add(faction);
		}

		/// <summary>
		/// remove enemy faction
		/// </summary>
		/// <param name="faction"></param>
		public void RemoveEnemyFaction(Faction faction)
		{
			if (m_enemyFactions.Contains(faction))
				m_enemyFactions.Remove(faction);
		}
		#endregion

		#region Killmember/direase/increase/get friendship
		/// <summary>
		/// methode call when player kill a mob with faction
		/// </summary>
		/// <param name="killer"></param>
		public void KillMember(GamePlayer killer)
		{
			this.DicreaseFriendship(killer);
			foreach(Faction faction in m_friendFactions)
			{
				faction.DicreaseFriendship(killer);
			}
			foreach(Faction faction in m_enemyFactions)
			{
				faction.IncreaseFriendship(killer);
			}
		}
		/// <summary>
		/// dicrease friendship of player with faction
		/// </summary>
		/// <param name="killer"></param>
		public void DicreaseFriendship(GamePlayer killer)
		{
			if (!m_updatePlayer.Contains(killer.PlayerCharacter.ObjectId))
				m_updatePlayer.Add(killer.PlayerCharacter.ObjectId);
			if(m_playerxFaction.ContainsKey(killer.PlayerCharacter.ObjectId))
			{
				int friendship = (int)m_playerxFaction[killer.PlayerCharacter.ObjectId];
				if (friendship > MIN_VALUE)
					m_playerxFaction[killer.PlayerCharacter.ObjectId] = friendship -DICREASE_VALUE;
			}
			else
			{
				m_playerxFaction.Add(killer.PlayerCharacter.ObjectId, BaseFriendShip - DICREASE_VALUE);
			}
			killer.Out.SendMessage("Your relationship with "+this.Name+" has decrease.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			
		}
		
		/// <summary>
		/// increase friendship of player with faction
		/// </summary>
		/// <param name="killer"></param>
		public void IncreaseFriendship(GamePlayer killer)
		{
			if (!m_updatePlayer.Contains(killer.PlayerCharacter.ObjectId))
				m_updatePlayer.Add(killer.PlayerCharacter.ObjectId);
			if(m_playerxFaction.ContainsKey(killer.PlayerCharacter.ObjectId))
			{
				int friendship = (int)m_playerxFaction[killer.PlayerCharacter.ObjectId];
				if (friendship < MAX_VALUE)
					m_playerxFaction[killer.PlayerCharacter.ObjectId] = friendship + INCREASE_VALUE;
			}
			else
			{
				m_playerxFaction.Add(killer.PlayerCharacter.ObjectId, BaseFriendShip + INCREASE_VALUE);
			}
			killer.Out.SendMessage("Your relationship with "+this.Name+" has increase.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}
		

		/// <summary>
		/// get aggro level of player with faction
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public int GetAggroToFaction(GamePlayer player)
		{
			if(m_playerxFaction.ContainsKey(player.PlayerCharacter.ObjectId))
				return (int)m_playerxFaction[player.PlayerCharacter.ObjectId];
			else
				return BaseFriendShip;
		}
		#endregion
	}
}
