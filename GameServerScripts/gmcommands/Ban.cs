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
using DOL.Database;
using DOL.GS.PacketHandler;
using log4net;
using NHibernate.Expression;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&ban",
		(uint) ePrivLevel.GM,
		"Ban an account",
		"/ban <day> <hour> <minute> [reason]",
		"/ban <accountName> <day> <hour> <minute> [reason]",
		"/ban status <accountName>")]
	public class BanCommandHandler : ICommandHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int OnCommand(GameClient client, string[] args)
		{
			try
			{
				if (args.Length < 3)
				{
					showCommandInformations(client);
					return 0;
				}
				if(args[1] == "status")
				{
					Account account = (Account) GameServer.Database.SelectObject(typeof (Account), Expression.Eq("AccountName", args[2]));
					if(account != null)
					{	
						TimeSpan durationLeft = (account.LastLogin.Add(account.BanDuration)).Subtract(DateTime.Now);
						if(durationLeft.CompareTo(TimeSpan.Zero) > 0)
						{
							client.Out.SendMessage("This account has been banned by "+account.BanAuthor+" for the reason "+account.BanReason+".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							client.Out.SendMessage("The ban will expire in "+durationLeft.Days+" days "+durationLeft.Hours+" hours "+durationLeft.Minutes+" minutes.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						else
						{
							client.Out.SendMessage("This account is not banned.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}	
					}
					else
					{
						client.Out.SendMessage("The account "+account.AccountName+" does not exist.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					return 1;
				}

				string reason= "No special reason";
				Account accountToBan;
				TimeSpan banDuration;

				GamePlayer player = client.Player.TargetObject as GamePlayer;
				if(player != null) // target selected
				{
					if(args.Length < 4)
					{
						showCommandInformations(client);
						return 0;
					}

					if (args.Length >= 5) reason = args[4];
					
					accountToBan = player.Client.Account;

					// Kick player
					player.Client.Out.SendPlayerQuit(true);
					GameServer.Instance.Disconnect(player.Client);

					banDuration =  new TimeSpan(Convert.ToInt32(args[1]), Convert.ToInt32(args[2]), Convert.ToInt32(args[3]), 0 , 0);
				}
				else
				{
					if(args.Length < 5)
					{
						showCommandInformations(client);
						return 0;
					}

					if (args.Length >= 6) reason = args[5];

					//Seek players ingame first
					GameClient clientToBan = WorldMgr.GetClientByAccountName(args[1]);
					if (clientToBan != null)
					{
						accountToBan = clientToBan.Account;
						
						// Kick player
						clientToBan.Out.SendPlayerQuit(true);
						GameServer.Instance.Disconnect(clientToBan);
					}
					else
					{
						//Get database object
						accountToBan = (Account) GameServer.Database.SelectObject(typeof (Account), Expression.Eq("AccountName", args[1]));
					}
					
					banDuration =  new TimeSpan(Convert.ToInt32(args[2]), Convert.ToInt32(args[3]), Convert.ToInt32(args[4]), 0 , 0);
				}

				if(accountToBan == null)
				{
					client.Out.SendMessage("The account to ban has not been found.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					return 0;
				}

				accountToBan.BanDuration = banDuration;
				accountToBan.BanAuthor = client.Player.Name;
				accountToBan.BanReason = reason;
		
				GameServer.Database.SaveObject(accountToBan);

				client.Out.SendMessage("Account " + accountToBan.AccountName + " banned.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("/ban Exception", e);
				showCommandInformations(client);
			}
			return 1;
		}

		private void showCommandInformations(GameClient client)
		{
			client.Out.SendMessage("Usage of /ban :", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage("/ban <day> <hour> <minute> [reason] : ban the account of the selected player.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage("/ban <accountName> <day> <hour> <minute> [reason] : ban the account with the given name.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage("/ban status <accountName> : view the ban status of account with the given name.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}			
	}
}