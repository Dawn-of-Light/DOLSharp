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
using System.Threading;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// The WeatherMgr takes care of rain/snow and other goodies inside
	/// all the regions
	/// </summary>
	public sealed class WeatherMgr
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region Variables
		/// <summary>
		/// The interval in which the weather chance will be tested, in milliseconds
		/// </summary>
		private const int CHECK_INTERVAL = 5 * 60 * 1000;
		/// <summary>
		/// The chance to start the weather.
		/// Will be tested every CHECK_INTERVAL milliseconds
		/// </summary>
		private const int CHANCE = 5;
		/// <summary>
		/// The width of zone
		/// </summary>
		private const int ZONE_WIDTH = 1048575;//0xFFFFF
		/// <summary>
		/// The starting line of the weather
		/// </summary>
		private uint m_startX;
		/// <summary>
		/// The duration of the weather that will be sent to the client
		/// SH: Actually I think this is the size of the weather 
		/// ... the width in coordinates
		/// </summary>
		private uint m_width;
		/// <summary>
		/// The fog diffusion of this weather
		/// </summary>
		private ushort m_fogDiffusion;
		/// <summary>
		/// The speed of this weather in coordinates/second
		/// </summary>
		private ushort m_speed;
		/// <summary>
		/// The intensity of this weather
		/// </summary>
		private ushort m_intensity;
		/// <summary>
		/// For which region is this weather?
		/// </summary>
		private ushort m_regionID = 0;
		/// <summary>
		/// The gametimer for this weather, which will check the start
		/// </summary>
		private readonly Timer m_weatherTimer;
		/// <summary>
		/// The tickcount when this weather started
		/// </summary>
		private int m_weatherStartTick;
		/// <summary>
		/// Holds all weather managers currently active
		/// </summary>
		private static readonly HybridDictionary m_weathers = new HybridDictionary();
		#endregion

		#region Constructor
		/// <summary>
		/// Constructs a new RegionWeather object for a given region
		/// </summary>
		/// <param name="regionID"></param>
		private WeatherMgr(ushort regionID)
		{
			this.m_regionID = regionID;
			this.m_weatherTimer = new Timer(new TimerCallback(CheckWeatherTimerCallback), null, 10000, CHECK_INTERVAL);
		}
		#endregion

		#region Properties / Helper Methods

		/// <summary>
		/// Gets if the weather manager currently is active
		/// </summary>
		public bool IsActive
		{
			get
			{
				return (CurrentWeatherLine > 0 && CurrentWeatherLine < (uint)0xFFFFF);
			}
		}

		/// <summary>
		/// Gets the weather line based on the time passed since the weather started
		/// </summary>
		public uint CurrentWeatherLine
		{
			get
			{
				if (m_weatherStartTick == 0)
					return 0;
				return (uint)(m_startX + (Environment.TickCount - m_weatherStartTick) * m_speed / 1000);
			}
		}

		public uint StartX
		{
			get { return m_startX; }
			set
			{
				m_startX = value;
				RestartStorm();
			}
		}

		public uint Width
		{
			get { return m_width; }
			set
			{
				m_width = value;
				RestartStorm();
			}
		}

		public ushort FogDiffusion
		{
			get { return m_fogDiffusion; }
			set
			{
				m_fogDiffusion = value;
				RestartStorm();
			}
		}

		public ushort Speed
		{
			get { return m_speed; }
			set
			{
				m_speed = value;
				RestartStorm();
			}
		}

		public ushort Intensity
		{
			get { return m_intensity; }
			set
			{
				m_intensity = value;
				RestartStorm();
			}
		}

		/// <summary>
		/// Gets a specific weather manager for a region
		/// </summary>
		/// <param name="regionID">The region to retrieve the weather manager for</param>
		/// <returns>The retrieved weather manager or null if none for this region</returns>
		public static WeatherMgr GetWeatherForRegion(ushort regionID)
		{
			return (WeatherMgr)m_weathers[regionID];
		}

		/// <summary>
		/// This is the GameTimer callback that will be called to test if to start
		/// a weather or not
		/// </summary>
		/// <param name="state"></param>
		private void CheckWeatherTimerCallback(object state)
		{
			if (IsActive)
				return;

			//Reset the weather start tick so it can not overflow
			m_weatherStartTick = 0;

			//Test our chance to start the weather
			if (!Util.Chance(CHANCE))
				return;

			//Start the weather
			StartStorm();

			m_weatherTimer.Change(CHECK_INTERVAL + ZONE_WIDTH * 1000 / m_speed, CHECK_INTERVAL);
			return; //0xFFFFF/speed*10 to wait time of storm  before check
		}

		/// <summary>
		/// This method forces the weather for this region to start with a random intensity,
		/// no matter if it is already active or not
		/// </summary>
		public void StartStorm()
		{
			StartStorm(1,
					   (uint)Util.Random(25000, 90000),
					   (ushort)Util.Random(100, 700),
					   16000,
					   (ushort)Util.Random(30, 120));
		}

		/// <summary>
		/// Restarts the storm with the current storm parameters
		/// </summary>
		public void RestartStorm()
		{
			StartStorm(m_startX, m_width, m_speed, m_fogDiffusion, m_intensity);
			m_weatherTimer.Change(ZONE_WIDTH * 1000 / m_speed, CHECK_INTERVAL);
		}

		/// <summary>
		/// Starts the storm in the region
		/// </summary>
		/// <param name="x">The starting line of the storm in coordinates</param>
		/// <param name="duration">The cloude width in coordinates</param>
		/// <param name="speed">The speed of the storm in coordinates/second</param>
		/// <param name="intensity">The intensity of the storm</param>
		public void StartStorm(uint x, uint duration, ushort speed, ushort fog, ushort intensity)
		{
			m_startX = x;
			m_width = duration;
			m_speed = speed;
			m_fogDiffusion = fog;
			m_intensity = intensity;
			m_weatherStartTick = Environment.TickCount;

			foreach (GameClient cl in WorldMgr.GetClientsOfRegion(m_regionID))
				cl.Out.SendWeather(m_startX, m_width, m_speed, m_fogDiffusion, m_intensity);
			if (log.IsInfoEnabled)
				log.Info("It starts to rain in " + WorldMgr.GetRegion(m_regionID).Description);
		}


		/// <summary>
		/// This method forces the weather for this region to stop if it is running
		/// </summary>
		public void StopStorm()
		{
			if (!IsActive) return;

			uint currentLine = CurrentWeatherLine;

			//We are stopping
			m_weatherStartTick = 0;

			foreach (GameClient cl in WorldMgr.GetClientsOfRegion(m_regionID))
			{
				cl.Out.SendWeather(0, 0, 0, 0, 0);
				if (cl.Player.X > (currentLine - m_width) && cl.Player.X < (currentLine + m_width))
					cl.Out.SendMessage("The sky clears up again as the storm clouds disperse!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}
			m_weatherTimer.Change(CHECK_INTERVAL, CHECK_INTERVAL);
			if (log.IsInfoEnabled)
				log.Info("Rain was stopped in " + WorldMgr.GetRegion(m_regionID).Description);
		}

		/// <summary>
		/// This method will be called from the PlayerInitRequestHandler
		/// </summary>
		/// <param name="player">The player entering the region</param>
		public static void UpdatePlayerWeather(GamePlayer player)
		{
			WeatherMgr mgr = GetWeatherForRegion(player.CurrentRegionID);
			if (mgr != null && mgr.IsActive)
				player.Out.SendWeather(mgr.CurrentWeatherLine, mgr.m_width, mgr.m_speed, mgr.m_fogDiffusion, mgr.m_intensity);
		}
		#endregion

		#region Load/Unload
		/// <summary>
		/// Initializes all weather manager instances for the regions
		/// </summary>
		/// <returns>true if successfull</returns>
		public static bool Load()
		{
#warning ideally we'd want this read from the region table instead of hardcoding which regions should produce storms
			lock (m_weathers)
			{
				foreach (RegionEntry region in DOL.GS.WorldMgr.GetRegionList())
				{
					switch (region.id)
					{
						case 1://Albion main
						case 2://Albion housing
						case 51://Albion SI
						case 70://Albion TOA
						case 71://Albion TOA
						case 73://Albion TOA
						case 100://Midgard main
						case 102://Midgard housing
						case 151://Midgard SI
						case 30://Midgard TOA
						case 200://Hibernia main
						case 202://Hibernia housing
						case 181://Hibernia SI
						case 72://Hibernia TOA
						case 130://Hibernia TOA
						case 163:// New Frontiers
						case 234://bg1-4
						case 235://bg5-9
						case 236://bg10-14
						case 237://bg15-19
						case 238://bg20-24
						case 239://bg25-29
						case 240://bg30-34
						case 241://bg35-39
						case 242://bg40-44
						case 165://bg45-49
							m_weathers.Add(region.id, new WeatherMgr(region.id));
							break;
					}
				}
				foreach (WeatherMgr weather in m_weathers.Values)
					weather.m_weatherTimer.Change(CHECK_INTERVAL, CHECK_INTERVAL);
			}
			return true;
		}

		/// <summary>
		/// Stops all weather managers
		/// </summary>
		public static void Unload()
		{
			lock (m_weathers)
			{
				foreach (WeatherMgr weather in m_weathers.Values)
					weather.m_weatherTimer.Change(Timeout.Infinite, Timeout.Infinite);
				m_weathers.Clear();
			}
		}
		#endregion
	}
}
