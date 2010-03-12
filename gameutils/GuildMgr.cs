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
using System.Collections.Specialized;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// The GuildMgr holds pointers to all guilds, and pointers
	/// to their members.
	/// </summary>
	public sealed class GuildMgr
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// ArrayList of all guilds in the game
		/// </summary>
		static private readonly HybridDictionary m_guilds = new HybridDictionary();
		
		/// <summary>
		/// ArrayList of all GuildIDs to GuildNames
		/// </summary>
		static private readonly HybridDictionary m_guildids = new HybridDictionary();

		/// <summary>
		/// Holds all the players combined with their guilds for the social window
		/// </summary>
		/// <remarks>Sorted on the player name in order to optimize the initial refresh (sorted by name)</remarks>
		private static readonly Dictionary<string, SortedList<string, SocialWindowMember>> m_guildXAllPlayers = new Dictionary<string, SortedList<string, SocialWindowMember>>();

		/// <summary>
		/// Gets the sorted dictionary for a guild to display in the social window
		/// </summary>
		/// <param name="guildID">The guild id for a player</param>
		/// <returns>The dictionary sorted on the player's name</returns>
		public static SortedList<string, SocialWindowMember> GetSocialWindowGuild(string guildID)
		{
			if (m_guildXAllPlayers.ContainsKey(guildID))
				return m_guildXAllPlayers[guildID];
			return null;
		}

		/// <summary>
		/// Add a player to the social window hash table
		/// </summary>
		/// <param name="player">Player to add</param>
		public static void AddPlayerToSocialWindow(GamePlayer player)
		{
			if (m_guildXAllPlayers.ContainsKey(player.GuildID))
			{
				if (!m_guildXAllPlayers[player.GuildID].ContainsKey(player.Name))
				{
					SortedList<string, SocialWindowMember> tempList = m_guildXAllPlayers[player.GuildID];
					SocialWindowMember member = new SocialWindowMember(player.Name, player.Level.ToString(), player.CharacterClass.ID.ToString(), player.GuildRank.RankLevel.ToString(), player.Group != null ? player.Group.MemberCount.ToString() : "1", player.CurrentZone.Description, player.GuildNote);
					tempList.Add(player.Name, member);
				}
			}
		}

		/// <summary>
		/// Remove a player from the social window hash table
		/// </summary>
		/// <param name="player">Player to remove</param>
		/// <returns>True if player was removed, else false.</returns>
		public static bool RemovePlayerFromSocialWindow(GamePlayer player)
		{
			if (m_guildXAllPlayers.ContainsKey(player.GuildID))
			{
				return m_guildXAllPlayers[player.GuildID].Remove(player.Name);
			}
			return false;
		}
		
		static private ushort m_lastID = 0;

		/// <summary>
		/// The cost in copper to reemblem the guild
		/// </summary>
		public const long COST_RE_EMBLEM = 1000000; //200 gold

		/// <summary>
		/// Adds a guild to the list of guilds
		/// </summary>
		/// <param name="guild">The guild to add</param>
		/// <returns>True if the function succeeded, otherwise false</returns>
		public static bool AddGuild(Guild guild)
		{
			if (guild == null)
				return false;

			lock (m_guilds.SyncRoot)
			{
				if (!m_guilds.Contains(guild.Name))
				{
					m_guilds.Add(guild.Name, guild);
					m_guildids.Add(guild.GuildID, guild.Name);
					guild.ID = ++m_lastID;
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Removes a guild from the manager
		/// </summary>
		/// <param name="guild">the guild</param>
		/// <returns></returns>
		public static bool RemoveGuild(Guild guild)
		{
			if (guild == null)
				return false;

			guild.RemoveAllMembers();
			lock (m_guilds.SyncRoot)
			{
				m_guilds.Remove(guild.Name);
				m_guildids.Remove(guild.GuildID);
			}
			return true;
		}

		/// <summary>
		/// Checks if a guild with guildName exists
		/// </summary>
		/// <param name="guildName">The guild to check</param>
		/// <returns>true or false</returns>
		public static bool DoesGuildExist(string guildName)
		{
			lock (m_guilds.SyncRoot)
			{
				if (m_guilds.Contains(guildName))
					return true;
				return false;
			}
		}

		/// <summary>
		/// Creates a new guild
		/// </summary>
		/// <returns>GuildEntry</returns>
		public static Guild CreateGuild(GamePlayer creator, string guildName)
		{
			try
			{
				if (DoesGuildExist(guildName))
				{
					if (creator != null)
						creator.Out.SendMessage(guildName + " already exists!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return null;
				}
				DBGuild dbGuild = new DBGuild();
				dbGuild.GuildName = guildName;
				dbGuild.GuildID = System.Guid.NewGuid().ToString();
				dbGuild.Realm = (byte)creator.Realm;
				Guild newguild = new Guild(dbGuild);
				CreateRanks(newguild);
				AddGuild(newguild);
				newguild.AddToDatabase();

				if (log.IsDebugEnabled)
					log.Debug("Create guild; guild name=\"" + guildName + "\" Realm=" + GlobalConstants.RealmToName(newguild.Realm));

				return newguild;
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("CreateGuild", e);
				return null;
			}
		}

		public static void CreateRanks(Guild newguild)
		{
			DBRank rank;
			for (int i = 0; i < 10; i++)
			{
				rank = new DBRank();
				rank.AcHear = false;
				rank.AcSpeak = false;
				rank.Alli = false;
				rank.Claim = false;
				rank.Emblem = false;
				rank.GcHear = true;
				rank.GcSpeak = false;
				rank.GuildID = newguild.GuildID;
				rank.Invite = false;
				rank.OcHear = false;
				rank.OcSpeak = false;
				rank.Promote = false;
				rank.RankLevel = (byte)i;
				rank.Release = false;
				rank.Remove = false;
				rank.Title = "Rank "+i.ToString();
				rank.Upgrade = false;
				rank.View = false;
                rank.View = false;
                rank.Dues = false;

                if (i < 9)
                {
                    rank.GcSpeak = true;
                    rank.View = true;
                    if (i < 8)
                    {
                        rank.Emblem = true;
                        if (i < 7)
                        {
                            rank.AcHear = true;
                            if (i < 6)
                            {
                                rank.AcSpeak = true;
                                if (i < 5)
                                {
                                    rank.OcHear = true;
                                    if (i < 4)
                                    {
                                        rank.OcSpeak = true;
                                        if (i < 3)
                                        {
                                            rank.Invite = true;
                                            rank.Promote = true;

                                            if (i < 2)
                                            {
                                                rank.Release = true;
                                                rank.Upgrade = true;
                                                rank.Claim = true;
                                                if (i < 1)
                                                {
                                                    rank.Remove = true;
                                                    rank.Alli = true;
                                                    rank.Dues = true;
                                                    rank.Withdraw = true;
													rank.Title = "Guildmaster";
													rank.Buff=true;
                                                }

                                            }

                                        }

                                    }

                                }

                            }

                        }

                    }

                }
                GameServer.Database.AddObject(rank);
				GameServer.Database.SaveObject(rank);
				newguild.Ranks[i] = rank;
			}
		}

		/// <summary>
		/// Deletes a guild
		/// </summary>
        public static bool DeleteGuild(string guildName)
        {
            try
            {
                Guild removeGuild = GetGuildByName(guildName);
                // Does guild exist, if not return false.
                if (removeGuild == null)
					return false;

				var guilds = GameServer.Database.SelectObjects<DBGuild>("GuildName='" + GameServer.Database.Escape(guildName) + "'");
				foreach (var guild in guilds)
				{
					foreach (var cha in GameServer.Database.SelectObjects<Character>("GuildID = '" + GameServer.Database.Escape(guild.GuildID) + "'"))
					{
						cha.GuildID = "";
					}

					GameServer.Database.DeleteObject(guild);
				}

                //[StephenxPimentel] We need to delete the guild specific ranks aswell!
                var ranks = GameServer.Database.SelectObjects<DBRank>("`GuildID` = '" + removeGuild.ID + "'");
                foreach (var guildRank in ranks)
                {
                    GameServer.Database.DeleteObject(guildRank);
                }


                lock (removeGuild.ListOnlineMembers())
                {
                    foreach (GamePlayer ply in removeGuild.ListOnlineMembers())
                    {
                        ply.Guild = null;
                        ply.GuildID = "";
                        ply.GuildName = "";
                        ply.GuildRank = null;
                    }
                }

                RemoveGuild(removeGuild);

                return true;
            }
            catch (Exception e)
            {
                if (log.IsErrorEnabled)
                    log.Error("DeleteGuild", e);
                return false;
            }
        }

		/// <summary>
		/// Returns a guild according to the matching name
		/// </summary>
		/// <returns>Guild</returns>
		public static Guild GetGuildByName(string guildName)
		{
			if (guildName == null) return null;
			lock (m_guilds.SyncRoot)
			{
				return (Guild)m_guilds[guildName];
			}
		}

		/// <summary>
		/// Returns a guild according to the matching database ID.
		/// </summary>
		/// <returns>Guild</returns>
		public static Guild GetGuildByGuildID(string guildid)
		{
			if(guildid == null) return null;
			
			lock (m_guildids.SyncRoot)
			{
				if(m_guildids[guildid] == null) return null;
				
				lock(m_guilds.SyncRoot)
				{
					return (Guild)m_guilds[m_guildids[guildid]];
				}
			}
		}

		/// <summary>
		/// Returns a database ID for a matching guild name.
		/// </summary>
		/// <returns>Guild</returns>
		public static string GuildNameToGuildID(string guildName)
		{
			Guild g = GetGuildByName(guildName);
			if (g == null)
				return "";
			return g.GuildID;
		}

		/// <summary>
		/// Returns a list of guilds by their status
		/// </summary>
		/// <returns>ArrayList of guilds</returns>
		public static ICollection ListGuild()
		{
			return m_guilds.Values;
		}

		/// <summary>
		/// Load all guilds and alliances from the database
		/// </summary>
		public static bool LoadAllGuilds()
		{
			lock (m_guilds.SyncRoot)
			{
				m_guilds.Clear(); //clear guild list before loading!
			}
			m_lastID = 0;

			//load guilds
			var guildObjs = GameServer.Database.SelectAllObjects<DBGuild>();
			foreach(var obj in guildObjs)
			{
				var myguild = new Guild(obj);
				AddGuild(myguild);

				if (obj.Ranks.Length == 0)
					CreateRanks(myguild);

				var guildCharacters = GameServer.Database.SelectObjects<Character>(string.Format("GuildID = '" + GameServer.Database.Escape(myguild.GuildID) + "'"));
				var tempList = new SortedList<string, SocialWindowMember>(guildCharacters.Count);

				foreach (Character ch in guildCharacters)
				{
					var member = new SocialWindowMember(ch.Name, ch.Level.ToString(), ch.Class.ToString(), ch.GuildRank.ToString(), "0", ch.LastPlayed.ToShortDateString(), ch.GuildNote);
					tempList.Add(ch.Name, member);
				}

				m_guildXAllPlayers.Add(myguild.GuildID, tempList);
			}

			//load alliances
			var allianceObjs = GameServer.Database.SelectAllObjects<DBAlliance>();
			foreach (DBAlliance dball in allianceObjs)
			{
				var myalliance = new Alliance();
				myalliance.LoadFromDatabase(dball);

				if (dball != null && dball.DBguilds != null)
				{
					foreach (DBGuild mydbgui in dball.DBguilds)
					{
						var gui = GetGuildByName(mydbgui.GuildName);
						myalliance.Guilds.Add(gui);
						gui.alliance = myalliance;
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Save all guild into database
		/// </summary>
		public static void SaveAllGuilds()
		{
			if (log.IsDebugEnabled)
				log.Debug("Saving all guilds...");
			try
			{
				lock (m_guilds.SyncRoot)
				{
					foreach (Guild g in m_guilds.Values)
					{
						g.SaveIntoDatabase();
					}
				}
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("Error saving guilds.", e);
			}
		}

		/// <summary>
		/// Returns true if a guild is using the emblem
		/// </summary>
		/// <param name="emblem"></param>
		/// <returns></returns>
		public static bool IsEmblemUsed(int emblem)
		{
			lock (m_guilds.SyncRoot)
			{
				foreach (Guild guild in m_guilds.Values)
				{
					if (guild.Emblem == emblem)
						return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Process for changing an emblem
		/// </summary>
		/// <param name="player"></param>
		/// <param name="oldemblem"></param>
		/// <param name="newemblem"></param>
		public static void ChangeEmblem(GamePlayer player, int oldemblem, int newemblem)
		{
			player.Guild.Emblem = newemblem;
			if (oldemblem != 0)
			{
				player.RemoveMoney(COST_RE_EMBLEM, null);
				var objs = GameServer.Database.SelectObjects<InventoryItem>("Emblem = " + GameServer.Database.Escape(oldemblem.ToString()));
				
				foreach (InventoryItem item in objs)
				{
					item.Emblem = newemblem;
					GameServer.Database.SaveObject(item);
				}
			}
		}

		public static List<Guild> GetAllGuilds()
		{
			var guilds = new List<Guild>(m_guilds.Count);
			lock (m_guilds.SyncRoot)
			{
				foreach (Guild guild in m_guilds.Values)
				{
					guilds.Add(guild);
				}
			}

			return guilds;
		}

		public class SocialWindowMember
		{
			#region Members
			string m_name;
			public string Name
			{
				get { return m_name; }
			} 
			string m_level;
			public string Level
			{
				get { return m_level; }
				set { m_level = value; } 
			} 
			string m_characterClassID;
			public string ClassID
			{
				get { return m_characterClassID; }
				set { m_characterClassID = value; }
			}
			string m_rank;
			public string Rank
			{
				get { return m_rank; }
				set { m_rank = value; }
			}
			string m_group = "0";
			public string GroupSize
			{
				get { return m_group; }
				set { m_group = value; }
			}
			string m_zoneOnline;
			public string ZoneOrOnline
			{
				get { return m_zoneOnline; }
				set { m_zoneOnline = value; }
			}
			string m_guildNote = "";
			public string Note
			{
				get { return m_guildNote; }
				set { m_guildNote = value; }
			}
			#endregion

			public string this[int i]
			{
				get
				{
					switch ((eSocialWindowIndex)i)
					{
						case eSocialWindowIndex.Name:
							return Name;
						case eSocialWindowIndex.ClassID:
							return ClassID;
						case eSocialWindowIndex.Group:
							return GroupSize;
						case eSocialWindowIndex.Level:
							return Level;
						case eSocialWindowIndex.Note:
							return Note;
						case eSocialWindowIndex.Rank:
							return Rank;
						case eSocialWindowIndex.ZoneOrOnline:
							return ZoneOrOnline;
						default:
							return "";
					}
				}
			}

			public SocialWindowMember(string name, string level, string classID, string rank, string group, string zoneOrOnline, string note)
			{
				m_name = name;
				m_level = level;
				m_characterClassID = classID;
				m_rank = rank;
				m_group = group;
				m_zoneOnline = zoneOrOnline;
				m_guildNote = note;
			}

            public SocialWindowMember(GamePlayer player)
            {
                m_name = player.Name;
				m_level = player.Level.ToString();
				m_characterClassID = player.CharacterClass.ID.ToString();
                m_rank = player.GuildRank.RankLevel.ToString(); ;
				m_group = player.Group == null ? "1" : player.Group.MemberCount.ToString();
				m_zoneOnline = player.CurrentZone.ToString();
				m_guildNote = player.GuildNote;
            }


			public string ToString(int position, int guildPop)
			{
				return string.Format("E,{0},{1},{2},{3},{4},{5},{6},\"{7}\",\"{8}\"",
					position, guildPop, m_name, m_level, m_characterClassID, m_rank, m_group, m_zoneOnline, m_guildNote);
			}

			public void UpdateMember(GamePlayer player)
			{
				Level = player.Level.ToString();
				ClassID = player.CharacterClass.ID.ToString();
				Rank = player.GuildRank.RankLevel.ToString();
				GroupSize = player.Group == null ? "1" : player.Group.MemberCount.ToString();
				Note = player.GuildNote;
				ZoneOrOnline = player.CurrentZone.Description;
			}

			public enum eSocialWindowSort : int
			{
				NameDesc = -1,
				NameAsc = 1,
				LevelDesc = -2,
				LevelAsc = 2,
				ClassDesc = -3,
				ClassAsc = 3,
				RankDesc = -4,
				RankAsc = 4,
				GroupDesc = -5,
				GroupAsc = 5,
				ZoneOrOnlineDesc = 6,
				ZoneOrOnlineAsc = -6,
				NoteDesc = 7,
				NoteAsc = -7
			}

			public enum eSocialWindowIndex : int
			{
				Name = 0,
				Level = 1,
				ClassID = 2,
				Rank = 3,
				Group = 4,
				ZoneOrOnline = 5,
				Note = 6
			}
		}
	}
}
