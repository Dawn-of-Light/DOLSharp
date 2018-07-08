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
using System.Linq;
using System.Collections.Generic;

using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;

namespace DOL.GS
{
    public enum eNewsType : byte
    {
        RvRGlobal = 0,
        RvRLocal = 1,
        PvE = 2,
    }

    public class NewsMgr
    {
        public static void CreateNews(string message, eRealm realm, eNewsType type, bool sendMessage)
        {
            if (sendMessage)
            {
                foreach (GameClient client in WorldMgr.GetAllClients())
                {
                    if (client.Player == null)
                    {
                        continue;
                    }

                    if ((client.Account.PrivLevel != 1 || realm == eRealm.None) || client.Player.Realm == realm)
                    {
                        client.Out.SendMessage(message, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
                }
            }

            if (ServerProperties.Properties.RECORD_NEWS)
            {
                DBNews news = new DBNews();
                news.Type = (byte)type;
                news.Realm = (byte)realm;
                news.Text = message;
                GameServer.Database.AddObject(news);
                GameEventMgr.Notify(DatabaseEvent.NewsCreated, new NewsEventArgs(news));
            }
        }

        public static void DisplayNews(GameClient client)
        {
            // N,chanel(0/1/2),index(0-4),string time,\"news\"
            for (int type = 0; type <= 2; type++)
            {
                int index = 0;
                string realm = string.Empty;

                // we can see all captures
                IList<DBNews> newsList;
                if (type > 0)
                {
                    newsList = GameServer.Database.SelectObjects<DBNews>(
                        "`Type` = @Type AND (`Realm` = @DefaultRealm OR `Realm` = @Realm)",
                                                                         new[] { new QueryParameter("@Type", type), new QueryParameter("@DefaultRealm", 0), new QueryParameter("@Realm", realm) });
                }
                else
                {
                    newsList = GameServer.Database.SelectObjects<DBNews>("`Type` = @Type", new QueryParameter("@Type", type));
                }

                newsList = newsList.OrderByDescending(it => it.CreationDate).Take(5).ToArray();
                int n = newsList.Count;

                while (n > 0)
                {
                    n--;
                    DBNews news = newsList[n];
                    client.Out.SendMessage(string.Format("N,{0},{1},{2},\"{3}\"", news.Type, index++, RetElapsedTime(news.CreationDate), news.Text), eChatType.CT_SocialInterface, eChatLoc.CL_SystemWindow);
                }
            }
        }

        private static string RetElapsedTime(DateTime dt)
        {
            TimeSpan playerEnterGame = DateTime.Now.Subtract(dt);
            string newsTime;
            if (playerEnterGame.Days > 0)
            {
                newsTime = playerEnterGame.Days.ToString() + " day" + ((playerEnterGame.Days > 1) ? "s" : string.Empty);
            }
            else if (playerEnterGame.Hours > 0)
            {
                newsTime = playerEnterGame.Hours.ToString() + " hour" + ((playerEnterGame.Hours > 1) ? "s" : string.Empty);
            }
            else if (playerEnterGame.Minutes > 0)
            {
                newsTime = playerEnterGame.Minutes.ToString() + " minute" + ((playerEnterGame.Minutes > 1) ? "s" : string.Empty);
            }
            else
            {
                newsTime = playerEnterGame.Seconds.ToString() + " second" + ((playerEnterGame.Seconds > 1) ? "s" : string.Empty);
            }

            return newsTime;
        }
    }
}