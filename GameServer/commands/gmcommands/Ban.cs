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
using System.Linq;
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
		"GMCommands.Ban.Description",
		"GMCommands.Ban.Usage.IP",
		"GMCommands.Ban.Usage.Account",
		"GMCommands.Ban.Usage.Both")]
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
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Ban.MustTarget"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			try
			{
				DatabaseObject[] objs;
				DBBannedAccount b = new DBBannedAccount();
				string accip = ((IPEndPoint)player.Client.Socket.RemoteEndPoint).Address.ToString();
				string accname = player.Client.Account.Name; //TODO: Escape ? 
				string reason;

				if (args.Length >= 3)
					reason = String.Join(" ", args, 2, args.Length - 2);
				else reason = "No Reason.";

				switch (args[1].ToLower())
				{
					#region Account
					case "account":
						objs = (DatabaseObject[])(from s in DatabaseLayer.Instance.OfType<DBBannedAccount>() 
                                where (s.Type == "A" || s.Type == "B") && s.Account ==accname
                                    select s);
						if (objs.Length > 0)
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
                        objs = (DatabaseObject[])(from s in DatabaseLayer.Instance.OfType<DBBannedAccount>()
                               where (s.Type == "B" || s.Type == "I") && s.Ip == accip
                               select s);
						if (objs.Length > 0)
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
                        objs = (DatabaseObject[])(from s in DatabaseLayer.Instance.OfType<DBBannedAccount>()
                                where (s.Type == "B" || s.Type == "A") && s.Account == accname && s.Ip == accip
                                select s);
						if (objs.Length > 0)
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
							break;
						}
					#endregion Default
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