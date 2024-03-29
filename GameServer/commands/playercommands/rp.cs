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
using DOL.Language;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&rp",
		ePrivLevel.Player,
		"toggle receiving realm points",
		"/rp <on/off>")]
	public class RPCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 2)
			{
				DisplaySyntax(client);
				return;
			}

			if (IsSpammingCommand(client.Player, "rp"))
				return;

			if (args[1].ToLower().Equals("on"))
			{
				client.Player.GainRP = true;
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Rp.On"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}
			else if (args[1].ToLower().Equals("off"))
			{
				client.Player.GainRP = false;
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Rp.Off"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}
		}
	}
}