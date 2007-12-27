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
	[CmdAttribute(
		"&mountgm",
		ePrivLevel.GM,
		"Mount a steed",
		"/mountgm")]
	public class RideMountGmCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (client.Player.IsRiding)
			{
				DisplayMessage(client, "You are already riding a steed!");
				return;
			}

			if (client.Player.TargetObject == null || !(client.Player.TargetObject is GameNPC))
			{
				DisplayMessage(client, "You can't ride THIS!");
				return;
			}

			client.Player.MountSteed((GameNPC) client.Player.TargetObject, false);
		}
	}
}