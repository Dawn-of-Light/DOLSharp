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
		"&whisper",
		new string[] {"&whis"}, //Important, don't remove this alias, its used for communication with mobs!
		ePrivLevel.Player,
		"Sends a private message to your target if it is close enough",
		"/whisper <message>")]
	public class WhisperCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 2)
			{
				client.Out.SendMessage("Usage: /whisper Message", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}
			GameObject obj = client.Player.TargetObject;
			if (obj == null)
			{
				client.Out.SendMessage("Select the target you want to whisper to!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}
			if (!(obj is GameLiving))
			{
				client.Out.SendMessage("You look pretty silly whispering to " + obj.GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}
			if (obj == client.Player)
			{
				client.Out.SendMessage("Hmmmm...you shouldn't talk to yourself!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

				return 1;
			}
			GameLiving target = (GameLiving) obj;
			string message = string.Join(" ", args, 1, args.Length - 1);
			client.Player.Whisper(target, message);
			return 1;
		}
	}
}