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
using System.Reflection;
using DOL.Database;
using DOL.GS.Database;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Scripts
{
	[Cmd(
		 "&gc",
		 new string[] {"&guildcommand"},
		 (uint) ePrivLevel.Player,
		 "Guild command (use /gc help for options)",
		 "/gc <option>")]
	public class GuildCommandHandler : ICommandHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int OnCommand(GameClient client, string[] args)
		{
			try
			{
				if (args.Length == 1)
				{
                    #region Ingame display messages
                    if (client.Account.PrivLevel != ePrivLevel.Player)
                    {
                        client.Out.SendMessage("Game Master commands:", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        client.Out.SendMessage("'/gc create <Name> <player>' Create a new guild with player as leader", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        client.Out.SendMessage("'/gc addplayer <player> <guild>' to add a player to a guild", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        client.Out.SendMessage("'/gc removeplayer <player> <guild>' to remove player from a guild", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }

                    client.Out.SendMessage("Member commands:", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    client.Out.SendMessage("'/gc form <Name>' to create a new guilde with all player of group", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    client.Out.SendMessage("'/gc info' to show information on the guild", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    client.Out.SendMessage("'/gc ranks' to show information about the guild's ranks", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    client.Out.SendMessage("'/gc cancel <option> [value]' to cancel all command done before", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    client.Out.SendMessage("'/gc decline' to decline the enter in guild ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    client.Out.SendMessage("'/gc claim' to claim a keep", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    client.Out.SendMessage("'/gc quit' to leave the guild", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    client.Out.SendMessage("'/gc motd <text>' to set the message of the day of the guild", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    client.Out.SendMessage("'/gc amotd <text>' to set the message of the day of the alliance's guild", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    client.Out.SendMessage("'/gc omotd <text>' to set the message of the day of the officier's guild", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    client.Out.SendMessage("'/gc promote <rank#> [name]' to promote player to a superior rank", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    client.Out.SendMessage("'/gc demote <rank#> [name]' to set the rank from 0 to 10 of the player to an inferior rank", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    client.Out.SendMessage("'/gc remove <playername>' to remove the player from the guild", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    client.Out.SendMessage("'/gc emblem' to set emblem", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    client.Out.SendMessage("'/gc edit' to show list of all option in guild edit", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    client.Out.SendMessage("'/gc accept ' to accept invite to guild", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    client.Out.SendMessage("'/gc invite [name]' to invite targeted player to join the guild", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    client.Out.SendMessage("'/gc who' to show all player in your guild", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    client.Out.SendMessage("'/gc list' to show all guild in your realm", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    client.Out.SendMessage("'/gc aaccept' to accept an alliance invitation", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    client.Out.SendMessage("'/gc acancel' to cancel an alliance invitation", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    client.Out.SendMessage("'/gc adecline' to decline an alliance invitation", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    client.Out.SendMessage("'/gc ainvite' to invite another guild to join your alliance", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    client.Out.SendMessage("'/gc aremove' to removes a guild from your alliance", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                    return 1; 
                    #endregion
				}
				
				switch (args[1])
				{
                    #region /gc create
                    // --------------------------------------------------------------------------------
                    // CREATE
                    // --------------------------------------------------------------------------------
                    case "create":
                        {
                            if (client.Account.PrivLevel == ePrivLevel.Player)
                                return 1;
                            if (args.Length != 4)
                            {
                                client.Out.SendMessage("Syntax error! /gc create guildname leadername with no long name", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 0;
                            }
                            if (GuildMgr.GetGuildByName(args[2]) == null)
                            {
                                GameClient guildLeader = WorldMgr.GetClientByPlayerName(args[3], false);
                                if (guildLeader == null)
                                {
                                    client.Out.SendMessage("Player \"" + args[3] + "\"  not found.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    return 0;
                                }
                                Guild newGuild = GuildMgr.CreateGuild(args[2]);
                                if (newGuild != null)
                                {
                                    newGuild.AddGuildMember(guildLeader.Player, 0);
                                    client.Out.SendMessage("Create guild \"" + args[2] + "\" with player " + args[3] + " as leader!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                }
                                return 1;
                            }
                            else
                            {
                                client.Out.SendMessage("Guild " + args[2] + " cannot be created because it already exsists!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                        }
                        break; 
                    #endregion

                    #region /gc addplayer
                    // --------------------------------------------------------------------------------
                    // ADDPLAYER
                    // --------------------------------------------------------------------------------
                    case "addplayer":
                        {
                            if (client.Account.PrivLevel == ePrivLevel.Player)
                                return 1;

                            if (args.Length < 4)
                            {
                                client.Out.SendMessage("Syntax error! /gc addplayer <playername> <guildname>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 0;
                            }

                            Guild guildWhereAdd = GuildMgr.GetGuildByName(args[3]);
                            if (guildWhereAdd != null)
                            {
                                GameClient playerToAdd = WorldMgr.GetClientByPlayerName(args[2], true);
                                if (playerToAdd.Player != null)
                                {
                                    guildWhereAdd.AddGuildMember(playerToAdd.Player, 9);
                                    client.Out.SendMessage("Player " + args[2] + " added to the guild " + args[3] + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                }
                                else
                                {
                                    client.Out.SendMessage("Player " + args[2] + " not found!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                }
                            }
                            else
                            {
                                client.Out.SendMessage("The guild " + args[3] + " does not exist!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                        }
                        break; 
                    #endregion

                    #region /gc removeplayer
                    // --------------------------------------------------------------------------------
                    // REMOVEPLAYER
                    // --------------------------------------------------------------------------------
                    case "removeplayer":
                        {
                            if (client.Account.PrivLevel == ePrivLevel.Player)
                                return 1;

                            if (args.Length < 4)
                            {
                                client.Out.SendMessage("Syntax error! /gc removeplayer <playername> <guildname>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 0;
                            }

                            Guild guildWhereRemove = GuildMgr.GetGuildByName(args[3]);
                            if (guildWhereRemove != null)
                            {
                                GameClient playerToRemove = WorldMgr.GetClientByPlayerName(args[2], true);
                                if (playerToRemove.Player != null)
                                {
                                    if (guildWhereRemove.RemoveGuildMember(playerToRemove.Player) == true)
                                    {
                                        client.Out.SendMessage("Player " + args[2] + " removed from the guild " + args[3] + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                    else
                                    {
                                        client.Out.SendMessage("Can't remove the player " + args[2] + ", maybe he is not online or in another guild!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                }
                                else
                                {
                                    client.Out.SendMessage("Player " + args[2] + " not found!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                }
                            }
                            else
                            {
                                client.Out.SendMessage("The guild " + args[3] + " does not exist!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                        }
                        break; 
                    #endregion

						/****************************************guild member command***********************************************/
                    #region /gc invite
                    // --------------------------------------------------------------------------------
                    // INVITE
                    // --------------------------------------------------------------------------------
                    case "invite":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage("You have to be a member of a guild, before you can use any of the commands!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }

                            if (!client.Player.Guild.CheckGuildPermission(client.Player, eGuildPerm.Invite))
                            {
                                client.Out.SendMessage("You dont have the priviledges for that!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }

                            GamePlayer obj = client.Player.TargetObject as GamePlayer;
                            if (args.Length > 2)
                            {
                                GameClient temp = WorldMgr.GetClientByPlayerName(args[2], true);
                                if (temp != null)
                                    obj = temp.Player;
                            }
                            if (obj == null)
                            {
                                client.Out.SendMessage("You must select a player!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }
                            if (obj == client.Player)
                            {
                                client.Out.SendMessage("You can't invite yourself.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }
                            if (obj.Guild != null)
                            {
                                client.Out.SendMessage("Target is already member of a guild!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }
                            if (!obj.Alive)
                            {
                                client.Out.SendMessage("You cannot invite a dead member to your guild.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }
                            if (!GameServer.ServerRules.IsAllowedToGroup(client.Player, obj, true))
                            {
                                client.Out.SendMessage("You cannot invite this member", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }
                            obj.Out.SendDialogBox(eDialogCode.GuildInvite, (ushort)client.Player.ObjectID, 0x00, 0x00, 0x00, eDialogType.YesNo, false, client.Player.Name + " has invited you to join\n" + client.Player.GetPronoun(1, false) + " guild, [" + client.Player.Guild.GuildName + "].\n" + "Do you wish to join?");
                            client.Out.SendMessage("You have invited " + obj.Name + " to join your guild.", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                        }
                        break; 
                    #endregion

                    #region /gc remove
                    // --------------------------------------------------------------------------------
                    // REMOVE
                    // --------------------------------------------------------------------------------
                    case "remove":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage("You have to be a member of a guild, before you can use any of the commands!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }

                            if (!client.Player.Guild.CheckGuildPermission(client.Player, eGuildPerm.Remove))
                            {
                                client.Out.SendMessage("You do not have permission to remove members.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }

                            if (args.Length < 3)
                            {
                                client.Out.SendMessage("Usage: /guild remove <PlayerName>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }
                            string playername = String.Join(" ", args, 2, args.Length - 2);
                            GameClient myclient = WorldMgr.GetClientByPlayerName(playername, true);
                            if (myclient == null)
                            {
                                client.Out.SendMessage("This player name does not exist", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }
                            if (myclient.Player.Guild != client.Player.Guild)
                            {
                                client.Out.SendMessage("Player is not a member of your guild!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }
                            client.Player.Guild.SendMessageToGuildMembers("[Guild]: " + client.Player.Name + " has removed " + myclient.Player.Name + " from the guild", eChatType.CT_Guild, eChatLoc.CL_ChatWindow);

                            client.Player.Guild.RemoveGuildMember(client.Player);
                        }
                        break; 
                    #endregion

                    #region /gc info
                    // --------------------------------------------------------------------------------
                    // INFO
                    // --------------------------------------------------------------------------------
                    case "info":
                        {
                            if (args.Length == 3)
                            {
                                switch (args[2])
                                {
                                    case "1": // show guild info
                                        {
                                            if (client.Player.Guild == null)
                                                return 1;
                                            string mes = "I";
                                            //mes += ",0"; // Guild Level
                                            mes += ',' + client.Player.Guild.Level.ToString();
                                            //mes += ",0"; // Guild Bank money
                                            mes += ',' + client.Player.Guild.TotalMoney.ToString();
                                            //mes += ",0"; // Guild Dues enable/disable
                                            mes += ',' + (client.Player.Guild.Due ? "1" : "0");
                                            mes += ',' + client.Player.Guild.BountyPoints.ToString(); // Guild Bounty
                                            mes += ',' + client.Player.Guild.RealmPoints.ToString(); // Guild Experience
                                            //mes += ",0"; // Guild Merit Points
                                            mes += ',' + client.Player.Guild.MeritPoints.ToString();
                                            mes += ',' + client.Player.LotNumber.ToString(); // Guild houseLot ?
                                            mes += ',' + (client.Player.Guild.ListOnlineMembers.Count + 1).ToString(); // online Guild member ?
                                            mes += ",\"not implemented\""; //"Banner available for purchase", "Missing banner buying permissions"
                                            mes += ",\"" + client.Player.Guild.Motd + '\"'; // Guild Motd
                                            if (client.Player.Guild.CheckGuildPermission(client.Player, eGuildPerm.OcHear))
                                                mes += ",\"" + client.Player.Guild.OMotd + '\"'; // Guild oMotd
                                            else
                                                mes += ",\"" + '\"';
                                            client.Out.SendMessage(mes, eChatType.CT_SocialInterface, eChatLoc.CL_SystemWindow);
                                            break;
                                        }
                                    case "2": //enable/disable social windows
                                        {
                                            // "P,ShowGuildWindow,ShowAllianceWindow,?,ShowLFGuildWindow(only with guild),0,0" // news and friend windows always showed
                                            client.Out.SendMessage("P," + (client.Player.Guild == null ? "0" : "1") + ",0,0,0,0,0", eChatType.CT_SocialInterface, eChatLoc.CL_SystemWindow);
                                            break;
                                        }
                                    default:
                                        break;
                                }
                                return 1;

                            }

							//[11:44:35] Guild Info for Mularn Protectors:
							//[11:44:35] Realm Points : 495536   Bounty Points : 14810  Merit Points : 404244
							//[11:44:35] Guild Level: 8
							//[11:44:35] Dues: 0% Bank:  0 copper pieces
							//[11:44:35] Current Merit Bonus: None
							//[11:44:35] Banner available for purchase
							//[11:44:35] Webpage: www.camelot-europe.com
							//[11:44:35] Contact Email: 
							//[11:44:35] Message: Use /gu <text> to talk to players in this startup guild.
							//[11:44:35] Officer Message: Type /gc quit to leave this startup guild.
							//[11:44:35] Alliance Message: The alliance chat (/asend TEXT) allows you to communicate with members of other new-player guilds.
							
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage("You have to be a member of a guild, before you can use any of the commands!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }

                            client.Out.SendMessage("Guild Info for " + client.Player.Guild.GuildName + ":", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            client.Out.SendMessage("Realm Points : " + client.Player.Guild.RealmPoints + "   Bounty Points : " + client.Player.Guild.BountyPoints + "  Merit Points : " + client.Player.Guild.MeritPoints, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            client.Out.SendMessage("Guild Level: " + client.Player.Guild.Level, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            client.Out.SendMessage("Webpage: " + client.Player.Guild.Webpage, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            client.Out.SendMessage("Contact Email: " + client.Player.Guild.Email, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            client.Out.SendMessage("Message: " + client.Player.Guild.Motd, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            if (client.Player.Guild.CheckGuildPermission(client.Player, eGuildPerm.OcHear))
                                client.Out.SendMessage("Officier Message: " + client.Player.Guild.OMotd, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            if ((client.Player.Guild.Alliance != null) && (client.Player.Guild.Alliance.AllianceGuilds.Count > 1) && client.Player.Guild.CheckGuildPermission(client.Player, eGuildPerm.AcHear))    
                                client.Out.SendMessage("Alliance Message: " + client.Player.Guild.Alliance.AMotd, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }
                        break;
                    #endregion

                    #region /gc ranks
                    // --------------------------------------------------------------------------------
                    // RANKS
                    // --------------------------------------------------------------------------------
                    case "ranks":
                        //[11:44:49] Guild Info for Mularn Protectors:
                        //[11:44:49] Guildmaster Title: 
                        //[11:44:49] Rank 1:  (Rank Level 0)
                        //[11:44:49]   Officer Chat - Hear N Speak N
                        //[11:44:49]   Guild Chat - Hear N Speak N
                        //[11:44:49]   Alliance Chat - Hear N Speak N
                        //[11:44:49]   Invite: N  Remove: N  Promote: N
                        //[11:44:49]   Deposit: N  Withdraw: N  View: N  Emblem: N
                        //[11:44:49]   Set Dues: N  Set Buff: N Get Mission: N Set Note: N Summon Banner: N Buy Banner: N
                        //[11:44:49]   Outposts - Claim: N  Release: N  Upgrade: N Purchase: N
                        //[11:44:49]   MOTD Edit: N  Alliances: N
                        if (client.Player.Guild == null)
                        {
                            client.Out.SendMessage("You have to be a member of a guild, before you can use any of the commands!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return 1;
                        }

                        client.Out.SendMessage("Guild Info for " + client.Player.Guild.GuildName + ":", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        for (byte i = 0; i < client.Player.Guild.GuildRanks.Length; i++)
                        {
                            DBGuildRank rank = client.Player.Guild.GuildRanks[i];
                            if (i == 0)
                            {
                                client.Out.SendMessage("GuildMaster Title: " + rank.Title, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                            else
                            {
                                client.Out.SendMessage("Rank " + rank.RankLevel.ToString() + ":  " + rank.Title, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage(" Officier Chat - Hear: " + (rank.OcHear ? "Y" : "N") + " Speak: " + (rank.OcSpeak ? "Y" : "N"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage(" Guild Chat - Hear: " + (rank.GcHear ? "Y" : "N") + " Speak: " + (rank.GcSpeak ? "Y" : "N"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage(" Alliance Chat - Hear: " + (rank.AcHear ? "Y" : "N") + " Speak: " + (rank.AcSpeak ? "Y" : "N"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage(" Invite: " + (rank.Invite ? "Y" : "N") + "  Remove: " + (rank.Remove ? "Y" : "N") + "  Promote: " + (rank.Promote ? "Y" : "N"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage(" Deposit: " +  (rank.Deposit ? "Y" : "N") + "  Withdraw: " + (rank.Withdraw ? "Y" : "N") + "  View: " + (rank.View ? "Y" : "N") + "  Emblem:" + (rank.Emblem ? "Y" : "N") , eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage(" Set Dues: " + (rank.Dues ? "Y" : "N") + "  Set Buff: " + (rank.Buff ? "Y" : "N") + " Get Mission: " + (rank.GetMission ? "Y" : "N") + " Set Note:"
                                	+ (rank.SetNote ? "Y" : "N") + " Summon Banner: " + (rank.SummonBanner ? "Y" : "N") + " Buy Banner: " + (rank.BuyBanner ? "Y" : "N") , eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage(" Outposts - Claim: " + (rank.Claim ? "Y" : "N") + "  Release: " + (rank.Release ? "Y" : "N") + "  Upgrade: " + (rank.Upgrade ? "Y" : "N"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage(" MOTD Edit: " + (rank.Motd ? "Y" : "N") + "  Alliances: " + (rank.Alli ? "Y" : "N"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                        }
                        break;
                    #endregion

                    #region /gc list
                    // --------------------------------------------------------------------------------
                    // LIST
                    // --------------------------------------------------------------------------------
                    case "list":
                        {
                            foreach (Guild gui in GuildMgr.AllGuilds)
                            {
                                string mesg = gui.GuildName + "  " + gui.ListOnlineMembers.Count + " members ";
                                client.Out.SendMessage(mesg, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                        }
                        break; 
                    #endregion

                    #region /gc edit
                    // --------------------------------------------------------------------------------
                    // EDIT
                    // --------------------------------------------------------------------------------
                    case "edit":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage("You have to be a member of a guild, before you can use any of the commands!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }

                            if (client.Player.GuildRank > 0)
                            {
                                client.Out.SendMessage("Only guild masters can change the guild settings!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }

                            if (args.Length < 4)
                            {
                                client.Out.SendMessage("Usage : ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage("'/gc edit <ranknum> title <text>' sets the title for a specific rank <#> for this guild", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage("'/gc edit <ranknum> emblem <y/n>' used the edit the rank of members who are allowed to wear the guild emblem on their cloak or shield.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage("'/gc edit <ranknum> gchear <y/n>' edits this rank to be able to hear guild chat", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage("'/gc edit <ranknum> gcspeak <y/n>' edits this rank to be able to speak in guild chat", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage("'/gc edit <ranknum> ochear <y/n>' edits this rank to be able to hear Officer chat", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage("'/gc edit <ranknum> ocspeak <y/n>' edits this rank to be able to speak in Officer chat", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage("'/gc edit <ranknum> achear <y/n>' edits this rank to be able to hear Alliance chat", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage("'/gc edit <ranknum> acspeak <y/n>' edits this rank to be able to speak in Alliance chat", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage("'/gc edit <ranknum> invite <y/n>' edits this rank with the ability to invite members into the guild", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage("'/gc edit <ranknum> promote <y/n>' edits this rank with the ability to promote/demote lower in rank", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage("'/gc edit <ranknum> remove <y/n>' edits this rank with the ability to remove those below him in rank", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage("'/gc edit <ranknum> view <y/n>'  edits this rank with the ability to display the /gc info screen", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage("'/gc edit <ranknum> alli <y/n>'  to allow a rank to enter/leave alliances (create a diplomatic officer)", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage("'/gc edit <ranknum> claim <y/n>'  to allow a rank to enter/leave alliances (create a diplomatic officer)", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage("'/gc edit <ranknum> upgrade <y/n>'  to allow a rank to enter/leave alliances (create a diplomatic officer)", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage("'/gc edit <ranknum> release <y/n>'  to set release the keep you have claim before", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage("'/gc edit <ranknum> motd <y/n>'  to allow a rank to edit the guild motd", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage("'/gc edit <ranknum> deposit <y/n>'  to allow a rank to edit the guild motd", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage("'/gc edit <ranknum> withdraw <y/n>'  to allow a rank to edit the guild motd", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage("'/gc edit <ranknum> due <y/n>'  to allow a rank to edit the guild motd", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage("'/gc edit <ranknum> getmission <y/n>'  to allow a rank to edit the guild motd", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage("'/gc edit <ranknum> buff <y/n>'  to allow a rank to edit the guild motd", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage("'/gc edit <ranknum> setnote <y/n>'  to allow a rank to edit the guild motd", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage("'/gc edit <ranknum> summonbanner <y/n>'  to allow a rank to edit the guild motd", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage("'/gc edit <ranknum> buybanner <y/n>'  to allow a rank to edit the guild motd", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 0;
                            }

                            bool reponse = false;
                            if (args[3] != "title" && args[4].Equals("y"))
                            {
                                reponse = true;
                            }


                            byte rankNumber;
                            try
                            {
                                rankNumber = Convert.ToByte(args[2]);
                                rankNumber = Math.Min((byte)0, rankNumber);
                                rankNumber = Math.Max((byte)9, rankNumber);
                            }
                            catch
                            {
                                client.Out.SendMessage("The <ranknum> argument must be a number between 0 and 9.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 0;
                            }

                            switch (args[3])
                            {
                                case "title":
                                    {
                                        string texte = args[4];
                                        client.Player.Guild.GuildRanks[rankNumber].Title = texte;
                                        client.Out.SendMessage("You have set the title of the rank " + rankNumber + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                    break;

                                case "emblem":
                                    {
                                        client.Player.Guild.GuildRanks[rankNumber].Emblem = reponse;
                                        client.Out.SendMessage("You have " + (reponse ? "allow" : "unallow") + " the emblem permission to the rank " + rankNumber + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                    break;
                                case "gchear":
                                    {
                                        client.Player.Guild.GuildRanks[rankNumber].GcHear = reponse;
                                        client.Out.SendMessage("You have " + (reponse ? "allow" : "unallow") + " the guild chat hear permission for the rank " + rankNumber + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                    break;
                                case "gcspeak":
                                    {
                                        client.Player.Guild.GuildRanks[rankNumber].GcSpeak = reponse;
                                        client.Out.SendMessage("You have " + (reponse ? "allow" : "unallow") + " the guild chat speak permission for the rank " + rankNumber + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                    break;
                                case "ochear":
                                    {
                                        client.Player.Guild.GuildRanks[rankNumber].OcHear = reponse;
                                        client.Out.SendMessage("You have " + (reponse ? "allow" : "unallow") + " the officer chat hear permission for the rank " + rankNumber + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                    break;
                                case "ocspeak":
                                    {
                                        client.Player.Guild.GuildRanks[rankNumber].OcSpeak = reponse;
                                        client.Out.SendMessage("You have " + (reponse ? "allow" : "unallow") + " the officer chat speak permission for the rank " + rankNumber + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                    break;
                                case "achear":
                                    {
                                        client.Player.Guild.GuildRanks[rankNumber].AcHear = reponse;
                                        client.Out.SendMessage("You have " + (reponse ? "allow" : "unallow") + " the alliance chat hear permission for the rank " + rankNumber + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                    break;
                                case "acspeak":
                                    {
                                        client.Player.Guild.GuildRanks[rankNumber].AcSpeak = reponse;
                                        client.Out.SendMessage("You have " + (reponse ? "allow" : "unallow") + " the alliance chat speak permission for the rank " + rankNumber + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                    break;
                                case "invite":
                                    {
                                        client.Player.Guild.GuildRanks[rankNumber].Invite = reponse;
                                        client.Out.SendMessage("You have " + (reponse ? "allow" : "unallow") + " the guild invite permission for the rank " + rankNumber + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                    break;
                                case "promote":
                                    {
                                        client.Player.Guild.GuildRanks[rankNumber].Promote = reponse;
                                        client.Out.SendMessage("You have " + (reponse ? "allow" : "unallow") + " the guild promote permission for the rank " + rankNumber + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                    break;
                                case "remove":
                                    {
                                        client.Player.Guild.GuildRanks[rankNumber].Remove = reponse;
                                        client.Out.SendMessage("You have " + (reponse ? "allow" : "unallow") + " the guild remove permission for the rank " + rankNumber + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                    break;
                                case "alli":
                                    {
                                        client.Player.Guild.GuildRanks[rankNumber].Alli = reponse;
                                        client.Out.SendMessage("You have " + (reponse ? "allow" : "unallow") + " the rank " + rankNumber + " to use aliiance commands.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                    break;
                                case "view":
                                    {
                                        client.Player.Guild.GuildRanks[rankNumber].View = reponse;
                                        client.Out.SendMessage("You have " + (reponse ? "allow" : "unallow") + " the rank " + rankNumber + " to view guild informations.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                    break;
                                case "claim":
                                    {
                                        client.Player.Guild.GuildRanks[rankNumber].Claim = reponse;
                                        client.Out.SendMessage("You have " + (reponse ? "allow" : "unallow") + " the rank " + rankNumber + " to claim keep.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                    break;
                                case "upgrade":
                                    {
                                        client.Player.Guild.GuildRanks[rankNumber].Upgrade = reponse;
                                        client.Out.SendMessage("You have " + (reponse ? "allow" : "unallow") + " the rank " + rankNumber + " to upgrade keep.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                    break;
                                case "release":
                                    {
                                        client.Player.Guild.GuildRanks[rankNumber].Release = reponse;
                                        client.Out.SendMessage("You have " + (reponse ? "allow" : "unallow") + " the rank " + rankNumber + " to release keep.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                    break;
                                case "motd":
                                    {
                                        client.Player.Guild.GuildRanks[rankNumber].Motd = reponse;
                                        client.Out.SendMessage("You have " + (reponse ? "allow" : "unallow") + " the rank " + rankNumber + " to edit the guild motd.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                    break;
                                case "deposit":
                                    {
                                        client.Player.Guild.GuildRanks[rankNumber].Deposit = reponse;
                                        client.Out.SendMessage("You have " + (reponse ? "allow" : "unallow") + " the rank " + rankNumber + " to deposit gold in the guild bank", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                    break;
                                case "withdraw":
                                    {
                                        client.Player.Guild.GuildRanks[rankNumber].Withdraw = reponse;
                                        client.Out.SendMessage("You have " + (reponse ? "allow" : "unallow") + " the rank " + rankNumber + " to withdraw gold from the guild bank.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                    break;
                                case "due":
                                    {
                                        client.Player.Guild.GuildRanks[rankNumber].Dues = reponse;
                                        client.Out.SendMessage("You have " + (reponse ? "allow" : "unallow") + " the rank " + rankNumber + " to set due for the guild", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                    break;
                                case "getmission":
                                    {
                                        client.Player.Guild.GuildRanks[rankNumber].GetMission = reponse;
                                        client.Out.SendMessage("You have " + (reponse ? "allow" : "unallow") + " the rank " + rankNumber + " to get guild's mission (not implemented yet).", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                    break;
                                case "buff":
                                    {
                                        client.Player.Guild.GuildRanks[rankNumber].Buff = reponse;
                                        client.Out.SendMessage("You have " + (reponse ? "allow" : "unallow") + " the rank " + rankNumber + " to set the guild's buff (not implemented yet).", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                    break;
                                case "setnote":
                                    {
                                        client.Player.Guild.GuildRanks[rankNumber].SetNote = reponse;
                                        client.Out.SendMessage("You have " + (reponse ? "allow" : "unallow") + " the rank " + rankNumber + " to set note for a guild member.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                    break;
                                case "summonbanner":
                                    {
                                        client.Player.Guild.GuildRanks[rankNumber].SummonBanner = reponse;
                                        client.Out.SendMessage("You have " + (reponse ? "allow" : "unallow") + " the rank " + rankNumber + " to summon the guild banner (not implemented yet)", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                    break;
                                case "buybanner":
                                    {
                                        client.Player.Guild.GuildRanks[rankNumber].BuyBanner = reponse;
                                        client.Out.SendMessage("You have " + (reponse ? "allow" : "unallow") + " the rank " + rankNumber + " to buy guild banner (not implemented yet)", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                    break;
                                default:
                                    {
                                        return 0;
                                    }
                            } //switch
                            GameServer.Database.SaveObject(client.Player.Guild.GuildRanks[rankNumber]);
                            return 1;
                        } 
                    #endregion

                    #region /gc form
                    // --------------------------------------------------------------------------------
                    // FORM
                    // --------------------------------------------------------------------------------
                    case "form":
						{
							if (args.Length < 3)
							{
								client.Out.SendMessage("Syntax error! /gc form <guildname>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}

							string guildname = String.Join(" ", args, 2, args.Length - 2);

							if (guildname.Length > Guild.MAX_GUILD_NAME_LENGTH)
							{
								client.Out.SendMessage("Guildnames can not be longer than " + Guild.MAX_GUILD_NAME_LENGTH + " characters!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}

							bool isValid = true;
							foreach (char c in guildname.ToCharArray())
							{
								if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')
									|| c == 'à' || c == 'á' || c == 'é' || c == 'è'
									|| c == 'ê' || c == 'ç' || c == 'ù' || c == 'î'
									|| c == ' ' || c == '\'')
								{
									continue;
								}
								isValid = false;
							} 
							
							if(isValid == false)
							{
								client.Out.SendMessage("The guildname must consist of valid characters!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							} 

							IList allInvalidNames = GameServer.Database.SelectAllObjects(typeof(InvalidName));
							string ToLowerNewGuildname = guildname.ToLower();
							foreach (InvalidName inv in allInvalidNames)
							{
								if (ToLowerNewGuildname.IndexOf(inv.Name.ToLower()) != -1)
								{
									client.Out.SendMessage(guildname + " is not a legal guild name! Choose another.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
									return 0;
								}
							} 
							
                            if (GuildMgr.GetGuildByName(guildname) == null)
                            {
                                PlayerGroup group = client.Player.PlayerGroup;

                                if (group == null)
                                {
                                    client.Out.SendMessage("You must have a group to create a guild.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    return 0;
                                }

                                lock (group)
                                {
                                    if (group.PlayerCount < PlayerGroup.MAX_GROUP_SIZE)
                                    {
                                        client.Out.SendMessage(PlayerGroup.MAX_GROUP_SIZE + " members must be in group to create a guild.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                        return 0;
                                    }

                                    // a member of group have a guild already, so quit!
                                    foreach (GamePlayer ply in group)
                                    {
                                        if (ply.Guild != null)
                                        {
                                            client.Player.PlayerGroup.SendMessageToGroupMembers(ply.Name + " is already member of a guild!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                            return 0;
                                        }
                                    }

                                    Guild newGuild = GuildMgr.CreateGuild(guildname);
                                    if (newGuild == null)
                                    {
                                        client.Out.SendMessage("Unable to create guild \"" + guildname + "\" with player " + client.Player.Name + " as leader!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                    else
                                    {
                                        foreach (GamePlayer ply in group)
                                        {
                                            if (ply != client.Player)
                                                newGuild.AddGuildMember(ply, 9);
                                            else
                                                newGuild.AddGuildMember(ply, 0);
                                        }
                                        client.Out.SendMessage("Create guild \"" + guildname + "\" with player " + client.Player.Name + " as leader!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                }
                            }
                            else
                            {
                                client.Out.SendMessage("Guild " + guildname + " cannot be created because it already exsists!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                        }
                        break; 
                    #endregion

                    #region /gc quit
                    // --------------------------------------------------------------------------------
                    // QUIT
                    // --------------------------------------------------------------------------------
                    case "quit":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage("You have to be a member of a guild, before you can use any of the commands!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }
							client.Out.SendDialogBox(eDialogCode.GuildLeave, (ushort)client.Player.ObjectID, 0x00, 0x00, 0x00, eDialogType.YesNo, false, "Do you really want to leave\n" + client.Player.Guild.GuildName + "?");
                        }
                        break; 
                    #endregion

                    #region /gc promote
                    // --------------------------------------------------------------------------------
                    // PROMOTE
                    // --------------------------------------------------------------------------------
                    case "promote":
                        {
                            if (args.Length < 3)
                            {
                                client.Out.SendMessage("Syntax error! /gc promote <newrank> [name]", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 0;
                            }

                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage("You have to be a member of a guild, before you can use any of the commands!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }
                            if (!client.Player.Guild.CheckGuildPermission(client.Player, eGuildPerm.Promote))
                            {
                                client.Out.SendMessage("You dont have the priviledges for that!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }
                            GamePlayer obj = client.Player.TargetObject as GamePlayer;
                            if (args.Length > 3)
                            {
                                GameClient temp = WorldMgr.GetClientByPlayerName(args[3], true);
                                if (temp != null && GameServer.ServerRules.IsAllowedToGroup(client.Player, temp.Player, true))
                                    obj = temp.Player;
                            }
                            if (obj == null)
                            {
                                client.Out.SendMessage("You must select a player!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }
                            if (obj.Guild != client.Player.Guild)
                            {
                                client.Out.SendMessage(obj.Name + " is not in your guild!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }
                            byte newrank = Convert.ToByte(args[2]);
                            if (newrank >= obj.GuildRank)
                            {
                                client.Out.SendMessage("You can only promote to a inferior rank.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }
                            obj.GuildRank = Math.Max(newrank, (byte)0);
                            obj.SaveIntoDatabase();
                            obj.Out.SendMessage("You are promoted to " + newrank + " by " + client.Player.Name, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            client.Out.SendMessage("You have promoted " + obj.Name + " to " + newrank, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }
                        break; 
                    #endregion

                    #region /gc demote
                    // --------------------------------------------------------------------------------
                    // DEMOTE
                    // --------------------------------------------------------------------------------
                    case "demote":
                        {
                            if (args.Length < 3)
                            {
                                client.Out.SendMessage("Syntax error! /gc demote <newrank> [name]", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 0;
                            }

                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage("You have to be a member of a guild, before you can use any of the commands!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }
                            if (!client.Player.Guild.CheckGuildPermission(client.Player, eGuildPerm.Promote))
                            {
                                client.Out.SendMessage("You dont have the priviledges for that!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }
                            GamePlayer obj = client.Player.TargetObject as GamePlayer;
                            if (args.Length > 3)
                            {
                                GameClient temp = WorldMgr.GetClientByPlayerName(args[3], true);
                                if (temp != null && GameServer.ServerRules.IsAllowedToGroup(client.Player, temp.Player, true))
                                    obj = temp.Player;
                            }
                            if (obj == null)
                            {
                                client.Out.SendMessage("You must select a player!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }
                            if (obj.Guild != client.Player.Guild)
                            {
                                client.Out.SendMessage(obj.Name + " is not in your guild!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }

                            byte newrank = Convert.ToByte(args[2]);
                            if (newrank <= obj.GuildRank)
                            {
                                client.Out.SendMessage("You can only demote to a superior rank.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }
                            obj.GuildRank = Math.Min(newrank, (byte)9);
                            obj.SaveIntoDatabase();
                            obj.Out.SendMessage("You are demoted to " + newrank + " by " + client.Player.Name, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            client.Out.SendMessage("You have demoted " + obj.Name + " to " + newrank, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }
                        break; 
                    #endregion

                    #region /gc who
                    //// --------------------------------------------------------------------------------
                    //// WHO
                    //// --------------------------------------------------------------------------------
                    case "who":
                        {
                            int ind = 0;
                            if (args.Length == 6 && args[2] == "window" && (client.Player.Guild != null))
                            {
                                int page = 1;
                                int sort = 1;
                                ArrayList onlineMembers = new ArrayList();
                                try
                                {
                                    sort = Convert.ToInt32(args[3]); // Sort(1:unsorted, -1:names, 2:level, -3:class, 4:rank, -5:grp/solo, 6:zone,-7:note)
                                    page = Convert.ToInt32(args[4]);
                                }
                                catch { }
                                foreach (GamePlayer ply in client.Player.Guild.ListOnlineMembers)
                                {
                                    if (ply.Client.IsPlaying && (!ply.IsAnonymous || ply == client.Player))
                                    {
                                        onlineMembers.Add(ply);
                                    }
                                }
                                int maxShowed = onlineMembers.Count % 10;
                                if (onlineMembers.Count > 1 && maxShowed == 0)
                                    maxShowed = 10;
                                page = Math.Max(1, Math.Min(onlineMembers.Count / 10 + 1, page));
                                client.Out.SendMessage(string.Format("TE,{0},{1},{2}", page, onlineMembers.Count, maxShowed), eChatType.CT_SocialInterface, eChatLoc.CL_SystemWindow);
                                for (byte i = 0; i < maxShowed; i++)
                                {
                                    GamePlayer ply = (GamePlayer)onlineMembers[(page - 1) * 10 + i];
                                    client.Out.SendMessage(string.Format("E,{0},{1},{2},{3},{4},{5},{6},\"{7}\",\"{8}\"",
                                    (i + 1), 0, ply.Name, ply.Level, ply.CharacterClass.ID, ply.GuildRank, (ply.PlayerGroup == null ? 1 : 2), (ply.Region.GetZone(ply.Position) == null ? "" : ply.Region.GetZone(ply.Position).Description), ""/*Note*/), eChatType.CT_SocialInterface, eChatLoc.CL_SystemWindow);
                                }
                                return 1;
                            }

                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage("You have to be a member of a guild, before you can use any of the commands!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }
                            if (args.Length == 3)
                            {
                                if (args[2] == "alliance" || args[2] == "a")
                                {
                                    foreach (Guild guild in client.Player.Guild.Alliance.AllianceGuilds)
                                    {
                                        foreach (GamePlayer ply in guild.ListOnlineMembers)
                                        {
                                            if (ply.Client.IsPlaying && !ply.IsAnonymous)
                                            {
                                                ind++;
                                                string zoneName = (ply.Region.GetZone(ply.Position) == null ? "(null)" : ply.Region.GetZone(ply.Position).Description);
                                                string mesg = ind + ") " + ply.Name + " <guild=" + guild.GuildName + "> the Level " + ply.Level + " " + ply.CharacterClass.Name + " in " + zoneName;
                                                client.Out.SendMessage(mesg, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                            }
                                        }
                                    }
                                }
                                else
                                    client.Out.SendMessage("Alliance list usage: /gc who alliance or /gc who a ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }

                            foreach (GamePlayer ply in client.Player.Guild.ListOnlineMembers)
                            {
                                if (ply.Client.IsPlaying && !ply.IsAnonymous)
                                {
                                    ind++;
                                    string zoneName = (ply.Region.GetZone(ply.Position) == null ? "(null)" : ply.Region.GetZone(ply.Position).Description);
                                    string mesg = ind + ") " + ply.Name + " <rank=" + ply.GuildRank + "> the Level " + ply.Level + " " + ply.CharacterClass.Name + " in " + zoneName;
                                    client.Out.SendMessage(mesg, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                }
                            }
                            client.Out.SendMessage("Total online members: " + ind.ToString(), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }
                        break;
                    #endregion

                    #region /gc emblem
                    // --------------------------------------------------------------------------------
                    // EMBLEM
                    // --------------------------------------------------------------------------------
                    case "emblem":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage("You have to be a member of a guild, before you can use any of the commands!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }
                            if (client.Player.GuildRank > 0)
                            {
                                client.Out.SendMessage("You dont have the priviledges for that!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }
                            if (client.Player.Guild.Emblem != 0)
                            {
                                if (client.Player.TargetObject is EmblemNPC == false)
                                {
                                    client.Out.SendMessage("Your guild already has an emblem but you may change it for a hefty fee of 200 gold. You must select the EmblemNPC again for this proceedure to happen.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    return 1;
                                }
                                client.Out.SendCustomDialog("Would you like to re-emblem your guild for 200 gold?", new CustomDialogResponse(EmblemChange));
                                return 1;
                            }
                            else
                            {
                                if (client.Player.TargetObject is EmblemNPC == false)
                                {
                                    client.Out.SendMessage("You must select the EmblemNPC to assign a emblem to your guild.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    return 1;
                                }
                                client.Out.SendEmblemDialogue();
                            }
                        }
                        break; 
                    #endregion

                    #region /gc motd
                    // --------------------------------------------------------------------------------
                    // MOTD
                    // --------------------------------------------------------------------------------
                    case "motd":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage("You have to be a member of a guild, before you can use any of the commands!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }
                            if (args.Length == 2)
                            {
                                client.Out.SendMessage("Guild motd : " + client.Player.Guild.Motd, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                            else if (args.Length == 3)
                            {
                                if (client.Player.Guild.CheckGuildPermission(client.Player, eGuildPerm.Motd))
                                {
                                    client.Player.Guild.Motd = args[2];
                                    GameServer.Database.SaveObject(client.Player.Guild);
                                    client.Out.SendMessage("You have set the motd of your guild.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                }
                                else
                                {
                                    client.Out.SendMessage("You don't have enough privilege to set the motd", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                }
                            }
                            client.Out.SendMessage("usage: /gc motd or /gc motd <message>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }
                        break; 
                    #endregion

                    #region /gc amotd
                    // --------------------------------------------------------------------------------
                    // AMOTD
                    // --------------------------------------------------------------------------------
                    case "amotd":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage("You have to be a member of a guild, before you can use any of the commands!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }
                            if (args.Length == 2)
                            {
                                client.Out.SendMessage("Alliance motd : " + client.Player.Guild.Alliance.AMotd, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                            else if (args.Length == 3)
                            {
                                if (client.Player.Guild.CheckGuildPermission(client.Player, eGuildPerm.Alli))
                                {
                                    client.Player.Guild.Alliance.AMotd = args[2];
                                    GameServer.Database.SaveObject(client.Player.Guild.Alliance);
                                    client.Out.SendMessage("You have set the alliance motd.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                }
                                else
                                {
                                    client.Out.SendMessage("You don't have enough privilege to set the alliance motd.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                }
                            }
                            client.Out.SendMessage("usage: /gc amotd or /gc amotd <message>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }
                        break; 
                    #endregion

                    #region /gc omotd
                    // --------------------------------------------------------------------------------
                    // OMOTD
                    // --------------------------------------------------------------------------------
                    case "omotd":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage("You have to be a member of a guild, before you can use any of the commands!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }
                            if (args.Length == 2)
                            {
                                if (client.Player.Guild.CheckGuildPermission(client.Player, eGuildPerm.OcHear))
                                {
                                    client.Out.SendMessage("Officer motd : " + client.Player.Guild.OMotd, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                }
                                else
                                {
                                    client.Out.SendMessage("You have to be a officer to see the officer motd.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                }
                            }
                            else if (args.Length == 3)
                            {
                                if (client.Player.Guild.CheckGuildPermission(client.Player, eGuildPerm.OcSpeak))
                                {
                                    client.Player.Guild.OMotd = args[2];
                                    GameServer.Database.SaveObject(client.Player.Guild);
                                    client.Out.SendMessage("You have set the officer motd.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                }
                                else
                                {
                                    client.Out.SendMessage("You don't have enough privilege to set the officer motd.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                }
                            }
                            client.Out.SendMessage("usage: /gc omotd or /gc omotd <message>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }
                        break; 
                    #endregion

                    #region /gc ainvite
                    // --------------------------------------------------------------------------------
                    // AINVITE
                    // --------------------------------------------------------------------------------
                    case "ainvite":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage("You have to be a member of a guild, before you can use any of the commands!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }
                            if (!client.Player.Guild.CheckGuildPermission(client.Player, eGuildPerm.Alli))
                            {
                                client.Out.SendMessage("You must be a alliance officer to invite a guild to your alliance", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }

                            GamePlayer obj = client.Player.TargetObject as GamePlayer;
                            if (obj == null || obj.Guild == null || !obj.Guild.CheckGuildPermission(obj, eGuildPerm.Alli))
                            {
                                client.Out.SendMessage("You must select a alliance officer of the guild you want to invite!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }

                            IList allianceGuilds = obj.Guild.Alliance.AllianceGuilds;
                            if (allianceGuilds.Count > 1)
                            {
                                client.Out.SendMessage("The guild is already in another alliance.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }

                            if (allianceGuilds.Contains(client.Player.Guild))
                            {
                                client.Out.SendMessage("The guild you want to invite is already in your alliance.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }

                            obj.TempProperties.setProperty("alliance", client.Player.Guild.Alliance);
                            client.Out.SendMessage("You invite the guild " + obj.Guild.GuildName + " to join your alliance.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            obj.Out.SendMessage("You have been invited to join the " + client.Player.Guild.GuildName + " alliance.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return 1;
                        } 
                    #endregion

                    #region /gc aaccept
                    // --------------------------------------------------------------------------------
                    // AINVITE
                    // --------------------------------------------------------------------------------
                    case "aaccept":
                        {
                            Alliance inviterAlliance = (Alliance)client.Player.TempProperties.getObjectProperty("alliance", null);
                            client.Player.TempProperties.removeProperty("alliance");

                            if (client.Player.Guild == null)
                            {
                                client.Player.Out.SendMessage("You have to be a member of a guild, before you can use any of the commands!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }

                            if (!client.Player.Guild.CheckGuildPermission(client.Player, eGuildPerm.Alli))
                            {
                                client.Out.SendMessage("You must be a alliance officer to accept a alliance invitation.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }

                            if (client.Player.Guild.Alliance.AllianceGuilds.Count > 1)
                            {
                                client.Out.SendMessage("Your guild is already in another alliance.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }

                            if (inviterAlliance.AllianceGuilds.Contains(client.Player.Guild))
                            {
                                client.Out.SendMessage("Your guild is already in this alliance.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }

                            inviterAlliance.AddGuildToAlliance(client.Player.Guild);
                            inviterAlliance.SendMessageToAllianceMembers("The guild " + client.Player.Guild.GuildName + " is now a member of your alliance.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return 1;
                        } 
                    #endregion

                    #region /gc acancel
                    // --------------------------------------------------------------------------------
                    // AINVITE
                    // --------------------------------------------------------------------------------
                    case "acancel":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage("You have to be a member of a guild, before you can use any of the commands!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }

                            if (!client.Player.Guild.CheckGuildPermission(client.Player, eGuildPerm.Alli))
                            {
                                client.Out.SendMessage("You must be a alliance officer to accept a alliance invitation.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }

                            GamePlayer obj = client.Player.TargetObject as GamePlayer;
                            if (obj == null || obj.Guild == null || !obj.Guild.CheckGuildPermission(obj, eGuildPerm.Alli))
                            {
                                client.Out.SendMessage("You must select the alliance officer of the guild you want to cancel your invitation!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }

                            Alliance inviterAlliance = (Alliance)obj.TempProperties.getObjectProperty("alliance", null);
                            if (inviterAlliance != null)
                            {
                                client.Player.TempProperties.removeProperty("alliance");
                                client.Out.SendMessage("Your alliance invitation has been cancelled.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                obj.Out.SendMessage("The alliance invitation has been cancelled.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                            else
                            {
                                client.Out.SendMessage("Your have not send any alliance invitation to this player.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }

                            return 1;
                        } 
                    #endregion

                    #region /gc adecline
                    // --------------------------------------------------------------------------------
                    // ADECLINE
                    // --------------------------------------------------------------------------------
                    case "adecline":
                        {
                            Alliance inviterAlliance = (Alliance)client.Player.TempProperties.getObjectProperty("alliance", null);
                            if (inviterAlliance != null)
                            {
                                client.Player.TempProperties.removeProperty("alliance");
                                client.Out.SendMessage("You have decline the " + inviterAlliance.AllianceLeader.GuildName + " alliance invitation.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                inviterAlliance.SendMessageToAllianceMembers("The guild " + client.Player.Guild.GuildName + " hes declined your alliance invitation.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                            else
                            {
                                client.Out.SendMessage("You have not any alliance invitation to decline.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                            return 1;
                        } 
                    #endregion

                    #region /gc aremove
                    // --------------------------------------------------------------------------------
                    // AREMOVE
                    // --------------------------------------------------------------------------------
                    case "aremove":
                        {
                            if (args.Length < 2)
                            {
                                client.Out.SendMessage("usage: /gc aremove <guildName>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }

                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage("You have to be a member of a guild, before you can use any of the commands!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }

                            if (!client.Player.Guild.CheckGuildPermission(client.Player, eGuildPerm.Alli))
                            {
                                client.Out.SendMessage("You must be a alliance officer to remove a guild from the alliance.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }

                            if (client.Player.Guild == client.Player.Guild.Alliance.AllianceLeader)
                            {
                                client.Out.SendMessage("You can't quit an alliance where you are the leader.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }

                            Guild guildToRemove = GuildMgr.GetGuildByName(args[1]);
                            if (guildToRemove == null)
                            {
                                client.Out.SendMessage("The guild " + args[1] + " does not exist.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }

                            if (!client.Player.Guild.Alliance.AllianceGuilds.Contains(guildToRemove))
                            {
                                client.Out.SendMessage("The guild " + args[1] + " is not in your alliance.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }

                            client.Player.Guild.Alliance.RemoveGuildFromAlliance(guildToRemove);

                            guildToRemove.SendMessageToGuildMembers("Your guild has been removed from " + client.Player.Guild.Alliance.AllianceLeader.GuildName + " alliance.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            client.Player.Guild.Alliance.SendMessageToAllianceMembers("The guild " + guildToRemove.GuildName + " is no longer a part of your alliance.", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                            return 1;
                        } 
                    #endregion

               /*     #region /gc claim
                    // --------------------------------------------------------------------------------
                    //ClAIM
                    // --------------------------------------------------------------------------------
                    case "claim":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage("You have to be a member of a guild, before you can use any of the commands!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }
                            AbstractGameKeep keep = KeepMgr.getKeepCloseToSpot((ushort)client.Player.RegionId, client.Player.Position, WorldMgr.VISIBILITY_DISTANCE);
                            if (keep == null)
                            {
                                client.Out.SendMessage("You have to be near the keep to claim it.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }
                            if (keep.CheckForClaim(client.Player))
                            {
                                keep.Claim(client.Player);
                            }
                            client.Player.Guild.SendMessageToGuildMembers("Your guild have claim " + keep.Name, eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                            return 1;
                        } 
                    #endregion*/

                  /*  #region /gc release
                    // --------------------------------------------------------------------------------
                    //RELEASE
                    // --------------------------------------------------------------------------------
                    case "release":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage("You must have a guild to release a claimed keep.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }
                            if (client.Player.Guild.ClaimedKeep == null)
                            {
                                client.Out.SendMessage("You must have a keep to release.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }
                            if (!client.Player.Guild.CheckGuildPermission(client.Player, eGuildPerm.Release))
                            {
                                client.Out.SendMessage("You have not enough priviledge to do that.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }
                            client.Player.Guild.ClaimedKeep.Release();
                            return 1;
                        } 
                    #endregion*/

                  /*  #region /gc upgrade
                    // --------------------------------------------------------------------------------
                    //UPGRADE
                    // --------------------------------------------------------------------------------
                    case "upgrade":
                        {
                            if (client.Player.Guild == null)
                            {
                                client.Out.SendMessage("You must have a guild to release a claimed keep.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }
                            if (client.Player.Guild.ClaimedKeep == null)
                            {
                                client.Out.SendMessage("You must have a keep to upgrate it.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }
                            if (!client.Player.Guild.CheckGuildPermission(client.Player, eGuildPerm.Upgrade))
                            {
                                client.Out.SendMessage("You do not have permission to upgrade your guild's outpost.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }
                            if (args.Length != 3)
                            {
                                client.Out.SendMessage("You must specify a level to target for upgrade.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 1;
                            }
                            int targetlevel = 0;
                            try
                            {
                                targetlevel = Convert.ToInt32(args[2]);
                                if (targetlevel > 10 || targetlevel < 1)
                                    return 0;
                            }
                            catch
                            {
                                client.Out.SendMessage("the 2e argument must be a number", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return 0;
                            }
                            client.Player.Guild.ClaimedKeep.Upgrade(targetlevel);
                            return 1;
                        }
                    default:
                        {
                            client.Out.SendMessage("Unknown command \"" + args[1] + "\", please type /gc for a command list.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }
						#endregion
                        break;*/
                } //switch
                return 1; 
                   
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("error in /gc script, " + args[1] + " command: " + e.ToString());
                #region Ingame display messages
                if (client.Account.PrivLevel != ePrivLevel.Player)
                {
                    client.Out.SendMessage("Game Master commands:", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    client.Out.SendMessage("'/gc create <Name> <player>' Create a new guild with player as leader", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    client.Out.SendMessage("'/gc addplayer <player> to <guild>' to add player to guild", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    client.Out.SendMessage("'/gc removeplayer <player> from <guild>' to remove player from a guild", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }

                client.Out.SendMessage("Member commands:", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage("'/gc form <Name>' to create a new guilde with all player of group", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage("'/gc info' to show information on the guild", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage("'/gc ranks' to show information about the guild's ranks", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage("'/gc cancel <option> [value]' to cancel all command done before", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage("'/gc decline' to decline the enter in guild ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage("'/gc claim' to claim a keep", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage("'/gc quit' to leave the guild", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage("'/gc motd <text>' to set the message of the day of the guild", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage("'/gc amotd <text>' to set the message of the day of the alliance's guild", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage("'/gc omotd <text>' to set the message of the day of the officier's guild", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage("'/gc promote <rank#> [name]' to promote player to a superior rank", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage("'/gc demote <rank#> [name]' to set the rank from 0 to 10 of the player to an inferior rank", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage("'/gc remove <playername>' to remove the player from the guild", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage("'/gc emblem' to set emblem", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage("'/gc edit' to show list of all option in guild edit", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage("'/gc accept ' to accept invite to guild", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage("'/gc invite [name]' to invite targeted player to join the guild", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage("'/gc who' to show all player in your guild", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage("'/gc list' to show all guild in your realm", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage("'/gc aaccept' to accept an alliance invitation", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage("'/gc acancel' to cancel an alliance invitation", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage("'/gc adecline' to decline an alliance invitation", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage("'/gc ainvite' to invite another guild to join your alliance", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage("'/gc aremove' to removes a guild from your alliance", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return 0; 
                #endregion
			}
		}

        #region change emblem
        public static void EmblemChange(GamePlayer player, byte response)
        {
            if (response != 0x01)
                return;

            if (player.TargetObject is EmblemNPC == false)
            {
                player.Client.Out.SendMessage("You must select the EmblemNPC before accept this dialog.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            if (!player.RemoveMoney(GuildMgr.COST_RE_EMBLEM, null)) //200 gold to re-emblem
            {
                player.Out.SendMessage("You must have 200 gold to re emblem your guild.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }
            player.Out.SendEmblemDialogue();
        } 
        #endregion
	}
}