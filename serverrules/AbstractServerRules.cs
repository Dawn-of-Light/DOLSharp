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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Reflection;

using DOL.Database;
using DOL.AI.Brain;
using DOL.GS;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;
using DOL.GS.ServerProperties;
using DOL.Language;

using log4net;

namespace DOL.GS.ServerRules
{
	public abstract class AbstractServerRules : IServerRules
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
		public virtual bool IsAllowedToConnect(GameClient client, string username)
		{
			if (!client.Socket.Connected)
				return false;

			// Ban account
			IList<DBBannedAccount> objs;
			objs = GameServer.Database.SelectObjects<DBBannedAccount>("((Type='A' OR Type='B') AND Account ='" + GameServer.Database.Escape(username) + "')");
			if (objs.Count > 0)
			{
				client.Out.SendLoginDenied(eLoginError.AccountIsBannedFromThisServerType);
				log.Debug("IsAllowedToConnect deny access to username " + username);
				return false;
			}

			// Ban IP Address or range (example: 5.5.5.%)
			string accip = GameServer.Database.Escape(client.TcpEndpointAddress);
			objs = GameServer.Database.SelectObjects<DBBannedAccount>("((Type='I' OR Type='B') AND '" + GameServer.Database.Escape(accip) + "' LIKE Ip)");
			if (objs.Count > 0)
			{
				client.Out.SendLoginDenied(eLoginError.AccountIsBannedFromThisServerType);
				log.Debug("IsAllowedToConnect deny access to IP " + accip);
				return false;
			}

			GameClient.eClientVersion min = (GameClient.eClientVersion)Properties.CLIENT_VERSION_MIN;
			if (min != GameClient.eClientVersion.VersionNotChecked && client.Version < min)
			{
				client.Out.SendLoginDenied(eLoginError.ClientVersionTooLow);
				log.Debug("IsAllowedToConnect deny access to client version (too low) " + client.Version);
				return false;
			}

			GameClient.eClientVersion max = (GameClient.eClientVersion)Properties.CLIENT_VERSION_MAX;
			if (max != GameClient.eClientVersion.VersionNotChecked && client.Version > max)
			{
				client.Out.SendLoginDenied(eLoginError.NotAuthorizedToUseExpansionVersion);
				log.Debug("IsAllowedToConnect deny access to client version (too high) " + client.Version);
				return false;
			}

			if (Properties.CLIENT_TYPE_MAX > -1)
			{
				GameClient.eClientType type = (GameClient.eClientType)Properties.CLIENT_TYPE_MAX;
				if ((int)client.ClientType > (int)type )
				{
					client.Out.SendLoginDenied(eLoginError.ExpansionPacketNotAllowed);
					log.Debug("IsAllowedToConnect deny access to expansion pack.");
					return false;
				}
			}

			/* Example to limit the connections from a certain IP range!
			if(client.Socket.RemoteEndPoint.ToString().StartsWith("192.168.0."))
			{
				client.Out.SendLoginDenied(eLoginError.AccountNoAccessAnyGame);
				return false;
			}
			 */


			/* Example to deny new connections on saturdays
			if(DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
			{
				client.Out.SendLoginDenied(eLoginError.GameCurrentlyClosed);
				return false;
			}
			 */

			/* Example to deny new connections between 10am and 12am
			if(DateTime.Now.Hour >= 10 && DateTime.Now.Hour <= 12)
			{
				client.Out.SendLoginDenied(eLoginError.GameCurrentlyClosed);
				return false;
			}
			 */

			Account account = GameServer.Database.SelectObject<Account>("Name ='" + GameServer.Database.Escape(username) + "'");

			if (Properties.MAX_PLAYERS > 0)
			{
				if (WorldMgr.GetAllClients().Count >= Properties.MAX_PLAYERS)
				{
					// GMs are still allowed to enter server
					if (account == null || (account.PrivLevel == 1 && account.Status <= 0))
					{
						// Normal Players will not be allowed over the max
						client.Out.SendLoginDenied(eLoginError.TooManyPlayersLoggedIn);
						log.Debug("IsAllowedToConnect deny access due to too many players.");
						return false;
					}

				}
			}

			if (Properties.STAFF_LOGIN)
			{
				if (account == null || account.PrivLevel == 1)
				{
					// GMs are still allowed to enter server
					// Normal Players will not be allowed to Log in
					client.Out.SendLoginDenied(eLoginError.GameCurrentlyClosed);
					log.Debug("IsAllowedToConnect deny access; staff only login");
					return false;
				}
			}

			if (!Properties.ALLOW_DUAL_LOGINS)
			{
				if ((account == null || account.PrivLevel == 1) && client.TcpEndpointAddress != "not connected")
				{
					foreach (GameClient cln in WorldMgr.GetAllClients())
					{
						if (cln == null || client == cln) continue;
						if (cln.TcpEndpointAddress == client.TcpEndpointAddress)
						{
							if (cln.Account != null && cln.Account.PrivLevel > 1)
							{
								break;
							}
							client.Out.SendLoginDenied(eLoginError.AccountAlreadyLoggedIntoOtherServer);
							log.Debug("IsAllowedToConnect deny access; dual login not allowed");
							return false;
						}
					}
				}
			}

			return true;
		}

		public abstract bool IsSameRealm(GameLiving source, GameLiving target, bool quiet);
		public abstract bool IsAllowedCharsInAllRealms(GameClient client);
		public abstract bool IsAllowedToGroup(GamePlayer source, GamePlayer target, bool quiet);
		public abstract bool IsAllowedToJoinGuild(GamePlayer source, Guild guild);
		public abstract bool IsAllowedToTrade(GameLiving source, GameLiving target, bool quiet);
		public abstract bool IsAllowedToUnderstand(GameLiving source, GamePlayer target);
		public abstract string RulesDescription();

		public virtual bool IsAllowedToMoveToBind(GamePlayer player)
		{
			return true;
		}

		public virtual bool CountsTowardsSlashLevel(DOLCharacters player)
		{
			return true;
		}

		/// <summary>
		/// Is attacker allowed to attack defender.
		/// </summary>
		/// <param name="attacker">living that makes attack</param>
		/// <param name="defender">attacker's target</param>
		/// <param name="quiet">should messages be sent</param>
		/// <returns>true if attack is allowed</returns>
		public virtual bool IsAllowedToAttack(GameLiving attacker, GameLiving defender, bool quiet)
		{
			if (attacker == null || defender == null)
				return false;

			//dead things can't attack
			if (!defender.IsAlive || !attacker.IsAlive )
				return false;

			GamePlayer playerAttacker = attacker as GamePlayer;
			GamePlayer playerDefender = defender as GamePlayer;

			if (playerDefender != null && playerDefender.Client.ClientState == GameClient.eClientState.WorldEnter)
			{
				if (!quiet)
					MessageToLiving(attacker, defender.Name + " is entering the game and is temporarily immune to PvP attacks!");
				return false;
			}

			if (playerAttacker != null && playerDefender != null)
			{
				// Attacker immunity
				if (playerAttacker.IsPvPInvulnerability)
				{
					if (quiet == false) MessageToLiving(attacker, "You can't attack players until your PvP invulnerability timer wears off!");
					return false;
				}

				// Defender immunity
				if (playerDefender.IsPvPInvulnerability)
				{
					if (quiet == false) MessageToLiving(attacker, defender.Name + " is temporarily immune to PvP attacks!");
					return false;
				}
			}

			// PEACE NPCs can't be attacked/attack
			if (attacker is GameNPC)
				if ((((GameNPC)attacker).Flags & (uint)GameNPC.eFlags.PEACE) != 0)
					return false;

			if (defender is GameNPC)
				if ((((GameNPC)defender).Flags & (uint)GameNPC.eFlags.PEACE) != 0)
					return false;

			//GMs can't be attacked
			if (playerDefender != null && playerDefender.Client.Account.PrivLevel > 1)
				return false;

			//safe area support for defender
			foreach (AbstractArea area in defender.CurrentAreas)
			{
				if (!area.IsSafeArea)
					continue;

				if (defender is GamePlayer)
				{
					if (quiet == false) MessageToLiving(attacker, "You can't attack someone in a safe area!");
					return false;
				}
			}

			//safe area support for attacker
			foreach (AbstractArea area in attacker.CurrentAreas)
			{
				if ((area.IsSafeArea) && (defender is GamePlayer) && (attacker is GamePlayer))
				{
					if (quiet == false) MessageToLiving(attacker, "You can't attack someone in a safe area!");
					return false;
				}
			}

			//I don't want mobs attacking guards
			if (defender is GameKeepGuard && attacker is GameNPC && attacker.Realm == 0)
				return false;

			//Checking for shadowed necromancer, can't be attacked.
			if(defender.ControlledBrain != null)
				if(defender.ControlledBrain.Body != null)
					if(defender.ControlledBrain.Body is NecromancerPet)
			{
				if (quiet == false) MessageToLiving(attacker, "You can't attack a shadowed necromancer!");
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
		public virtual bool IsAllowedToCastSpell(GameLiving caster, GameLiving target, Spell spell, SpellLine spellLine)
		{
			//we only allow certain spell targets to be cast when targeting a keep component
			//tolakram - live allows most damage spells to be cast on doors. This should be handled in spell handlers
			if (target is GameKeepComponent || target is GameKeepDoor)
			{
				bool isAllowed = false;

				switch (spell.Target.ToLower())
				{
					case "self":
					case "group":
					case "pet":
					case "controlled":
					case "realm":
					case "area":
						isAllowed = true;
						break;

					case "enemy":

						if (spell.Radius == 0)
						{
							switch (spell.SpellType.ToLower())
							{
								case "archery":
								case "bolt":
								case "bomber":
								case "damagespeeddecrease":
								case "directdamage":
								case "magicalstrike":
								case "siegearrow":
								case "summontheurgistpet":
								case "directdamagewithdebuff":
									isAllowed = true;
									break;
							}
						}

						// pbaoe
						if (spell.Radius > 0 && spell.Range == 0)
						{
							isAllowed = true;
						}

						break;
				}

				if (!isAllowed && caster is GamePlayer)
					(caster as GamePlayer).Client.Out.SendMessage("You can't cast this spell on the " + target.Name, eChatType.CT_System, eChatLoc.CL_SystemWindow);

				return isAllowed;
			}

			return true;
		}

		public virtual bool IsAllowedToSpeak(GamePlayer source, string communicationType)
		{
			if (source.IsAlive == false)
			{
				MessageToLiving(source, "Hmmmm...you can't " + communicationType + " while dead!");
				return false;
			}
			return true;
		}

		/// <summary>
		/// Is player allowed to bind
		/// </summary>
		/// <param name="player"></param>
		/// <param name="point"></param>
		/// <returns></returns>
		public virtual bool IsAllowedToBind(GamePlayer player, BindPoint point)
		{
			return true;
		}

		/// <summary>
		/// Is player allowed to make the item
		/// </summary>
		/// <param name="player"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public virtual bool IsAllowedToCraft(GamePlayer player, ItemTemplate item)
		{
			return true;
		}

		/// <summary>
		/// Is player allowed to claim in this region
		/// </summary>
		/// <param name="player"></param>
		/// <param name="region"></param>
		/// <returns></returns>
		public virtual bool IsAllowedToClaim(GamePlayer player, Region region)
		{
			if (region.IsInstance)
			{
				return false;
			}

			return true;
		}

		public virtual bool IsAllowedToZone(GamePlayer player, Region region)
		{
			return true;
		}

		public virtual bool CanTakeFallDamage(GamePlayer player)
		{
			if (player.Client.Account.PrivLevel > 1)
				return false;

			return true;
		}

		public virtual long GetExperienceForLiving(int level)
		{
			level = (level < 0) ? 0 : level;

			// use exp table
			if (level < GameLiving.XPForLiving.Length)
				return GameLiving.XPForLiving[level];

			// use formula if level is not in exp table
			// long can hold values up to level 238
			if (level > 238)
				level = 238;

			double k1, k1_inc, k1_lvl;

			// noret: using these rules i was able to reproduce table from
			// http://www.daocweave.com/daoc/general/experience_table.htm
			if (level >= 35)
			{
				k1_lvl = 35;
				k1_inc = 0.2;
				k1 = 20;
			}
			else if (level >= 20)
			{
				k1_lvl = 20;
				k1_inc = 0.3334;
				k1 = 15;
			}
			else if (level >= 10)
			{
				k1_lvl = 10;
				k1_inc = 0.5;
				k1 = 10;
			}
			else
			{
				k1_lvl = 0;
				k1_inc = 1;
				k1 = 0;
			}

			long exp = (long)(Math.Pow(2, k1 + (level - k1_lvl) * k1_inc) * 5);
			if (exp < 0)
			{
				exp = 0;
			}

			return exp;
		}

		public virtual bool CheckAbilityToUseItem(GameLiving living, ItemTemplate item)
		{
			if (living == null || item == null)
				return false;

			//armor
			if (item.Object_Type >= (int)eObjectType._FirstArmor && item.Object_Type <= (int)eObjectType._LastArmor)
			{
				int bestLevel = -1;
				bestLevel = Math.Max(bestLevel, living.GetAbilityLevel(Abilities.AlbArmor));
				bestLevel = Math.Max(bestLevel, living.GetAbilityLevel(Abilities.HibArmor));
				bestLevel = Math.Max(bestLevel, living.GetAbilityLevel(Abilities.MidArmor));

				switch ((eObjectType)item.Object_Type)
				{
						case eObjectType.Cloth: return bestLevel >= 1;
						case eObjectType.Leather: return bestLevel >= 2;
					case eObjectType.Reinforced:
						case eObjectType.Studded: return bestLevel >= 3;
					case eObjectType.Scale:
						case eObjectType.Chain: return bestLevel >= 4;
						case eObjectType.Plate: return bestLevel >= 5;
						default: return false;
				}
			}

			string[] oneHandCheck = new string[0];
			string[] twoHandCheck = new string[0];
			string[] otherCheck = new string[0];

			//http://dol.kitchenhost.de/files/dol/Info/itemtable.txt
			//http://support.darkageofcamelot.com/cgi-bin/support.cfg/php/enduser/std_adp.php?p_sid=frxnPUjg&p_lva=&p_refno=020709-000000&p_created=1026248996&p_sp=cF9ncmlkc29ydD0mcF9yb3dfY250PTE0JnBfc2VhcmNoX3RleHQ9JnBfc2VhcmNoX3R5cGU9MyZwX2NhdF9sdmwxPTI2JnBfY2F0X2x2bDI9fmFueX4mcF9zb3J0X2J5PWRmbHQmcF9wYWdlPTE*&p_li
			switch ((eObjectType)item.Object_Type)
			{
					case eObjectType.GenericItem: return true;
					case eObjectType.GenericArmor: return true;
					case eObjectType.GenericWeapon: return true;
					case eObjectType.Staff: twoHandCheck = new string[] { Abilities.Weapon_Staves }; break;
					case eObjectType.Fired: otherCheck = new string[] { Abilities.Weapon_Shortbows }; break;

					//generic
					case eObjectType.FistWraps: oneHandCheck = new string[] { Abilities.Weapon_FistWraps }; break;
					case eObjectType.MaulerStaff: twoHandCheck = new string[] { Abilities.Weapon_MaulerStaff }; break;

					//alb
					case eObjectType.CrushingWeapon: oneHandCheck = new string[] { Abilities.Weapon_Crushing, Abilities.Weapon_Blunt, Abilities.Weapon_Hammers }; break;
					case eObjectType.SlashingWeapon: oneHandCheck = new string[] { Abilities.Weapon_Slashing, Abilities.Weapon_Blades, Abilities.Weapon_Swords, Abilities.Weapon_Axes }; break;
					case eObjectType.ThrustWeapon: oneHandCheck = new string[] { Abilities.Weapon_Thrusting, Abilities.Weapon_Piercing }; break;
					case eObjectType.Flexible: oneHandCheck = new string[] { Abilities.Weapon_Flexible }; break;
					case eObjectType.TwoHandedWeapon: twoHandCheck = new string[] { Abilities.Weapon_TwoHanded, Abilities.Weapon_LargeWeapons }; break;
					case eObjectType.PolearmWeapon: twoHandCheck = new string[] { Abilities.Weapon_Polearms, Abilities.Weapon_CelticSpear, Abilities.Weapon_Spears }; break;
					case eObjectType.Longbow: otherCheck = new string[] { Abilities.Weapon_Longbows, Abilities.Weapon_Archery }; break;
					case eObjectType.Crossbow: otherCheck = new string[] { Abilities.Weapon_Crossbow }; break;

					//mid
					case eObjectType.Sword: oneHandCheck = new string[] { Abilities.Weapon_Swords, Abilities.Weapon_Slashing, Abilities.Weapon_Blades }; twoHandCheck = new string[] { Abilities.Weapon_Swords }; break;
					case eObjectType.Hammer: oneHandCheck = new string[] { Abilities.Weapon_Hammers, Abilities.Weapon_Crushing, Abilities.Weapon_Blunt }; twoHandCheck = new string[] { Abilities.Weapon_Hammers }; break;
				case eObjectType.LeftAxe:
					case eObjectType.Axe: oneHandCheck = new string[] { Abilities.Weapon_Axes, Abilities.Weapon_Slashing, Abilities.Weapon_Blades }; twoHandCheck = new string[] { Abilities.Weapon_Axes }; break;
					case eObjectType.HandToHand: oneHandCheck = new string[] { Abilities.Weapon_HandToHand }; break;
					case eObjectType.Spear: twoHandCheck = new string[] { Abilities.Weapon_Spears, Abilities.Weapon_CelticSpear, Abilities.Weapon_Polearms }; break;
					case eObjectType.CompositeBow: otherCheck = new string[] { Abilities.Weapon_CompositeBows, Abilities.Weapon_Archery }; break;
					case eObjectType.Thrown: otherCheck = new string[] { Abilities.Weapon_Thrown }; break;

					//hib
					case eObjectType.Blades: oneHandCheck = new string[] { Abilities.Weapon_Blades, Abilities.Weapon_Slashing, Abilities.Weapon_Swords, Abilities.Weapon_Axes }; break;
					case eObjectType.Blunt: oneHandCheck = new string[] { Abilities.Weapon_Blunt, Abilities.Weapon_Crushing, Abilities.Weapon_Hammers }; break;
					case eObjectType.Piercing: oneHandCheck = new string[] { Abilities.Weapon_Piercing, Abilities.Weapon_Thrusting }; break;
					case eObjectType.LargeWeapons: twoHandCheck = new string[] { Abilities.Weapon_LargeWeapons, Abilities.Weapon_TwoHanded }; break;
					case eObjectType.CelticSpear: twoHandCheck = new string[] { Abilities.Weapon_CelticSpear, Abilities.Weapon_Spears, Abilities.Weapon_Polearms }; break;
					case eObjectType.Scythe: twoHandCheck = new string[] { Abilities.Weapon_Scythe }; break;
					case eObjectType.RecurvedBow: otherCheck = new string[] { Abilities.Weapon_RecurvedBows, Abilities.Weapon_Archery }; break;

					//misc
					case eObjectType.Magical: return true;
					case eObjectType.Shield: return living.GetAbilityLevel(Abilities.Shield) >= item.Type_Damage;
					case eObjectType.Arrow: otherCheck = new string[] { Abilities.Weapon_CompositeBows, Abilities.Weapon_Longbows, Abilities.Weapon_RecurvedBows, Abilities.Weapon_Shortbows, Abilities.Weapon_Archery }; break;
					case eObjectType.Bolt: otherCheck = new string[] { Abilities.Weapon_Crossbow }; break;
					case eObjectType.Poison: return living.GetModifiedSpecLevel(Specs.Envenom) > 0;
					case eObjectType.Instrument: return living.HasAbility(Abilities.Weapon_Instruments);

					//housing
					case eObjectType.GardenObject: return true;
					case eObjectType.HouseWallObject: return true;
					case eObjectType.HouseFloorObject: return true;
					//TODO: different shield sizes
			}

			if (item.Item_Type == Slot.RIGHTHAND || item.Item_Type == Slot.LEFTHAND)
			{
				foreach (string check in oneHandCheck)
				{
					if (living.HasAbility(check))
						return true;
				}
			}
			else if (item.Item_Type == Slot.TWOHAND)
			{
				foreach (string check in twoHandCheck)
				{
					if (living.HasAbility(check))
						return true;
				}
			}

			foreach (string check in otherCheck)
			{
				if (living.HasAbility(check))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Get object specialization level based on server type
		/// </summary>
		/// <param name="player">player whom specializations are checked</param>
		/// <param name="objectType">object type</param>
		/// <returns>specialization in object or 0</returns>
		public virtual int GetObjectSpecLevel(GamePlayer player, eObjectType objectType)
		{
			int res = 0;

			foreach (eObjectType obj in GetCompatibleObjectTypes(objectType))
			{
				int spec = player.GetModifiedSpecLevel(SkillBase.ObjectTypeToSpec(obj));
				if (res < spec)
					res = spec;
			}
			return res;
		}

		/// <summary>
		/// Get object specialization level based on server type
		/// </summary>
		/// <param name="player">player whom specializations are checked</param>
		/// <param name="objectType">object type</param>
		/// <returns>specialization in object or 0</returns>
		public virtual int GetBaseObjectSpecLevel(GamePlayer player, eObjectType objectType)
		{
			int res = 0;

			foreach (eObjectType obj in GetCompatibleObjectTypes(objectType))
			{
				int spec = player.GetBaseSpecLevel(SkillBase.ObjectTypeToSpec(obj));
				if (res < spec)
					res = spec;
			}
			return res;
		}

		/// <summary>
		/// Checks whether one object type is equal to another
		/// based on server type
		/// </summary>
		/// <param name="type1"></param>
		/// <param name="type2"></param>
		/// <returns>true if equals</returns>
		public virtual bool IsObjectTypesEqual(eObjectType type1, eObjectType type2)
		{
			foreach (eObjectType obj in GetCompatibleObjectTypes(type1))
			{
				if (obj == type2)
					return true;
			}
			return false;
		}

		#region GetCompatibleObjectTypes

		/// <summary>
		/// Holds arrays of compatible object types
		/// </summary>
		protected Hashtable m_compatibleObjectTypes = null;

		/// <summary>
		/// Translates object type to compatible object types based on server type
		/// </summary>
		/// <param name="objectType">The object type</param>
		/// <returns>An array of compatible object types</returns>
		protected virtual eObjectType[] GetCompatibleObjectTypes(eObjectType objectType)
		{
			if (m_compatibleObjectTypes == null)
			{
				m_compatibleObjectTypes = new Hashtable();
				m_compatibleObjectTypes[(int)eObjectType.Staff] = new eObjectType[] { eObjectType.Staff };
				m_compatibleObjectTypes[(int)eObjectType.Fired] = new eObjectType[] { eObjectType.Fired };

				m_compatibleObjectTypes[(int)eObjectType.FistWraps] = new eObjectType[] { eObjectType.FistWraps };
				m_compatibleObjectTypes[(int)eObjectType.MaulerStaff] = new eObjectType[] { eObjectType.MaulerStaff };

				//alb
				m_compatibleObjectTypes[(int)eObjectType.CrushingWeapon] = new eObjectType[] { eObjectType.CrushingWeapon, eObjectType.Blunt, eObjectType.Hammer };
				m_compatibleObjectTypes[(int)eObjectType.SlashingWeapon] = new eObjectType[] { eObjectType.SlashingWeapon, eObjectType.Blades, eObjectType.Sword, eObjectType.Axe };
				m_compatibleObjectTypes[(int)eObjectType.ThrustWeapon] = new eObjectType[] { eObjectType.ThrustWeapon, eObjectType.Piercing };
				m_compatibleObjectTypes[(int)eObjectType.TwoHandedWeapon] = new eObjectType[] { eObjectType.TwoHandedWeapon, eObjectType.LargeWeapons };
				m_compatibleObjectTypes[(int)eObjectType.PolearmWeapon] = new eObjectType[] { eObjectType.PolearmWeapon, eObjectType.CelticSpear, eObjectType.Spear };
				m_compatibleObjectTypes[(int)eObjectType.Flexible] = new eObjectType[] { eObjectType.Flexible };
				m_compatibleObjectTypes[(int)eObjectType.Longbow] = new eObjectType[] { eObjectType.Longbow };
				m_compatibleObjectTypes[(int)eObjectType.Crossbow] = new eObjectType[] { eObjectType.Crossbow };
				//TODO: case 5: abilityCheck = Abilities.Weapon_Thrown; break;

				//mid
				m_compatibleObjectTypes[(int)eObjectType.Hammer] = new eObjectType[] { eObjectType.Hammer, eObjectType.CrushingWeapon, eObjectType.Blunt };
				m_compatibleObjectTypes[(int)eObjectType.Sword] = new eObjectType[] { eObjectType.Sword, eObjectType.SlashingWeapon, eObjectType.Blades };
				m_compatibleObjectTypes[(int)eObjectType.LeftAxe] = new eObjectType[] { eObjectType.LeftAxe };
				m_compatibleObjectTypes[(int)eObjectType.Axe] = new eObjectType[] { eObjectType.Axe, eObjectType.SlashingWeapon, eObjectType.Blades, eObjectType.LeftAxe };
				m_compatibleObjectTypes[(int)eObjectType.HandToHand] = new eObjectType[] { eObjectType.HandToHand };
				m_compatibleObjectTypes[(int)eObjectType.Spear] = new eObjectType[] { eObjectType.Spear, eObjectType.CelticSpear, eObjectType.PolearmWeapon };
				m_compatibleObjectTypes[(int)eObjectType.CompositeBow] = new eObjectType[] { eObjectType.CompositeBow };
				m_compatibleObjectTypes[(int)eObjectType.Thrown] = new eObjectType[] { eObjectType.Thrown };

				//hib
				m_compatibleObjectTypes[(int)eObjectType.Blunt] = new eObjectType[] { eObjectType.Blunt, eObjectType.CrushingWeapon, eObjectType.Hammer };
				m_compatibleObjectTypes[(int)eObjectType.Blades] = new eObjectType[] { eObjectType.Blades, eObjectType.SlashingWeapon, eObjectType.Sword, eObjectType.Axe };
				m_compatibleObjectTypes[(int)eObjectType.Piercing] = new eObjectType[] { eObjectType.Piercing, eObjectType.ThrustWeapon };
				m_compatibleObjectTypes[(int)eObjectType.LargeWeapons] = new eObjectType[] { eObjectType.LargeWeapons, eObjectType.TwoHandedWeapon };
				m_compatibleObjectTypes[(int)eObjectType.CelticSpear] = new eObjectType[] { eObjectType.CelticSpear, eObjectType.Spear, eObjectType.PolearmWeapon };
				m_compatibleObjectTypes[(int)eObjectType.Scythe] = new eObjectType[] { eObjectType.Scythe };
				m_compatibleObjectTypes[(int)eObjectType.RecurvedBow] = new eObjectType[] { eObjectType.RecurvedBow };

				m_compatibleObjectTypes[(int)eObjectType.Shield] = new eObjectType[] { eObjectType.Shield };
				m_compatibleObjectTypes[(int)eObjectType.Poison] = new eObjectType[] { eObjectType.Poison };
				//TODO: case 45: abilityCheck = Abilities.instruments; break;
			}

			eObjectType[] res = (eObjectType[])m_compatibleObjectTypes[(int)objectType];
			if (res == null)
				return new eObjectType[0];
			return res;
		}

		#endregion

		/// <summary>
		/// Invoked on NPC death and deals out
		/// experience/realm points if needed
		/// </summary>
		/// <param name="killedNPC">npc that died</param>
		/// <param name="killer">killer</param>
		public virtual void OnNPCKilled(GameNPC killedNPC, GameObject killer)
		{
			lock (killedNPC.XPGainers.SyncRoot)
			{
				#region Worth no experience
				//"This monster has been charmed recently and is worth no experience."
				string message = "You gain no experience from this kill!";
				if (killedNPC.CurrentRegion.Time - GameNPC.CHARMED_NOEXP_TIMEOUT < killedNPC.TempProperties.getProperty<long>(GameNPC.CHARMED_TICK_PROP))
				{
					message = "This monster has been charmed recently and is worth no experience.";
				}

				if (!killedNPC.IsWorthReward)
				{
					foreach (DictionaryEntry de in killedNPC.XPGainers)
					{
						GamePlayer player = de.Key as GamePlayer;
						if (player != null)
							player.Out.SendMessage(message, eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					return;
				}
				#endregion

				#region Group/Total Damage
				float totalDamage = 0;
				Dictionary<Group, int> plrGrpExp = new Dictionary<Group, int>();
				GamePlayer highestPlayer = null;
				bool isGroupInRange = false;
				//Collect the total damage
				foreach (DictionaryEntry de in killedNPC.XPGainers)
				{
					totalDamage += (float)de.Value;
					GamePlayer player = de.Key as GamePlayer;

					//Check stipulations (this will ignore all pet damage)
					if (player == null || player.ObjectState != GameObject.eObjectState.Active || !player.IsWithinRadius(killedNPC, WorldMgr.MAX_EXPFORKILL_DISTANCE))
						continue;

					if (player.Group != null)
					{
						// checking to see if any group members are in range of the killer
						if (player != (killer as GamePlayer))
							isGroupInRange = true;

						if (plrGrpExp.ContainsKey(player.Group))
							plrGrpExp[player.Group] += 1;
						else
							plrGrpExp[player.Group] = 1;

						// tolakram: only prepare for xp challenge code if player is in a group
						if (highestPlayer == null || (player.Level > highestPlayer.Level))
							highestPlayer = player;
					}

				}
				#endregion

				long npcExpValue = killedNPC.ExperienceValue;
				int npcRPValue = killedNPC.RealmPointsValue;
				int npcBPValue = killedNPC.BountyPointsValue;
				double npcExceedXPCapAmount = killedNPC.ExceedXPCapAmount;

				//Need to do this before hand so we only do it once - just in case if the player levels!
				double highestConValue = 0;
				if (highestPlayer != null)
					highestConValue = highestPlayer.GetConLevel(killedNPC);

				//Now deal the XP to all livings
				foreach (DictionaryEntry de in killedNPC.XPGainers)
				{
					GameLiving living = de.Key as GameLiving;
					GamePlayer player = living as GamePlayer;

					if (living is NecromancerPet)
					{
						NecromancerPet necroPet = living as NecromancerPet;
						player = ((necroPet.Brain as IControlledBrain).Owner) as GamePlayer;
					}

					//Check stipulations
					if (living == null || living.ObjectState != GameObject.eObjectState.Active || !living.IsWithinRadius(killedNPC, WorldMgr.MAX_EXPFORKILL_DISTANCE))
						continue;

					//Changed: people were getting penalized for their pets doing damage
					double damagePercent = (float)de.Value / totalDamage;

					#region Realm Points

					// realm points
					int rpCap = living.RealmPointsValue * 2;
					int realmPoints = 0;

					// Keep and Tower captures reward full RP and BP value to each player
					if (killedNPC is GuardLord)
					{
						realmPoints = npcRPValue;
					}
					else
					{
						realmPoints = (int)(npcRPValue * damagePercent);
						//rp bonuses from RR and Group
						//100% if full group,scales down according to player count in group and their range to target
						if (player != null && player.Group != null && plrGrpExp.ContainsKey(player.Group))
						{
							realmPoints = (int)(realmPoints * (1.0 + plrGrpExp[player.Group] * 0.125));
						}
					}

					if (realmPoints > rpCap)
						realmPoints = rpCap;

					if (realmPoints > 0)
						living.GainRealmPoints(realmPoints);

					#endregion

					#region Bounty Points

					// bounty points

					int bpCap = living.BountyPointsValue * 2;
					int bountyPoints = 0;

					// Keep and Tower captures reward full RP and BP value to each player
					if (killedNPC is GuardLord)
					{
						bountyPoints = npcBPValue;
					}
					else
					{
						bountyPoints = (int)(npcBPValue * damagePercent);
					}

					if (bountyPoints > bpCap)
						bountyPoints = bpCap;
					if (bountyPoints > 0)
						living.GainBountyPoints(bountyPoints);

					#endregion

					// experience points
					long xpReward = 0;
					long campBonus = 0;
					long groupExp = 0;
					long outpostXP = 0;

					if (player != null && (player.Group == null || !plrGrpExp.ContainsKey(player.Group)))
						xpReward = (long)(npcExpValue * damagePercent); // exp for damage percent
					else
						xpReward = npcExpValue;

					// exp cap
					/*
					
					http://support.darkageofcamelot.com/kb/article.php?id=438
					 
					Experience clamps have been raised from 1.1x a same level kill to 1.25x a same level kill.
					This change has two effects: it will allow lower level players in a group to gain more experience faster (15% faster),
					and it will also let higher level players (the 35-50s who tend to hit this clamp more often) to gain experience faster.
					 */
					long expCap = (long)(GameServer.ServerRules.GetExperienceForLiving(living.Level) * ServerProperties.Properties.XP_CAP_PERCENT / 100);

					if (player != null)
					{
						expCap = (long)(GameServer.ServerRules.GetExperienceForLiving(player.Level) * ServerProperties.Properties.XP_CAP_PERCENT / 100);

						if (player.Group != null && isGroupInRange)
						{
							// Optional group cap can be set different from standard player cap
							expCap = (long)(GameServer.ServerRules.GetExperienceForLiving(player.Level) * ServerProperties.Properties.XP_GROUP_CAP_PERCENT / 100);
						}
					}

					#region Challenge Code
					//let's check the con, for example if a level 50 kills a green, we want our level 1 to get green xp too
					/*
					 * http://www.camelotherald.com/more/110.shtml
					 * All group experience is divided evenly amongst group members, if they are in the same level range. What's a level range? One color range.
					 * If everyone in the group cons yellow to each other (or high blue, or low orange), experience will be shared out exactly evenly, with no leftover points.
					 * How can you determine a color range? Simple - Level divided by ten plus one. So, to a level 40 player (40/10 + 1), 36-40 is yellow, 31-35 is blue,
					 * 26-30 is green, and 25-less is gray. But for everyone in the group to get the maximum amount of experience possible, the encounter must be a challenge to
					 * the group. If the group has two people, the monster must at least be (con) yellow to the highest level member. If the group has four people, the monster
					 * must at least be orange. If the group has eight, the monster must at least be red.
					 *
					 * If "challenge code" has been activated, then the experience is divided roughly like so in a group of two (adjust the colors up if the group is bigger): If
					 * the monster was blue to the highest level player, each lower level group member will ROUGHLY receive experience as if they soloed a blue monster.
					 * Ditto for green. As everyone knows, a monster that cons gray to the highest level player will result in no exp for anyone. If the monster was high blue,
					 * challenge code may not kick in. It could also kick in if the monster is low yellow to the high level player, depending on the group strength of the pair.
					 */
					//xp challenge
					if (player != null && highestPlayer != null && highestConValue < 0)
					{
						//challenge success, the xp needs to be reduced to the proper con
						expCap = (long)(GameServer.ServerRules.GetExperienceForLiving(GameObject.GetLevelFromCon(player.Level, highestConValue)));
					}


					#endregion

					expCap = (long)(expCap * npcExceedXPCapAmount);

					if (xpReward > expCap)
						xpReward = expCap;

					#region Camp Bonus
					// camp bonus
					double fullCampBonus = ServerProperties.Properties.MAX_CAMP_BONUS;
					const double fullCampBonusTicks = 600000; //1 hour (in ms) = full 100%
					long livingLifeSpan = killedNPC.CurrentRegion.Time - killedNPC.SpawnTick;
					double campBonusPerc = fullCampBonus * (livingLifeSpan / fullCampBonusTicks);
					//1.49 http://news-daoc.goa.com/view_patchnote_archive.php?id_article=2478
					//"Camp bonuses" have been substantially upped in dungeons. Now camp bonuses in dungeons are, on average, 20% higher than outside camp bonuses.
					if (killer.CurrentZone.IsDungeon)
						campBonusPerc += 0.20;

					if (campBonusPerc < 0.01)
						campBonusPerc = 0;
					else if (campBonusPerc > fullCampBonus)
						campBonusPerc = fullCampBonus;

					campBonus = (long)(xpReward * campBonusPerc);
					#endregion

					#region Outpost Bonus
					//outpost XP
					//1.54 http://www.camelotherald.com/more/567.shtml
					//- Players now receive an exp bonus when fighting within 16,000
					//units of a keep controlled by your realm or your guild.
					//You get 20% bonus if your guild owns the keep or a 10% bonus
					//if your realm owns the keep.

					if (player != null)
					{
						AbstractGameKeep keep = KeepMgr.getKeepCloseToSpot(living.CurrentRegionID, living, 16000);
						if (keep != null)
						{
							byte bonus = 0;
							if (keep.Guild != null && keep.Guild == player.Guild)
								bonus = 20;
							else if (GameServer.Instance.Configuration.ServerType == eGameServerType.GST_Normal &&
							         keep.Realm == living.Realm)
								bonus = 10;

							outpostXP = (xpReward / 100) * bonus;
						}

						//FIXME: [WARN] this is a guess, I do not know the real way this is applied
						//apply the keep bonus for experience
						if (Keeps.KeepBonusMgr.RealmHasBonus(eKeepBonusType.Experience_5, living.Realm))
							outpostXP += (xpReward / 100) * 5;
						else if (Keeps.KeepBonusMgr.RealmHasBonus(eKeepBonusType.Experience_3, living.Realm))
							outpostXP += (xpReward / 100) * 3;
					}
					#endregion

					if (xpReward > 0)
					{
						if (player != null)
						{
							if (player.Group != null && plrGrpExp.ContainsKey(player.Group))
								groupExp += (long)(0.125 * xpReward * (int)plrGrpExp[player.Group]);

							// tolakram - remove this for now.  Correct calculation should be reduced XP based on damage pet did, not a flat reduction
							//if (player.ControlledNpc != null)
							//    xpReward = (long)(xpReward * 0.75);
						}

						//Ok we've calculated all the base experience.  Now let's add them all together.
						xpReward += (long)campBonus + groupExp + outpostXP;

						if (!living.IsAlive)//Dead living gets 25% exp only
							xpReward = (long)(xpReward * 0.25);

						//XP Rate is handled in GainExperience
						living.GainExperience(GameLiving.eXPSource.NPC, xpReward, campBonus, groupExp, outpostXP, true, true, true);
					}
				}
			}
		}

		/// <summary>
		/// Called on living death that is not gameplayer or gamenpc
		/// </summary>
		/// <param name="killedLiving">The living object</param>
		/// <param name="killer">The killer object</param>
		public virtual void OnLivingKilled(GameLiving killedLiving, GameObject killer)
		{
			lock (killedLiving.XPGainers.SyncRoot)
			{
				bool dealNoXP = false;
				float totalDamage = 0;
				//Collect the total damage
				foreach (DictionaryEntry de in killedLiving.XPGainers)
				{
					GameObject obj = (GameObject)de.Key;
					if (obj is GamePlayer)
					{
						//If a gameplayer with privlevel > 1 attacked the
						//mob, then the players won't gain xp ...
						if (((GamePlayer)obj).Client.Account.PrivLevel > 1)
						{
							dealNoXP = true;
							break;
						}
					}
					totalDamage += (float)de.Value;
				}

				if (dealNoXP || (killedLiving.ExperienceValue == 0 && killedLiving.RealmPointsValue == 0 && killedLiving.BountyPointsValue == 0))
				{
					return;
				}


				long ExpValue = killedLiving.ExperienceValue;
				int RPValue = killedLiving.RealmPointsValue;
				int BPValue = killedLiving.BountyPointsValue;

				//Now deal the XP and RPs to all livings
				foreach (DictionaryEntry de in killedLiving.XPGainers)
				{
					GameLiving living = de.Key as GameLiving;
					GamePlayer expGainPlayer = living as GamePlayer;
					if (living == null)
					{
						continue;
					}
					if (living.ObjectState != GameObject.eObjectState.Active)
					{
						continue;
					}
					/*
					 * http://www.camelotherald.com/more/2289.shtml
					 * Dead players will now continue to retain and receive their realm point credit
					 * on targets until they release. This will work for solo players as well as
					 * grouped players in terms of continuing to contribute their share to the kill
					 * if a target is being attacked by another non grouped player as well.
					 */
					//if (!living.Alive) continue;
					if (!living.IsWithinRadius(killedLiving, WorldMgr.MAX_EXPFORKILL_DISTANCE))
					{
						continue;
					}


					double damagePercent = (float)de.Value / totalDamage;
					if (!living.IsAlive)//Dead living gets 25% exp only
						damagePercent *= 0.25;

					// realm points
					int rpCap = living.RealmPointsValue * 2;
					int realmPoints = (int)(RPValue * damagePercent);
					//rp bonuses from RR and Group
					//20% if R1L0 char kills RR10,if RR10 char kills R1L0 he will get -20% bonus
					//100% if full group,scales down according to player count in group and their range to target
					if (living is GamePlayer)
					{
						GamePlayer killerPlayer = living as GamePlayer;
						if (killerPlayer.Group != null && killerPlayer.Group.MemberCount > 1)
						{
							lock (killerPlayer.Group)
							{
								int count = 0;
								foreach (GamePlayer player in killerPlayer.Group.GetPlayersInTheGroup())
								{
									if (!player.IsWithinRadius(killedLiving, WorldMgr.MAX_EXPFORKILL_DISTANCE)) continue;
									count++;
								}
								realmPoints = (int)(realmPoints * (1.0 + count * 0.125));
							}
						}
					}
					if (realmPoints > rpCap)
						realmPoints = rpCap;
					if (realmPoints != 0)
					{
						living.GainRealmPoints(realmPoints);
					}

					// bounty points
					int bpCap = living.BountyPointsValue * 2;
					int bountyPoints = (int)(BPValue * damagePercent);
					if (bountyPoints > bpCap)
						bountyPoints = bpCap;
					if (bountyPoints != 0)
					{
						living.GainBountyPoints(bountyPoints);
					}

					// experience
					// TODO: pets take 25% and owner gets 75%
					long xpReward = (long)(ExpValue * damagePercent); // exp for damage percent

					long expCap = (long)(living.ExperienceValue * 1.25);
					if (xpReward > expCap)
						xpReward = expCap;

					GameLiving.eXPSource xpSource = GameLiving.eXPSource.NPC;
					if (killedLiving is GamePlayer)
					{
						xpSource = GameLiving.eXPSource.Player;
					}

					if (xpReward > 0)
						living.GainExperience(xpSource, xpReward);

				}
			}
		}

		/// <summary>
		/// Invoked on Player death and deals out
		/// experience/realm points if needed
		/// </summary>
		/// <param name="killedPlayer">player that died</param>
		/// <param name="killer">killer</param>
		public virtual void OnPlayerKilled(GamePlayer killedPlayer, GameObject killer)
		{
			if (ServerProperties.Properties.ENABLE_WARMAPMGR && killer is GamePlayer && killer.CurrentRegion.ID == 163)
				WarMapMgr.AddFight((byte)killer.CurrentZone.ID, killer.X, killer.Y, (byte)killer.Realm, (byte)killedPlayer.Realm);

			killedPlayer.LastDeathRealmPoints = 0;
			// "player has been killed recently"
			long noExpSeconds = ServerProperties.Properties.RP_WORTH_SECONDS;
			if (killedPlayer.DBCharacter.DeathTime + noExpSeconds > killedPlayer.PlayedTime)
			{
				lock (killedPlayer.XPGainers.SyncRoot)
				{
					foreach (DictionaryEntry de in killedPlayer.XPGainers)
					{
						if (de.Key is GamePlayer)
						{
							((GamePlayer)de.Key).Out.SendMessage(killedPlayer.Name + " has been killed recently and is worth no realm points!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
							((GamePlayer)de.Key).Out.SendMessage(killedPlayer.Name + " has been killed recently and is worth no experience!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
						}
					}
				}
				return;
			}

			lock (killedPlayer.XPGainers.SyncRoot)
			{
				bool dealNoXP = false;
				float totalDamage = 0;
				//Collect the total damage
				foreach (DictionaryEntry de in killedPlayer.XPGainers)
				{
					GameObject obj = (GameObject)de.Key;
					if (obj is GamePlayer)
					{
						//If a gameplayer with privlevel > 1 attacked the
						//mob, then the players won't gain xp ...
						if (((GamePlayer)obj).Client.Account.PrivLevel > 1)
						{
							dealNoXP = true;
							break;
						}
					}
					totalDamage += (float)de.Value;
				}

				if (dealNoXP)
				{
					foreach (DictionaryEntry de in killedPlayer.XPGainers)
					{
						GamePlayer player = de.Key as GamePlayer;
						if (player != null)
							player.Out.SendMessage("You gain no experience from this kill!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					return;
				}


				long playerExpValue = killedPlayer.ExperienceValue;
				playerExpValue = (long)(playerExpValue * ServerProperties.Properties.XP_RATE);
				int playerRPValue = killedPlayer.RealmPointsValue;
				int playerBPValue = 0;

				bool BG = false;
				if (!ServerProperties.Properties.ALLOW_BPS_IN_BGS)
				{
					foreach (AbstractGameKeep keep in KeepMgr.GetKeepsOfRegion(killedPlayer.CurrentRegionID))
					{
						if (keep.DBKeep.BaseLevel < 50)
						{
							BG = true;
							break;
						}
					}
				}
				if (!BG)
					playerBPValue = killedPlayer.BountyPointsValue;
				long playerMoneyValue = killedPlayer.MoneyValue;

				List<KeyValuePair<GamePlayer, int>> playerKillers = new List<KeyValuePair<GamePlayer,int>>();

				//Now deal the XP and RPs to all livings
				foreach (DictionaryEntry de in killedPlayer.XPGainers)
				{
					GameLiving living = de.Key as GameLiving;
					GamePlayer expGainPlayer = living as GamePlayer;
					if (living == null) continue;
					if (living.ObjectState != GameObject.eObjectState.Active) continue;
					/*
					 * http://www.camelotherald.com/more/2289.shtml
					 * Dead players will now continue to retain and receive their realm point credit
					 * on targets until they release. This will work for solo players as well as
					 * grouped players in terms of continuing to contribute their share to the kill
					 * if a target is being attacked by another non grouped player as well.
					 */
					//if (!living.Alive) continue;
					if (!living.IsWithinRadius(killedPlayer, WorldMgr.MAX_EXPFORKILL_DISTANCE)) continue;


					double damagePercent = (float)de.Value / totalDamage;
					if (!living.IsAlive)//Dead living gets 25% exp only
						damagePercent *= 0.25;

					// realm points
					int rpCap = living.RealmPointsValue * 2;
					int realmPoints = (int)(playerRPValue * damagePercent);
					//rp bonuses from RR and Group
					//20% if R1L0 char kills RR10,if RR10 char kills R1L0 he will get -20% bonus
					//100% if full group,scales down according to player count in group and their range to target
					if (living is GamePlayer)
					{
						GamePlayer killerPlayer = living as GamePlayer;
						//only gain rps in a battleground if you are under the cap
						Battleground bg = KeepMgr.GetBattleground(killerPlayer.CurrentRegionID);
						if (bg == null || (killerPlayer.RealmLevel < bg.MaxRealmLevel))
						{
							realmPoints = (int)(realmPoints * (1.0 + 2.0 * (killedPlayer.RealmLevel - killerPlayer.RealmLevel) / 900.0));
							if (killerPlayer.Group != null && killerPlayer.Group.MemberCount > 1)
							{
								lock (killerPlayer.Group)
								{
									int count = 0;
									foreach (GamePlayer player in killerPlayer.Group.GetPlayersInTheGroup())
									{
										if (!player.IsWithinRadius(killedPlayer, WorldMgr.MAX_EXPFORKILL_DISTANCE)) continue;
										count++;
									}
									realmPoints = (int)(realmPoints * (1.0 + count * 0.125));
								}
							}
						}
						if (realmPoints > rpCap)
							realmPoints = rpCap;
						if (realmPoints > 0)
						{
							if (living is GamePlayer)
							{
								killedPlayer.LastDeathRealmPoints += realmPoints;
								playerKillers.Add(new KeyValuePair<GamePlayer, int>(living as GamePlayer, realmPoints));
							}

							living.GainRealmPoints(realmPoints);
						}
					}

					// bounty points
					int bpCap = living.BountyPointsValue * 2;
					int bountyPoints = (int)(playerBPValue * damagePercent);
					if (bountyPoints > bpCap)
						bountyPoints = bpCap;

					//FIXME: [WARN] this is guessed, i do not believe this is the right way, we will most likely need special messages to be sent
					//apply the keep bonus for bounty points
					if (killer != null)
					{
						if (Keeps.KeepBonusMgr.RealmHasBonus(eKeepBonusType.Bounty_Points_5, (eRealm)killer.Realm))
							bountyPoints += (bountyPoints / 100) * 5;
						else if (Keeps.KeepBonusMgr.RealmHasBonus(eKeepBonusType.Bounty_Points_3, (eRealm)killer.Realm))
							bountyPoints += (bountyPoints / 100) * 3;
					}

					if (bountyPoints > 0)
					{
						living.GainBountyPoints(bountyPoints);
					}

					// experience
					// TODO: pets take 25% and owner gets 75%
					long xpReward = (long)(playerExpValue * damagePercent); // exp for damage percent

					long expCap = (long)(living.ExperienceValue * ServerProperties.Properties.XP_PVP_CAP_PERCENT / 100);
					if (xpReward > expCap)
						xpReward = expCap;

					//outpost XP
					//1.54 http://www.camelotherald.com/more/567.shtml
					//- Players now receive an exp bonus when fighting within 16,000
					//units of a keep controlled by your realm or your guild.
					//You get 20% bonus if your guild owns the keep or a 10% bonus
					//if your realm owns the keep.

					long outpostXP = 0;

					if (!BG && living is GamePlayer)
					{
						AbstractGameKeep keep = KeepMgr.getKeepCloseToSpot(living.CurrentRegionID, living, 16000);
						if (keep != null)
						{
							byte bonus = 0;
							if (keep.Guild != null && keep.Guild == (living as GamePlayer).Guild)
								bonus = 20;
							else if (GameServer.Instance.Configuration.ServerType == eGameServerType.GST_Normal &&
							         keep.Realm == living.Realm)
								bonus = 10;

							outpostXP = (xpReward / 100) * bonus;
						}
					}
					xpReward += outpostXP;

					living.GainExperience(GameLiving.eXPSource.Player, xpReward);

					//gold
					if (living is GamePlayer)
					{
						long money = (long)(playerMoneyValue * damagePercent);
						GamePlayer player = living as GamePlayer;
						if (player.GetSpellLine("Spymaster") != null)
						{
							money += 20 * money / 100;
						}
						//long money = (long)(Money.GetMoney(0, 0, 17, 85, 0) * damagePercent * killedPlayer.Level / 50);
						((GamePlayer)living).AddMoney(money, "You recieve {0}");
					}

					if (killedPlayer.ReleaseType != GamePlayer.eReleaseType.Duel && expGainPlayer != null)
					{
						switch ((eRealm)killedPlayer.Realm)
						{
							case eRealm.Albion:
								expGainPlayer.KillsAlbionPlayers++;
								if (expGainPlayer == killer)
								{
									expGainPlayer.KillsAlbionDeathBlows++;
									if ((float)de.Value == totalDamage)
										expGainPlayer.KillsAlbionSolo++;
								}
								break;

							case eRealm.Hibernia:
								expGainPlayer.KillsHiberniaPlayers++;
								if (expGainPlayer == killer)
								{
									expGainPlayer.KillsHiberniaDeathBlows++;
									if ((float)de.Value == totalDamage)
										expGainPlayer.KillsHiberniaSolo++;
								}
								break;

							case eRealm.Midgard:
								expGainPlayer.KillsMidgardPlayers++;
								if (expGainPlayer == killer)
								{
									expGainPlayer.KillsMidgardDeathBlows++;
									if ((float)de.Value == totalDamage)
										expGainPlayer.KillsMidgardSolo++;
								}
								break;
						}
						killedPlayer.DeathsPvP++;
					}
				}

				if (ServerProperties.Properties.LOG_PVP_KILLS && playerKillers.Count > 0)
				{
					try
					{
						foreach (KeyValuePair<GamePlayer, int> pair in playerKillers)
						{

							DOL.Database.PvPKillsLog killLog = new DOL.Database.PvPKillsLog();
							killLog.KilledIP = killedPlayer.Client.TcpEndpointAddress;
							killLog.KilledName = killedPlayer.Name;
							killLog.KilledRealm = GlobalConstants.RealmToName(killedPlayer.Realm);
							killLog.KillerIP = pair.Key.Client.TcpEndpointAddress;
							killLog.KillerName = pair.Key.Name;
							killLog.KillerRealm = GlobalConstants.RealmToName(pair.Key.Realm);
							killLog.RPReward = pair.Value;
							killLog.RegionName = killedPlayer.CurrentRegion.Description;
							killLog.IsInstance = killedPlayer.CurrentRegion.IsInstance;

							if (killedPlayer.Client.TcpEndpointAddress == pair.Key.Client.TcpEndpointAddress)
								killLog.SameIP = 1;

							GameServer.Database.AddObject(killLog);
						}
					}
					catch (System.Exception ex)
					{
						log.Error(ex);
					}
				}
			}
		}

		/// <summary>
		/// Gets the Realm of an living for name text coloring
		/// </summary>
		/// <param name="player"></param>
		/// <param name="target"></param>
		/// <returns>byte code of realm</returns>
		public virtual byte GetLivingRealm(GamePlayer player, GameLiving target)
		{
			if (player == null || target == null) return 0;

			// clients with priv level > 1 are considered friendly by anyone
			GamePlayer playerTarget = target as GamePlayer;
			if (playerTarget != null && playerTarget.Client.Account.PrivLevel > 1) return (byte)player.Realm;

			return (byte)target.Realm;
		}

		/// <summary>
		/// Gets the player name based on server type
		/// </summary>
		/// <param name="source">The "looking" player</param>
		/// <param name="target">The considered player</param>
		/// <returns>The name of the target</returns>
		public virtual string GetPlayerName(GamePlayer source, GamePlayer target)
		{
			return target.Name;
		}

		/// <summary>
		/// Gets the player Realmrank 12 or 13 title
		/// </summary>
		/// <param name="source">The "looking" player</param>
		/// <param name="target">The considered player</param>
		/// <returns>The Realmranktitle of the target</returns>
		public virtual string GetPlayerPrefixName(GamePlayer source, GamePlayer target)
		{
			if (IsSameRealm(source, target, true))
				return target.PrefixName;
			return string.Empty;
		}

		/// <summary>
		/// Gets the player last name based on server type
		/// </summary>
		/// <param name="source">The "looking" player</param>
		/// <param name="target">The considered player</param>
		/// <returns>The last name of the target</returns>
		public virtual string GetPlayerLastName(GamePlayer source, GamePlayer target)
		{
			return target.LastName;
		}

		/// <summary>
		/// Gets the player guild name based on server type
		/// </summary>
		/// <param name="source">The "looking" player</param>
		/// <param name="target">The considered player</param>
		/// <returns>The guild name of the target</returns>
		public virtual string GetPlayerGuildName(GamePlayer source, GamePlayer target)
		{
			return target.GuildName;
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
		public virtual byte GetColorHandling(GameClient client)
		{
			return 0;
		}

		/// <summary>
		/// Formats player statistics.
		/// </summary>
		/// <param name="player">The player to read statistics from.</param>
		/// <returns>List of strings.</returns>
		public virtual IList<string> FormatPlayerStatistics(GamePlayer player)
		{
			List<string> stat = new List<string>();

			int total = 0;
			#region Players Killed
			//only show if there is a kill [by Suncheck]
			if ((player.KillsAlbionPlayers + player.KillsMidgardPlayers + player.KillsHiberniaPlayers) > 0)
			{
				stat.Add(LanguageMgr.GetTranslation(player.Client, "PlayerStatistic.Kill.Title"));
				switch ((eRealm)player.Realm)
				{
					case eRealm.Albion:
						if (player.KillsMidgardPlayers > 0) stat.Add(LanguageMgr.GetTranslation(player.Client, "PlayerStatistic.Kill.MidgardPlayer") + ": " + player.KillsMidgardPlayers.ToString("N0"));
						if (player.KillsHiberniaPlayers > 0) stat.Add(LanguageMgr.GetTranslation(player.Client, "PlayerStatistic.Kill.HiberniaPlayer") + ": " + player.KillsHiberniaPlayers.ToString("N0"));
						total = player.KillsMidgardPlayers + player.KillsHiberniaPlayers;
						break;
					case eRealm.Midgard:
						if (player.KillsAlbionPlayers > 0) stat.Add(LanguageMgr.GetTranslation(player.Client, "PlayerStatistic.Kill.AlbionPlayer") + ": " + player.KillsAlbionPlayers.ToString("N0"));
						if (player.KillsHiberniaPlayers > 0) stat.Add(LanguageMgr.GetTranslation(player.Client, "PlayerStatistic.Kill.HiberniaPlayer") + ": " + player.KillsHiberniaPlayers.ToString("N0"));
						total = player.KillsAlbionPlayers + player.KillsHiberniaPlayers;
						break;
					case eRealm.Hibernia:
						if (player.KillsAlbionPlayers > 0) stat.Add(LanguageMgr.GetTranslation(player.Client, "PlayerStatistic.Kill.AlbionPlayer") + ": " + player.KillsAlbionPlayers.ToString("N0"));
						if (player.KillsMidgardPlayers > 0) stat.Add(LanguageMgr.GetTranslation(player.Client, "PlayerStatistic.Kill.MidgardPlayer") + ": " + player.KillsMidgardPlayers.ToString("N0"));
						total = player.KillsMidgardPlayers + player.KillsAlbionPlayers;
						break;
				}
				stat.Add(LanguageMgr.GetTranslation(player.Client, "PlayerStatistic.Kill.TotalPlayers") + ": " + total.ToString("N0"));
			}
			#endregion
			stat.Add(" ");
			#region Players Deathblows
			//only show if there is a kill [by Suncheck]
			if ((player.KillsAlbionDeathBlows + player.KillsMidgardDeathBlows + player.KillsHiberniaDeathBlows) > 0)
			{
				total = 0;
				switch ((eRealm)player.Realm)
				{
					case eRealm.Albion:
						if (player.KillsMidgardDeathBlows > 0) stat.Add(LanguageMgr.GetTranslation(player.Client, "PlayerStatistic.Deathblows.MidgardPlayer") + ": " + player.KillsMidgardDeathBlows.ToString("N0"));
						if (player.KillsHiberniaDeathBlows > 0) stat.Add(LanguageMgr.GetTranslation(player.Client, "PlayerStatistic.Deathblows.HiberniaPlayer") + ": " + player.KillsHiberniaDeathBlows.ToString("N0"));
						total = player.KillsMidgardDeathBlows + player.KillsHiberniaDeathBlows;
						break;
					case eRealm.Midgard:
						if (player.KillsAlbionDeathBlows > 0) stat.Add(LanguageMgr.GetTranslation(player.Client, "PlayerStatistic.Deathblows.AlbionPlayer") + ": " + player.KillsAlbionDeathBlows.ToString("N0"));
						if (player.KillsHiberniaDeathBlows > 0) stat.Add(LanguageMgr.GetTranslation(player.Client, "PlayerStatistic.Deathblows.HiberniaPlayer") + ": " + player.KillsHiberniaDeathBlows.ToString("N0"));
						total = player.KillsAlbionDeathBlows + player.KillsHiberniaDeathBlows;
						break;
					case eRealm.Hibernia:
						if (player.KillsAlbionDeathBlows > 0) stat.Add(LanguageMgr.GetTranslation(player.Client, "PlayerStatistic.Deathblows.AlbionPlayer") + ": " + player.KillsAlbionDeathBlows.ToString("N0"));
						if (player.KillsMidgardDeathBlows > 0) stat.Add(LanguageMgr.GetTranslation(player.Client, "PlayerStatistic.Deathblows.MidgardPlayer") + ": " + player.KillsMidgardDeathBlows.ToString("N0"));
						total = player.KillsMidgardDeathBlows + player.KillsAlbionDeathBlows;
						break;
				}
				stat.Add(LanguageMgr.GetTranslation(player.Client, "PlayerStatistic.Deathblows.TotalPlayers") + ": " + total.ToString("N0"));
			}
			#endregion
			stat.Add(" ");
			#region Players Solo Kills
			//only show if there is a kill [by Suncheck]
			if ((player.KillsAlbionSolo + player.KillsMidgardSolo + player.KillsHiberniaSolo) > 0)
			{
				total = 0;
				switch ((eRealm)player.Realm)
				{
					case eRealm.Albion:
						if (player.KillsMidgardSolo > 0) stat.Add(LanguageMgr.GetTranslation(player.Client, "PlayerStatistic.Solo.MidgardPlayer") + ": " + player.KillsMidgardSolo.ToString("N0"));
						if (player.KillsHiberniaSolo > 0) stat.Add(LanguageMgr.GetTranslation(player.Client, "PlayerStatistic.Solo.HiberniaPlayer") + ": " + player.KillsHiberniaSolo.ToString("N0"));
						total = player.KillsMidgardSolo + player.KillsHiberniaSolo;
						break;
					case eRealm.Midgard:
						if (player.KillsAlbionSolo > 0) stat.Add(LanguageMgr.GetTranslation(player.Client, "PlayerStatistic.Solo.AlbionPlayer") + ": " + player.KillsAlbionSolo.ToString("N0"));
						if (player.KillsHiberniaSolo > 0) stat.Add(LanguageMgr.GetTranslation(player.Client, "PlayerStatistic.Solo.HiberniaPlayer") + ": " + player.KillsHiberniaSolo.ToString("N0"));
						total = player.KillsAlbionSolo + player.KillsHiberniaSolo;
						break;
					case eRealm.Hibernia:
						if (player.KillsAlbionSolo > 0) stat.Add(LanguageMgr.GetTranslation(player.Client, "PlayerStatistic.Solo.AlbionPlayer") + ": " + player.KillsAlbionSolo.ToString("N0"));
						if (player.KillsMidgardSolo > 0) stat.Add(LanguageMgr.GetTranslation(player.Client, "PlayerStatistic.Solo.MidgardPlayer") + ": " + player.KillsMidgardSolo.ToString("N0"));
						total = player.KillsMidgardSolo + player.KillsAlbionSolo;
						break;
				}
				stat.Add(LanguageMgr.GetTranslation(player.Client, "PlayerStatistic.Solo.TotalPlayers") + ": " + total.ToString("N0"));
			}
			#endregion
			stat.Add(" ");
			#region Keeps
			//only show if there is a capture [by Suncheck]
			if ((player.CapturedKeeps + player.CapturedTowers) > 0)
			{
				stat.Add(LanguageMgr.GetTranslation(player.Client, "PlayerStatistic.Capture.Title"));
				//stat.Add("Relics Taken: " + player.RelicsTaken.ToString("N0"));
				//stat.Add("Albion Keeps Captured: " + player.CapturedAlbionKeeps.ToString("N0"));
				//stat.Add("Midgard Keeps Captured: " + player.CapturedMidgardKeeps.ToString("N0"));
				//stat.Add("Hibernia Keeps Captured: " + player.CapturedHiberniaKeeps.ToString("N0"));
				if (player.CapturedKeeps > 0) stat.Add(LanguageMgr.GetTranslation(player.Client, "PlayerStatistic.Capture.Keeps") + ": " + player.CapturedKeeps.ToString("N0"));
				//stat.Add("Keep Lords Slain: " + player.KeepLordsSlain.ToString("N0"));
				//stat.Add("Albion Towers Captured: " + player.CapturedAlbionTowers.ToString("N0"));
				//stat.Add("Midgard Towers Captured: " + player.CapturedMidgardTowers.ToString("N0"));
				//stat.Add("Hibernia Towers Captured: " + player.CapturedHiberniaTowers.ToString("N0"));
				if (player.CapturedTowers > 0) stat.Add(LanguageMgr.GetTranslation(player.Client, "PlayerStatistic.Capture.Towers") + ": " + player.CapturedTowers.ToString("N0"));
				//stat.Add("Tower Captains Slain: " + player.TowerCaptainsSlain.ToString("N0"));
				//stat.Add("Realm Guard Kills Albion: " + player.RealmGuardTotalKills.ToString("N0"));
				//stat.Add("Realm Guard Kills Midgard: " + player.RealmGuardTotalKills.ToString("N0"));
				//stat.Add("Realm Guard Kills Hibernia: " + player.RealmGuardTotalKills.ToString("N0"));
				//stat.Add("Total Realm Guard Kills: " + player.RealmGuardTotalKills.ToString("N0"));
			}
			#endregion
			stat.Add(" ");
			#region PvE
			//only show if there is a kill [by Suncheck]
			if ((player.KillsDragon + player.KillsEpicBoss + player.KillsLegion) > 0)
			{
				stat.Add(LanguageMgr.GetTranslation(player.Client, "PlayerStatistic.PvE.Title"));
				if (player.KillsDragon > 0) stat.Add(LanguageMgr.GetTranslation(player.Client, "PlayerStatistic.PvE.KillsDragon") + ": " + player.KillsDragon.ToString("N0"));
				if (player.KillsEpicBoss > 0) stat.Add(LanguageMgr.GetTranslation(player.Client, "PlayerStatistic.PvE.KillsEpic") + ": " + player.KillsEpicBoss.ToString("N0"));
				if (player.KillsLegion > 0) stat.Add(LanguageMgr.GetTranslation(player.Client, "PlayerStatistic.PvE.KillsLegion") + ": " + player.KillsLegion.ToString("N0"));
			}
			#endregion

			return stat;
		}

		/// <summary>
		/// Reset the keep with special server rules handling
		/// </summary>
		/// <param name="lord">The lord that was killed</param>
		/// <param name="killer">The lord's killer</param>
		public virtual void ResetKeep(GuardLord lord, GameObject killer)
		{
			PlayerMgr.UpdateStats(lord);
		}

		/// <summary>
		/// Experience a keep is worth when captured
		/// </summary>
		/// <param name="keep"></param>
		/// <returns></returns>
		public virtual long GetExperienceForKeep(AbstractGameKeep keep)
		{
			return 0;
		}

		public virtual double GetExperienceCapForKeep(AbstractGameKeep keep)
		{
			return 1.0;
		}

		/// <summary>
		/// Realm points a keep is worth when captured
		/// </summary>
		/// <param name="keep"></param>
		/// <returns></returns>
		public virtual int GetRealmPointsForKeep(AbstractGameKeep keep)
		{
			int value = 0;

			if (keep is GameKeep)
			{
				value = Math.Max(50, ServerProperties.Properties.KEEP_RP_BASE + ((keep.BaseLevel - 50) * ServerProperties.Properties.KEEP_RP_MULTIPLIER));
			}
			else
			{
				value = Math.Max(5, ServerProperties.Properties.TOWER_RP_BASE + ((keep.BaseLevel - 50) * ServerProperties.Properties.TOWER_RP_MULTIPLIER));
			}

			value += ((keep.Level - ServerProperties.Properties.STARTING_KEEP_LEVEL) * ServerProperties.Properties.UPGRADE_MULTIPLIER);

			return Math.Max(5, value);
		}

		/// <summary>
		/// Bounty points a keep is worth when captured
		/// </summary>
		/// <param name="keep"></param>
		/// <returns></returns>
		public virtual int GetBountyPointsForKeep(AbstractGameKeep keep)
		{
			return 0;
		}


		/// <summary>
		/// How much money does this keep reward when captured
		/// </summary>
		/// <param name="keep"></param>
		/// <returns></returns>
		public virtual long GetMoneyValueForKeep(AbstractGameKeep keep)
		{
			return 0;
		}


		/// <summary>
		/// Is the player allowed to generate news
		/// </summary>
		/// <param name="player">the player</param>
		/// <returns>true if the player is allowed to generate news</returns>
		public virtual bool CanGenerateNews(GamePlayer player)
		{
			if (player.Client.Account.PrivLevel > 1)
				return false;

			return true;
		}

		/// <summary>
		/// Gets the NPC name based on server type
		/// </summary>
		/// <param name="source">The "looking" player</param>
		/// <param name="target">The considered NPC</param>
		/// <returns>The name of the target</returns>
		public virtual string GetNPCName(GamePlayer source, GameNPC target)
		{
			return target.Name;
		}
		
		/// <summary>
		/// Gets the NPC guild name based on server type
		/// </summary>
		/// <param name="source">The "looking" player</param>
		/// <param name="target">The considered NPC</param>
		/// <returns>The guild name of the target</returns>
		public virtual string GetNPCGuildName(GamePlayer source, GameNPC target)
		{
			return target.GuildName;
		}
		
		#region MessageToLiving
		/// <summary>
		/// Send system text message to system window
		/// </summary>
		/// <param name="living"></param>
		/// <param name="message"></param>
		public virtual void MessageToLiving(GameLiving living, string message)
		{
			MessageToLiving(living, message, eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}
		/// <summary>
		/// Send custom text message to system window
		/// </summary>
		/// <param name="living"></param>
		/// <param name="message"></param>
		/// <param name="type"></param>
		public virtual void MessageToLiving(GameLiving living, string message, eChatType type)
		{
			MessageToLiving(living, message, type, eChatLoc.CL_SystemWindow);
		}
		/// <summary>
		/// Send custom text message to GameLiving
		/// </summary>
		/// <param name="living"></param>
		/// <param name="message"></param>
		/// <param name="type"></param>
		/// <param name="loc"></param>
		public virtual void MessageToLiving(GameLiving living, string message, eChatType type, eChatLoc loc)
		{
			if (living is GamePlayer)
				((GamePlayer)living).Out.SendMessage(message, type, loc);
		}
		#endregion
	}
}
