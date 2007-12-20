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
using DOL.GS;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		 "&safety",
		 ePrivLevel.Player,
		 "Turns off PvP safety flag.",
		 "/safety off")]
	public class SafetyCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if(GameServer.Instance.Configuration.ServerType != eGameServerType.GST_PvP)
				return 1;

			if(args.Length >= 2 && args[1].ToLower() == "off")
			{
				client.Player.SafetyFlag = false;
				client.Out.SendMessage("Your safety flag is now set to OFF!  You can now attack non allied players, as well as be attacked.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else if(client.Player.SafetyFlag)
			{
				client.Out.SendMessage("The safety flag keeps your character from participating in combat", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("with non allied players in designated zones when you are below level 10.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("Type /safety off to begin participating in PvP combat in these zones, though once it is off it can NOT be turned back on!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
			{
				client.Out.SendMessage("Your safety flag is already off.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}

			return 1;
		}
	}
}