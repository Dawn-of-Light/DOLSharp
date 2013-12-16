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

namespace DOL.GS.Commands
{
	[CmdAttribute("&groundassist", //command to handle
		 ePrivLevel.Player, //minimum privelege level
		 "Show the current coordinates", //command description
		 "/groundassist")] //command usage
	public class GroundAssistCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (IsSpammingCommand(client.Player, "groundassist"))
				return;

			GameLiving target = client.Player.TargetObject as GameLiving;
			if (args.Length > 1)
			{
				GameClient myclient;
				myclient = WorldMgr.GetClientByPlayerName(args[1], true, true);
				if (myclient == null)
				{
					client.Player.Out.SendMessage("No player with this name in game.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
					return;
				}
				target = myclient.Player;
			}

			if (target == client.Player)
			{
				client.Out.SendMessage("You can't groundassist yourself.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (target == null)
				return;

			// can't assist an enemy
			if (GameServer.ServerRules.IsAllowedToAttack(client.Player, target as GameLiving, true))
				return;

			if (!client.Player.IsWithinRadius( target, 2048 ))
			{
				client.Out.SendMessage("You don't see " + args[1] + " around here!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (target.GroundTarget == null || (target.GroundTarget.X == 0 && target.GroundTarget.Y == 0 && target.GroundTarget.Z == 0))
			{
				client.Out.SendMessage(target.Name + " doesn't currently have a ground target.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			client.Player.Out.SendChangeGroundTarget(target.GroundTarget);
			client.Player.SetGroundTarget(target.GroundTarget.X, target.GroundTarget.Y, target.GroundTarget.Z);
		}
	}
}