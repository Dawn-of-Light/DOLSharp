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
		"&invite",
		ePrivLevel.Player,
		"Invite a specified or targeted player to join your group", "/invite <player>")]
	public class InviteCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (client.Player.Group != null && client.Player.Group.Leader != client.Player)
			{
                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Invite.NotLeader"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
			}

			if (IsSpammingCommand(client.Player, "invite"))
				return;

			string targetName = string.Join(" ", args, 1, args.Length - 1);
			GamePlayer target;

			if (args.Length < 2)
			{ // Inviting by target
				if (client.Player.TargetObject == null || client.Player.TargetObject == client.Player)
				{
                    client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Invite.TargetInvalid"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return;
				}

				if (!(client.Player.TargetObject is GamePlayer))
				{
                    client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Invite.TargetInvalid"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return;
				}
				target = (GamePlayer) client.Player.TargetObject;
				if (!GameServer.ServerRules.IsAllowedToGroup(client.Player, target, false))
				{
					return;
				}
			}
			else
			{ // Inviting by name
				GameClient targetClient = WorldMgr.GetClientByPlayerNameAndRealm(targetName, 0, true);
				if (targetClient == null)
					target = null;
				else
					target = targetClient.Player;
				if (target == null || !GameServer.ServerRules.IsAllowedToGroup(client.Player, target, true))
				{ // Invalid target or realm restriction
                    client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Invite.NotFound"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return;
				}
				if (target == client.Player)
				{
                    client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Invite.Yourself"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return;
				}
			}

			if (target.Group != null)
			{
                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Invite.PlayerHasGroup"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
			}

			if (GameServer.Instance.Configuration.ServerType == eGameServerType.GST_PvP &&
				target.IsStealthed)
			{
                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Invite.NotAround"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
			}

			if (client.Account.PrivLevel > target.Client.Account.PrivLevel)
			{
				// you have no choice!

				if (client.Player.Group == null)
				{
					Group group = new Group(client.Player);
					GroupMgr.AddGroup(group);
					group.AddMember(client.Player);
					group.AddMember(target);
				}
				else
				{
					client.Player.Group.AddMember(target);
				}

                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Invite.GMAdded", target.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                target.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Invite.AddedByGM", client.Player.Name, client.Player.GetPronoun(1, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
			else
			{
                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Invite.Invite", target.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                target.Out.SendGroupInviteCommand(client.Player, LanguageMgr.GetTranslation(client, "Scripts.Players.Invite.Invited", client.Player.Name, client.Player.GetPronoun(1, false)));
                target.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Invite.HasInvited", client.Player.Name, client.Player.GetPronoun(1, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
		}
	}
}