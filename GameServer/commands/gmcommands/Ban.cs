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
using DOL.Database2;
using DOL.GS.PacketHandler;
using DOL.Language;
using log4net;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&ban",
		ePrivLevel.GM,
		"Usage of /ban command :",
		"/ban ip [reason] : Ban target's IP adress",
		"/ban account [reason] : Ban target's account",
		"/ban both [reason] : Ban target's account and its related IP adress")]
	public class BanCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 2)
			{
				DisplaySyntax(client);
				return;
			}

			GamePlayer player = client.Player.TargetObject as GamePlayer;
			if (player == null)
			{
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.GM.Ban.MustTarget"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			try
			{
				DatabaseObject[] objs;
				DBBannedAccount b = new DBBannedAccount();
				string accip = ((IPEndPoint)player.Client.Socket.RemoteEndPoint).Address.ToString();
				string accname = GameServer.Database.Escape(player.Client.Account.Name);
				string reason;

				if (args.Length >= 3)
					reason = String.Join(" ", args, 2, args.Length - 2);
				else reason = "No Reason.";

				switch (args[1].ToLower())
				{
					case "account":
						objs = GameServer.Database.SelectObjects(typeof(DBBannedAccount), "((Type='A' OR Type='B') AND Account ='" + GameServer.Database.Escape(accname) + "')");
						if (objs.Length > 0)
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.GM.Ban.AAlreadyBanned"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
							return;
						}

						b.Type = "A";
						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.GM.Ban.ABanned", accname), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
						break;

					case "ip":
						objs = GameServer.Database.SelectObjects(typeof(DBBannedAccount), "((Type='I' OR Type='B') AND Ip ='" + GameServer.Database.Escape(accip) + "')");
						if (objs.Length > 0)
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.GM.Ban.IAlreadyBanned"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
							return;
						}

						b.Type = "I";
						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.GM.Ban.IBanned", accip), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
						break;

					case "both":
						objs = GameServer.Database.SelectObjects(typeof(DBBannedAccount), "Type='B' AND Account ='" + GameServer.Database.Escape(accname) + "' AND Ip ='" + GameServer.Database.Escape(accip) + "'");
						if (objs.Length > 0)
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.GM.Ban.BAlreadyBanned"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
							return;
						}

						b.Type = "B";
						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.GM.Ban.BBanned", accname, accip), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
						break;

					default:
						{
							DisplaySyntax(client);
							break;
						}
				}

				b.Author = client.Player.PlayerCharacter.Name;
				b.Ip = accip;
				b.Account = accname;
				b.DateBan = DateTime.Now;
				b.Reason = reason;
				GameServer.Database.AddNewObject(b);
				GameServer.Database.SaveObject(b);

				if (log.IsInfoEnabled)
					log.Info("Ban added [" + args[1].ToLower() + "]: " + accname + "(" + accip + ")");
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