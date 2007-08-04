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
		/// ArrayList of all guilds in the game
		/// </summary>
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
			if (log.IsDebugEnabled)
				log.Debug("Create guild; guild name=\"" + guildName + "\"");
			try
			{
				// Does guild exist, if so return null
				if (DoesGuildExist(guildName) == true)
				{
					if (creator != null)
						creator.Out.SendMessage(guildName + " already exists!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return null;
				}

				// Check if client exists
				if (creator == null)
					return null;


				//create table of rank in guild
				Guild newguild = new Guild();
				newguild.theGuildDB = new DBGuild();
				newguild.Name = guildName;
				newguild.theGuildDB.GuildName = guildName;
				// we save the guild so we can have a guild id.
				GameServer.Database.AddNewObject(newguild.theGuildDB);
				//newguild.GuildID = System.Guid.NewGuid().ToString(); //Assume this is unique, which I don't like, but it seems to be commonly used elsewhere in the code.
				//newguild.theGuildDB.GuildID = newguild.GuildID;
				CreateRanks(newguild);
				
				AddGuild(newguild);				
				//GameServer.Database.AddNewObject(newguild.theGuildDB);
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
				rank.Title = "";
				rank.Upgrade = false;
				rank.View = false;

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

												}

											}

										}

									}

								}

							}

						}

					}

				}
				GameServer.Database.AddNewObject(rank);
				GameServer.Database.SaveObject(rank);
				newguild.theGuildDB.Ranks[i] = rank;
			}
		}

		/// <summary>
		/// Delete's a guild
		/// </summary>
		/// <returns>true or false</returns>
		public static bool DeleteGuild(string guildName)
		{
			try
			{
				Guild removeGuild = GetGuildByName(guildName);
				// Does guild exist, if not return null
				if (removeGuild == null)
				{
					return false;
				}

				DBGuild[] guilds = (DBGuild[])GameServer.Database.SelectObjects(typeof(DBGuild), "GuildName='" + GameServer.Database.Escape(guildName) + "'");
				foreach (DBGuild guild in guilds)
				{
					foreach (Character cha in GameServer.Database.SelectObjects(typeof(Character), "GuildID = '" + guild.ObjectId + "'"))
						cha.GuildID = 0;
					GameServer.Database.DeleteObject(guild);
				}

				lock (removeGuild.ListOnlineMembers())
				{
					foreach (GamePlayer ply in removeGuild.ListOnlineMembers())
					{
						ply.Guild = null;
						ply.GuildID = 0;
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
		public static Guild GetGuildByGuildID(uint guildid)
		{
			if(guildid == 0) return null;
			
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
		public static uint GuildNameToGuildID(string guildName)
		{
			Guild g = GetGuildByName(guildName);
			if (g == null)
				return 0;
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
			DataObject[] objs = GameServer.Database.SelectAllObjects(typeof(DBGuild));
			foreach (DataObject obj in objs)
			{
				Guild myguild = new Guild();
				myguild.LoadFromDatabase(obj);
				AddGuild(myguild);
				if (((DBGuild)obj).Ranks.Length == 0)
					CreateRanks(myguild);
			}

			//load alliances
			objs = GameServer.Database.SelectAllObjects(typeof(DBAlliance));
			foreach (DBAlliance dball in objs)
			{
				Alliance myalliance = new Alliance();
				myalliance.LoadFromDatabase(dball);
				if (dball != null && dball.DBguilds != null)
				{
					foreach (DBGuild mydbgui in dball.DBguilds)
					{
						Guild gui = GetGuildByName(mydbgui.GuildName);
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
					if (guild.theGuildDB.Emblem == emblem)
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
			player.Guild.theGuildDB.Emblem = newemblem;
			GameServer.Database.SaveObject(player.Guild.theGuildDB);
			if (oldemblem != 0)
			{
				player.RemoveMoney(COST_RE_EMBLEM, null);
				DataObject[] objs = GameServer.Database.SelectObjects(typeof(InventoryItem), "Emblem = " + GameServer.Database.Escape(oldemblem.ToString()));
				foreach (InventoryItem item in objs)
				{
					item.Emblem = newemblem;
					GameServer.Database.SaveObject(item);
				}
			}
		}

		public static ArrayList GetAllGuilds()
		{
			ArrayList guilds = new ArrayList();
			lock (m_guilds.SyncRoot)
			{
				foreach (Guild guild in m_guilds.Values)
				{
					guilds.Add(guild);
				}
			}
			return guilds;
		}
	}
}
