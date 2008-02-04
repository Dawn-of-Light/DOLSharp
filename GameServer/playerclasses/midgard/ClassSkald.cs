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
	[PlayerClassAttribute((int)eCharacterClass.Skald, "Skald", "Viking")]
	public class ClassSkald : ClassViking
	{

		public ClassSkald() : base()
		{
			m_profession = "House of Bragi";
			m_specializationMultiplier = 15;
			m_primaryStat = eStat.CHR;
			m_secondaryStat = eStat.STR;
			m_tertiaryStat = eStat.CON;
			m_manaStat = eStat.CHR;
			m_wsbase = 380;
			m_baseHP = 760;
		}

		public override string GetTitle(int level) {
			if (level>=50) return "Hand of Bragi";
			if (level>=45) return "Lord of Eldas";
			if (level>=40) return "Lord of Sagas";
			if (level>=35) return "Master Tale-Spinner";
			if (level>=30) return "Hymn Weaver Adept";
			if (level>=25) return "Saga-Spinner";
			if (level>=20) return "Song Weaver";
			if (level>=15) return "Battle Chanter";
			if (level>=10) return "Chanter";
			if (level>=5) return "Chanter Initiate";
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
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Battlesongs));
				player.AddSpellLine(SkillBase.GetSpellLine("Battlesongs"));
				player.AddAbility(SkillBase.GetAbility(Abilities.Shield, ShieldLevel.Medium));
			}
			if (player.Level >= 12)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 1));
			}
			if (player.Level >= 15)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Tireless));
			}
			if (player.Level >= 19)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 2));
			}
			if (player.Level >= 20)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.MidArmor, ArmorLevel.Chain));
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 1));
			}
		}

		public override bool HasAdvancedFromBaseClass()
		{
			return true;
		}
	}
}
