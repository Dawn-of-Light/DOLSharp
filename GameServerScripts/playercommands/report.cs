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
using DOL.GS.GameEvents;
using DOL.Database;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&report",
		(uint)ePrivLevel.Player,
		"Reports a bug",
		"Usage: /report <message>")]
	public class ReportCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (ServerProperties.Properties.DISABLE_BUG_REPORTS)
			{
				DisplayError(client, "Bug reporting has been disabled for this server!", new object[] { });
				return 1;
			}

			if (args.Length < 2)
			{
				DisplaySyntax(client);
				return 1;
			}

			string message = string.Join(" ", args, 1, args.Length - 1);
			BugReport report = new BugReport();
			report.ID = GameServer.Database.GetObjectCount(typeof(BugReport)) + 1;
			report.Message = message;
			report.Submitter = client.Player.Name + " [" + client.Account.Name + "]";
			GameServer.Database.AddNewObject(report);
			client.Player.Out.SendMessage("Report submitted, if this is not a bug report it will be ignored!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

			return 1;
		}
	}
}