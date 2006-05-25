/*
 * Author:		Nocto
 * Mod:			Kristopher Gilbert <ogre@fallenrealms.net>
 * Rev:			$Id: password.cs,v 1.4 2005/11/19 20:04:40 doulbousiouf Exp $
 * Copyright:	2004 by Hired Goons, LLC
 * License:		http://www.gnu.org/licenses/gpl.txt
 * 
 * Implements the /password command which allows players to change the password
 * associated with their account.
 * 
 */

using System;
using System.Reflection;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.PacketHandler.v168;
using log4net;

namespace DOL.GS.Scripts
{
	[CmdAttribute("&password", (int)ePrivLevel.Player,
		"Changes your account password",
		"/password <current_password> <new_password>")]
	public class PasswordCommand : ICommandHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int OnCommand(GameClient client, string[] args)
		{
			string usage = "Usage: /password <current_password> <new_password>";
			
			if (args.Length < 3)
			{
				client.Out.SendMessage(usage, eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}
			try
			{
				string old_password = args[1];
				string new_password = args[2];
				if ((client.Account != null) && (LoginRequestHandler.CryptPassword(old_password) == client.Account.Password))
				{
					// TODO: Add confirmation dialog
					// TODO: If user has set their email address, mail them the change notification
					client.Player.TempProperties.setProperty(this, new_password);
					client.Out.SendCustomDialog("Do you wish to change your password to \n" + new_password, new CustomDialogResponse(PasswordCheckCallback));
				}
				else
				{
					client.Out.SendMessage("Your current password was incorrect.",
						eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					if (log.IsInfoEnabled)
						log.Info(client.Player.Name + " (" + client.Account.AccountName +
								") attempted to change password but failed!");
					return 0;
				}
			}
			catch (Exception)
			{
				client.Out.SendMessage(usage, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			return 1;
		}

		public void PasswordCheckCallback(GamePlayer player, byte response)
		{
			if (response != 0x01) return;
			string newPassword = (string)player.TempProperties.getObjectProperty(this, null);
			if (newPassword == null) return;
			player.TempProperties.removeProperty(this);
			player.Out.SendMessage("Your password has been changed.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			player.Client.Account.Password = LoginRequestHandler.CryptPassword(newPassword);
			GameServer.Database.SaveObject(player.Client.Account);
			if (log.IsInfoEnabled)
				log.Info(player.Name + " (" + player.Client.Account.AccountName + ") changed password.");
		}
	}
}
