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
using log4net;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&ban",
		(uint) ePrivLevel.GM,
		"Ban an account, user name, IP",
		"/ban <type> <ip/pseudo/compte>")]
	public class BanCommandHandler : ICommandHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int OnCommand(GameClient client, string[] args)
		{
			//GamePlayer player = client.Player;
			GamePlayer player = client.Player.TargetObject as GamePlayer;
			if (player == null)
			{
				client.Out.SendMessage("You must select a target!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 0;
			}

			try
			{
				if (args.Length < 2)
				{
					client.Out.SendMessage("Usage of /ban :", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					client.Out.SendMessage("/ban ip <reason> : ban an IP.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					client.Out.SendMessage("/ban account <reason> : ban an account.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					client.Out.SendMessage("/ban account+ip <reason> : ban an account and this IP.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 0;
				}

				string TypeOfBan = args[1];
				DataObject[] objs;
				DBBannedAccount b = new DBBannedAccount();
				string idban = System.Guid.NewGuid().ToString();
				string accip = ((IPEndPoint) player.Client.Socket.RemoteEndPoint).Address.ToString();
				string accname = GameServer.Database.Escape(player.Client.Account.Name);
				string reason;

				if (args.Length >= 3)
					reason = String.Join(" ", args, 2, args.Length - 2);
				else
					reason = "No Reason.";

				switch (TypeOfBan)
				{
					case "account+ip":
						objs = GameServer.Database.SelectObjects(typeof (DBBannedAccount), "Type ='" + GameServer.Database.Escape(TypeOfBan) + "' AND Account ='" + GameServer.Database.Escape(accname) + "' AND Ip ='" + GameServer.Database.Escape(accip) + "'");
						if (objs.Length > 0)
						{
							client.Out.SendMessage("this Account+Ip has already been banned.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
							return 0;
						}

						b.Type = "Account+Ip";
						client.Out.SendMessage("Account " + accname + " and IP +" + accip + " banned.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
						break;

					case "account":
						objs = GameServer.Database.SelectObjects(typeof(DBBannedAccount), "(Type ='Account' AND Account ='" + GameServer.Database.Escape(accname) + "') OR (Type ='Account+Ip' AND Account ='" + GameServer.Database.Escape(accname) + "')");
						if (objs.Length > 0)
						{
							client.Out.SendMessage("this account has already been banned.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
							return 0;
						}

						b.Type = "Account";
						client.Out.SendMessage("Account " + accname + " banned.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
						break;

					case "ip":
						objs = GameServer.Database.SelectObjects(typeof (DBBannedAccount), "(Type ='Ip' AND Ip ='" + GameServer.Database.Escape(accip) + "') OR (Type ='Account+Ip' AND Ip ='" + GameServer.Database.Escape(accip) + "')");
						if (objs.Length > 0)
						{
							client.Out.SendMessage("this IP adress has already been banned.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
							return 0;
						}

						b.Type = "Ip";
						client.Out.SendMessage("IP address " + accip + " banned.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
						break;
				}

				b.IDBan = idban;
				b.Author = client.Player.PlayerCharacter.Name;
				b.Ip = accip;
				b.Account = accname;
				b.DateTime = DateTime.Now.ToString();
				b.Reason = reason;
				GameServer.Database.AddNewObject(b);
				GameServer.Database.SaveObject(b);
				if (log.IsInfoEnabled)
					log.Info("Ban added [" + TypeOfBan + "]: " + accname + "(" + accip + ")");
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("/ban Exception", e);
				client.Out.SendMessage("Exception! Usage:", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("/ban ip <reason> : ban an IP.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("/ban account <reason> : ban an account.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("/ban account+ip <reason> : ban an account and this IP.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			return 1;
		}
	}
}