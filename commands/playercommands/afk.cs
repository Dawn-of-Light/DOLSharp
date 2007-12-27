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
		"&afk",
		ePrivLevel.Player,
		"Toggle away from keyboard. You may optional set a message to display.", "/afk <text>")]
	public class AFKCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (client.Player.TempProperties.getProperty(GamePlayer.AFK_MESSAGE, null) != null && args.Length == 1)
			{
				client.Player.TempProperties.removeProperty(GamePlayer.AFK_MESSAGE);
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Afk.Off"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
			{
				if (args.Length > 1)
				{
					string message = string.Join(" ", args, 1, args.Length - 1);
					client.Player.TempProperties.setProperty(GamePlayer.AFK_MESSAGE, message);
				}
				else
				{
					client.Player.TempProperties.setProperty(GamePlayer.AFK_MESSAGE, "");
				}
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Afk.On"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}
	}
}
