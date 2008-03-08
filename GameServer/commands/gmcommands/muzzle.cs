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
* Written by Supgee.
* August 25, 2007.
* changed by Ultra2k.
* March 05, 2008.
*/
using System;
using System.IO;
using System.Reflection;
using System.Collections;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Commands
{
    [CmdAttribute(
    "&muzzle", //command to handle
    ePrivLevel.GM, //minimum privelege level
    "A command to mute players.", //command description
    "/muzzle <on> / <off>")] //usage
    public class MuzzleCommandHandler : ICommandHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void OnCommand(GameClient client, string[] args)
        {
            if (args.Length < 2)
            {
                client.Out.SendMessage("Usage: /muzzle <on> / <off>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }
            switch (args[1])
            {
                case "on":
                    {
                        GamePlayer player = client.Player.TargetObject as GamePlayer;
                        if (player == null)
                        {
                            client.Out.SendMessage("You must select a valid target.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return;
                        }
                        if (player.Client.Account.PrivLevel > 1)
                        {
                            client.Out.SendMessage("You cannot muzzle a GM.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                            return;
                        }
                        player.IsMuzzled = true;
                        player.Out.SendMessage("You've been muzzled by " + client.Player.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        client.Out.SendMessage("You've muzzled " + player.Name + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        log.Info(client.Player.Name + " muzzled " + player.Name + ".");
                    }
                    break;
                case "off":
                    {
                        GamePlayer player = client.Player.TargetObject as GamePlayer;
                        if (player == null)
                        {
                            client.Out.SendMessage("You must select a valid target.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return;
                        }
                        player.IsMuzzled = false;
                        player.Out.SendMessage("You've been unmuzzled by " + client.Player.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        client.Out.SendMessage("You've unmuzzled " + player.Name + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        log.Info(client.Player.Name + " unmuzzled " + player.Name + ".");
                    }
                    break;
                default: break;
            }
            return;
        }
    }
}