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
using DOL.Database2;
using DOL.GS.PacketHandler;

namespace DOL.GS
{
	/// <summary>
	/// Faction of mob
	/// </summary>
	public class Faction : DatabaseObject
	{
		private const int DECREASE_AGGRO_AMOUNT = -1;
		private const int INCREASE_AGGRO_AMOUNT = 1;
		private const int MAX_AGGRO_VALUE = 100;
		private const int MIN_AGGRO_VALUE = -100;

		public Faction()
            :base()
		{
			m_name = String.Empty;
			m_friendFactions = new ArrayList(1);
			m_enemyFactions = new ArrayList(1);
			m_playerxFaction = new Hashtable(1);
			m_updatePlayer = new ArrayList(1);
		}

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
			get { return m_name; }
		}

		/// <summary>
		/// hold friend factions
		/// </summary>
		private List<Faction> m_friendFactions;
		/// <summary>
		/// friend factions
		/// </summary>
		public List<Faction> FriendFactions
		{
			get { return m_friendFactions; }
		}

		/// <summary>
		/// hold enemy factions
		/// </summary>
        [NonSerialized]
        private List<Faction> m_enemyFactions_secret = new List<Faction>();
        private List<Faction> m_enemyFactions
        {
            get
            {
                if (m_enemyFactions_secret == null)
                {
                }
            }
        }
        private List<UInt64> m_enemyFactionIDs = new List<ulong>();
		/// <summary>
		/// enemy factions
		/// </summary>
		public List<Faction> EnemyFactions
		{
			get { return m_enemyFactions; }
		}


		/// <summary>
		/// hold base aggro level
		/// </summary>
		private int m_baseAggroLevel;
		/// <summary>
		/// base aggro when player have never meet faction before
		/// </summary>
		public int BaseAggroLevel
		{
			get { return m_baseAggroLevel; }
		}

		/// <summary>
		/// this is the table of player aggrolevel
		/// </summary>
		private Dictionary<UInt64,int>  m_playerxFaction = new Dictionary<UInt64,int>();

		private List<GamePlayer> m_updatePlayer;

		/// <summary>
		/// table of player and aggrolevel (characterid/aggrolevel)
		/// </summary>
		public Dictionary<UInt64,int> PlayerxFaction
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

		#region changes for interactions with faction members
		/// <summary>
		/// called when a player kills a mob from the faction
		/// </summary>
		/// <param name="killer"></param>
		public void KillMember(GamePlayer killer)
		{
			this.ChangeAggroLevel(killer, INCREASE_AGGRO_AMOUNT);
			foreach (Faction faction in m_friendFactions)
			{
				faction.ChangeAggroLevel(killer, INCREASE_AGGRO_AMOUNT);
			}
			foreach (Faction faction in m_enemyFactions)
			{
				faction.ChangeAggroLevel(killer, DECREASE_AGGRO_AMOUNT);
			}
		}

		/// <summary>
        /// changes aggro of faction and related factions to player
		/// </summary>
		/// <param name="player"></param>
		/// <param name="amount"></param>
		public void ChangeAggroLevel(GamePlayer player, int amount)
		{
			// remember the player
			if (!m_updatePlayer.Contains(player.PlayerCharacter.ID))
			{
				m_updatePlayer.Add(player.PlayerCharacter.ID);
			}
			int oldAggro;
			// remember the player's relation to the faction
			if (m_playerxFaction.ContainsKey(player.PlayerCharacter.ID))
			{
				oldAggro = (int)m_playerxFaction[player.PlayerCharacter.ID];
			}
			else
			{
				oldAggro = BaseAggroLevel;
				m_playerxFaction.Add(player.PlayerCharacter.ID, BaseAggroLevel);
			}
			// get the new relation
			int newAggro = oldAggro + amount;
			// clamp it between MIN and MAX
			if (newAggro < MIN_AGGRO_VALUE)
			{
				newAggro = MIN_AGGRO_VALUE;
			}
			else if (newAggro > MAX_AGGRO_VALUE)
			{
				newAggro = MAX_AGGRO_VALUE;
			}
			// check if changed
			if (newAggro != oldAggro)
			{
				// save the change
				m_playerxFaction[player.PlayerCharacter.ID] = newAggro;
				// tell the player
				string msg = "Your relationship with " + this.Name + " has ";
				if (amount > 0)
				{
					msg += "decreased.";
				}
				else
				{
					msg += "increased.";
				}
				player.Out.SendMessage(msg, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}

		/// <summary>
		/// gets aggro level of player with faction
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public int GetAggroToFaction(GamePlayer player)
		{
			if (m_playerxFaction.ContainsKey(player.PlayerCharacter.ID))
				return (int)m_playerxFaction[player.PlayerCharacter.ID];
			else
				return BaseAggroLevel;
		}
		#endregion
	}
}
