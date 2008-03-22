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
using DOL.Language;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&plvl",
		ePrivLevel.Admin,
		"AdminCommands.plvl.Description",
		"AdminCommands.plvl.Usage",
		"AdminCommands.plvl.Usage.Single",
		"AdminCommands.plvl.Usage.Remove")]
	public class PlvlCommand : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 2)
			{
				DisplaySyntax(client);
				return;
			}

			uint plvl;
			switch (args[1])
			{
				#region Single
				case "single":
					{
						GamePlayer target = client.Player.TargetObject as GamePlayer;
						if (target == null)
						{
							DisplayMessage(client, LanguageMgr.GetTranslation(client, "AdminCommands.plvl.NoSelectPlayer"));
							return;
						}
						if (args.Length != 3)
						{
							DisplaySyntax(client);
							return;
						}
						SinglePermission.setPermission(target, args[2]);
						DisplayMessage(client, LanguageMgr.GetTranslation(client, "AdminCommands.plvl.AddedSinglePermission", target.Name, args[2]));
						break;
					}
				#endregion Single
				#region Remove
				case "remove":
					{
						GamePlayer player = client.Player.TargetObject as GamePlayer;
						if (player == null)
						{
							DisplayMessage(client, LanguageMgr.GetTranslation(client, "AdminCommands.plvl.NoSelectPlayer"));
							return;
						}
						if (args.Length != 3)
						{
							DisplaySyntax(client);
							return;
						}
						if (SinglePermission.removePermission(player, args[2]))
							DisplayMessage(client, LanguageMgr.GetTranslation(client, "AdminCommands.plvl.RemoveSinglePermission", player.Name, args[2]));
						else
							DisplayMessage(client, LanguageMgr.GetTranslation(client, "AdminCommands.plvl.NoPermissionForCommand", player.Name, args[2]));
						break;
					}
				#endregion Remove
				#region Default
				default:
					{
						try
						{
							plvl = Convert.ToUInt16(args[1]);

							GamePlayer target = client.Player.TargetObject as GamePlayer;
							if (target == null)
							{
								target = client.Player;
							}

							if (target != null)
							{
								if (target.Client.Account != null)
								{
									target.Client.Account.PrivLevel = plvl;
									GameServer.Database.SaveObject(target.Client.Account);
									foreach (GameNPC npc in client.Player.GetNPCsInRadius(WorldMgr.VISIBILITY_DISTANCE))
									{
										if ((npc.Flags & (int)GameNPC.eFlags.CANTTARGET) != 0 || (npc.Flags & (int)GameNPC.eFlags.DONTSHOWNAME) != 0)
										{
											client.Out.SendNPCCreate(npc);
										}
									}
									target.Client.Out.SendMessage(LanguageMgr.GetTranslation(client, "AdminCommands.plvl.YourPlvlHasBeenSetted", plvl.ToString()), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
									if (target != client.Player)
										client.Out.SendMessage(LanguageMgr.GetTranslation(client, "AdminCommands.plvl.PlayerPlvlHasBeenSetted", target.Name, plvl.ToString()), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
								}
							}
						}
						catch (Exception)
						{
							DisplaySyntax(client);
						}
						break;
					}
				#endregion Default
			}
		}
	}
}