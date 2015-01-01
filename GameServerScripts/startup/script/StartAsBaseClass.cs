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
	/// Startup Script to Revert Player Class to Base Class.
	/// </summary>
	public static class StartAsBaseClass
	{
		/// <summary>
		/// Should the server start characters as Base Class?
		/// </summary>
		[ServerProperty("startup", "start_as_base_class", "Should we start all players as their base class? true if yes (e.g. Armsmen become Fighters on Creation)", false)]
		public static bool START_AS_BASE_CLASS;

		/// <summary>
		/// Register Character Creation Events
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		[GameServerStartedEvent]
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
		[GameServerStoppedEvent]
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
			// Only act if enabled.
			if (!START_AS_BASE_CLASS)
				return;
			
			// Check Args
			var chArgs = args as CharacterEventArgs;
			
			if (chArgs == null)
				return;
			
			DOLCharacters ch = chArgs.Character;

			// Revert to Base Class.
			var chClass = ScriptMgr.FindCharacterBaseClass(ch.Class);
			
			if (chClass != null && chClass.ID != ch.Class)
				ch.Class = chClass.ID;
		}
		
	}
}
