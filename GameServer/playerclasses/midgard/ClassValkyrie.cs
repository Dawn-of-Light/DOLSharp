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

namespace DOL.GS.PlayerClass
{
	/// <summary>
	///
	/// </summary>
	[PlayerClassAttribute((int)eCharacterClass.Valkyrie, "Valkyrie", "Viking")]
	public class ClassValkyrie : ClassViking
	{

		public ClassValkyrie()
			: base()
		{
			m_profession = "House of Odin";
			m_specializationMultiplier = 20;
			m_primaryStat = eStat.CON;
			m_secondaryStat = eStat.STR;
			m_tertiaryStat = eStat.DEX;
			m_manaStat = eStat.PIE;
			m_wsbase = 360;
			m_baseHP = 720;
		}

		public override string GetTitle(int level)
		{
			if (level >= 50) return "Odin's Valkyrie";
			if (level >= 45) return "TODO";
			if (level >= 40) return "TODO";
			if (level >= 35) return "Protector of Valhalla ";
			if (level >= 30) return "TODO";
			if (level >= 25) return "Shieldmaiden";
			if (level >= 20) return "TODO";
			if (level >= 15) return "Servant of Midgard ";
			if (level >= 10) return "Handmaiden ";
			if (level >= 5) return "Apprentice Handmaiden";
			return "None";
		}

		public override eClassType ClassType
		{
			get { return eClassType.Hybrid; }
		}

		public override void OnLevelUp(GamePlayer player)
		{

			base.OnLevelUp(player);

			if (player.Level >= 5)
			{
				player.RemoveSpecialization(Specs.Axe);
				player.RemoveSpecialization(Specs.Hammer);
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Shields));
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Spear));
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Sword));
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Mending));
				player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Swords));
				player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Spears));
				player.AddAbility(SkillBase.GetAbility(Abilities.Guard, 1));
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.OdinsWill));
				player.AddSpellLine(SkillBase.GetSpellLine("Odin's Will"));
				player.AddSpellLine(SkillBase.GetSpellLine("Mending"));
				player.AddSpellLine(SkillBase.GetSpellLine("Valkyrie Mending Spec"));
			}
			if (player.Level >= 7)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Engage));
			}
			if (player.Level >= 10)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Shield, ShieldLevel.Large));
				player.AddAbility(SkillBase.GetAbility(Abilities.Guard, 2));
			}
			if (player.Level >= 12)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.MidArmor, ArmorLevel.Chain));
			}
			if (player.Level >= 15)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Guard, 3));
				player.AddAbility(SkillBase.GetAbility(Abilities.Tireless));
			}
		}
	}
}
