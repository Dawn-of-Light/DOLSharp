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
using System.Collections;
using System.Text;

namespace DOL.GS.Database
{
    public class DBMobSpawner
    {
        private int m_mobSpawnerID;
        private int m_spawnAreaId;
        private string m_mobSpawner;
        private string m_mobSpawnerParams;

        public int MobSpawnerID
        {
            get
            {
                return m_mobSpawnerID;
            }
            set
            {
                m_mobSpawnerID = value;
            }
        }

        public int SpawnAreaID
        {
            get
            {
                return m_spawnAreaId;
            }
            set
            {
                m_spawnAreaId = value;
            }
        }
        /// <summary>
        ///  The spawner class, 
        ///  eg. "DOL.GS.SpawnGenerators.StandardMobSpawner"
        /// </summary>
        public string MobSpawnerType
        {
            get
            {
                return m_mobSpawner;
            }
            set
            {
                m_mobSpawner = value;
            }
        }

        /// <summary>
        /// A string containing parameters for the generator, 
        /// eg. "template=piglet,levelrange=1-3,min=10,max=20,time=0-24,respawntime=0.5,brain,chance=3"
        /// </summary>
        public string MobSpawnerParams
        {
            get
            {
                return m_mobSpawnerParams;
            }
            set
            {
                m_mobSpawnerParams = value;
            }
        }
    }
}
