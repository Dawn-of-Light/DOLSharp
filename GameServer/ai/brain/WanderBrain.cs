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
    class WanderBrain : StandardMobBrain
    {
        private int m_radius = 750; //default value

        /// <summary>
        /// Initializes a new instance of the <see cref="T:WanderSpawnBrain"/> class.
        /// </summary>
        public WanderBrain()
            : base()
        {

        }
        /// <summary>
        /// Do the mob AI
        /// </summary>
        public override void Think()
        {
            if(!Body.InCombat && !this.Body.IsMoving && m_aggroTable.Count == 0 )
                Wander();
            base.Think();
        }

        /// <summary>
        /// Wander this mob.
        /// </summary>
        public void Wander()
        {
            int range = Util.Random(0, m_radius);
            double angle = Util.RandomDouble() * 2 * Math.PI;
            //0 on Z to avoid issue with Z position when move best is to have something to get Z from X and Y
            Body.WalkTo(new Point(Body.SpawnPosition.X + (int)(range * Math.Cos(angle)), Body.SpawnPosition.Y + (int)(range * Math.Sin(angle)), 0), Body.MaxSpeedBase / 2);
        }
    }
}
