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
using DOL.Language;

namespace DOL.GS.Commands
{
    [Cmd(
        "&debug",
        ePrivLevel.GM,
        "GMCommands.Debug.Description",
        "GMCommands.Debug.Usage")]
    public class DebugCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        public void OnCommand(GameClient client, string[] args)
        {
            if (client == null || client.Player == null)
            {
                return;
            }

            if (IsSpammingCommand(client.Player, "Debug"))
            {
                return;
            }

            // extra check to disallow all but server GM's
            if (client.Account.PrivLevel < 2)
            {
                return;
            }

            if (args.Length < 2)
            {
                DisplaySyntax(client);
                return;
            }

            if (args[1].ToLower().Equals("on"))
            {
                client.Player.TempProperties.setProperty(GamePlayer.DEBUG_MODE_PROPERTY, true);
                client.Player.IsAllowedToFly = true;
                client.Out.SendDebugMode(true);
                DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Debug.ModeON"));
            }
            else if (args[1].ToLower().Equals("off"))
            {
                client.Player.TempProperties.removeProperty(GamePlayer.DEBUG_MODE_PROPERTY);
                client.Out.SendDebugMode(false);
                client.Player.IsAllowedToFly = false;
                DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Debug.ModeOFF"));
            }
        }
    }
}