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
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[Cmd(
		"&weather",
		ePrivLevel.GM,
		"Sets the weather for the current region",
		"'/weather info' for information about the current weather in this region",
		"'/weather start <line> <duration> <speed> <diffusion> <intensity>' to start a storm in this region",
		"'/weather start' to start a random storm in this region",
		"'/weather restart' to restart the storm in this region",
		"'/weather stop' to stop the storm in this region")]
	public class WeatherCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void PrintStormInfo(GameClient client)
		{
			WeatherMgr mgr = WeatherMgr.GetWeatherForRegion(client.Player.CurrentRegionID);
			if (mgr == null)
				return;
			bool active = mgr.IsActive;
			if (!active)
			{
				DisplayMessage(client, "WEATHERINFO: There is no storm active in this region!");
				return;
			}
			DisplayMessage(client, string.Format("WEATHERINFO: Storm is at X={0}, Width={1}, Speed={2}, Fog={3}, Intensity={4}", mgr.CurrentWeatherLine, mgr.Width, mgr.Speed, mgr.FogDiffusion, mgr.Intensity));
		}

		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length == 1)
			{
				DisplaySyntax(client);
				return;
			}

			WeatherMgr mgr = WeatherMgr.GetWeatherForRegion(client.Player.CurrentRegionID);

			if (args.Length == 2)
			{
				if (mgr == null)
				{
					DisplayMessage(client, "WEATHERINFO: There is no weather manager for this region!");
					return;
				}

				switch (args[1])
				{
					case "info":
						{
							PrintStormInfo(client);
							return;
						}
					case "start":
						{
							mgr.StartStorm();
							DisplayMessage(client, "WEATHERINFO: A random storm has been started for this region!");
							PrintStormInfo(client);
							return;
						}
					case "restart":
						{
							mgr.RestartStorm();
							DisplayMessage(client, "WEATHERINFO: The storm has been restarted for this region!");
							PrintStormInfo(client);
							return;
						}
					case "stop":
						{
							if (!mgr.IsActive)
								DisplayMessage(client, "WEATHERINFO: There is no storm active in this region!");
							else
							{
								mgr.StopStorm();
								DisplayMessage(client, "WEATHERINFO: The storm has been stopped for this region until the next normal random interval!");
							}
							return;
						}
				}
			}

			if (args.Length == 7 && args[1].Equals("start"))
			{
				try
				{
					uint line = Convert.ToUInt32(args[2]);
					uint duration = Convert.ToUInt32(args[3]);
					ushort speed = Convert.ToUInt16(args[4]);
					ushort diffusion = Convert.ToUInt16(args[5]);
					ushort intensity = Convert.ToUInt16(args[6]);
					mgr.StartStorm(line, duration, speed, diffusion, intensity);
					DisplayMessage(client, "WEATHERINFO: The storm has been started for this region!");
					PrintStormInfo(client);
					return;
				}
				catch
				{
				}
			}
			DisplaySyntax(client);
		}
	}
}