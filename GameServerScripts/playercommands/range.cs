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
	[CmdAttribute(
		"&range",
		(uint) ePrivLevel.Player,
		"Gives a range to a target",
		"/range")]
	public class RangeCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (client.Player.TargetObject != null)
			{
//				if (client.Player.TargetObject is GameLiving)
//				{
//					client.Out.SendMessage("This command only works on objects.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
//					return 1;
//				}
				int range = client.Player.Position.GetDistance(client.Player.TargetObject.Position);
				client.Out.SendMessage("Range to target: " + range + " units." + (client.Player.TargetInView ? "" : " (Target not visible)"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
				client.Out.SendMessage("Range to target: You don't have a target set.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			return 0;
		}
	}
}