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
    class WanderSpawnedMobBrain : SpawnedMobBrain
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="T:WanderSpawnBrain"/> class.
        /// </summary>
        public WanderSpawnedMobBrain()
            : base()
		{
			
		}
        /// <summary>
        /// Do the mob AI
        /// </summary>
        public override void Think()
        {
            if (!Body.InCombat && !this.Body.IsMoving && m_aggroTable.Count == 0)
                Wander();
            base.Think();
        }

        /// <summary>
        /// Wanders this mob.
        /// </summary>
        public void Wander()
        {
            Point mylocation = this.Spawner.SpawnGenerator.Area.GetRandomLocation();
            Body.WalkTo(mylocation, Body.MaxSpeedBase / 2);//walk slowly
        }
    }
}
