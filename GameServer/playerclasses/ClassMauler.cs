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

namespace DOL.GS.Scripts
{
	/// <summary>
	/// 
	/// </summary>
	[PlayerClassAttribute((int)eCharacterClass.Mauler_Alb, "Mauler", "Fighter")]
	[PlayerClassAttribute((int)eCharacterClass.Mauler_Mid, "Mauler", "Viking")]
	[PlayerClassAttribute((int)eCharacterClass.Mauler_Hib, "Mauler", "Guardian")]
	public class ClassMauler : CharacterClassSpec
	{
		public ClassMauler()
			: base()
		{
			m_profession = "Temple of the Iron Fist";
			m_specializationMultiplier = 15;
			m_wsbase = 440;
			m_baseHP = 600;
			m_primaryStat = eStat.STR;
			m_secondaryStat = eStat.CON;
			m_tertiaryStat = eStat.QUI;
		}

		public override bool CanUseLefthandedWeapon(GamePlayer player)
		{
			return true;
		}

		public override eClassType ClassType
		{
			get { return eClassType.PureTank; }
		}

		public override string GetTitle(int level)
		{
			if (level >= 50) return "Master Mauler";
			if (level >= 45) return "Scrapper";
			if (level >= 40) return "Iron Fist";
			if (level >= 35) return "Brawler";
			if (level >= 30) return "Ten Hands";
			if (level >= 25) return "Aggressor";
			if (level >= 20) return "Pugilist";
			if (level >= 15) return "Five Paws";
			if (level >= 10) return "Stone Palm";
			if (level >= 5) return "Initiate Mauler";
			return "None";
		}

		public override IList AutoTrainableSkills()
		{
			ArrayList skills = new ArrayList();
			return skills;
		}

		/// <summary>
		/// Update all skills and add new for current level
		/// </summary>
		/// <param name="player"></param>
		public override void OnLevelUp(GamePlayer player)
		{
			base.OnLevelUp(player);

			if (player.Level >= 5)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Sprint));
				player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_MaulerStaff));
				player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_FistWraps));
                switch (player.Realm)
                {
                    case 1:
                        {
                            player.RemoveSpecialization(Specs.Slash);
                            player.RemoveSpecialization(Specs.Thrust);
                            player.RemoveSpecialization(Specs.Parry);
                            player.RemoveSpecialization(Specs.Crush);
                            player.RemoveAllStyles();
                            player.RemoveAbility(Abilities.AlbArmor);
                            player.AddAbility(SkillBase.GetAbility(Abilities.AlbArmor, ArmorLevel.Leather));
                            break;
                        }
                    case 2:
                        {
                            player.RemoveSpecialization(Specs.Sword);
                            player.RemoveSpecialization(Specs.Hammer);
                            player.RemoveSpecialization(Specs.Axe);
                            player.RemoveSpecialization(Specs.Parry);
                            player.RemoveAllStyles();
                            player.RemoveAbility(Abilities.MidArmor);
                            player.AddAbility(SkillBase.GetAbility(Abilities.MidArmor, ArmorLevel.Leather));
                            break;
                        }
                    case 3:
                        {
                            player.RemoveSpecialization(Specs.Blades);
                            player.RemoveSpecialization(Specs.Piercing);
                            player.RemoveSpecialization(Specs.Parry);
                            player.RemoveSpecialization(Specs.Blunt);
                            player.RemoveAllStyles();
                            player.RemoveAbility(Abilities.HibArmor);
                            player.AddAbility(SkillBase.GetAbility(Abilities.HibArmor, ArmorLevel.Leather));
                            break;
                        }
                }

				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 1));

				player.AddSpellLine(SkillBase.GetSpellLine("Aura Manipulation"));
				player.AddSpellLine(SkillBase.GetSpellLine("Magnetism"));
				player.AddSpellLine(SkillBase.GetSpellLine("Power Strikes"));

				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Aura_Manipulation));
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Magnetism));
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Power_Strikes));
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Mauler_Staff));
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Fist_Wraps));
			}
			if (player.Level >= 7)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 1));
			}
			if (player.Level >= 13)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 2));
			}
			if (player.Level >= 15)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Tireless));
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 2));
			}
			if (player.Level >= 18)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 3));
			}
		}
	}
}
