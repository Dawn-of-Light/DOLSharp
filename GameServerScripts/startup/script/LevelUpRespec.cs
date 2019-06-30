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
using System.Reflection;

using log4net;

using DOL.Events;
using DOL.GS.ServerProperties;
using DOL.GS.PacketHandler;

namespace DOL.GS.GameEvents
{
	/// <summary>
	/// Class Handling Respec Granting on player Level Up.
	/// </summary>
	public static class LevelUpRespec
	{		
		/// <summary>
		/// Declare a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
		/// <summary>
		/// What levels did we allow a DOL respec ? serialized
		/// </summary>
		[ServerProperty("startup", "give_dol_respec_at_level", "What levels does we give a DOL respec ? separated by a semi-colon or a range with a dash (ie 1-5;7;9)", "0")]
		public static string GIVE_DOL_RESPEC_AT_LEVEL;
		
		[ScriptLoadedEvent]
		public static void OnScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			GameEventMgr.AddHandler(GamePlayerEvent.LevelUp, new DOLEventHandler(OnLevelUp));
			
			if (log.IsInfoEnabled)
				log.Info("Level Up Respec Gift initialized");
		}

		[ScriptUnloadedEvent]
		public static void OnScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			GameEventMgr.RemoveHandler(GamePlayerEvent.LevelUp, new DOLEventHandler(OnLevelUp));
		}
		
		/// <summary>
		/// Level Up Event for Triggering Respec Gifts
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public static void OnLevelUp(DOLEvent e, object sender, EventArgs args)
		{
			var player = sender as GamePlayer;
			
			if (player == null)
				return;
			
			// Graveen: give a DOL respec on the GIVE_DOL_RESPEC_ON_LEVELS levels
			foreach (string str in Util.SplitCSV(GIVE_DOL_RESPEC_AT_LEVEL, true))
			{
				byte level_respec = 0;
				
				if(!byte.TryParse(str, out level_respec))
					level_respec = 0;

				if (player.Level == level_respec)
				{
					int oldAmount = player.RespecAmountDOL;
					player.RespecAmountDOL++;

					if (oldAmount != player.RespecAmountDOL)
					{
						player.Out.SendMessage(string.Format("As you reached level {0}, you are awarded a DOL (full) respec!", player.Level), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					}
				}
			}
			
			// Fixed Level Respecs
			switch (player.Level)
			{
					// full respec on level 5 since 1.70
				case 5:
					player.RespecAmountAllSkill++;
					player.IsLevelRespecUsed = false;
					break;
				case 6:
					if (player.IsLevelRespecUsed) break;
					player.RespecAmountAllSkill--;
					break;

					// single line respec
				case 20:
				case 40:
					{
						player.RespecAmountSingleSkill++; // Give character their free respecs at 20 and 40
						player.IsLevelRespecUsed = false;
						break;
					}
				case 21:
				case 41:
					{
						if (player.IsLevelRespecUsed) break;
						player.RespecAmountSingleSkill--; // Remove free respecs if it wasn't used
						break;
					}
			}
		}
	}
}
