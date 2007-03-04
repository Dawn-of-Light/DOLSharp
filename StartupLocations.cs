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
using System.Collections.Specialized;
using System.Reflection;

using DOL.Database;
using DOL.Events;
using DOL.GS.Keeps;

using log4net;

namespace DOL.GS.GameEvents
{
    /// <summary>
    /// Moves new created Characters to the starting location based on region, class and race
    /// </summary>
    public class TrojanStartupLocations
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [ScriptLoadedEvent]
        public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
        {
            GameEventMgr.AddHandler(DatabaseEvent.CharacterCreated, new DOLEventHandler(CharacterCreation));
            if(log.IsInfoEnabled)
                log.Info("Trojan StartupLocations initialized");
        }


        [ScriptUnloadedEvent]
        public static void OnScriptUnloaded(DOLEvent e, object sender, EventArgs args)
        {
            GameEventMgr.RemoveHandler(DatabaseEvent.CharacterCreated, new DOLEventHandler(CharacterCreation));
        }

        /// <summary>
        /// Change location on character creation
        /// </summary>
        /// 
        public static void CharacterCreation(DOLEvent ev, object sender, EventArgs args)
        {
            CharacterEventArgs chArgs = args as CharacterEventArgs;
            if(chArgs == null)
                return;
            Character ch = chArgs.Character;
            try
            {
                StartLocation loc = null;
                if(ch.Realm == 1)
                {
					AbstractGameKeep keep = KeepMgr.getKeepByID(385);
					ch.Region = keep.Region;
					ch.BindRegion = ch.Region;
					loc = new StartLocation(keep.X, keep.Y, keep.Z, keep.Heading);
					ch.BindHeading = loc.Heading;
					ch.BindXpos = loc.X;
					ch.BindYpos = loc.Y;
					ch.BindZpos = loc.Z;
                }
                else if(ch.Realm == 2)
                {
					AbstractGameKeep keep = KeepMgr.getKeepByID(641);
					ch.Region = keep.Region;
					ch.BindRegion = ch.Region;
					loc = new StartLocation(keep.X, keep.Y, keep.Z, keep.Heading);
					ch.BindHeading = loc.Heading;
					ch.BindXpos = loc.X;
					ch.BindYpos = loc.Y;
					ch.BindZpos = loc.Z;
                }
                else if(ch.Realm == 3)
                {
					AbstractGameKeep keep = KeepMgr.getKeepByID(897);
					ch.Region = keep.Region;
					ch.BindRegion = ch.Region;
					loc = new StartLocation(keep.X, keep.Y, keep.Z, keep.Heading);
					ch.BindHeading = loc.Heading;
					ch.BindXpos = loc.X;
					ch.BindYpos = loc.Y;
					ch.BindZpos = loc.Z;

                }
                ch.Xpos = loc.X;
                ch.Ypos = loc.Y;
                ch.Zpos = loc.Z;

                BindCharacter(ch);
            }
            catch(Exception e)
            {
                if(log.IsErrorEnabled)
                    log.Error("StartupLocations script: error changing location. account=" + ch.AccountName + "; char name=" + ch.Name + "; region=" + ch.Region + "; realm=" + ch.Realm + "; class=" + ch.Class + " (" + (eCharacterClass)ch.Class + "); race=" + ch.Race + " (" + (eRace)ch.Race + ")", e);
            }
        }

        /// <summary>
        /// Binds character to current location
        /// </summary>
        /// <param name="ch"></param>
        protected static void BindCharacter(Character ch)
        {
            ch.BindRegion = ch.Region;
            ch.BindHeading = ch.Direction;
            ch.BindXpos = ch.Xpos;
            ch.BindYpos = ch.Ypos;
            ch.BindZpos = ch.Zpos;
        }

        /// <summary>
        /// Converts direction given by /loc command to heading
        /// </summary>
        /// <param name="dir">/loc command direction</param>
        /// <returns>heading (0 .. 4095)</returns>
        protected static int LocDirectionToHeading(int dir)
        {
            return (int)((dir + 180) % 360 * 4096.0 / 360.0);
        }

        /// <summary>
        /// Converts zone coordinate to region
        /// </summary>
        /// <param name="zone">zone coordinate</param>
        /// <param name="zoneOff">zone offset in the region</param>
        /// <returns>region coordinate</returns>
        protected static int ZoneToRegion(int zone, int zoneOff)
        {
            return zone + zoneOff * 8192;
        }

        protected class StartLocation
        {
            protected int m_x;
            protected int m_y;
            protected int m_z;
            protected int m_heading;

            public int X
            {
                get { return m_x; }
            }

            public int Y
            {
                get { return m_y; }
            }

            public int Z
            {
                get { return m_z; }
            }

            public int Heading
            {
                get { return m_heading; }
            }

            public StartLocation(int x, int y, int z, int heading)
            {
                m_x = x;
                m_y = y;
                m_z = z;
                m_heading = heading;
            }
        }
    }
}