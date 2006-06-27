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
	public class SquareArea : ISpawnArea
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private Region m_region;
		private int x1;
		private int x2;
		private int y1;
		private int y2;

        public SquareArea(Region region)
        {
            m_region = region;
        }

		public void ParseParams(string strparams)
		{
			string[] strparam = strparams.Split(',');
			foreach (string str in strparam)
			{
				if (str == null ||str == "")
					continue;
				LoadParam(str);
			}
		}
		
		public void LoadParam(string str)
		{
			string[] param = str.Split('=');
			switch (param[0])
			{
				case "x1":
				{
					try
					{
						x1 = Convert.ToInt32(param[1]);
					}
					catch
					{
						if (log.IsErrorEnabled)
							log.Error("Could not convert to int param x1.");
					}
				}break;
				case "x2":
				{
					try
					{
						x2 = Convert.ToInt32(param[1]);
					}
					catch
					{
						if (log.IsErrorEnabled)
							log.Error("Could not convert to int param x2.");
					}
				}break;
				case "y1":
				{
					try
					{
						y1 = Convert.ToInt32(param[1]);
					}
					catch
					{
						if (log.IsErrorEnabled)
							log.Error("Could not convert to int param y1.");
					}
				}break;
				case "y2":
				{
					try
					{
						y2 = Convert.ToInt32(param[1]);
					}
					catch
					{
						if (log.IsErrorEnabled)
							log.Error("Could not convert to int param y2.");
					}
				}break;
				default :
				{
					if (log.IsErrorEnabled)
						log.Error("Could not recognize param :"+param[0]+".");
				}break;
			}
		}

        public Point GetRandomLocation()
		{
 			return new Point(Util.Random(x1,x2),Util.Random(y1,y2),0);
		}


		public Region Region
		{
			get{return m_region;}
		}
	}
}
