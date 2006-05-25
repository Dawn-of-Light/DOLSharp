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
using DOL.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[Cmd(
		"&weather",
		(uint) ePrivLevel.GM,
		"Sets the weather for the current region",
		"'/weather info' for information about the current weather in this region",
		"'/weather start <line> <duration> <speed> <diffusion> <intensity>' to start a storm in this region",
		"'/weather start' to start a random storm in this region",
		"'/weather restart' to restart the storm in this region",
		"'/weather stop' to stop the storm in this region")]
	public class WeatherCommandHandler : ICommandHandler
	{
		public void PrintUsage(GameClient client)
		{
			client.Out.SendMessage("Usage: '/weather info' to get information about the current weather", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage("Usage: '/weather start' to start a random storm in this region", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage("Usage: '/weather start <line> <duration> <speed> <diffusion> <intensity>' to start a storm with the given parameters", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage("Usage: '/weather restart' to restart the storm in this region", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage("Usage: '/weather stop' to stop the storm in this region", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		public void PrintStormInfo(GameClient client)
		{
			WeatherMgr mgr = WeatherMgr.GetWeatherForRegion((ushort)client.Player.Region.RegionID);
			if (mgr == null)
				return;
			bool active = mgr.IsActive;
			if (!active)
			{
				client.Out.SendMessage("WEATHERINFO: There is no storm active in this region!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			client.Out.SendMessage("WEATHERINFO: Storm is at X=" + mgr.CurrentWeatherLine +
				", Width=" + mgr.Width +
				", Speed=" + mgr.Speed +
				", Fog=" + mgr.FogDiffusion +
				", Intensity=" + mgr.Intensity, eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		public int OnCommand(GameClient client, string[] args)
		{
			if (args.Length == 1)
			{
				PrintUsage(client);
				return 1;
			}

			WeatherMgr mgr = WeatherMgr.GetWeatherForRegion((ushort)client.Player.Region.RegionID);

			if (args.Length == 2)
			{
				if (mgr == null)
				{
					client.Out.SendMessage("WEATHERINFO: There is no weather manager for this region!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 1;
				}

				switch (args[1])
				{
					case "info":
						PrintStormInfo(client);
						return 1;
					case "start":
						mgr.StartStorm();
						client.Out.SendMessage("WEATHERINFO: A random storm has been started for this region!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						PrintStormInfo(client);
						return 1;
					case "restart":
						mgr.RestartStorm();
						client.Out.SendMessage("WEATHERINFO: The storm has been restarted for this region!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						PrintStormInfo(client);
						return 1;
					case "stop":
						if (!mgr.IsActive)
							client.Out.SendMessage("WEATHERINFO: There is no storm active in this region!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						else
						{
							mgr.StopStorm();
							client.Out.SendMessage("WEATHERINFO: The storm has been stopped for this region until the next normal random interval!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						return 1;
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
					client.Out.SendMessage("WEATHERINFO: The storm has been started for this region!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					PrintStormInfo(client);
					return 1;
				}
				catch
				{
				}
			}
			PrintUsage(client);
			return 1;
		}
	}
}