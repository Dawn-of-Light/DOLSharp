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
        "&freeze",
        ePrivLevel.Admin,
        "Freeze The region timer you're in. (Test purpose only)",
        "/freeze {seconds}")]
    public class Freeze : AbstractCommandHandler, ICommandHandler
    {
        private int delay = 0;

        public void OnCommand(GameClient client, string[] args)
        {
            if (args.Length < 2)
            {
                DisplaySyntax(client);
                return;
            }

            if (client != null && client.Player != null)
            {
                try
                {
                    delay = Convert.ToInt32(args[1]);
                    new RegionTimer(client.Player, FreezeCallback).Start(1);
                }
                catch
                {
                }
            }
        }

        private int FreezeCallback(RegionTimer timer)
        {
            System.Threading.Thread.Sleep(delay * 1000);
            return 0;
        }
    }
}
