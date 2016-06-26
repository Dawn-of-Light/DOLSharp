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

using System;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS.Commands
{
	[CmdAttribute(
	  "&anonymous",
	  ePrivLevel.Player,
	  "Toggle anonymous mode (name doesn't show up in /who)",
	  "/anonymous")]
	public class AnonymousCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		/// <summary>
		/// Change Player Anonymous Flag on Command
		/// </summary>
		/// <param name="client"></param>
		/// <param name="args"></param>
		public void OnCommand(GameClient client, string[] args)
		{
			if (client.Player == null)
				return;
			
			if (client.Account.PrivLevel == 1 && ServerProperties.Properties.ANON_MODIFIER == -1)
			{
				DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Anonymous.Error"));
				return;
			}

			client.Player.IsAnonymous = !client.Player.IsAnonymous;

			if (client.Player.IsAnonymous)
				client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Anonymous.On"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			else
				client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Anonymous.Off"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}
	}
}
