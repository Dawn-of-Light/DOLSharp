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
using System.Linq;
using System.Collections.Generic;
using System.Timers;

using DOL.Events;
using DOL.GS.PacketHandler;

using log4net;

namespace DOL.GS
{
	/// <summary>
	/// WeatherManager class handle current weather in compatible Regions.
	/// </summary>
	public sealed class WeatherManager
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Sync Lock Object
		/// </summary>
		private object LockObject = new object();
		
		/// <summary>
		/// Default Weather Check Timer Interval
		/// </summary>
		private int DefaultTimerInterval { get { return Math.Max(1000, ServerProperties.Properties.WEATHER_CHECK_INTERVAL); } }
		
		/// <summary>
		/// Default Weather Chance
		/// </summary>
		private int DefaultWeatherChance { get { return Math.Min(99, ServerProperties.Properties.WEATHER_CHANCE); } }
		
		/// <summary>
		/// Log Weather Change to Info Logger
		/// </summary>
		private bool EventLogWeather { get { return ServerProperties.Properties.WEATHER_LOG_EVENTS; } }
		
		/// <summary>
		/// Dictionary of Regions to be handled.
		/// </summary>
		private Dictionary<ushort, RegionWeather> RegionsWeather { get; set; }
		
		/// <summary>
		/// Timer for triggering Weather Event
		/// </summary>
		private Timer WeatherTimer { get; set; }
		
		/// <summary>
		/// Get the Actual Timestamp
		/// </summary>
		private long NowTicks { get { return GameTimer.GetTickCount(); } }
		
		/// <summary>
		/// OnTick Flag prevent running Multiple Tick concurrently
		/// </summary>
		private volatile bool OnTick = false;
		
		/// <summary>
		/// Retrieve Region Weather from Region ID
		/// </summary>
		public RegionWeather this[ushort regionId]
		{
			get
			{
				RegionWeather weather;
				lock (LockObject)
					RegionsWeather.TryGetValue(regionId, out weather);
				
				return weather;
			}
		}
		
		/// <summary>
		/// Create a new Instance of <see cref="WeatherManager"/>
		/// </summary>
		public WeatherManager()
		{
			RegionsWeather = new Dictionary<ushort, RegionWeather>();

			WeatherTimer = new Timer();
			WeatherTimer.AutoReset = false;
			WeatherTimer.Enabled = false;			
			WeatherTimer.Elapsed += OnWeatherTimerTick;
			WeatherTimer.Interval = DefaultTimerInterval;

			GameEventMgr.AddHandler(RegionEvent.RegionStart, OnRegionStart);
			GameEventMgr.AddHandler(RegionEvent.RegionStop, OnRegionStop);
			GameEventMgr.AddHandler(RegionEvent.PlayerEnter, OnPlayerEnter);
		}
		
		/// <summary>
		/// Start a Random Weather for Region
		/// </summary>
		/// <param name="regionId">Region ID where weather must be changed</param>
		/// <returns>True if weather was changed</returns>
		public bool StartWeather(ushort regionId)
		{
			var weather = this[regionId];
			
			if (weather == null)
				return false;
			
			weather.CreateWeather(NowTicks);
			StartWeather(weather);
			return true;
		}
		
		/// <summary>
		/// Start a Parametrized Weather for Region
		/// </summary>
		/// <param name="regionId">Region ID where weather must be changed</param>
		/// <param name="position">Weather Position</param>
		/// <param name="width">Weather Width</param>
		/// <param name="speed">Weather Speed</param>
		/// <param name="diffusion">Weather Diffusion</param>
		/// <param name="intensity">Weather Intensity</param>
		/// <returns>True if weather was changed</returns>
		public bool StartWeather(ushort regionId, uint position, uint width, ushort speed, ushort diffusion, ushort intensity)
		{
			var weather = this[regionId];
			
			if (weather == null)
				return false;
			
			weather.CreateWeather(position, width, speed, diffusion, intensity, NowTicks);
			StartWeather(weather);
			return true;
		}
		
		/// <summary>
		/// Restart Weather for Region
		/// </summary>
		/// <param name="regionId">Region ID where weather must be restarted</param>
		/// <returns>True if weather was restarted</returns>
		public bool RestartWeather(ushort regionId)
		{
			var weather = this[regionId];
			
			if (weather == null)
				return false;
			
			if (weather.StartTime == 0)
				return false;
			
			weather.CreateWeather(weather.Position, weather.Width, weather.Speed, weather.FogDiffusion, weather.Intensity, NowTicks);
			StartWeather(weather);
			return true;
		}
		
		/// <summary>
		/// Stop Weather for Region
		/// </summary>
		/// <param name="regionId">Region ID where weather must be stopped</param>
		/// <returns>True if weather was stopped</returns>
		public bool StopWeather(ushort regionId)
		{
			var weather = this[regionId];
			
			if (weather == null)
				return false;
			
			if (weather.StartTime == 0)
				return false;
			
			StopWeather(weather);
			return true;
		}
		
		#region Registering
		/// <summary>
		/// Register a new Region to Weather Manager
		/// </summary>
		/// <param name="region"></param>
		private void RegisterRegion(Region region)
		{
			var success = false;
			lock (LockObject)
			{
				if (!RegionsWeather.ContainsKey(region.ID))
				{
					try
					{
						var weather = new RegionWeather(region);
						RegionsWeather.Add(region.ID, weather);
						success = true;
					}
					catch (Exception ex)
					{
						if (log.IsErrorEnabled)
							log.ErrorFormat("Error While Registering Region's Weather : {0} (ID:{1})\n{2}", region.Description, region.ID, ex);
					}
				}
				else
				{
					if (log.IsWarnEnabled)
						log.WarnFormat("Trying to Add Region {0} (ID:{1}) to WeatherManager while already Registered!", region.Description, region.ID);
				}
			}
			
			if (success && !OnTick && !WeatherTimer.Enabled)
			{
				WeatherTimer.Enabled = true;

				if (log.IsInfoEnabled)
					log.Info("Weather Manager Global Timer Started after Registering a Region...");
			}
		}
		
		/// <summary>
		/// UnRegister a Stopped Region from Weather Manager
		/// </summary>
		/// <param name="region"></param>
		private void UnRegisterRegion(Region region)
		{
			RegionWeather current;
			lock (LockObject)
			{
				if (RegionsWeather.TryGetValue(region.ID, out current))
				{
					RegionsWeather.Remove(region.ID);
				}
				else
				{
					if (log.IsWarnEnabled)
						log.WarnFormat("Trying to Remove Region {0} (ID:{1}) from WeatherManager but was not registered!", region.Description, region.ID);
				}
			}

			if (current != null && current.StartTime != 0)
				StopWeather(current);
		}
		#endregion
		
		#region Update Handlers
		/// <summary>
		/// Stop Weather from given Weather Object
		/// </summary>
		/// <param name="weather">Weather to Stop</param>
		private void StopWeather(RegionWeather weather)
		{
			var weatherCurrentPosition = weather.CurrentPosition(NowTicks);
			if (EventLogWeather && log.IsInfoEnabled)
				log.InfoFormat("Weather Stopped in Region {0} (ID {1}) CurrentPosition : {2}\n{3}", weather.Region.Description, weather.Region.ID, weatherCurrentPosition, weather);

			weather.Clear();
			weather.DueTime = NowTicks + DefaultTimerInterval;
			foreach (var player in weather.Region.Objects.OfType<GamePlayer>())
			{
				SendWeatherUpdate(weather, player);
				
				if (player.X > weatherCurrentPosition - weather.Width && player.X < weatherCurrentPosition)
					player.Out.SendMessage("The sky clears up again as the storm clouds disperse!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}
		}
		
		/// <summary>
		/// Start Weather according given Weather Object
		/// </summary>
		/// <param name="weather">Weather to Start</param>
		private void StartWeather(RegionWeather weather)
		{
			if (EventLogWeather && log.IsInfoEnabled)
				log.InfoFormat("Weather Started in Region {0} (ID {1})\n{2}", weather.Region.Description, weather.Region.ID, weather);
			
			foreach (var player in weather.Region.Objects.OfType<GamePlayer>())
				SendWeatherUpdate(weather, player);
		}
		
		/// <summary>
		/// Send Weather Update for this Region's Player
		/// </summary>
		/// <param name="region">Region of Player</param>
		/// <param name="player">Player</param>
		private void SendWeatherUpdate(Region region, GamePlayer player)
		{
			var current = this[region.ID];
			if (current != null)
				SendWeatherUpdate(current, player);
		}
		
		/// <summary>
		/// Send Weather Update to Player
		/// </summary>
		/// <param name="weather">Weather to send</param>
		/// <param name="player">Player Targeted</param>
		private void SendWeatherUpdate(RegionWeather weather, GamePlayer player)
		{
			if (player == null || player.ObjectState != GameObject.eObjectState.Active)
				return;
			
			if (weather.StartTime == 0)
				player.Out.SendWeather(0, 0, 0, 0, 0);
			else
				player.Out.SendWeather(weather.CurrentPosition(NowTicks), weather.Width, weather.Speed, weather.FogDiffusion, weather.Intensity);
		}
		#endregion
		
		#region Event Handlers
		/// <summary>
		/// Check Weather Periodically to trigger or stop events.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		private void OnWeatherTimerTick(object source, ElapsedEventArgs e)
		{
			if (OnTick)
				return;
			
			OnTick = true;
			long interval = DefaultTimerInterval;
			var anyActive = true;
			
			try
			{
				var stopWeather = new List<RegionWeather>();
				var startWeather = new List<RegionWeather>();
				
				lock (LockObject)
				{
					anyActive = RegionsWeather.Any();

					foreach (var entry in RegionsWeather.OrderBy(kv => kv.Value.DueTime).ToArray())
					{
						if (entry.Value.DueTime <= NowTicks)
						{
							// Check for a chance of Weather or stop existing
							if (!Util.Chance(DefaultWeatherChance))
							{
								if (entry.Value.StartTime != 0)
									stopWeather.Add(entry.Value);
								
								entry.Value.DueTime = NowTicks + DefaultTimerInterval;
								continue;
							}
							
							// Trigger Weather
							entry.Value.CreateWeather(NowTicks);
							interval = Math.Min(interval, Math.Max(1000, entry.Value.DueTime - NowTicks));
							startWeather.Add(entry.Value);
						}
						else
						{
							// Reschedule for next Check
							interval = Math.Min(interval, Math.Max(1000, entry.Value.DueTime - NowTicks));
							break;
						}
					}
				}
			
				foreach (var weather in stopWeather)
					StopWeather(weather);
				
				foreach (var weather in startWeather)
					StartWeather(weather);
			}
			catch (Exception ex)
			{
				if (log.IsErrorEnabled)
					log.Error("Exception in Weather Manager Timer: ", ex);
			}
			finally
			{
				WeatherTimer.Interval = interval;
				OnTick = false;
				
				if (anyActive)
				{
					WeatherTimer.Enabled = true;
				}
				else
				{
					WeatherTimer.Enabled = false;
					if (log.IsInfoEnabled)
						log.Info("Weather Manager Global Timer Stopped, no more Region Registered...");
				}
			}
		}
				
		/// <summary>
		/// When Region Start, Register to WeatherManager
		/// </summary>
		private void OnRegionStart(DOLEvent e, object sender, EventArgs arguments)
		{
			var region = sender as Region;
			
			if (region != null && !region.IsDungeon)
				RegisterRegion(region);
		}

		/// <summary>
		/// When Region Stop, unRegister to WeatherManager
		/// </summary>
		private void OnRegionStop(DOLEvent e, object sender, EventArgs arguments)
		{
			var region = sender as Region;
			
			if (region != null && !region.IsDungeon)
				UnRegisterRegion(region);
		}
		
		/// <summary>
		/// When Player Enter Region, Refresh Current Weather
		/// </summary>
		private void OnPlayerEnter(DOLEvent e, object sender, EventArgs arguments)
		{
			var region = sender as Region;
			var args = arguments as RegionPlayerEventArgs;
			
			if (region != null && args != null)
				SendWeatherUpdate(region, args.Player);
		}
		#endregion
	}
}
