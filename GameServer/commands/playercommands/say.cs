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
		"&say",
		new string[] {"&s"},
		ePrivLevel.Player,
		"Say something to other players around you",
		"/say <message>")]
	public class SayCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			const string SAY_TICK = "Say_Tick";

			if (args.Length < 2)
			{
				client.Out.SendMessage("You must say something...", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			string message = string.Join(" ", args, 1, args.Length - 1);

			long SayTick = client.Player.TempProperties.getLongProperty(SAY_TICK, 0);
			if (SayTick > 0 && SayTick - client.Player.CurrentRegion.Time <= 0)
			{
				client.Player.TempProperties.removeProperty(SAY_TICK);
			}

			long changeTime = client.Player.CurrentRegion.Time - SayTick;
			if (changeTime < 500 && SayTick > 0)
			{
				client.Player.Out.SendMessage("Slow down! Think before you say each word!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			client.Player.Say(message);
			client.Player.TempProperties.setProperty(SAY_TICK, client.Player.CurrentRegion.Time);
		}
	}
}