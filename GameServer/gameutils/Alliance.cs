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
using System.Collections.Generic;
using System.Collections.Specialized;
using System;
using System.Runtime.Serialization;
using DOL.Database2;

namespace DOL.GS
{
	/// <summary>
	/// Alliance are the alliance between guild in game
	/// </summary>
	public class Alliance : DatabaseObject
	{
        [NonSerialized]
		protected List<Guild> m_guilds = null;
        private string m_Motd;
        private string m_AllianceName;

        public string AllianceName
        {
            get { return m_AllianceName; }
            set { m_AllianceName = value; }
        }
        protected string Motd
        {
            get { return m_Motd; }
            set { m_Motd = value;
            Dirty = true;
        }
        }
        protected List<UInt64> m_guildids = null;
		public Alliance()
		{
			m_guilds = new ArrayList(2);
            m_guildids = new ArrayList(2);
		}
        public List<Guild> Guilds
		{
			get
			{                
                //runtime context creation
                if (m_guilds = null)
                {
                    m_guilds = new ArrayList(m_guildids.Count);
                    foreach (UInt64 guildid in m_guildids)
                    {
                        Guild guild;
                        if (!DatabaseLayer.Instance.DatabaseObjects.TryGetValue(guild, guild))
                        {
                            log.Error("Could not get Guild in Alliance " + m_name + "with key" + m_alliance_id);
                        }
                        else
                        {
                            m_guilds.Add(guild);
                        }
                    }
                }
				return m_guilds;
			}
			set
			{
				m_guilds = value;
                Dirty = true;
			}
		}
        [OnSerializing]
        public void StoreContext()
        {
            m_guildids = new List<ulong>();
            foreach (Guild g in m_guilds)
                m_guildids.Add(g.ID);
        }
		#region IList
		public void AddGuild(Guild myguild)
		{
			lock (Guilds.SyncRoot)
			{
				myguild.alliance = this;
				Guilds.Add(myguild);
				//sirru 23.12.06 save changes to db for each guild
				SaveIntoDatabase();
				SendMessageToAllianceMembers(myguild.Name + " has joined the alliance of " + m_dballiance.AllianceName, PacketHandler.eChatType.CT_System, PacketHandler.eChatLoc.CL_SystemWindow);
			}
		}
		public void RemoveGuild(Guild myguild)
		{
			lock (Guilds.SyncRoot)
			{
				myguild.alliance = null;
				Guilds.Remove(myguild);
				myguild.theGuildDB.AllianceID = "";
				m_dballiance.DBguilds = null;
				GameServer.Database.SaveObject(m_dballiance);
				GameServer.Database.FillObjectRelations(m_dballiance);
				//sirru 23.12.06 save changes to db for each guild
				myguild.SaveIntoDatabase();
				SendMessageToAllianceMembers(myguild.Name + " has left the alliance of " + m_dballiance.AllianceName, PacketHandler.eChatType.CT_System, PacketHandler.eChatLoc.CL_SystemWindow);
			}
		}
		public void Clear()
		{
			lock (Guilds.SyncRoot)
			{
				foreach (Guild guild in Guilds)
				{
					guild.alliance = null;
					guild.theGuildDB.AllianceID = "";
					//sirru 23.12.06 save changes to db
					guild.SaveIntoDatabase();
				}
				Guilds.Clear();
			}
		}
		public bool Contains(Guild myguild)
		{
			lock (Guilds.SyncRoot)
			{
				return Guilds.Contains(myguild);
			}
		}

		#endregion

		/// <summary>
		/// send message to all member of alliance
		/// </summary>
		public void SendMessageToAllianceMembers(string msg, PacketHandler.eChatType type, PacketHandler.eChatLoc loc)
		{
			lock (Guilds.SyncRoot)
			{
				foreach (Guild guild in Guilds)
				{
					guild.SendMessageToGuildMembers(msg, type, loc);
				}
			}
		}

	}
}
