/*
 * Author:		Nocto
 * Mod:			Kristopher Gilbert <ogre@fallenrealms.net>
 * Rev:			$Id: password.cs,v 1.3 2005/09/03 12:02:36 noret Exp $
 * Copyright:	2004 by Hired Goons, LLC
 * License:		http://www.gnu.org/licenses/gpl.txt
 * 
 * Implements the /password command which allows players to change the password
 * associated with their account.
 * 
 */

using System;
using DOL.GS.PacketHandler;
using DOL.GS.PacketHandler.Client.v168;

namespace DOL.GS.Commands
{
	[Cmd("&password", ePrivLevel.Player,
		"Changes your account password",
		"/password <current_password> <new_password>")]
	public class PasswordCommand : AbstractCommandHandler, ICommandHandler
	{
		#region ICommandHandler Members

		public void OnCommand(GameClient client, string[] args)
		{
			string usage = "Usage: /password <current_password> <new_password>";

			if (args.Length < 3)
			{
				client.Out.SendMessage(usage, eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			try
			{
				string oldPassword = args[1];
				string newPassword = args[2];

				if ((client.Account != null) && (LoginRequestHandler.CryptPassword(oldPassword) == client.Account.Password))
				{
					// TODO: Add confirmation dialog
					// TODO: If user has set their email address, mail them the change notification
					client.Player.TempProperties.setProperty(this, newPassword);
					client.Out.SendCustomDialog("Do you wish to change your password to \n" + newPassword, PasswordCheckCallback);
				}
				else
				{
					client.Out.SendMessage("Your current password was incorrect.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);

					if (log.IsInfoEnabled)
						log.Info(client.Player.Name + " (" + client.Account.Name + ") attempted to change password but failed!");

					return;
				}
			}
			catch (Exception)
			{
				client.Out.SendMessage(usage, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}

		#endregion

		private void PasswordCheckCallback(GamePlayer player, byte response)
		{
			if (response != 0x01)
				return;

			var newPassword = player.TempProperties.getProperty<string>(this, null);
			if (newPassword == null)
				return;

			player.TempProperties.removeProperty(this);
			player.Out.SendMessage("Your password has been changed.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			player.Client.Account.Password = LoginRequestHandler.CryptPassword(newPassword);

			GameServer.Database.SaveObject(player.Client.Account);

			// log change
			AuditMgr.AddAuditEntry(player, AuditType.Account, AuditSubtype.AccountPasswordChange, "", player.Name);

			if (log.IsInfoEnabled)
				log.Info(player.Name + " (" + player.Client.Account.Name + ") changed password.");
		}
	}
}