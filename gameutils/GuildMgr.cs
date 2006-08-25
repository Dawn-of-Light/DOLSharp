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
using System.Collections.Specialized;
using System.Reflection;
using DOL.Database;
using DOL.Database.DataAccessInterfaces;
using DOL.Database.DataTransferObjects;
using DOL.Events;
using DOL.GS.PacketHandler;
using NHibernate.Expression;
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

		#region Declarations
		/// <summary>
		/// ArrayList of all guilds in the game
		/// </summary>
		static private readonly IList<Guild> m_allGuilds = new List<Guild>();

		/// <summary>
		/// The cost in copper to reemblem the guild
		/// </summary>
		public const long COST_RE_EMBLEM = 2000000; //200 gold

		/// <summary>
		/// Returns a list of all guilds
		/// </summary>
		/// <returns>ArrayList of guilds</returns>
		public static IList<Guild> AllGuilds
		{
			get { return m_allGuilds; }
		}
		#endregion

		#region CreateGuild / LoadAllGuilds

		/// <summary>
		/// Creates a new guild
		/// </summary>
		/// <returns>GuildEntry</returns>
		public static Guild CreateGuild(string guildName)
		{
			try
			{	
				if(GetGuildByName(guildName) != null) return null; // Another guild with that name
			
				if (log.IsDebugEnabled)
					log.Debug("Creating new guild ("+guildName+")");

				Guild newguild = new Guild();
				newguild.GuildName = guildName;
				newguild.BountyPoints = 0;
				newguild.RealmPoints = 0;
				newguild.MeritPoints = 0;
				newguild.Due = false;
				newguild.TotalMoney = 0;
				newguild.Level = 0;
				newguild.Emblem = 0;
				newguild.Motd = "Welcome to the new guild "+newguild.GuildName+".";
				newguild.OMotd = "";
				newguild.Webpage = "";
				newguild.Email = "";
				newguild.Alliance = new Alliance();			// create a new empty alliance
				newguild.Alliance.AMotd = "Your guild have no alliances.";
				newguild.Alliance.AllianceLeader = newguild;
				newguild.Alliance.AllianceGuilds.Add(newguild);

				GameServer.Database.AddNewObject(newguild);	// save the new guild and its empty alliance
				
				m_allGuilds.Add(newguild);

				return newguild;
			}
			catch(Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("CreateGuild", e);
				return null;
			}
		}

		/// <summary>
		/// Load all guilds and alliances from the database
		/// </summary>
		public static bool LoadAllGuilds()
		{
			IList<GuildEntity> guilds = GameServer.DatabaseNew.Using<IGuildDao>().SelectAll();

			foreach (GuildEntity dbGuild in guilds)
			{
				Guild myguild = new Guild(dbGuild);// not ok because missing alliance to load 
				if (!m_allGuilds.Contains(myguild))
				{
					m_allGuilds.Add(myguild);
				}
			}

			IList<AllianceEntity> alliances = GameServer.DatabaseNew.Using<IAllianceDao>().SelectAll();
			IDictionary<int, Alliance> allianceList = new Dictionary<int, Alliance>();
			foreach (AllianceEntity dballi in alliances)
			{
				Alliance myalli = new Alliance(dballi); // ok here guild ever loaded
				allianceList.Add(dballi.Id, myalli);
			}

			//second pass to put alliance
			foreach (GuildEntity dbGuild in guilds)
			{
				Guild myguilde = GetGuildById(dbGuild.Id);
				myguilde.Alliance = allianceList[dbGuild.Alliance];
			}

			return true;
		}
		#endregion

		#region GetGuildByName
		/// <summary>
		/// Returns a guild according to the matching name
		/// </summary>
		/// <returns>Guild</returns>
		public static Guild GetGuildByName(string guildName)
		{
			foreach (Guild guild in m_allGuilds)
			{
				if (guild.GuildName == guildName)
					return guild;
			}
			return null;
		}
		#endregion

		#region Guild emblem functions
		/// <summary>
		/// Returns true if a guild is using the emblem
		/// </summary>
		/// <param name="emblem"></param>
		/// <returns></returns>
		public static bool IsEmblemUsed(ushort emblem)
		{
			foreach (Guild guild in m_allGuilds)
			{
				if (guild.Emblem == emblem)
					return true;
			}
			return false;
		}

		/// <summary>
		/// Process for changing an emblem
		/// </summary>
		/// <param name="player"></param>
		/// <param name="oldemblem"></param>
		/// <param name="newemblem"></param>
		public static void ChangeEmblem(GamePlayer player, ushort oldemblem, ushort newemblem)
		{
			player.Guild.Emblem = newemblem;
			GameServer.Database.SaveObject(player.Guild);
			if (oldemblem != 0)
			{
				player.RemoveMoney(COST_RE_EMBLEM);
			}
		}
		#endregion

		public static Guild GetGuildById(int index)
		{
			foreach (Guild guild in m_allGuilds)
			{
				if (guild.GuildID == index)
					return guild;
			}
			return null;
		}
	}
}
