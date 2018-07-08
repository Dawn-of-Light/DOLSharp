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

namespace DOL.GS.Spells
{
    /// <summary>
    ///  Static Extension Class For Implementing Helper Targeted at Stat Changing Spells.
    /// </summary>
    public static class StatsSpellHelper
    {
        /// <summary>
        /// Send Update According Living Stats after Change made by SpellHandler
        /// If Living is Player send Update Packets
        /// Check for Health/Endurence/Power Regen
        /// </summary>
        /// <param name="target">Living needing Updates</param>
        public static void SendLivingStatsAndRegenUpdate(this GameLiving target)
        {
            if (target is GamePlayer player)
            {
                player.Out.SendCharStatsUpdate();
                player.Out.SendUpdateWeaponAndArmorStats();
                player.UpdateEncumberance();
                player.UpdatePlayerStatus();
            }

            if (target.IsAlive)
            {
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
}
