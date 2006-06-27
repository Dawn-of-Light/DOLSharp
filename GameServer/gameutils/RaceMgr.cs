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
using DOL.GS.Database;
using log4net;

namespace DOL.GS
{
    class RaceMgr
    {
        private static IList m_raceList;

        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void Init()
        {
           m_raceList = GameServer.Database.SelectAllObjects(typeof(DBRace));
        }

        public static DBRace GetRace(int raceID)
        { 
            foreach(DBRace race in m_raceList)
            {
                if (race.RaceID == raceID)
                    return race;
            }
            if (log.IsWarnEnabled)
                log.Warn("Race not found :" + raceID);
            return null;
        }
        public static int GetResist(int raceid, eResist resist)
        {
            DBRace race = GetRace(raceid);
            if (race == null)
            {
                return 0;
            }
            switch (resist)
            {
                case eResist.Body :
                    return race.ResistBody;
                    break;
                case eResist.Cold :
                    return race.ResistCold;
                    break;
                case eResist.Crush :
                    return race.ResistCrush;
                    break;
                case eResist.Energy :
                    return race.ResistEnergy;
                    break;
                case eResist.Heat :
                    return race.ResistHeat;
                    break;
                case eResist.Matter :
                    return race.ResistMatter;
                    break;
                case eResist.Slash :
                    return race.ResistSlash;
                    break;
                case eResist.Spirit :
                    return race.ResistSpirit;
                    break;
                 case eResist.Thrust :
                    return race.ResistThrust;
                    break;
                default :
                    return 0;
            }
        }
    }
}
