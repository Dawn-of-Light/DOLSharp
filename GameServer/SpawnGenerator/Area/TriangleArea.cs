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
using log4net;
namespace DOL.GS.SpawnGenerators
{
    class TriangleArea :  ISpawnArea
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        Point[] cote = new Point[3];
        private Region m_region;
        public TriangleArea(Region region)
        {
            m_region = region;
    	    cote[0] = Point.Zero;
	        cote[1] = Point.Zero;
	        cote[2] = Point.Zero;
        }
        public void ParseParams(string strparams)
        {
            string[] strparam = strparams.Split(',');
            foreach (string str in strparam)
            {
                if (str == null || str == "")
                    continue;
                LoadParam(str);
            }
        }

        private void LoadParam(string str)
        {
            string[] param = str.Split('=');
            switch (param[0])
            {
                case "x1":
                    {
                        try
                        {
                            cote[0].X = Convert.ToInt32(param[1]);
                        }
                        catch
                        {
                            if (log.IsErrorEnabled)
                                log.Error("Could not convert to int param x1.");
                        }
                    } break;
                case "y1":
                    {
                        try
                        {
                            cote[0].Y = Convert.ToInt32(param[1]);
                        }
                        catch
                        {
                            if (log.IsErrorEnabled)
                                log.Error("Could not convert to int param y1.");
                        }
                    } break;
                case "x2":
                    {
                        try
                        {
                            cote[1].X = Convert.ToInt32(param[1]);
                        }
                        catch
                        {
                            if (log.IsErrorEnabled)
                                log.Error("Could not convert to int param x2.");
                        }
                    } break;
                case "y2":
                    {
                        try
                        {
                            cote[1].Y = Convert.ToInt32(param[1]);
                        }
                        catch
                        {
                            if (log.IsErrorEnabled)
                                log.Error("Could not convert to int param y2.");
                        }
                    } break;
             case "x3":
                    {
                        try
                        {
                            cote[2].X = Convert.ToInt32(param[1]);
                        }
                        catch
                        {
                            if (log.IsErrorEnabled)
                                log.Error("Could not convert to int param x3.");
                        }
                    } break;
                case "y3":
                    {
                        try
                        {
                            cote[2].Y = Convert.ToInt32(param[1]);
                        }
                        catch
                        {
                            if (log.IsErrorEnabled)
                                log.Error("Could not convert to int param y3.");
                        }
                    } break;
                default:
                    {
                        if (log.IsErrorEnabled)
                            log.Error("Could not recognize param :" + param[0] + ".");
                    } break;
            }
        }
        public Point GetRandomLocation()
        {
            int a = Util.Random(0, 1);
            int b = Util.Random(0, 1);
            if ( a + b > 1 )
            {
                a = 1 - a;
                b = 1 - b;
            }
            int c = 1 - a - b;
            Point ranpoint = a * cote[0] + b * cote[1] + c * cote[2];

            return ranpoint;
        }

        public Region Region
        {
            get { return m_region; }
        }
    }
}
