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
using DOL.GS.Realm;
using System.Collections.Generic;

namespace DOL.GS.PlayerClass
{
	[CharacterClass((int)eCharacterClass.MaulerAlb, "Mauler", "Fighter")]
	public class ClassMaulerAlb : ClassFighter
	{
		private static readonly List<PlayerRace> DefaultEligibleRaces = new List<PlayerRace>()
		{
			PlayerRace.Korazh, PlayerRace.Briton, PlayerRace.Inconnu,
		};

		public ClassMaulerAlb()
			: base()
		{
			m_profession = "PlayerClass.Profession.TempleofIronFist";
			m_specializationMultiplier = 15;
			m_baseHP = 600;
			m_primaryStat = eStat.STR;
			m_secondaryStat = eStat.CON;
			m_tertiaryStat = eStat.QUI;
            m_manaStat = eStat.STR;
			m_eligibleRaces = DefaultEligibleRaces;
		}

		public override bool CanUseLefthandedWeapon
		{
			get { return true; }
		}

		public override eClassType ClassType
		{
			get { return eClassType.Hybrid; }
		}

		public override GameTrainer.eChampionTrainerType ChampionTrainerType()
		{
			return GameTrainer.eChampionTrainerType.Fighter;
		}

		public override bool HasAdvancedFromBaseClass()
		{
			return true;
		}
	}
}
