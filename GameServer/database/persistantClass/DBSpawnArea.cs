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
using DOL.GS.Database;
using System.Collections;

namespace DOL.GS.Database
{
	/// <summary>
	/// Aggro level of faction against character
	/// </summary>
	/// 
	public class DBSpawnArea
	{
        private int m_id;
		private string m_areaType;
		private string m_areaParams;
		private string m_spawnGenerator;
		private string m_spawnGeneratorParams;
        private int m_regionID;

		/// <summary>
		/// Create faction aggro level against character
		/// </summary>
		public DBSpawnArea()
		{
		}

        public int SpawnAreaId
        {
            get
            {
                return m_id;
            }
            set
            {
                m_id = value;
            }
        }
        
		/// <summary>
		/// The type of area this Spawnarea represents. 
		/// Points towards a class file to allow for custom areas. 
		/// Square and Circle are already in
		/// </summary>
		public string  AreaType
		{
			get 
			{
				return m_areaType;
			}
			set	
			{
				m_areaType = value;
			}
		}		

		/// <summary>
		/// The additional data needed to define the area of the given type. 
		/// eg. "x=123,y=123,z=123,region=123,radius=123"
		/// </summary>
		public string AreaParams
		{
			get 
			{
				return m_areaParams;
			}
			set	
			{
				m_areaParams = value;
			}
		}		
		
		/// <summary>
		///  The spawn generator class for this area, 
		///  eg. "DOL.GS.SpawnGenerators.StandardGenerator"
		/// </summary>
		public string SpawnGenerator
		{
			get
			{
				return m_spawnGenerator;
			}
			set	
			{
				m_spawnGenerator = value;
			}
		}	
	
		/// <summary>
		/// A string containing parameters for the generator, 
		/// global param for spawn generator
		/// </summary>
		public string SpawnGeneratorParams
		{
			get 
			{
				return m_spawnGeneratorParams;
			}
			set	
			{
				m_spawnGeneratorParams = value;
			}
		}


        
        /// <summary>
        /// this contain the regionid witch link area to a region.
        /// Usefull to filtre area when not all region are loaded
        /// </summary>
        public int RegionID
        {
            get { return m_regionID; }
            set { m_regionID = value; }
        }
	}
}
