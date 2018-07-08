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
using DOL.Database;

namespace DOL.GS.SkillHandler
{
    /// <summary>
    /// Ability which Level is based on Owner Level instead of Skill Level
    /// Each time Level is modified, it's enforced to User's Level if Applicable
    /// </summary>
    public class LevelBasedStatChangingAbility : StatChangingAbility
    {
        /// <summary>
        /// Override Level Setter/Getter to Report Living Level instead of Skill Level.
        /// </summary>
        public override int Level
        {
            get
            {
                // Report Max Value if no living assigned to trigger the ability override
                if (m_activeLiving != null)
                {
                    return m_activeLiving.Level;
                }

                return int.MaxValue;
            }

            set
            {
                // Override Setter to have Living Level Updated if available.
                base.Level = m_activeLiving?.Level ?? value;
            }
        }

        /// <summary>
        /// Name with stat amount appended
        /// </summary>
        public override string Name {
            get { return m_activeLiving != null ? $"{base.Name} +{GetAmountForLevel(Level)}" : base.Name; }
            set { base.Name = value; }
        }

        /// <summary>
        /// Activate method Enforcing Living Level for Ability Level
        /// </summary>
        /// <param name="living">Living Activating this Ability</param>
        /// <param name="sendUpdates">Update Flag for Packets Sending</param>
        public override void Activate(GameLiving living, bool sendUpdates)
        {
            // Set Base level Before Living is set.
            Level = living.Level;
            base.Activate(living, sendUpdates);
        }

        public LevelBasedStatChangingAbility(DBAbility dba, int level, eProperty property)
            : base(dba, level, property)
        {
        }
    }
}
