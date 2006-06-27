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
    /// <summary>
    /// Description résumée de SquareArea.
    /// </summary>
    public class CircleArea : ISpawnArea
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private Region m_region;
        private int x;
        private int y;
        private int radius;


        public CircleArea(Region region)
        {
            m_region = region;
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

        public void LoadParam(string str)
        {
            string[] param = str.Split('=');
            switch (param[0])
            {
                case "x":
                    {
                        try
                        {
                            x = Convert.ToInt32(param[1]);
                        }
                        catch
                        {
                            if (log.IsErrorEnabled)
                                log.Error("Could not convert to int param x.");
                        }
                    } break;
                case "y":
                    {
                        try
                        {
                            y = Convert.ToInt32(param[1]);
                        }
                        catch
                        {
                            if (log.IsErrorEnabled)
                                log.Error("Could not convert to int param y.");
                        }
                    } break;
                case "radius":
                    {
                        try
                        {
                            radius = Convert.ToInt32(param[1]);
                        }
                        catch
                        {
                            if (log.IsErrorEnabled)
                                log.Error("Could not convert to int param radius.");
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
            int range = Util.Random(0, radius);
            double angle = Util.RandomDouble()* 2 * Math.PI;
            return new Point( x + (int)(range * Math.Cos(angle)), y + (int)(range * Math.Sin(angle)), 0);
        }


        public Region Region
        {
            get { return m_region; }
        }
    }
}
