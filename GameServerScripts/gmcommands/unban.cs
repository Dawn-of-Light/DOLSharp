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
		 "&unban",
		 (uint) ePrivLevel.GM,
		 "Unban an account",
		 "/unban <accountName>")]
	public class UnbanCommandHandler : ICommandHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int OnCommand(GameClient client, string[] args)
		{
			try
			{
				Account accountToUnban = (Account) GameServer.Database.SelectObject(typeof (Account), Expression.Eq("AccountName", args[1]));
				if(accountToUnban != null)
				{
					if(accountToUnban.BanDuration > TimeSpan.Zero)
					{
						accountToUnban.BanDuration = TimeSpan.Zero;
					
						GameServer.Database.SaveObject(accountToUnban);

						client.Out.SendMessage("[Unban] Ban lifted for account '" + args[1] + "'", eChatType.CT_Group, eChatLoc.CL_ChatWindow);
					}
					else
					{
						client.Out.SendMessage("[Unban] The account '" + args[1] + "' is no longer banned!", eChatType.CT_Group, eChatLoc.CL_ChatWindow);
					}
				}
				else
				{
					client.Out.SendMessage("[Unban] No such account found '" + args[1] + "'", eChatType.CT_Group, eChatLoc.CL_ChatWindow);
				}
			}
			catch(Exception e)
			{
			}
			return 1;
		}	
		
	}
}