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
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&account",
		(uint) ePrivLevel.Admin,
		"Account modification command",
		"/account changepassword <AccountName> <NewPassword>",
		"/account delete <AccountName>",
		"/account deletecharacter <CharacterName>",
		"/account movecharacter <CharacterName> <NewAccountName>",
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
						return DisplayMessage(client, "Account {0} deleted!", acc.Name);
					}

					//Delete a character from an account
				case "deletecharacter":
					{
						//Test correct parameters 
						if (args.Length < 3)
							return DisplaySyntax(client);

						string charname = args[2];
						Character cha = GetCharacter(charname);
						if (cha == null)
							return DisplayError(client, "Character {0} not found!", charname);

						//Kick the player if active
						KickCharacter(cha);

						//Delete the character object
						GameServer.Database.DeleteObject(cha);
						return DisplayMessage(client, "Character {0} deleted!", cha.Name);
					}
				case "movecharacter":
					{
						//Test correct parameters 
						if (args.Length < 4)
							return DisplaySyntax(client);

						string charname = args[2];
						string accountname = args[3];

						Character cha = GetCharacter(charname);
						if (cha == null)
							return DisplayError(client, "Character {0} not found!", charname);

						Account acc = GetAccount(accountname);
						if (acc == null)
							return DisplayError(client, "Account {0} not found!", accountname);

						int firstAccountSlot = 0;
						switch ((eRealm) cha.Realm)
						{
							case eRealm.Albion:
								firstAccountSlot = 1*8;
								break;
							case eRealm.Midgard:
								firstAccountSlot = 2*8;
								break;
							case eRealm.Hibernia:
								firstAccountSlot = 3*8;
								break;
							default:
								return DisplayError(client, "The character is not from a valid realm!");
						}

						int freeslot = 0;
						for (freeslot = firstAccountSlot; freeslot < firstAccountSlot + 8; freeslot++)
						{
							bool found = false;
							foreach (Character ch in acc.Characters)
							{
								if (ch.Realm == cha.Realm && ch.AccountSlot == freeslot)
								{
									found = true;
									break;
								}
							}
							if (!found)
								break;

						}

						if (freeslot == 0)
							return DisplayError(client, "Account {0} has no free character slots for this realm anymore!", accountname);

						//Check if the player is online
						GameClient playingclient = WorldMgr.GetClientByPlayerName(cha.Name, true, false);
						if (playingclient != null)
						{
							//IF the player is online, kick him out
							playingclient.Out.SendPlayerQuit(true);
							playingclient.Disconnect();
						}

						cha.AccountName = acc.Name;
						cha.AccountSlot = freeslot;

						GameServer.Database.SaveObject(cha);
						return DisplayMessage(client, "Character {0} moved to Account {1}!", cha.Name, acc.Name);
					}

				case "getaccountname":
					{
						//Test correct parameters 
						if (args.Length < 3)
							return DisplaySyntax(client);

						string charname = args[2];
						Character cha = GetCharacter(charname);
						if (cha == null)
							return DisplayError(client, "Character {0} not found!", charname);

						string accname = GetAccountName(charname);
						return DisplayMessage(client, "Account name for character " + cha.Name + " is : " + accname);
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
			GameClient client = WorldMgr.GetClientByAccountName(name, true);
			if (client != null)
				return client.Account;

			//Return database object
			return (Account)GameServer.Database.SelectObject(typeof(Account), "Name ='" + GameServer.Database.Escape(name) + "'");
		}

		/// <summary>
		/// Returns a given character
		/// </summary>
		/// <param name="charname">the charactername</param>
		/// <returns>the found character or null</returns>
		private Character GetCharacter(string charname)
		{
			//Seek players ingame first
			GameClient client = WorldMgr.GetClientByPlayerName(charname, true, false);
			if (client != null)
				return client.Player.PlayerCharacter;

			//Return database object
			return (Character) GameServer.Database.SelectObject(typeof (Character), "Name='" + GameServer.Database.Escape(charname) + "'");
		}

		/// <summary>
		/// Kicks an active playing account from the server
		/// </summary>
		/// <param name="acc">The account</param>
		private void KickAccount(Account acc)
		{
			//Check if the player is online
			GameClient playingclient = WorldMgr.GetClientByAccountName(acc.Name, true);
			if (playingclient != null)
			{
				//IF the player is online, kick him out
				playingclient.Out.SendPlayerQuit(true);
				playingclient.Disconnect();
			}
		}

		/// <summary>
		/// Kicks an active playing character from the server
		/// </summary>
		/// <param name="cha">the character</param>
		private void KickCharacter(Character cha)
		{
			//Check if the player is online
			GameClient playingclient = WorldMgr.GetClientByPlayerName(cha.Name, true, false);
			if (playingclient != null)
			{
				//IF the player is online, kick him out
				playingclient.Out.SendPlayerQuit(true);
				playingclient.Disconnect();
			}
		}

		/// <summary>
		/// Returns an account name with a given character name
		/// </summary>
		/// <param name="charname">the charactername</param>
		/// <returns>the account name or null</returns>
		private string GetAccountName(string charname)
		{
			//Seek players ingame first
			GameClient client = WorldMgr.GetClientByPlayerName(charname, true, false);
			if (client != null)
				return client.Account.Name;

			//Return database object
			Character ch = (Character) GameServer.Database.SelectObject(typeof (Character), "Name='" + GameServer.Database.Escape(charname) + "'");
			if (ch != null)
				return ch.AccountName;
			else
				return null;
		}
	}
}