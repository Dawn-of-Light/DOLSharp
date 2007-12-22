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
		"&disband",
		ePrivLevel.Player,
		"Disband from a group", "/disband")]
	public class DisbandCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (client.Player.Group == null)
			{
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Disband.NotInGroup"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}

			if (args.Length < 2)//disband myslef
			{
				client.Player.Group.RemoveMember(client.Player);
				return 1;
			}
			else//disband by name
			{
				if (client.Player.Group.Leader != client.Player)
				{
					client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Disband.NotLeader"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 1;
				}

				string name = args[1];

				if (name == client.Player.Name)
				{
					client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Disband.NoYourself"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 1;
				}

				int startCount = client.Player.Group.MemberCount;

				foreach (GameLiving living in client.Player.Group.GetMembersInTheGroup())
				{
					if (living.Name == name)
						client.Player.Group.RemoveMember(living);
				}

				//no target found to remove
				if (client.Player.Group.MemberCount == startCount)
				{
					client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Disband.NoPlayer"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 1;
				}
			}

			return 0;
		}
	}
}
