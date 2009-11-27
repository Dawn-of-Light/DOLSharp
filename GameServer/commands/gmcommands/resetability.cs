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
using DOL.GS.PacketHandler;
using DOL.GS.RealmAbilities;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&resetability",
		ePrivLevel.GM,
		"/resetability - <self|target|group|cg|bg>")]
	
	public class ResetAbilityCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			GamePlayer target = client.Player.TargetObject as GamePlayer;
			BattleGroup bg = (BattleGroup)client.Player.TempProperties.getObjectProperty(BattleGroup.BATTLEGROUP_PROPERTY, null);
			ChatGroup cg = (ChatGroup)client.Player.TempProperties.getObjectProperty(ChatGroup.CHATGROUP_PROPERTY, null);


			if (args.Length < 2)
			{
				DisplaySyntax(client);
				return;
			}

			switch (args[1].ToLower())
			{
					#region group
				case "group":
					{
						if (target != null)
						{
							if (target.Group != null)
							{
								foreach (GamePlayer groupedplayers in client.Player.Group.GetMembersInTheGroup())
								{

									groupedplayers.ResetDisabledSkills();
									groupedplayers.Out.SendMessage(client.Player.Name +" has reset your ability and spell timers!", eChatType.CT_Spell, eChatLoc.CL_ChatWindow);
								}
							}
						}
						else
							client.Player.ResetDisabledSkills();
						client.Player.Out.SendMessage("Target does not have a group so, ability and spell timers have been reset for you!", eChatType.CT_Spell, eChatLoc.CL_ChatWindow);
						break;
					}
					#endregion

					#region chatgrp
				case "cg":
					{
						if (cg != null)
						{
							foreach (GamePlayer cgplayers in cg.Members.Keys)
							{
								cgplayers.ResetDisabledSkills();
								cgplayers.Out.SendMessage(client.Player.Name + " has reset your ability and spell timers!", eChatType.CT_Spell, eChatLoc.CL_ChatWindow);
							}
						}
						else
							client.Player.ResetDisabledSkills();
						client.Player.Out.SendMessage("Target does not have a chatgroup so, ability and spell timers have been reset for you!", eChatType.CT_Spell, eChatLoc.CL_ChatWindow);
						break;
					}
					#endregion

					#region target
				case "target":
					{
						if (target == null)
							target = (GamePlayer)client.Player;
						target.ResetDisabledSkills();
						target.Out.SendMessage("Your ability and spell timers have been reset!", eChatType.CT_Spell, eChatLoc.CL_ChatWindow);
					}
					break;
					#endregion

					#region self
				case "self":
					{
						client.Player.ResetDisabledSkills();
						client.Player.Out.SendMessage("Your ability and spell timers have been reset!", eChatType.CT_Spell, eChatLoc.CL_ChatWindow);
					}
					break;
					#endregion

					#region battlegroup
				case "bg":
					{
						if (target != null)
						{
							if (bg != null)
							{
								foreach (GamePlayer bgplayers in bg.Members.Keys)
								{
									bgplayers.ResetDisabledSkills();
									bgplayers.Out.SendMessage(client.Player.Name + " has reset your ability and spell timers!", eChatType.CT_Spell, eChatLoc.CL_ChatWindow);
								}
							}
						}
						else
							client.Player.ResetDisabledSkills();
						client.Player.Out.SendMessage("Target does not have a battlegroup so, ability and spell timers have been reset for you!", eChatType.CT_Spell, eChatLoc.CL_ChatWindow);
						break;
					}
					#endregion
				default:
					{
						client.Out.SendMessage("'" + args[1] + "' is not a valid arguement.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					}
					break;
			}
		}
	}
}