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

namespace DOL.GS.Scripts
{
	/// <summary>
	/// 
	/// </summary>
	[PlayerClassAttribute((int)eCharacterClass.Bonedancer, "Bonedancer", "Mystic")]
	public class ClassBonedancer : ClassMystic
	{
		public ClassBonedancer() : base()
		{
			m_profession = "House of Bodgar";
			m_specializationMultiplier = 10;
			m_primaryStat = eStat.PIE;
			m_secondaryStat = eStat.DEX;
			m_tertiaryStat = eStat.QUI;
			m_manaStat = eStat.PIE;
		}

		public override string GetTitle(int level) 
		{
			if (level>=50) return "Tribal Chieftan";
			if (level>=45) return "Fervent of Bodgar";
			if (level>=40) return "Bonemage";
			if (level>=35) return "Tribal Elder";
			if (level>=30) return "Servant of Bodgar";
			if (level>=25) return "Bonesearcher";
			if (level>=20) return "Apprentice of Bodgar";
			if (level>=15) return "Tribal Mystic";
			if (level>=10) return "Bonegatherer"; 
			if (level>=5) return "Initiate of Bodgar"; 
			return "None"; 
		}

		public override void OnLevelUp(GamePlayer player) 
		{
			
			base.OnLevelUp(player);

		
			// Specializations
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.BoneArmy));			

			// Spell lines
			player.AddSpellLine(SkillBase.GetSpellLine("Bone Army"));
			player.AddSpellLine(SkillBase.GetSpellLine("Bone Mystics"));
			player.AddSpellLine(SkillBase.GetSpellLine("Bone Guardians"));
			player.AddSpellLine(SkillBase.GetSpellLine("Bone Warriors"));

			if(player.Level >= 5)
			{
				// Abilities
				player.AddAbility(SkillBase.GetAbility(Abilities.Quickcast));
			}
		}
	}
}
