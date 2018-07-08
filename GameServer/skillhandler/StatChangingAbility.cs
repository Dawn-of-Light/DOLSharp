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
    public class StatChangingAbility : PropertyChangingAbility
    {
        public StatChangingAbility(DBAbility dba, int level, eProperty[] property)
            : base(dba, level, property)
        {
        }

        public StatChangingAbility(DBAbility dba, int level, eProperty property)
            : base(dba, level, property)
        {
        }

        /// <summary>
        /// send updates about the changes
        /// </summary>
        /// <param name="target"></param>
        public override void SendUpdates(GameLiving target)
        {
            if (target is GamePlayer player)
            {
                player.Out.SendCharStatsUpdate();
                player.Out.SendCharResistsUpdate();
                player.Out.SendUpdateWeaponAndArmorStats();
                player.UpdateEncumberance();
                player.UpdatePlayerStatus();
            }

            if (!target.IsAlive)
            {
                return;
            }

            if (target.Health < target.MaxHealth)
            {
                target.StartHealthRegeneration();
            }
            else if (target.Health > target.MaxHealth)
            {
                target.Health = target.MaxHealth;
            }

            if (target.Mana < target.MaxMana)
            {
                target.StartPowerRegeneration();
            }
            else if (target.Mana > target.MaxMana)
            {
                target.Mana = target.MaxMana;
            }

            if (target.Endurance < target.MaxEndurance)
            {
                target.StartEnduranceRegeneration();
            }
            else if (target.Endurance > target.MaxEndurance)
            {
                target.Endurance = target.MaxEndurance;
            }
        }
    }
}
