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
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using DOL.Database;
using DOL.GS.ServerProperties;
using log4net;

namespace DOL.GS.PacketHandler.Client.v168
{
	/// <summary>
	/// Handles the login request packet.
	/// </summary>
	/// <remarks>
	/// Included is a PHP snippet for generating passwords that will work with the system/hashing algorithm DOL uses:
	/// 
	/// PHP version of CryptPass(string password):
	///
	///	$pass = "abc";
	///	cryptPassword($pass);
	///
	///	function cryptPassword($pass)
	///	{
	///		$len = strlen($pass);
	///		$res = "";
	///		for ($i = 0; $i < $len; $i++)
	///		{
	///			$res = $res . chr(ord(substr($pass, $i, 1)) >> 8);
	///			$res = $res . chr(ord(substr($pass, $i, 1)));
	///		}
	///
	///		$hash = strtoupper(md5($res));
	///		$len = strlen($hash);
	///		for ($i = ($len-1)&~1; $i >= 0; $i-=2)
	///		{
	///			if (substr($hash, $i, 1) == "0")
	///				$hash = substr($hash, 0, $i) . substr($hash, $i+1, $len);
	///		}
	///
	///		$crypted = "##" . $hash;
	///		return $crypted;
	///	}
	/// </remarks>
	[PacketHandler(PacketHandlerType.TCP, 0x0F ^ 168, "Handles the login")]
	public class LoginRequestHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static DateTime m_lastAccountCreateTime;
		private readonly Dictionary<string, LockCount> m_locks = new Dictionary<string, LockCount>();

		#region IPacketHandler Members

		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			string ipAddress = client.TcpEndpointAddress;

			packet.Skip(2); //Skip the client_type byte
			var major = (byte)packet.ReadByte();
			var minor = (byte)packet.ReadByte();
			var build = (byte)packet.ReadByte();
			string password = packet.ReadString(20);

			bool v174;
			//the logger detection we had is no longer working
			//bool loggerUsing = false;
			switch (client.Version)
			{
				case GameClient.eClientVersion.Version168:
				case GameClient.eClientVersion.Version169:
				case GameClient.eClientVersion.Version170:
				case GameClient.eClientVersion.Version171:
				case GameClient.eClientVersion.Version172:
				case GameClient.eClientVersion.Version173:
					v174 = false;
					break;
				default:
					v174 = true;
					break;
			}

			if (v174)
			{
				packet.Skip(11);
			}
			else
			{
				packet.Skip(7);
			}

			uint c2 = packet.ReadInt();
			uint c3 = packet.ReadInt();
			uint c4 = packet.ReadInt();

			if (v174)
			{
				packet.Skip(27);
			}
			else
			{
				packet.Skip(31);
			}

			string userName = packet.ReadString(20);
			/*
			if (c2 == 0 && c3 == 0x05000000 && c4 == 0xF4000000)
			{
				loggerUsing = true;
				Log.Warn("logger detected (" + username + ")");
			}*/

			// check server status
			if (GameServer.Instance.ServerStatus == eGameServerStatus.GSS_Closed)
			{
				client.Out.SendLoginDenied(eLoginError.GameCurrentlyClosed);
				GameServer.Instance.Disconnect(client);

				return 1;
			}

			// check connection allowed with serverrules
			try
			{
				if (!GameServer.ServerRules.IsAllowedToConnect(client, userName))
				{
					if (Log.IsInfoEnabled)
						Log.Info(ipAddress + " disconnected because IsAllowedToConnect returned false!");

					GameServer.Instance.Disconnect(client);

					return 1;
				}
			}
			catch (Exception e)
			{
				if (Log.IsErrorEnabled)
					Log.Error("Error shutting down Client after IsAllowedToConnect failed!", e);
			}

			// Handle connection
			EnterLock(userName);

			try
			{
				Account playerAccount;
				// Make sure that client won't quit
				lock (client)
				{
					GameClient.eClientState state = client.ClientState;
					if (state != GameClient.eClientState.NotConnected)
					{
						Log.DebugFormat("wrong client state on connect {0} {1}", userName, state.ToString());
						return 1;
					}

					if (Log.IsInfoEnabled)
						Log.Info(string.Format("({0})User {1} logging on! ({2} type:{3} add:{4})", ipAddress, userName, client.Version,
											   (client.ClientType), client.ClientAddons.ToString("G")));
					// check client already connected
					GameClient findclient = WorldMgr.GetClientByAccountName(userName, true);
					if (findclient != null)
					{
						if (findclient.ClientState == GameClient.eClientState.Connecting)
						{
							if (Log.IsInfoEnabled)
								Log.Info("User is already connecting, ignored.");

							client.Out.SendLoginDenied(eLoginError.AccountAlreadyLoggedIn);

							return 1;
						} // in login

						if (findclient.ClientState == GameClient.eClientState.Linkdead)
						{
							if (Log.IsInfoEnabled)
								Log.Info("User is still being logged out from linkdeath!");

							client.Out.SendLoginDenied(eLoginError.AccountIsInLogoutProcedure);
						}
						else
						{
							if (Log.IsInfoEnabled)
								Log.Info("User already logged in!");

							client.Out.SendLoginDenied(eLoginError.AccountAlreadyLoggedIn);
						}

						GameServer.Instance.Disconnect(client);

						return 1;
					}

					bool goodname = true;
					foreach (char c in userName.ToLower())
					{
						if ((c < '0' || c > '9') && (c < 'a' || c > 'z') && client.Account.PrivLevel == (uint)ePrivLevel.Player)
						{
							goodname = false;
							break;
						}
					}

					// Yes! Stoping!
					if (!goodname)
					{
						if (Log.IsInfoEnabled)
							Log.Info("Invalid symbols in account name \"" + userName + "\" found!");

						client.Out.SendLoginDenied(eLoginError.AccountInvalid);
						GameServer.Instance.Disconnect(client);

						return 1;
					}
					else
					{
						playerAccount = GameServer.Database.FindObjectByKey<Account>(userName);

						client.PingTime = DateTime.Now.Ticks;

						if (playerAccount == null)
						{
							//check autocreate ...

							if (GameServer.Instance.Configuration.AutoAccountCreation && Properties.ALLOW_AUTO_ACCOUNT_CREATION)
							{
								// autocreate account
								if (string.IsNullOrEmpty(password))
								{
									client.Out.SendLoginDenied(eLoginError.AccountInvalid);
									GameServer.Instance.Disconnect(client);

									if (Log.IsInfoEnabled)
										Log.Info("Account creation failed, no password set for Account: " + userName);

									return 1;
								}

								// check for account bombing
								TimeSpan ts;
								IList<Account> allAccByIp = GameServer.Database.SelectObjects<Account>("LastLoginIP = '" + ipAddress + "'");
								int totalacc = 0;
								foreach (Account ac in allAccByIp)
								{
									ts = DateTime.Now - ac.CreationDate;
									if (ts.TotalMinutes < Properties.TIME_BETWEEN_ACCOUNT_CREATION_SAMEIP && totalacc > 1)
									{
										Log.Warn("Account creation: too many from same IP within set minutes - " + userName + " : " + ipAddress);

										client.Out.SendLoginDenied(eLoginError.PersonalAccountIsOutOfTime);
										GameServer.Instance.Disconnect(client);

										return 1;
									}

									totalacc++;
								}
								if (totalacc >= Properties.TOTAL_ACCOUNTS_ALLOWED_SAMEIP)
								{
									Log.Warn("Account creation: too many accounts created from same ip - " + userName + " : " + ipAddress);

									client.Out.SendLoginDenied(eLoginError.AccountNoAccessThisGame);
									GameServer.Instance.Disconnect(client);

									return 1;
								}

								// per timeslice - for preventing account bombing via different ip
								if (Properties.TIME_BETWEEN_ACCOUNT_CREATION > 0)
								{
									ts = DateTime.Now - m_lastAccountCreateTime;
									if (ts.TotalMinutes < Properties.TIME_BETWEEN_ACCOUNT_CREATION)
									{
										Log.Warn("Account creation: time between account creation too small - " + userName + " : " + ipAddress);

										client.Out.SendLoginDenied(eLoginError.PersonalAccountIsOutOfTime);
										GameServer.Instance.Disconnect(client);

										return 1;
									}
								}

								m_lastAccountCreateTime = DateTime.Now;

								playerAccount = new Account();
								playerAccount.Name = userName;
								playerAccount.Password = CryptPassword(password);
								playerAccount.Realm = 0;
								playerAccount.CreationDate = DateTime.Now;
								playerAccount.LastLogin = DateTime.Now;
								playerAccount.LastLoginIP = ipAddress;
								playerAccount.LastClientVersion = ((int)client.Version).ToString();
								playerAccount.Language = Properties.SERV_LANGUAGE;
								playerAccount.PrivLevel = 1;

								if (Log.IsInfoEnabled)
									Log.Info("New account created: " + userName);

								GameServer.Database.AddObject(playerAccount);

								// Log account creation
								AuditMgr.AddAuditEntry(client, AuditType.Account, AuditSubtype.AccountCreate, "", userName);
							}
							else
							{
								if (Log.IsInfoEnabled)
									Log.Info("No such account found and autocreation deactivated!");

								client.Out.SendLoginDenied(eLoginError.AccountNotFound);
								GameServer.Instance.Disconnect(client);

								return 1;
							}
						}
						else
						{
							// check password
							if (!playerAccount.Password.StartsWith("##"))
							{
								playerAccount.Password = CryptPassword(playerAccount.Password);
							}

							if (!CryptPassword(password).Equals(playerAccount.Password))
							{
								if (Log.IsInfoEnabled)
									Log.Info("(" + client.TcpEndpoint + ") Wrong password!");

								client.Out.SendLoginDenied(eLoginError.WrongPassword);

								// Log failure
								AuditMgr.AddAuditEntry(client, AuditType.Account, AuditSubtype.AccountFailedLogin, "", userName);

								GameServer.Instance.Disconnect(client);

								return 1;
							}

							// save player infos
							playerAccount.LastLogin = DateTime.Now;
							playerAccount.LastLoginIP = ipAddress;
							playerAccount.LastClientVersion = ((int)client.Version).ToString();
							if (string.IsNullOrEmpty(playerAccount.Language))
							{
								playerAccount.Language = Properties.SERV_LANGUAGE;
							}

							GameServer.Database.SaveObject(playerAccount);
						}
					}

					//Save the account table
					client.Account = playerAccount;

					// create session ID here to disable double login bug
					if (WorldMgr.CreateSessionID(client) < 0)
					{
						if (Log.IsInfoEnabled)
							Log.InfoFormat("Too many clients connected, denied login to " + playerAccount.Name);

						client.Out.SendLoginDenied(eLoginError.TooManyPlayersLoggedIn);
						client.Disconnect();

						return 1;
					}

					client.Out.SendLoginGranted();
					client.ClientState = GameClient.eClientState.Connecting;

					// Log entry
					AuditMgr.AddAuditEntry(client, AuditType.Account, AuditSubtype.AccountSuccessfulLogin, "", userName);
				}
			}
			catch (DatabaseException e)
			{
				if (Log.IsErrorEnabled)
					Log.Error("LoginRequestHandler", e);

				client.Out.SendLoginDenied(eLoginError.CannotAccessUserAccount);
				GameServer.Instance.Disconnect(client);
			}
			catch (Exception e)
			{
				if (Log.IsErrorEnabled)
					Log.Error("LoginRequestHandler", e);

				client.Out.SendLoginDenied(eLoginError.CannotAccessUserAccount);
				GameServer.Instance.Disconnect(client);
			}
			finally
			{
				ExitLock(userName);
			}

			return 1;
		}

		#endregion

		public static string CryptPassword(string password)
		{
			MD5 md5 = new MD5CryptoServiceProvider();

			char[] pw = password.ToCharArray();

			var res = new byte[pw.Length * 2];
			for (int i = 0; i < pw.Length; i++)
			{
				res[i * 2] = (byte)(pw[i] >> 8);
				res[i * 2 + 1] = (byte)(pw[i]);
			}

			byte[] bytes = md5.ComputeHash(res);

			var crypted = new StringBuilder();
			crypted.Append("##");
			for (int i = 0; i < bytes.Length; i++)
			{
				crypted.Append(bytes[i].ToString("X"));
			}

			return crypted.ToString();
		}

		/// <summary>
		/// Acquires the lock on account.
		/// </summary>
		/// <param name="accountName">Name of the account.</param>
		private void EnterLock(string accountName)
		{
			// Safety check
			if (accountName == null)
			{
				accountName = string.Empty;
				Log.Warn("(Enter) No account name");
			}

			LockCount lockObj = null;
			lock (m_locks)
			{
				// Get/create lock object
				if (!m_locks.TryGetValue(accountName, out lockObj))
				{
					lockObj = new LockCount();
					m_locks.Add(accountName, lockObj);
				}

				if (lockObj == null)
				{
					Log.Error("(Enter) No lock object for account: '" + accountName + "'");
				}
				else
				{
					// Increase count of locks
					lockObj.count++;
				}
			}

			if (lockObj != null)
			{
				Monitor.Enter(lockObj);
			}
		}

		/// <summary>
		/// Releases the lock on account.
		/// </summary>
		/// <param name="accountName">Name of the account.</param>
		private void ExitLock(string accountName)
		{
			// Safety check
			if (accountName == null)
			{
				accountName = string.Empty;
				Log.Warn("(Exit) No account name");
			}

			LockCount lockObj = null;
			lock (m_locks)
			{
				// Get lock object
				if (!m_locks.TryGetValue(accountName, out lockObj))
				{
					Log.Error("(Exit) No lock object for account: '" + accountName + "'");
				}

				// Remove lock object if no more locks on it
				if (lockObj != null)
				{
					if (--lockObj.count <= 0)
					{
						m_locks.Remove(accountName);
					}
				}
			}

			Monitor.Exit(lockObj);
		}

		#region Nested type: LockCount

		/// <summary>
		/// This class is used as lock object. Contains the count of locks held.
		/// </summary>
		private class LockCount
		{
			/// <summary>
			/// Count of locks held.
			/// </summary>
			public int count;
		}

		#endregion
	}
}