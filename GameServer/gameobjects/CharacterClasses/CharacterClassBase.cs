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
        private DefaultClassBehavior behavior;

        public int ID => characterClass.ID;
        protected int m_baseWeaponSkillRanged;
        protected ushort m_maxPulsingSpells;
        public GamePlayer Player { get; private set; }

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
        public int WeaponSkillRangedBase => m_baseWeaponSkillRanged;
        public ushort MaxPulsingSpells => m_maxPulsingSpells;
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
            m_baseWeaponSkillRanged = 440;
            m_maxPulsingSpells = 1;
            behavior = DefaultClassBehavior.Create(null,characterClass.ID);
        }

        public static CharacterClassBase GetClass(int classID)
        {
            return new CharacterClassBase()
            {
                characterClass = CharacterClass.GetClass(classID)
            };
        }

        public string GetTitle(GamePlayer player, int level)
        {
            if (!HasAdvancedFromBaseClass()) level = 0;

            // Clamp level in 5 by 5 steps - 50 is the max available translation for now
            int clamplevel = Math.Min(50, (level / 5) * 5);

            string none = LanguageMgr.TryTranslateOrDefault(player, "!None!", "PlayerClass.GetTitle.none");

            if (clamplevel > 0)
                return LanguageMgr.TryTranslateOrDefault(player, string.Format("!{0}!", characterClass.Name), string.Format("PlayerClass.{0}.GetTitle.{1}", characterClass.Name, clamplevel));

            return none;
        }

        public virtual void Init(GamePlayer player)
        {
            // TODO : Should Throw Exception Here.
            if (behavior != null && log.IsWarnEnabled)
                log.WarnFormat("Character Class initializing Player when it was already initialized ! Old Player : {0} New Player : {1}", behavior.Player, player);

            behavior = DefaultClassBehavior.Create(player, characterClass.ID);
        }

        public virtual void OnLevelUp(GamePlayer player, int previousLevel)
            => behavior.OnLevelUp(player, previousLevel);

        public virtual void OnRealmLevelUp(GamePlayer player)
            => behavior.OnRealmLevelUp(player);

        public virtual void OnSkillTrained(GamePlayer player, Specialization skill)
            => behavior.OnSkillTrained(player, skill);

        public virtual void SetControlledBrain(IControlledBrain controlledBrain)
            => behavior.SetControlledBrain(controlledBrain);

        public virtual void CommandNpcRelease()
            => behavior.CommandNpcRelease();

        public virtual void OnPetReleased()
            => behavior.OnPetReleased();

        public virtual bool StartAttack(GameObject attackTarget)
            => behavior.StartAttack(attackTarget);

        public virtual byte HealthPercentGroupWindow
            => behavior.HealthPercentGroupWindow;

        public virtual ShadeEffect CreateShadeEffect()
            => behavior.CreateShadeEffect();

        public virtual void Shade(bool makeShade)
            => behavior.Shade(makeShade);

        public virtual bool RemoveFromWorld()
            => behavior.RemoveFromWorld();

        public virtual void Die(GameObject killer)
            => behavior.Die(killer);

        public virtual void Notify(DOLEvent e, object sender, EventArgs args)
            => behavior.Notify(e, sender, args);

        public virtual bool CanChangeCastingSpeed(SpellLine line, Spell spell)
            => behavior.CanChangeCastingSpeed(line, spell);

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
