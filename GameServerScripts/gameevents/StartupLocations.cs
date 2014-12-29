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
/*
 * Author:	code by Noret, locations by Avatarius
 * Date:	19.07.2004
 * This script should be put in /scripts/gameevents directory.
 * This event script changes starting location of created characters
 * based on region, class and race
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;

using DOL.Database;
using DOL.Events;
using DOL.GS;

namespace DOL.GS.GameEvents
{
	/// <summary>
	/// Moves new created Characters to the starting location based on region, class and race
	/// </summary>
	public static class StartupLocations
	{
		/// <summary>
		/// Declare a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Cached DB Startup Location
		/// </summary>
		private static readonly List<StartupLocation> m_cachedLocations = new List<StartupLocation>();

		/// <summary>
		/// Current Game Request Tutorial Region ID.
		/// </summary>
		private const int TUTORIAL_REGIONID = 27;
		
		[ScriptLoadedEvent]
		public static void OnScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			GameEventMgr.AddHandler(DatabaseEvent.CharacterCreated, new DOLEventHandler(CharacterCreation));
			
			foreach (var obj in GameServer.Database.SelectAllObjects<StartupLocation>())
				m_cachedLocations.Add(obj);
			
			if (log.IsInfoEnabled)
				log.Info("StartupLocations initialized");
		}

		[ScriptUnloadedEvent]
		public static void OnScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			GameEventMgr.RemoveHandler(DatabaseEvent.CharacterCreated, new DOLEventHandler(CharacterCreation));
		}

		/// <summary>
		/// Change location on character creation
		/// </summary>
		public static void CharacterCreation(DOLEvent ev, object sender, EventArgs args)
		{
			// Check Args
			var chArgs = args as CharacterEventArgs;
			
			if (chArgs == null)
				return;
			
			DOLCharacters ch = chArgs.Character;
			
			try
			{
				
				var availableLocation = GetAllStartupLocationForCharacter(ch, chArgs.GameClient.Version);

				StartupLocation dbStartupLocation = null;
				
				// get the first entry according to Tutorial Enabling.
				foreach (var location in availableLocation)
				{
					if (ServerProperties.Properties.DISABLE_TUTORIAL && location.ClientRegionID == TUTORIAL_REGIONID)
						continue;
					
					dbStartupLocation = location;
					break;
				}
				
				if (dbStartupLocation == null)
				{
					log.WarnFormat("startup location not found: account={0}; char name={1}; region={2}; realm={3}; class={4} ({5}); race={6} ({7}); version={8}",
					             ch.AccountName, ch.Name, ch.Region, ch.Realm, ch.Class, (eCharacterClass) ch.Class, ch.Race, (eRace)ch.Race, chArgs.GameClient.Version);
				}
				else
				{
					ch.Xpos = dbStartupLocation.XPos;
					ch.Ypos = dbStartupLocation.YPos;
					ch.Zpos = dbStartupLocation.ZPos;
					ch.Region = dbStartupLocation.Region;
					ch.Direction = dbStartupLocation.Heading;
					BindCharacter(ch);
				}				
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.ErrorFormat("StartupLocations script: error changing location. account={0}; char name={1}; region={2}; realm={3}; class={4} ({5}); race={6} ({7}); version={8}; {9}",
					                ch.AccountName, ch.Name, ch.Region, ch.Realm, ch.Class, (eCharacterClass) ch.Class, ch.Race, (eRace)ch.Race, chArgs.GameClient.Version, e);
			}
		}
		
		public static IList<StartupLocation> GetAllStartupLocationForCharacter(DOLCharacters ch, GameClient.eClientVersion cli)
		{
			var tmp = m_cachedLocations.Where(sl => sl.MinVersion <= (int)cli)
				.Where(sl => sl.ClassID == 0 || sl.ClassID == ch.Class)
				.Where(sl => sl.RaceID == 0 || sl.RaceID == ch.Race)
				.Where(sl => sl.RealmID == 0 || sl.RealmID == ch.Realm)
				.Where(sl => sl.ClientRegionID == 0 || sl.ClientRegionID == ch.Region)
				.OrderByDescending(sl => sl.MinVersion).ThenByDescending(sl => sl.ClientRegionID)
				.ThenByDescending(sl => sl.RealmID).ThenByDescending(sl => sl.ClassID)
				.ThenByDescending(sl => sl.RaceID).ToList();
			
			foreach(var it in tmp)
				log.InfoFormat("Location for {0} - {1}", ch.Name, it.StartupLoc_ID);
			
			return tmp;
		}
		
		public static StartupLocation GetNonTutorialLocation(GamePlayer player)
		{
			return GetAllStartupLocationForCharacter(player.DBCharacter, player.Client.Version).First(sl => sl.ClientRegionID != TUTORIAL_REGIONID);
		}

		/// <summary>
		/// Binds character to current location
		/// </summary>
		/// <param name="ch"></param>
		public static void BindCharacter(DOLCharacters ch)
		{
			ch.BindRegion = ch.Region;
			ch.BindHeading = ch.Direction;
			ch.BindXpos = ch.Xpos;
			ch.BindYpos = ch.Ypos;
			ch.BindZpos = ch.Zpos;
		}
	}
}
