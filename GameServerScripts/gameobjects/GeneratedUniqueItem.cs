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


//
// This is a class writen from the Storm Unique Object Generator.
//
// Original version by Etaew
// Modified by Tolakram to add live like names and item models
//
// Released to the public on July 12th, 2010
//
// Updating to Class by Leodagan on Aug 2013.


using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using DOL.Events;
using DOL.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS
{
	/// <summary>
	/// GeneratedUniqueItem is a subclass of UniqueItem used to create RoG object
	/// Using it as a class is much more extendable to other usage than just loot and inventory
	/// </summary>
	public class GeneratedUniqueItem : ItemUnique
	{
		// TOA Chance in %
		public const ushort ROG_TOA_ITEM_CHANCE = 33;
		// Armor Chance in %
		public const ushort ROG_ARMOR_CHANCE = 40;
		// Magical Chance in %
		public const ushort ROG_MAGICAL_CHANCE = 15;
		
		// Item lowest quality
		public const ushort ROG_STARTING_QUAL = 90;
		
		// Item highest quality
		public const ushort ROG_CAP_QUAL = 99;
		
		// Item chance to get a TOA advanced stat in a TOA Item
		public const ushort ROG_TOA_STAT_CHANCE = 20;
		
		// Item chance to get stat bonus
		public const ushort ROG_ITEM_STAT_CHANCE = 50;
		
		// Item chance to get resist bonus
		public const ushort ROG_ITEM_RESIST_CHANCE = 40;
		
		// Item chance to get All skills stat
		public const ushort ROG_STAT_ALLSKILL_CHANCE = 25;
		
		// base Chance to get a magical RoG item, Level*2 is added to get final value
		public const ushort ROG_100_MAGICAL_OFFSET = 40;
		
		protected static Dictionary<eProperty, string> hPropertyToMagicPrefix = new Dictionary<eProperty, string>();

		[ScriptLoadedEvent]
		public static void OnScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			InitializeHashtables();
		}
		
		public GeneratedUniqueItem()
			:this((eRealm)Util.Random(1,3), (byte)Util.Random(1,50))
		{
			
		}
		
		#region Constructor Randomized
		
		
		public GeneratedUniqueItem(eRealm realm, byte level)
			:this(realm, level, GenerateObjectType(realm, level))
		{

		}
		
		public GeneratedUniqueItem(eRealm realm, byte level, eObjectType type)
			:this(realm, level, type, GenerateItemType(type))
		{

		}
		
		public GeneratedUniqueItem(eRealm realm, byte level, eObjectType type, eInventorySlot slot)
			:this(realm, level, type, slot, GenerateDamageType(type))
		{

		}
		
		public GeneratedUniqueItem(eRealm realm, byte level, eObjectType type, eInventorySlot slot, eDamageType dmg)
			:this(Util.Chance(ROG_TOA_ITEM_CHANCE), realm, level, type, slot, dmg)
		{

		}
		
		public GeneratedUniqueItem(bool toa)
			:this(toa, (eRealm)Util.Random(1,3), (byte)Util.Random(1,50))
		{

		}
		
		public GeneratedUniqueItem(bool toa, eRealm realm, byte level)
			:this(toa, realm, level, GenerateObjectType(realm, level))
		{

		}
		
		public GeneratedUniqueItem(bool toa, eRealm realm, byte level, eObjectType type)
			:this(toa, realm, level, type, GenerateItemType(type))
		{

		}
		
		public GeneratedUniqueItem(bool toa, eRealm realm, byte level, eObjectType type, eInventorySlot slot)
			:this(toa, realm, level, type, slot, GenerateDamageType(type))
		{

		}
		
		public GeneratedUniqueItem(bool toa, eRealm realm, byte level, eObjectType type, eInventorySlot slot, eDamageType dmg)
			:base()
		{
			this.Realm = (int)realm;
			this.Level = level;
			this.Object_Type = (int)type;
			this.Item_Type = (int)slot;
			this.Type_Damage = (int)dmg;
			
			// shouldn't need more Randomized public set values
			
			//need stats before naming
			this.GenerateItemStats();
			
			//name item
			this.GenerateItemNameModel();

			//set item quality (this can be called again by any script with real mob values)
			this.GenerateItemQuality((double)Util.Random(0,6)-3);

			//item magical bonuses
			//if staff and magic..... focus
			this.GenerateMagicalBonuses(toa);
			
			this.IsDropable = true;
			this.IsPickable = true;
			this.IsTradable = true;

			//item bonus
			int temp = this.Level - 15;
			temp -= temp % 5;
			this.Bonus = temp;
			if (this.Bonus < 0)
				this.Bonus = 0;

			//constants
			int condition = this.Level * 2000;
			this.Condition = condition;
			this.MaxCondition = condition;
			this.Durability = condition;
			this.MaxDurability = condition;

			this.GenerateItemWeight();
			
			this.Description = "Unique Item";
			
			//don't add to database implicitly, must be done explicitly
			this.AllowAdd = false;
		}
		
		#endregion	
		
		#region generate item properties
		public void GenerateItemQuality(double conlevel)
		{
			// set base quality
			int minQuality = ROG_STARTING_QUAL + Math.Max(0, this.Level - 46);
			int maxQuality = (int)(1.310 * conlevel + 94.29 + 3);
			
			// CAPS
			maxQuality = Math.Min(maxQuality, ROG_CAP_QUAL);  // unique objects capped at 99 quality
			minQuality = Math.Min(minQuality, ROG_CAP_QUAL);  // unique objects capped at 99 quality
			
			maxQuality = Math.Max(maxQuality, minQuality);

			this.Quality = Util.Random(minQuality, maxQuality);

			this.Price = Money.SetAutoPrice(this.Level, this.Quality);
		}
		
		private void GenerateItemStats()
		{
			eObjectType type = (eObjectType)this.Object_Type;

			//special property for instrument
			if (type == eObjectType.Instrument)
				this.DPS_AF = Util.Random(0, 3);

			//set hand
			switch (type)
			{
				//two handed weapons
				case eObjectType.CelticSpear:
				case eObjectType.CompositeBow:
				case eObjectType.Crossbow:
				case eObjectType.Fired:
				case eObjectType.Instrument:
				case eObjectType.LargeWeapons:
				case eObjectType.Longbow:
				case eObjectType.PolearmWeapon:
				case eObjectType.RecurvedBow:
				case eObjectType.Scythe:
				case eObjectType.Spear:
				case eObjectType.Staff:
				case eObjectType.TwoHandedWeapon:
				case eObjectType.MaulerStaff: //Maulers
					{
						this.Hand = 1;
						break;
					}
				//right or left handed weapons
				case eObjectType.Blades:
				case eObjectType.Blunt:
				case eObjectType.CrushingWeapon:
				case eObjectType.HandToHand:
				case eObjectType.Piercing:
				case eObjectType.SlashingWeapon:
				case eObjectType.ThrustWeapon:
				case eObjectType.FistWraps: //Maulers
					{
						if ((eInventorySlot)this.Item_Type == eInventorySlot.LeftHandWeapon)
							this.Hand = 2;
						break;
					}
				//left handed weapons
				case eObjectType.LeftAxe:
				case eObjectType.Shield:
					{
						this.Hand = 2;
						break;
					}
				//right or two handed weapons
				case eObjectType.Sword:
				case eObjectType.Hammer:
				case eObjectType.Axe:
					{
						if ((eInventorySlot)this.Item_Type == eInventorySlot.TwoHandWeapon)
							this.Hand = 1;
						break;
					}
			}

			//set dps_af and spd_abs
			if ((int)type >= (int)eObjectType._FirstArmor && (int)type <= (int)eObjectType._LastArmor)
			{
				if (type == eObjectType.Cloth)
					this.DPS_AF = this.Level;
				else this.DPS_AF = this.Level * 2;
				this.SPD_ABS = GetAbsorb(type);
			}
			
			switch (type)
			{
				case eObjectType.Axe:
				case eObjectType.Blades:
				case eObjectType.Blunt:
				case eObjectType.CelticSpear:
				case eObjectType.CompositeBow:
				case eObjectType.Crossbow:
				case eObjectType.CrushingWeapon:
				case eObjectType.Fired:
				case eObjectType.Flexible:
				case eObjectType.Hammer:
				case eObjectType.HandToHand:
				case eObjectType.LargeWeapons:
				case eObjectType.LeftAxe:
				case eObjectType.Longbow:
				case eObjectType.Piercing:
				case eObjectType.PolearmWeapon:
				case eObjectType.RecurvedBow:
				case eObjectType.Scythe:
				case eObjectType.Shield:
				case eObjectType.SlashingWeapon:
				case eObjectType.Spear:
				case eObjectType.Staff:
				case eObjectType.Sword:
				case eObjectType.ThrustWeapon:
				case eObjectType.TwoHandedWeapon:
				case eObjectType.MaulerStaff: //Maulers
				case eObjectType.FistWraps: //Maulers
					{
						this.DPS_AF = (int)(((this.Level * 0.3) + 1.2) * 10);
						SetWeaponSpeed();
						break;
					}
			}
		}
		
		private void GenerateMagicalBonuses(bool toa)
		{
			// unique objects have more bonuses as level rises

			int number = 1;
			
			if (Util.Chance(ROG_100_MAGICAL_OFFSET + this.Level * 2)) // 100% magical starting at level 40
			{
				//2
				number++;

				if (Util.Chance(this.Level * 8 - 40)) // level 6 - 17 (100%)
				{
					//3
					number++;

					if (Util.Chance(this.Level * 6 - 60)) // level 11 - 27 (100%)
					{
						//4
						number++;

						if (Util.Chance(this.Level * 4 - 80)) // level 21 - 45 (100%)
						{
							//5
							number++;
						}
					}
				}

			}
			
			if (toa)
				number++; // up to 6

			// Magical items have at least 1 bonus
			if (this.Object_Type == (int)eObjectType.Magical && number < 1)
				number = 1;


			bool fNamed = false;
			bool fAddedBonus = false;

			double quality = (double)this.Quality * .01;

			double multiplier = (quality * quality * quality) + 0.15;

			if (toa)
			{
				multiplier += 0.15;
			}

			for (int i = 0; i < number; i++)
			{
				eBonusType type = this.GetPropertyType(toa);
				eProperty property = this.GetProperty(type);
				if (!this.BonusExists(property))
				{
					int amount = (int)Math.Ceiling((double)GetBonusAmount(type, property) * multiplier);
					this.WriteBonus(property, amount);
					fAddedBonus = true;
					if (!fNamed && this.WriteMagicalName(property))
					{
						fNamed = true;
						multiplier *= 0.65;
					}
				}
			}

			// non magical items get lowercase names
			if (number == 0 || !fAddedBonus)
				this.Name = this.Name.ToLower();
		}	
		
		private eBonusType GetPropertyType(bool toa)
		{
			//allfocus
			if (CanAddFocus())
				return eBonusType.Focus;

			// ToA allows stat cap bonuses
			if (toa && Util.Chance(ROG_TOA_STAT_CHANCE))
			{
				return eBonusType.AdvancedStat;
			}
			

			//stats
			if (Util.Chance(ROG_ITEM_STAT_CHANCE))
				return eBonusType.Stat;
			//resists
			if (Util.Chance(ROG_ITEM_RESIST_CHANCE))
				return eBonusType.Resist;
			//skills
			return eBonusType.Skill;
		}
		
		private bool CanAddFocus()
		{
			if (this.Object_Type == (int)eObjectType.Staff)
			{
				if (this.Bonus1Type != 0)
					return false;

				if (this.Realm == (int)eRealm.Albion && this.Description == "friar")
					return false;

				return true;
			}

			return false;
		}
		#endregion
		
		#region check valid stat
		private eProperty GetProperty(eBonusType type)
		{
			switch (type)
			{
				case eBonusType.Focus:
					{
						return eProperty.AllFocusLevels;
					}
				case eBonusType.Resist:
					{
						return (eProperty)Util.Random((int)eProperty.Resist_First, (int)eProperty.Resist_Last);
					}
				case eBonusType.Skill:
					{
						// fill valid skills
						List<eProperty> validSkills = new List<eProperty>();

						bool fIndividualSkill = false;

						// All Skills is never combined with any other skill
						if (!BonusExists(eProperty.AllSkills))
						{
							// All type skills never combined with individual skills
							if (!BonusExists(eProperty.AllMagicSkills) &&
								!BonusExists(eProperty.AllMeleeWeaponSkills) &&
								!BonusExists(eProperty.AllDualWieldingSkills) &&
								!BonusExists(eProperty.AllArcherySkills))
							{
								// individual realm specific skills
								if ((eRealm)this.Realm == eRealm.Albion)
								{
									foreach (eProperty property in AlbSkillBonus)
									{
										if (!BonusExists(property))
										{
											if (SkillIsValidForObjectType(property))
												validSkills.Add(property);
										}
										else
											fIndividualSkill = true;
									}
								}
								else if ((eRealm)this.Realm == eRealm.Hibernia)
								{
									foreach (eProperty property in HibSkillBonus)
									{
										if (!BonusExists(property))
										{
											if (SkillIsValidForObjectType(property))
												validSkills.Add(property);
										}
										else
											fIndividualSkill = true;
									}
								}
								else if ((eRealm)this.Realm == eRealm.Midgard)
								{
									foreach (eProperty property in MidSkillBonus)
									{
										if (!BonusExists(property))
										{
											if (SkillIsValidForObjectType(property))
												validSkills.Add(property);
										}
										else
											fIndividualSkill = true;
									}
								}

								if (!fIndividualSkill)
								{
									// ok to add AllSkills, but reduce the chance
									if (SkillIsValidForObjectType(eProperty.AllSkills) && Util.Chance(ROG_STAT_ALLSKILL_CHANCE))
										validSkills.Add(eProperty.AllSkills);
								}
							}

							// All type skills never combined with individual skills
							if (!fIndividualSkill)
							{
								if (!BonusExists(eProperty.AllMagicSkills) && SkillIsValidForObjectType(eProperty.AllMagicSkills))
									validSkills.Add(eProperty.AllMagicSkills);

								if (!BonusExists(eProperty.AllMeleeWeaponSkills) && SkillIsValidForObjectType(eProperty.AllMeleeWeaponSkills))
									validSkills.Add(eProperty.AllMeleeWeaponSkills);

								if (!BonusExists(eProperty.AllDualWieldingSkills) && SkillIsValidForObjectType(eProperty.AllDualWieldingSkills))
									validSkills.Add(eProperty.AllDualWieldingSkills);

								if (!BonusExists(eProperty.AllArcherySkills) && SkillIsValidForObjectType(eProperty.AllArcherySkills))
									validSkills.Add(eProperty.AllArcherySkills);
							}

						}

						int index = 0;
						index = validSkills.Count - 1;

						if (index < 1)
						{
							// return a safe random stat

							type = eBonusType.Stat;

							switch (Util.Random(0, 4))
							{
								case 0:
									return eProperty.MaxHealth;
								case 1:
									return eProperty.Strength;
								case 2:
									return eProperty.Dexterity;
								case 3:
									return eProperty.Quickness;
								case 4:
									return eProperty.Constitution;
							}
						}

						return (eProperty)validSkills[Util.Random(0, index)];
					}
				case eBonusType.Stat:
					{
						// ToDo: this does not check for duplicates like INT and Acuity
						List<eProperty> validStats = new List<eProperty>();
						foreach (eProperty property in StatBonus)
						{
							if (!BonusExists(property) && StatIsValidForObjectType(property) && StatIsValidForRealm(property))
							{
								validStats.Add(property);
							}
						}
						return (eProperty)validStats[Util.Random(0, validStats.Count - 1)];
					}
				case eBonusType.AdvancedStat:
					{
						// ToDo: this does not check for duplicates like INT and Acuity
						List<eProperty> validStats = new List<eProperty>();
						foreach (eProperty property in AdvancedStats)
						{
							if (!BonusExists(property) && StatIsValidForObjectType(property) && StatIsValidForRealm(property))
								validStats.Add(property);
						}
						return (eProperty)validStats[Util.Random(0, validStats.Count - 1)];
					}
			}
			return eProperty.MaxHealth;
		}

		private bool StatIsValidForObjectType(eProperty property)
		{
			switch ((eObjectType)this.Object_Type)
			{
				case eObjectType.Magical: 
					return StatIsValidForRealm(property);
				case eObjectType.Cloth:
				case eObjectType.Leather:
				case eObjectType.Studded:
				case eObjectType.Reinforced:
				case eObjectType.Chain:
				case eObjectType.Scale:
				case eObjectType.Plate: 
					return StatIsValidForArmor(property);
				case eObjectType.Axe:
				case eObjectType.Blades:
				case eObjectType.Blunt:
				case eObjectType.CelticSpear:
				case eObjectType.CompositeBow:
				case eObjectType.Crossbow:
				case eObjectType.CrushingWeapon:
				case eObjectType.Fired:
				case eObjectType.Flexible:
				case eObjectType.Hammer:
				case eObjectType.HandToHand:
				case eObjectType.Instrument:
				case eObjectType.LargeWeapons:
				case eObjectType.LeftAxe:
				case eObjectType.Longbow:
				case eObjectType.Piercing:
				case eObjectType.PolearmWeapon:
				case eObjectType.RecurvedBow:
				case eObjectType.Scythe:
				case eObjectType.Shield:
				case eObjectType.SlashingWeapon:
				case eObjectType.Spear:
				case eObjectType.Staff:
				case eObjectType.Sword:
				case eObjectType.ThrustWeapon:
				case eObjectType.FistWraps: //Maulers
				case eObjectType.MaulerStaff: //Maulers
				case eObjectType.TwoHandedWeapon: 
					return StatIsValidForWeapon(property);
			}
			return true;
		}

		private bool SkillIsValidForObjectType(eProperty property)
		{
			switch ((eObjectType)this.Object_Type)
			{
				case eObjectType.Magical: 
					return true;
				case eObjectType.Cloth:
				case eObjectType.Leather:
				case eObjectType.Studded:
				case eObjectType.Reinforced:
				case eObjectType.Chain:
				case eObjectType.Scale:
				case eObjectType.Plate: 
					return SkillIsValidForArmor(property);
				case eObjectType.Axe:
				case eObjectType.Blades:
				case eObjectType.Blunt:
				case eObjectType.CelticSpear:
				case eObjectType.CompositeBow:
				case eObjectType.Crossbow:
				case eObjectType.CrushingWeapon:
				case eObjectType.Fired:
				case eObjectType.Flexible:
				case eObjectType.Hammer:
				case eObjectType.HandToHand:
				case eObjectType.Instrument:
				case eObjectType.LargeWeapons:
				case eObjectType.LeftAxe:
				case eObjectType.Longbow:
				case eObjectType.Piercing:
				case eObjectType.PolearmWeapon:
				case eObjectType.RecurvedBow:
				case eObjectType.Scythe:
				case eObjectType.Shield:
				case eObjectType.SlashingWeapon:
				case eObjectType.Spear:
				case eObjectType.Staff:
				case eObjectType.Sword:
				case eObjectType.ThrustWeapon:
				case eObjectType.MaulerStaff:
				case eObjectType.FistWraps:
				case eObjectType.TwoHandedWeapon: 
					return SkillIsValidForWeapon(property);
			}
			return true;
		}

		private bool SkillIsValidForArmor(eProperty property)
		{
			int level = this.Level;
			eRealm realm = (eRealm)this.Realm;
			eObjectType type = (eObjectType)this.Object_Type;

			switch (property)
			{
				case eProperty.Skill_Augmentation:
					{
						if (level < 10)
						{
							if (type == eObjectType.Leather)
								return true;
							return false;
						}
						else if (level < 20)
						{
							if (type == eObjectType.Studded)
								return true;
							return false;
						}
						else
						{
							if (type == eObjectType.Chain)
								return true;
							return false;
						}
					}
				case eProperty.Skill_Axe:
					{
						if (type == eObjectType.Leather || type == eObjectType.Studded)
							return true;
						else if (type == eObjectType.Chain && level >= 10)
							return true;

						return false;
					}
				case eProperty.Skill_Battlesongs:
					{
						if (level < 20)
						{
							if (type == eObjectType.Studded)
								return true;
							return false;
						}
						else
						{
							if (type == eObjectType.Chain)
								return true;
							return false;
						}
					}
				case eProperty.Skill_Pathfinding:
				case eProperty.Skill_BeastCraft:
					{
						if (level < 10)
						{
							if (type == eObjectType.Leather)
								return true;
							return false;
						}
						else
						{
							if (type == eObjectType.Studded)
								return true;
							return false;
						}
					}
				case eProperty.Skill_Blades:
					{
						if (type == eObjectType.Leather || type == eObjectType.Reinforced || type == eObjectType.Scale)
							return true;
						return false;
					}
				case eProperty.Skill_Blunt:
					{
						if (type == eObjectType.Leather && level < 10)
							return true;
						else if (type == eObjectType.Reinforced || type == eObjectType.Scale)
							return true;
						return false;
					}
				//Cloth skills
				case eProperty.Skill_Arboreal:
				case eProperty.Skill_Body:
				case eProperty.Skill_BoneArmy:
				case eProperty.Skill_Cold:
				case eProperty.Skill_Creeping:
				case eProperty.Skill_Cursing:
				case eProperty.Skill_Darkness:
				case eProperty.Skill_Death_Servant:
				case eProperty.Skill_DeathSight:
				case eProperty.Skill_Earth:
				case eProperty.Skill_Enchantments:
				case eProperty.Skill_EtherealShriek:
				case eProperty.Skill_Fire:
				case eProperty.Skill_Hexing:
				case eProperty.Skill_Light:
				case eProperty.Skill_Mana:
				case eProperty.Skill_Matter:
				case eProperty.Skill_Mentalism:
				case eProperty.Skill_Mind:
				case eProperty.Skill_Pain_working:
				case eProperty.Skill_PhantasmalWail:
				case eProperty.Skill_Runecarving:
				case eProperty.Skill_SpectralForce:
				case eProperty.Skill_Spirit:
				case eProperty.Skill_Summoning:
				case eProperty.Skill_Suppression:
				case eProperty.Skill_Verdant:
				case eProperty.Skill_Void:
				case eProperty.Skill_Wind:
				case eProperty.Skill_Witchcraft:
					{
						if (type == eObjectType.Cloth)
							return true;
						return false;
					}
				case eProperty.Skill_Celtic_Dual:
					{
						if (type == eObjectType.Leather ||
							type == eObjectType.Reinforced)
							return true;
						return false;
					}
				case eProperty.Skill_Celtic_Spear:
					{
						if (level < 15)
						{
							if (type == eObjectType.Reinforced)
								return true;
							return false;
						}
						else
						{
							if (type == eObjectType.Scale)
								return true;
							return false;
						}
					}
				case eProperty.Skill_Chants:
					{
						return false;
					}
				case eProperty.Skill_Composite:
				case eProperty.Skill_RecurvedBow:
				case eProperty.Skill_Long_bows:
				case eProperty.Skill_Archery:
					{
						if (level < 10)
						{
							if (type == eObjectType.Leather)
								return true;

							return false;
						}
						else
						{
							if (type == eObjectType.Studded || type == eObjectType.Reinforced)
								return true;

							return false;
						}
					}
				case eProperty.Skill_Critical_Strike:
				case eProperty.Skill_Envenom:
				case eProperty.Skill_Dementia:
				case eProperty.Skill_Nightshade:
				case eProperty.Skill_ShadowMastery:
				case eProperty.Skill_VampiiricEmbrace:
					{
						if (type == eObjectType.Leather)
							return true;
						return false;
					}
				case eProperty.Skill_Cross_Bows:
					{
						return false; // disabled for armor

//						if (level < 15)
//						{
//							if (type == eObjectType.Chain)
//								return true;
//							return false;
//						}
//						else
//						{
//							if (type == eObjectType.Plate)
//								return true;
//							return false;
//						}
					}
				case eProperty.Skill_Crushing:
					{
						if (realm == eRealm.Albion && type == eObjectType.Cloth) // heretic
							return true;

						if (level < 15)
						{
							if (type == eObjectType.Studded)
								return true;
							return false;
						}
						else
						{
							if (type == eObjectType.Chain || type == eObjectType.Plate)
								return true;
							return false;
						}
					}
				case eProperty.Skill_Dual_Wield:
					{
						if (level < 20)
						{
							if (type == eObjectType.Leather || type == eObjectType.Studded)
								return true;
							return false;
						}
						else
						{
							if (type == eObjectType.Leather || type == eObjectType.Chain)
								return true;
							return false;
						}
					}
				case eProperty.Skill_Enhancement:
					{
						// friar
						if (type == eObjectType.Leather)
							return true;

						if (level < 20)
						{
							if (type == eObjectType.Studded)
								return true;
							return false;
						}
						else
						{
							if (type == eObjectType.Chain)
								return true;
							return false;
						}
					}
				case eProperty.Skill_Flexible_Weapon:
					{
						if (type == eObjectType.Cloth) // Heretic
							return true;

						if (level < 10)
						{
							if (type == eObjectType.Studded)
								return true;
							return false;
						}
						else
						{
							if (type == eObjectType.Chain)
								return true;
							return false;
						}
					}
				case eProperty.Skill_Hammer:
					{
						if (level < 10)
						{
							if (type == eObjectType.Leather)
								return true;
							return false;
						}
						if (level < 20)
						{
							if (type == eObjectType.Studded)
								return true;
							return false;
						}
						else
						{
							if (type == eObjectType.Chain)
								return true;
							return false;
						}
					}
				case eProperty.Skill_HandToHand:
					{
						if (type == eObjectType.Studded)
							return true;
						return false;
					}
				case eProperty.Skill_Instruments:
					{
						if (level < 10)
						{
							if (type == eObjectType.Leather)
								return true;
							return false;
						}
						else if (level < 20)
						{
							if (type == eObjectType.Studded)
								return true;
							return false;
						}
						else
						{
							if (type == eObjectType.Chain)
								return true;
							return false;
						}
					}
				case eProperty.Skill_Large_Weapon:
					{
						if (level < 15)
						{
							if (type == eObjectType.Reinforced)
								return true;

							return false;
						}
						else
						{
							if (type == eObjectType.Scale)
								return true;

							return false;
						}
					}
				case eProperty.Skill_Left_Axe:
					{
						if (type == eObjectType.Leather || type == eObjectType.Studded)
							return true;
						break;
					}
				case eProperty.Skill_Music:
					{
						if (level < 15)
						{
							if (type == eObjectType.Leather)
								return true;
							return false;
						}
						else
						{
							if (type == eObjectType.Reinforced)
								return true;
							return false;
						}
					}
				case eProperty.Skill_Nature:
					{
						if (level < 10)
						{
							if (type == eObjectType.Leather)
								return true;
							return false;
						}
						else if (level < 20)
						{
							if (type == eObjectType.Reinforced)
								return true;
							return false;
						}
						else
						{
							if (type == eObjectType.Scale)
								return true;
							return false;
						}
					}
				case eProperty.Skill_Nurture:
				case eProperty.Skill_Regrowth:
					{
						if (level < 10)
						{
							if (type == eObjectType.Leather)
								return true;
							return false;
						}
						else
						{
							if (type == eObjectType.Reinforced || type == eObjectType.Scale)
								return true;
							return false;
						}
					}
				case eProperty.Skill_OdinsWill:
					{
						if (level < 10)
						{
							if (type == eObjectType.Studded)
								return true;
							return false;
						}
						else
						{
							if (type == eObjectType.Chain)
								return true;
							return false;
						}
					}
				case eProperty.Skill_Pacification:
					{
						if (level < 10)
						{
							if (type == eObjectType.Leather)
								return true;
							return false;
						}
						else if (level < 20)
						{
							if (type == eObjectType.Studded)
								return true;
							return false;
						}
						else
						{
							if (type == eObjectType.Chain)
								return true;
							return false;
						}
					}
				case eProperty.Skill_Parry:
					{
						if (type == eObjectType.Cloth && realm == eRealm.Hibernia && level >= 5)
							return true;
						else if (realm == eRealm.Hibernia && level < 2)
							return false;
						else if (realm == eRealm.Albion && level < 5)
							return false;
						else if (realm == eRealm.Albion && level < 10 && type == eObjectType.Studded)
							return true;
						else if (realm == eRealm.Albion && level >= 10 && (type == eObjectType.Leather || type == eObjectType.Chain || type == eObjectType.Plate))
							return true;
						else if (realm == eRealm.Hibernia && level < 20 && type == eObjectType.Reinforced)
							return true;
						else if (realm == eRealm.Hibernia && level >= 15 && type == eObjectType.Scale)
							return true;
						else if (realm == eRealm.Midgard && (type == eObjectType.Studded || type == eObjectType.Chain))
							return true;

						break;
					}
				case eProperty.Skill_Piercing:
					{
						if (type == eObjectType.Leather || type == eObjectType.Reinforced)
							return true;
						return false;
					}
				case eProperty.Skill_Polearms:
					{
						if (level < 5 && type == eObjectType.Studded)
						{
							return true;
						}
						else if (level < 15)
						{
							if (type == eObjectType.Chain)
								return true;

							return false;
						}
						else
						{
							if (type == eObjectType.Plate)
								return true;

							return false;
						}
					}
				case eProperty.Skill_Rejuvenation:
					{
						if (type == eObjectType.Cloth)
							return true;
						else if (type == eObjectType.Leather)
							return true;
						else if (type == eObjectType.Studded && level >= 10 && level < 20)
							return true;
						else if (type == eObjectType.Chain && level >= 20)
							return true;
						break;
					}
				case eProperty.Skill_Savagery:
					{
						if (type == eObjectType.Studded)
							return true;
						break;
					}
				case eProperty.Skill_Scythe:
					{
						if (type == eObjectType.Cloth)
							return true;
						break;
					}
				case eProperty.Skill_Shields:
					{
						if (type == eObjectType.Cloth && realm == eRealm.Albion)
							return true;
						else if (type == eObjectType.Studded || type == eObjectType.Chain || type == eObjectType.Reinforced || type == eObjectType.Scale || type == eObjectType.Plate)
							return true;
						break;
					}
				case eProperty.Skill_ShortBow:
					{
						return false;
					}
				case eProperty.Skill_Smiting:
					{
						if (type == eObjectType.Leather && level < 10)
							return true;
						else if (type == eObjectType.Studded && level < 20)
							return true;
						else if (type == eObjectType.Chain && level >= 20)
							return true;
						break;
					}
				case eProperty.Skill_SoulRending:
					{
						if (type == eObjectType.Studded && level < 10)
							return true;
						else if (type == eObjectType.Chain && level >= 10)
							return true;
						break;
					}
				case eProperty.Skill_Spear:
					{
						if (type == eObjectType.Leather && level < 10)
							return true;
						else if (type == eObjectType.Studded)
							return true;
						else if (type == eObjectType.Chain && level >= 10)
							return true;
						break;
					}
				case eProperty.Skill_Staff:
					{
						if (type == eObjectType.Leather && realm == eRealm.Albion)
							return true;
						break;
					}
				case eProperty.Skill_Stealth:
					{
						if (type == eObjectType.Leather || type == eObjectType.Studded || type == eObjectType.Reinforced)
							return true;
						else if (realm == eRealm.Albion && level >= 20 && type == eObjectType.Chain)
							return true;
						break;
					}
				case eProperty.Skill_Stormcalling:
					{
						if (type == eObjectType.Studded && level < 10)
							return true;
						else if (type == eObjectType.Chain && level >= 10)
							return true;
						break;
					}
				case eProperty.Skill_Subterranean:
					{
						if (type == eObjectType.Leather && level < 10)
							return true;
						else if (type == eObjectType.Studded && level < 20)
							return true;
						else if (type == eObjectType.Chain && level >= 20)
							return true;
						break;
					}
				case eProperty.Skill_Sword:
					{
						if (type == eObjectType.Studded || type == eObjectType.Chain)
							return true;
						break;
					}
				case eProperty.Skill_Slashing:
					{
						if (type == eObjectType.Leather || type == eObjectType.Studded || type == eObjectType.Chain || type == eObjectType.Plate)
							return true;
						break;
					}
				case eProperty.Skill_Thrusting:
					{
						if (type == eObjectType.Leather || type == eObjectType.Studded || type == eObjectType.Chain || type == eObjectType.Plate)
							return true;
						break;
					}
				case eProperty.Skill_Two_Handed:
					{
						if (type == eObjectType.Studded && level < 10)
							return true;
						else if (type == eObjectType.Chain && level < 20)
							return true;
						else if (type == eObjectType.Plate)
							return true;
						break;
					}
				case eProperty.Skill_Valor:
					{
						if (type == eObjectType.Reinforced && level < 20)
							return true;
						else if (type == eObjectType.Scale)
							return true;
						break;
					}
				case eProperty.AllArcherySkills:
					{
						if (type == eObjectType.Leather && level < 10)
							return true;
						else if (level >= 10 && (type == eObjectType.Reinforced || type == eObjectType.Studded))
							return true;

						break;
					}
				case eProperty.AllDualWieldingSkills:
					{
						//Dualwielders are always above level 4 and can wear better than cloth from the start.
						if (type == eObjectType.Cloth)
							return false;
						//mercs are the only dualwielder who can wear chain
						else if (realm == eRealm.Albion && type == eObjectType.Studded && level < 10)
							return true;
						else if (realm == eRealm.Albion && type == eObjectType.Chain)
							return true;
						//all assassins wear leather, blademasters and zerks wear studded.
						else if (type == eObjectType.Leather || type == eObjectType.Reinforced || (type == eObjectType.Studded && realm == eRealm.Midgard))
							return true;
						break;
					}
				case eProperty.AllMagicSkills:
					{
						// not for scouts
						if (realm == eRealm.Albion && type == eObjectType.Studded && level >= 20)
							return false;
						// Paladins can't use + magic skills
						if (realm == eRealm.Albion && type == eObjectType.Plate)
							return false;

						return true;
					}
				case eProperty.AllMeleeWeaponSkills:
					{
						if (realm == eRealm.Midgard && type == eObjectType.Cloth)
							return false;
						else if (level >= 5)
							return true;

						break;
					}
				case eProperty.AllSkills:
					{
						return true;
					}
				case eProperty.Skill_Power_Strikes:
				case eProperty.Skill_Magnetism:
				case eProperty.Skill_MaulerStaff:
				case eProperty.Skill_Aura_Manipulation:
				case eProperty.Skill_FistWraps:
					{
						//Maulers
						if (type == eObjectType.Leather) //Maulers can only wear leather.
							return true;

						break;
					}

			}

			return false;
		}

		private bool SkillIsValidForWeapon(eProperty property)
		{
			int level = this.Level;
			eRealm realm = (eRealm)this.Realm;
			eObjectType type = (eObjectType)this.Object_Type;

			switch (property)
			{
				case eProperty.Skill_Arboreal:
				case eProperty.Skill_Body:
				case eProperty.Skill_BoneArmy:
				case eProperty.Skill_Cold:
				case eProperty.Skill_Creeping:
				case eProperty.Skill_Cursing:
				case eProperty.Skill_Darkness:
				case eProperty.Skill_Death_Servant:
				case eProperty.Skill_DeathSight:
				case eProperty.Skill_Earth:
				case eProperty.Skill_Enchantments:
				case eProperty.Skill_EtherealShriek:
				case eProperty.Skill_Fire:
				case eProperty.Skill_Hexing:
				case eProperty.Skill_Light:
				case eProperty.Skill_Mana:
				case eProperty.Skill_Matter:
				case eProperty.Skill_Mentalism:
				case eProperty.Skill_Mind:
				case eProperty.Skill_Pain_working:
				case eProperty.Skill_PhantasmalWail:
				case eProperty.Skill_Runecarving:
				case eProperty.Skill_SpectralForce:
				case eProperty.Skill_Spirit:
				case eProperty.Skill_Summoning:
				case eProperty.Skill_Suppression:
				case eProperty.Skill_Verdant:
				case eProperty.Skill_Void:
				case eProperty.Skill_Wind:
				case eProperty.Skill_Witchcraft:
					{
						if (type == eObjectType.Staff && this.Description != "friar")
							return true;
						break;
					}
				//healer things
				case eProperty.Skill_Smiting:
					{
						if ((type == eObjectType.Shield && this.Type_Damage < 3) || type == eObjectType.CrushingWeapon)
							return true;
						break;
					}
				case eProperty.Skill_Enhancement:
				case eProperty.Skill_Rejuvenation:
					{
						if ((type == eObjectType.Staff && this.Description == "friar") || (type == eObjectType.Shield && this.Type_Damage < 3) || type == eObjectType.CrushingWeapon)
							return true;
						break;
					}
				case eProperty.Skill_Augmentation:
				case eProperty.Skill_Mending:
				case eProperty.Skill_Subterranean:
				case eProperty.Skill_Nurture:
				case eProperty.Skill_Nature:
				case eProperty.Skill_Regrowth:
					{
						if (type == eObjectType.Hammer || (type == eObjectType.Shield && this.Type_Damage < 2) || type == eObjectType.Blunt || type == eObjectType.Blades)
							return true;
						break;
					}
				//archery things
				case eProperty.Skill_Archery:
					if (type == eObjectType.CompositeBow || type == eObjectType.RecurvedBow || type == eObjectType.Longbow)
						return true;
					break;
				case eProperty.Skill_Composite:
					{
						if (type == eObjectType.CompositeBow)
							return true;
						break;
					}
				case eProperty.Skill_RecurvedBow:
					{
						if (type == eObjectType.RecurvedBow)
							return true;
						break;
					}
				case eProperty.Skill_Long_bows:
					{
						if (type == eObjectType.Longbow)
							return true;
						break;
					}
				//other specifics
				case eProperty.Skill_Staff:
					{
						if (type == eObjectType.Staff && this.Description == "friar")
							return true;
						break;
					}
				case eProperty.Skill_Axe:
					{
						if (type == eObjectType.Axe || type == eObjectType.LeftAxe || type == eObjectType.Shield)
							return true;
						break;
					}
				case eProperty.Skill_Battlesongs:
					{
						if (type == eObjectType.Sword || type == eObjectType.Axe || type == eObjectType.Hammer || (type == eObjectType.Shield && this.Type_Damage < 3))
							return true;
						break;
					}
				case eProperty.Skill_BeastCraft:
					{
						if (type == eObjectType.Spear)
							return true;
						break;
					}
				case eProperty.Skill_Blades:
					{
						if (type == eObjectType.Blades || type == eObjectType.Shield)
							return true;
						break;
					}
				case eProperty.Skill_Blunt:
					{
						if (type == eObjectType.Blunt || type == eObjectType.Shield)
							return true;
						break;
					}
				case eProperty.Skill_Celtic_Dual:
					{
						if (type == eObjectType.Piercing || type == eObjectType.Blades || type == eObjectType.Blunt)
							return true;
						break;
					}
				case eProperty.Skill_Celtic_Spear:
					{
						if (type == eObjectType.CelticSpear)
							return true;
						break;
					}
				case eProperty.Skill_Chants:
					{
						return false;
					}
				case eProperty.Skill_Critical_Strike:
					{
						if (type == eObjectType.Piercing || type == eObjectType.SlashingWeapon || type == eObjectType.ThrustWeapon || type == eObjectType.Blades || type == eObjectType.Sword || type == eObjectType.Axe || type == eObjectType.LeftAxe)
							return true;
						break;
					}
				case eProperty.Skill_Cross_Bows:
					{
						if (type == eObjectType.Crossbow)
							return true;
						break;
					}
				case eProperty.Skill_Crushing:
					{
						if (type == eObjectType.CrushingWeapon || 
							((type == eObjectType.TwoHandedWeapon || type == eObjectType.PolearmWeapon) && this.Type_Damage == (int)eDamageType.Crush) || 
							type == eObjectType.Shield)
							return true;
						break;
					}
				case eProperty.Skill_Dual_Wield:
					{
						if (type == eObjectType.SlashingWeapon || type == eObjectType.ThrustWeapon || type == eObjectType.CrushingWeapon)
							return true;
						break;
					}
				case eProperty.Skill_Envenom:
					{
						if (type == eObjectType.SlashingWeapon || type == eObjectType.ThrustWeapon)
							return true;
						break;
					}
				case eProperty.Skill_Flexible_Weapon:
					{
						if (type == eObjectType.Flexible || type == eObjectType.Shield)
							return true;
						break;
					}
				case eProperty.Skill_Hammer:
					{
						if (type == eObjectType.Hammer || type == eObjectType.Shield)
							return true;
						break;
					}
				case eProperty.Skill_HandToHand:
					{
						if (type == eObjectType.HandToHand)
							return true;
						break;
					}
				case eProperty.Skill_Instruments:
					{
						if (type == eObjectType.Instrument || (type == eObjectType.Shield && this.Type_Damage == 1))
							return true;
						break;
					}
				case eProperty.Skill_Large_Weapon:
					{
						if (type == eObjectType.LargeWeapons)
							return true;
						break;
					}
				case eProperty.Skill_Left_Axe:
					{
						if (type == eObjectType.Axe || type == eObjectType.LeftAxe)
							return true;
						break;
					}
				case eProperty.Skill_Music:
					{
						if (type == eObjectType.Blades || type == eObjectType.Blunt || (type == eObjectType.Shield && this.Type_Damage == 1) || type == eObjectType.Instrument)
							return true;
						break;
					}
				case eProperty.Skill_Nightshade:
					{
						if (type == eObjectType.Blades || type == eObjectType.Piercing || type == eObjectType.Shield)
							return true;
						break;
					}
				case eProperty.Skill_OdinsWill:
					{
						if (type == eObjectType.Sword || type == eObjectType.Spear || type == eObjectType.Shield)
							return true;
						break;
					}
				case eProperty.Skill_Pathfinding:
					{
						if (type == eObjectType.RecurvedBow || type == eObjectType.Piercing || type == eObjectType.Blades)
							return true;
						break;
					}
				case eProperty.Skill_Piercing:
					{
						if (type == eObjectType.Piercing || type == eObjectType.Shield)
							return true;
						break;
					}
				case eProperty.Skill_Polearms:
					{
						if (type == eObjectType.PolearmWeapon)
							return true;
						break;
					}
				case eProperty.Skill_Savagery:
					{
						if (type == eObjectType.Sword || type == eObjectType.Axe || type == eObjectType.Hammer || type == eObjectType.HandToHand)
							return true;
						break;
					}
				case eProperty.Skill_Scythe:
					{
						if (type == eObjectType.Scythe)
							return true;
						break;
					}

				case eProperty.Skill_VampiiricEmbrace:
				case eProperty.Skill_ShadowMastery:
					{
						if (type == eObjectType.Piercing)
							return true;
						break;
					}
				case eProperty.Skill_Shields:
					{
						if (type == eObjectType.SlashingWeapon || type == eObjectType.CrushingWeapon || type == eObjectType.ThrustWeapon || type == eObjectType.Blunt || type == eObjectType.Blades || type == eObjectType.Piercing || type == eObjectType.Shield || type == eObjectType.Axe || type == eObjectType.Sword || type == eObjectType.Hammer)
							return true;
						break;
					}
				case eProperty.Skill_ShortBow:
					{
						return false;
					}
				case eProperty.Skill_Slashing:
					{
						if (type == eObjectType.SlashingWeapon ||
							((type == eObjectType.TwoHandedWeapon || type == eObjectType.PolearmWeapon) && this.Type_Damage == (int)eDamageType.Slash) ||
							type == eObjectType.Shield)
							return true;
						break;
					}
				case eProperty.Skill_SoulRending:
					{
						if (type == eObjectType.SlashingWeapon || type == eObjectType.CrushingWeapon || type == eObjectType.ThrustWeapon || type == eObjectType.Flexible || type == eObjectType.Shield)
							return true;
						break;
					}
				case eProperty.Skill_Spear:
					{
						if (type == eObjectType.Spear)
							return true;
						break;
					}
				case eProperty.Skill_Stealth:
					{
						if (type == eObjectType.Longbow || type == eObjectType.RecurvedBow || type == eObjectType.CompositeBow || (realm == eRealm.Albion && type == eObjectType.Shield && this.Type_Damage == 1) || type == eObjectType.Spear || type == eObjectType.Sword || type == eObjectType.Axe || type == eObjectType.LeftAxe || type == eObjectType.SlashingWeapon || type == eObjectType.ThrustWeapon || type == eObjectType.Piercing || type == eObjectType.Blades || (realm == eRealm.Albion && type == eObjectType.Instrument))
							return true;
						break;
					}
				case eProperty.Skill_Stormcalling:
					{
						if (type == eObjectType.Sword || type == eObjectType.Axe || type == eObjectType.Hammer || type == eObjectType.Shield)
							return true;
						break;
					}
				case eProperty.Skill_Sword:
					{
						if (type == eObjectType.Sword || type == eObjectType.Shield)
							return true;
						break;
					}
				case eProperty.Skill_Thrusting:
					{
						if (type == eObjectType.ThrustWeapon ||
							((type == eObjectType.TwoHandedWeapon || type == eObjectType.PolearmWeapon) && this.Type_Damage == (int)eDamageType.Thrust) ||
							type == eObjectType.Shield)
							return true;
						break;
					}
				case eProperty.Skill_Two_Handed:
					{
						if (type == eObjectType.TwoHandedWeapon)
							return true;
						break;
					}
				case eProperty.Skill_Valor:
					{
						if (type == eObjectType.Blades || type == eObjectType.Piercing || type == eObjectType.Blunt || type == eObjectType.LargeWeapons || type == eObjectType.Shield)
							return true;
						break;
					}
				case eProperty.Skill_Thrown_Weapons:
					{
						return false;
					}
				case eProperty.Skill_Pacification:
					{
						if (type == eObjectType.Hammer)
							return true;
						break;
					}
				case eProperty.Skill_Dementia:
					{
						if (type == eObjectType.Piercing)
							return true;
						break;
					}
				case eProperty.AllArcherySkills:
					{
						if (type == eObjectType.CompositeBow || type == eObjectType.Longbow || type == eObjectType.RecurvedBow)
							return true;
						break;
					}
				case eProperty.AllDualWieldingSkills:
					{
						if (type == eObjectType.Axe || type == eObjectType.Sword || type == eObjectType.Hammer || type == eObjectType.LeftAxe || type == eObjectType.SlashingWeapon || type == eObjectType.CrushingWeapon || type == eObjectType.ThrustWeapon || type == eObjectType.Piercing || type == eObjectType.Blades || type == eObjectType.Blunt)
							return true;
						break;
					}
				case eProperty.AllMagicSkills:
					{
						//scouts, armsmen, paladins, mercs, blademasters, heroes, zerks, warriors do not need this.
						if (type == eObjectType.Longbow || type == eObjectType.CelticSpear || type == eObjectType.PolearmWeapon || type == eObjectType.TwoHandedWeapon || type == eObjectType.Crossbow || (type == eObjectType.Shield && this.Type_Damage > 2))
							return false;
						else
							return true;
					}
				case eProperty.AllMeleeWeaponSkills:
					{

						if (type == eObjectType.Staff && realm != eRealm.Albion)
							return false;
						else if (type == eObjectType.Staff && this.Description != "friar") // do not add if caster staff
							return false;
						else if (type == eObjectType.Longbow || type == eObjectType.CompositeBow || type == eObjectType.RecurvedBow || type == eObjectType.Crossbow || type == eObjectType.Fired || type == eObjectType.Instrument)
							return false;
						else
							return true;
					}
				case eProperty.Skill_Aura_Manipulation: //Maulers
					{
						if (type == eObjectType.MaulerStaff || type == eObjectType.FistWraps)
							return true;
						break;
					}
				case eProperty.Skill_Magnetism: //Maulers
					{
						if (type == eObjectType.FistWraps || type == eObjectType.MaulerStaff)
							return true;
						break;
					}
				case eProperty.Skill_MaulerStaff: //Maulers
					{
						if (type == eObjectType.MaulerStaff)
							return true;
						break;
					}
				case eProperty.Skill_Power_Strikes: //Maulers
					{
						if (type == eObjectType.MaulerStaff || type == eObjectType.FistWraps)
							return true;
						break;
					}
				case eProperty.Skill_FistWraps: //Maulers
					{
						if (type == eObjectType.FistWraps)
							return true;
						break;
					}
			}
			return false;
		}

		private bool StatIsValidForRealm(eProperty property)
		{
			switch (property)
			{
				case eProperty.Piety:
				case eProperty.PieCapBonus:
					{
						if (this.Realm == (int)eRealm.Hibernia)
							return false;
						break;
					}
				case eProperty.Empathy:
				case eProperty.EmpCapBonus:
					{
						if (this.Realm == (int)eRealm.Midgard || this.Realm == (int)eRealm.Albion)
							return false;
						break;
					}
				case eProperty.Intelligence:
				case eProperty.IntCapBonus:
					{
						if (this.Realm == (int)eRealm.Midgard)
							return false;
						break;
					}
			}
			return true;
		}

		private bool StatIsValidForArmor(eProperty property)
		{
			eRealm realm = (eRealm)this.Realm;
			eObjectType type = (eObjectType)this.Object_Type;

			switch (property)
			{
				case eProperty.Intelligence:
				case eProperty.IntCapBonus:
					{
						if (realm == eRealm.Midgard)
							return false;

						if (realm == eRealm.Hibernia && this.Level < 20 && type != eObjectType.Reinforced && type != eObjectType.Cloth)
							return false;

						if (realm == eRealm.Hibernia && this.Level >= 20 && type != eObjectType.Scale && type != eObjectType.Cloth)
							return false;

						if (type != eObjectType.Cloth)
							return false;

						break;
					}
				case eProperty.Acuity:
				case eProperty.AcuCapBonus:
				case eProperty.PowerPool:
				case eProperty.PowerPoolCapBonus:
					{
						if (realm == eRealm.Albion && this.Level >= 20 && type == eObjectType.Studded)
							return false;

						if (realm == eRealm.Midgard && this.Level >= 10 && type == eObjectType.Leather)
							return false;

						if (realm == eRealm.Midgard && this.Level >= 20 && type == eObjectType.Studded)
							return false;

						break;
					}
				case eProperty.Piety:
				case eProperty.PieCapBonus:
					{
						if (realm == eRealm.Albion)
						{
							if (type == eObjectType.Leather && this.Level >= 10)
								return false;

							if (type == eObjectType.Studded && this.Level >= 20)
								return false;

							if (type == eObjectType.Chain && this.Level < 10)
								return false;
						}
						else if (realm == eRealm.Midgard)
						{
							if (type == eObjectType.Leather && this.Level >= 10)
								return false;

							if (type == eObjectType.Studded && this.Level >= 20)
								return false;

							if (type == eObjectType.Chain && this.Level < 10)
								return false;
						}
						else if (realm == eRealm.Hibernia)
						{
							return false;
						}
						break;
					}
				case eProperty.Charisma:
				case eProperty.ChaCapBonus:
					{
						if (realm == eRealm.Albion)
						{
							if (type == eObjectType.Leather && this.Level >= 10)
								return false;

							if (type == eObjectType.Studded && this.Level >= 20)
								return false;

							if (type == eObjectType.Chain && this.Level < 20)
								return false;
						}
						if (realm == eRealm.Midgard)
						{
							if (type == eObjectType.Studded && this.Level >= 20)
								return false;

							if (type == eObjectType.Chain && this.Level < 20)
								return false;
						}
						else if (realm == eRealm.Hibernia)
						{
							if (type == eObjectType.Leather && this.Level >= 15)
								return false;

							if (type == eObjectType.Reinforced && this.Level < 15)
								return false;
						}
						break;
					}
				case eProperty.Empathy:
				case eProperty.EmpCapBonus:
					{
						if (realm != eRealm.Hibernia)
							return false;

						if (type == eObjectType.Leather && this.Level >= 10)
							return false;

						if (type == eObjectType.Reinforced && this.Level >= 20)
							return false;

						if (type == eObjectType.Scale && this.Level < 20)
							return false;

						break;
					}
			}
			return true;
		}

		private bool StatIsValidForWeapon(eProperty property)
		{
			eRealm realm = (eRealm)this.Realm;
			eObjectType type = (eObjectType)this.Object_Type;

			switch (type)
			{
				case eObjectType.Staff:
					{
						if ((property == eProperty.Piety || property == eProperty.PieCapBonus) && realm == eRealm.Hibernia)
							return false;
						else if ((property == eProperty.Piety || property == eProperty.PieCapBonus) && realm == eRealm.Albion && this.Description != "friar")
							return false; // caster staff
						else if (property == eProperty.Charisma || property == eProperty.Empathy || property == eProperty.ChaCapBonus || property == eProperty.EmpCapBonus)
							return false;
						else if ((property == eProperty.Intelligence || property == eProperty.IntCapBonus || property == eProperty.AcuCapBonus) && this.Description == "friar")
							return false;
						break;
					}

				case eObjectType.Shield:
					{
						if ((realm == eRealm.Albion || realm == eRealm.Midgard) && (property == eProperty.Intelligence || property == eProperty.IntCapBonus || property == eProperty.Empathy || property == eProperty.EmpCapBonus))
							return false;
						else if (realm == eRealm.Hibernia && (property == eProperty.Piety || property == eProperty.PieCapBonus))
							return false;
						else if ((realm == eRealm.Albion || realm == eRealm.Hibernia) && this.Type_Damage > 1 && (property == eProperty.Charisma || property == eProperty.ChaCapBonus))
							return false;
						else if (realm == eRealm.Midgard && this.Type_Damage > 2 && (property == eProperty.Charisma || property == eProperty.ChaCapBonus))
							return false;
						else if (this.Type_Damage > 2 && property == eProperty.MaxMana)
							return false;

						break;
					}
				case eObjectType.Blades:
				case eObjectType.Blunt:
					{
						if (property == eProperty.Piety || property == eProperty.PieCapBonus)
							return false;
						break;
					}
				case eObjectType.LargeWeapons:
				case eObjectType.Piercing:
				case eObjectType.Scythe:
					{
						if (property == eProperty.Piety || property == eProperty.Empathy || property == eProperty.Charisma)
							return false;
						break;
					}
				case eObjectType.CrushingWeapon:
					{
						if (property == eProperty.Intelligence || property == eProperty.IntCapBonus || property == eProperty.Empathy || property == eProperty.EmpCapBonus || property == eProperty.Charisma || property == eProperty.ChaCapBonus)
							return false;
						break;
					}
				case eObjectType.SlashingWeapon:
				case eObjectType.ThrustWeapon:
				case eObjectType.Hammer:
				case eObjectType.Sword:
				case eObjectType.Axe:
					{
						if (property == eProperty.Intelligence || property == eProperty.IntCapBonus || property == eProperty.Empathy || property == eProperty.EmpCapBonus || property == eProperty.AcuCapBonus || property == eProperty.Acuity)
							return false;
						break;
					}
				case eObjectType.TwoHandedWeapon:
				case eObjectType.Flexible:
					{
						if (property == eProperty.Intelligence || property == eProperty.IntCapBonus || property == eProperty.Empathy || property == eProperty.EmpCapBonus || property == eProperty.Charisma || property == eProperty.ChaCapBonus)
							return false;
						break;
					}
				case eObjectType.RecurvedBow:
				case eObjectType.CompositeBow:
				case eObjectType.Longbow:
				case eObjectType.Crossbow:
				case eObjectType.Fired:
					{
						if (property == eProperty.Intelligence || property == eProperty.IntCapBonus || property == eProperty.Empathy || property == eProperty.EmpCapBonus || property == eProperty.Charisma || property == eProperty.ChaCapBonus ||
							property == eProperty.MaxMana || property == eProperty.PowerPool || property == eProperty.PowerPoolCapBonus || property == eProperty.AcuCapBonus || property == eProperty.Acuity || property == eProperty.Piety || property == eProperty.PieCapBonus)
							return false;
						break;
					}
				case eObjectType.Spear:
				case eObjectType.CelticSpear:
				case eObjectType.LeftAxe:
				case eObjectType.PolearmWeapon:
				case eObjectType.HandToHand:
				case eObjectType.FistWraps: //Maulers
				case eObjectType.MaulerStaff: //Maulers
					{
						if (property == eProperty.Intelligence || property == eProperty.IntCapBonus || property == eProperty.Empathy || property == eProperty.EmpCapBonus || property == eProperty.Charisma || property == eProperty.ChaCapBonus ||
							property == eProperty.MaxMana || property == eProperty.PowerPool || property == eProperty.PowerPoolCapBonus || property == eProperty.AcuCapBonus || property == eProperty.Acuity || property == eProperty.Piety || property == eProperty.PieCapBonus)
							return false;
						break;
					}
				case eObjectType.Instrument:
					{
						if (property == eProperty.Intelligence || property == eProperty.IntCapBonus || property == eProperty.Empathy || property == eProperty.EmpCapBonus || property == eProperty.Piety || property == eProperty.PieCapBonus)
							return false;
						break;
					}
			}
			return true;
		}

		private void WriteBonus(eProperty property, int amount)
		{
			if (property == eProperty.AllFocusLevels)
			{
				amount = Math.Min(50, amount);
			}

			if (this.Bonus1 == 0)
			{
				this.Bonus1 = amount;
				this.Bonus1Type = (int)property;

				if (property == eProperty.AllFocusLevels)
					this.Name = "Focus " + this.Name;
			}
			else if (this.Bonus2 == 0)
			{
				this.Bonus2 = amount;
				this.Bonus2Type = (int)property;
			}
			else if (this.Bonus3 == 0)
			{
				this.Bonus3 = amount;
				this.Bonus3Type = (int)property;
			}
			else if (this.Bonus4 == 0)
			{
				this.Bonus4 = amount;
				this.Bonus4Type = (int)property;
			}
			else if (this.Bonus5 == 0)
			{
				this.Bonus5 = amount;
				this.Bonus5Type = (int)property;
			}
			else if (this.Bonus6 == 0)
			{
				this.Bonus6 = amount;
				this.Bonus6Type = (int)property;
			}
			else if (this.Bonus7 == 0)
			{
				this.Bonus7 = amount;
				this.Bonus7Type = (int)property;
			}
		}

		private bool BonusExists(eProperty property)
		{
			if (this.Bonus1Type == (int)property ||
				this.Bonus2Type == (int)property ||
				this.Bonus3Type == (int)property ||
				this.Bonus4Type == (int)property ||
				this.Bonus5Type == (int)property)
				return true;

			return false;
		}

		private int GetBonusAmount(eBonusType type, eProperty property)
		{
			switch (type)
			{
				case eBonusType.Focus:
					{
						return this.Level;
					}
				case eBonusType.Resist:
					{
						int max = (int)Math.Ceiling((((this.Level / 2.0) + 1) / 4));
						return Util.Random((int)Math.Ceiling((double)max / 2.0), max);
					}
				case eBonusType.Skill:
					{
						int max = (int)Math.Ceiling(((this.Level / 5.0) + 1) / 3);
						if (property == eProperty.AllSkills)
							max = (int)Math.Ceiling((double)max / 2.0);
						return Util.Random((int)Math.Ceiling((double)max / 2.0), max);
					}
				case eBonusType.Stat:
					{
						if (property == eProperty.MaxHealth)
						{
							int max = (int)Math.Ceiling(((double)this.Level * 4.0) / 4);
							return Util.Random((int)Math.Ceiling((double)max / 2.0), max);
						}
						else if (property == eProperty.MaxMana)
						{
							int max = (int)Math.Ceiling(((double)this.Level / 2.0 + 1) / 4);
							return Util.Random((int)Math.Ceiling((double)max / 2.0), max);
						}
						else
						{
							int max = (int)Math.Ceiling(((double)this.Level * 1.5) / 3);
							return Util.Random((int)Math.Ceiling((double)max / 2.0), max);
						}
					}
				case eBonusType.AdvancedStat:
					{
						if (property == eProperty.MaxHealthCapBonus)
							return Util.Random(5, 25); // cap is 400
						else if (property == eProperty.PowerPoolCapBonus)
							return Util.Random(1, 10); // cap is 50
						else
							return Util.Random(1, 6); // cap is 26
					}
			}
			return 1;
		}
		#endregion
		
		#region generate item type
		
		
		private static eObjectType GenerateObjectType(eRealm realm, byte level)
		{
			eGenerateType type = eGenerateType.None;
			if (Util.Chance(ROG_ARMOR_CHANCE)) type = eGenerateType.Armor;
			else if (Util.Chance(ROG_MAGICAL_CHANCE)) type = eGenerateType.Magical;
			else type = eGenerateType.Weapon;

			switch ((eRealm)realm)
			{
				case eRealm.Albion:
					{
						int maxArmor = AlbionArmor.Length - 1;
						int maxWeapon = AlbionWeapons.Length - 1;

						if (level < 15)
							maxArmor--; // remove plate

						if (level < 5)
						{
							maxArmor--; // remove chain
							maxWeapon = 4; // remove all but base weapons and shield
						}

						switch (type)
						{
							case eGenerateType.Armor: return AlbionArmor[Util.Random(0, maxArmor)];
							case eGenerateType.Weapon: return AlbionWeapons[Util.Random(0, maxWeapon)];
							case eGenerateType.Magical: return eObjectType.Magical;
						}
						break;
					}
				case eRealm.Midgard:
					{
						int maxArmor = MidgardArmor.Length - 1;
						int maxWeapon = MidgardWeapons.Length - 1;

						if (level < 10)
							maxArmor--; // remove chain

						if (level < 5)
						{
							maxWeapon = 4; // remove all but base weapons and shield
						}

						switch (type)
						{
							case eGenerateType.Armor: return MidgardArmor[Util.Random(0, maxArmor)];
							case eGenerateType.Weapon: return MidgardWeapons[Util.Random(0, maxWeapon)];
							case eGenerateType.Magical: return eObjectType.Magical;
						}
						break;
					}
				case eRealm.Hibernia:
					{
						int maxArmor = HiberniaArmor.Length - 1;
						int maxWeapon = HiberniaWeapons.Length - 1;

						if (level < 15)
							maxArmor--; // remove scale

						if (level < 5)
						{
							maxWeapon = 4; // remove all but base weapons and shield
						}

						switch (type)
						{
							case eGenerateType.Armor: return HiberniaArmor[Util.Random(0, maxArmor)];
							case eGenerateType.Weapon: return HiberniaWeapons[Util.Random(0, maxWeapon)];
							case eGenerateType.Magical: return eObjectType.Magical;
						}
						break;
					}
			}
			return eObjectType.GenericItem;
		}

		private static eInventorySlot GenerateItemType(eObjectType type)
		{
			if ((int)type >= (int)eObjectType._FirstArmor && (int)type <= (int)eObjectType._LastArmor)
				return (eInventorySlot)ArmorSlots[Util.Random(0, ArmorSlots.Length - 1)];
			switch (type)
			{
				//left or right standard
				//tolakram - left hand usable now set based on speed
				case eObjectType.HandToHand:
				case eObjectType.Piercing:
				case eObjectType.Blades:
				case eObjectType.Blunt:
				case eObjectType.SlashingWeapon:
				case eObjectType.CrushingWeapon:
				case eObjectType.ThrustWeapon:
				case eObjectType.FistWraps: //Maulers
				case eObjectType.Flexible:
					return (eInventorySlot)Slot.RIGHTHAND;
				//left or right or twohand
				case eObjectType.Sword:
				case eObjectType.Axe:
				case eObjectType.Hammer:
					return (eInventorySlot)Util.Random(Slot.RIGHTHAND, Slot.TWOHAND);
				//left
				case eObjectType.LeftAxe:
				case eObjectType.Shield:
					return (eInventorySlot)Slot.LEFTHAND;
				//twohanded
				case eObjectType.LargeWeapons:
				case eObjectType.CelticSpear:
				case eObjectType.PolearmWeapon:
				case eObjectType.Spear:
				case eObjectType.Staff:
				case eObjectType.Scythe:
				case eObjectType.TwoHandedWeapon:
				case eObjectType.MaulerStaff:
					return (eInventorySlot)Slot.TWOHAND;
				//ranged
				case eObjectType.CompositeBow:
				case eObjectType.Fired:
				case eObjectType.Longbow:
				case eObjectType.RecurvedBow:
				case eObjectType.Crossbow:
					return (eInventorySlot)Slot.RANGED;
				case eObjectType.Magical:
					return (eInventorySlot)MagicalSlots[Util.Random(0, MagicalSlots.Length - 1)];
				case eObjectType.Instrument:
					return (eInventorySlot)Util.Random(Slot.TWOHAND, Slot.RANGED);
			}
			return eInventorySlot.FirstEmptyBackpack;
		}

		private static eDamageType GenerateDamageType(eObjectType type)
		{
			switch (type)
			{
				//all
				case eObjectType.TwoHandedWeapon:
				case eObjectType.PolearmWeapon:
				case eObjectType.Instrument:
					return (eDamageType)Util.Random(1, 3);
				//slash
				case eObjectType.Axe:
				case eObjectType.Blades:
				case eObjectType.SlashingWeapon:
				case eObjectType.LeftAxe:
				case eObjectType.Sword:
				case eObjectType.Scythe:
					return eDamageType.Slash;
				//thrust
				case eObjectType.ThrustWeapon:
				case eObjectType.Piercing:
				case eObjectType.CelticSpear:
				case eObjectType.Longbow:
				case eObjectType.RecurvedBow:
				case eObjectType.CompositeBow:
				case eObjectType.Fired:
				case eObjectType.Crossbow:
					return eDamageType.Thrust;
				//crush
				case eObjectType.Hammer:
				case eObjectType.CrushingWeapon:
				case eObjectType.Blunt:
				case eObjectType.MaulerStaff: //Maulers
				case eObjectType.FistWraps: //Maulers
				case eObjectType.Staff:
					return eDamageType.Crush;
				//specifics
				case eObjectType.HandToHand:
				case eObjectType.Spear:
					return (eDamageType)Util.Random(2, 3);
				case eObjectType.LargeWeapons:
				case eObjectType.Flexible:
					return (eDamageType)Util.Random(1, 2);
				//do shields return the shield size?
				case eObjectType.Shield:
					return (eDamageType)Util.Random(1, 3);

			}
			return eDamageType.Natural;
		}		

		#endregion		
		
		#region generate item speed and abs

		private static int GetAbsorb(eObjectType type)
		{
			switch (type)
			{
				case eObjectType.Cloth: return 0;
				case eObjectType.Leather: return 10;
				case eObjectType.Studded: return 19;
				case eObjectType.Reinforced: return 19;
				case eObjectType.Chain: return 27;
				case eObjectType.Scale: return 27;
				case eObjectType.Plate: return 34;
				default: return 0;
			}
		}

		private void SetWeaponSpeed()
		{
			// tolakram - reset speeds based on data from allakhazam 1-26-2008
			// removed specific left hand speed - left hand usable set based on speed in GenerateItemNameModel

			switch ((eObjectType)this.Object_Type)
			{
				case eObjectType.SlashingWeapon:
				case eObjectType.CrushingWeapon:
					{
						this.SPD_ABS = Util.Random(24, 42);
						return;
					}
				case eObjectType.ThrustWeapon:
					{
						this.SPD_ABS = Util.Random(22, 38);
						return;
					}
				case eObjectType.Fired:
					{
						this.SPD_ABS = Util.Random(40, 46);
						return;
					}
				case eObjectType.TwoHandedWeapon:
					{
						this.SPD_ABS = Util.Random(38, 55);
						return;
					}
				case eObjectType.PolearmWeapon:
					{
						this.SPD_ABS = Util.Random(43, 60);
						return;
					}
				case eObjectType.Staff:
					{
						this.SPD_ABS = Util.Random(30, 55);
						return;
					}
				case eObjectType.MaulerStaff: //Maulers
					{
						this.SPD_ABS = Util.Random(34, 54);
						return;
					}
				case eObjectType.Longbow:
					{
						this.SPD_ABS = Util.Random(40, 55);
						return;
					}
				case eObjectType.Crossbow:
					{
						this.SPD_ABS = Util.Random(33, 54);
						return;
					}
				case eObjectType.Flexible:
					{
						this.SPD_ABS = Util.Random(30, 47);
						return;
					}
				case eObjectType.Sword:
				case eObjectType.Hammer:
				case eObjectType.Axe:
					{
						//28 to 58
						if (this.Hand == 1)
						{
							this.SPD_ABS = Util.Random(46, 55);  // two handed
							return;
						}
						else 
						{
							this.SPD_ABS = Util.Random(23, 45); // one handed
							return;
						}
					}
				case eObjectType.Spear:
					{
						this.SPD_ABS = Util.Random(39, 55);
						return;
					}
				case eObjectType.CompositeBow:
					{
						this.SPD_ABS = Util.Random(37, 53);
						return;
					}
				case eObjectType.LeftAxe:
					{
						this.SPD_ABS = Util.Random(24, 36);
						return;
					}
				case eObjectType.HandToHand:
					{
						this.SPD_ABS = Util.Random(26, 37);
						return;
					}
				case eObjectType.FistWraps:
					{
						this.SPD_ABS = Util.Random(28, 41);
						return;
					}
				case eObjectType.RecurvedBow:
					{
						this.SPD_ABS = Util.Random(40, 55);
						return;
					}
				case eObjectType.Blades:
				case eObjectType.Blunt:
				case eObjectType.Piercing:
					{
						this.SPD_ABS = Util.Random(23, 45);
						return;
					}
				case eObjectType.LargeWeapons:
					{
						this.SPD_ABS = Util.Random(37, 57);
						return;
					}
				case eObjectType.CelticSpear:
					{
						this.SPD_ABS = Util.Random(33, 58);
						return;
					}
				case eObjectType.Scythe:
					{
						this.SPD_ABS = Util.Random(38, 55);
						return;
					}
				case eObjectType.Shield:
					{
						switch (this.Type_Damage)
						{
							case 1:
								this.SPD_ABS = 30;
								return;
							case 2:
								this.SPD_ABS = 40;
								return;
							case 3:
								this.SPD_ABS = 50;
								return;
						}
						this.SPD_ABS = 50;
						return;
					}
			}
			// for unhandled types
			if (this.Hand == 1)
			{
				this.SPD_ABS = 50;  // two handed
				return;
			}
			else if (this.Hand == 2)
			{
				this.SPD_ABS = 30;  // left hand
				return;
			}
			else 
			{
				this.SPD_ABS = 40; // right hand
				return;
			}
		}

		public void GenerateItemWeight()
		{
			eObjectType type = (eObjectType)this.Object_Type;
			eInventorySlot slot = (eInventorySlot)this.Item_Type;
			
			switch (type)
			{
				case eObjectType.LeftAxe:
				case eObjectType.Flexible:
				case eObjectType.Axe:
				case eObjectType.Blades:
				case eObjectType.HandToHand:
				case eObjectType.FistWraps: //Maulers
					this.Weight = 20;
					return;
				case eObjectType.CompositeBow:
				case eObjectType.RecurvedBow:
				case eObjectType.Longbow:
				case eObjectType.Blunt:
				case eObjectType.CrushingWeapon:
				case eObjectType.Fired:
				case eObjectType.Hammer:
				case eObjectType.Piercing:
				case eObjectType.SlashingWeapon:
				case eObjectType.Sword:
				case eObjectType.ThrustWeapon:
					this.Weight = 30;
					return;
				case eObjectType.Crossbow:
				case eObjectType.Spear:
				case eObjectType.CelticSpear:
				case eObjectType.Staff:
				case eObjectType.TwoHandedWeapon:
				case eObjectType.MaulerStaff: //Maulers
					this.Weight = 40;
					return;
				case eObjectType.Scale:
				case eObjectType.Chain:
					{
						switch (slot)
						{
							case eInventorySlot.ArmsArmor: this.Weight = 48; return;
							case eInventorySlot.FeetArmor: this.Weight = 32; return;
							case eInventorySlot.HandsArmor: this.Weight = 32; return;
							case eInventorySlot.HeadArmor: this.Weight = 32; return;
							case eInventorySlot.LegsArmor: this.Weight = 56; return;
							case eInventorySlot.TorsoArmor: this.Weight = 80; return;
						}
						this.Weight = 0;
						return;
					}
				case eObjectType.Cloth:
					{
						switch (slot)
						{
							case eInventorySlot.ArmsArmor: this.Weight = 8; return;
							case eInventorySlot.FeetArmor: this.Weight = 8; return;
							case eInventorySlot.HandsArmor: this.Weight = 8; return;
							case eInventorySlot.HeadArmor: this.Weight = 32; return;
							case eInventorySlot.LegsArmor: this.Weight = 14; return;
							case eInventorySlot.TorsoArmor: this.Weight = 20; return;
						}
						this.Weight = 0;
						return;
					}
				case eObjectType.Instrument:
					this.Weight = 15;
					return;
				case eObjectType.LargeWeapons:
					this.Weight = 50;
					return;
				case eObjectType.Leather:
					{
						switch (slot)
						{
							case eInventorySlot.ArmsArmor: this.Weight = 24; return;
							case eInventorySlot.FeetArmor: this.Weight = 16; return;
							case eInventorySlot.HandsArmor: this.Weight = 16; return;
							case eInventorySlot.HeadArmor: this.Weight = 16; return;
							case eInventorySlot.LegsArmor: this.Weight = 28; return;
							case eInventorySlot.TorsoArmor: this.Weight = 40; return;
						}
						this.Weight = 0;
						return;
					}
				case eObjectType.Magical:
					this.Weight = 5;
					return;
				case eObjectType.Plate:
					{
						switch (slot)
						{
							case eInventorySlot.ArmsArmor: this.Weight = 54; return;
							case eInventorySlot.FeetArmor: this.Weight = 36; return;
							case eInventorySlot.HandsArmor: this.Weight = 36; return;
							case eInventorySlot.HeadArmor: this.Weight = 40; return;
							case eInventorySlot.LegsArmor: this.Weight = 63; return;
							case eInventorySlot.TorsoArmor: this.Weight = 90; return;
						}
						this.Weight = 0;
						return;
					}
				case eObjectType.PolearmWeapon:
					this.Weight = 60;
					return;
				case eObjectType.Reinforced:
				case eObjectType.Studded:
					{
						switch (slot)
						{
							case eInventorySlot.ArmsArmor: this.Weight = 36; return;
							case eInventorySlot.FeetArmor: this.Weight = 24; return;
							case eInventorySlot.HandsArmor: this.Weight = 24; return;
							case eInventorySlot.HeadArmor: this.Weight = 24; return;
							case eInventorySlot.LegsArmor: this.Weight = 42; return;
							case eInventorySlot.TorsoArmor: this.Weight = 60; return;
						}
						this.Weight = 0;
						return;
					}
				case eObjectType.Scythe:
					this.Weight = 40;
					return;
				case eObjectType.Shield:
					switch (this.Type_Damage)
					{
						case 1:
							this.Weight = 31;
							return;
						case 2:
							this.Weight = 35;
							return;
						case 3:
							this.Weight = 38;
							return;
					}
					this.Weight = 31;
					return;
			}
			this.Weight = 10;
			return;
		}		
		
		#endregion
		
		#region Naming and Modeling
		public bool WriteMagicalName(eProperty property)
		{

			if (hPropertyToMagicPrefix.ContainsKey(property))
			{
				string str = hPropertyToMagicPrefix[property];
				if (str != string.Empty)
					this.Name = str + " " + this.Name;
				return true;
			}

			return false;
		}
		
		
		private void GenerateItemNameModel()
		{
			eInventorySlot slot = (eInventorySlot)this.Item_Type;
			eDamageType damage = (eDamageType)this.Type_Damage;
			eRealm realm = (eRealm)this.Realm;
			eObjectType type = (eObjectType)this.Object_Type;

			string name = "No Name";
			int model = 488;
			bool canAddExtension = false;

			switch (type)
			{
				//armor
				case eObjectType.Cloth:
					{
						name = "Cloth " + ArmorSlotToName(slot, type);

						switch (realm)
						{
							case eRealm.Albion:
								switch (slot)
								{
									case eInventorySlot.ArmsArmor: model = 141; break;
									case eInventorySlot.LegsArmor: model = 140; break;
									case eInventorySlot.FeetArmor: model = 143; break;
									case eInventorySlot.HeadArmor: model = 822; break;
									case eInventorySlot.HandsArmor: model = 142; break;
									case eInventorySlot.TorsoArmor:
										if (Util.Chance(60))
										{
											model = 139;
										}
										else
										{
											name = "Cloth Robe";

											switch (Util.Random(2))
											{
												case 0: model = 58; break;
												case 1: model = 65; break;
												case 2: model = 66; break;
											}
										}
										break;
								}
								break;

							case eRealm.Midgard:
								switch (slot)
								{
									case eInventorySlot.ArmsArmor: model = 247; break;
									case eInventorySlot.LegsArmor: model = 246; break;
									case eInventorySlot.FeetArmor: model = 249; break;
									case eInventorySlot.HeadArmor: model = 825; break;
									case eInventorySlot.HandsArmor: model = 248; break;
									case eInventorySlot.TorsoArmor:
										if (Util.Chance(60))
										{
											model = 245;
										}
										else
										{
											name = "Cloth Robe";

											switch (Util.Random(2))
											{
												case 0: model = 58; break;
												case 1: model = 65; break;
												case 2: model = 66; break;
											}
										}
										break;
								}
								break;

							case eRealm.Hibernia:
								switch (slot)
								{
									case eInventorySlot.ArmsArmor: model = 380; break;
									case eInventorySlot.LegsArmor: model = 379; break;
									case eInventorySlot.FeetArmor: model = 382; break;
									case eInventorySlot.HeadArmor: model = 826; break;
									case eInventorySlot.HandsArmor: model = 381; break;
									case eInventorySlot.TorsoArmor:
										if (Util.Chance(60))
										{
											model = 378;
										}
										else
										{
											name = "Cloth Robe";

											switch (Util.Random(2))
											{
												case 0: model = 58; break;
												case 1: model = 65; break;
												case 2: model = 66; break;
											}
										}
										break;
								}
								break;

						}

						if (slot != eInventorySlot.HeadArmor)
							canAddExtension = true;

						break;
					}
				case eObjectType.Leather:
					{
						name = "Leather " + ArmorSlotToName(slot, type);

						switch (realm)
						{
							case eRealm.Albion:
								switch (slot)
								{
									case eInventorySlot.ArmsArmor: model = 38; break;
									case eInventorySlot.LegsArmor: model = 37; break;
									case eInventorySlot.FeetArmor: model = 40; break;
									case eInventorySlot.HeadArmor: model = 62; break;
									case eInventorySlot.TorsoArmor: model = 36; break;
									case eInventorySlot.HandsArmor: model = 39; break;
								}
								break;

							case eRealm.Midgard:
								switch (slot)
								{
									case eInventorySlot.ArmsArmor: model = 242; break;
									case eInventorySlot.LegsArmor: model = 241; break;
									case eInventorySlot.FeetArmor: model = 244; break;
									case eInventorySlot.HeadArmor: model = 335; break;
									case eInventorySlot.TorsoArmor: model = 240; break;
									case eInventorySlot.HandsArmor: model = 243; break;
								}
								break;

							case eRealm.Hibernia:
								switch (slot)
								{
									case eInventorySlot.ArmsArmor: model = 395; break;
									case eInventorySlot.LegsArmor: model = 394; break;
									case eInventorySlot.FeetArmor: model = 397; break;
									case eInventorySlot.HeadArmor: model = 438; break;
									case eInventorySlot.TorsoArmor: model = 393; break;
									case eInventorySlot.HandsArmor: model = 396; break;
								}
								break;

						}

						if (slot != eInventorySlot.HeadArmor)
							canAddExtension = true;

						break;
					}
				case eObjectType.Studded:
					{
						name = "Studded " + ArmorSlotToName(slot, type);
						switch (realm)
						{
							case eRealm.Albion:
								switch (slot)
								{
									case eInventorySlot.ArmsArmor: model = 83; break;
									case eInventorySlot.LegsArmor: model = 82; break;
									case eInventorySlot.FeetArmor: model = 84; break;
									case eInventorySlot.HeadArmor: model = 824; break;
									case eInventorySlot.TorsoArmor: model = 81; break;
									case eInventorySlot.HandsArmor: model = 85; break;
								}
								break;

							case eRealm.Midgard:
								switch (slot)
								{
									case eInventorySlot.ArmsArmor: model = 232; break;
									case eInventorySlot.LegsArmor: model = 231; break;
									case eInventorySlot.FeetArmor: model = 234; break;
									case eInventorySlot.HeadArmor: model = 829; break;
									case eInventorySlot.TorsoArmor: model = 230; break;
									case eInventorySlot.HandsArmor: model = 233; break;
								}
								break;
						}

						if (slot != eInventorySlot.HeadArmor)
							canAddExtension = true;

						break;
					}
				case eObjectType.Plate:
					{
						name = "Plate " + ArmorSlotToName(slot, type);
						switch (slot)
						{
							case eInventorySlot.ArmsArmor: model = 48; break;
							case eInventorySlot.LegsArmor: model = 47; break;
							case eInventorySlot.FeetArmor: model = 50; break;
							case eInventorySlot.HandsArmor: model = 49; break;
							case eInventorySlot.HeadArmor:
								if (Util.Chance(25))
								{
									model = 93;
									name = "Plate Full Helm";
								}
								else
									model = 64;

							break;

							case eInventorySlot.TorsoArmor:
								name = ArmorSlotToName(slot, type); // Breastplate
								model = 46;
							break;
						}

						if (slot != eInventorySlot.HeadArmor)
							canAddExtension = true;

						break;
					}
				case eObjectType.Chain:
					{
						name = "Chain " + ArmorSlotToName(slot, type);
						switch (realm)
						{
							case eRealm.Albion:
								switch (slot)
								{
									case eInventorySlot.ArmsArmor: model = 43; break;
									case eInventorySlot.LegsArmor: model = 42; break;
									case eInventorySlot.FeetArmor: model = 45; break;
									case eInventorySlot.HeadArmor: model = 63; break;
									case eInventorySlot.TorsoArmor:	model = 41;	break;
									case eInventorySlot.HandsArmor: model = 44; break;
								}
								break;

							case eRealm.Midgard:
								switch (slot)
								{
									case eInventorySlot.ArmsArmor: model = 237; break;
									case eInventorySlot.LegsArmor: model = 236; break;
									case eInventorySlot.FeetArmor: model = 239; break;
									case eInventorySlot.HeadArmor: model = 832; break;
									case eInventorySlot.TorsoArmor:	model = 235; break;
									case eInventorySlot.HandsArmor: model = 238; break;
								}
								break;
						}

						if (slot != eInventorySlot.HeadArmor)
							canAddExtension = true;

						break;
					}
				case eObjectType.Reinforced:
					{
						name = "Reinforced " + ArmorSlotToName(slot, type);
						switch (slot)
						{
							case eInventorySlot.ArmsArmor: model = 385; break;
							case eInventorySlot.LegsArmor: model = 384; break;
							case eInventorySlot.FeetArmor: model = 387; break;
							case eInventorySlot.HeadArmor: model = 835; break;
							case eInventorySlot.TorsoArmor: model = 383; break;
							case eInventorySlot.HandsArmor: model = 386; break;
						}

						if (slot != eInventorySlot.HeadArmor)
							canAddExtension = true;

						break;
					}
				case eObjectType.Scale:
					{
						name = "Scale " + ArmorSlotToName(slot, type);
						switch (slot)
						{
							case eInventorySlot.ArmsArmor: model = 390; break;
							case eInventorySlot.LegsArmor: model = 389; break;
							case eInventorySlot.FeetArmor: model = 392; break;
							case eInventorySlot.HeadArmor: model = 838; break;
							case eInventorySlot.TorsoArmor: model = 388; break;
							case eInventorySlot.HandsArmor: model = 391; break;
						}

						if (slot != eInventorySlot.HeadArmor)
							canAddExtension = true;

						break;
					}

				//weapons
				case eObjectType.Axe:
					{
						if (this.Hand == 1)
						{
							if (this.SPD_ABS < 51)
							{
								name = "Large Axe";
								model = 577;
							}
							else
							{
								name = "Great Axe";
								model = 317;
							}
						}
						else // 1 handed axe; speed 28-45; 578 (hand), 316 (Bearded), 319 (War), 315 (Spiked), 573 (Double)
						{
							if (this.SPD_ABS < 25)
							{
								name = "Hand Axe";
								model = 578;
							}
							else if (this.SPD_ABS < 30)
							{
								name = "Bearded Axe";
								model = 316;
							}
							else if (this.SPD_ABS < 36)
							{
								name = "War Axe";
								model = 319;
							}
							else if (this.SPD_ABS < 40)
							{
								name = "Spiked Axe";
								model = 315;
							}
							else
							{
								name = "Double-bladed Axe";
								model = 573;
							}
						}
						break;
					}
				case eObjectType.Blades:
					{
						// Blades; speed 22 - 45; Short Sword (445), Falcata (444), Broadsword (447), Longsword (446), Bastard Sword (473)
						if (this.SPD_ABS < 27)
						{
							name = "Short Sword";
							model = 445;
							this.Hand = 2; // allow left hand
							this.Item_Type = Slot.LEFTHAND;
						}
						else if (this.SPD_ABS < 30)
						{
							name = "Falcata";
							model = 444;
							this.Hand = 2; // allow left hand
							this.Item_Type = Slot.LEFTHAND;
						}
						else if (this.SPD_ABS < 33)
						{
							name = "Broadsword";
							model = 447;
						}
						else if (this.SPD_ABS < 40)
						{
							name = "Long Sword";
							model = 446;
						}
						else
						{
							name = "Bastard Sword";
							model = 473;
						}
						break;
					}
				case eObjectType.Blunt:
					{
						// Blunt; speed 22 - 45; Club (449), Mace (450), Hammer (461), Spiked Mace (451), Pick Hammer (641)

						if (this.SPD_ABS < 30)
						{
							name = "Club";
							model = 449;
							this.Hand = 2; // allow left hand
							this.Item_Type = Slot.LEFTHAND;
						}
						else if (this.SPD_ABS < 35)
						{
							name = "Mace";
							model = 450;
							this.Hand = 2; // allow left hand
							this.Item_Type = Slot.LEFTHAND;
						}
						else if (this.SPD_ABS < 40)
						{
							name = "Hammer";
							model = 461;
						}
						else if (this.SPD_ABS < 43)
						{
							name = "Spiked Mace";
							model = 451;
						}
						else
						{
							name = "Pick Hammer";
							model = 641;
						}
						break;
					}
				case eObjectType.CelticSpear:
					{
						// Short Spear (470), Spear (469), Long Spear (476), War Spear (477)
						if (this.SPD_ABS < 35)
						{
							name = "Short Spear";
							model = 470;
						}
						else if (this.SPD_ABS < 45)
						{
							name = "Spear";
							model = 469;
						}
						else if (this.SPD_ABS < 50)
						{
							name = "Long Spear";
							model = 476;
						}
						else
						{
							name = "War Spear";
							model = 477;
						}
						break;
					}
				case eObjectType.CompositeBow:
					{
						if (this.SPD_ABS > 40)
							name = "Great Composite Bow";
						else
							name = "Composite Bow";

						model = 564;
						break;
					}
				case eObjectType.Crossbow:
					{
						name = "Crossbow";
						model = 226;
						break;
					}
				case eObjectType.CrushingWeapon:
					{
						// Hammer (12), Mace (13), Flanged Mace (14), War Hammer (15)
						if (this.SPD_ABS < 33)
						{
							name = "Hammer";
							model = 12;
							this.Hand = 2; // allow left hand
							this.Item_Type = Slot.LEFTHAND;
						}
						else if (this.SPD_ABS < 35)
						{
							name = "Mace";
							model = 13;
							this.Hand = 2; // allow left hand
							this.Item_Type = Slot.LEFTHAND;
						}
						else if (this.SPD_ABS < 40)
						{
							name = "Flanged Mace";
							model = 14;
						}
						else
						{
							name = "War Hammer";
							model = 15;
						}
						break;
					}
				case eObjectType.Fired:
					{
						if (realm == eRealm.Albion)
						{
							name = "Short Bow";
							model = 569;
						}
						else // hibernia
						{
							name = "Short Bow";
							model = 922;
						}
						break;
					}
				case eObjectType.Flexible:
					{
						switch (damage)
						{
							case eDamageType.Crush:
								{
									if (this.SPD_ABS < 33)
									{
										name = "Morning Star";
										model = 862;
									}
									else if (this.SPD_ABS < 40)
									{
										name = "Flail";
										model = 861;
									}
									else
									{
										name = "Weighted Flail";
										model = 864;
									}
									break;
								}
							case eDamageType.Slash:
								{
									if (this.SPD_ABS < 33)
									{
										name = "Whip";
										model = 867;
									}
									else if (this.SPD_ABS < 40)
									{
										name = "Chain";
										model = 857;
									}
									else
									{
										name = "War Chain";
										model = 866;
									}
									break;
								}
						}
						break;

					}
				case eObjectType.Hammer:
					{
						if (this.Hand == 1)
						{
							if (this.SPD_ABS < 50)
							{
								name = "Two Handed Hammer";
								model = 574;
							}
							if (this.SPD_ABS < 53)
							{
								name = "Two Handed War Hammer";
								model = 575;
							}
							else
							{
								name = "Great Hammer";
								model = 576;
							}
						}
						else
						{
							if (this.SPD_ABS < 30)
							{
								name = "Small Hammer";
								model = 320;
							}
							else if (this.SPD_ABS < 35)
							{
								name = "Hammer";
								model = 321;
							}
							else if (this.SPD_ABS < 40)
							{
								name = "Pick Hammer";
								model = 323;
							}
							else
							{
								name = "Battle Hammer";
								model = 324;
							}
						}
						break;
					}
				case eObjectType.HandToHand:
					{
						switch (damage)
						{
							case eDamageType.Slash:
								{
									if (this.SPD_ABS < 30)
									{
										name = "Moon Claw";
										model = 981;
									}
									else if (this.SPD_ABS < 35)
									{
										name = "Bladed Moon Claw";
										model = 961;
									}
									else
									{
										name = "Heavy Bladed Moon Claw";
										model = 975;
									}
									break;
								}
							case eDamageType.Thrust:
								{
									if (this.SPD_ABS < 30)
									{
										name = "Claw Greave";
										model = 963;
									}
									else if (this.SPD_ABS < 35)
									{
										name = "Bladed Claw Greave";
										model = 959;
									}
									else
									{
										name = "Heavy Bladed Claw Greave";
										model = 973;
									}
									break;
								}
						}
						// all hand to hand weapons usable in left hand
						this.Hand = 2; // allow left hand
						this.Item_Type = Slot.LEFTHAND;
						break;
					}
				case eObjectType.Instrument:
					{
						switch (this.DPS_AF)
						{
							case 0:
								{
									name = "Harp";
									model = 3688;
									break;
								}
							case 1:
								{
									name = "Drum";
									model = 228;
									break;
								}
							case 2:
								{
									name = "Lute";
									model = 227;
									break;
								}
							case 3:
								{
									name = "Flute";
									model = 325;
									break;
								}
						}
						break;
					}
				case eObjectType.LargeWeapons:
					{
						switch (damage)
						{
							case eDamageType.Slash:
								{
									if (this.SPD_ABS < 50)
									{
										name = "Great Falcata";
										model = 639;
									}
									else
									{
										name = "Great Sword";
										model = 459;
									}
									break;
								}
							case eDamageType.Crush:
								{
									if (this.SPD_ABS < 50)
									{
										name = "Big Shillelagh";
										model = 474;
									}
									else
									{
										name = "Great Hammer";
										model = 462;
									}
									break;
								}
						}
						break;
					}
				case eObjectType.LeftAxe:
					{
						if (this.SPD_ABS < 25)
						{
							name = "Hand Axe";
							model = 578;
						}
						else if (this.SPD_ABS < 30)
						{
							name = "Bearded Axe";
							model = 316;
						}
						else
						{
							name = "War Axe";
							model = 319;
						}
						break;
					}
				case eObjectType.Longbow:
					{
						if (this.SPD_ABS < 44)
						{
							name = "Hunting Bow";
							model = 569;
						}
						else if (this.SPD_ABS < 55)
						{
							name = "Longbow";
							model = 132;
						}
						else
						{
							name = "Heavy Longbow";
							model = 570;
						}
						break;
					}
				case eObjectType.Magical:
					{
						switch (slot)
						{
							case eInventorySlot.Cloak:
								{
									if (Util.Chance(50))
										name = "Mantle";
									else
										name = "Cloak";

									if (Util.Chance(50))
										model = 57;
									else if (Util.Chance(50))
										model = 559;
									else
										model = 560;

									break;
								}
							case eInventorySlot.Waist:
								{
									if (Util.Chance(50))
										name = "Belt";
									else
										name = "Girdle";

									model = 597;
									break;
								}
							case eInventorySlot.Neck:
								{
									if (Util.Chance(50))
										name = "Choker";
									else
										name = "Pendant";

									model = 101;
									break;
								}
							case eInventorySlot.Jewellery:
								{
									if (Util.Chance(50))
										name = "Gem";
									else
										name = "Jewel";

									model = Util.Random(110, 119);
									break;
								}
							case eInventorySlot.LeftBracer:
							case eInventorySlot.RightBracer:
								{
									if (Util.Chance(50))
									{
										name = "Bracelet";
										model = 619;
									}
									else
									{
										name = "Bracer";
										model = 598;
									}

									break;
								}
							case eInventorySlot.LeftRing:
							case eInventorySlot.RightRing:
								{
									if (Util.Chance(50))
										name = "Ring";
									else
										name = "Wrap";

									model = 103;
									break;
								}
						}
						break;
					}
				case eObjectType.Piercing:
					{
						if (this.SPD_ABS < 24)
						{
							name = "Dirk";
							model = 454;
							this.Hand = 2; // allow left hand
							this.Item_Type = Slot.LEFTHAND;
						}
						else if (this.SPD_ABS < 27)
						{
							name = "Stiletto";
							model = 456;
							this.Hand = 2; // allow left hand
							this.Item_Type = Slot.LEFTHAND;
						}
						else if (this.SPD_ABS < 30)
						{
							name = "Curved Dagger";
							model = 457;
						}
						else
						{
							name = "Rapier";
							model = 455;
						}
						break;
					}
				case eObjectType.PolearmWeapon:
					{
						switch (damage)
						{
							case eDamageType.Slash:
								{
									name = "Lochaber Axe";
									model = 68;
									break;
								}
							case eDamageType.Thrust:
								{
									name = "Pike";
									model = 69;
									break;
								}
							case eDamageType.Crush:
								{
									name = "Lucerne Hammer";
									model = 70;
									break;
								}
						}
						break;
					}
				case eObjectType.RecurvedBow:
					{
						if (this.SPD_ABS > 49)
						{
							name = "Great Recurve Bow";
							model = 925;
						}
						else
						{
							name = "Recurve Bow";
							model = 924;
						}
						break;
					}
				case eObjectType.Scythe:
					{
						if (this.SPD_ABS < 47)
						{
							name = "Scythe";
							model = 931;
						}
						else if (this.SPD_ABS < 51)
						{
							name = "Martial Scythe";
							model = 930;
						}
						else
						{
							name = "War Scythe";
							model = 932;
						}
						break;
					}
				case eObjectType.Shield:
					{
						switch ((int)damage)
						{
							case 1:
								{
									name = "Small Shield";
									model = 59;
									break;
								}
							case 2:
								{
									name = "Medium Shield";
									model = 61;
									break;
								}
							case 3:
								{
									name = "Large Shield";
									model = 60;
									break;
								}
						}
						break;
					}
				case eObjectType.SlashingWeapon:
					{
						if (this.SPD_ABS < 26)
						{
							name = "Dagger";
							model = 1;
							this.Hand = 2; // allow left hand
							this.Item_Type = Slot.LEFTHAND;
						}
						else if (this.SPD_ABS < 30)
						{
							if (Util.Chance(25))
							{
								name = "Jambiya";
								model = 651;
							}
							else
							{
								name = "Short Sword";
								model = 3;
							}
							this.Hand = 2; // allow left hand
							this.Item_Type = Slot.LEFTHAND;
						}
						else if (this.SPD_ABS < 32)
						{
							name = "Broadsword";
							model = 5;
						}
						else if (this.SPD_ABS < 35)
						{
							name = "Scimitar";
							model = 8;
						}
						else if (this.SPD_ABS < 40)
						{
							name = "Long Sword";
							model = 4;
						}
						else
						{
							name = "Bastard Sword";
							model = 10;
						}
						break;
					}
				case eObjectType.Spear:
					{
						if (this.SPD_ABS < 43)
						{
							name = "Spear";
							model = 328;
						}
						else if (this.SPD_ABS < 50)
						{
							name = "Long Spear";
							model = 329;
						}
						else
						{
							name = "Great Spear";
							model = 332;
						}
						break;
					}
				case eObjectType.MaulerStaff:
					{
						name = "Mauler Staff";
						model = 19;
						break;
					}
				case eObjectType.Staff:
					{
						switch (realm)
						{
							case eRealm.Albion:

								if (Util.Chance(20))
								{
									this.Description = "friar";

									if (this.SPD_ABS < 40)
									{
										name = "Quarterstaff";
										model = 442;
									}
									else if (this.SPD_ABS < 50)
									{
										name = "Shod Quarterstaff";
										model = 567;
									}
									else
									{
										name = "Heavy Shod Quarterstaff";
										model = 884;
									}
								}
								else
								{
									if (this.SPD_ABS < 40)
									{
										name = "Staff";
										model = 568;
									}
									else
									{
										name = "Wand";
										model = 19;
									}
								}
								break;

							case eRealm.Midgard:

								if (this.SPD_ABS < 40)
								{
									name = "Staff";
									model = 327;
								}
								else
								{
									name = "Rod";
									model = 565;
								}
								break;

							case eRealm.Hibernia:

								if (this.SPD_ABS < 40)
								{
									name = "Staff";
									model = 468;
								}
								else
								{
									name = "Wand";
									model = 1178;
								}
								break;
						}
						break;
					}
				case eObjectType.Sword:
					{
						if (this.Hand == 1)
						{
							if (this.SPD_ABS > 46)
							{
								name = "Great Sword";
								model = 572;
							}
							else
							{
								name = "Two Handed Sword";
								model = 314;
							}
						}
						else
						{
							if (this.SPD_ABS < 25)
							{
								name = "Dagger";
								model = 571;
							}
							else if (this.SPD_ABS < 30)
							{
								name = "Short Sword";
								model = 311;
							}
							else if (this.SPD_ABS < 32)
							{
								name = "Broadsword";
								model = 312;
							}
							else if (this.SPD_ABS < 35)
							{
								name = "Long Sword";
								model = 310;
							}
							else
							{
								name = "Bastard Sword";
								model = 313;
							}
						}
						break;
					}
				case eObjectType.ThrustWeapon:
					{
						if (this.SPD_ABS < 24)
						{
							name = "Dirk";
							model = 21;
							this.Hand = 2; // allow left hand
							this.Item_Type = Slot.LEFTHAND;
						}
						else if (this.SPD_ABS < 28)
						{
							name = "Stiletto";
							model = 71;
							this.Hand = 2; // allow left hand
							this.Item_Type = Slot.LEFTHAND;
						}
						else if (this.SPD_ABS < 30)
						{
							name = "Main Gauche";
							model = 25;
							this.Hand = 2; // allow left hand
							this.Item_Type = Slot.LEFTHAND;
						}
						else if (this.SPD_ABS < 36)
						{
							name = "Rapier";
							model = 22;
						}
						else
						{
							name = "Gladius";
							model = 30;
						}
						break;
					}
				case eObjectType.TwoHandedWeapon:
					{
						switch (damage)
						{
							case eDamageType.Slash:
								{
									if (this.SPD_ABS < 44)
									{
										name = "Two Handed Sword";
										model = 6;
									}
									else if (this.SPD_ABS < 48)
									{
										name = "Great Axe";
										model = 72;
									}
									else if (this.SPD_ABS < 51)
									{
										name = "Great Scimitar";
										model = 645;
									}
									else
									{
										name = "Great Sword";
										model = 7;
									}
									break;
								}
							case eDamageType.Crush:
								{
									name = "Great Hammer";
									model = 17;
									break;
								}
							case eDamageType.Thrust:
								{
									if (this.SPD_ABS < 46)
									{
										name = "War Mattock";
										model = 16;
									}
									else
									{
										name = "War Pick";
										model = 646;
									}
									break;
								}
						}
						break;
					}
				case eObjectType.FistWraps: // Maulers
					{
						string str = "Fist";

						if (Util.Chance(50))
							str = "Hand";

						if (this.SPD_ABS < 31)
						{
							name = str + " Wrap";
							model = 3476;
							this.Effect = 102; // smoke
							this.Hand = 2; // allow left hand
							this.Item_Type = Slot.LEFTHAND;
						}
						else if (this.SPD_ABS < 35)
						{
							name = "Studded " + str + " Wrap";
							model = 3477;
							this.Effect = 48; // fire
							this.Hand = 2; // allow left hand
							this.Item_Type = Slot.LEFTHAND;
						}
						else
						{
							name = "Spiked Fist Wrap";
							model = 3478;
							this.Effect = 49; // sparkle fire
						}

						break;
					}
			}

			this.Name = name;
			this.Model = model;

			if (canAddExtension)
			{
				if (this.Level > 50)
					this.Extension = 3;
				if (this.Level > 35)
					this.Extension = 2;
			}

		}

		private static string ArmorSlotToName(eInventorySlot slot, eObjectType type)
		{
			switch (slot)
			{
				case eInventorySlot.ArmsArmor:
					if (type == eObjectType.Plate)
						return "Arms";
					else
						return "Sleeves";

				case eInventorySlot.FeetArmor: 
					return "Boots";

				case eInventorySlot.HandsArmor:
					if (type == eObjectType.Plate)
						return "Gauntlets";
					else
						return "Gloves";

				case eInventorySlot.HeadArmor:
					if (type == eObjectType.Cloth)
						return "Cap";
					else if (type == eObjectType.Scale)
						return "Coif";
					else
						return "Helm";

				case eInventorySlot.LegsArmor:
					if (type == eObjectType.Cloth)
						return "Pants";
					else if (type == eObjectType.Plate)
						return "Legs";
					else
						return "Leggings";

				case eInventorySlot.TorsoArmor:
					if (type == eObjectType.Chain || type == eObjectType.Scale)
						return "Hauberk";
					else if (type == eObjectType.Plate)
						return "Breastplate";
					else if ((type == eObjectType.Leather || type == eObjectType.Studded) && Util.Chance(50))
						return "Jerkin";
					else
						return "Vest";

				default: return GlobalConstants.SlotToName((int)slot);
			}
		}
		
		#endregion
				
		#region definitions
		public enum eBonusType
		{
			Stat,
			AdvancedStat,
			Resist,
			Skill,
			Focus,
		}
		
		public enum eGenerateType
		{
			Weapon,
			Armor,
			Magical,
			None,
		}

		private static eProperty[] StatBonus = new eProperty[] 
		{
			eProperty.Strength,
			eProperty.Dexterity,
			eProperty.Constitution,
			eProperty.Quickness,
			//eProperty.Intelligence,
			//eProperty.Piety,
			//eProperty.Empathy,
			//eProperty.Charisma,
			eProperty.MaxMana,
			eProperty.MaxHealth,
			eProperty.Acuity,
		};

		private static eProperty[] AdvancedStats = new eProperty[] 
		{
			eProperty.PowerPool,
			eProperty.PowerPoolCapBonus,
			eProperty.StrCapBonus,
			eProperty.DexCapBonus,
			eProperty.ConCapBonus,
			eProperty.QuiCapBonus,
			//eProperty.IntCapBonus,
			//eProperty.PieCapBonus,
			//eProperty.EmpCapBonus,
			//eProperty.ChaCapBonus,
			eProperty.MaxHealthCapBonus,
			eProperty.AcuCapBonus,
		};

		private static eProperty[] ResistBonus = new eProperty[] 
		{
			eProperty.Resist_Body,
			eProperty.Resist_Cold,
			eProperty.Resist_Crush,
			eProperty.Resist_Energy,
			eProperty.Resist_Heat,
			eProperty.Resist_Matter,
			eProperty.Resist_Slash,
			eProperty.Resist_Spirit,
			eProperty.Resist_Thrust,
		};


		private static eProperty[] AlbSkillBonus = new eProperty[] 
        {
	        eProperty.Skill_Two_Handed,
	        eProperty.Skill_Body,
	        //eProperty.Skill_Chants, // bonus not used
	        eProperty.Skill_Critical_Strike,
	        eProperty.Skill_Cross_Bows,
	        eProperty.Skill_Crushing,
	        eProperty.Skill_Death_Servant,
	        eProperty.Skill_DeathSight,
	        eProperty.Skill_Dual_Wield,
	        eProperty.Skill_Earth,
	        eProperty.Skill_Enhancement,
	        eProperty.Skill_Envenom,
	        eProperty.Skill_Fire,
	        eProperty.Skill_Flexible_Weapon,
	        eProperty.Skill_Cold,
	        eProperty.Skill_Instruments,
			eProperty.Skill_Archery,
	        eProperty.Skill_Matter,
	        eProperty.Skill_Mind,
	        eProperty.Skill_Pain_working,
	        eProperty.Skill_Parry,
	        eProperty.Skill_Polearms,
	        eProperty.Skill_Rejuvenation,
	        eProperty.Skill_Shields,
	        eProperty.Skill_Slashing,
	        eProperty.Skill_Smiting,
	        eProperty.Skill_SoulRending,
	        eProperty.Skill_Spirit,
	        eProperty.Skill_Staff,
	        eProperty.Skill_Stealth,
	        eProperty.Skill_Thrusting,
	        eProperty.Skill_Wind,
			eProperty.Skill_Aura_Manipulation, //Maulers
			eProperty.Skill_FistWraps, //Maulers
			eProperty.Skill_MaulerStaff, //Maulers
			eProperty.Skill_Magnetism, //Maulers
			eProperty.Skill_Power_Strikes, //Maulers
        };


		private static eProperty[] HibSkillBonus = new eProperty[] 
        {
	        eProperty.Skill_Critical_Strike,
	        eProperty.Skill_Envenom,
	        eProperty.Skill_Parry,
	        eProperty.Skill_Shields,
	        eProperty.Skill_Stealth,
	        eProperty.Skill_Light,
	        eProperty.Skill_Void,
	        eProperty.Skill_Mana,
	        eProperty.Skill_Blades,
	        eProperty.Skill_Blunt,
	        eProperty.Skill_Piercing,
	        eProperty.Skill_Large_Weapon,
	        eProperty.Skill_Mentalism,
	        eProperty.Skill_Regrowth,
	        eProperty.Skill_Nurture,
	        eProperty.Skill_Nature,
	        eProperty.Skill_Music,
	        eProperty.Skill_Celtic_Dual,
	        eProperty.Skill_Celtic_Spear,
			eProperty.Skill_Archery,
	        eProperty.Skill_Valor,
	        eProperty.Skill_Verdant,
	        eProperty.Skill_Creeping,
	        eProperty.Skill_Arboreal,
	        eProperty.Skill_Scythe,
	        //eProperty.Skill_Nightshade, // bonus not used
	        //eProperty.Skill_Pathfinding, // bonus not used
	        eProperty.Skill_Dementia,
	        eProperty.Skill_ShadowMastery,
	        eProperty.Skill_VampiiricEmbrace,
	        eProperty.Skill_EtherealShriek,
	        eProperty.Skill_PhantasmalWail,
	        eProperty.Skill_SpectralForce,
			eProperty.Skill_Aura_Manipulation, //Maulers
			eProperty.Skill_FistWraps, //Maulers
			eProperty.Skill_MaulerStaff, //Maulers
			eProperty.Skill_Magnetism, //Maulers
			eProperty.Skill_Power_Strikes, //Maulers
        };

		private static eProperty[] MidSkillBonus = new eProperty[] 
        {
	        eProperty.Skill_Critical_Strike,
	        eProperty.Skill_Envenom,
	        eProperty.Skill_Parry,
	        eProperty.Skill_Shields,
	        eProperty.Skill_Stealth,
	        eProperty.Skill_Sword,
	        eProperty.Skill_Hammer,
	        eProperty.Skill_Axe,
	        eProperty.Skill_Left_Axe,
	        eProperty.Skill_Spear,
	        eProperty.Skill_Mending,
	        eProperty.Skill_Augmentation,
	        //Skill_Cave_Magic = 59,
	        eProperty.Skill_Darkness,
	        eProperty.Skill_Suppression,
	        eProperty.Skill_Runecarving,
	        eProperty.Skill_Stormcalling,
	        //eProperty.Skill_BeastCraft, // bonus not used
			eProperty.Skill_Archery,
	        eProperty.Skill_Battlesongs,
	        eProperty.Skill_Subterranean,
	        eProperty.Skill_BoneArmy,
	        eProperty.Skill_Thrown_Weapons,
	        eProperty.Skill_HandToHand,
    		//eProperty.Skill_Pacification,
	        //eProperty.Skill_Savagery,
	        eProperty.Skill_OdinsWill,
	        eProperty.Skill_Cursing,
	        eProperty.Skill_Hexing,
	        eProperty.Skill_Witchcraft,
    		eProperty.Skill_Summoning,
			eProperty.Skill_Aura_Manipulation, //Maulers
			eProperty.Skill_FistWraps, //Maulers
			eProperty.Skill_MaulerStaff, //Maulers
			eProperty.Skill_Magnetism, //Maulers
			eProperty.Skill_Power_Strikes, //Maulers
		};



		private static int[] ArmorSlots = new int[] { 21, 22, 23, 25, 27, 28, };
		private static int[] MagicalSlots = new int[] { 24, 26, 29, 32, 33, 34, 35, 36 };

		// the following are doubled up to work around an apparent mid-number bias to the random number generator

		// note that weapon array has been adjusted to add weight to more commonly used items
		private static eObjectType[] AlbionWeapons = new eObjectType[] 
		{
			eObjectType.ThrustWeapon,
			eObjectType.CrushingWeapon,
			eObjectType.SlashingWeapon, 
			eObjectType.Shield,
			eObjectType.Staff,
			eObjectType.TwoHandedWeapon,
			eObjectType.Longbow,
			eObjectType.Flexible,
			eObjectType.PolearmWeapon,
			eObjectType.FistWraps, //Maulers
			eObjectType.MaulerStaff,//Maulers
			eObjectType.Instrument,
			eObjectType.Crossbow,
			eObjectType.ThrustWeapon,
			eObjectType.CrushingWeapon,
			eObjectType.SlashingWeapon, 
			eObjectType.Shield,
			eObjectType.Staff,
			eObjectType.TwoHandedWeapon,
			eObjectType.Longbow,
			eObjectType.Flexible,
			eObjectType.PolearmWeapon,
			eObjectType.FistWraps, //Maulers
			eObjectType.MaulerStaff,//Maulers
			eObjectType.Instrument,
			eObjectType.Crossbow,
		};

		private static eObjectType[] AlbionArmor = new eObjectType[] 
		{
			eObjectType.Cloth,
			eObjectType.Leather,
			eObjectType.Studded,
			eObjectType.Chain,
			eObjectType.Plate,
			eObjectType.Cloth,
			eObjectType.Leather,
			eObjectType.Studded,
			eObjectType.Chain,
			eObjectType.Plate,
		};
		private static eObjectType[] MidgardWeapons = new eObjectType[] 
		{
			eObjectType.Sword,
			eObjectType.Hammer,
			eObjectType.Axe,
			eObjectType.Shield,
			eObjectType.Staff,
			eObjectType.Spear,
			eObjectType.CompositeBow ,
			eObjectType.LeftAxe,
			eObjectType.HandToHand,
			eObjectType.FistWraps,//Maulers
			eObjectType.MaulerStaff,//Maulers
			eObjectType.Sword,
			eObjectType.Hammer,
			eObjectType.Axe,
			eObjectType.Shield,
			eObjectType.Staff,
			eObjectType.Spear,
			eObjectType.CompositeBow ,
			eObjectType.LeftAxe,
			eObjectType.HandToHand,
			eObjectType.FistWraps,//Maulers
			eObjectType.MaulerStaff,//Maulers
		};

		private static eObjectType[] MidgardArmor = new eObjectType[] 
		{
			eObjectType.Cloth,
			eObjectType.Leather,
			eObjectType.Studded,
			eObjectType.Chain,
			eObjectType.Cloth,
			eObjectType.Leather,
			eObjectType.Studded,
			eObjectType.Chain,
		};

		private static eObjectType[] HiberniaWeapons = new eObjectType[] 
		{
			eObjectType.Blades,
			eObjectType.Blunt,
			eObjectType.Piercing,
			eObjectType.Shield,
			eObjectType.Staff,
			eObjectType.LargeWeapons,
			eObjectType.CelticSpear,
			eObjectType.Scythe,
			eObjectType.RecurvedBow,
			eObjectType.Instrument,
			eObjectType.FistWraps,//Maulers
			eObjectType.MaulerStaff,//Maulers
			eObjectType.Blades,
			eObjectType.Blunt,
			eObjectType.Piercing,
			eObjectType.Shield,
			eObjectType.Staff,
			eObjectType.LargeWeapons,
			eObjectType.CelticSpear,
			eObjectType.Scythe,
			eObjectType.RecurvedBow,
			eObjectType.Instrument,
			eObjectType.FistWraps,//Maulers
			eObjectType.MaulerStaff,//Maulers
		};

		private static eObjectType[] HiberniaArmor = new eObjectType[] 
		{
			eObjectType.Cloth,
			eObjectType.Leather,
			eObjectType.Reinforced,
			eObjectType.Scale,
			eObjectType.Cloth,
			eObjectType.Leather,
			eObjectType.Reinforced,
			eObjectType.Scale,
		};

		#endregion definitions

		public static void InitializeHashtables()
		{
			// Magic Prefix

			hPropertyToMagicPrefix.Add(eProperty.Strength, "Mighty");
			hPropertyToMagicPrefix.Add(eProperty.Dexterity, "Adroit");
			hPropertyToMagicPrefix.Add(eProperty.Constitution, "Fortifying");
			hPropertyToMagicPrefix.Add(eProperty.Quickness, "Speedy");
			hPropertyToMagicPrefix.Add(eProperty.Intelligence, "Insightful");
			hPropertyToMagicPrefix.Add(eProperty.Piety, "Willful");
			hPropertyToMagicPrefix.Add(eProperty.Empathy, "Attuned");
			hPropertyToMagicPrefix.Add(eProperty.Charisma, "Glib");
			hPropertyToMagicPrefix.Add(eProperty.MaxMana, "Arcane");
			hPropertyToMagicPrefix.Add(eProperty.MaxHealth, "Sturdy");
			hPropertyToMagicPrefix.Add(eProperty.PowerPool, "Arcane");

			hPropertyToMagicPrefix.Add(eProperty.Resist_Body, "Bodybender");
			hPropertyToMagicPrefix.Add(eProperty.Resist_Cold, "Icebender");
			hPropertyToMagicPrefix.Add(eProperty.Resist_Crush, "Bluntbender");
			hPropertyToMagicPrefix.Add(eProperty.Resist_Energy, "Energybender");
			hPropertyToMagicPrefix.Add(eProperty.Resist_Heat, "Heatbender");
			hPropertyToMagicPrefix.Add(eProperty.Resist_Matter, "Matterbender");
			hPropertyToMagicPrefix.Add(eProperty.Resist_Slash, "Edgebender");
			hPropertyToMagicPrefix.Add(eProperty.Resist_Spirit, "Spiritbender");
			hPropertyToMagicPrefix.Add(eProperty.Resist_Thrust, "Thrustbender");

			hPropertyToMagicPrefix.Add(eProperty.Skill_Two_Handed, "Sundering");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Body, "Soul Crusher");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Critical_Strike, "Lifetaker");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Cross_Bows, "Truefire");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Crushing, "Battering");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Death_Servant, "Death Binder");
			hPropertyToMagicPrefix.Add(eProperty.Skill_DeathSight, "Minionbound");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Dual_Wield, "Whirling");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Earth, "Earthborn");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Enhancement, "Fervent");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Envenom, "Venomous");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Fire, "Flameborn");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Flexible_Weapon, "Tensile");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Cold, "Iceborn");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Instruments, "Melodic");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Long_bows, "Winged");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Matter, "Earthsplitter");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Mind, "Dominating");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Pain_working, "Painbound");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Parry, "Bladeblocker");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Polearms, "Decimator");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Rejuvenation, "Rejuvenating");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Shields, "Protector's");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Slashing, "Honed");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Smiting, "Earthshaker");
			hPropertyToMagicPrefix.Add(eProperty.Skill_SoulRending, "Soul Taker");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Spirit, "Spiritbound");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Staff, "Thunderer");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Stealth, "Shadowwalker");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Thrusting, "Perforator");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Wind, "Airy");


			hPropertyToMagicPrefix.Add(eProperty.AllMagicSkills, "Mystical");
			hPropertyToMagicPrefix.Add(eProperty.AllMeleeWeaponSkills, "Gladiator");
			hPropertyToMagicPrefix.Add(eProperty.AllSkills, "Skillful");
			hPropertyToMagicPrefix.Add(eProperty.AllDualWieldingSkills, "Duelist");
			hPropertyToMagicPrefix.Add(eProperty.AllArcherySkills, "Bowmaster");


			hPropertyToMagicPrefix.Add(eProperty.Skill_Sword, "Serrated");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Hammer, "Demolishing");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Axe, "Swathe Cutter's");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Left_Axe, "Cleaving");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Spear, "Impaling");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Mending, "Bodymender");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Augmentation, "Empowering");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Darkness, "Shadowbender");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Suppression, "Spiritbinder");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Runecarving, "Runebender");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Stormcalling, "Stormcaller");
			hPropertyToMagicPrefix.Add(eProperty.Skill_BeastCraft, "Lifebender");

			hPropertyToMagicPrefix.Add(eProperty.Skill_Light, "Lightbender");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Void, "Voidbender");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Mana, "Starbinder");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Enchantments, "Chanter");

			hPropertyToMagicPrefix.Add(eProperty.Skill_Blades, "Razored");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Blunt, "Crushing");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Piercing, "Lancenator");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Large_Weapon, "Sundering");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Mentalism, "Mindbinder");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Regrowth, "Forestbound");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Nurture, "Plantbound");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Nature, "Animalbound");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Music, "Resonant");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Celtic_Dual, "Whirling");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Celtic_Spear, "Impaling");
			hPropertyToMagicPrefix.Add(eProperty.Skill_RecurvedBow, "Hawk");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Valor, "Courageous");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Subterranean, "Ancestral");
			hPropertyToMagicPrefix.Add(eProperty.Skill_BoneArmy, "Blighted");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Verdant, "Vale Defender");

			hPropertyToMagicPrefix.Add(eProperty.Skill_Battlesongs, "Motivating");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Composite, "Dragon");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Creeping, "Withering");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Arboreal, "Arbor Defender");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Scythe, "Reaper's");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Thrown_Weapons, "Catapult");
			hPropertyToMagicPrefix.Add(eProperty.Skill_HandToHand, "Martial");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Pacification, "Pacifying");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Savagery, "Savage");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Nightshade, "Nightshade");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Pathfinding, "Trail");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Summoning, "Soulbinder");

			hPropertyToMagicPrefix.Add(eProperty.Skill_Dementia, "Feverish");
			hPropertyToMagicPrefix.Add(eProperty.Skill_ShadowMastery, "Ominous");
			hPropertyToMagicPrefix.Add(eProperty.Skill_VampiiricEmbrace, "Deathly");
			hPropertyToMagicPrefix.Add(eProperty.Skill_EtherealShriek, "Shrill");
			hPropertyToMagicPrefix.Add(eProperty.Skill_PhantasmalWail, "Keening");
			hPropertyToMagicPrefix.Add(eProperty.Skill_SpectralForce, "Uncanny");
			hPropertyToMagicPrefix.Add(eProperty.Skill_OdinsWill, "Ardent");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Cursing, "Infernal");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Hexing, "Bedeviled");
			hPropertyToMagicPrefix.Add(eProperty.Skill_Witchcraft, "Diabolic");

			// Mauler - live mauler prefixes do not exist, as lame as that sounds.
			hPropertyToMagicPrefix.Add(eProperty.Skill_Aura_Manipulation, string.Empty);
			hPropertyToMagicPrefix.Add(eProperty.Skill_FistWraps, string.Empty);
			hPropertyToMagicPrefix.Add(eProperty.Skill_MaulerStaff, string.Empty);
			hPropertyToMagicPrefix.Add(eProperty.Skill_Magnetism, string.Empty);
			hPropertyToMagicPrefix.Add(eProperty.Skill_Power_Strikes, string.Empty);
		}

		
	}
}
