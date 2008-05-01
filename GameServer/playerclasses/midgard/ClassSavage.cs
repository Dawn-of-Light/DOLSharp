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
	[PlayerClassAttribute((int)eCharacterClass.Savage, "Savage", "Viking")]
	public class ClassSavage : ClassViking
	{

		public ClassSavage() : base()
		{
			m_profession = "House of Kelgor";
			m_specializationMultiplier = 15;
			m_primaryStat = eStat.DEX;
			m_secondaryStat = eStat.QUI;
			m_tertiaryStat = eStat.STR;
			m_wsbase = 400;
		}

		public override string GetTitle(int level) {
			if (level>=50) return "Fist of Kelgor";
			if (level>=45) return "Tribal Legend";
			if (level>=40) return "Protector of Kelgor";
			if (level>=35) return "Tribal Warrior";
			if (level>=30) return "Tribal Wilding";
			if (level>=25) return "Defender of Kelgor";
			if (level>=20) return "Tribal Defender";
			if (level>=15) return "Servant of Kelgor";
			if (level>=10) return "Apprentice of Kelgor";
			if (level>=5) return "Initiate of Kelgor";
			return "None";
		}

		public override bool CanUseLefthandedWeapon(GamePlayer player)
		{
			return true;
		}

		public override void OnLevelUp(GamePlayer player)
		{

			base.OnLevelUp(player);

			if (player.Level >= 5)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_HandToHand));
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.HandToHand));

				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 1));

				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Savagery));
				player.AddSpellLine(SkillBase.GetSpellLine("Savagery"));
			}
			if (player.Level >= 10)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 2));
			}
			if (player.Level >= 15)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 1));
				player.AddAbility(SkillBase.GetAbility(Abilities.Tireless));
			}
			if (player.Level >= 20)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 3));
			}
			if (player.Level >= 23)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 2));
			}
			if (player.Level >= 24)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.PreventFlight));
			}
			if (player.Level >= 30)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Evade, 4));
			}
			if (player.Level >= 32)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Protect, 3));
			}
			if (player.Level >= 35)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Advanced_Evade));
				player.AddAbility(SkillBase.GetAbility(Abilities.Stoicism));
			}
		}

		public override bool HasAdvancedFromBaseClass()
		{
			return true;
		}
	}
}
