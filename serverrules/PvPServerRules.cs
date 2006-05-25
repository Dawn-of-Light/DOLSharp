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
using DOL;
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS;
using DOL.GS.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS.ServerRules
{
	/// <summary>
	/// Set of rules for "PvP" server type.
	/// </summary>
	[ServerRules(eGameServerType.GST_PvP)]
	public class PvPServerRules : AbstractServerRules
	{
		public override string RulesDescription()
		{
			return "standard PvP server rules";
		}

		//release city
		//alb=26315, 21177, 8256, dir=0
		//mid=24664, 21402, 8759, dir=0
		//hib=15780, 22727, 7060, dir=0
		//0,"You will now release automatically to your home city in 8 more seconds!"

		//TODO: 2min immunity after release if killed by player

		/// <summary>
		/// Constructor
		/// </summary>
		public PvPServerRules()
		{
			GameEventMgr.AddHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(OnGameEntered));
			GameEventMgr.AddHandler(GamePlayerEvent.RegionChanged, new DOLEventHandler(OnRegionChanged));
			GameEventMgr.AddHandler(GamePlayerEvent.Released, new DOLEventHandler(OnReleased));
			m_invExpiredCallback = new GamePlayer.InvulnerabilityExpiredCallback(ImmunityOverCallback);
		}

		#region PvP Immunity

		/// <summary>
		/// TempProperty set if killed by player
		/// </summary>
		protected const string KILLED_BY_PLAYER_PROP = "PvP killed by player";

		/// <summary>
		/// Level at which players safety flag has no effect
		/// </summary>
		protected int m_safetyLevel = 10;

		/// <summary>
		/// Called when player enters the game for first time
		/// </summary>
		/// <param name="e">event</param>
		/// <param name="sender">GamePlayer object that has entered the game</param>
		/// <param name="args"></param>
		public virtual void OnGameEntered(DOLEvent e, object sender, EventArgs args)
		{
			SetImmunity((GamePlayer)sender, 10*1000); //10sec immunity
		}

		/// <summary>
		/// Called when player has changed the region
		/// </summary>
		/// <param name="e">event</param>
		/// <param name="sender">GamePlayer object that has changed the region</param>
		/// <param name="args"></param>
		public virtual void OnRegionChanged(DOLEvent e, object sender, EventArgs args)
		{
			SetImmunity((GamePlayer)sender, 30*1000); //30sec immunity
		}

		/// <summary>
		/// Called after player has released
		/// </summary>
		/// <param name="e">event</param>
		/// <param name="sender">GamePlayer that has released</param>
		/// <param name="args"></param>
		public virtual void OnReleased(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = (GamePlayer)sender;
			if(player.TempProperties.getObjectProperty(KILLED_BY_PLAYER_PROP, null) != null)
			{
				player.TempProperties.removeProperty(KILLED_BY_PLAYER_PROP);
				SetImmunity(player, 2*60*1000); //2min immunity if killed by a player
			}
			else
			{
				SetImmunity(player, 30*1000); //30sec immunity if killed by a mob
			}
		}

		/// <summary>
		/// Sets PvP immunity for a player and starts the timer if needed
		/// </summary>
		/// <param name="player">player that gets immunity</param>
		/// <param name="duration">amount of milliseconds when immunity ends</param>
		public virtual void SetImmunity(GamePlayer player, int duration)
		{
			// left for compatibility
			player.SetPvPInvulnerability(duration, m_invExpiredCallback);
		}

		/// <summary>
		/// Holds the delegate called when PvP invulnerability is expired
		/// </summary>
		protected GamePlayer.InvulnerabilityExpiredCallback m_invExpiredCallback;
		
		/// <summary>
		/// Removes PvP immunity from the players
		/// </summary>
		/// <player></player>
		public virtual void ImmunityOverCallback(GamePlayer player)
		{
			if (player.ObjectState != eObjectState.Active) return;
			if (player.Client.IsPlaying == false) return;

			if(player.Level < m_safetyLevel && player.SafetyFlag)
				player.Out.SendMessage("Your temporary pvp invulnerability timer has expired, but your safety flag is still on.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			else
				player.Out.SendMessage("Your temporary pvp invulnerability timer has expired.", eChatType.CT_System, eChatLoc.CL_SystemWindow);

			return;
		}

		/// <summary>
		/// Invoked on Player death and deals out
		/// experience/realm points if needed
		/// </summary>
		/// <param name="killedPlayer">player that died</param>
		/// <param name="killer">killer</param>
		public override void OnPlayerKilled(GamePlayer killedPlayer, GameObject killer)
		{
			base.OnPlayerKilled(killedPlayer, killer);
			if(killer == null || killer is GamePlayer)
				killedPlayer.TempProperties.setProperty(KILLED_BY_PLAYER_PROP, KILLED_BY_PLAYER_PROP);
			else
				killedPlayer.TempProperties.removeProperty(KILLED_BY_PLAYER_PROP);
		}

		#endregion

		/// <summary>
		/// Regions where players can't be attacked
		/// </summary>
		/*protected int[] m_safeRegions =
		{
			10,  //City of Camelot
			101, //Jordheim
			201, //Tir Na Nog

			2,   //Albion Housing
			102, //Midgard Housing
			202, //Hibernia Housing

			//No PVP Dungeons: http://support.darkageofcamelot.com/cgi-bin/support.cfg/php/enduser/std_adp.php?p_sid=frxnPUjg&p_lva=&p_refno=020709-000000&p_created=1026248996&p_sp=cF9ncmlkc29ydD0mcF9yb3dfY250PTE0JnBfc2VhcmNoX3RleHQ9JnBfc2VhcmNoX3R5cGU9MyZwX2NhdF9sdmwxPTI2JnBfY2F0X2x2bDI9fmFueX4mcF9zb3J0X2J5PWRmbHQmcF9wYWdlPTE*&p_li
			21,  //Tomb of Mithra
			129, //Nisse’s Lair (Nisee's Lair in regions.ini)
			221, //Muire Tomb (Undead in regions.ini)

		};*/

		/// <summary>
		/// Regions unsafe for players with safety flag
		/// </summary>
		/*protected int[] m_unsafeRegions =
		{
			163, // new frontiers
		};*/

		public override bool IsAllowedToAttack(GameLiving attacker, GameLivingBase defender, bool quiet)
		{
			// if controlled NPC - do checks for owner instead
			if (attacker is GameNPC)
			{
				IControlledBrain controlled = ((GameNPC)attacker).Brain as IControlledBrain;
				if (controlled != null)
				{
					attacker = controlled.Owner;
					quiet = true; // silence all attacks by controlled npc
				}
			}
			if (defender is GameNPC)
			{
				IControlledBrain controlled = ((GameNPC)defender).Brain as IControlledBrain;
				if (controlled != null)
					defender = controlled.Owner;
			}

			if (!base.IsAllowedToAttack(attacker, defender, quiet))
				return false;

			GamePlayer playerAttacker = attacker as GamePlayer;
			GamePlayer playerDefender = defender as GamePlayer;
			if(playerAttacker != null && playerDefender != null)
			{
				//check group
				if(playerAttacker.PlayerGroup != null && playerAttacker.PlayerGroup.IsInTheGroup(playerDefender))
				{
					if (!quiet) MessageToLiving(playerAttacker, "You can't attack your group members.");
					return false;
				}

				if (playerAttacker.DuelTarget != defender)
				{
					//check guild
					if(playerAttacker.Guild != null && playerAttacker.Guild == playerDefender.Guild)
					{
						if (!quiet) MessageToLiving(playerAttacker, "You can't attack your guild members.");
						return false;
					}

					//todo: battlegroups

					switch(playerAttacker.Region.Type)
					{
						case eRegionType.Normal :
							{
								// Players with safety flag can not attack other players
								if(playerAttacker.Level < m_safetyLevel && playerAttacker.SafetyFlag)
								{
									if(quiet == false) MessageToLiving(playerAttacker, "Your PvP safety flag is ON.");
									return false;
								}

								// Players with safety flag can't be attacked
								if(playerDefender.Level < m_safetyLevel && playerDefender.SafetyFlag && playerDefender.Region.RegionID != 163) // new frontiers
								{
									if(quiet == false) MessageToLiving(attacker, playerDefender.Name + " has " + playerDefender.GetPronoun(1, false) + " safety flag on and is in a safe area, you can't attack " + playerDefender.GetPronoun(2, false) + " here.");
									return false;
								}

								return true;
							}
						case eRegionType.Safe :
							{
								if(quiet == false) MessageToLiving(playerAttacker, "You're currently in a safe zone, you can't attack other players here.");
								return false;
							}

						case eRegionType.Frontier :
							{
								return true;
							}
					}
				}
			}

			// allow mobs to attack mobs
			if (attacker.Realm == 0 && defender.Realm == 0)
				return true;

			// "friendly" NPCs can't attack "friendly" players
			if(defender is GameNPC && defender.Realm != 0 && attacker.Realm != 0)
			{
				if(quiet == false) MessageToLiving(attacker, "You can't attack a friendly NPC!");
				return false;
			}
			// "friendly" NPCs can't be attacked by "friendly" players
			if(attacker is GameNPC && attacker.Realm != 0 && defender.Realm != 0)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Is caster allowed to cast a spell
		/// </summary>
		/// <param name="caster"></param>
		/// <param name="target"></param>
		/// <param name="spell"></param>
		/// <param name="spellLine"></param>
		/// <returns>true if allowed</returns>
		public override bool IsAllowedToCastSpell(GameLiving caster, GameLivingBase target, Spell spell, SpellLine spellLine)
		{
			if(!base.IsAllowedToCastSpell(caster, target, spell, spellLine)) return false;

			GamePlayer casterPlayer = caster as GamePlayer;
			if(casterPlayer != null)
			{
				if(casterPlayer.IsPvPInvulnerability)
				{
					// always allow selftargeted spells
					if(spell.Target == "Self") return true;

					// only caster can be the target, can't buff/heal other players
					// PBAE/GTAE doesn't need a target so we check spell type as well
					if(caster != target || spell.Target == "Area" || spell.Target == "Enemy" || (spell.Target == "Group" && spell.SpellType != "SpeedEnhancement"))
					{
						MessageToLiving(caster, "You can only cast spells on yourself until your PvP invulnerability timer wears off!", eChatType.CT_Important);
						return false;
					}
				}

			}
			return true;
		}

		public override bool IsSameRealm(GameLiving source, GameLivingBase target, bool quiet)
		{
			if(source == null || target == null) return false;

			// if controlled NPC - do checks for owner instead
			if (source is GameNPC)
			{
				IControlledBrain controlled = ((GameNPC)source).Brain as IControlledBrain;
				if (controlled != null)
				{
					source = controlled.Owner;
					quiet = true; // silence all attacks by controlled npc
				}
			}
			if (target is GameNPC)
			{
				IControlledBrain controlled = ((GameNPC)target).Brain as IControlledBrain;
				if (controlled != null)
					target = controlled.Owner;
			}

			if(base.IsSameRealm(source, target, quiet)) return true;

			// mobs can heal mobs, players heal players/NPC
			if(source.Realm == 0 && target.Realm == 0) return true;
			if(source.Realm != 0 && target.Realm != 0) return true;			
			
			if(quiet == false) MessageToLiving(source, target.GetName(0, true) + " is not a member of your realm!");
			return false;
		}


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
		public override byte GetColorHandling(GameClient client)
		{
			return 1;
		}
	}
}
