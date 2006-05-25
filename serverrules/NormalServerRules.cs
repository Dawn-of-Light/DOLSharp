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
using DOL.AI;
using DOL.AI.Brain;
using DOL.Database;
using DOL.GS.Database;
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

		public override bool IsAllowedToAttack(GameLiving attacker, GameLivingBase defender, bool quiet)
		{
			if(attacker is GamePlayer && ((GamePlayer)attacker).Client.Account.PrivLevel > ePrivLevel.Player)
			{
				return true;
			}

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

			//Don't allow attacks on same realm members on Normal Servers
			if (attacker.Realm == defender.Realm && !(attacker is GamePlayer && ((GamePlayer)attacker).DuelTarget == defender))
			{
				// allow mobs to attack mobs
				if (attacker.Realm == 0)
					return true;

				if(quiet == false) MessageToLiving(attacker, "You can't attack a member of your realm!");
				return false;
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

			if(source.Realm != target.Realm)
			{
				if(quiet == false) MessageToLiving(source, target.GetName(0, true) + " is not a member of your realm!");
				return false;
			}
			return true;
		}

		public override bool IsAllowedCharsInAllRealms(GameClient client)
		{
			if(client.Account.PrivLevel == ePrivLevel.Player) return false;
			return base.IsAllowedCharsInAllRealms(client);
		}

		public override bool IsAllowedToGroup(GamePlayer source, GamePlayer target, bool quiet)
		{
			if(!base.IsAllowedToGroup(source, target, quiet)) return false;
			
			if(source.Realm != target.Realm)
			{
				if(quiet == false) MessageToLiving(source, "You can't invite a player of another realm.");
				return false;
			}
			return true;
		}

		public override bool IsAllowedToTrade(GamePlayer source, GamePlayer target, bool quiet)
		{
			if(!base.IsAllowedToTrade(source, target, quiet)) return false;
			
			// clients with priv level > 1 can trade with anyone
			if(target.Client.Account.PrivLevel > ePrivLevel.Player
			|| source.Client.Account.PrivLevel > ePrivLevel.Player)
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
			if(!base.IsAllowedToUnderstand(source, target)) return false;

			// clients with priv level > 1 can understand everything
			if(target.Client.Account.PrivLevel > ePrivLevel.Player
			|| source is GamePlayer && ((GamePlayer)source).Client.Account.PrivLevel > ePrivLevel.Player)
				return true;

			// npc with peace brain can understand everything
			if (source is GameNPC && ((GameNPC)source).OwnBrain is PeaceBrain)
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
		/// <param name="point"></param>
		/// <returns></returns>
		public override bool IsAllowedToCraft(GamePlayer player, GenericItemTemplate item)
		{
			return player.Realm == (byte)item.Realm;
		}

		public override bool CheckAbilityToUseItem(GamePlayer player, EquipableItem item)
		{
			if(player == null)
				return false;

			if(player.Client.Account.PrivLevel > ePrivLevel.Player)
			{
				return true;
			}

			if(item.Realm != eRealm.None && item.Realm != (eRealm)player.Realm)
			{
				return false;
			}

			if(item.AllowedClass.Count > 0 && !item.AllowedClass.Contains((eCharacterClass)player.CharacterClassID))
			{
				return false;
			}

			//armor
			if (item.ObjectType >= eObjectType._FirstArmor && item.ObjectType <= eObjectType._LastArmor)
			{
				int armorAbility = -1;
				switch (item.Realm)
				{
					case eRealm.Albion   : armorAbility = player.GetAbilityLevel(Abilities.AlbArmor); break;
					case eRealm.Hibernia : armorAbility = player.GetAbilityLevel(Abilities.HibArmor); break;
					case eRealm.Midgard  : armorAbility = player.GetAbilityLevel(Abilities.MidArmor); break;
					default: // use old system
						armorAbility = Math.Max(armorAbility, player.GetAbilityLevel(Abilities.AlbArmor));
						armorAbility = Math.Max(armorAbility, player.GetAbilityLevel(Abilities.HibArmor));
						armorAbility = Math.Max(armorAbility, player.GetAbilityLevel(Abilities.MidArmor));
						break;
				}
				switch (item.ObjectType)
				{
					case eObjectType.Cloth       : return armorAbility >= (int)eArmorLevel.VeryLow;
					case eObjectType.Leather     : return armorAbility >= (int)eArmorLevel.Low;
					case eObjectType.Reinforced  :
					case eObjectType.Studded     : return armorAbility >= (int)eArmorLevel.Medium;
					case eObjectType.Scale       :
					case eObjectType.Chain       : return armorAbility >= (int)eArmorLevel.High;
					case eObjectType.Plate       : return armorAbility >= (int)eArmorLevel.VeryHigh;
					default: return false;
				}
			}

			string abilityCheck = null;
			string[] otherCheck = new string[0];

			//http://dol.kitchenhost.de/files/dol/Info/itemtable.txt
			switch(item.ObjectType)
			{
				case eObjectType.GenericItem     : return true;
				case eObjectType.GenericArmor    : return true;
				case eObjectType.GenericWeapon   : return true;
				case eObjectType.Staff           : abilityCheck = Abilities.Weapon_Staves; break;
				case eObjectType.ShortBow        : abilityCheck = Abilities.Weapon_Shortbows; break;

					//alb
				case eObjectType.CrushingWeapon  : abilityCheck = Abilities.Weapon_Crushing; break;
				case eObjectType.SlashingWeapon  : abilityCheck = Abilities.Weapon_Slashing; break;
				case eObjectType.ThrustWeapon    : abilityCheck = Abilities.Weapon_Thrusting; break;
				case eObjectType.TwoHandedWeapon : abilityCheck = Abilities.Weapon_TwoHanded; break;
				case eObjectType.PolearmWeapon   : abilityCheck = Abilities.Weapon_Polearms; break;
				case eObjectType.Longbow         : abilityCheck = Abilities.Weapon_Longbows; break;
				case eObjectType.Crossbow        : abilityCheck = Abilities.Weapon_Crossbow; break;
				case eObjectType.FlexibleWeapon        : abilityCheck = Abilities.Weapon_Flexible; break;
				//TODO: case 5: abilityCheck = Abilities.Weapon_Thrown; break;

					//mid
				case eObjectType.Sword           : abilityCheck = Abilities.Weapon_Swords; break;
				case eObjectType.Hammer          : abilityCheck = Abilities.Weapon_Hammers; break;
				case eObjectType.LeftAxe:
				case eObjectType.Axe             : abilityCheck = Abilities.Weapon_Axes; break;
				case eObjectType.Spear           : abilityCheck = Abilities.Weapon_Spears; break;
				case eObjectType.CompositeBow    : abilityCheck = Abilities.Weapon_CompositeBows; break;
				case eObjectType.ThrownWeapon    : abilityCheck = Abilities.Weapon_Thrown; break;
				case eObjectType.HandToHand      : abilityCheck = Abilities.Weapon_HandToHand; break;

					//hib
				case eObjectType.RecurvedBow     : abilityCheck = Abilities.Weapon_RecurvedBows; break;
				case eObjectType.Blades          : abilityCheck = Abilities.Weapon_Blades; break;
				case eObjectType.Blunt           : abilityCheck = Abilities.Weapon_Blunt; break;
				case eObjectType.Piercing        : abilityCheck = Abilities.Weapon_Piercing; break;
				case eObjectType.LargeWeapon     : abilityCheck = Abilities.Weapon_LargeWeapons; break;
				case eObjectType.CelticSpear     : abilityCheck = Abilities.Weapon_CelticSpear; break;
				case eObjectType.Scythe          : abilityCheck = Abilities.Weapon_Scythe; break;

					//misc
				//case eObjectType.Magical         : return true;
				case eObjectType.Shield          : return player.GetAbilityLevel(Abilities.Shield) >= (byte)(((Shield)item).Size);
				//case eObjectType.Bolt            : abilityCheck = Abilities.Weapon_Crossbow; break;
				//case eObjectType.Arrow           : otherCheck = new string[] { Abilities.Weapon_CompositeBows, Abilities.Weapon_Longbows, Abilities.Weapon_RecurvedBows, Abilities.Weapon_Shortbows }; break;
				//case eObjectType.Poison          : return player.GetModifiedSpecLevel(Specs.Envenom) > 0;
				case eObjectType.Instrument      : return player.HasAbility(Abilities.Weapon_Instruments);
			}

			//player.Out.SendMessage("ability: \""+abilityCheck+"\"; type: "+item.Object_Type, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			if(abilityCheck != null && player.HasAbility(abilityCheck))
				return true;

			foreach(string str in otherCheck)
				if(player.HasAbility(str))
					return true;

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
				m_compatibleObjectTypes[(int)eObjectType.ShortBow] = new eObjectType[] { eObjectType.ShortBow };

				//alb
				m_compatibleObjectTypes[(int)eObjectType.CrushingWeapon]  = new eObjectType[] { eObjectType.CrushingWeapon };
				m_compatibleObjectTypes[(int)eObjectType.SlashingWeapon]  = new eObjectType[] { eObjectType.SlashingWeapon };
				m_compatibleObjectTypes[(int)eObjectType.ThrustWeapon]    = new eObjectType[] { eObjectType.ThrustWeapon };
				m_compatibleObjectTypes[(int)eObjectType.TwoHandedWeapon] = new eObjectType[] { eObjectType.TwoHandedWeapon };
				m_compatibleObjectTypes[(int)eObjectType.PolearmWeapon]   = new eObjectType[] { eObjectType.PolearmWeapon };
				m_compatibleObjectTypes[(int)eObjectType.FlexibleWeapon]  = new eObjectType[] { eObjectType.FlexibleWeapon };
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
				m_compatibleObjectTypes[(int)eObjectType.ThrownWeapon] = new eObjectType[] { eObjectType.ThrownWeapon };

				//hib
				m_compatibleObjectTypes[(int)eObjectType.Blunt]        = new eObjectType[] { eObjectType.Blunt };
				m_compatibleObjectTypes[(int)eObjectType.Blades]       = new eObjectType[] { eObjectType.Blades };
				m_compatibleObjectTypes[(int)eObjectType.Piercing]     = new eObjectType[] { eObjectType.Piercing };
				m_compatibleObjectTypes[(int)eObjectType.LargeWeapon]  = new eObjectType[] { eObjectType.LargeWeapon };
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
			return GlobalConstants.RaceToName((eRace)target.Race);
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
				return base.GetPlayerGuildName(source, target);
			return string.Empty;
		}
	}
}
