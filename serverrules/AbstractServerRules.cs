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
using System.Net;
using System.Reflection;

using DOL.Database;
using DOL.GS;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;
using DOL.GS.ServerProperties;

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
			string accip = ((IPEndPoint)client.Socket.RemoteEndPoint).Address.ToString();

			// Ban account
			DataObject[] objs;
			objs = GameServer.Database.SelectObjects(typeof(DBBannedAccount), "(Type ='Account' AND Account ='" + GameServer.Database.Escape(username) + "') OR (Type ='Account+Ip' AND Account ='" + GameServer.Database.Escape(username) + "')");
			if (objs.Length > 0)
			{
				client.Out.SendLoginDenied(eLoginError.AccountIsBannedFromThisServerType);
				return false;
			}

			// Ban IP Adress
			objs = GameServer.Database.SelectObjects(typeof(DBBannedAccount), "(Type = 'Ip' AND Ip ='" + GameServer.Database.Escape(accip) + "') OR (Type ='Account+Ip' AND Ip ='" + GameServer.Database.Escape(accip) + "')");
			if (objs.Length > 0)
			{
				client.Out.SendLoginDenied(eLoginError.AccountIsBannedFromThisServerType);
				return false;
			}

			GameClient.eClientVersion min = (GameClient.eClientVersion)Properties.CLIENT_VERSION_MIN;
			if (min != GameClient.eClientVersion.VersionNotChecked && client.Version < min)
			{
				client.Out.SendLoginDenied(eLoginError.ClientVersionTooLow);
				return false;
			}

			GameClient.eClientVersion max = (GameClient.eClientVersion)Properties.CLIENT_VERSION_MAX;
			if (max != GameClient.eClientVersion.VersionNotChecked && client.Version > max)
			{
				client.Out.SendLoginDenied(eLoginError.ClientVersionTooLow);
				return false;
			}

			if (Properties.CLIENT_TYPE_MAX > -1)
			{
				GameClient.eClientType type = (GameClient.eClientType)Properties.CLIENT_TYPE_MAX;
				if ((int)client.ClientType > (int)type )
				{
					client.Out.SendLoginDenied(eLoginError.ExpansionPacketNotAllowed);
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

			if (Properties.MAX_PLAYERS > 0)
			{
				if (WorldMgr.GetAllClients().Count > Properties.MAX_PLAYERS)
				{
					// GMs are still allowed to enter server
					objs = GameServer.Database.SelectObjects(typeof(Account), string.Format("Name = '{0}'", GameServer.Database.Escape(username)));
					if (objs.Length > 0)
					{
						Account account = objs[0] as Account;
						if (account.PrivLevel > 1) return true;
					}

					// Normal Players will not be allowed over the max
					client.Out.SendLoginDenied(eLoginError.TooManyPlayersLoggedIn);
					return false;
				}
			}

			if (Properties.STAFF_LOGIN)
			{
				// GMs are still allowed to enter server
				objs = GameServer.Database.SelectObjects(typeof(Account), string.Format("Name = '{0}'", GameServer.Database.Escape(username)));
				if (objs.Length > 0)
				{
					Account account = objs[0] as Account;
					if (account.PrivLevel > 1) return true;
				}

				// Normal Players will not be allowed to log in
				client.Out.SendLoginDenied(eLoginError.GameCurrentlyClosed);
				return false;
			}

			return true;
		}

		public abstract bool IsSameRealm(GameLiving source, GameLiving target, bool quiet);
		public abstract bool IsAllowedCharsInAllRealms(GameClient client);
		public abstract bool IsAllowedToGroup(GamePlayer source, GamePlayer target, bool quiet);
		public abstract bool IsAllowedToTrade(GameLiving source, GameLiving target, bool quiet);
		public abstract bool IsAllowedToUnderstand(GameLiving source, GamePlayer target);
		public abstract string RulesDescription();

		/// <summary>
		/// Is attacker allowed to attack defender.
		/// </summary>
		/// <param name="attacker">living that makes attack</param>
		/// <param name="defender">attacker's target</param>
		/// <param name="quiet">should messages be sent</param>
		/// <returns>true if attack is allowed</returns>
		public virtual bool IsAllowedToAttack(GameLiving attacker, GameLiving defender, bool quiet)
		{
			if (attacker == null || defender == null) return false;

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

			foreach (AbstractArea area in defender.CurrentAreas)
			{
				if (area.IsSafeArea)
				{
					if (quiet == false) MessageToLiving(attacker, "You can't attack someone in a safe area!");
					return false;
				}
			}

			foreach (AbstractArea area in attacker.CurrentAreas)
			{
				if (area.IsSafeArea)
				{
					if (quiet == false) MessageToLiving(attacker, "You can't attack someone in a safe area!");
					return false;
				}
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

		public virtual bool CanTakeFallDamage(GamePlayer player)
		{
			if (player.Client.Account.PrivLevel > 1)
				return false;
			return true;
		}

		public virtual long GetExperienceForLevel(int level)
		{
			if (level > GamePlayer.MAX_LEVEL)
				return GamePlayer.XPLevel[GamePlayer.MAX_LEVEL]; // exp for level 51, needed to get exp after 50
			if (level <= 0)
				return GamePlayer.XPLevel[0];
			return GamePlayer.XPLevel[level - 1];
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

				//alb
				case eObjectType.CrushingWeapon: oneHandCheck = new string[] { Abilities.Weapon_Crushing, Abilities.Weapon_Blunt, Abilities.Weapon_Hammers }; break;
				case eObjectType.SlashingWeapon: oneHandCheck = new string[] { Abilities.Weapon_Slashing, Abilities.Weapon_Blades, Abilities.Weapon_Swords, Abilities.Weapon_Axes }; break;
				case eObjectType.ThrustWeapon: oneHandCheck = new string[] { Abilities.Weapon_Thrusting, Abilities.Weapon_Piercing }; break;
				case eObjectType.Flexible: oneHandCheck = new string[] { Abilities.Weapon_Flexible }; break;
				case eObjectType.TwoHandedWeapon: twoHandCheck = new string[] { Abilities.Weapon_TwoHanded, Abilities.Weapon_LargeWeapons }; break;
				case eObjectType.PolearmWeapon: twoHandCheck = new string[] { Abilities.Weapon_Polearms, Abilities.Weapon_CelticSpear, Abilities.Weapon_Spears }; break;
				case eObjectType.Longbow: otherCheck = new string[] { Abilities.Weapon_Longbows }; break;
				case eObjectType.Crossbow: otherCheck = new string[] { Abilities.Weapon_Crossbow }; break;

				//mid
				case eObjectType.Sword: oneHandCheck = new string[] { Abilities.Weapon_Swords, Abilities.Weapon_Slashing, Abilities.Weapon_Blades }; twoHandCheck = new string[] { Abilities.Weapon_Swords }; break;
				case eObjectType.Hammer: oneHandCheck = new string[] { Abilities.Weapon_Hammers, Abilities.Weapon_Crushing, Abilities.Weapon_Blunt }; twoHandCheck = new string[] { Abilities.Weapon_Hammers }; break;
				case eObjectType.LeftAxe:
				case eObjectType.Axe: oneHandCheck = new string[] { Abilities.Weapon_Axes, Abilities.Weapon_Slashing, Abilities.Weapon_Blades }; twoHandCheck = new string[] { Abilities.Weapon_Axes }; break;
				case eObjectType.HandToHand: oneHandCheck = new string[] { Abilities.Weapon_HandToHand }; break;
				case eObjectType.Spear: twoHandCheck = new string[] { Abilities.Weapon_Spears, Abilities.Weapon_CelticSpear, Abilities.Weapon_Polearms }; break;
				case eObjectType.CompositeBow: otherCheck = new string[] { Abilities.Weapon_CompositeBows }; break;
				case eObjectType.Thrown: otherCheck = new string[] { Abilities.Weapon_Thrown }; break;

				//hib
				case eObjectType.Blades: oneHandCheck = new string[] { Abilities.Weapon_Blades, Abilities.Weapon_Slashing, Abilities.Weapon_Swords, Abilities.Weapon_Axes }; break;
				case eObjectType.Blunt: oneHandCheck = new string[] { Abilities.Weapon_Blunt, Abilities.Weapon_Crushing, Abilities.Weapon_Hammers }; break;
				case eObjectType.Piercing: oneHandCheck = new string[] { Abilities.Weapon_Piercing, Abilities.Weapon_Thrusting }; break;
				case eObjectType.LargeWeapons: twoHandCheck = new string[] { Abilities.Weapon_LargeWeapons, Abilities.Weapon_TwoHanded }; break;
				case eObjectType.CelticSpear: twoHandCheck = new string[] { Abilities.Weapon_CelticSpear, Abilities.Weapon_Spears, Abilities.Weapon_Polearms }; break;
				case eObjectType.Scythe: twoHandCheck = new string[] { Abilities.Weapon_Scythe }; break;
				case eObjectType.RecurvedBow: otherCheck = new string[] { Abilities.Weapon_RecurvedBows }; break;

				//misc
				case eObjectType.Magical: return true;
				case eObjectType.Shield: return living.GetAbilityLevel(Abilities.Shield) >= item.Type_Damage;
				case eObjectType.Arrow: otherCheck = new string[] { Abilities.Weapon_CompositeBows, Abilities.Weapon_Longbows, Abilities.Weapon_RecurvedBows, Abilities.Weapon_Shortbows }; break;
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
			//"This monster has been charmed recently and is worth no experience."

			string message = "You gain no experience from this kill!";
			if (killedNPC.CurrentRegion.Time - GameNPC.CHARMED_NOEXP_TIMEOUT < killedNPC.TempProperties.getLongProperty(GameNPC.CHARMED_TICK_PROP, 0L))
			{
				message = "This monster has been charmed recently and is worth no experience.";
			}

			lock (killedNPC.XPGainers.SyncRoot)
			{
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

				float totalDamage = 0;
				Hashtable plrGrpExp = new Hashtable();
				//Collect the total damage
				foreach (DictionaryEntry de in killedNPC.XPGainers)
				{
					totalDamage += (float)de.Value;
					GamePlayer player = de.Key as GamePlayer;
					if (player == null) continue;
					if (player.ObjectState != GameObject.eObjectState.Active) continue;
					//					if (!player.Alive) continue;
					if (!WorldMgr.CheckDistance(player, killedNPC, WorldMgr.MAX_EXPFORKILL_DISTANCE)) continue;
					if (player.PlayerGroup != null)
					{
						if (plrGrpExp.ContainsKey(player.PlayerGroup))
							plrGrpExp[player.PlayerGroup] = (int)plrGrpExp[player.PlayerGroup] + 1;
						else
							plrGrpExp[player.PlayerGroup] = 1;
					}
				}

				long npcExpValue = killedNPC.ExperienceValue;
				int npcRPValue = killedNPC.RealmPointsValue;
				int npcBPValue = killedNPC.BountyPointsValue;

				npcExpValue = (long)(npcExpValue * ServerProperties.Properties.XP_RATE);

				//Now deal the XP to all livings
				foreach (DictionaryEntry de in killedNPC.XPGainers)
				{
					GameLiving living = de.Key as GameLiving;
					if (living == null) continue;
					if (living.ObjectState != GameObject.eObjectState.Active) continue;
					//if (!living.Alive) continue;
					if (!WorldMgr.CheckDistance(living, killedNPC, WorldMgr.MAX_EXPFORKILL_DISTANCE)) continue;
					// TODO: pets take 25% and owner gets 75%
					double damagePercent = (float)de.Value / totalDamage;

					// realm points
					int rpCap = living.RealmPointsValue * 2;
					int realmPoints = (int)(npcRPValue * damagePercent);
					//rp bonuses from RR and Group
					//20% if R1L0 char kills RR10,if RR10 char kills R1L0 he will get -20% bonus
					//100% if full group,scales down according to player count in group and their range to target
					if (living is GamePlayer)
					{
						GamePlayer killerPlayer = living as GamePlayer;
						if (killerPlayer.PlayerGroup != null && killerPlayer.PlayerGroup.PlayerCount > 1)
						{
							lock (killerPlayer.PlayerGroup)
							{
								int count = 0;
								foreach (GamePlayer player in killerPlayer.PlayerGroup.GetPlayersInTheGroup())
								{
									if (!WorldMgr.CheckDistance(player, killedNPC, WorldMgr.MAX_EXPFORKILL_DISTANCE)) continue;
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
					int bountyPoints = (int)(npcBPValue * damagePercent);
					if (bountyPoints > bpCap)
						bountyPoints = bpCap;
					if (bountyPoints != 0)
					{
						living.GainBountyPoints(bountyPoints);
					}

					// experience points
					long xpReward = (long)(npcExpValue * damagePercent); // exp for damage percent

					// camp bonus
					const double fullCampBonus = 2.0;
					const double fullCampBonusTicks = (1000 * 60 * 60); //1 hour (in ms) = full 100%
					long livingLifeSpan = killedNPC.CurrentRegion.Time - killedNPC.SpawnTick;

					// exp cap
					long expCap = (long)(living.ExperienceValue * 1.25);
					if (xpReward > expCap)
						xpReward = expCap;

					double campBonus = fullCampBonus * (livingLifeSpan / fullCampBonusTicks);
					//1.49 http://news-daoc.goa.com/view_patchnote_archive.php?id_article=2478
					//"Camp bonuses" have been substantially upped in dungeons. Now camp bonuses in dungeons are, on average, 20% higher than outside camp bonuses.
					if (killer.CurrentZone.IsDungeon)
						campBonus += 0.20;

					if (campBonus < 0.01)
						campBonus = 0;
					else if (campBonus > fullCampBonus)
						campBonus = fullCampBonus;

					campBonus = xpReward * campBonus;

					if (!living.IsAlive)//Dead living gets 25% exp only
					{
						campBonus = (long)(campBonus * 0.25);
						xpReward = (long)(xpReward * 0.25);
					}

					long groupExp = 0;

					if (xpReward > 0)
					{
						if (living is GamePlayer)
						{
							GamePlayer player = living as GamePlayer;
							if (player != null && player.PlayerGroup != null && plrGrpExp.ContainsKey(player.PlayerGroup))
								groupExp += (long)(0.125 * xpReward * (int)plrGrpExp[player.PlayerGroup]);
						}

						xpReward += (long)campBonus + groupExp;
						living.GainExperience(xpReward, (long)campBonus, groupExp, true, false);
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
					if (!WorldMgr.CheckDistance(living, killedLiving, WorldMgr.MAX_EXPFORKILL_DISTANCE))
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
						if (killerPlayer.PlayerGroup != null && killerPlayer.PlayerGroup.PlayerCount > 1)
						{
							lock (killerPlayer.PlayerGroup)
							{
								int count = 0;
								foreach (GamePlayer player in killerPlayer.PlayerGroup.GetPlayersInTheGroup())
								{
									if (!WorldMgr.CheckDistance(player, killedLiving, WorldMgr.MAX_EXPFORKILL_DISTANCE)) continue;
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

					if (xpReward > 0)
						living.GainExperience(xpReward);

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
			killedPlayer.LastDeathRealmPoints = 0;
			// "player has been killed recently"
			long noExpSeconds = ServerProperties.Properties.RP_WORTH_SECONDS;
			if (killedPlayer.PlayerCharacter.DeathTime + noExpSeconds > killedPlayer.PlayedTime)
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
				foreach (AbstractGameKeep keep in KeepMgr.GetKeepsOfRegion(killedPlayer.CurrentRegionID))
				{
					if (keep.BaseLevel < 50)
					{
						BG = true;
						break;
					}
				}
				if (!BG)
					playerBPValue = killedPlayer.BountyPointsValue;
				long playerMoneyValue = killedPlayer.MoneyValue;

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
					if (!WorldMgr.CheckDistance(living, killedPlayer, WorldMgr.MAX_EXPFORKILL_DISTANCE)) continue;


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
						realmPoints = (int)(realmPoints * (1.0 + 2.0 * (killedPlayer.RealmLevel - killerPlayer.RealmLevel) / 900.0));
						if (killerPlayer.PlayerGroup != null && killerPlayer.PlayerGroup.PlayerCount > 1)
						{
							lock (killerPlayer.PlayerGroup)
							{
								int count = 0;
								foreach (GamePlayer player in killerPlayer.PlayerGroup.GetPlayersInTheGroup())
								{
									if (!WorldMgr.CheckDistance(player, killedPlayer, WorldMgr.MAX_EXPFORKILL_DISTANCE)) continue;
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
						if (living is GamePlayer)
							killedPlayer.LastDeathRealmPoints += realmPoints;
						living.GainRealmPoints(realmPoints);
					}

					// bounty points
					int bpCap = living.BountyPointsValue * 2;
					int bountyPoints = (int)(playerBPValue * damagePercent);
					if (bountyPoints > bpCap)
						bountyPoints = bpCap;
					if (bountyPoints != 0)
					{
						living.GainBountyPoints(bountyPoints);
					}

					// experience
					// TODO: pets take 25% and owner gets 75%
					long xpReward = (long)(playerExpValue * damagePercent); // exp for damage percent

					long expCap = (long)(living.ExperienceValue * 1.25);
					if (xpReward > expCap)
						xpReward = expCap;

					living.GainExperience(xpReward, 0, 0, true, false);

					//gold
					if (living is GamePlayer)
					{
						long money = (long)(playerMoneyValue * damagePercent);
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
			if (playerTarget != null && playerTarget.Client.Account.PrivLevel > 1) return player.Realm;

			return target.Realm;
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
		public IList FormatPlayerStatistics(GamePlayer player)
		{
			ArrayList stat = new ArrayList();
			int total = 0;
			#region Players Killed
			stat.Add("Kill report");
			switch ((eRealm)player.Realm)
			{
				case eRealm.Albion:
					stat.Add("Midgard Players Killed: " + player.KillsMidgardPlayers.ToString("N0"));
					stat.Add("Hibernia Players Killed: " + player.KillsHiberniaPlayers.ToString("N0"));
					total = player.KillsMidgardPlayers + player.KillsHiberniaPlayers;
					break;
				case eRealm.Midgard:
					stat.Add("Albion Players Killed: " + player.KillsAlbionPlayers.ToString("N0"));
					stat.Add("Hibernia Players Killed: " + player.KillsHiberniaPlayers.ToString("N0"));
					total = player.KillsAlbionPlayers + player.KillsHiberniaPlayers;
					break;
				case eRealm.Hibernia:
					stat.Add("Albion Players Killed: " + player.KillsAlbionPlayers.ToString("N0"));
					stat.Add("Midgard Players Killed: " + player.KillsMidgardPlayers.ToString("N0"));
					total = player.KillsMidgardPlayers + player.KillsAlbionPlayers;
					break;
			}
			stat.Add("Total Players Killed: " + total.ToString("N0"));
			#endregion
			stat.Add(" ");
			#region Players Deathblows
			total = 0;
			switch ((eRealm)player.Realm)
			{
				case eRealm.Albion:
					stat.Add("Midgard Deathblows: " + player.KillsMidgardDeathBlows.ToString("N0"));
					stat.Add("Hibernia Deathblows: " + player.KillsHiberniaDeathBlows.ToString("N0"));
					total = player.KillsMidgardDeathBlows + player.KillsHiberniaDeathBlows;
					break;
				case eRealm.Midgard:
					stat.Add("Albion Deathblows: " + player.KillsAlbionDeathBlows.ToString("N0"));
					stat.Add("Hibernia Deathblows: " + player.KillsHiberniaDeathBlows.ToString("N0"));
					total = player.KillsAlbionDeathBlows + player.KillsHiberniaDeathBlows;
					break;
				case eRealm.Hibernia:
					stat.Add("Albion Deathblows: " + player.KillsAlbionDeathBlows.ToString("N0"));
					stat.Add("Midgard Deathblows: " + player.KillsMidgardDeathBlows.ToString("N0"));
					total = player.KillsMidgardDeathBlows + player.KillsAlbionDeathBlows;
					break;
			}
			stat.Add("Total Deathblows: " + total.ToString("N0"));
			#endregion
			stat.Add(" ");
			#region Players Solo Kills
			total = 0;
			switch ((eRealm)player.Realm)
			{
				case eRealm.Albion:
					stat.Add("Midgard Solo Kills: " + player.KillsMidgardSolo.ToString("N0"));
					stat.Add("Hibernia Solo Kills: " + player.KillsHiberniaSolo.ToString("N0"));
					total = player.KillsMidgardSolo + player.KillsHiberniaSolo;
					break;
				case eRealm.Midgard:
					stat.Add("Albion Solo Kills: " + player.KillsAlbionSolo.ToString("N0"));
					stat.Add("Hibernia Solo Kills: " + player.KillsHiberniaSolo.ToString("N0"));
					total = player.KillsAlbionSolo + player.KillsHiberniaSolo;
					break;
				case eRealm.Hibernia:
					stat.Add("Albion Solo Kills: " + player.KillsAlbionSolo.ToString("N0"));
					stat.Add("Midgard Solo Kills: " + player.KillsMidgardSolo.ToString("N0"));
					total = player.KillsMidgardSolo + player.KillsAlbionSolo;
					break;
			}
			stat.Add("Total Solo Kills: " + total.ToString("N0"));
			#endregion
			stat.Add(" ");
			#region Keeps
			stat.Add("Capture Report");
			//stat.Add("Relics Taken: " + player.RelicsTaken.ToString("N0"));
			//stat.Add("Albion Keeps Captured: " + player.CapturedAlbionKeeps.ToString("N0"));
			//stat.Add("Midgard Keeps Captured: " + player.CapturedMidgardKeeps.ToString("N0"));
			//stat.Add("Hibernia Keeps Captured: " + player.CapturedHiberniaKeeps.ToString("N0"));
			stat.Add("Total Keeps Captured: " + player.CapturedKeeps.ToString("N0"));
			//stat.Add("Keep Lords Slain: " + player.KeepLordsSlain.ToString("N0"));
			//stat.Add("Albion Towers Captured: " + player.CapturedAlbionTowers.ToString("N0"));
			//stat.Add("Midgard Towers Captured: " + player.CapturedMidgardTowers.ToString("N0"));
			//stat.Add("Hibernia Towers Captured: " + player.CapturedHiberniaTowers.ToString("N0"));
			stat.Add("Total Towers Captured: " + player.CapturedTowers.ToString("N0"));
			//stat.Add("Tower Captains Slain: " + player.TowerCaptainsSlain.ToString("N0"));
			//stat.Add("Realm Guard Kills Albion: " + player.RealmGuardTotalKills.ToString("N0"));
			//stat.Add("Realm Guard Kills Midgard: " + player.RealmGuardTotalKills.ToString("N0"));
			//stat.Add("Realm Guard Kills Hibernia: " + player.RealmGuardTotalKills.ToString("N0"));
			//stat.Add("Total Realm Guard Kills: " + player.RealmGuardTotalKills.ToString("N0"));
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
