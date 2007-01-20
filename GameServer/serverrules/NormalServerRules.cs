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
using DOL.AI.Brain;
using DOL.Database;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;
using DOL.GS.Styles;

namespace DOL.GS.ServerRules
{
	/// <summary>
	/// Set of rules for "normal" server type.
	/// </summary>
	[ServerRules(eGameServerType.GST_Normal)]
	public class NormalServerRules : AbstractServerRules
	{
		public override string RulesDescription()
		{
			return "standard Normal server rules";
		}

		/// <summary>
		/// Invoked on NPC death and deals out
		/// experience/realm points if needed
		/// </summary>
		/// <param name="killedNPC">npc that died</param>
		/// <param name="killer">killer</param>
		public override void OnNPCKilled(GameNPC killedNPC, GameObject killer)
		{
			base.OnNPCKilled(killedNPC, killer); 	
		}

		public override bool IsAllowedToAttack(GameLiving attacker, GameLiving defender, bool quiet)
		{
			if (!base.IsAllowedToAttack(attacker, defender, quiet))
				return false;

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

			//"You can't attack yourself!"
			if(attacker == defender)
			{
				if (quiet == false) MessageToLiving(attacker, "You can't attack yourself!");
				return false;
			}

			//Don't allow attacks on same realm members on Normal Servers
			if (attacker.Realm == defender.Realm && !(attacker is GamePlayer && ((GamePlayer)attacker).DuelTarget == defender))
			{
				// allow mobs to attack mobs
				if (attacker.Realm == 0)
					return true;

				//allow confused mobs to attack same realm
				if (attacker is GameNPC && (attacker as GameNPC).IsConfused && attacker.Realm == defender.Realm)
					return true;

				if(quiet == false) MessageToLiving(attacker, "You can't attack a member of your realm!");
				return false;
			}

			return true;
		}

		public override bool IsSameRealm(GameLiving source, GameLiving target, bool quiet)
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

			// clients with priv level > 1 are considered friendly by anyone
			if(target is GamePlayer && ((GamePlayer)target).Client.Account.PrivLevel > 1) return true;

			//Peace flag NPCs are same realm
			if (target is GameNPC)
				if ((((GameNPC)target).Flags & (uint)GameNPC.eFlags.PEACE) != 0)
					return true;

			if (source is GameNPC)
				if ((((GameNPC)source).Flags & (uint)GameNPC.eFlags.PEACE) != 0)
					return true;

			if(source.Realm != target.Realm)
			{
				if(quiet == false) MessageToLiving(source, target.GetName(0, true) + " is not a member of your realm!");
				return false;
			}
			return true;
		}

		public override bool IsAllowedCharsInAllRealms(GameClient client)
		{
			if (client.Account.PrivLevel > 1)
				return true;
			if (ServerProperties.Properties.ALLOW_ALL_REALMS)
				return true;
			return false;
		}

		public override bool IsAllowedToGroup(GamePlayer source, GamePlayer target, bool quiet)
		{
			if(source == null || target == null) return false;

			if(source.Realm != target.Realm)
			{
				if(quiet == false) MessageToLiving(source, "You can't invite a player of another realm.");
				return false;
			}
			return true;
		}

		public override bool IsAllowedToTrade(GameLiving source, GameLiving target, bool quiet)
		{
			if(source == null || target == null) return false;

			// clients with priv level > 1 are allowed to trade with anyone
			if(source is GamePlayer && target is GamePlayer)
			{
				if ((source as GamePlayer).Client.Account.PrivLevel > 1 ||(target as GamePlayer).Client.Account.PrivLevel > 1)
					return true;
			}

			//Peace flag NPCs can trade with everyone
			if (target is GameNPC)
				if ((((GameNPC)target).Flags & (uint)GameNPC.eFlags.PEACE) != 0)
					return true;

			if (source is GameNPC)
				if ((((GameNPC)source).Flags & (uint)GameNPC.eFlags.PEACE) != 0)
					return true;

			if(source.Realm != target.Realm)
			{
				if(quiet == false) MessageToLiving(source, "You can't trade with enemy realm!");
				return false;
			}
			return true;
		}

		public override bool IsAllowedToUnderstand(GameLiving source, GamePlayer target)
		{
			if(source == null || target == null) return false;

			// clients with priv level > 1 are allowed to talk and hear anyone
			if(source is GamePlayer && ((GamePlayer)source).Client.Account.PrivLevel > 1) return true;
			if(target.Client.Account.PrivLevel > 1) return true;

			//Peace flag NPCs can be understood by everyone

			if (source is GameNPC)
				if ((((GameNPC)source).Flags & (uint)GameNPC.eFlags.PEACE) != 0)
					return true;

			if(source.Realm != target.Realm) return false;
			return true;
		}

		/// <summary>
		/// Is player allowed to bind
		/// </summary>
		/// <param name="player"></param>
		/// <param name="point"></param>
		/// <returns></returns>
		public override bool IsAllowedToBind(GamePlayer player, BindPoint point)
		{
			if (point.Realm == 0) return true;
			return player.Realm == point.Realm;
		}

		/// <summary>
		/// Is player allowed to make the item
		/// </summary>
		/// <param name="player"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public override bool IsAllowedToCraft(GamePlayer player, ItemTemplate item)
		{
			return player.Realm == item.Realm;
		}

		/// <summary>
		/// Check a living has the ability to use an item
		/// </summary>
		/// <param name="living"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public override bool CheckAbilityToUseItem(GameLiving living, ItemTemplate item)
		{
			if(living == null || item == null)
				return false;

			if(item.Realm != 0 && item.Realm != living.Realm)
				return false;

			//armor
			if (item.Object_Type >= (int)eObjectType._FirstArmor && item.Object_Type <= (int)eObjectType._LastArmor)
			{
				int armorAbility = -1;
				switch ((eRealm)item.Realm)
				{
					case eRealm.Albion   : armorAbility = living.GetAbilityLevel(Abilities.AlbArmor); break;
					case eRealm.Hibernia : armorAbility = living.GetAbilityLevel(Abilities.HibArmor); break;
					case eRealm.Midgard  : armorAbility = living.GetAbilityLevel(Abilities.MidArmor); break;
					default: // use old system
						armorAbility = Math.Max(armorAbility, living.GetAbilityLevel(Abilities.AlbArmor));
						armorAbility = Math.Max(armorAbility, living.GetAbilityLevel(Abilities.HibArmor));
						armorAbility = Math.Max(armorAbility, living.GetAbilityLevel(Abilities.MidArmor));
						break;
				}
				switch ((eObjectType)item.Object_Type)
				{
					case eObjectType.GenericArmor: return armorAbility >= ArmorLevel.GenericArmor;
					case eObjectType.Cloth       : return armorAbility >= ArmorLevel.Cloth;
					case eObjectType.Leather     : return armorAbility >= ArmorLevel.Leather;
					case eObjectType.Reinforced  :
					case eObjectType.Studded     : return armorAbility >= ArmorLevel.Studded;
					case eObjectType.Scale       :
					case eObjectType.Chain       : return armorAbility >= ArmorLevel.Chain;
					case eObjectType.Plate       : return armorAbility >= ArmorLevel.Plate;
					default: return false;
				}
			}

			string abilityCheck = null;
			string[] otherCheck = new string[0];

			//http://dol.kitchenhost.de/files/dol/Info/itemtable.txt
			switch((eObjectType)item.Object_Type)
			{
				case eObjectType.GenericItem     : return true;
				case eObjectType.GenericArmor    : return true;
				case eObjectType.GenericWeapon   : return true;
				case eObjectType.Staff           : abilityCheck = Abilities.Weapon_Staves; break;
				case eObjectType.Fired           : abilityCheck = Abilities.Weapon_Shortbows; break;

					//alb
				case eObjectType.CrushingWeapon  : abilityCheck = Abilities.Weapon_Crushing; break;
				case eObjectType.SlashingWeapon  : abilityCheck = Abilities.Weapon_Slashing; break;
				case eObjectType.ThrustWeapon    : abilityCheck = Abilities.Weapon_Thrusting; break;
				case eObjectType.TwoHandedWeapon : abilityCheck = Abilities.Weapon_TwoHanded; break;
				case eObjectType.PolearmWeapon   : abilityCheck = Abilities.Weapon_Polearms; break;
				case eObjectType.Longbow         : abilityCheck = Abilities.Weapon_Longbows; break;
				case eObjectType.Crossbow        : abilityCheck = Abilities.Weapon_Crossbow; break;
				case eObjectType.Flexible        : abilityCheck = Abilities.Weapon_Flexible; break;
				//TODO: case 5: abilityCheck = Abilities.Weapon_Thrown; break;

					//mid
				case eObjectType.Sword           : abilityCheck = Abilities.Weapon_Swords; break;
				case eObjectType.Hammer          : abilityCheck = Abilities.Weapon_Hammers; break;
				case eObjectType.LeftAxe:
				case eObjectType.Axe             : abilityCheck = Abilities.Weapon_Axes; break;
				case eObjectType.Spear           : abilityCheck = Abilities.Weapon_Spears; break;
				case eObjectType.CompositeBow    : abilityCheck = Abilities.Weapon_CompositeBows; break;
				case eObjectType.Thrown          : abilityCheck = Abilities.Weapon_Thrown; break;
				case eObjectType.HandToHand      : abilityCheck = Abilities.Weapon_HandToHand; break;

					//hib
				case eObjectType.RecurvedBow     : abilityCheck = Abilities.Weapon_RecurvedBows; break;
				case eObjectType.Blades          : abilityCheck = Abilities.Weapon_Blades; break;
				case eObjectType.Blunt           : abilityCheck = Abilities.Weapon_Blunt; break;
				case eObjectType.Piercing        : abilityCheck = Abilities.Weapon_Piercing; break;
				case eObjectType.LargeWeapons    : abilityCheck = Abilities.Weapon_LargeWeapons; break;
				case eObjectType.CelticSpear     : abilityCheck = Abilities.Weapon_CelticSpear; break;
				case eObjectType.Scythe          : abilityCheck = Abilities.Weapon_Scythe; break;

					//misc
				case eObjectType.Magical         : return true;
				case eObjectType.Shield          : return living.GetAbilityLevel(Abilities.Shield) >= item.Type_Damage;
				case eObjectType.Bolt            : abilityCheck = Abilities.Weapon_Crossbow; break;
				case eObjectType.Arrow           : otherCheck = new string[] { Abilities.Weapon_CompositeBows, Abilities.Weapon_Longbows, Abilities.Weapon_RecurvedBows, Abilities.Weapon_Shortbows }; break;
				case eObjectType.Poison          : return living.GetModifiedSpecLevel(Specs.Envenom) > 0;
				case eObjectType.Instrument      : return living.HasAbility(Abilities.Weapon_Instruments);
				//TODO: different shield sizes
			}

			//player.Out.SendMessage("ability: \""+abilityCheck+"\"; type: "+item.Object_Type, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			if(abilityCheck != null && living.HasAbility(abilityCheck))
				return true;

			foreach (string str in otherCheck)
			{
				if (living.HasAbility(str))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Translates object type to compatible object types based on server type
		/// </summary>
		/// <param name="objectType">The object type</param>
		/// <returns>An array of compatible object types</returns>
		protected override eObjectType[] GetCompatibleObjectTypes(eObjectType objectType)
		{
			if(m_compatibleObjectTypes == null)
			{
				m_compatibleObjectTypes = new Hashtable();
				m_compatibleObjectTypes[(int)eObjectType.Staff] = new eObjectType[] { eObjectType.Staff };
				m_compatibleObjectTypes[(int)eObjectType.Fired] = new eObjectType[] { eObjectType.Fired };

				//alb
				m_compatibleObjectTypes[(int)eObjectType.CrushingWeapon]  = new eObjectType[] { eObjectType.CrushingWeapon };
				m_compatibleObjectTypes[(int)eObjectType.SlashingWeapon]  = new eObjectType[] { eObjectType.SlashingWeapon };
				m_compatibleObjectTypes[(int)eObjectType.ThrustWeapon]    = new eObjectType[] { eObjectType.ThrustWeapon };
				m_compatibleObjectTypes[(int)eObjectType.TwoHandedWeapon] = new eObjectType[] { eObjectType.TwoHandedWeapon };
				m_compatibleObjectTypes[(int)eObjectType.PolearmWeapon]   = new eObjectType[] { eObjectType.PolearmWeapon };
				m_compatibleObjectTypes[(int)eObjectType.Flexible]        = new eObjectType[] { eObjectType.Flexible };
				m_compatibleObjectTypes[(int)eObjectType.Longbow]         = new eObjectType[] { eObjectType.Longbow };
				m_compatibleObjectTypes[(int)eObjectType.Crossbow]        = new eObjectType[] { eObjectType.Crossbow };
				//TODO: case 5: abilityCheck = Abilities.Weapon_Thrown; break;                                         

				//mid
				m_compatibleObjectTypes[(int)eObjectType.Hammer]       = new eObjectType[] { eObjectType.Hammer };
				m_compatibleObjectTypes[(int)eObjectType.Sword]        = new eObjectType[] { eObjectType.Sword };
				m_compatibleObjectTypes[(int)eObjectType.LeftAxe]      = new eObjectType[] { eObjectType.LeftAxe };
				m_compatibleObjectTypes[(int)eObjectType.Axe]          = new eObjectType[] { eObjectType.Axe };
				m_compatibleObjectTypes[(int)eObjectType.HandToHand]   = new eObjectType[] { eObjectType.HandToHand };
				m_compatibleObjectTypes[(int)eObjectType.Spear]        = new eObjectType[] { eObjectType.Spear };
				m_compatibleObjectTypes[(int)eObjectType.CompositeBow] = new eObjectType[] { eObjectType.CompositeBow };
				m_compatibleObjectTypes[(int)eObjectType.Thrown]       = new eObjectType[] { eObjectType.Thrown };

				//hib
				m_compatibleObjectTypes[(int)eObjectType.Blunt]        = new eObjectType[] { eObjectType.Blunt };
				m_compatibleObjectTypes[(int)eObjectType.Blades]       = new eObjectType[] { eObjectType.Blades };
				m_compatibleObjectTypes[(int)eObjectType.Piercing]     = new eObjectType[] { eObjectType.Piercing };
				m_compatibleObjectTypes[(int)eObjectType.LargeWeapons] = new eObjectType[] { eObjectType.LargeWeapons };
				m_compatibleObjectTypes[(int)eObjectType.CelticSpear]  = new eObjectType[] { eObjectType.CelticSpear };
				m_compatibleObjectTypes[(int)eObjectType.Scythe]       = new eObjectType[] { eObjectType.Scythe };
				m_compatibleObjectTypes[(int)eObjectType.RecurvedBow]  = new eObjectType[] { eObjectType.RecurvedBow };

				m_compatibleObjectTypes[(int)eObjectType.Shield]       = new eObjectType[] { eObjectType.Shield };
				m_compatibleObjectTypes[(int)eObjectType.Poison]       = new eObjectType[] { eObjectType.Poison };
				//TODO: case 45: abilityCheck = Abilities.instruments; break;
			}

			eObjectType[] res = (eObjectType[])m_compatibleObjectTypes[(int)objectType];
			if(res == null)
				return new eObjectType[0];
			return res;
		}

		/// <summary>
		/// Gets the player name based on server type
		/// </summary>
		/// <param name="source">The "looking" player</param>
		/// <param name="target">The considered player</param>
		/// <returns>The name of the target</returns>
		public override string GetPlayerName(GamePlayer source, GamePlayer target)
		{
			if (IsSameRealm(source, target, true))
				return target.Name;
			return target.RaceName;
		}

		/// <summary>
		/// Gets the player last name based on server type
		/// </summary>
		/// <param name="source">The "looking" player</param>
		/// <param name="target">The considered player</param>
		/// <returns>The last name of the target</returns>
		public override string GetPlayerLastName(GamePlayer source, GamePlayer target)
		{
			if (IsSameRealm(source, target, true))
				return target.LastName;
			return target.RealmTitle;
		}

		/// <summary>
		/// Gets the player guild name based on server type
		/// </summary>
		/// <param name="source">The "looking" player</param>
		/// <param name="target">The considered player</param>
		/// <returns>The guild name of the target</returns>
		public override string GetPlayerGuildName(GamePlayer source, GamePlayer target)
		{
			if (IsSameRealm(source, target, true))
				return target.GuildName;
			return string.Empty;
		}

		/// <summary>
		/// Reset the keep with special server rules handling
		/// </summary>
		/// <param name="lord">The lord that was killed</param>
		/// <param name="killer">The lord's killer</param>
		public override void ResetKeep(GuardLord lord, GameObject killer)
		{
			base.ResetKeep(lord, killer);
			lord.Component.Keep.Reset((eRealm)killer.Realm);
		}
	}
}
