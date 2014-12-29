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

using DOL.Events;
using DOL.GS.ServerProperties;
using DOL.Database;

namespace DOL.GS.GameEvents
{
	/// <summary>
	/// This class hold the Character Creation Custom Settings
	/// This is the best example on how to change Characters parameters on Creation.
	/// </summary>
	public static class CharacterCreationSettings
	{
		#region Properties
		/// <summary>
		/// The amount of Bounty Points a player starts with
		/// </summary>
		[ServerProperty("startup", "starting_bps", "Starting Bounty Points - Edit this to change the amount of Bounty Points the new characters start the game with", 0)]
		public static long STARTING_BPS;
		
		/// <summary>
		/// The amount of copper a player starts with
		/// </summary>
		[ServerProperty("startup", "starting_money", "Starting Money - Edit this to change the amount in copper of money new characters start the game with, max 214 plat", 0)]
		public static long STARTING_MONEY;
		
		/// <summary>
		/// The message players get when they enter the game at level 1
		/// </summary>
		[ServerProperty("startup", "starting_realm_level", "Starting Realm level - Edit this to set which realm level a new player starts the game with", 0)]
		public static int STARTING_REALM_LEVEL;
		
		/// <summary>
		/// The level of experience a player should start with
		/// </summary>
		[ServerProperty("startup", "starting_level", "Starting Level - Edit this to set which levels experience a new player start the game with", 1)]
		public static int STARTING_LEVEL;
		#endregion
		
		/// <summary>
		/// Register Character Creation Events
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		[ScriptLoadedEvent]
		public static void OnScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			GameEventMgr.AddHandler(DatabaseEvent.CharacterCreated, new DOLEventHandler(OnCharacterCreation));
		}
		
		/// <summary>
		/// Unregister Character Creation Events
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		[ScriptUnloadedEvent]
		public static void OnScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			GameEventMgr.RemoveHandler(DatabaseEvent.CharacterCreated, new DOLEventHandler(OnCharacterCreation));
		}
		
		/// <summary>
		/// Triggered When New Character is Created.
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public static void OnCharacterCreation(DOLEvent e, object sender, EventArgs args)
		{
			// Check Args
			var chArgs = args as CharacterEventArgs;
			
			if (chArgs == null)
				return;
			
			DOLCharacters ch = chArgs.Character;

			// Property BPS
			if (STARTING_BPS > 0)
				ch.BountyPoints = STARTING_BPS;
			
			// Property Money
			if (STARTING_MONEY > 0)
			{
				long value = STARTING_MONEY;
				ch.Copper = Money.GetCopper(value);
				ch.Silver = Money.GetSilver(value);
				ch.Gold = Money.GetGold(value);
				ch.Platinum = Money.GetPlatinum(value);
			}

			// Property Realm Level
			if (STARTING_REALM_LEVEL > 0)
			{
				int realmLevel = STARTING_REALM_LEVEL;
				long rpamount = 0;
				if (realmLevel < GamePlayer.REALMPOINTS_FOR_LEVEL.Length)
					rpamount = GamePlayer.REALMPOINTS_FOR_LEVEL[realmLevel];

				// thanks to Linulo from http://daoc.foren.4players.de/viewtopic.php?t=40839&postdays=0&postorder=asc&start=0
				if (rpamount == 0)
					rpamount = (long)(25.0 / 3.0 * (realmLevel * realmLevel * realmLevel) - 25.0 / 2.0 * (realmLevel * realmLevel) + 25.0 / 6.0 * realmLevel);

				ch.RealmPoints = rpamount;
				ch.RealmLevel = realmLevel;
			}
			
			// Property Starting Level
			if (STARTING_LEVEL > 1 && ch.Experience < GamePlayer.GetExperienceAmountForLevel(STARTING_LEVEL - 1))
			{
				ch.Experience = GamePlayer.GetExperienceAmountForLevel(STARTING_LEVEL - 1);
				ch.Level = STARTING_LEVEL;
			}


			// Default 2 Respec Realm Skill
			ch.RespecAmountRealmSkill += 2;
		}
	}
}