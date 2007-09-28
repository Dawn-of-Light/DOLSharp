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
using System.Collections;
using System.Collections.Specialized;
using DOL.GS.PacketHandler;
namespace DOL.GS
{
	/// <summary>
	/// Battlegroups
	/// </summary>
	public class BattleGroup
	{
		public const string BATTLEGROUP_PROPERTY="battlegroup";
		/// <summary>
		/// This holds all players inside the battlegroup
		/// </summary>
		protected HybridDictionary m_battlegroupMembers = new HybridDictionary();

        bool battlegroupLootType = false;
        GamePlayer battlegroupTreasurer = null;
        int battlegroupLootTypeThreshold = 0;

		/// <summary>
		/// constructor of battlegroup
		/// </summary>
		public BattleGroup()
		{
            battlegroupLootType = false;
            battlegroupTreasurer = null;
		}
		public HybridDictionary Members
		{
			get{return m_battlegroupMembers;}
			set{m_battlegroupMembers=value;}
		}
		private bool listen=false;
		public bool Listen
		{
			get{return listen;}
			set{listen = value;}
		}
		private bool ispublic=true;
		public bool IsPublic
		{
			get{return ispublic;}
			set{ispublic = value;}
		}
		private string password="";
		public string Password
		{
			get{return password;}
			set{password = value;}
		}

		/// <summary>
		/// Adds a player to the chatgroup
		/// </summary>
		/// <param name="player">GamePlayer to be added to the group</param>
		/// <param name="leader"></param>
		/// <returns>true if added successfully</returns>
		public virtual bool AddBattlePlayer(GamePlayer player,bool leader) 
		{
			if (player == null) return false;
			lock (m_battlegroupMembers)
			{
				if (m_battlegroupMembers.Contains(player))
					return false;
				player.TempProperties.setProperty(BATTLEGROUP_PROPERTY, this);
				player.Out.SendMessage("You join the battle group.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				foreach(GamePlayer member in Members.Keys)
				{
					member.Out.SendMessage(player.Name+" has joined the battle group.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				m_battlegroupMembers.Add(player,leader);
			}
			return true;
		}

        public virtual bool IsInTheBattleGroup(GamePlayer player)
        {
            lock (m_battlegroupMembers) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
            {
                return m_battlegroupMembers.Contains(player);
            }
        }

        public bool GetBGLootType()
        {
            return battlegroupLootType;
        }

        public GamePlayer GetBGTreasurer()
        {
            return battlegroupTreasurer;
        }

        public int GetBGLootTypeThreshold()
        {
            return battlegroupLootTypeThreshold;
        }

        public bool SetBGLootTypeThreshold(int thresh)
        {
            battlegroupLootTypeThreshold = thresh;

            if (thresh < 0 || thresh > 50)
            {
                battlegroupLootTypeThreshold = 0;
                return false;
            }
            else
            {
                return true;
            }
        }

        public void SetBGLootType(bool type)
        {
            battlegroupLootType = type;
            if (battlegroupLootType == false)
            {
                // Within bounds, continue
            }
            if (battlegroupLootType == true)
            {
                // Within bounds, continue
            }
            else
            {
                // Data has entered here that should not be, set to normal = 0
                battlegroupLootType = false;
            }
        }

        public bool SetBGTreasurer(GamePlayer treasurer)
        {
            battlegroupTreasurer = treasurer;
            if (battlegroupTreasurer == null)
            {
                // Do not set treasurer
                return false;
            }

            if (battlegroupTreasurer != null)
            {
                // Good input, got a treasurer, continue
                return true;
            }
            else
            {
                // Bad input, fix with null
                battlegroupTreasurer = null;
                return false;
            }
        }
        
        public GamePlayer[] GetPlayersInTheBattleGroup()
        {
            ArrayList players = new ArrayList(ServerProperties.Properties.BATTLEGROUP_MAX_MEMBER);
            lock (m_battlegroupMembers.SyncRoot) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
            {
                for (int i = 0; i < m_battlegroupMembers.Count; i++)
                {
                    players.Add((GamePlayer)m_battlegroupMembers[i]);
                }
            }
            return (GamePlayer[])players.ToArray(typeof(GamePlayer));
        }

        public virtual void SendMessageToBattleGroupMembers(string msg, eChatType type, eChatLoc loc)
        {
            lock (m_battlegroupMembers) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
            {
                foreach (GamePlayer player in m_battlegroupMembers)
                {
                    player.Out.SendMessage(msg, type, loc);
                }
            }
        }

        public int PlayerCount
        {
            get { return m_battlegroupMembers.Count; }
        }
        /// <summary>
		/// Removes a player from the group
		/// </summary>
		/// <param name="player">GamePlayer to be removed</param>
		/// <returns>true if removed, false if not</returns>
		public virtual bool RemoveBattlePlayer(GamePlayer player)
		{
			if (player == null) return false;
			lock (m_battlegroupMembers)
			{
				if (!m_battlegroupMembers.Contains(player))
					return false;
				m_battlegroupMembers.Remove(player);
				player.TempProperties.removeProperty(BATTLEGROUP_PROPERTY);
				player.Out.SendMessage("You leave the battle group.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				foreach(GamePlayer member in Members.Keys)
				{
					member.Out.SendMessage(player.Name+" has left the battle group.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				if (m_battlegroupMembers.Count == 1)
				{
					ArrayList lastPlayers = new ArrayList(m_battlegroupMembers.Count);
					lastPlayers.AddRange(m_battlegroupMembers.Keys);
					foreach (GamePlayer plr in lastPlayers)
					{
						RemoveBattlePlayer(plr);
					}
				}
			}
			return true;
		}
	}
}
