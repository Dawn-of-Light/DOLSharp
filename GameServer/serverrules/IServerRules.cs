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
using DOL.Database;
using DOL.GS.Styles;
using DOL.GS.Keeps;

namespace DOL.GS.ServerRules
{
	/// <summary>
	/// Interface for custom server rules
	/// </summary>
	public interface IServerRules
	{
		/// <summary>
		/// Allows or denies a client from connecting to the server ...
		/// NOTE: The client has not been fully initialized when this method is called.
		/// For example, no account or character data has been loaded yet.
		/// </summary>
		/// <param name="client">The client that sent the login request</param>
		/// <param name="username">The username of the client wanting to connect</param>
		/// <returns>true if connection allowed, false if connection should be terminated</returns>
		/// <remarks>You can only send ONE packet to the client and this is the
		/// LoginDenied packet before returning false. Trying to send any other packet
		/// might result in unexpected behaviour on server and client!</remarks>
		bool IsAllowedToConnect(GameClient client, string username);

		/// <summary>
		/// Is attacker allowed to attack defender.
		/// </summary>
		/// <param name="attacker">living that makes attack</param>
		/// <param name="defender">attacker's target</param>
		/// <param name="quiet">should messages be sent</param>
		/// <returns>true if attack is allowed</returns>
		bool IsAllowedToAttack(GameLiving attacker, GameLiving defender, bool quiet);

		/// <summary>
		/// Is caster allowed to cast a spell
		/// </summary>
		/// <param name="caster"></param>
		/// <param name="target"></param>
		/// <param name="spell"></param>
		/// <param name="spellLine"></param>
		/// <returns>true if allowed</returns>
		bool IsAllowedToCastSpell(GameLiving caster, GameLiving target, Spell spell, SpellLine spellLine);

		/// <summary>
		/// Does source considers target "friendly".
		/// Used for spells with "Realm" and "Group" spell types, friend list.
		/// </summary>
		/// <param name="source">spell source, considering object</param>
		/// <param name="target">spell target, considered object</param>
		/// <param name="quiet"></param>
		/// <returns></returns>
		bool IsSameRealm(GameLiving source, GameLiving target, bool quiet);

		/// <summary>
		/// Does the server type allows to play/create characters in all realms on one account
		/// </summary>
		/// <param name="client"></param>
		/// <returns></returns>
		bool IsAllowedCharsInAllRealms(GameClient client);

		/// <summary>
		/// Is source allowed to group target.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="target"></param>
		/// <param name="quiet"></param>
		/// <returns></returns>
		bool IsAllowedToGroup(GamePlayer source, GamePlayer target, bool quiet);

		/// <summary>
		/// Is source allowed to trade with target.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="target"></param>
		/// <param name="quiet"></param>
		/// <returns></returns>
		bool IsAllowedToTrade(GameLiving source, GameLiving target, bool quiet);

		/// <summary>
		/// Is target allowed to understand source.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		bool IsAllowedToUnderstand(GameLiving source, GamePlayer target);

		/// <summary>
		/// Is source allowed to speak.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="communicationType">type of communication used by the source: talk/yell/whisper</param>
		/// <returns></returns>
		bool IsAllowedToSpeak(GamePlayer source, string communicationType);

		/// <summary>
		/// Is player allowed to bind
		/// </summary>
		/// <param name="player"></param>
		/// <param name="point"></param>
		/// <returns></returns>
		bool IsAllowedToBind(GamePlayer player, BindPoint point);

		/// <summary>
		/// Is player allowed to make the item
		/// </summary>
		/// <param name="player"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		bool IsAllowedToCraft(GamePlayer player, ItemTemplate item);

		/// <summary>
		/// Is this player allowed to move to this region
		/// </summary>
		/// <param name="player">The player trying to move</param>
		/// <param name="region">The region trying to be moved to</param>
		/// <returns></returns>
		bool IsAllowedToZone(GamePlayer player, Region region);

		/// <summary>
		/// Short description of server rules
		/// </summary>
		/// <returns></returns>
		string RulesDescription();

		/// <summary>
		/// Check if living can take fall damage
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		bool CanTakeFallDamage(GamePlayer player);

		/// <summary>
		/// Experience needed for specific level
		/// </summary>
		/// <param name="level"></param>
		/// <returns></returns>
		long GetExperienceForLevel(int level);

		/// <summary>
		/// Experience living is worth
		/// </summary>
		/// <param name="level">the level of living</param>
		/// <returns></returns>
		long GetExperienceForLiving(int level);

		/// <summary>
		/// Checks if living has ability to use items of this type
		/// </summary>
		/// <param name="living"></param>
		/// <param name="item"></param>
		/// <returns>true if player has ability to use item</returns>
		bool CheckAbilityToUseItem(GameLiving living, ItemTemplate item);

		/// <summary>
		/// Checks whether one object type is equal to another
		/// based on server type
		/// </summary>
		/// <param name="type1"></param>
		/// <param name="type2"></param>
		/// <returns>true if equals</returns>
		bool IsObjectTypesEqual(eObjectType type1, eObjectType type2);

		/// <summary>
		/// Get object specialization level based on server type
		/// </summary>
		/// <param name="player">player whom specializations are checked</param>
		/// <param name="objectType">object type</param>
		/// <returns>specialization in object or 0</returns>
		int GetObjectSpecLevel(GamePlayer player, eObjectType objectType);

		/// <summary>
		/// Get object specialization level based on server type
		/// </summary>
		/// <param name="player">player whom specializations are checked</param>
		/// <param name="objectType">object type</param>
		/// <returns>specialization in object or 0</returns>
		int GetBaseObjectSpecLevel(GamePlayer player, eObjectType objectType);

		/// <summary>
		/// Invoked on NPC death and deals out
		/// experience/realm points if needed
		/// </summary>
		/// <param name="killedNPC">npc that died</param>
		/// <param name="killer">killer</param>
		void OnNPCKilled(GameNPC killedNPC, GameObject killer);

		/// <summary>
		/// Invoked on Player death and deals out
		/// experience/realm points if needed
		/// </summary>
		/// <param name="killedPlayer">player that died</param>
		/// <param name="killer">killer</param>
		void OnPlayerKilled(GamePlayer killedPlayer, GameObject killer);

		/// <summary>
		/// Invoked on a livings death and deals out
		/// experience / rps if needed
		/// </summary>
		/// <param name="living">the living that died</param>
		/// <param name="killer"></param>
		void OnLivingKilled(GameLiving living, GameObject killer);

		/// <summary>
		/// Gets the Realm of an living for name text coloring
		/// </summary>
		/// <param name="player"></param>
		/// <param name="target"></param>
		/// <returns>byte code of realm</returns>
		byte GetLivingRealm(GamePlayer player, GameLiving target);

		/// <summary>
		/// Gets the player name based on server type
		/// </summary>
		/// <param name="source">The "looking" player</param>
		/// <param name="target">The considered player</param>
		/// <returns>The name of the target</returns>
		string GetPlayerName(GamePlayer source, GamePlayer target);

		/// <summary>
		/// Gets the player last name based on server type
		/// </summary>
		/// <param name="source">The "looking" player</param>
		/// <param name="target">The considered player</param>
		/// <returns>The last name of the target</returns>
		string GetPlayerLastName(GamePlayer source, GamePlayer target);

		/// <summary>
		/// Gets the player guild name based on server type
		/// </summary>
		/// <param name="source">The "looking" player</param>
		/// <param name="target">The considered player</param>
		/// <returns>The guild name of the target</returns>
		string GetPlayerGuildName(GamePlayer source, GamePlayer target);

        /// <summary>
        /// Gets the player Realmrank 12 or 13 title
        /// </summary>
        /// <param name="source">The "looking" player</param>
        /// <param name="target">The considered player</param>
        /// <returns>The Realmranktitle of the target</returns>
        string GetPlayerPrefixName(GamePlayer source, GamePlayer target); 

		/// <summary>
		/// Gets the server type color handling scheme
		/// 
		/// ColorHandling: this byte tells the client how to handle color for PC and NPC names (over the head) 
		/// 0: standard way, other realm PC appear red, our realm NPC appear light green 
		/// 1: standard PvP way, all PC appear red, all NPC appear with their level color 
		/// 2: Same realm livings are friendly, other realm livings are enemy; nearest friend/enemy buttons work
		/// 3: standard PvE way, all PC friendly, realm 0 NPC enemy rest NPC appear light green 
		/// 4: All NPC are enemy, all players are friendly; nearest friend button selects self, nearest enemy don't work at all
		/// </summary>
		/// <param name="client">The client asking for color handling</param>
		/// <returns>The color handling</returns>
		byte GetColorHandling(GameClient client);
		
		/// <summary>
		/// Formats player statistics.
		/// </summary>
		/// <param name="player">The player to read statistics from.</param>
		/// <returns>List of strings.</returns>
		IList FormatPlayerStatistics(GamePlayer player);

		/// <summary>
		/// Reset the keep with special server rules handling
		/// </summary>
		/// <param name="lord">The lord that was killed</param>
		/// <param name="killer">The lord's killer</param>
		void ResetKeep(GuardLord lord, GameObject killer);

		/// <summary>
		/// Is the player allowed to generate news
		/// </summary>
		/// <param name="type">the type of news</param>
		/// <param name="player">the player</param>
		/// <returns>true if the player is allowed to generate news</returns>
		bool CanGenerateNews(GamePlayer player);

		/// <summary>
		/// Is the player allowed to /level
		/// </summary>
		/// <param name="player">The player</param>
		/// <returns>True if the player can use /level</returns>
		bool CountsTowardsSlashLevel(Character player);
	}
}
