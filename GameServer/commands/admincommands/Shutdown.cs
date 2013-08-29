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
using System.Collections;
using System.Reflection;
using System.Threading;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;
using DOL.Database;

namespace DOL.GS.Commands
{
    [CmdAttribute(
        "&shutdown",
        ePrivLevel.Admin,
        "Shutdown the server in next minute",
        "/shutdown on <hePrivLevel.our>:<min>  - shutdown on this time",
        "/shutdown <mins>  - shutdown in minutes")]
    public class ShutdownCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static long m_counter = 0;
        private static Timer m_timer;
        private static int m_time = 5;

        public static long getShutdownCounter()
        {
            return m_counter;
        }

        [ScriptLoadedEvent]
        public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
        {
            DOL.Events.GameEventMgr.AddHandler(GameServerEvent.WorldSave, new DOLEventHandler(AutomaticShutdown));
        }

        public static void AutomaticShutdown(DOLEvent e, object sender, EventArgs args)
        {
            if (m_timer != null)
                return;

            TimeSpan uptime = TimeSpan.FromTicks(GameServer.Instance.TickCount);

            ServerProperty propertyHour = GameServer.Database.SelectObject<ServerProperty>("`Key` = 'shutdown_hour'");
            if (propertyHour == null)
            {
                propertyHour = new ServerProperty();
                propertyHour.DefaultValue = "-1";
                propertyHour.Description = "The hour the server should shut down";
                propertyHour.Key = "shutdown_hour";
                propertyHour.Value = "-1";

                GameServer.Database.AddObject(propertyHour);
            }

            ServerProperty propertyDays = GameServer.Database.SelectObject<ServerProperty>("`Key` = 'shutdown_days'");
            if (propertyDays == null)
            {
                propertyDays = new ServerProperty();
                propertyDays.DefaultValue = "-1";
                propertyDays.Description = "The max days the server should up before shut down";
                propertyDays.Key = "shutdown_days";
                propertyDays.Value = "-1";

                GameServer.Database.AddObject(propertyDays);
            }


            int hour = -1, days = -1;
            bool success = int.TryParse(propertyHour.Value, out hour);
            success = int.TryParse(propertyDays.Value, out days);

            if (!success || hour == -1)
                return;

            if (24 * days - hour < uptime.TotalHours && DateTime.Now.Hour == hour)
            {
                DOL.Events.GameEventMgr.RemoveHandler(GameServerEvent.WorldSave, new DOLEventHandler(AutomaticShutdown));
                m_counter = 15 * 60;
                m_timer = new Timer(new TimerCallback(CountDown), null, 0, 15000);
            }
        }

        public static void CountDown(object param)
        {
            if (m_counter <= 0)
            {
                m_timer.Dispose();
                new Thread(new ThreadStart(ShutDownServer)).Start();
                return;
            }
            else
            {
                log.Info("Server reboot in " + m_counter + " seconds!");
                long secs = m_counter;
                long mins = secs / 60;
                long hours = mins / 60;

                foreach (GameClient client in WorldMgr.GetAllPlayingClients())
                {
                    if (hours > 3) //hours...
                    {
                        if (mins % 60 == 0 && secs % 60 == 0) //every hour..
                            client.Out.SendMessage("Server reboot in " + hours + " hours!", eChatType.CT_Broadcast,
                                                   eChatLoc.CL_ChatWindow);
                    }
                    else if (hours > 0) //hours...
                    {
                        if (mins % 30 == 0 && secs % 60 == 0) //every 30 mins..
                            client.Out.SendMessage("Server reboot in " + hours + " hours and " + (mins - (hours * 60)) + "mins!", eChatType.CT_Staff,
                                                   eChatLoc.CL_ChatWindow);
                    }
                    else if (mins >= 10)
                    {
                        if (mins % 15 == 0 && secs % 60 == 0) //every 15 mins..
                            client.Out.SendMessage("Server reboot in " + mins + " mins!", eChatType.CT_Broadcast,
                                                   eChatLoc.CL_ChatWindow);
                    }
                    else if (mins >= 3)
                    {
                        if (secs % 60 == 0) //every min...
                            client.Out.SendMessage("Server reboot in " + mins + " mins!", eChatType.CT_Broadcast,
                                                   eChatLoc.CL_ChatWindow);
                    }
                    else if (secs > 60)
                    {
                        client.Out.SendMessage("Server reboot in " + mins + " minutes! (" + secs + " secs)", eChatType.CT_Broadcast,
                                                   eChatLoc.CL_ChatWindow);
                    }
                    else
                        client.Out.SendMessage("Server reboot in " + secs + " secs! Please logout!", eChatType.CT_Broadcast,
                                                   eChatLoc.CL_ChatWindow);
                }

                if (mins <= 5 && GameServer.Instance.ServerStatus != eGameServerStatus.GSS_Closed) // 5 mins remaining
                {
                    GameServer.Instance.Close();
                    string msg = "Server is now closed (reboot in " + mins + " mins)";
                    /*if (ServerIRC.IRCBot != null)
                        ServerIRC.IRCBot.SendMessage(ServerIRC.CHANNEL, msg);*/
                }
            }
            m_counter -= 15;
        }

        public static void ShutDownServer()
        {
            if (GameServer.Instance.IsRunning)
            {
                GameServer.Instance.Stop();
                log.Info("Automated server shutdown!");
                Thread.Sleep(2000);
                Environment.Exit(0);
            }
        }

        public void OnCommand(GameClient client, string[] args)
        {
            DateTime date;
            //if (m_counter > 0) return 0;
            if (args.Length >= 2)
            {
                if (args.Length == 2)
                {
                    try
                    {
                        m_counter = System.Convert.ToInt32(args[1]) * 60;
                    }
                    catch (Exception)
                    {
                        DisplaySyntax(client);
                        return;
                    }
                }
                
                else
                {
                    if ((args.Length == 3) && (args[1] == "on"))
                    {
                        string[] shutdownsplit = args[2].Split(':');

                        if ((shutdownsplit == null) || (shutdownsplit.Length < 2))
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        int hour = Convert.ToInt32(shutdownsplit[0]);
                        int min = Convert.ToInt32(shutdownsplit[1]);
                        // found next date with hour:min

                        date = DateTime.Now;

                        if ((date.Hour > hour) ||
                             (date.Hour == hour && date.Minute > min)
                           )
                            date = new DateTime(date.Year, date.Month, date.Day + 1);

                        if (date.Minute > min)
                            date = new DateTime(date.Year, date.Month, date.Day, date.Hour + 1, 0, 0);

                        date = date.AddHours(hour - date.Hour);
                        date = date.AddMinutes(min - date.Minute + 2);
                        date = date.AddSeconds(-date.Second);

                        m_counter = (date.ToFileTime() - DateTime.Now.ToFileTime()) / TimeSpan.TicksPerSecond;

                        if (m_counter < 60) m_counter = 60;

                    }
                    else
                    {
                        DisplaySyntax(client);
                        return;
                    }

                }
            }
            else
            {
                DisplaySyntax(client);
                return;
            }

            if (m_counter % 5 != 0)
                m_counter = (m_counter / 5 * 5);

            if (m_counter == 0)
                m_counter = m_time * 60;

            date = DateTime.Now;
            date = date.AddSeconds(m_counter);

            foreach (GameClient m_client in WorldMgr.GetAllPlayingClients())
            {
                m_client.Out.SendMessage("Server Shutdown in " + m_counter / 60 + " mins! (Reboot on " + date.ToString("HH:mm \"GMT\" zzz") + ")", eChatType.CT_System, eChatLoc.CL_PopupWindow);
            }

            string msg = "Server Shutdown in " + m_counter / 60 + " mins! (Reboot on " + date.ToString("HH:mm \"GMT\" zzz") + ")";
            log.Info(msg);

            m_timer = new Timer(new TimerCallback(CountDown), null, 0, 15000);
        }
    }
}