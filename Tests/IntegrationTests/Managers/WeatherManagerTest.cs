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

using DOL.GS;
using DOL.GS.Scheduler;
using NUnit.Framework;

namespace DOL.Integration.Managers
{
	/// <summary>
	/// Unit Tests for WeatherManagerTest.
	/// </summary>
	[TestFixture]
	public class WeatherManagerTest
	{
		public WeatherManagerTest()
		{
		}
		
		[OneTimeSetUp]
		public void SetUp()
		{
			log4net.Config.BasicConfigurator.Configure(
				new log4net.Appender.ConsoleAppender {
					Layout = new log4net.Layout.SimpleLayout()});
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			log4net.LogManager.Shutdown();
		}
		
		Region FakeRegion()
		{
			DOL.GS.ServerProperties.Properties.DISABLED_REGIONS = string.Empty;
			DOL.GS.ServerProperties.Properties.DISABLED_EXPANSIONS = string.Empty;
			var region = Region.Create(new GameTimer.TimeManager("TestWeather"), new RegionData { Id = 1 });
			region.Zones.Add(new Zone(region, 1, string.Empty, 0, 0, 65535, 65535, 1, false, 0, false, 0, 0, 0, 0, 1));
			return region;
		}
		
		[Test]
		public void WeatherManager_GetNonExistentRegion_ReturnNull()
		{
			var weatherMgr = new WeatherManager(new SimpleScheduler());
			
			Assert.That(weatherMgr[1], Is.Null);
		}
		
		[Test]
		public void WeatherManager_ChangeWeatherNonExistentRegion_ReturnFalse()
		{
			var weatherMgr = new WeatherManager(new SimpleScheduler());
			
			Assert.That(weatherMgr.ChangeWeather(1, weather => weather.Clear()), Is.False);
		}
		
		[Test]
		public void WeatherManager_RegisterRegion_ReturnRegion()
		{
			var weatherMgr = new WeatherManager(new SimpleScheduler());
			
			var region = FakeRegion();
			weatherMgr.RegisterRegion(region);
			
			Assert.That(region, Is.EqualTo(weatherMgr[1].Region));
		}
		
		[Test]
		public void WeatherManager_UnRegisterRegion_ReturnNull()
		{
			var weatherMgr = new WeatherManager(new SimpleScheduler());
			
			var region = FakeRegion();
			weatherMgr.RegisterRegion(region);
			weatherMgr.UnRegisterRegion(region);
			
			Assert.That(weatherMgr[1], Is.Null);
		}
		
		[Test]
		public void WeatherManager_StartWeatherRegion_ReturnTrue()
		{
			var weatherMgr = new WeatherManager(new SimpleScheduler());
			
			var region = FakeRegion();
			weatherMgr.RegisterRegion(region);
			
			Assert.That(weatherMgr.StartWeather(1), Is.True);
		}

		[Test]
		public void WeatherManager_StopWeatherRegion_ReturnTrue()
		{
			var weatherMgr = new WeatherManager(new SimpleScheduler());
			
			var region = FakeRegion();
			weatherMgr.RegisterRegion(region);
			
			weatherMgr.StartWeather(1);
			Assert.That(weatherMgr.StopWeather(1), Is.True);
		}

		[Test]
		public void WeatherManager_StartWeatherRegion_StartTime()
		{
			var weatherMgr = new WeatherManager(new SimpleScheduler());
			
			var region = FakeRegion();
			weatherMgr.RegisterRegion(region);
			
			weatherMgr.StartWeather(1);
			Assert.That(weatherMgr[1].StartTime, Is.GreaterThan(0));
		}

		[Test]
		public void WeatherManager_StopWeatherRegion_StartTime()
		{
			var weatherMgr = new WeatherManager(new SimpleScheduler());
			
			var region = FakeRegion();
			weatherMgr.RegisterRegion(region);
			
			weatherMgr.StartWeather(1);
			weatherMgr.StopWeather(1);
			Assert.That(weatherMgr[1].StartTime, Is.EqualTo(0));
		}

		[Test]
		public void WeatherManager_ChangeWeatherRegion_WeatherEqual()
		{
			var weatherMgr = new WeatherManager(new SimpleScheduler());
			
			var region = FakeRegion();
			weatherMgr.RegisterRegion(region);
			
			weatherMgr.ChangeWeather(1, weather => weather.CreateWeather(65000, 300, 100, 16000, 0));

			Assert.That(weatherMgr[1].Duration / 1000, Is.EqualTo((65535 + 65000) / 300));
			Assert.That(weatherMgr[1].Width, Is.EqualTo(65000));
		}

		[Test]
		public void WeatherManager_ChangeWeatherRegionException_WeatherEqual()
		{
			var weatherMgr = new WeatherManager(new SimpleScheduler());
			
			var region = FakeRegion();
			weatherMgr.RegisterRegion(region);
			
			weatherMgr.ChangeWeather(1, weather => { weather.CreateWeather(65000, 300, 100, 16000, 0); throw new Exception(); });

			Assert.That(weatherMgr[1].Duration / 1000, Is.EqualTo((65535 + 65000) / 300));
			Assert.That(weatherMgr[1].Width, Is.EqualTo(65000));
		}
		
		[Test]
		public void WeatherRegion_InitWeather_MinMaxEqualZone()
		{
			var region = FakeRegion();
			var weather = new RegionWeather(region);
			weather.CreateWeather(65000, 300, 100, 16000, 0);
			
			var duration = (65535 + 65000) * 1000 / 300;
			
			Assert.That(weather.CurrentPosition(duration), Is.EqualTo(65535 + 65000));
			Assert.That(weather.CurrentPosition(0), Is.EqualTo(0));
		}

	}
}
