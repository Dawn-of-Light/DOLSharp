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

using DOL.GS;

namespace DOL.AI.Brain
{
    /// <summary>
    /// Dummy Controlled Brain to provide a Following Pet.
    /// </summary>
    public class FollowOwnerBrain : ControlledNpcBrain
    {
        /// <summary>
        /// Passive default
        /// </summary>
        /// <param name="owner"></param>
        public FollowOwnerBrain(GameLiving owner)
            : base(owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }

            m_aggressionState = eAggressionState.Passive;
        }

        /// <summary>
        /// Follow Even if not Main Pet
        /// </summary>
        public override void FollowOwner()
        {
            Body.StopAttack();
            Body.Follow(Owner, MIN_OWNER_FOLLOW_DIST, MAX_OWNER_FOLLOW_DIST);
        }

        /// <summary>
        /// Follow Owner on Think
        /// </summary>
        public override void Think()
        {
            base.Think();
            FollowOwner();
        }
    }
}
