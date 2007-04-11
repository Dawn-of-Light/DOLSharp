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
using DOL.GS.Housing;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&housefriend",
		(uint)ePrivLevel.Player,
		"Invite a specified player to hour house", "/housefriend player <player>")]
	public class HousefriendCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (args.Length == 1)
				return 1;
			if (!client.Player.InHouse)
				return 1;
			
			switch (args[1])
			{
				case "player":
					{
						if (args.Length == 2)
							return 1;
						if (client.Player.Name == args[2])
							return 1;
						GameClient targetClient = WorldMgr.GetClientByPlayerNameAndRealm(args[2], 0, true);
						if (targetClient == null)
						{
							client.Out.SendMessage("No players online with that name.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						if (client.Player.CurrentHouse.AddToPerm(targetClient.Player, ePermsTypes.Player, 1))
							client.Out.SendMessage("You added " + targetClient.Player.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						break;
					}
                case "all":
                    {
                        if (client.Player.CurrentHouse.AddToPerm(null, ePermsTypes.All, 1))
                            client.Out.SendMessage("You added Everybody !", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        break;
                    }
			}

			return 1;
		}
	}
}