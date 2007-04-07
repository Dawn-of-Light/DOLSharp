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
using DOL.GS;

namespace DOL.Events
{
	/// <summary>
	/// This class holds all possible player events.
	/// Only constants defined here!
	/// </summary>
	public class GamePlayerEvent : GameLivingEvent
	{
		/// <summary>
		/// Constructs a new GamePlayer event
		/// </summary>
		/// <param name="name">the event name</param>
		protected GamePlayerEvent(string name) : base(name)
		{
		}

		/// <summary>
		/// Tests if this event is valid for the specified object
		/// </summary>
		/// <param name="o">The object for which the event wants to be registered</param>
		/// <returns>true if valid, false if not</returns>
		public override bool IsValidFor(object o)
		{
			return o is GamePlayer;
		}

		/// <summary>
		/// The GameEntered event is fired whenever the player enters the game
		/// </summary>
		public static readonly GamePlayerEvent GameEntered = new GamePlayerEvent("GamePlayer.GameEntered");
		/// <summary>
		/// The RegionChanged event is fired whenever the player changes the region
		/// </summary>
		public static readonly GamePlayerEvent RegionChanged = new GamePlayerEvent("GamePlayer.RegionChanged");
		/// <summary>
		/// The Released event is fired whenever the player has released
		/// </summary>
		public static readonly GamePlayerEvent Released = new GamePlayerEvent("GamePlayer.Released");
		/// <summary>
		/// The Revive event is fired whenever the player is resurrected and at /release
		/// </summary>
		public static readonly GamePlayerEvent Revive = new GamePlayerEvent("GamePlayer.Revive");
		/// <summary>
		/// The Linkdeath event is fired whenever the player went linkdead
		/// </summary>
		public static readonly GamePlayerEvent Linkdeath = new GamePlayerEvent("GamePlayer.Linkdeath");
		/// <summary>
		/// The MoveItem event is fired whenever the player wants to give away an item
		/// </summary>
		public static readonly GamePlayerEvent GiveItem = new GamePlayerEvent("GamePlayer.GiveItem");
		/// <summary>
		/// The MoveItem event is fired whenever the player wants to give away an item
		/// </summary>
		public static readonly GamePlayerEvent GiveMoney = new GamePlayerEvent("GamePlayer.GiveMoney");
		/// <summary>
		/// The Quit event is fired whenever the player the player quits the game
		/// </summary>
		public static readonly GamePlayerEvent Quit = new GamePlayerEvent("GamePlayer.Quit");
		/// <summary>
		/// The UseSlot event is fired whenever the player tries to use the slot
		/// </summary>
		public static readonly GamePlayerEvent UseSlot = new GamePlayerEvent("GamePlayer.UseSlot");
		/// <summary>
		/// The StealthStateChanged event is fired whenever the player's stealth state changes
		/// </summary>
		public static readonly GamePlayerEvent StealthStateChanged = new GamePlayerEvent("GamePlayer.StealthStateChanged");
		/// <summary>
		/// The OnLevelSecondStage event is fired whenever the player gains experience for level second stage
		/// </summary>
		public static readonly GamePlayerEvent LevelSecondStage = new GamePlayerEvent("GamePlayer.LevelSecondStage");
		/// <summary>
		/// The OnLevelSecondStage event is fired whenever the player gains experience for next level
		/// </summary>
		public static readonly GamePlayerEvent LevelUp = new GamePlayerEvent("GamePlayer.LevelUp");
		/// <summary>
		/// The CommandNpcRelease event is fired whenever the player commands to release controlled NPC
		/// </summary>
		public static readonly GamePlayerEvent CommandNpcRelease = new GamePlayerEvent("GamePlayer.CommandNpcRelease");
		/// <summary>
        /// The AcceptQuest event is fired whenever the player accepts a quest offer generated via BaseQuestPart.AddAction(eActionType.OfferQuest)
        /// </summary>
        public static readonly GamePlayerEvent AcceptQuest = new GamePlayerEvent("GamePlayer.AcceptQuest");
        /// <summary>
        /// The DeclineQuest event is fired whenever the player declines a quest offer generated via BaseQuestPart.AddAction(eActionType.OfferQuest)
        /// </summary>
        public static readonly GamePlayerEvent DeclineQuest = new GamePlayerEvent("GamePlayer.DeclineQuest");
        /// <summary>
        /// The ContinueQuest event is fired whenever the player continues a quest abort generated via BaseQuestPart.AddAction(eActionType.OfferQuestAbort)
        /// </summary>
        public static readonly GamePlayerEvent ContinueQuest = new GamePlayerEvent("GamePlayer.ContinueQuest");
        /// <summary>
        /// The AbortQuest event is fired whenever the player aborts a quest generated via BaseQuestPart.AddAction(eActionType.OfferQuestAbort)
        /// </summary>
        public static readonly GamePlayerEvent AbortQuest = new GamePlayerEvent("GamePlayer.AbortQuest");
		/// <summary>
		/// The UseAbility event is fired whenever the player uses ability
		/// </summary>
		public static readonly GamePlayerEvent UseAbility = new GamePlayerEvent("GamePlayer.UseAbility");
		/// <summary>
		/// The RRLevelUp event is fired whenever the player gains a realm rank
		/// </summary>
		public static readonly GamePlayerEvent RRLevelUp = new GamePlayerEvent("GamePlayer.RRLevelUp");
		/// <summary>
		/// The RLLevelUp event is fired whenever the player gains a realm level
		/// </summary>
		public static readonly GamePlayerEvent RLLevelUp = new GamePlayerEvent("GamePlayer.RLLevelUp");

		#region Statistics

		/// <summary>
		/// The KillsTotalPlayersChanged event is fired when any of the KillsxxxPlayersChanged property changes.
		/// </summary>
		public static readonly GamePlayerEvent KillsTotalPlayersChanged = new GamePlayerEvent("GamePlayer.KillsTotalPlayersChanged");
		/// <summary>
		/// The KillsAlbionPlayersChanged event is fired when KillsAlbionPlayers property changes.
		/// </summary>
		public static readonly GamePlayerEvent KillsAlbionPlayersChanged = new GamePlayerEvent("GamePlayer.KillsAlbionPlayersChanged");
		/// <summary>
		/// The KillsMidgardPlayersChanged event is fired when KillsMidgardPlayers property changes.
		/// </summary>
		public static readonly GamePlayerEvent KillsMidgardPlayersChanged = new GamePlayerEvent("GamePlayer.KillsMidgardPlayersChanged");
		/// <summary>
		/// The KillsHiberniaPlayersChanged event is fired when KillsHiberniaPlayers property changes.
		/// </summary>
		public static readonly GamePlayerEvent KillsHiberniaPlayersChanged = new GamePlayerEvent("GamePlayer.KillsHiberniaPlayersChanged");
		/// <summary>
		/// The KillsTotalDeathBlowsChanged event is fired when KillsAlbionDeathBlows, KillsMidgardDeathBlows or KillsHiberniaDeathBlows properties changes.
		/// </summary>
		public static readonly GamePlayerEvent KillsTotalDeathBlowsChanged = new GamePlayerEvent("GamePlayer.KillsTotalDeathBlowsChanged");
		/// <summary>
		/// The KillsTotalSoloChanged event is fired when KillsAlbionSolo, KillsMidgardSolo or KillsHiberniaSolo properties changes.
		/// </summary>
		public static readonly GamePlayerEvent KillsTotalSoloChanged = new GamePlayerEvent("GamePlayer.KillsTotalSoloChanged");
		/// <summary>
		/// The CapturedKeepsChanged event is fired when CapturedKeeps properties changes.
		/// </summary>
		public static readonly GamePlayerEvent CapturedKeepsChanged = new GamePlayerEvent("GamePlayer.CapturedKeepsChanged");
		/// <summary>
		/// The CapturedTowersChanged event is fired when CapturedTowers properties changes.
		/// </summary>
		public static readonly GamePlayerEvent CapturedTowersChanged = new GamePlayerEvent("GamePlayer.CapturedTowersChanged");
		/// <summary>
		/// The KillsDragonChanged event is fired when KillsDragon properties changes.
		/// </summary>
		public static readonly GamePlayerEvent KillsDragonChanged = new GamePlayerEvent("GamePlayer.KillsDragonChanged"); 
		#endregion
	}
}
