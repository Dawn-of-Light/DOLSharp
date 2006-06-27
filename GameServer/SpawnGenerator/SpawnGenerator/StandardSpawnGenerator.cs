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
using System.Reflection;
using log4net;
using DOL.GS;
using DOL.Events;

namespace DOL.GS.SpawnGenerators
{
	public class StandardSpawnGenerator : ISpawnGenerator
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private ISpawnArea m_spawnArea;
        private IList m_mobSpawners = new ArrayList(1);

		#region params
		public void ParseParams(string strparams)
		{
			string[] strparam = strparams.Split(',');
			foreach (string str in strparam)
			{
				if (str == null ||str == "")
					continue;
                //todo parse when needed
			}
		}
		#endregion

		public StandardSpawnGenerator()
		{
		}

		public void Init()
		{
            foreach (IMobSpawner temp in m_mobSpawners)
			{
				temp.Init();
                temp.SpawnGenerator = this;
			}
		}

		public ISpawnArea Area
		{
			get{return m_spawnArea;}
			set{m_spawnArea = value;}
		}

        public IList MobSpawners
        {
            get { return m_mobSpawners; }
            set { m_mobSpawners = value; }
        }
    }
}
