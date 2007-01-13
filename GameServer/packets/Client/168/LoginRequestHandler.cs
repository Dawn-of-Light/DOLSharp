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
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using DOL.Database;
using log4net;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP,0x0F^168,"Handles the login")]
	public class LoginRequestHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			string ipAddress = client.TcpEndpoint;

			packet.Skip(2); //Skip the client_type byte
			byte major = (byte)packet.ReadByte();
			byte minor = (byte)packet.ReadByte();
			byte build = (byte)packet.ReadByte();
			string password = packet.ReadString(20);
			bool v174;
			bool loggerUsing = false;
			switch(client.Version)
			{
				case GameClient.eClientVersion.Version168:
				case GameClient.eClientVersion.Version169:
				case GameClient.eClientVersion.Version170:
				case GameClient.eClientVersion.Version171:
				case GameClient.eClientVersion.Version172:
				case GameClient.eClientVersion.Version173:
					v174 = false; break;
				default:
					v174 = true; break;
			}
			if(v174)
				packet.Skip(11);
			else
				packet.Skip(7);
			uint c2 = packet.ReadInt();
			uint c3 = packet.ReadInt();
			uint c4 = packet.ReadInt();
			if(v174)
				packet.Skip(27);
			else
				packet.Skip(31);
			string username = packet.ReadString(20);
			if (c2 == 0 && c3 == 0x05000000 && c4 == 0xF4000000)
			{
				loggerUsing = true;
				log.Warn("logger detected (" + username + ")");
			}

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
				if(!GameServer.ServerRules.IsAllowedToConnect(client, username))
				{
					if (log.IsInfoEnabled)
						log.Info(ipAddress + " disconnected because IsAllowedToConnect returned false!");
					GameServer.Instance.Disconnect(client);
					return 1;
				}
			}
			catch(Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("Error shutting down Client after IsAllowedToConnect failed!",e);
			}

			try
			{
				Account playerAccount;
				// handle connection
				lock (this)
				{
					lock (client)
					{
						GameClient.eClientState state = client.ClientState;
						if (state != GameClient.eClientState.NotConnected)
						{
							log.DebugFormat("wrong client state on connect {0} {1}", username, state.ToString());
							return 1;
						}
						if (log.IsInfoEnabled)
							log.Info(string.Format("({0})User {1} logging on! ({2} type:{3} add:{4})", ipAddress, username, client.Version.ToString(), client.ClientType.ToString(), client.ClientAddons.ToString("G")));
						// check client already connected
						GameClient findclient = WorldMgr.GetClientByAccountName(username, true);
						if(findclient != null)
						{
							if(findclient.ClientState == GameClient.eClientState.Connecting)
							{
								if (log.IsInfoEnabled)
									log.Info("User is already connecting, ignored.");
								return 1;
							} // in login

							if(findclient.ClientState == GameClient.eClientState.Linkdead)
							{
								if (log.IsInfoEnabled)
									log.Info("User is still being logged out from linkdeath!");
								client.Out.SendLoginDenied(eLoginError.AccountIsInLogoutProcedure);
							}
							else
							{
								if (log.IsInfoEnabled)
									log.Info("User already logged in!");
								client.Out.SendLoginDenied(eLoginError.AccountAlreadyLoggedIn);
							}
							GameServer.Instance.Disconnect(client);
							return 1;
						}
						/*
						if (loggerUsing)
							client.LoggerUsing = true;
						else
							client.LoggerUsing = false;
						 */
						playerAccount = (Account) GameServer.Database.FindObjectByKey(typeof(Account), username);
						client.PingTime = DateTime.Now.Ticks;

						if(playerAccount == null)
						{
							//check autocreate ...
							bool goodname = true;
							if(GameServer.Instance.Configuration.AutoAccountCreation)
							{
								// autocreate account
								foreach(char c in username.ToLower().ToCharArray())
								{
									if((c < '0' || c > '9') && (c < 'a' || c > 'z'))
									{
										goodname = false;
										break;
									}
								}

								// Yes! Stoping!
								if (!goodname)
								{
									if (log.IsInfoEnabled)
										log.Info("Invalid symbols in account name \""+username+"\" found!");
									client.Out.SendLoginDenied(eLoginError.AccountInvalid);
									GameServer.Instance.Disconnect(client);
									return 1;
								}

								playerAccount = new Account();
								playerAccount.Name = username;
								playerAccount.Password = CryptPassword(password);
								playerAccount.Realm = 0;
								playerAccount.CreationDate = DateTime.Now;
								playerAccount.LastLogin = DateTime.Now;
								playerAccount.LastLoginIP = ipAddress;
								playerAccount.Language = ServerProperties.Properties.SERV_LANGUAGE;
								
								if(GameServer.Database.GetObjectCount(typeof (Account)) == 0)
								{
									playerAccount.PrivLevel = 3;
									if (log.IsInfoEnabled)
										log.Info("New admin account created: " + username);
								}
								else
								{
									playerAccount.PrivLevel = 1;
									if (log.IsInfoEnabled)
										log.Info("New account created: " + username);
								}

								GameServer.Database.AddNewObject(playerAccount);
							}
							else
							{
								if (log.IsInfoEnabled)
									log.Info("No such account found and autocreation deactivated!");
								client.Out.SendLoginDenied(eLoginError.AccountNotFound);
								GameServer.Instance.Disconnect(client);
								return 1;
							}
						}
						else
						{
//							// autoconvert all
//							foreach (Account acc in GameServer.Database.SelectAllObjects(typeof(Account))) {
//								if (acc.Password != null && !acc.Password.StartsWith("##")) {
//									acc.Password = CryptPassword(acc.Password);
//									GameServer.Database.SaveObject(acc);
//								}
//							}

							// check password
							//if (!playerAccount.Password.StartsWith("##"))
							//{
							//	playerAccount.Password = CryptPassword(playerAccount.Password);
							//}
							if (!CryptPassword(password).Equals(playerAccount.Password))
							{
								if (log.IsInfoEnabled)
									log.Info("(" + client.TcpEndpoint + ") Wrong password!");
								client.Out.SendLoginDenied(eLoginError.WrongPassword);
								GameServer.Instance.Disconnect(client);
								return 1;
							}
							// save player infos
							playerAccount.LastLogin = DateTime.Now;
							playerAccount.LastLoginIP = ipAddress;
							if (playerAccount.Language == null || playerAccount.Language == "")
								playerAccount.Language = ServerProperties.Properties.SERV_LANGUAGE;
							
							GameServer.Database.SaveObject(playerAccount);
						}

						//Save the account table
						client.Account = playerAccount;

						// create session ID here to disable double login bug
						if (WorldMgr.CreateSessionID(client) < 0)
						{
							if (log.IsInfoEnabled)
								log.InfoFormat("Too many clients connected, denied login to " + playerAccount.Name);
							client.Out.SendLoginDenied(eLoginError.TooManyPlayersLoggedIn);
							client.Disconnect();
							return 1;
						}
						client.Out.SendLoginGranted();
						client.ClientState = GameClient.eClientState.Connecting;
					}
				}

			}
			catch (DatabaseException e)
			{
				if (log.IsErrorEnabled)
					log.Error("LoginRequestHandler", e);
				client.Out.SendLoginDenied(eLoginError.CannotAccessUserAccount);
				GameServer.Instance.Disconnect(client);
			}
			catch(Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("LoginRequestHandler", e);
				client.Out.SendLoginDenied(eLoginError.CannotAccessUserAccount);
				GameServer.Instance.Disconnect(client);
			}

			return 1;
		}

		public static string CryptPassword(string password) {
			MD5 md5 = new MD5CryptoServiceProvider();
			char[] pw = password.ToCharArray();
			byte[] res = new byte[pw.Length*2];
			for (int i=0; i<pw.Length; i++) {
				res[i*2] = (byte)(pw[i]>>8);
				res[i*2+1] = (byte)(pw[i]);
			}
			byte[] bytes = md5.ComputeHash(res);
			StringBuilder crypted = new StringBuilder();
			crypted.Append("##");
			for (int i=0; i<bytes.Length; i++) {
				crypted.Append(bytes[i].ToString("X"));
			}
			return crypted.ToString();
		}

/*
php version of CryptPass(string password)

$pass = "abc";
cryptPassword($pass);

function cryptPassword($pass)
{
	$len = strlen($pass);
	$res = "";
	for ($i = 0; $i < $len; $i++)
	{
		$res = $res . chr(ord(substr($pass, $i, 1)) >> 8);
		$res = $res . chr(ord(substr($pass, $i, 1)));
	}

	$hash = strtoupper(md5($res));
	$len = strlen($hash);
	for ($i = ($len-1)&~1; $i >= 0; $i-=2)
	{
		if (substr($hash, $i, 1) == "0")
			$hash = substr($hash, 0, $i) . substr($hash, $i+1, $len);
	}

	$crypted = "##" . $hash;
	return $crypted;
}
*/
	}
}
