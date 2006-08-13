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
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
    [CmdAttribute("&groundassist", //command to handle
         (uint)ePrivLevel.Player, //minimum privelege level
         "Show the current coordinates", //command description
         "/groundassist")] //command usage
    public class GroundAssistCommandHandler : ICommandHandler
    {
        public int OnCommand(GameClient client, string[] args)
        {
            GameObject obj = client.Player.TargetObject;
            if (args.Length > 1)
            {
                GameClient myclient;
                myclient = WorldMgr.GetClientByPlayerName(args[1], true, true);
                if (myclient == null)
                {
                    client.Player.Out.SendMessage("No player with this name in game.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                    return 1;
                }
                obj = myclient.Player;
            }

            if (obj == client.Player)
            {
                client.Out.SendMessage("You can't groundassist yourself.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return 0;
            }

            if (!GameServer.ServerRules.IsSameRealm(client.Player, obj, false))
                return 0;

            if (!WorldMgr.CheckDistance(client.Player, obj, 2048))
            {
                client.Out.SendMessage("You don't see " + args[1] + " around here!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return 0;
            }

            if (obj != null)
                client.Player.SetGroundTarget(obj.X, obj.Y, obj.Z);
            else
                client.Player.Out.SendMessage("You must select a target before use this command.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
            return 1;
        }
    }
}