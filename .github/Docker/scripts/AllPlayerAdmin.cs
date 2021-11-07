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
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS.GameEvents
{
        /// <summary>
        /// Set everyone Admin
        /// </summary>
        public class AllPlayerAdmin
        {
                /// <summary>
                /// Event handler fired when server is started
                /// </summary>
                [GameServerStartedEvent]
                public static void OnServerStart(DOLEvent e, object sender, EventArgs arguments)
                {
                        GameEventMgr.AddHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEntered));
                }

                /// <summary>
                /// Event handler fired when players enters the game
                /// </summary>
                /// <param name="e"></param>
                /// <param name="sender"></param>
                /// <param name="arguments"></param>
                private static void PlayerEntered(DOLEvent e, object sender, EventArgs arguments)
                {
                        GamePlayer player = sender as GamePlayer;
                        if (player == null) return;
                        if (player.Client.Account.PrivLevel != 3)
                        {
                                player.Client.Account.PrivLevel = 3;
                                GameServer.Database.SaveObject(player.Client.Account);
                                player.RefreshWorld();
                        }
                }
        }
}
