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
/*
 * Author:		Roach <roach@fallenrealms.net>
 * Mod:			Kristopher Gilbert <ogrefallenrealms.net>
 * Rev:			$Id: anonymous.cs,v 1.1 2005/04/17 10:00:52 noret Exp $
 * Copyright:	2004 by Hired Goons, LLC
 * License:		http://www.gnu.org/licenses/gpl.txt
 * 
 * Implements the /anonymous Admin command.
 * 
 */

using System;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.Language;
using log4net;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
	  "&anonymous",
	  (uint)ePrivLevel.Player,
	  "Toggle anonymous mode (name doesn't show up in /who)",
	  "/anonymous")]
	public class AnonymousCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (client.Account.PrivLevel == 1 && ServerProperties.Properties.ANON_MODIFIER == -1)
			{
                DisplayError(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Anonymous.Error"), new object[] { });
                return 0;
			}

			client.Player.IsAnonymous = !client.Player.IsAnonymous;
			string[] friendList = new string[]
				{
					client.Player.Name
				};
			if (client.Player.IsAnonymous)
			{
                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Anonymous.On"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                foreach (GameClient pclient in WorldMgr.GetAllPlayingClients())
				{
					if (pclient.Player.Friends.Contains(client.Player.Name))
						pclient.Out.SendRemoveFriends(friendList);
				}
			}
			else
			{
                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Anonymous.Off"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                foreach (GameClient pclient in WorldMgr.GetAllPlayingClients())
				{
					if (pclient.Player.Friends.Contains(client.Player.Name))
						pclient.Out.SendAddFriends(friendList);
				}
			}
			return 1;
		}
	}
}
