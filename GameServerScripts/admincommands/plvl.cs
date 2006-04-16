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

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&plvl",
		(uint) ePrivLevel.Admin,
		"Change privilege level and add single permision to player",
		"/plvl <newPlvl>",
		"/plvl single <command>",
		"/plvl remove <command>")]
	public class PlvlCommand : AbstractCommandHandler, ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (args.Length == 1)
			{
				DisplaySyntax(client);
				return 0;
			}

			uint plvl;
			switch(args[1])
			{
				case "single":
				{
					GamePlayer player = client.Player.TargetObject as GamePlayer;
					if (player == null)
					{
						client.Out.SendMessage("You must select a player to add single permission on him",eChatType.CT_System,eChatLoc.CL_SystemWindow);
						return 0;
					}
					if (args.Length != 3)
					{
						DisplaySyntax(client);
						return 0;
					}
					SinglePermission.setPermission(player,args[2]);
					client.Out.SendMessage("You add the single permission to " + player.Name + "for " + args[2] + " command.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					return 1;
				}
				case "remove":
				{
					GamePlayer player = client.Player.TargetObject as GamePlayer;
					if (player == null)
					{
						client.Out.SendMessage("You must select a player to add single permission on him",eChatType.CT_System,eChatLoc.CL_SystemWindow);
						return 0;
					}
					if (args.Length != 3)
					{
						DisplaySyntax(client);
						return 0;
					}
					if (SinglePermission.removePermission(player,args[2]))
						client.Out.SendMessage("You remove the single permission of " + player.Name + "for " + args[2] + " command.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					else
						client.Out.SendMessage("there is no permission of " + player.Name + "for " + args[2] + " command.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					return 1;
				}
				default:
				{
					try
					{
						plvl = Convert.ToUInt16(args[1]);

						GamePlayer obj = (GamePlayer) client.Player.TargetObject;

						if (obj != null)
						{
							if (obj.Client.Account != null)
							{
								obj.Client.Account.PrivLevel = (ePrivLevel)plvl;
								GameServer.Database.SaveObject(obj.Client.Account);
								foreach (GameNPC npc in client.Player.GetInRadius(typeof(GameNPC), WorldMgr.VISIBILITY_DISTANCE))
								{
									if ((npc.Flags & (int) GameNPC.eFlags.CANTTARGET) != 0 || (npc.Flags & (int) GameNPC.eFlags.DONTSHOWNAME) != 0)
									{
										client.Out.SendNPCCreate(npc);
										//client.Out.SendNPCUpdate(npc); <-- BIG NO NO causes a racing condition!
									}
								}
								obj.Client.Out.SendMessage("Your privilege level has been set to " + plvl.ToString(),
									eChatType.CT_Important,
									eChatLoc.CL_SystemWindow);
							}
						}
					}
					catch (Exception)
					{
						DisplaySyntax(client);
					}
					return 1;
				}
			}
		}
	}
}