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
using System.Collections;
using log4net;

namespace DOL.GS.Database
{
    /// <summary>
    /// Race of kind of living (player,mob,npc,...)
    /// </summary>
    class DBRace
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private int m_raceID;
        private string m_name;
        private int m_Resist_Crush;
        private int m_Resist_Slash;
        private int m_Resist_Thrust;
        private int m_Resist_Body;
        private int m_Resist_Cold;
        private int m_Resist_Energy;
        private int m_Resist_Heat;
        private int m_Resist_Matter;
        private int m_Resist_Spirit;

        public int RaceID
        {
            get { return m_raceID;}
            set { m_raceID = value; }
        }

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        public int ResistCrush
        {
            get { return m_Resist_Crush; }
            set { m_Resist_Crush = value; }
        }

        public int ResistSlash
        {
            get { return m_Resist_Slash; }
            set { m_Resist_Slash = value; }
        }

        public int ResistThrust
        {
            get { return m_Resist_Thrust; }
            set { m_Resist_Thrust = value; }
        }

        public int ResistBody
        {
            get { return m_Resist_Body; }
            set { m_Resist_Body = value; }
        }

        public int ResistCold
        {
            get { return m_Resist_Cold; }
            set { m_Resist_Cold = value; }
        }

        public int ResistEnergy
        {
            get { return m_Resist_Energy; }
            set { m_Resist_Energy = value; }
        }

        public int ResistHeat
        {
            get { return m_Resist_Heat; }
            set { m_Resist_Heat = value; }
        }

        public int ResistMatter
        {
            get { return m_Resist_Matter; }
            set { m_Resist_Matter = value; }
        }

        public int ResistSpirit
        {
            get { return m_Resist_Spirit; }
            set { m_Resist_Spirit = value; }
        }
    }
}
