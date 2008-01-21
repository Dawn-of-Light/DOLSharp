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
	[PlayerClassAttribute((int)eCharacterClass.Warlock, "Warlock", "Mystic")]
	public class ClassWarlock : ClassMystic
	{
		public ClassWarlock() : base()
		{
			m_profession = "House of Hel";
			m_specializationMultiplier = 10;
			m_primaryStat = eStat.PIE;
			m_secondaryStat = eStat.CON;
			m_tertiaryStat = eStat.DEX;
			m_manaStat = eStat.PIE;
		}

		public override string GetTitle(int level) 
		{
			if (level>=50) return "TODO";
			if (level>=45) return "TODO";
			if (level>=40) return "Hel's Chosen";
			if (level>=35) return "TODO";
			if (level >= 30) return "Warlock Advocate";
			if (level>=25) return "TODO";
			if (level>=20) return "TODO";
			if (level>=15) return "Initiate of Hel";
			if (level>=10) return "Conjurer";
			if (level >= 5) return "Apprentice Conjurer"; 
			return "None"; 
		}

		public override void OnLevelUp(GamePlayer player) 
		{
			base.OnLevelUp(player);

			//Remove
			player.RemoveSpellLine("Darkness");
			player.RemoveSpellLine("Suppression");
			player.RemoveSpecialization(Specs.Darkness);
			player.RemoveSpecialization(Specs.Suppression);
		
			// Specializations
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Cursing));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Hexing));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Witchcraft));
						

			// Spell lines
			player.AddSpellLine(SkillBase.GetSpellLine("Cursing"));
			player.AddSpellLine(SkillBase.GetSpellLine("Cursing Spec"));
			player.AddSpellLine(SkillBase.GetSpellLine("Hexing"));
			player.AddSpellLine(SkillBase.GetSpellLine("Witchcraft"));

			if (player.Level >= 6)
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Tireless));
			}
		}
	}
}
