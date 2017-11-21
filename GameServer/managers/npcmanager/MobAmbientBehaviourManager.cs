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
using System.Linq;
using System.Collections.Generic;
using DOL.Database;

namespace DOL.GS
{
    /// <summary>
    /// MobAmbientBehaviourManager handles Mob Ambient Behaviour Lazy Loading
    /// </summary>
    public sealed class MobAmbientBehaviourManager
    {
        /// <summary>
        /// Mob X Ambient Behaviour Cache indexed by Mob Name
        /// </summary>
        private List<MobXAmbientBehaviour> AmbientBehaviour { get; }

        /// <summary>
        /// Retrieve MobXambiemtBehaviour Objects from Mob Name
        /// </summary>
        public MobXAmbientBehaviour[] this[string index]
        {
            get
            {
                if (string.IsNullOrEmpty(index))
                {
                    return new MobXAmbientBehaviour[0];
                }

                return AmbientBehaviour
                    .Where(x => x.Source.Equals(index, StringComparison.InvariantCultureIgnoreCase))
                    .ToArray();
            }
        }

        /// <summary>
        /// Create a new Instance of <see cref="MobAmbientBehaviourManager"/>
        /// </summary>
        public MobAmbientBehaviourManager(IObjectDatabase database)
        {
            if (database == null)
            {
                throw new ArgumentNullException(nameof(database));
            }

            AmbientBehaviour = database.SelectAllObjects<MobXAmbientBehaviour>().ToList();
        }
    }
}
