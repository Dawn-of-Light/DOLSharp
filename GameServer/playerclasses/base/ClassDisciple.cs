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
using System.Collections.Generic;
using DOL.GS.Realm;

namespace DOL.GS.PlayerClass
{
	[CharacterClass((int)eCharacterClass.Disciple, "Disciple", "Disciple")]
	public class ClassDisciple : CharacterClassBase
	{
		private static readonly List<PlayerRace> DefaultEligibleRaces = new()
		{
			 PlayerRace.Briton, PlayerRace.Inconnu, PlayerRace.Saracen,
		};

		public ClassDisciple()
			: base()
		{
			m_baseWeaponSkill = 280;
			m_baseHP = 560;
			m_manaStat = eStat.INT;
			m_eligibleRaces = DefaultEligibleRaces;
		}

		public override string GetTitle(GamePlayer player, int level)
		{
			return HasAdvancedFromBaseClass() ? base.GetTitle(player, level) : base.GetTitle(player, 0);
		}

		public override eClassType ClassType
		{
			get { return eClassType.ListCaster; }
		}

		public override GameTrainer.eChampionTrainerType ChampionTrainerType()
		{
			return GameTrainer.eChampionTrainerType.Disciple;
		}

		public override bool HasAdvancedFromBaseClass()
		{
			return false;
		}		
	}
}
