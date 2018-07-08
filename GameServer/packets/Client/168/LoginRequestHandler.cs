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
using System.Text.RegularExpressions;
using System.Threading;
using DOL.Database;
using DOL.GS.ServerProperties;
using log4net;

namespace DOL.GS.PacketHandler.Client.v168
{
    /// <summary>
    /// Handles the login request packet.
    /// </summary>
    [PacketHandler(PacketHandlerType.TCP, eClientPackets.LoginRequest, "Handles the login.", eClientStatus.None)]
    public class LoginRequestHandler : IPacketHandler
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static DateTime _lastAccountCreateTime;
        private readonly Dictionary<string, LockCount> _locks = new Dictionary<string, LockCount>();

        public void HandlePacket(GameClient client, GSPacketIn packet)
        {
            if (client == null)
            {
                return;
            }

            string ipAddress = client.TcpEndpointAddress;
            
            string password;
            string userName;

            /// <summary>
            /// Packet Format Change above 1.115
            /// </summary>
                        
            // 1.115c+

            // client type
            packet.Skip(1);

            // version
            packet.ReadByte(); // major
            packet.ReadByte(); // minor
            packet.ReadByte(); // build

            // revision
            packet.Skip(1);

            // build
            packet.Skip(2);

            // Read Login
            userName = packet.ReadLowEndianShortPascalString();

            // Read Password
            password = packet.ReadLowEndianShortPascalString();
            

            // check server status
            if (GameServer.Instance.ServerStatus == eGameServerStatus.GSS_Closed)
            {
                client.IsConnected = false;
                client.Out.SendLoginDenied(eLoginError.GameCurrentlyClosed);
                Log.Info(ipAddress + " disconnected because game is closed!");
                GameServer.Instance.Disconnect(client);

                return;
            }

            // check connection allowed with serverrules
            try
            {
                if (!GameServer.ServerRules.IsAllowedToConnect(client, userName))
                {
                    if (Log.IsInfoEnabled)
                    {
                        Log.Info($"{ipAddress} disconnected because IsAllowedToConnect returned false!");
                    }

                    GameServer.Instance.Disconnect(client);

                    return;
                }
            }
            catch (Exception e)
            {
                if (Log.IsErrorEnabled)
                {
                    Log.Error("Error shutting down Client after IsAllowedToConnect failed!", e);
                }
            }

            // Handle connection
            EnterLock(userName);

            try
            {
                // Make sure that client won't quit
                lock (client)
                {
                    GameClient.eClientState state = client.ClientState;
                    if (state != GameClient.eClientState.NotConnected)
                    {
                        Log.Debug($"wrong client state on connect {userName} {state}");
                        return;
                    }

                    if (Log.IsInfoEnabled)
                    {
                        Log.Info($"({ipAddress})User {userName} logging on! ({client.Version} type:{client.ClientType} add:{client.ClientAddons:G})");
                    }

                    // check client already connected
                    GameClient findclient = WorldMgr.GetClientByAccountName(userName, true);
                    if (findclient != null)
                    {
                        client.IsConnected = false;

                        if (findclient.ClientState == GameClient.eClientState.Connecting)
                        {
                            if (Log.IsInfoEnabled)
                            {
                                Log.Info("User is already connecting, ignored.");
                            }

                            client.Out.SendLoginDenied(eLoginError.AccountAlreadyLoggedIn);

                            return;
                        } // in login

                        if (findclient.ClientState == GameClient.eClientState.Linkdead)
                        {
                            if (Log.IsInfoEnabled)
                            {
                                Log.Info("User is still being logged out from linkdeath!");
                            }

                            client.Out.SendLoginDenied(eLoginError.AccountIsInLogoutProcedure);
                        }
                        else
                        {
                            if (Log.IsInfoEnabled)
                            {
                                Log.Info("User already logged in!");
                            }

                            client.Out.SendLoginDenied(eLoginError.AccountAlreadyLoggedIn);
                        }

                        GameServer.Instance.Disconnect(client);

                        return;
                    }

                    Regex goodName = new Regex("^[a-zA-Z0-9]*$");
                    Account playerAccount;
                    if (!goodName.IsMatch(userName) || string.IsNullOrWhiteSpace(userName))
                    {
                        if (Log.IsInfoEnabled)
                        {
                            Log.Info($"Invalid symbols in account name \"{userName}\" found!");
                        }

                        client.IsConnected = false;
                        if (client.Out != null)
                        {
                            client.Out.SendLoginDenied(eLoginError.AccountInvalid);
                        }
                        else
                        {
                            Log.Warn("Client or Client.Out null on invalid name failure.  Disconnecting.");
                        }

                        GameServer.Instance.Disconnect(client);

                        return;
                    }
                    else
                    {
                        playerAccount = GameServer.Database.FindObjectByKey<Account>(userName);

                        client.PingTime = DateTime.Now.Ticks;

                        if (playerAccount == null)
                        {
                            // check autocreate ...
                            if (GameServer.Instance.Configuration.AutoAccountCreation && Properties.ALLOW_AUTO_ACCOUNT_CREATION)
                            {
                                // autocreate account
                                if (string.IsNullOrEmpty(password))
                                {
                                    client.IsConnected = false;
                                    client.Out.SendLoginDenied(eLoginError.AccountInvalid);
                                    GameServer.Instance.Disconnect(client);

                                    if (Log.IsInfoEnabled)
                                    {
                                        Log.Info($"Account creation failed, no password set for Account: {userName}");
                                    }

                                    return;
                                }

                                // check for account bombing
                                TimeSpan ts;
                                IList<Account> allAccByIp = GameServer.Database.SelectObjects<Account>("`LastLoginIP` = @LastLoginIP", new QueryParameter("@LastLoginIP", ipAddress));
                                int totalacc = 0;
                                foreach (Account ac in allAccByIp)
                                {
                                    ts = DateTime.Now - ac.CreationDate;
                                    if (ts.TotalMinutes < Properties.TIME_BETWEEN_ACCOUNT_CREATION_SAMEIP && totalacc > 1)
                                    {
                                        Log.Warn($"Account creation: too many from same IP within set minutes - {userName} : {ipAddress}");

                                        client.IsConnected = false;
                                        client.Out.SendLoginDenied(eLoginError.PersonalAccountIsOutOfTime);
                                        GameServer.Instance.Disconnect(client);

                                        return;
                                    }

                                    totalacc++;
                                }

                                if (totalacc >= Properties.TOTAL_ACCOUNTS_ALLOWED_SAMEIP)
                                {
                                    Log.Warn($"Account creation: too many accounts created from same ip - {userName} : {ipAddress}");

                                    client.IsConnected = false;
                                    client.Out.SendLoginDenied(eLoginError.AccountNoAccessThisGame);
                                    GameServer.Instance.Disconnect(client);

                                    return;
                                }

                                // per timeslice - for preventing account bombing via different ip
                                if (Properties.TIME_BETWEEN_ACCOUNT_CREATION > 0)
                                {
                                    ts = DateTime.Now - _lastAccountCreateTime;
                                    if (ts.TotalMinutes < Properties.TIME_BETWEEN_ACCOUNT_CREATION)
                                    {
                                        Log.Warn($"Account creation: time between account creation too small - {userName} : {ipAddress}");

                                        client.IsConnected = false;
                                        client.Out.SendLoginDenied(eLoginError.PersonalAccountIsOutOfTime);
                                        GameServer.Instance.Disconnect(client);

                                        return;
                                    }
                                }

                                _lastAccountCreateTime = DateTime.Now;

                                playerAccount = new Account
                                {
                                    Name = userName,
                                    Password = CryptPassword(password),
                                    Realm = 0,
                                    CreationDate = DateTime.Now,
                                    LastLogin = DateTime.Now,
                                    LastLoginIP = ipAddress,
                                    LastClientVersion = ((int) client.Version).ToString(),
                                    Language = Properties.SERV_LANGUAGE,
                                    PrivLevel = 1
                                };

                                if (Log.IsInfoEnabled)
                                {
                                    Log.Info($"New account created: {userName}");
                                }

                                GameServer.Database.AddObject(playerAccount);

                                // Log account creation
                                AuditMgr.AddAuditEntry(client, AuditType.Account, AuditSubtype.AccountCreate, string.Empty, userName);
                            }
                            else
                            {
                                if (Log.IsInfoEnabled)
                                {
                                    Log.Info("No such account found and autocreation deactivated!");
                                }

                                client.IsConnected = false;
                                client.Out.SendLoginDenied(eLoginError.AccountNotFound);
                                GameServer.Instance.Disconnect(client);

                                return;
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
                                {
                                    Log.Info($"({client.TcpEndpoint}) Wrong password!");
                                }

                                client.IsConnected = false;
                                client.Out.SendLoginDenied(eLoginError.WrongPassword);

                                // Log failure
                                AuditMgr.AddAuditEntry(client, AuditType.Account, AuditSubtype.AccountFailedLogin, string.Empty, userName);

                                GameServer.Instance.Disconnect(client);

                                return;
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

                    // Save the account table
                    client.Account = playerAccount;

                    // create session ID here to disable double login bug
                    if (WorldMgr.CreateSessionID(client) < 0)
                    {
                        if (Log.IsInfoEnabled)
                        {
                            Log.InfoFormat("Too many clients connected, denied login to {0}", playerAccount.Name);
                        }

                        client.IsConnected = false;
                        client.Out.SendLoginDenied(eLoginError.TooManyPlayersLoggedIn);
                        client.Disconnect();

                        return;
                    }

                    client.Out.SendLoginGranted();
                    client.ClientState = GameClient.eClientState.Connecting;

                    // Log entry
                    AuditMgr.AddAuditEntry(client, AuditType.Account, AuditSubtype.AccountSuccessfulLogin, string.Empty, userName);
                }
            }
            catch (DatabaseException e)
            {
                if (Log.IsErrorEnabled)
                {
                    Log.Error("LoginRequestHandler", e);
                }

                client.IsConnected = false;
                client.Out.SendLoginDenied(eLoginError.CannotAccessUserAccount);
                GameServer.Instance.Disconnect(client);
            }
            catch (Exception e)
            {
                if (Log.IsErrorEnabled)
                {
                    Log.Error("LoginRequestHandler", e);
                }

                client.Out.SendLoginDenied(eLoginError.CannotAccessUserAccount);
                GameServer.Instance.Disconnect(client);
            }
            finally
            {
                ExitLock(userName);
            }
        }

        public static string CryptPassword(string password)
        {
            MD5 md5 = new MD5CryptoServiceProvider();

            char[] pw = password.ToCharArray();

            var res = new byte[pw.Length * 2];
            for (int i = 0; i < pw.Length; i++)
            {
                res[i * 2] = (byte)(pw[i] >> 8);
                res[i * 2 + 1] = (byte)pw[i];
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

            LockCount lockObj;
            lock (_locks)
            {
                // Get/create lock object
                if (!_locks.TryGetValue(accountName, out lockObj))
                {
                    lockObj = new LockCount();
                    _locks.Add(accountName, lockObj);
                }

                if (lockObj == null)
                {
                    Log.Error($"(Enter) No lock object for account: \'{accountName}\'");
                }
                else
                {
                    // Increase count of locks
                    lockObj.Count++;
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

            LockCount lockObj;
            lock (_locks)
            {
                // Get lock object
                if (!_locks.TryGetValue(accountName, out lockObj))
                {
                    Log.Error($"(Exit) No lock object for account: \'{accountName}\'");
                }

                // Remove lock object if no more locks on it
                if (lockObj != null)
                {
                    if (--lockObj.Count <= 0)
                    {
                        _locks.Remove(accountName);
                    }
                }
            }

            Monitor.Exit(lockObj);
        }

        /// <summary>
        /// This class is used as lock object. Contains the count of locks held.
        /// </summary>
        private class LockCount
        {
            /// <summary>
            /// Count of locks held.
            /// </summary>
            public int Count { get; set; }
        }
    }
}