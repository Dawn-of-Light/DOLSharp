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

namespace DOL.GS
{
    /// <summary>
    /// Description of LiveWeaponSpecialization.
    /// </summary>
    public class LiveWeaponSpecialization : Specialization
    {
        public LiveWeaponSpecialization(string keyname, string displayname, ushort icon, int ID)
            : base(keyname, displayname, icon, ID)
        {
        }

        /// <summary>
        /// No Spells for Weapon Specs.
        /// </summary>
        /// <param name="living"></param>
        /// <returns></returns>
        protected override IDictionary<SpellLine, List<Skill>> GetLinesSpellsForLiving(GameLiving living, int level)
        {
            return new Dictionary<SpellLine, List<Skill>>();
        }

        /// <summary>
        /// No Spells for Weapon Specs.
        /// </summary>
        /// <param name="living"></param>
        /// <returns></returns>
        protected override List<SpellLine> GetSpellLinesForLiving(GameLiving living, int level)
        {
            return new List<SpellLine>();
        }
    }
}
