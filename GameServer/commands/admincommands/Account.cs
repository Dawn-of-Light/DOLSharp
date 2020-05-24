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
using DOL.Database;
using DOL.GS.PacketHandler.Client.v168;
using DOL.Language;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&account",
		ePrivLevel.Admin,
		"AdminCommands.Account.Description",
        "AdminCommands.Account.Usage.Create",
		"AdminCommands.Account.Usage.ChangePassword",
		"AdminCommands.Account.Usage.Delete",
		"AdminCommands.Account.Usage.DeleteChar",
		"AdminCommands.Account.Usage.MoveChar",
        "AdminCommands.Account.Usage.Status",
        "AdminCommands.Account.Usage.UnBan",
        "AdminCommands.Account.Usage.AccountName")]
	public class AccountCommand : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 2)
			{
				DisplaySyntax(client);
				return;
			}

			switch (args[1].ToLower())
			{
				#region Create
                case "create":
                    {
                        if (args.Length < 4)
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        string AccountName = args[2].ToLower();
                        string Password = args[3];

                        foreach (char c in AccountName.ToCharArray())
                        {
                            if ((c < '0' || c > '9') && (c < 'a' || c > 'z'))
                            {
                                DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "AdminCommands.Account.InvalidAccountName"));
                                return;
                            }
                        }

                        if (AccountName.Length < 4 || Password.Length < 4)
                        {
                            DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "AdminCommands.Account.InvalidAccountNameOrPassword"));
                            return;
                        }

                        Account account = GetAccount(AccountName);
                        if (account != null)
                        {
                            DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "AdminCommands.Account.AccountNameAlreadyRegistered"));
                            return;
                        }

                        account = new Account
                        {
                            Name = AccountName,
                            Password = PacketHandler.Client.v168.LoginRequestHandler.CryptPassword(Password),
                            PrivLevel = (uint)ePrivLevel.Player,
                            Realm = (int)eRealm.None,
                            CreationDate = DateTime.Now,
                            Language = ServerProperties.Properties.SERV_LANGUAGE
                        };
                        GameServer.Database.AddObject(account);

                        DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "AdminCommands.Account.AccountCreated"));
                    }
                    break;
                #endregion Create
                #region ChangePassword
				case "changepassword":
					{
						if (args.Length < 4)
						{
							DisplaySyntax(client);
							return;
						}

						string accountname = args[2];
						string newpass = args[3];

						Account acc = GetAccount(accountname);
						if (acc == null)
						{
							DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "AdminCommands.Account.AccountNotFound", accountname));
							return;
						}

						acc.Password = LoginRequestHandler.CryptPassword(newpass);
						GameServer.Database.SaveObject(acc);

						// Log change
						AuditMgr.AddAuditEntry(client, AuditType.Account, AuditSubtype.AccountPasswordChange, "", (client.Player != null ? client.Player.Name : ""));
					}
					break;
				#endregion ChangePassword
				#region Delete
				case "delete":
					{
						if (args.Length < 3)
						{
							DisplaySyntax(client);
							return;
						}

						string AccountName = args[2];
                        Account acc = GetAccount(AccountName);

						if (acc == null)
						{
                            DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "AdminCommands.Account.AccountNotFound", AccountName));
							return;
						}

						KickAccount(acc);
						GameServer.Database.DeleteObject(acc);

						// Log change
						AuditMgr.AddAuditEntry(client, AuditType.Account, AuditSubtype.AccountDelete, "acct="+AccountName, (client.Player != null ? client.Player.Name : ""));

                        DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "AdminCommands.Account.AccountDeleted", acc.Name));
						return;
					}
				#endregion Delete
                #region DeleteCharacter
                case "deletecharacter":
                    {
                        if (args.Length < 3)
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        string charname = args[2];
                        DOLCharacters cha = GetCharacter(charname);

                        if (cha == null)
                        {
                            DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "AdminCommands.Account.CharacterNotFound", charname));
                            return;
                        }

                        KickCharacter(cha);
                        GameServer.Database.DeleteObject(cha);

						// Log change
						AuditMgr.AddAuditEntry(client, AuditType.Character, AuditSubtype.CharacterDelete, "char="+charname, (client.Player != null ? client.Player.Name : ""));

                        DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "AdminCommands.Account.CharacterDeleted", cha.Name));
                        return;
                    }
                #endregion DeleteCharacter
                #region MoveCharacter
                case "movecharacter":
                    {
                        if (args.Length < 4)
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        string charname = args[2];
                        string accountname = args[3];

                        DOLCharacters cha = GetCharacter(charname);
                        if (cha == null)
                        {
                            DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "AdminCommands.Account.CharacterNotFound", charname));
                            return;
                        }

                        Account acc = GetAccount(accountname);
                        if (acc == null)
                        {
                            DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "AdminCommands.Account.AccountNotFound", accountname));
                            return;
                        }

                        int firstAccountSlot;
                        switch ((eRealm)cha.Realm)
                        {
                            case eRealm.Albion:
                                firstAccountSlot = 1 * 8;
                                break;
                            case eRealm.Midgard:
                                firstAccountSlot = 2 * 8;
                                break;
                            case eRealm.Hibernia:
                                firstAccountSlot = 3 * 8;
                                break;
                            default:
                                DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "AdminCommands.Account.CharNotFromValidRealm"));
                                return;
                        }

                        int freeslot;
                        for (freeslot = firstAccountSlot; freeslot < firstAccountSlot + 8; freeslot++)
                        {
                            bool found = false;
                            foreach (DOLCharacters ch in acc.Characters)
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
                        {
                            DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "AdminCommands.Account.AccountHasNoFreeSlots", accountname));
                            return;
                        }

                        GameClient playingclient = WorldMgr.GetClientByPlayerName(cha.Name, true, false);
                        if (playingclient != null)
                        {
                            playingclient.Out.SendPlayerQuit(true);
                            playingclient.Disconnect();
                        }

                        cha.AccountName = acc.Name;
                        cha.AccountSlot = freeslot;

                        GameServer.Database.SaveObject(cha);
                        DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "AdminCommands.Account.CharacterMovedToAccount", cha.Name, acc.Name));
                        return;
                    }
                #endregion MoveCharacter
				#region Status
				case "status":
					{
						if (args.Length < 4)
						{
							DisplaySyntax(client);
							return;
						}

						string accountname = args[2];
						Account acc = GetAccount(accountname);

						if (acc == null)
						{
							DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "AdminCommands.Account.AccountNotFound", accountname));
							return;
						}

                        int status;
						try { status=Convert.ToInt32(args[3]); } catch(Exception) { DisplaySyntax(client); return; }
						if(status >= 0 && status < 256 )
						{
							acc.Status=status;
							GameServer.Database.SaveObject(acc);
							DisplayMessage(client, "Account "+acc.Name+" Status is now set to : "+acc.Status);
						}
						else DisplaySyntax(client);
						return;
					}
				#endregion Status
				#region Unban
				case "unban":
					{
						if (args.Length < 3)
						{
							DisplaySyntax(client);
							return;
						}

						string accountname = args[2];
						Account acc = GetAccount(accountname);

						if (acc == null)
						{
							DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "AdminCommands.Account.AccountNotFound", accountname));
							return;
						}

						var banacc = GameServer.Database.SelectObjects<DBBannedAccount>("(`Type` = @TypeA OR `Type` = @TypeB) AND `Account` = @Account", new[] { new QueryParameter("@TypeA", "A"), new QueryParameter("@TypeB", "B"), new QueryParameter("@Account", accountname) });
						if (banacc.Count == 0)
						{
							DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "AdminCommands.Account.AccountNotFound", accountname));
							return;
						}
						
						try
                        {
                            GameServer.Database.DeleteObject(banacc);
                        }
                        catch(Exception) { DisplaySyntax(client); return; }
						DisplayMessage(client, "Account "+accountname+" unbanned!");
						return;
					}
				#endregion Unban
                #region AccountName
                case "accountname":
                    {
                        if (args.Length < 3)
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        string CharName = args[2];
                        DOLCharacters Char = GetCharacter(CharName);
                        
                        if (Char == null)
                        {
                            DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "AdminCommands.Account.CharacterNotFound", CharName));
                            return;
                        }

                        string AccName = GetAccountName(Char.Name);
                        DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "AdminCommands.Account.AccNameForChar", Char.Name, AccName));
                        
                        return;
                    }
                #endregion AccountName
            }
		}
        
		/// <summary>
		/// Loads an account
		/// </summary>
		/// <param name="name">the accountname</param>
		/// <returns>the found account or null</returns>
		private Account GetAccount(string name)
		{
			GameClient client = WorldMgr.GetClientByAccountName(name, true);
			if (client != null)
				return client.Account;
			return GameServer.Database.FindObjectByKey<Account>(name);
		}

		/// <summary>
		/// Returns a given character
		/// </summary>
		/// <param name="charname">the charactername</param>
		/// <returns>the found character or null</returns>
		private DOLCharacters GetCharacter(string charname)
		{
			GameClient client = WorldMgr.GetClientByPlayerName(charname, true, false);
			if (client != null)
				return client.Player.DBCharacter;
			return GameServer.Database.SelectObjects<DOLCharacters>("`Name` = @Name", new QueryParameter("@Name", charname)).FirstOrDefault();
		}

		/// <summary>
		/// Kicks an active playing account from the server
		/// </summary>
		/// <param name="acc">The account</param>
		private void KickAccount(Account acc)
		{
			GameClient playingclient = WorldMgr.GetClientByAccountName(acc.Name, true);
			if (playingclient != null)
			{
				playingclient.Out.SendPlayerQuit(true);
				playingclient.Disconnect();
			}
		}

		/// <summary>
		/// Kicks an active playing character from the server
		/// </summary>
		/// <param name="cha">the character</param>
		private void KickCharacter(DOLCharacters cha)
		{
			GameClient playingclient = WorldMgr.GetClientByPlayerName(cha.Name, true, false);
			if (playingclient != null)
			{
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
			GameClient client = WorldMgr.GetClientByPlayerName(charname, true, false);
			if (client != null)
				return client.Account.Name;

			DOLCharacters ch = GameServer.Database.SelectObjects<DOLCharacters>("`Name` = @Name", new QueryParameter("@Name", charname)).FirstOrDefault();
			if (ch != null)
				return ch.AccountName;
			else
				return null;
		}
	}
}