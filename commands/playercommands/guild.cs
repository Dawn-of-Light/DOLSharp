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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DOL.GS;
using DOL.Database;
using DOL.Language;
using DOL.GS.Keeps;
using DOL.GS.Housing;
using DOL.GS.ServerProperties;
using DOL.GS.PacketHandler;

using log4net;

namespace DOL.GS.Commands
{
	/// <summary>
	/// command handler for /gc command
	/// </summary>
	[Cmd(
		"&gc",
		new string[] { "&guildcommand" },
		ePrivLevel.Player,
		"Guild command (use /gc help for options)",
		"/gc <option>")]
	public class GuildCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		/// <summary>
		/// Contains all characters that are valid in a guild name. non case sensitive
		/// </summary>
		public static string AllowedGuildNameChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ‹÷ƒ ˆ‰¸·ÈÌÛ˙¡…Õ”⁄";

		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Checks if a guildname has valid characters
		/// </summary>
		/// <param name="guildName"></param>
		/// <returns></returns>
		public static bool IsValidGuildName(string guildName)
		{
			foreach (char c in guildName)
			{
				if (AllowedGuildNameChars.IndexOf(char.ToLower(c)) < 0)
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// method to handle /gc commands from a client
		/// </summary>
		/// <param name="client"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public void OnCommand(GameClient client, string[] args)
        {
            try
            {
                if (args.Length == 1)
                {
                    DisplayHelp(client);
                    return;
                }
                string message;

                switch (args[1])
                {
                    #region Create
                    // --------------------------------------------------------------------------------
                    // CREATE
                    // --------------------------------------------------------------------------------
                    case "create":
                        {
                            if (client.Account.PrivLevel == (uint)ePrivLevel.Player)
                                return;
                            if (args.Length < 3)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildGMCreate"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            GameLiving guildLeader = client.Player.TargetObject as GameLiving;
                            string guildname = String.Join(" ", args, 2, args.Length - 2);
                            guildname = GameServer.Database.Escape(guildname);
                            if (!GuildMgr.DoesGuildExist(guildname))
                            {
                                if (guildLeader == null)
                                {
                                    client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.PlayerNotFound"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    return;
                                }

                                if (!IsValidGuildName(guildname))
                                {
                                    client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.InvalidLetters"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    return;
                                }
                                else
                                {
                                    Guild newGuild = GuildMgr.CreateGuild(client.Player, guildname);
                                    if (newGuild == null)
                                    {
                                        client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.UnableToCreate", newGuild.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                    else
                                    {
                                        newGuild.AddPlayer((GamePlayer)guildLeader);
                                        ((GamePlayer)guildLeader).GuildRank = ((GamePlayer)guildLeader).Guild.GetRankByID(0);
                                        client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.GuildCreated", guildname, ((GamePlayer)guildLeader).Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                    return;
                                }
                            }
                            else
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.GuildExists"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                            client.Player.Guild.UpdateGuildWindow();
                        }
                        break;
                    #endregion
                    #region Purge
                    // --------------------------------------------------------------------------------
                    // PURGE
                    // --------------------------------------------------------------------------------
                    case "purge":
                        {
                            if (client.Account.PrivLevel == (uint)ePrivLevel.Player)
                                return;

                            if (args.Length < 3)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildGMPurge"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            string guildname = String.Join(" ", args, 2, args.Length - 2);
                            if (!GuildMgr.DoesGuildExist(guildname))
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildNotExist"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (GuildMgr.DeleteGuild(guildname))
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Purged", guildname), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }
                        break;
                    #endregion
                    #region Rename
                    // --------------------------------------------------------------------------------
                    // RENAME
                    // --------------------------------------------------------------------------------
                    case "rename":
                        {
                            if (client.Account.PrivLevel == (uint)ePrivLevel.Player)
                                return;

                            if (args.Length < 5)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildGMRename"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            int i;
                            for (i = 2; i < args.Length; i++)
                            {
                                if (args[i] == "to")
                                    break;
                            }

                            string oldguildname = String.Join(" ", args, 2, i - 2);
                            string newguildname = String.Join(" ", args, i + 1, args.Length - i - 1);
                            if (!GuildMgr.DoesGuildExist(oldguildname))
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildNotExist"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            Guild myguild = GuildMgr.GetGuildByName(oldguildname);
                            myguild.Name = newguildname;
                            GuildMgr.AddGuild(myguild);
                            foreach (GamePlayer ply in myguild.ListOnlineMembers())
                            {
                                ply.GuildName = newguildname;
                            }
                            client.Player.Guild.UpdateGuildWindow();
                        }
                        break;
                    #endregion
                    #region AddPlayer
                    // --------------------------------------------------------------------------------
                    // ADDPLAYER
                    // --------------------------------------------------------------------------------
                    case "addplayer":
                        {
                            if (client.Account.PrivLevel == (uint)ePrivLevel.Player)
                                return;

                            if (args.Length < 5)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildGMAddPlayer"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            int i;
                            for (i = 2; i < args.Length; i++)
                            {
                                if (args[i] == "to")
                                    break;
                            }

                            string playername = String.Join(" ", args, 2, i - 2);
                            string guildname = String.Join(" ", args, i + 1, args.Length - i - 1);

                            GuildMgr.GetGuildByName(guildname).AddPlayer(WorldMgr.GetClientByPlayerName(playername, true, false).Player);
                            client.Player.Guild.UpdateGuildWindow();
                        }
                        break;
                    #endregion
                    #region RemovePlayer
                    // --------------------------------------------------------------------------------
                    // REMOVEPLAYER
                    // --------------------------------------------------------------------------------
                    case "removeplayer":
                        {
                            if (client.Account.PrivLevel == (uint)ePrivLevel.Player)
                                return;

                            if (args.Length < 5)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildGMRemovePlayer"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            int i;
                            for (i = 2; i < args.Length; i++)
                            {
                                if (args[i] == "from")
                                    break;
                            }

                            string playername = String.Join(" ", args, 2, i - 2);
                            string guildname = String.Join(" ", args, i + 1, args.Length - i - 1);

                            if (!GuildMgr.DoesGuildExist(guildname))
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildNotExist"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            GuildMgr.GetGuildByName(guildname).RemovePlayer("gamemaster", WorldMgr.GetClientByPlayerName(playername, true, false).Player);
                            client.Player.Guild.UpdateGuildWindow();
                        }
                        break;
                    #endregion
                    #region Invite
                    /****************************************guild member command***********************************************/
                    // --------------------------------------------------------------------------------
                    // INVITE
                    // --------------------------------------------------------------------------------
                    case "invite":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Invite))
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            GamePlayer obj = client.Player.TargetObject as GamePlayer;
                            if (args.Length > 2)
                            {
                                GameClient temp = WorldMgr.GetClientByPlayerName(args[2], true, true);
                                if (temp != null)
                                    obj = temp.Player;
                            }
                            if (obj == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.InviteNoSelected"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (obj == client.Player)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.InviteNoSelf"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            if (obj.Guild != null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.AlreadyInGuild"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (!obj.IsAlive)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.InviteDead"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (!GameServer.ServerRules.IsAllowedToGroup(client.Player, obj, true))
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.InviteNotThis"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
							if (!GameServer.ServerRules.IsAllowedToJoinGuild(obj, client.Player.Guild))
                            {
								client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.InviteNotThis"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return;
							}
                            obj.Out.SendGuildInviteCommand(client.Player, LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.InviteRecieved", client.Player.Name, client.Player.Guild.Name));
                            client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.InviteSent", obj.Name, client.Player.Guild.Name), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                            client.Player.Guild.UpdateGuildWindow();
                        }
                        break;
                    #endregion
                    #region Remove
                    // --------------------------------------------------------------------------------
                    // REMOVE
                    // --------------------------------------------------------------------------------
                    case "remove":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Remove))
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            if (args.Length < 3)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildRemove"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            object obj = null;
                            string playername = args[2];
                            if (playername == "")
                                obj = client.Player.TargetObject as GamePlayer;
                            else
                            {
                                GameClient myclient = WorldMgr.GetClientByPlayerName(playername, true, false);
                                if (myclient == null)
                                {
                                    // Patch 1.84: look for offline players
                                    obj = (Character)GameServer.Database.SelectObject(typeof(Character), "Name='" + GameServer.Database.Escape(playername) + "'");
                                }
                                else
                                    obj = myclient.Player;
                            }
                            if (obj == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.PlayerNotFound"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            string guildId = "";
                            ushort guildRank = 9;
                            string plyName = "";
                            GamePlayer ply = obj as GamePlayer;
                            Character ch = obj as Character;
                            if (obj is GamePlayer)
                            {
                                plyName = ply.Name;
                                guildId = ply.GuildID;
                                if (ply.GuildRank != null)
                                    guildRank = ply.GuildRank.RankLevel;
                            }
                            else
                            {
                                plyName = ch.Name;
                                guildId = ch.GuildID;
                                guildRank = (byte)ch.GuildRank;
                            }
                            if (guildId != client.Player.GuildID)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotInYourGuild"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            foreach (GamePlayer plyon in client.Player.Guild.ListOnlineMembers())
                            {
                                plyon.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.MemberRemoved", client.Player.Name, plyName), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                            if (obj is GamePlayer)
                                client.Player.Guild.RemovePlayer(client.Player.Name, ply);
                            else
                            {
                                ch.GuildID = "";
                                ch.GuildRank = 9;
                                GameServer.Database.SaveObject(ch);
                            }
                            
                            client.Player.Guild.UpdateGuildWindow();
                        }
                        break;
                    #endregion
                    #region Remove account
                    // --------------------------------------------------------------------------------
                    // REMOVE ACCOUNT (Patch 1.84)
                    // --------------------------------------------------------------------------------
                    case "removeaccount":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Remove))
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            if (args.Length < 3)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildRemAccount"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            string playername = String.Join(" ", args, 2, args.Length - 2);
                            // Patch 1.84: look for offline players
                            Character[] chs = (Character[])GameServer.Database.SelectObjects(typeof(Character), "AccountName='" + GameServer.Database.Escape(playername) + "' AND GuildID='" + client.Player.GuildID + "'");
                            if (chs != null && chs.GetLength(0) > 0)
                            {
                                GameClient myclient = WorldMgr.GetClientByAccountName(playername, false);
                                string plys = "";
                                bool isOnline = (myclient != null);
                                foreach (Character ch in chs)
                                {
                                    plys += (plys != "" ? "," : "") + ch.Name;
                                    if (isOnline && ch.Name == myclient.Player.Name)
                                        client.Player.Guild.RemovePlayer(client.Player.Name, myclient.Player);
                                    else
                                    {
                                        ch.GuildID = "";
                                        ch.GuildRank = 9;
                                        GameServer.Database.SaveObject(ch);
                                    }
                                }

                                foreach (GamePlayer ply in client.Player.Guild.ListOnlineMembers())
                                {
                                    ply.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.AccountRemoved", client.Player.Name, plys), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                }
                            }
                            else
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPlayersInAcc"), eChatType.CT_System, eChatLoc.CL_SystemWindow);

                            client.Player.Guild.UpdateGuildWindow();
                        }
                        break;
                    #endregion
                    #region Info
                    // --------------------------------------------------------------------------------
                    // INFO
                    // --------------------------------------------------------------------------------
                    case "info":
                        {
                            bool typed = false;
                            if (args.Length != 3)
                                typed = true;

                            if (client.Player.Guild == null)
                            {
                                if (!(args.Length == 3 && args[2] == "1"))
                                {
                                    client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                }
                                return;
                            }

                            if (typed)
                            {
                                /*
                                 * Guild Info for Clan Cotswold:
                                 * Realm Points: xxx Bouty Points: xxx Merit Points: xxx
                                 * Guild Level: xx
                                 * Dues: 0% Bank: 0 copper pieces
                                 * Current Merit Bonus: None
                                 * Banner available for purchase
                                 * Webpage: xxx
                                 * Contact Email:
                                 * Message: motd
                                 * Officer Message: xxx
                                 * Alliance Message: xxx
                                 * Claimed Keep: xxx
                                 */
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.InfoGuild", client.Player.Guild.Name), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.InfoRPBPMP", client.Player.Guild.RealmPoints, client.Player.Guild.BountyPoints, client.Player.Guild.MeritPoints), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.InfoGuildLevel", client.Player.Guild.GuildLevel), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.InfoGDuesBank", client.Player.Guild.GetGuildDuesPercent().ToString() + "%", Money.GetString(long.Parse(client.Player.Guild.GetGuildBank().ToString()))), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.InfoWebpage", client.Player.Guild.Webpage), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.InfoCEmail", client.Player.Guild.Email), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);

                                string motd = client.Player.Guild.Motd;
                                if (!Util.IsEmpty(motd) && client.Player.GuildRank.GcHear)
                                    client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.InfoMotd", motd), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                string omotd = client.Player.Guild.Omotd;
                                if (!Util.IsEmpty(omotd) && client.Player.GuildRank.OcHear)
                                    client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.InfoOMotd", omotd), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                if (client.Player.Guild.alliance != null)
                                {
                                    string amotd = client.Player.Guild.alliance.Dballiance.Motd;
                                    if (!Util.IsEmpty(amotd) && client.Player.GuildRank.AcHear)
                                        client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.InfoaMotd", amotd), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                }
								if (client.Player.Guild.ClaimedKeeps.Count > 0)
								{
									foreach (AbstractGameKeep keep in client.Player.Guild.ClaimedKeeps)
										client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Keep", keep.Name), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
								}
                            }
                            else
                            {
                                switch (args[2])
                                {
                                    case "1": // show guild info
                                        {
                                            if (client.Player.Guild == null)
                                                return;

                                            int housenum;
                                            if (client.Player.Guild.GuildOwnsHouse)
                                            {
                                                housenum = client.Player.Guild.GuildHouseNumber;
                                            }
                                            else
                                                housenum = 0;

                                            string mes = "I";
                                            mes += ',' + client.Player.Guild.GuildLevel.ToString(); // Guild Level
                                            mes += ',' + client.Player.Guild.GetGuildBank().ToString(); // Guild Bank money
                                            mes += ',' + client.Player.Guild.GetGuildDuesPercent().ToString(); // Guild Dues enable/disable
                                            mes += ',' + client.Player.Guild.BountyPoints.ToString(); // Guild Bounty
                                            mes += ',' + client.Player.Guild.RealmPoints.ToString(); // Guild Experience
                                            mes += ',' + client.Player.Guild.MeritPoints.ToString(); // Guild Merit Points
                                            mes += ',' + housenum.ToString(); // Guild houseLot ?
                                            mes += ',' + (client.Player.Guild.MemberOnlineCount + 1).ToString(); // online Guild member ?
                                            mes += ',' + client.Player.Guild.GuildBannerStatus(client.Player); //"Banner available for purchase", "Missing banner buying permissions"
                                            mes += ",\"" + client.Player.Guild.Motd + '\"'; // Guild Motd
                                            mes += ",\"" + client.Player.Guild.Omotd + '\"'; // Guild oMotd
                                            client.Out.SendMessage(mes, eChatType.CT_SocialInterface, eChatLoc.CL_SystemWindow);
                                            break;
                                        }
                                    case "2": //enable/disable social windows
                                        {
                                            // "P,ShowGuildWindow,ShowAllianceWindow,?,ShowLFGuildWindow(only with guild),0,0" // news and friend windows always showed
                                            client.Out.SendMessage("P," + (client.Player.Guild == null ? "0" : "1") + (client.Player.Guild.AllianceId!=string.Empty?"0":"1") + ",0,0,0,0", eChatType.CT_SocialInterface, eChatLoc.CL_SystemWindow);
                                            break;
                                        }
                                    default:
                                        break;
                                }
                                return;

                            }
                            client.Player.Guild.UpdateGuildWindow();
                            break;
                        }
                    #endregion
                    #region Buybanner
                    case "buybanner":
                        {
                            long Costs = (client.Player.Guild.GuildLevel * 100);
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return; 
                            }
                            if (client.Player.Guild.GuildBanner)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.BannerAlready"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            client.Player.Guild.UpdateGuildWindow();
                            if (client.Player.Guild.BountyPoints > Costs)
                            {
                                client.Player.Guild.RemoveBountyPoints(Costs);
                                client.Player.Guild.GuildBanner = true;
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.BannerBought", Costs), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                            }
                            else
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.BannerNotAfford"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            //  client.Player.Guild.BountyPoints;
                            client.Player.Guild.UpdateGuildWindow();
                            break;
                        }
                    #endregion
                    #region Summon
                    case "summon":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (!client.Player.Guild.GuildBanner)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.BannerNone"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (client.Player.Group == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.BannerNoGroup"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            foreach (GamePlayer player in client.Player.Guild.ListOnlineMembers())
                            {
                                if (player.IsCarryingGuildBanner)
                                {
                                    client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.BannerGuildSummoned"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                    return;
                                }
                            }
                            foreach (GamePlayer playa in client.Player.Group.GetPlayersInTheGroup())
                            {
                                if (playa.IsCarryingGuildBanner)
                                {
                                    client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.BannerGroupSummoned"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    return;
                                }
                            }
                            GuildBanner banner = new GuildBanner(client.Player);
                            banner.Start();
                            client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.BannerSummoned"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                            client.Player.Guild.UpdateGuildWindow();
                            break;
                        }
                    #endregion
                    #region Buff
                    // --------------------------------------------------------------------------------
                    // GUILD BUFF
                    // --------------------------------------------------------------------------------
                    case "buff":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            if (client.Player.Guild.GuildLevel < 15)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.GuildLevelReq"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            if (client.Player.Guild.BuffType > 0)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.ActiveBuff"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Leader) || !client.Player.Guild.GotAccess(client.Player, eGuildRank.Buff))
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            if (client.Player.Guild.MeritPoints < 1000)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.MeritPointReq"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            if (args.Length < 3)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildBuff"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            if (args[2] == "realmpoints")
                            {
                                client.Player.Guild.BuffType = 1;
                                client.Player.Guild.RemoveMeritPoints(1000);
                                client.Player.Guild.BuffTime = DateTime.Now;
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.BuffRealmPoints"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                foreach (GamePlayer ply in client.Player.Guild.ListOnlineMembers())
                                {
                                    ply.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.BuffActivated", client.Player.Name), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                    ply.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.BuffMPActivated"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                }
                                client.Player.Guild.UpdateGuildWindow();
                                return;
                            }
                            else if (args[2] == "bountypoints")
                            {
                                client.Player.Guild.BuffType = 2;
                                client.Player.Guild.RemoveMeritPoints(1000);
                                client.Player.Guild.BuffTime = DateTime.Now;
								client.Out.SendMessage( LanguageMgr.GetTranslation( client, "Scripts.Player.Guild.BuffBountyPoints" ), eChatType.CT_Guild, eChatLoc.CL_SystemWindow );
                                foreach (GamePlayer ply in client.Player.Guild.ListOnlineMembers())
                                {
                                    ply.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.BuffActivated", client.Player.Name), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                    ply.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.BuffBPActivated"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                }
                                client.Player.Guild.UpdateGuildWindow();
                                return;
                            }
                            else
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildBuff"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                return;
                            }
                        }
                    #endregion
                    #region Unsummon
                    case "unsummon":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (!client.Player.Guild.GuildBanner)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.BannerNone"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (client.Player.Group == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.BannerNoGroup"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (client.Player.InCombat)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.InCombat"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            foreach (GamePlayer player in client.Player.Guild.ListOnlineMembers())
                            {
                                if (player.IsCarryingGuildBanner && client.Player.Name == player.Name)
                                {
                                    client.Player.IsCarryingGuildBanner = false;
                                }
                            }
                            client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.BannerUnsummoned"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                            client.Player.Guild.UpdateGuildWindow();
                            break;
                        }
                    #endregion
                    #region Ranks
                    case "ranks":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            client.Player.Guild.UpdateGuildWindow();
                            if (!client.Player.GuildRank.GcHear)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            foreach (DBRank rank in client.Player.Guild.Ranks)
                            {
                                client.Out.SendMessage("RANK :" + rank.RankLevel.ToString() + "Name :" + rank.Title, eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage("AcHear :" + (rank.AcHear ? "y" : "n") + " AcSpeak :" + (rank.AcSpeak ? "y" : "n"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage("OcHear :" + (rank.OcHear ? "y" : "n") + " OcSpeak :" + (rank.OcSpeak ? "y" : "n"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage("GcHear :" + (rank.GcHear ? "y" : "n") + " GcSpeak :" + (rank.GcSpeak ? "y" : "n"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage("Emblem :" + (rank.Emblem ? "y" : "n") + " Promote :" + (rank.Promote ? "y" : "n"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage("Remove :" + (rank.Remove ? "y" : "n") + " View :" + (rank.View ? "y" : "n"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage("Dues :" + (rank.Dues ? "y" : "n") + " Withdraw :" + (rank.Withdraw ? "y" : "n"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                            }
                            client.Player.Guild.UpdateGuildWindow();
                            break;
                        }
                    #endregion
                    #region Webpage
                    case "webpage":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            client.Player.Guild.UpdateGuildWindow();
                            if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Leader))
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            message = String.Join(" ", args, 2, args.Length - 2);
                            client.Player.Guild.Webpage = message;
                            client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.WebpageSet", client.Player.Guild.Webpage), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                            client.Player.Guild.UpdateGuildWindow();
                            break;
                        }
                    #endregion
                    #region Email
                    case "email":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            client.Player.Guild.UpdateGuildWindow();
                            if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Leader))
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            message = String.Join(" ", args, 2, args.Length - 2);
                            client.Player.Guild.Email = message;
                            client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.EmailSet", client.Player.Guild.Email), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                            client.Player.Guild.UpdateGuildWindow();
                            break;
                        }
                    #endregion
                    #region List
                    // --------------------------------------------------------------------------------
                    // LIST
                    // --------------------------------------------------------------------------------
                    case "list":
                        {
                            ICollection guildList = GuildMgr.ListGuild();
                            lock (guildList.SyncRoot)
                            {
                                foreach (Guild gui in guildList)
                                {
                                    string mesg = gui.Name + "  " + gui.MemberOnlineCount + " members ";
                                    client.Out.SendMessage(mesg, eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                }
                                client.Player.Guild.UpdateGuildWindow();
                            }
                        }
                        client.Player.Guild.UpdateGuildWindow();
                        break;
                    #endregion
                    #region Edit
                    // --------------------------------------------------------------------------------
                    // EDIT
                    // --------------------------------------------------------------------------------
                    case "edit":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            client.Player.Guild.UpdateGuildWindow();
                            SetCmd(client, args);
                        }
                        client.Player.Guild.UpdateGuildWindow();
                        break;
                    #endregion
                    #region Form
                    // --------------------------------------------------------------------------------
                    // FORM
                    // --------------------------------------------------------------------------------
                    case "form":
                        {
                            if (args.Length < 3)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildForm"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            string guildname = String.Join(" ", args, 2, args.Length - 2);
							if (guildname.Length > 30)
							{
								client.Out.SendMessage("Sorry, your guild name is too long.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return;
							}
                            guildname = GameServer.Database.Escape(guildname);

                            if (!IsValidGuildName(guildname))
                            {
                                // Mannen doesn't know the live server message, so someone needs to enter it . ;-)
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.InvalidLetters"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            else
                            {
                                if (!GuildMgr.DoesGuildExist(guildname))
                                {
                                    if (Properties.GUILD_NUM > 1 && client.Account.PrivLevel == 1)
                                    {
                                        Group group = client.Player.Group;

                                        if (group == null)
                                        {
                                            client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.FormNoGroup"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                            return;
                                        }

                                        lock (group)
                                        {
                                            if (group.MemberCount < Properties.GUILD_NUM)
                                            {
                                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.FormNoMembers" + Properties.GUILD_NUM), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                                return;
                                            }

                                            // a member of group have a guild already, so quit!
                                            foreach (GamePlayer ply in group.GetPlayersInTheGroup())
                                            {
                                                if (ply.Guild != null)
                                                {
                                                    client.Player.Group.SendMessageToGroupMembers(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.AlreadyInGuildName", ply.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                                    return;
                                                }
												if (ply.Realm != client.Player.Realm && ServerProperties.Properties.ALLOW_CROSS_REALM_GUILDS == false)
												{
													client.Out.SendMessage("All group members must be of the same realm in order to create a guild.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
													return;
												}
                                            }

                                            Guild newGuild = GuildMgr.CreateGuild(client.Player, guildname);
                                            if (newGuild == null)
                                            {
                                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.UnableToCreateLead", guildname, client.Player.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                            }
                                            else
                                            {
                                                foreach (GamePlayer ply in group.GetPlayersInTheGroup())
                                                {
													if (ply == client.Player)
													{
														newGuild.AddPlayer(ply, newGuild.GetRankByID(0));
													}
													else
													{
														newGuild.AddPlayer(ply);
													}
                                                }

                                                client.Player.GuildRank = client.Player.Guild.GetRankByID(0); //creator is leader
                                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.GuildCreated", guildname, client.Player.Name), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                            }
                                        }
                                    }
                                    else
                                    {
										Guild newGuild = GuildMgr.CreateGuild(client.Player, guildname);

										if( newGuild == null )
                                        {
                                            client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.UnableToCreateLead", guildname, client.Player.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                        }
                                        else
                                        {
											newGuild.AddPlayer(client.Player, newGuild.GetRankByID(0));
                                            client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.GuildCreated", guildname, client.Player.Name), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                        }
                                    }
                                }
                                else
                                {
                                    client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.GuildExists"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                }
                            }
                        }
                        break;
                    #endregion
                    #region Quit
                    // --------------------------------------------------------------------------------
                    // QUIT
                    // --------------------------------------------------------------------------------
                    case "quit":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            client.Out.SendGuildLeaveCommand(client.Player, LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.ConfirmLeave", client.Player.Guild.Name));
                            client.Player.Guild.UpdateGuildWindow();
                        }
                        break;
                    #endregion
                    #region Promote
                    // --------------------------------------------------------------------------------
                    // PROMOTE
                    // --------------------------------------------------------------------------------
                    case "promote":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Promote))
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (args.Length < 3)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildPromote"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }


                            object obj = null;
                            string playername = args[3];
                            if (playername == "")
                                obj = client.Player.TargetObject as GamePlayer;
                            else
                            {
                                GameClient myclient = WorldMgr.GetClientByPlayerName(playername, true, false);
                                if (myclient == null)
                                {
                                    // Patch 1.84: look for offline players
                                    obj = (Character)GameServer.Database.SelectObject(typeof(Character), "Name='" + GameServer.Database.Escape(playername) + "'");
                                }
                                else
                                    obj = myclient.Player;
                            }
                            if (obj == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPlayerSelected"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            string guildId = "";
                            ushort guildRank = 9;
                            string plyName = "";
                            GamePlayer ply = obj as GamePlayer;
                            Character ch = obj as Character;
                            if (obj is GamePlayer)
                            {
                                plyName = ply.Name;
                                guildId = ply.GuildID;
                                if (ply.GuildRank != null)
                                    guildRank = ply.GuildRank.RankLevel;
                            }
                            else
                            {
                                plyName = ch.Name;
                                guildId = ch.GuildID;
                                guildRank = ch.GuildRank;
                            }
                            if (guildId != client.Player.GuildID)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotInYourGuild"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            ushort newrank = guildRank;
                            try
                            {
                                newrank = Convert.ToUInt16(args[2]);
                            }
                            catch
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildPromote"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
								return;
                            }
                            if ((newrank >= guildRank && guildRank != 0) || (newrank < 0))
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.PromoteHigherThanPlayer"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (obj is GamePlayer)
                            {
                                ply.GuildRank = client.Player.Guild.GetRankByID(newrank);
                                ply.SaveIntoDatabase();
                                ply.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.PromotedSelf", newrank.ToString()), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                            else
                            {
                                ch.GuildRank = newrank;
                                GameServer.Database.SaveObject(ch);
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.PromotedOther", plyName, newrank.ToString()), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                            }
                            client.Player.Guild.UpdateGuildWindow();
                        }
                        break;
                    #endregion
                    #region Demote
                    // --------------------------------------------------------------------------------
                    // DEMOTE
                    // --------------------------------------------------------------------------------
                    case "demote":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Demote))
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (args.Length < 3)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildDemote"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }


                            object obj = null;
                            string playername = args[3];
                            if (playername == "")
                                obj = client.Player.TargetObject as GamePlayer;
                            else
                            {
                                GameClient myclient = WorldMgr.GetClientByPlayerName(playername, true, false);
                                if (myclient == null)
                                {
                                    // Patch 1.84: look for offline players
                                    obj = (Character)GameServer.Database.SelectObject(typeof(Character), "Name='" + GameServer.Database.Escape(playername) + "'");
                                }
                                else
                                    obj = myclient.Player;
                            }
                            if (obj == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPlayerSelected"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            string guildId = "";
                            ushort guildRank = 1;
                            string plyName = "";
                            GamePlayer ply = obj as GamePlayer;
                            Character ch = obj as Character;
                            if (obj is GamePlayer)
                            {
                                plyName = ply.Name;
                                guildId = ply.GuildID;
                                if (ply.GuildRank != null)
                                    guildRank = ply.GuildRank.RankLevel;
                            }
                            else
                            {
                                plyName = ch.Name;
                                guildId = ch.GuildID;
                                guildRank = ch.GuildRank;
                            }
                            if (guildId != client.Player.GuildID)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotInYourGuild"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            try
                            {
                                ushort newrank = Convert.ToUInt16(args[2]);
                                if (newrank < guildRank || newrank > 10)
                                {
                                    client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.DemotedHigherThanPlayer"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    return;
                                }
                                if (obj is GamePlayer)
                                {
                                    ply.GuildRank = client.Player.Guild.GetRankByID(newrank);
                                    ply.SaveIntoDatabase();
                                    ply.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.DemotedSelf", newrank.ToString()), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                }
                                else
                                {
                                    ch.GuildRank = newrank;
                                    GameServer.Database.SaveObject(ch);
                                    client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.DemotedOther", plyName, newrank.ToString()), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                }
                            }
                            catch
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.InvalidRank"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                            client.Player.Guild.UpdateGuildWindow();
                        }
                        break;
                    #endregion
                    #region Who
                    // --------------------------------------------------------------------------------
                    // WHO
                    // --------------------------------------------------------------------------------
                    case "who":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            int ind = 0;
                            int startInd = 0;

                            #region Social Window
                            if (args.Length == 6 && args[2] == "window")
                            {
                                //First get the sorted list 
                                SortedList<string, GuildMgr.SocialWindowMember> guildMembers = GuildMgr.GetSocialWindowGuild(client.Player.GuildID);
                                if (guildMembers != null)
                                {
                                    int sorttemp;
                                    byte showtemp;
                                    int page;
                                    //Lets get the variables that were sent over
                                    if (Int32.TryParse(args[3], out sorttemp) && Int32.TryParse(args[4], out page) && Byte.TryParse(args[5], out showtemp) && sorttemp >= -7 && sorttemp <= 7)
                                    {
                                        bool showOffline = showtemp == 0 ? false : true;
                                        //The type of sorting we will be sending
                                        GuildMgr.SocialWindowMember.eSocialWindowSort sort = (GuildMgr.SocialWindowMember.eSocialWindowSort)sorttemp;
                                        //Let's sort the sorted list - we don't need to sort if sort = name
                                        SortedList<string, GuildMgr.SocialWindowMember> sortedList = null;
                                        GuildMgr.SocialWindowMember.eSocialWindowIndex index = GuildMgr.SocialWindowMember.eSocialWindowIndex.Name;

                                        #region Sort
                                        switch (sort)
                                        {
                                            case GuildMgr.SocialWindowMember.eSocialWindowSort.ClassAsc:
                                            case GuildMgr.SocialWindowMember.eSocialWindowSort.ClassDesc:
                                                index = GuildMgr.SocialWindowMember.eSocialWindowIndex.ClassID;
                                                break;
                                            case GuildMgr.SocialWindowMember.eSocialWindowSort.GroupAsc:
                                            case GuildMgr.SocialWindowMember.eSocialWindowSort.GroupDesc:
                                                index = GuildMgr.SocialWindowMember.eSocialWindowIndex.Group;
                                                break;
                                            case GuildMgr.SocialWindowMember.eSocialWindowSort.LevelAsc:
                                            case GuildMgr.SocialWindowMember.eSocialWindowSort.LevelDesc:
                                                index = GuildMgr.SocialWindowMember.eSocialWindowIndex.Level;
                                                break;
                                            case GuildMgr.SocialWindowMember.eSocialWindowSort.NoteAsc:
                                            case GuildMgr.SocialWindowMember.eSocialWindowSort.NoteDesc:
                                                index = GuildMgr.SocialWindowMember.eSocialWindowIndex.Note;
                                                break;
                                            case GuildMgr.SocialWindowMember.eSocialWindowSort.RankAsc:
                                            case GuildMgr.SocialWindowMember.eSocialWindowSort.RankDesc:
                                                index = GuildMgr.SocialWindowMember.eSocialWindowIndex.Rank;
                                                break;
                                            case GuildMgr.SocialWindowMember.eSocialWindowSort.ZoneOrOnlineAsc:
                                            case GuildMgr.SocialWindowMember.eSocialWindowSort.ZoneOrOnlineDesc:
                                                index = GuildMgr.SocialWindowMember.eSocialWindowIndex.ZoneOrOnline;
                                                break;
                                        }
                                        #endregion

                                        //We have to make a new sorted list if they just want online, lets do that first
                                        if (!showOffline)
                                        {
                                            SortedList<string, GamePlayer> onlineMembers = client.Player.Guild.SortedListOnlineMembers();
											sortedList = new SortedList<string, GuildMgr.SocialWindowMember>(onlineMembers.Count);
                                            foreach (GamePlayer player in onlineMembers.Values)
                                            {
                                                if (guildMembers.ContainsKey(player.Name))
                                                {
                                                    GuildMgr.SocialWindowMember member = guildMembers[player.Name];
													member.UpdateMember(player);
													string key = member[(int)index];
													if (sortedList.ContainsKey(key))
														key += sortedList.Count.ToString();

													sortedList.Add(key, member);
                                                }
                                            }
                                        }
										else if (index == GuildMgr.SocialWindowMember.eSocialWindowIndex.Name)
										{
											sortedList = guildMembers;
										}
										else
										//They sorted on something besides what we presorted on!
										{
											sortedList = new SortedList<string, GuildMgr.SocialWindowMember>(guildMembers.Count);
											foreach (GuildMgr.SocialWindowMember member in guildMembers.Values)
											{
												if (client.Player.Guild.SortedListOnlineMembers().ContainsKey(member.Name))
													//Update to make sure we have the most up to date info
													member.UpdateMember(client.Player.Guild.SortedListOnlineMembers()[member.Name]);
												else
													//Make sure that since they are offline they get the offline flag!
													member.GroupSize = "0";
												//Add based on the new index
												string key = member[(int)index];
												if (sortedList.ContainsKey(key))
												{
													key += sortedList.Count.ToString();
												}

												try
												{
													sortedList.Add(key, member);
												}
												catch
												{
													if (log.IsErrorEnabled)
														log.Error(string.Format("Sorted List duplicate entry - Key: {0} Member: {1}. Replacing - Member: {2}.  Sorted count: {3}.  Guild ID: {4}", key, member.Name, sortedList[key].Name, sortedList.Count, client.Player.GuildID));
												}
											}
										}

										//Finally lets send the list we made
										IList<GuildMgr.SocialWindowMember> finalList = sortedList.Values;
										int i = 0;
										string[] buffer=new string[10];
										for (i = 0; i < 10 && finalList.Count > i + (page - 1) * 10; i++)
										{
											GuildMgr.SocialWindowMember member;

											if ((int)sort > 0)
											{
												//They want it normal
												member = finalList[i + (page - 1) * 10];
											}
											else
											{
												//They want it in reverse
												member = finalList[(finalList.Count - 1) - (i + (page - 1) * 10)];
											}
											buffer[i]=member.ToString((i + 1) + (page - 1) * 10, finalList.Count);
										}
										client.Out.SendMessage("TE," + page.ToString() + "," + finalList.Count + "," + i.ToString(), eChatType.CT_SocialInterface, eChatLoc.CL_SystemWindow);
										foreach(string member in buffer)
											client.Player.Out.SendMessage(member, eChatType.CT_SocialInterface, eChatLoc.CL_SystemWindow);

                                    }
                                }
                                return;
                            }
                            #endregion
                            #region Alliance Who
                            else if (args.Length == 3)
                            {
                                if (args[2] == "alliance" || args[2] == "a")
                                {
                                    foreach (Guild guild in client.Player.Guild.alliance.Guilds)
                                    {
                                        lock (guild.ListOnlineMembers())
                                        {
                                            foreach (GamePlayer ply in guild.ListOnlineMembers())
                                            {
                                                if (ply.Client.IsPlaying && !ply.IsAnonymous)
                                                {
                                                    ind++;
                                                    string zoneName = (ply.CurrentZone == null ? "(null)" : ply.CurrentZone.Description);
                                                    string mesg = ind + ") " + ply.Name + " <guild=" + guild.Name + "> the Level " + ply.Level + " " + ply.CharacterClass.Name + " in " + zoneName;
                                                    client.Out.SendMessage(mesg, eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                                }
                                            }
                                        }
                                    }
                                    return;
                                }
                                else
                                {
                                    int.TryParse(args[2], out startInd);
                                }
                            }
                            #endregion
                            #region Who
                            IList<GamePlayer> onlineGuildMembers = client.Player.Guild.ListOnlineMembers();

                            foreach (GamePlayer ply in onlineGuildMembers)
                            {
                                if (ply.Client.IsPlaying && !ply.IsAnonymous)
                                {
                                    if (startInd + ind > startInd + WhoCommandHandler.MAX_LIST_SIZE)
                                        break;
                                    ind++;
                                    string zoneName = (ply.CurrentZone == null ? "(null)" : ply.CurrentZone.Description);
                                    string mesg;
                                    if (ply.GuildRank.Title != null)
                                        mesg = ind.ToString() + ") " + ply.Name + " <" + ply.GuildRank.Title + "> the Level " + ply.Level.ToString() + " " + ply.CharacterClass.Name + " in " + zoneName;
                                    else
                                        mesg = ind.ToString() + ") " + ply.Name + " <" + ply.GuildRank.RankLevel.ToString() + "> the Level " + ply.Level.ToString() + " " + ply.CharacterClass.Name + " in " + zoneName;
                                    if (ServerProperties.Properties.ALLOW_CHANGE_LANGUAGE)
                                        mesg += " <" + ply.Client.Account.Language + ">";
                                    if (ind >= startInd)
                                        client.Out.SendMessage(mesg, eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                }
                            }
                            if (ind != onlineGuildMembers.Count)
                                client.Out.SendMessage(string.Format(WhoCommandHandler.MESSAGE_LIST_TRUNCATED, onlineGuildMembers.Count), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                            else client.Out.SendMessage("total member online:        " + ind.ToString(), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);

                            break;
                            #endregion
                        }
                    #endregion
                    #region Leader
                    // --------------------------------------------------------------------------------
                    // LEADER
                    // --------------------------------------------------------------------------------
                    case "leader":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Leader))
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            GamePlayer obj = client.Player.TargetObject as GamePlayer;
                            if (args.Length > 2)
                            {
                                GameClient temp = WorldMgr.GetClientByPlayerName(args[2], true, false);
                                if (temp != null && GameServer.ServerRules.IsAllowedToGroup(client.Player, temp.Player, true))
                                    obj = temp.Player;
                            }
                            if (obj == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPlayerSelected"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (obj.Guild != client.Player.Guild)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotInYourGuild"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            obj.GuildRank = obj.Guild.GetRankByID(0);
                            obj.SaveIntoDatabase();
                            obj.Out.SendMessage(LanguageMgr.GetTranslation(obj.Client, "Scripts.Player.Guild.MadeLeader", obj.Name), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                            foreach (GamePlayer ply in client.Player.Guild.ListOnlineMembers())
                            {
                                ply.Out.SendMessage(LanguageMgr.GetTranslation(ply.Client, "Scripts.Player.Guild.MadeLeaderOther", client.Player.Name, obj.Name), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                            }
                            client.Player.Guild.UpdateGuildWindow();
                        }
                        break;
                    #endregion
                    #region Emblem
                    // --------------------------------------------------------------------------------
                    // EMBLEM
                    // --------------------------------------------------------------------------------
                    case "emblem":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Leader))
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (client.Player.Guild.Emblem != 0)
                            {
                                if (client.Player.TargetObject is EmblemNPC == false)
                                {
                                    client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.EmblemAlready"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    return;
                                }
                                client.Out.SendCustomDialog(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.EmblemRedo"), new CustomDialogResponse(EmblemChange));
                                return;
                            }
                            if (client.Player.TargetObject is EmblemNPC == false)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.EmblemNPCNotSelected"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            client.Out.SendEmblemDialogue();

                            client.Player.Guild.UpdateGuildWindow();
                            break;
                        }
                    #endregion
                    #region Autoremove
                    case "autoremove":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Remove))
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            if (args.Length == 4 && args[3].ToLower() == "account")
                            {
                                //#warning how can player name  !=  account if args[3] = account ?
                                string playername = args[3];
                                string accountId = "";

                                GameClient targetClient = WorldMgr.GetClientByPlayerName(args[3], false, true);
                                if (targetClient != null)
                                {
                                    OnCommand(client, new string[] { "gc", "remove", args[3] });
                                    accountId = targetClient.Account.ObjectId;
                                }
                                else
                                {
                                    Character c = (Character)GameServer.Database.SelectObject(typeof(Character), "Name = '" + GameServer.Database.Escape(playername) + "'");
                                    //if (c == null)
                                    //c = (Character)GameServer.Database.SelectObject(typeof(CharacterArchive), "Name = '" + GameServer.Database.Escape(playername) + "'");

                                    if (c == null)
                                    {
                                        client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.PlayerNotFound"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                        return;
                                    }

                                    accountId = c.Name;
                                }
                                List<Character> chars = new List<Character>();
                                chars.AddRange((Character[])GameServer.Database.SelectObjects(typeof(Character), "AccountID = '" + accountId + "'"));
                                //chars.AddRange((Character[])GameServer.Database.SelectObjects(typeof(CharacterArchive), "AccountID = '" + accountId + "'"));

                                foreach (Character ply in chars)
                                {
                                    ply.GuildID = "";
                                    ply.GuildRank = 0;
                                    GameServer.Database.SaveObject(ply);
                                }
                                break;
                            }
                            else if (args.Length == 3)
                            {
                                GameClient targetClient = WorldMgr.GetClientByPlayerName(args[2], false, true);
                                if (targetClient != null)
                                {
                                    OnCommand(client, new string[] { "gc", "remove", args[2] });
                                    return;
                                }
                                else
                                {
                                    Character c = (Character)GameServer.Database.SelectObject(typeof(Character), "Name = '" + GameServer.Database.Escape(args[2]) + "'");
                                    //if (c == null)
                                    //    c = (Character)GameServer.Database.SelectObject(typeof(CharacterArchive), "Name = '" + GameServer.Database.Escape(args[2]) + "'");
                                    if (c == null)
                                    {
                                        client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.PlayerNotFound"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                        return;
                                    }
                                    if (c.GuildID != client.Player.GuildID)
                                    {
                                        client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotInYourGuild"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                        return;
                                    }
                                    else
                                    {
                                        c.GuildID = "";
                                        c.GuildRank = 0;
                                        GameServer.Database.SaveObject(c);
                                    }
                                }
                                break;
                            }
                            else
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildAutoRemoveAcc"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildAutoRemove"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                            }
                            client.Player.Guild.UpdateGuildWindow();
                        }
                        break;
                    #endregion
                    #region MOTD
                    // --------------------------------------------------------------------------------
                    // MOTD
                    // --------------------------------------------------------------------------------
                    case "motd":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Leader))
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            message = String.Join(" ", args, 2, args.Length - 2);
                            client.Player.Guild.Motd = message;
                            client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.MotdSet"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                            client.Player.Guild.UpdateGuildWindow();
                        }
                        break;
                    #endregion
                    #region AMOTD
                    // --------------------------------------------------------------------------------
                    // AMOTD
                    // --------------------------------------------------------------------------------
                    case "amotd":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Leader))
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
							if (client.Player.Guild.AllianceId == string.Empty)
							{
								client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return;
							}
                            message = String.Join(" ", args, 2, args.Length - 2);
                            client.Player.Guild.alliance.Dballiance.Motd = message;
                            GameServer.Database.SaveObject(client.Player.Guild.alliance.Dballiance);
                            client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.AMotdSet"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                            client.Player.Guild.UpdateGuildWindow();
                        }
                        break;
                    #endregion
                    #region OMOTD
                    // --------------------------------------------------------------------------------
                    // OMOTD
                    // --------------------------------------------------------------------------------
                    case "omotd":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Leader))
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            message = String.Join(" ", args, 2, args.Length - 2);
                            client.Player.Guild.Omotd = message;
                            client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.OMotdSet"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                            client.Player.Guild.UpdateGuildWindow();
                        }
                        break;
                    #endregion
                    #region Alliance
                    // --------------------------------------------------------------------------------
                    // ALLIANCE
                    // --------------------------------------------------------------------------------
                    case "alliance":
                        {
							if (client.Player.Guild == null)
							{
								client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return;
							}

                            Alliance alliance = null;
							if (client.Player.Guild.AllianceId != null && client.Player.Guild.AllianceId != string.Empty)
                            {
                                alliance = client.Player.Guild.alliance;
                            }
                            else
                            {
                                DisplayMessage(client, "Your guild is not a member of an alliance!");
                                return;
                            }

                            DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.AllianceInfo", alliance.Dballiance.AllianceName));
                            DBGuild leader = alliance.Dballiance.DBguildleader;
                            if (leader != null)
                                DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.AllianceLeader", leader.GuildName));
                            else
                                DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.AllianceNoLeader"));

                            DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.AllianceMembers"));
                            int i = 0;
                            foreach (DBGuild guild in alliance.Dballiance.DBguilds)
                                if (guild != null)
                                    DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.AllianceMember", i++, guild.GuildName));
                            client.Player.Guild.UpdateGuildWindow();
                            return;
                        }
                    #endregion
                    #region Alliance Invite
                    // --------------------------------------------------------------------------------
                    // AINVITE
                    // --------------------------------------------------------------------------------
                    case "ainvite":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Alli))
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            GamePlayer obj = client.Player.TargetObject as GamePlayer;
                            if (obj == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPlayerSelected"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (obj.GuildRank.RankLevel != 0)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.AllianceNoGMSelected"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (obj.Guild.alliance != null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.AllianceAlreadyOther"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (ServerProperties.Properties.ALLIANCE_MAX == 0)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.AllianceDisabled"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (ServerProperties.Properties.ALLIANCE_MAX != -1)
                            {
                                if (client.Player.Guild.alliance != null)
                                {
                                    if (client.Player.Guild.alliance.Guilds.Count + 1 > ServerProperties.Properties.ALLIANCE_MAX)
                                    {
                                        client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.AllianceMax"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                        return;
                                    }
                                }
                            }
                            obj.TempProperties.setProperty("allianceinvite", client.Player); //finish that
                            client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.AllianceInvite"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                            obj.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.AllianceInvited", client.Player.Guild.Name), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                            client.Player.Guild.UpdateGuildWindow();
                            return;
                        }
                    #endregion
                    #region Alliance Invite Accept
                    // --------------------------------------------------------------------------------
                    // AINVITE
                    // --------------------------------------------------------------------------------
                    case "aaccept":
                        {
                            AllianceInvite(client.Player, 0x01);
                            client.Player.Guild.UpdateGuildWindow();
                            return;
                        }
                    #endregion
                    #region Alliance Invite Cancel
                    // --------------------------------------------------------------------------------
                    // ACANCEL
                    // --------------------------------------------------------------------------------
                    case "acancel":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Alli))
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            GamePlayer obj = client.Player.TargetObject as GamePlayer;
                            if (obj == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPlayerSelected"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            GamePlayer inviter = client.Player.TempProperties.getProperty<object>("allianceinvite", null) as GamePlayer;
                            if (inviter == client.Player)
                                obj.TempProperties.removeProperty("allianceinvite");
                            client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.AllianceAnsCancel"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                            obj.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.AllianceAnsCancel"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                            return;
                        }
                    #endregion
                    #region Alliance Invite Decline
                    // --------------------------------------------------------------------------------
                    // ADECLINE
                    // --------------------------------------------------------------------------------
                    case "adecline":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Alli))
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            GamePlayer inviter = client.Player.TempProperties.getProperty<object>("allianceinvite", null) as GamePlayer;
                            client.Player.TempProperties.removeProperty("allianceinvite");
                            client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.AllianceDeclined"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                            inviter.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.AllianceDeclinedOther"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                            return;
                        }
                    #endregion
                    #region Alliance Remove
                    // --------------------------------------------------------------------------------
                    // AREMOVE
                    // --------------------------------------------------------------------------------
                    case "aremove":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Alli))
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (client.Player.Guild.alliance == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.AllianceNotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (client.Player.Guild.GuildID != client.Player.Guild.alliance.Dballiance.DBguildleader.GuildID)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.AllianceNotLeader"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (args.Length > 3)
                            {
                                if (args[2] == "alliance")
                                {
                                    try
                                    {
                                        int index = Convert.ToInt32(args[3]);
                                        Guild myguild = (Guild)client.Player.Guild.alliance.Guilds[index];
                                        if (myguild != null)
                                            client.Player.Guild.alliance.RemoveGuild(myguild);
                                    }
                                    catch
                                    {
                                        client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.AllianceIndexNotVal"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }

                                }
                                client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildARemove"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildARemoveAlli"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            else
                            {
                                GamePlayer obj = client.Player.TargetObject as GamePlayer;
                                if (obj == null)
                                {
                                    client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPlayerSelected"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    return;
                                }
                                if (obj.Guild == null)
                                {
                                    client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.AllianceMemNotSel"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    return;
                                }
                                if (obj.Guild.alliance != client.Player.Guild.alliance)
                                {
                                    client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.AllianceMemNotSel"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    return;
                                }
                                client.Player.Guild.alliance.RemoveGuild(obj.Guild);
                            }
                            client.Player.Guild.UpdateGuildWindow();
                            return;
                        }
                    #endregion
                    #region Alliance Leave
                    // --------------------------------------------------------------------------------
                    // ALEAVE
                    // --------------------------------------------------------------------------------
                    case "aleave":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Alli))
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (client.Player.Guild.alliance == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.AllianceNotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            client.Player.Guild.alliance.RemoveGuild(client.Player.Guild);
                            client.Player.Guild.UpdateGuildWindow();
                            return;
                        }
                    #endregion
                    #region Claim
                    // --------------------------------------------------------------------------------
                    //ClAIM
                    // --------------------------------------------------------------------------------
                    case "claim":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            AbstractGameKeep keep = KeepMgr.getKeepCloseToSpot(client.Player.CurrentRegionID, client.Player, WorldMgr.VISIBILITY_DISTANCE);
                            if (keep == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.ClaimNotNear"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (keep.CheckForClaim(client.Player))
                            {
                                keep.Claim(client.Player);
                            }
                            client.Player.Guild.UpdateGuildWindow();
                            return;
                        }
                    #endregion
                    #region Release
                    // --------------------------------------------------------------------------------
                    //RELEASE
                    // --------------------------------------------------------------------------------
                    case "release":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (client.Player.Guild.ClaimedKeeps.Count == 0)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoKeep"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Release))
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
   							if (client.Player.Guild.ClaimedKeeps.Count == 1)
                     		{
                                client.Player.Guild.ClaimedKeeps[0].Release();
                    		}
							else
							{
								foreach (AbstractArea area in client.Player.CurrentAreas)
								{
									if (area is KeepArea && ((KeepArea)area).Keep.Guild == client.Player.Guild)
										((KeepArea)area).Keep.Release();
								}
							}
                            client.Player.Guild.UpdateGuildWindow();
                            return;
                        }
                    #endregion
                    #region Upgrade
                    // --------------------------------------------------------------------------------
                    //UPGRADE
                    // --------------------------------------------------------------------------------
                    case "upgrade":
                        {
                            client.Out.SendMessage("Keep upgrading is currently disabled!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return;
							/* un-comment this to work on allowing keep upgrading
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (client.Player.Guild.ClaimedKeeps.Count == 0)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoKeep"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Upgrade))
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (args.Length != 3)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.KeepNoLevel"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            byte targetlevel = 0;
                            try
                            {
                                targetlevel = Convert.ToByte(args[2]);
                                if (targetlevel > 10 || targetlevel < 1)
                                    return;
                            }
                            catch
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.UpgradeScndArg"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                return;
                            }
							if (client.Player.Guild.ClaimedKeeps.Count == 1)
							{
								foreach (AbstractGameKeep keep in client.Player.Guild.ClaimedKeeps)
									keep.StartChangeLevel(targetlevel);
							}
							else
							{
								foreach (AbstractArea area in client.Player.CurrentAreas)
								{
									if (area is KeepArea && ((KeepArea)area).Keep.Guild == client.Player.Guild)
										((KeepArea)area).Keep.StartChangeLevel(targetlevel);
								}
							}
                            client.Player.Guild.UpdateGuildWindow();
                            return;
							*/
                        }
                    #endregion
                    #region Type
                    //TYPE
                    // --------------------------------------------------------------------------------
                    case "type":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (client.Player.Guild.ClaimedKeeps.Count == 0)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoKeep"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Upgrade))
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            int type = 0;
                            try
                            {
                                type = Convert.ToInt32(args[2]);
                                if (type != 1 || type != 2 || type != 4)
                                    return;
                            }
                            catch
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.UpgradeScndArg"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            //client.Player.Guild.ClaimedKeep.Release();
                            client.Player.Guild.UpdateGuildWindow();
                            return;
                        }
                    #endregion
                    #region Noteself
                    case "noteself":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            string note = String.Join(" ", args, 2, args.Length - 2);
                            client.Player.GuildNote = note;
                            client.Player.SaveIntoDatabase();
                            client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoteSet", note), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            client.Player.Guild.UpdateGuildWindow();
                            break;
                        }
                    #endregion
                    #region Dues
                    case "dues":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                            if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Dues))
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (client.Player.Guild.GuildLevel < 5)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.GuildLevelReq"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (args[2] == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildDues"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            long amount = long.Parse(args[2]);
                            if (amount == 0)
                            {
                                client.Player.Guild.SetGuildDues(false);
                                client.Player.Guild.SetGuildDuesPercent(0);
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.DuesOff"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                            }
                            else if (amount > 0 && amount <= 100)
                            {
                                client.Player.Guild.SetGuildDues(true);
                                if (ServerProperties.Properties.NEW_GUILD_DUES)
                                {
                                    client.Player.Guild.SetGuildDuesPercent(amount);
                                    client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.DuesOn", amount), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                }
                                else
                                {
                                    client.Player.Guild.SetGuildDuesPercent(2);
                                    client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.DuesOn", 2), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                }

                                client.Player.Guild.SetGuildDuesPercent(amount);
                            }
                            else
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildDues"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                            }
                            client.Player.Guild.UpdateGuildWindow();
                        }
                        break;
                    #endregion
                    #region Deposit
                    case "deposit":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }

                            double amount = double.Parse(args[2]);
                            if (amount < 0 || amount > 1000000001)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.DepositInvalid"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                            else if (client.Player.GetCurrentMoney() < amount)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.DepositTooMuch"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                            else
                            {
                                client.Player.Guild.SetGuildBank(client.Player, amount);
                            }
                            client.Player.Guild.UpdateGuildWindow();
                        }
                        break;
                    #endregion
                    #region Withdraw
                    case "withdraw":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                            if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Withdraw))
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            double amount = double.Parse(args[2]);
                            if (amount < 0 || amount > 1000000001)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.WithdrawInvalid"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                            else if ((client.Player.Guild.GetGuildBank() - amount) < 0)
                            {
                                client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Player.Client, "Scripts.Player.Guild.WithdrawTooMuch"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            else
                            {
                                client.Player.Guild.WithdrawGuildBank(client.Player, amount);

                            }
                            client.Player.Guild.UpdateGuildWindow();
                        }
                        break;
                    #endregion
                    #region Logins
                    case "logins":
                        {
                            client.Player.ShowGuildLogins = !client.Player.ShowGuildLogins;

                            if (client.Player.ShowGuildLogins)
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.LoginsOn"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                            }
                            else
                            {
                                client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.LoginsOff"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                            }
                            client.Player.Guild.UpdateGuildWindow();
                            break;
                        }
                    #endregion
                    #region Default
                    default:
                        {
                            client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.UnknownCommand", args[1]), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							this.DisplayHelp(client);
                        }
                        break;
                    #endregion
                }
            }
            catch (Exception e)
            {
                if (log.IsErrorEnabled)
                    log.Error("error in /gc script, " + args[1] + " command: " + e.ToString());

                DisplayHelp(client);
            }
        }

		/// <summary>
		/// method to handle the aliance invite
		/// </summary>
		/// <param name="player"></param>
		/// <param name="reponse"></param>
		protected void AllianceInvite(GamePlayer player, byte reponse)
		{
			if (reponse != 0x01)
				return; //declined

			GamePlayer inviter = player.TempProperties.getProperty<object>("allianceinvite", null) as GamePlayer;

			if (player.Guild == null)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Scripts.Player.Guild.NotMember"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (inviter == null || inviter.Guild == null)
			{
				return;
			}

			if (!player.Guild.GotAccess(player, eGuildRank.Alli))
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			player.TempProperties.removeProperty("allianceinvite");

			if (inviter.Guild.alliance == null)
			{
				//create alliance
				Alliance alli = new Alliance();
				DBAlliance dballi = new DBAlliance();
				dballi.AllianceName = inviter.Guild.Name;
				dballi.DBguildleader = null;
				dballi.Motd = "";
				alli.Dballiance = dballi;
				alli.Guilds.Add(inviter.Guild);
				inviter.Guild.alliance = alli;
				inviter.Guild.AllianceId = inviter.Guild.alliance.Dballiance.ObjectId;
			}
			inviter.Guild.alliance.AddGuild(player.Guild);
			inviter.Guild.alliance.SaveIntoDatabase();
			player.Guild.UpdateGuildWindow();
			inviter.Guild.UpdateGuildWindow();
		}

		/// <summary>
		/// method to handle the emblem change
		/// </summary>
		/// <param name="player"></param>
		/// <param name="reponse"></param>
		public static void EmblemChange(GamePlayer player, byte reponse)
		{
			if (reponse != 0x01)
				return;
			if (player.TargetObject is EmblemNPC == false)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Scripts.Player.Guild.EmblemNeedNPC"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (player.GetCurrentMoney() < GuildMgr.COST_RE_EMBLEM) //200 gold to re-emblem
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Scripts.Player.Guild.EmblemNeedGold"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			player.Out.SendEmblemDialogue();
			player.Guild.UpdateGuildWindow();
		}

		public void DisplayHelp(GameClient client)
		{
			if (client.Account.PrivLevel > 1)
			{
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildGMCommands"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildGMCreate"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildGMPurge"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildGMRename"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildGMAddPlayer"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildGMRemovePlayer"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			}
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildUsage"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildForm"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildInfo"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildRanks"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildCancel"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildDecline"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildClaim"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildQuit"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildMotd"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildAMotd"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildOMotd"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildPromote"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildDemote"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildRemove"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildRemAccount"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildEmblem"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildEdit"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildLeader"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildAccept"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildInvite"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildWho"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildList"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildAlli"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildAAccept"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildACancel"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildADecline"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildAInvite"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildARemove"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildARemoveAlli"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildNoteSelf"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildDues"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildDeposit"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildWithdraw"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildWebpage"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildEmail"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildBuff"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildBuyBanner"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildBannerSummon"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
		}

		/// <summary>
		/// method to handle commands for /gc edit
		/// </summary>
		/// <param name="client"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public int SetCmd(GameClient client, string[] args)
		{
			if (args.Length < 4)
			{
				DisplayEditHelp(client);
				return 0;
			}

			bool reponse = true;
			if (args.Length > 4)
			{
				if (args[4].StartsWith("y"))
					reponse = true;
				else if (args[4].StartsWith("n"))
					reponse = false;
				else if (args[3] != "title" && args[3] != "ranklevel")
				{
					DisplayEditHelp(client);
					return 1;
				}
			}
			byte number;
			try
			{
				number = Convert.ToByte(args[2]);
				if (number > 9 || number < 0)
					return 0;
			}
			catch
			{
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.ThirdArgNotNum"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 0;
			}

			switch (args[3])
			{
				case "title":
					{
						if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Leader))
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						string message = String.Join(" ", args, 4, args.Length - 4);
						client.Player.Guild.GetRankByID(number).Title = message;
						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.RankTitleSet", number.ToString()), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
						client.Player.Guild.UpdateGuildWindow();
					}
					break;
				case "ranklevel":
					{
						if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Leader))
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						if (args.Length >= 5)
						{
							byte lvl = Convert.ToByte(args[4]);
							client.Player.Guild.GetRankByID(number).RankLevel = lvl;
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.RankLevelSet", lvl.ToString()), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
						}
						else
						{
							DisplaySyntax(client);
						}
					}
					break;

				case "emblem":
					{
						if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Leader))
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						client.Player.Guild.GetRankByID(number).Emblem = reponse;
						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.RankEmblemSet", (reponse ? "enabled" : "disabled"), number.ToString()), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
					}
					break;
				case "gchear":
					{
						if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Leader))
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						client.Player.Guild.GetRankByID(number).GcHear = reponse;
						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.RankGCHearSet", (reponse ? "enabled" : "disabled"), number.ToString()), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
					}
					break;
				case "gcspeak":
					{
						if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Leader))
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}

						client.Player.Guild.GetRankByID(number).GcSpeak = reponse;
						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.RankGCSpeakSet", (reponse ? "enabled" : "disabled"), number.ToString()), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
					}
					break;
				case "ochear":
					{
						if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Leader))
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						client.Player.Guild.GetRankByID(number).OcHear = reponse;
						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.RankOCHearSet", (reponse ? "enabled" : "disabled"), number.ToString()), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
					}
					break;
				case "ocspeak":
					{
						if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Leader))
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						client.Player.Guild.GetRankByID(number).OcSpeak = reponse;
						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.RankOCSpeakSet", (reponse ? "enabled" : "disabled"), number.ToString()), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
					}
					break;
				case "achear":
					{
						if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Leader))
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						client.Player.Guild.GetRankByID(number).AcHear = reponse;
						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.RankACHearSet", (reponse ? "enabled" : "disabled"), number.ToString()), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
					}
					break;
				case "acspeak":
					{
						if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Leader))
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						client.Player.Guild.GetRankByID(number).AcSpeak = reponse;
						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.RankACSpeakSet", (reponse ? "enabled" : "disabled"), number.ToString()), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
					}
					break;
				case "invite":
					{
						if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Leader))
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						client.Player.Guild.GetRankByID(number).Invite = reponse;
						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.RankInviteSet", (reponse ? "enabled" : "disabled"), number.ToString()), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
					}
					break;
				case "promote":
					{
						if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Leader))
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						client.Player.Guild.GetRankByID(number).Promote = reponse;
						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.RankPromoteSet", (reponse ? "enabled" : "disabled"), number.ToString()), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
					}
					break;
				case "remove":
					{
						if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Leader))
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						client.Player.Guild.GetRankByID(number).Remove = reponse;
						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.RankRemoveSet", (reponse ? "enabled" : "disabled"), number.ToString()), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
					}
					break;
				case "alli":
					{
						if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Leader))
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						client.Player.Guild.GetRankByID(number).Alli = reponse;
						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.RankAlliSet", (reponse ? "enabled" : "disabled"), number.ToString()), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
					}
					break;
				case "view":
					{
						if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.View))
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						client.Player.Guild.GetRankByID(number).View = reponse;
						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.RankViewSet", (reponse ? "enabled" : "disabled"), number.ToString()), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
					}
					break;
				case "buff":
					{
						if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Leader))
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						client.Player.Guild.GetRankByID(number).Buff = reponse;
						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.RankBuffSet", (reponse ? "enabled" : "disabled"), number.ToString()), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
					}
					break;
				case "claim":
					{
						if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Claim))
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						client.Player.Guild.GetRankByID(number).Claim = reponse;
						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.RankClaimSet", (reponse ? "enabled" : "disabled"), number.ToString()), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
					}
					break;
				case "upgrade":
					{
						if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Upgrade))
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						client.Player.Guild.GetRankByID(number).Upgrade = reponse;
						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.RankUpgradeSet", (reponse ? "enabled" : "disabled"), number.ToString()), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
					}
					break;
				case "release":
					{
						if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Release))
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						client.Player.Guild.GetRankByID(number).Release = reponse;
						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.RankReleaseSet", (reponse ? "enabled" : "disabled"), number.ToString()), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
					}
					break;
				case "dues":
					{
						if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Dues))
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						client.Player.Guild.GetRankByID(number).Release = reponse;
						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.RankDuesSet", (reponse ? "enabled" : "disabled"), number.ToString()), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
					}
					break;
				case "withdraw":
					{
						if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.Withdraw))
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.NoPrivilages"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						client.Player.Guild.GetRankByID(number).Release = reponse;
						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.RankWithdrawSet", (reponse ? "enabled" : "disabled"), number.ToString()), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
					}
					break;
				default:
					{
						return 0;
					}
			} //switch
			DBRank rank = client.Player.Guild.GetRankByID(number);
			if (rank != null)
				GameServer.Database.SaveObject(rank);
			return 1;
		}

		public void DisplayEditHelp(GameClient client)
		{
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildUsage"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildEditTitle"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildEditRankLevel"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildEditEmblem"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildEditGCHear"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildEditGCSpeak"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildEditOCHear"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildEditOCSpeak"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildEditACHear"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildEditACSpeak"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildEditInvite"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildEditPromote"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildEditRemove"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildEditView"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildEditAlli"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildEditClaim"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildEditUpgrade"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildEditRelease"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildEditDues"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Guild.Help.GuildEditWithdraw"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
		}
	}
}
