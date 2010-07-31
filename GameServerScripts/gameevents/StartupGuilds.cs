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
using System.Reflection;
using DOL.Database;
using DOL.Events;
using log4net;
using DOL.Language;

namespace DOL.GS.GameEvents
{
	/// <summary>
	/// This class makes sure that all the startup guilds are created in the database
	/// </summary>
	public class StartupGuilds
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// This method runs the checks
		/// </summary>
		/// <param name="e">The event</param>
		/// <param name="sender">The sender</param>
		/// <param name="args">The arguments</param>
		[ScriptLoadedEvent]
		public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
		{
			foreach (eRealm currentRealm in Enum.GetValues(typeof(eRealm)))
			{
				if (currentRealm == eRealm.None ||currentRealm == eRealm.Door)
					continue;
				CheckGuild(currentRealm,LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Guild.StartupGuild."+GlobalConstants.RealmToName (currentRealm)));
			}
		}

		/// <summary>
		/// This method checks if a guild exists
		/// if not, the guild is created with default values
		/// </summary>
		/// <param name="guildName">The guild name that is being checked</param>
		private static void CheckGuild(eRealm currentRealm, string guildName)
		{
			if (!GuildMgr.DoesGuildExist(guildName))
			{
				Guild newguild = GuildMgr.CreateGuild(currentRealm, guildName);
				newguild.Ranks[8].OcHear = true;
				newguild.Motd = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE,"Guild.StartupGuild.Motd");
				newguild.Omotd = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE,"Guild.StartupGuild.Omotd");
				newguild.Ranks[8].Title =  LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE,"Guild.StartupGuild.Title");
			}
		}
	}
}