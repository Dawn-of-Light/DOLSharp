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
	[PlayerClassAttribute((int)eCharacterClass.Bainshee, "Bainshee", "Magician")]
	public class ClassBainshee : ClassMagician
	{
		public ClassBainshee() : base() 
		{
			m_profession = "Path of Affinity";
			m_specializationMultiplier = 10;
			m_primaryStat = eStat.INT;
			m_secondaryStat = eStat.DEX;
			m_tertiaryStat = eStat.CON;
			m_manaStat = eStat.INT;
		}

		public override string GetTitle(int level) 
		{
			if (level>=50) return "Spectral Warrior";
			if (level>=45) return "Spectral Guardian";
			if (level>=40) return "Revenant Prowler";
			if (level>=35) return "TODO";
			if (level>=30) return "Ethereal Shrieker";
			if (level>=25) return "Ethereal Wailer";
			if (level>=20) return "Phantom Reaper";
			if (level>=15) return "Phantom Adept";
			if (level>=10) return "Wraith Apprentice"; 
			if (level>=5) return "Wraith Initiate"; 
			return "None"; 
		}

		/// <summary>
		/// Update all skills and add new for current level
		/// </summary>
		/// <param name="player"></param>
		public override void OnLevelUp(GamePlayer player) 
		{
			base.OnLevelUp(player);
			// Specializations
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.EtherealShriek));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.PhantasmalWail));
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.SpectralForce));
			
			// Spell lines
			player.AddSpellLine(SkillBase.GetSpellLine(Specs.EtherealShriek));
			player.AddSpellLine(SkillBase.GetSpellLine(Specs.PhantasmalWail));
			player.AddSpellLine(SkillBase.GetSpellLine(Specs.SpectralForce));
			player.RemoveSpellLine(SkillBase.GetSpellLine("Way of the Moon"));
			player.RemoveSpellLine(SkillBase.GetSpellLine("Way of the Sun"));
            player.RemoveSpecialization(Specs.Light);
            player.RemoveSpecialization(Specs.Mana);

			if (player.Level >= 5) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Quickcast));
			}
			if (player.Level >= 15) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Tireless));
			}
		}
	}
}
