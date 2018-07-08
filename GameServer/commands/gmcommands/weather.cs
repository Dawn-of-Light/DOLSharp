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
        /// <summary>
        /// Execute Weather Command
        /// </summary>
        public void OnCommand(GameClient client, string[] args)
        {
            if (args.Length >= 2)
            {
                var action = args[1].ToLower();

                switch (action)
                {
                    case "info":
                        break;
                    case "restart":
                        if (GameServer.Instance.WorldManager.WeatherManager.RestartWeather(client.Player.CurrentRegionID))
                        {
                            DisplayMessage(client, "Weather (restart): Restarting Weather in this region!");
                        }
                        else
                        {
                            DisplayMessage(client, "Weather (restart): Weather could not be restarted in this region!");
                        }

                        break;
                    case "stop":
                        if (GameServer.Instance.WorldManager.WeatherManager.StopWeather(client.Player.CurrentRegionID))
                        {
                            DisplayMessage(client, "Weather (stop): Weather was Stopped in this Region!");
                        }
                        else
                        {
                            DisplayMessage(client, "Weather (stop): Weather could not be Stopped in this Region!");
                        }

                        break;
                    case "start":
                        if (args.Length > 2)
                        {
                            try
                            {
                                uint position = Convert.ToUInt32(args[2]);
                                uint width = Convert.ToUInt32(args[3]);
                                ushort speed = Convert.ToUInt16(args[4]);
                                ushort diffusion = Convert.ToUInt16(args[5]);
                                ushort intensity = Convert.ToUInt16(args[6]);
                                if (!GameServer.Instance.WorldManager.WeatherManager.StartWeather(client.Player.CurrentRegionID, position, width, speed, diffusion, intensity))
                                {
                                    DisplayMessage(client, "Weather (start): Weather could not be started in this Region!");
                                    break;
                                }
                            }
                            catch
                            {
                                DisplayMessage(client, "Weather (start): Wrong Arguments...");
                                DisplaySyntax(client);
                                return;
                            }
                        }
                        else
                        {
                            if (!GameServer.Instance.WorldManager.WeatherManager.StartWeather(client.Player.CurrentRegionID))
                            {
                                DisplayMessage(client, "Weather (start): Weather could not be started in this Region!");
                                break;
                            }
                        }

                        DisplayMessage(client, "Weather (start): The Weather has been started for this region!");
                        break;
                }

                PrintInfo(client);
                return;
            }

            DisplaySyntax(client);
        }

        /// <summary>
        /// Display Weather Info to Client
        /// </summary>
        /// <param name="client"></param>
        public void PrintInfo(GameClient client)
        {
            var weather = GameServer.Instance.WorldManager.WeatherManager[client.Player.CurrentRegionID];

            if (weather == null)
            {
                DisplayMessage(client, "Weather (info): No Weather Registered for current Region...");
            }
            else
            {
                if (weather.StartTime == 0)
                {
                    DisplayMessage(client, "Weather (info): Weather is stopped for current Region...");
                }
                else
                {
                    DisplayMessage(client, "Weather (info): Current Position - {0} - {1}", weather.CurrentPosition(Scheduler.SimpleScheduler.Ticks), weather);
                }
            }
        }
    }
}