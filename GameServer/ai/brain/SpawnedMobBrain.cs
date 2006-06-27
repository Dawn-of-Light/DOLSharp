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
using System.Reflection;
using DOL.Events;
using DOL.GS;
using DOL.GS.PacketHandler;
using log4net;
using DOL.GS.SpawnGenerators;

namespace DOL.AI.Brain
{
    class SpawnedMobBrain : StandardMobBrain
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IMobSpawner m_spawner;

        /// <summary>
        /// Gets or sets the spawner.
        /// </summary>
        /// <value>The spawner.</value>
        public IMobSpawner Spawner
        {
            get { return m_spawner; }
            set { m_spawner = value; }
        }

        /// <summary>
        /// searches for a friend to group for combat
        /// </summary>
        /// <returns></returns>
        protected override GameNPC FindFriendForAttack()
        {
            foreach (GameMob mob in Spawner.Mobs)
            {
                if (mob != Body && !mob.InCombat)//avoid ever taken mob
                {
                    return mob;
                }
            }
            return null;
        }
    }
}
