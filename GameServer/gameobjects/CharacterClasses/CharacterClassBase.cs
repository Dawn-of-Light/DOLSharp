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
using System.Collections.Generic;
using DOL.Language;
using DOL.GS.Realm;
using System.Linq;

namespace DOL.GS
{
    public class CharacterClassBase : ICharacterClass
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private CharacterClass characterClass;

        public int ID => characterClass.ID;
        private GamePlayer player;

        public string Name => characterClass.GetSalutation(player);

        public List<PlayerRace> EligibleRaces => characterClass.EligibleRaces.ToList();
        public string FemaleName => characterClass.FemaleName;
        public int BaseHP => characterClass.BaseHP;
        public string BaseName => characterClass.BaseName;
        public string Profession
            => LanguageMgr.TryTranslateOrDefault(player, characterClass.ProfessionTranslationId, characterClass.ProfessionTranslationId);
        public int SpecPointsMultiplier => characterClass.SpecPointsMultiplier;
        /// <summary>
        /// This is specifically used for adjusting spec points as needed for new training window
        /// For standard DOL classes this will simply return the standard spec multiplier
        /// </summary>
        public int AdjustedSpecPointsMultiplier => characterClass.SpecPointsMultiplier;
        public eStat PrimaryStat => characterClass.LeveledStats.Primary;
        public eStat SecondaryStat => characterClass.LeveledStats.Secondary;
        public eStat TertiaryStat => characterClass.LeveledStats.Tertiary;
        public eStat ManaStat => characterClass.ManaStat;
        public int WeaponSkillBase => characterClass.BaseWeaponSkill;
        public ushort MaxPulsingSpells => characterClass.MaxPulsingSpells;
        public virtual eClassType ClassType
            => characterClass.ClassType;
        public IList<string> GetAutotrainableSkills()
            => characterClass.AutoTrainSkills.ToList();
        public virtual GameTrainer.eChampionTrainerType ChampionTrainerType()
            => (GameTrainer.eChampionTrainerType)characterClass.ChampionTrainerID;
        public virtual bool CanUseLefthandedWeapon
            => characterClass.CanUseLeftHandedWeapon;
        public virtual bool HasAdvancedFromBaseClass()
            => characterClass.HasAdvancedFromBaseClass;

        private CharacterClassBase() { }

        public static CharacterClassBase Create(GamePlayer player, int classID)
        {
            var characterClass = new CharacterClassBase();
            characterClass.player = player;
            characterClass.characterClass = CharacterClass.GetClass(classID);
            return characterClass;
        }

        public static CharacterClassBase GetClass(int classID)
            => Create(null, classID);

        public static CharacterClassBase GetBaseClass(int classID)
            => Create(null, CharacterClass.GetClass(classID).GetBaseClass().ID);

        public string GetTitle(GamePlayer player, int level)
            => characterClass.GetTitle(player, level);

        public override bool Equals(object obj)
        {
            if (obj is CharacterClassBase characterClassBase)
            {
                return characterClassBase.characterClass.Equals(characterClass);
            }
            return false;
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}
