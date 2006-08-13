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
	[CmdAttribute("&assist", (uint) ePrivLevel.Player, "Assist your target", "/assist [playerName]")]
	public class AssistCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			GamePlayer assistPlayer = null;
			if (args.Length > 1)
			{
				GameClient assistClient = WorldMgr.GetClientByPlayerName(args[1], true, true);
				if (assistClient != null)
				{
                    if (!GameServer.ServerRules.IsSameRealm(client.Player, assistPlayer, false))
                        return 0;

                    if (!WorldMgr.CheckDistance(client.Player, assistClient.Player, 2048))
					{
						client.Out.SendMessage("You don't see " + args[1] + " around here!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return 0;
					}
					assistPlayer = assistClient.Player;
				}
			}
			else if (client.Player.TargetObject == null)
			{
				client.Out.SendMessage("You have no target.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 0;
			}
			else
			{
				assistPlayer = client.Player.TargetObject as GamePlayer;
			}

			if (assistPlayer == null)
			{
				client.Out.SendMessage("Target is not valid.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 0;
			}
			if (assistPlayer == client.Player)
			{
				client.Out.SendMessage("You can't assist yourself.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 0;
			}

			if (assistPlayer.TargetObject == null)
			{
				client.Out.SendMessage(assistPlayer.GetName(0, true) + " doesn't currently have a target", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 0;
			}

			client.Out.SendChangeTarget(assistPlayer.TargetObject);
			client.Out.SendMessage("You assist " + assistPlayer.GetName(0, true) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			return 1;
		}
	}
}