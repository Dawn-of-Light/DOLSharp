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
using System.Net;
using System.Reflection;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.Language;
using log4net;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&ban",
		ePrivLevel.GM,
		"GMCommands.Ban.Description",
		"GMCommands.Ban.Usage.IP",
		"GMCommands.Ban.Usage.Account",
		"GMCommands.Ban.Usage.Both",
		"#<ClientID> can be used in place of player name.  Use /clientlist to see playing clients."
	)]
	public class BanCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 3)
			{
				DisplaySyntax(client);
				return;
			}

			GameClient gc = null;

			if (args[1].StartsWith("#"))
			{
				try
				{
					int sessionID = Convert.ToInt32(args[1].Substring(1));
					gc = WorldMgr.GetClientFromID(sessionID);
				}
				catch
				{
					DisplayMessage(client, "Invalid client ID");
				}
			}
			else
			{
				gc = WorldMgr.GetClientByPlayerName(args[1], false, false);
			}

			if (gc == null)
			{
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Ban.UnableToFindPlayer"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (client.Account.PrivLevel < gc.Account.PrivLevel)
			{
				DisplayMessage(client, "Your privlevel is not high enough to ban this player.");
				return;
			}

			if (client == gc)
			{
				DisplayMessage(client, "Your can't ban yourself!");
				return;
			}

			try
			{
				DBBannedAccount b = new DBBannedAccount();
				string accip = gc.TcpEndpointAddress;
				string accname = GameServer.Database.Escape(gc.Account.Name);
				string reason;

				if (args.Length >= 4)
					reason = String.Join(" ", args, 2, args.Length - 2);
				else reason = "No Reason.";

				switch (args[1].ToLower())
				{
						#region Account
					case "account":
						var acctBans = GameServer.Database.SelectObjects<DBBannedAccount>("((Type='A' OR Type='B') AND Account ='" + GameServer.Database.Escape(accname) + "')");
						if (acctBans.Count > 0)
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Ban.AAlreadyBanned"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
							return;
						}

						b.Type = "A";
						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Ban.ABanned", accname), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
						break;
						#endregion Account
						#region IP
					case "ip":
						var ipBans = GameServer.Database.SelectObjects<DBBannedAccount>("((Type='I' OR Type='B') AND Ip ='" + GameServer.Database.Escape(accip) + "')");
						if (ipBans.Count > 0)
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Ban.IAlreadyBanned"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
							return;
						}

						b.Type = "I";
						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Ban.IBanned", accip), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
						break;
						#endregion IP
						#region Both
					case "both":
						var acctIpBans = GameServer.Database.SelectObjects<DBBannedAccount>("Type='B' AND Account ='" + GameServer.Database.Escape(accname) + "' AND Ip ='" + GameServer.Database.Escape(accip) + "'");
						if (acctIpBans.Count > 0)
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Ban.BAlreadyBanned"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
							return;
						}

						b.Type = "B";
						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Ban.BBanned", accname, accip), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
						break;
						#endregion Both
						#region Default
					default:
						{
							DisplaySyntax(client);
							return;
						}
						#endregion Default
				}

				b.Author = client.Player.DBCharacter.Name;
				b.Ip = accip;
				b.Account = accname;
				b.DateBan = DateTime.Now;
				b.Reason = reason;
				GameServer.Database.AddObject(b);
				GameServer.Database.SaveObject(b);

				if (Log.IsInfoEnabled)
					Log.Info("Ban added [" + args[1].ToLower() + "]: " + accname + "(" + accip + ")");
				return;
			}
			catch (Exception e)
			{
				if (Log.IsErrorEnabled)
					Log.Error("/ban Exception", e);
			}

			// if not returned here, there is an error
			DisplaySyntax(client);
		}
	}
}