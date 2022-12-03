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
using DOL.Database;
using DOL.GS.PacketHandler;
using System.Collections;
using DOL.Language;

namespace DOL.GS.Commands
{
	[CmdAttribute("&makeleader",
		 ePrivLevel.Player,
		 "Set a new group leader (can be used by current leader).",
		 "/makeleader <playerName>")]

	public class MakeLeaderCommandHandler : ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (client.Player.Group == null || client.Player.Group.MemberCount < 2)
			{
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Makeleader.NoGroup"), eChatType.CT_System,eChatLoc.CL_SystemWindow);
				return;
			}
			if(client.Player.Group.Leader != client.Player)
			{
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Makeleader.NotLeader"), eChatType.CT_System,eChatLoc.CL_SystemWindow);
				return;
			}

			GamePlayer target;

			if(args.Length<2) // Setting by target
			{
				if(client.Player.TargetObject == null || client.Player.TargetObject == client.Player)
				{
					client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Makeleader.TargetInvalid"), eChatType.CT_System,eChatLoc.CL_SystemWindow);
					return;
				}

				if(!(client.Player.TargetObject is GamePlayer))
				{
					client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Makeleader.TargetInvalid"),eChatType.CT_System,eChatLoc.CL_SystemWindow);
					return;
				}
				target = (GamePlayer)client.Player.TargetObject;
				if(client.Player.Group != target.Group)
				{
					client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Makeleader.TargetInvalid"), eChatType.CT_System,eChatLoc.CL_SystemWindow);
					return;
				}
			}
			else //Setting by name
			{
				string targetName = args[1];
				GameClient targetClient = WorldMgr.GetClientByPlayerName(targetName, false, true);
				if (targetClient == null)
					target = null;
				else target = targetClient.Player;
				if(target==null || client.Player.Group != target.Group)
				{ // Invalid target
					client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Makeleader.NoPlayer"), eChatType.CT_System,eChatLoc.CL_SystemWindow);
					return;
				}
				if(target==client.Player)
				{
					client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Makeleader.AlreadyLeader"), eChatType.CT_System,eChatLoc.CL_SystemWindow);
					return;
				}

			}

			client.Player.Group.MakeLeader(target);
		}
	}
}