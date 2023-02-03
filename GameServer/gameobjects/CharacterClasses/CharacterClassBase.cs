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
using DOL.AI.Brain;
using DOL.GS.Effects;
using DOL.Events;
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
        public GamePlayer Player => Behavior.Player;
        public DefaultClassBehavior Behavior { get; private set; }

        public string Name
        {
            get
            {
                var femaleName = characterClass.FemaleName;
                var useFemaleName = (Player != null && Player.Gender == eGender.Female && !Util.IsEmpty(femaleName));
                if (useFemaleName) return femaleName;
                else return characterClass.Name;
            }
        }

        public List<PlayerRace> EligibleRaces => characterClass.EligibleRaces.ToList();
        public string FemaleName => characterClass.FemaleName;
        public int BaseHP => characterClass.BaseHP;
        public string BaseName => characterClass.BaseName;
        public string Profession
            => LanguageMgr.TryTranslateOrDefault(Player, characterClass.ProfessionTranslationId, characterClass.ProfessionTranslationId);
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

        public CharacterClassBase()
        {
            characterClass = CharacterClass.Unknown;
            Behavior = DefaultClassBehavior.Create(null, characterClass.ID);
        }

        public static CharacterClassBase GetClass(int classID)
        {
            return new CharacterClassBase()
            {
                characterClass = CharacterClass.GetClass(classID)
            };
        }

        public string GetTitle(GamePlayer player, int level)
            => characterClass.GetTitle(player,level);

        public virtual void Init(GamePlayer player)
        {
            // TODO : Should Throw Exception Here.
            if (Behavior != null && log.IsWarnEnabled)
                log.WarnFormat("Character Class initializing Player when it was already initialized ! Old Player : {0} New Player : {1}", Behavior.Player, player);

            Behavior = DefaultClassBehavior.Create(player, characterClass.ID);
        }

        public override bool Equals(object obj)
        {
            if (obj is CharacterClassBase characterClass)
            {
                return characterClass.characterClass.ID == ID;
            }
            return false;
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}
