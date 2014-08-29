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
using System.Reflection;
using System.Collections.Generic;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.Language;
using log4net;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&ban",
		ePrivLevel.Admin,
		"GMCommands.Ban.Description",
		"GMCommands.Ban.Usage.IP",
		"GMCommands.Ban.Usage.Account",
		"GMCommands.Ban.Usage.Both",
		"GMCommands.Ban.Usage.Sharp",
		"GMCommands.Ban.Usage.Duration"
	)]
	public class BanCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		private static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 3)
			{
				DisplaySyntax(client);
				return;
			}

			GameClient gc = null;

			if (args[2].StartsWith("#"))
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
				gc = WorldMgr.GetClientByPlayerName(args[2], false, false);
			}

			Account acc = gc != null ? gc.Account : GameServer.Database.SelectObject<Account>("Name LIKE '" + GameServer.Database.Escape(args[2]) + "'");
			if (acc == null)
			{
				client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Ban.UnableToFindPlayer"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (client.Account.PrivLevel < acc.PrivLevel)
			{
				DisplayMessage(client, "Your privlevel is not high enough to ban this player.");
				return;
			}

			if (client.Account.Name == acc.Name)
			{
				DisplayMessage(client, "Your can't ban yourself!");
				return;
			}

			try
			{
				DBBannedAccount b = new DBBannedAccount
				                    {
				                    	DateBan = DateTime.Now,
				                    	Author = client.Player.DBCharacter.Name,
				                    	Ip = acc.LastLoginIP,
				                    	Account = acc.Name,
				                    	Unbanned = false
				                    };
				
				if (args.Length >= 4) {
					int b_dur = 0;
					if (args[3].StartsWith("-d")) {
						try {
							b.BanDuration = Convert.ToUInt16(args[3].Replace("-d", ""));
							b_dur = 1/b.BanDuration;
						}
						catch
						{
							DisplayMessage(client, "Invalid ban duration");
							return;
						}
						b_dur = 1;
					} else {
						b.BanDuration = -1;
					}
					
					b.Reason = String.Join(" ", args, 3 + b_dur, args.Length - (3 + b_dur));
					
					if (b_dur > 0 && args.Length == 4)
					{
						b.Reason = "No Reason.";
					}
				} else {
					b.Reason = "No Reason.";
					b.BanDuration = -1;
				}
				
				switch (args[1].ToLower())
				{
						#region Account
					case "account":
						IList<DBBannedAccount> acctBans;
						if (GameServer.Database is Database.Handlers.SQLiteObjectDatabase)
						{
							acctBans = GameServer.Database.SelectObjects<DBBannedAccount>("((Type='A' OR Type='B') AND Account ='" + GameServer.Database.Escape(acc.Name) + "' AND (datetime(DateBan, '+' || BanDuration || ' days') > datetime('now','localhost') OR BanDuration < 0) AND Unbanned = 0)");
						}
						else
						{
							acctBans = GameServer.Database.SelectObjects<DBBannedAccount>("((Type='A' OR Type='B') AND Account ='" + GameServer.Database.Escape(acc.Name) + "' AND ((DateBan + INTERVAL BanDuration DAY) > NOW() OR BanDuration < 0) AND Unbanned = 0)");
						}
						
						if (acctBans.Count > 0)
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Ban.AAlreadyBanned"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
							return;
						}

						b.Type = "A";
						if (b.BanDuration > 0)
							client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Ban.ABannedTemp", acc.Name, b.BanDuration), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
						else
							client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Ban.ABannedPerma", acc.Name), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
						break;
						#endregion Account
						#region IP
					case "ip":
						IList<DBBannedAccount> ipBans;
						if (GameServer.Database is Database.Handlers.SQLiteObjectDatabase)
						{
							ipBans = GameServer.Database.SelectObjects<DBBannedAccount>("((Type='I' OR Type='B') AND Ip ='" + GameServer.Database.Escape(acc.LastLoginIP) + "' AND (datetime(DateBan, '+' || BanDuration || ' days') > datetime('now','localhost') OR BanDuration < 0)  AND Unbanned = 0)");
						}
						else
						{
							ipBans = GameServer.Database.SelectObjects<DBBannedAccount>("((Type='I' OR Type='B') AND Ip ='" + GameServer.Database.Escape(acc.LastLoginIP) + "' AND ((DateBan + INTERVAL BanDuration DAY) > NOW() OR BanDuration < 0)  AND Unbanned = 0)");
						}
						if (ipBans.Count > 0)
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Ban.IAlreadyBanned"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
							return;
						}

						b.Type = "I";
						if (b.BanDuration > 0)
							client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Ban.IBannedTemp", acc.LastLoginIP, b.BanDuration), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
						else
							client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Ban.IBannedPerma", acc.LastLoginIP), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
						break;
						#endregion IP
						#region Both
					case "both":
						IList<DBBannedAccount> acctIpBans;
						if (GameServer.Database is Database.Handlers.SQLiteObjectDatabase)
						{
							acctIpBans = GameServer.Database.SelectObjects<DBBannedAccount>("Type='B' AND Account ='" + GameServer.Database.Escape(acc.Name) + "' AND Ip ='" + GameServer.Database.Escape(acc.LastLoginIP) + "' AND (datetime(DateBan, '+' || BanDuration || ' days') > datetime('now','localhost') OR BanDuration < 0) AND Unbanned = 0");
						}
						else
						{
							acctIpBans = GameServer.Database.SelectObjects<DBBannedAccount>("Type='B' AND Account ='" + GameServer.Database.Escape(acc.Name) + "' AND Ip ='" + GameServer.Database.Escape(acc.LastLoginIP) + "' AND ((DateBan + INTERVAL BanDuration DAY) > NOW() OR BanDuration < 0) AND Unbanned = 0");
						}
						if (acctIpBans.Count > 0)
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Ban.BAlreadyBanned"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
							return;
						}

						b.Type = "B";
						if (b.BanDuration > 0)
							client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Ban.BBannedTemp", acc.Name, acc.LastLoginIP, b.BanDuration), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
						else
							client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Ban.BBannedPerma", acc.Name, acc.LastLoginIP), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
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
				GameServer.Database.AddObject(b);

				if (log.IsInfoEnabled)
				{
					if (b.BanDuration > 0)
						log.Info("Temporary Ban added [" + args[1].ToLower() + "]: " + acc.Name + "(" + acc.LastLoginIP + ") for " + b.BanDuration + "days");
					else
						log.Info("Permanent Ban added [" + args[1].ToLower() + "]: " + acc.Name + "(" + acc.LastLoginIP + ")");
				}
				return;
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("/ban Exception", e);
			}

			// if not returned here, there is an error
			DisplaySyntax(client);
		}
	}
}