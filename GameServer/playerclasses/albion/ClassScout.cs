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
using DOL.GS.ServerProperties;

namespace DOL.GS.PlayerClass
{
    /// <summary>
    /// Albion Scout Class
    /// </summary>
    [CharacterClass((int)eCharacterClass.Scout, "Scout", "Rogue")]
    public class ClassScout : ClassAlbionRogue
    {
        private static readonly string[] AutotrainableSkills = { Specs.Archery, Specs.Longbow };

        public ClassScout()
        {
            Profession = "PlayerClass.Profession.DefendersofAlbion";
            SpecPointsMultiplier = 20;
            PrimaryStat = eStat.DEX;
            SecondaryStat = eStat.QUI;
            TertiaryStat = eStat.STR;
            BaseHP = 720;
            ManaStat = eStat.DEX;
        }

        public override eClassType ClassType => eClassType.Hybrid;

        public override IList<string> GetAutotrainableSkills()
        {
            return AutotrainableSkills;
        }

        /// <summary>
        /// Add all spell-lines and other things that are new when this skill is trained
        /// FIXME : This should be in database
        /// </summary>
        /// <param name="player"></param>
        /// <param name="skill"></param>
        public override void OnSkillTrained(GamePlayer player, Specialization skill)
        {
            base.OnSkillTrained(player, skill);

            switch (skill.KeyName)
            {
                case Specs.Longbow:
                    if (Properties.ALLOW_OLD_ARCHERY)
                    {
                        if (skill.Level < 3)
                        {
                            // do nothing
                        }
                        else if (skill.Level < 6)
                        {
                            player.AddAbility(SkillBase.GetAbility(Abilities.Critical_Shot, 1));
                        }
                        else if (skill.Level < 9)
                        {
                            player.AddAbility(SkillBase.GetAbility(Abilities.Critical_Shot, 2));
                        }
                        else if (skill.Level < 12)
                        {
                            player.AddAbility(SkillBase.GetAbility(Abilities.Critical_Shot, 3));
                        }
                        else if (skill.Level < 15)
                        {
                            player.AddAbility(SkillBase.GetAbility(Abilities.Critical_Shot, 4));
                        }
                        else if (skill.Level < 18)
                        {
                            player.AddAbility(SkillBase.GetAbility(Abilities.Critical_Shot, 5));
                        }
                        else if (skill.Level < 21)
                        {
                            player.AddAbility(SkillBase.GetAbility(Abilities.Critical_Shot, 6));
                        }
                        else if (skill.Level < 24)
                        {
                            player.AddAbility(SkillBase.GetAbility(Abilities.Critical_Shot, 7));
                        }
                        else if (skill.Level < 27)
                        {
                            player.AddAbility(SkillBase.GetAbility(Abilities.Critical_Shot, 8));
                        }
                        else if (skill.Level >= 27)
                        {
                            player.AddAbility(SkillBase.GetAbility(Abilities.Critical_Shot, 9));
                        }

                        if (skill.Level >= 45)
                        {
                            player.AddAbility(SkillBase.GetAbility(Abilities.RapidFire, 2));
                        }
                        else if (skill.Level >= 35)
                        {
                            player.AddAbility(SkillBase.GetAbility(Abilities.RapidFire, 1));
                        }

                        if (skill.Level >= 45)
                        {
                            player.AddAbility(SkillBase.GetAbility(Abilities.SureShot));
                        }

                        if (skill.Level >= 50)
                        {
                            player.AddAbility(SkillBase.GetAbility(Abilities.PenetratingArrow, 3));
                        }
                        else if (skill.Level >= 40)
                        {
                            player.AddAbility(SkillBase.GetAbility(Abilities.PenetratingArrow, 2));
                        }
                        else if (skill.Level >= 30)
                        {
                            player.AddAbility(SkillBase.GetAbility(Abilities.PenetratingArrow, 1));
                        }
                    }

                    break;
            }
        }

        public override bool HasAdvancedFromBaseClass()
        {
            return true;
        }
    }
}
