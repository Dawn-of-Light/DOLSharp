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
using DOL.Database;
using NHibernate.Expression;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&account",
		(uint) ePrivLevel.Admin,
		"Account modification command",
		"/account changepassword <AccountName> <NewPassword>",
		"/account delete <AccountName>",
		"/account deletecharacter <CharacterName>",
		"/account getaccountname <CharacterName>")]
	public class AccountCommand : AbstractCommandHandler, ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			//Test for parameters, if none provided, display the syntax
			if (args.Length < 2)
				return DisplaySyntax(client);

			switch (args[1])
			{
					//Change the password for an account
				case "changepassword":
					{
						//Test correct parameters for password change
						if (args.Length < 4)
							return DisplaySyntax(client);

						string accountname = args[2];
						string newpass = args[3];

						//Test if the account can be found
						Account acc = GetAccount(accountname);
						if (acc == null)
							return DisplayError(client, "Account {0} not found!", accountname);

						//Set new password
						acc.Password = newpass;
						//Save the changed account
						GameServer.Database.SaveObject(acc);

					}
					break;

					//Delete an account
				case "delete":
					{
						//Test correct parameters 
						if (args.Length < 3)
							return DisplaySyntax(client);

						//Test if the account can be found
						string accountname = args[2];
						Account acc = GetAccount(accountname);
						if (acc == null)
							return DisplayError(client, "Account {0} not found!", accountname);

						//Kick the player if active
						KickAccount(acc);

						//Delete the account object
						GameServer.Database.DeleteObject(acc);
						return DisplayMessage(client, "Account {0} deleted!", acc.AccountName);
					}

					//Delete a character from an account
				case "deletecharacter":
					{
						//Test correct parameters 
						if (args.Length < 3)
							return DisplaySyntax(client);

						string charname = args[2];
						GamePlayer cha = GetCharacter(charname);
						if (cha == null)
							return DisplayError(client, "Character {0} not found!", charname);

						//Kick the player if active
						KickCharacter(cha);

						//Delete the character object
						GameServer.Database.DeleteObject(cha);
						return DisplayMessage(client, "Character {0} deleted!", cha.Name);
					}

				case "getaccountname":
					{
						//Test correct parameters 
						if (args.Length < 3)
							return DisplaySyntax(client);

						string charname = args[2];
						GamePlayer cha = GetCharacter(charname);
						if (cha == null)
							return DisplayError(client, "Character {0} not found!", charname);

						//Seek players ingame first
						GameClient clien = WorldMgr.GetClientByPlayerName(charname, true);
						if (clien != null)
						{
							DisplayMessage(client, "Account name for character " + cha.Name + " is : " + clien.Account.AccountName);
						}
						else
						{
							//Return database object
							Account acc = (Account) GameServer.Database.SelectObject(typeof (Account), Expression.Eq("AccountId",cha.AccountId));
							if (acc != null)
								DisplayMessage(client, "Account name for character " + cha.Name + " is : " + acc.AccountName);
							
						}

						return DisplayMessage(client, "Account name for character " + cha.Name + " not found!");
					}
			}
			return 1;
		}

		/// <summary>
		/// Loads an account
		/// </summary>
		/// <param name="name">the accountname</param>
		/// <returns>the found account or null</returns>
		private Account GetAccount(string name)
		{
			//Seek players ingame first
			GameClient client = WorldMgr.GetClientByAccountName(name);
			if (client != null)
				return client.Account;

			//Return database object
			return (Account) GameServer.Database.SelectObject(typeof (Account), Expression.Eq("AccountName", name));
		}

		/// <summary>
		/// Returns a given character
		/// </summary>
		/// <param name="charname">the charactername</param>
		/// <returns>the found character or null</returns>
		private GamePlayer GetCharacter(string charname)
		{
			//Seek players ingame first
			GameClient client = WorldMgr.GetClientByPlayerName(charname, true);
			if (client != null)
				return client.Player;

			//Return database object
			return (GamePlayer) GameServer.Database.SelectObject(typeof (GamePlayer), Expression.Eq("Name", charname));
		}

		/// <summary>
		/// Kicks an active playing account from the server
		/// </summary>
		/// <param name="acc">The account</param>
		private void KickAccount(Account acc)
		{
			//Check if the player is online
			GameClient playingclient = WorldMgr.GetClientByAccountName(acc.AccountName);
			if (playingclient != null)
			{
				//IF the player is online, kick him out
				playingclient.Out.SendPlayerQuit(true);
				GameServer.Instance.Disconnect(playingclient);
			}
		}

		/// <summary>
		/// Kicks an active playing character from the server
		/// </summary>
		/// <param name="cha">the character</param>
		private void KickCharacter(GamePlayer cha)
		{
			//Check if the player is online
			if (cha.Client != null)
			{
				//IF the player is online, kick him out
				cha.Client.Out.SendPlayerQuit(true);
				GameServer.Instance.Disconnect(cha.Client);
			}
		}
	}
}