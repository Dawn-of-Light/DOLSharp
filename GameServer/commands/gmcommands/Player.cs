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
using System.Reflection;
using System.Collections;
using DOL;
using DOL.Events;
using DOL.Database;
using DOL.GS;
using DOL.GS.Spells;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using log4net;
using DOL.GS.Quests;

namespace DOL.GS.Commands
{
    [CmdAttribute(
     "&player",
     ePrivLevel.GM,
     "Various Admin/GM commands to edit characters.",
     "/player name <newName>",
     "/player lastname <change|reset> <newLastName>",
     "/player level <newLevel>",
     "/player realm <newRealm>",
     "/player inventory",
     "/player <rps|bps|xp|clxp> <amount>",
     "/player stat <typeofStat> <value>",
     "/player money <copp|silv|gold|plat|mith> <amount>",
	 "/player respec <all|line|realm|dol|champion> <amount=1>",
     "/player model <reset|[change]> <modelid>",
     "/player friend <list|playerName>",
     "/player <rez|kill> <albs|mids|hibs|self|all>", // if realm not specified, it will rez target.
     "/player jump <group|guild|cg> <name>", // to jump a group to you, just type in a player's name and his or her entire group will come with.
     "/player kick <all>",
     "/player save <all>",
     "/player purge",
     "/player update",
     "/player info",
     "/player showgroup",
     "/player showeffects",
     "/player articredit",
	 "/player allchars [PlayerName]"
   )]

    public class PlayerCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void OnCommand(GameClient client, string[] args)
        {
            if (args.Length > 4 || args.Length == 1)
            {
                DisplaySyntax(client);
                return;
            }

            switch (args[1])
            {
                #region articredit
                case "articredit":
                    {
                        if (args.Length != 3)
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        GamePlayer player = client.Player.TargetObject as GamePlayer;
                        if (player == null)
                            	player = client.Player;

                        ArtifactMgr.GrantArtifactCredit(player, args[2]);
                        break;
                    }
                #endregion
                #region name

				case "name":
                    {
                        GamePlayer player = client.Player.TargetObject as GamePlayer;
                        if (args.Length != 3)
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        if (player == null)
                           	player = client.Player;
                            
                        player.Name = args[2];
                        player.Out.SendMessage(client.Player.Name + "(PrivLevel: " + client.Account.PrivLevel + ") has changed your name to " + player.Name + "!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                        client.Out.SendMessage("You successfully changed this players name to " + player.Name + "!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                        player.SaveIntoDatabase();
                        break;
                    }
				#endregion
				#region lastname
				case "lastname":
                    {
                        GamePlayer player = client.Player.TargetObject as GamePlayer;
                        if (args.Length > 4)
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        if (player == null)
                           	player = client.Player;
                            
                        switch (args[2])
                        {
                            case "change":
                                {
                                    player.LastName = args[3];
                                    player.Out.SendMessage(client.Player.Name + "(PrivLevel: " + client.Account.PrivLevel + ") has changed your lastname to " + player.LastName + "!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                    client.Out.SendMessage("You successfully changed " + player.Name + "'s lastname to " + player.LastName + "!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                    player.SaveIntoDatabase();
                                    break;
                                }

                            case "reset":
                                {
                                    player.LastName = null;
                                    client.Out.SendMessage("You cleared " + player.Name + "'s lastname successfully!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                    player.Out.SendMessage(client.Player.Name + "(PrivLevel: " + client.Account.PrivLevel + ") has cleared your lastname!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                    player.SaveIntoDatabase();
                                    break;
                                }
                        }
                        break;
                    }
                #endregion
                #region level
                case "level":
                    {
                        try
                        {
                            byte newLevel = Convert.ToByte(args[2]);
                            GamePlayer player = client.Player.TargetObject as GamePlayer;

                            if (args.Length != 3)
                            {
                                DisplaySyntax(client);
                                return;
                            }

                            if (player == null)
                            	player = client.Player;
                            
                            if (newLevel <= 0 || newLevel > 255)
                            {
                                client.Out.SendMessage(player.Name + "'s level can only be set to a number 1 to 255!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            int curLevel = player.Level;
                            if (newLevel == curLevel)
                                return;
                            bool curSecondStage = player.IsLevelSecondStage;
                            if (newLevel > curLevel && curSecondStage)
                            {
                                player.GainExperience(player.GetExperienceValueForLevel(++curLevel));
                            }
                            if (newLevel != curLevel || !curSecondStage)
                                player.Level = newLevel;

                            // If new level is more than 40, then we have
                            // to add the skill points from half-levels
                            if (newLevel > 40)
                            {
                                if (curLevel < 40)
                                    curLevel = 40;
                                for (int i = curLevel; i < newLevel; i++)
                                {
                                    // we skip the first add if was in level 2nd stage
                                    if (curSecondStage)
                                        curSecondStage = false;
                                    else
                                        player.SkillSpecialtyPoints += player.CharacterClass.SpecPointsMultiplier * i / 20;
                                }
                            }

                            client.Out.SendMessage("You changed " + player.Name + "'s level successfully to " + newLevel.ToString() + "!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                            player.Out.SendMessage(client.Player.Name + "(PrivLevel: " + client.Account.PrivLevel + ") has changed your level to " + newLevel.ToString() + "!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                            player.Out.SendUpdatePlayer();
                            player.Out.SendUpdatePoints();
                            player.UpdatePlayerStatus();
                            player.SaveIntoDatabase();
                        }

                        catch (Exception)
                        {
                            DisplaySyntax(client);
                            return;
                        }
                    }
                    break;
                #endregion
                #region realm
                case "realm":
                    {
                        try
                        {
                            byte newRealm = Convert.ToByte(args[2]);
                            GamePlayer player = client.Player.TargetObject as GamePlayer;

                            if (args.Length != 3)
                            {
                                DisplaySyntax(client);
                                return;
                            }

                            if (player == null)
                            	player = client.Player;

                            if (newRealm < 0 || newRealm > 3)
                            {
                                client.Out.SendMessage(player.Name + "'s realm can only be set to numbers 0-3!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            player.Realm = (eRealm)newRealm;

                            client.Out.SendMessage("You successfully changed " + player.Name + "'s realm to " + GlobalConstants.RealmToName((eRealm)newRealm) + "!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                            player.Out.SendMessage(client.Player.Name + " has changed your realm to " + GlobalConstants.RealmToName((eRealm)newRealm) + "!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);

                            player.Out.SendUpdatePlayer();
                            player.SaveIntoDatabase();
                        }

                        catch (Exception)
                        {
                            DisplaySyntax(client);
                            return;
                        }
                    }
                    break;
                #endregion
                #region model
                case "model":
                    {
                        GamePlayer player = client.Player.TargetObject as GamePlayer;

                        try
                        {
                            if (args.Length > 4)
                            {
                                DisplaySyntax(client);
                                return;
                            }

                            if (player == null)
                            	player = client.Player;


                            switch (args[2])
                            {
                                case "reset":
                                    {
                                        player.Model = (ushort)player.Client.Account.Characters[player.Client.ActiveCharIndex].CreationModel;
                                        client.Out.SendMessage("You changed " + player.Name + " back to his or her original model successfully!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                        player.Out.SendMessage(client.Player.Name + "(PrivLevel: " + client.Account.PrivLevel + ") has changed your model back to its original creation model!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                        player.Out.SendUpdatePlayer();
                                        player.SaveIntoDatabase();
                                    }
                                    break;

                                default:
                                    {
										ushort modelID = 0;
										int modelIndex = 0;

										if ( args[2] == "change" )
											modelIndex = 3;
										else
											modelIndex = 2;

										if ( ushort.TryParse( args[modelIndex], out modelID ) == false )
										{
											DisplaySyntax( client, args[1] );
											return;
										}

                                        player.Model = modelID;
                                        client.Out.SendMessage("You successfully changed " + player.Name + "'s form! (ID:#" + modelID + ")", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                        player.Out.SendMessage(client.Player.Name + "(PrivLevel: " + client.Account.PrivLevel + ") has changed your form! (ID:#" + modelID + ")", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                        player.Out.SendUpdatePlayer();
                                        player.SaveIntoDatabase();
                                    }
                                    break;
                            }
                        }
                        catch (Exception)
                        {
                            DisplaySyntax(client);
                            return;
                        }
                    }
                    break;
                #endregion
                #region money
                case "money":
                    {
                        GamePlayer player = client.Player.TargetObject as GamePlayer;

                        try
                        {
                            if (args.Length != 4)
                            {
                                DisplaySyntax(client);
                                return;
                            }

                            if (player == null)
                            	player = client.Player;

                            switch (args[2])
                            {
                                case "copp":
                                    {
                                        long amount = long.Parse(args[3]);
                                        player.AddMoney(amount);
                                        client.Out.SendMessage("You gave " + player.Name + " copper successfully!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                        player.Out.SendMessage(client.Player.Name + "(PrivLevel: " + client.Account.PrivLevel + ") has given you some copper!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                        return;
                                    }





                                case "silv":
                                    {
                                        long amount = long.Parse(args[3]) * 100;
                                        player.AddMoney(amount);
                                        client.Out.SendMessage("You gave " + player.Name + " silver successfully!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                        player.Out.SendMessage(client.Player.Name + "(PrivLevel: " + client.Account.PrivLevel + ") has given you some silver!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                        return;
                                    }

                                case "gold":
                                    {
                                        long amount = long.Parse(args[3]) * 100 * 100;
                                        player.AddMoney(amount);
                                        client.Out.SendMessage("You gave " + player.Name + " gold successfully!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                        player.Out.SendMessage(client.Player.Name + "(PrivLevel: " + client.Account.PrivLevel + ") has given you some gold!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                        return;
                                    }

                                case "plat":
                                    {
                                        long amount = long.Parse(args[3]) * 100 * 100 * 1000;
                                        player.AddMoney(amount);
                                        client.Out.SendMessage("You gave " + player.Name + " platinum successfully!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                        player.Out.SendMessage(client.Player.Name + "(PrivLevel: " + client.Account.PrivLevel + ") has given you some platinum!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                        return;
                                    }

                                case "mith":
                                    {
                                        long amount = long.Parse(args[3]) * 100 * 100 * 1000 * 1000;
                                        player.AddMoney(amount);
                                        client.Out.SendMessage("You gave " + player.Name + " mithril successfully!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                        player.Out.SendMessage(client.Player.Name + "(PrivLevel: " + client.Account.PrivLevel + ") has given you some mithril!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                        return;
                                    }
                            }
                            player.Out.SendUpdatePlayer();
                            player.SaveIntoDatabase();

                        }

                        catch (Exception)
                        {
                            DisplaySyntax(client);
                            return;
                        }
                    }
                    break;
                #endregion
                #region points
                case "rps":
                    {

                        GamePlayer player = client.Player.TargetObject as GamePlayer;
                        try
                        {
                            if (args.Length != 3)
                            {
                                DisplaySyntax(client);
                                return;
                            }

                            if (player == null)
                            	player = client.Player;
                            
                            long amount = long.Parse(args[2]);
                            player.GainRealmPoints(amount, false);
                            client.Out.SendMessage("You gave " + player.Name + " " + amount + " realmpoints succesfully!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                            player.Out.SendMessage(client.Player.Name + "(PrivLevel: " + client.Account.PrivLevel + ") has given you " + amount + " realmpoints!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                            player.SaveIntoDatabase();
                            player.Out.SendUpdatePlayer();
                        }
                        catch (Exception)
                        {
                            DisplaySyntax(client);
                            return;
                        }

                    }
                    break;


                case "xp":
                    {
                        GamePlayer player = client.Player.TargetObject as GamePlayer;
                        try
                        {
                            if (args.Length != 3)
                            {
                                DisplaySyntax(client);
                                return;
                            }

                            if (player == null)
                            	player = client.Player;
                            
                            long amount = long.Parse(args[2]);
                            player.GainExperience(amount, false);
                            client.Out.SendMessage("You gave " + player.Name + " " + amount + " experience succesfully!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                            player.Out.SendMessage(client.Player.Name + "(PrivLevel: " + client.Account.PrivLevel + ") has given you " + amount + " experience!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                            player.SaveIntoDatabase();
                            player.Out.SendUpdatePlayer();
                        }
                        catch (Exception)
                        {
                            DisplaySyntax(client);
                            return;
                        }
                    }
                    break;

                case "clxp":
                    {
                        GamePlayer player = client.Player.TargetObject as GamePlayer;
                        try
                        {
                            if (args.Length != 3)
                            {
                                DisplaySyntax(client);
                                return;
                            }

                            if (player == null)
                            	player = client.Player;
                            
                            long amount = long.Parse(args[2]);
                            player.GainChampionExperience(amount);
                            client.Out.SendMessage("You gave " + player.Name + " " + amount + " Champion experience succesfully!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                            player.Out.SendMessage(client.Player.Name + "(PrivLevel: " + client.Account.PrivLevel + ") has given you " + amount + " Champion experience!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                            player.SaveIntoDatabase();
                            player.Out.SendUpdatePlayer();
                        }
                        catch (Exception)
                        {
                            DisplaySyntax(client);
                            return;
                        }
                    }
                    break;

                case "bps":
                    {
                        GamePlayer player = client.Player.TargetObject as GamePlayer;
                        try
                        {
                            if (args.Length != 3)
                            {
                                DisplaySyntax(client);
                                return;
                            }

                            if (player == null)
                            	player = client.Player;
                            
                            long amount = long.Parse(args[2]);
							player.GainBountyPoints(amount, false);
                            client.Out.SendMessage("You gave " + player.Name + " " + amount + " bountypoints succesfully!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                            player.Out.SendMessage(client.Player.Name + "(PrivLevel: " + client.Account.PrivLevel + ") has given you " + amount + " bountypoints!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                            player.SaveIntoDatabase();
                            player.Out.SendUpdatePlayer();
                        }
                        catch (Exception)
                        {
                            DisplaySyntax(client);
                            return;
                        }
                    }
                    break;
                #endregion
                #region stat
                case "stat":
                    {
                        GamePlayer player = client.Player.TargetObject as GamePlayer;

                        try
                        {
                            short value = Convert.ToInt16(args[3]);

                            if (args.Length != 4)
                            {
                                DisplaySyntax(client);
                                return;
                            }

                            if (player == null)
                            	player = client.Player;

                            switch (args[2])
                            {
                                /*1*/
                                case "dex":
                                    {
                                        player.ChangeBaseStat(DOL.GS.eStat.DEX, value);
                                        player.Out.SendMessage(client.Player.Name + "(PrivLevel: " + client.Account.PrivLevel + ") has given you " + value + " dexterity!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                        client.Out.SendMessage("You gave " + player.Name + " " + value + " dexterity successfully!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                    }
                                    break;

                                /*2*/
                                case "str":
                                    {
                                        player.ChangeBaseStat(DOL.GS.eStat.STR, value);
                                        player.Out.SendMessage(client.Player.Name + "(PrivLevel: " + client.Account.PrivLevel + ") has given you " + value + " strength!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                        client.Out.SendMessage("You gave " + player.Name + " " + value + " strength successfully!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                    }
                                    break;

                                /*3*/
                                case "con":
                                    {
                                        player.ChangeBaseStat(DOL.GS.eStat.CON, value);
                                        player.Out.SendMessage(client.Player.Name + "(PrivLevel: " + client.Account.PrivLevel + ") has given you " + value + " consititution!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                        client.Out.SendMessage("You gave " + player.Name + " " + value + " constitution successfully!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                    }
                                    break;

                                /*4*/
                                case "emp":
                                    {
                                        player.ChangeBaseStat(DOL.GS.eStat.EMP, value);
                                        player.Out.SendMessage(client.Player.Name + "(PrivLevel: " + client.Account.PrivLevel + ") has given you " + value + " empathy!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                        client.Out.SendMessage("You gave " + player.Name + " " + value + " empathy successfully!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                    }
                                    break;

                                /*5*/
                                case "int":
                                    {
                                        player.ChangeBaseStat(DOL.GS.eStat.INT, value);
                                        player.Out.SendMessage(client.Player.Name + "(PrivLevel: " + client.Account.PrivLevel + ") has given you " + value + " intelligence!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                        client.Out.SendMessage("You gave " + player.Name + " " + value + " intelligence successfully!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                    }
                                    break;

                                /*6*/
                                case "pie":
                                    {
                                        player.ChangeBaseStat(DOL.GS.eStat.PIE, value);
                                        player.Out.SendMessage(client.Player.Name + "(PrivLevel: " + client.Account.PrivLevel + ") has given you " + value + " piety!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                        client.Out.SendMessage("You gave " + player.Name + " " + value + " piety successfully!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                    }
                                    break;

                                /*7*/
                                case "qui":
                                    {
                                        player.ChangeBaseStat(DOL.GS.eStat.QUI, value);
                                        player.Out.SendMessage(client.Player.Name + "(PrivLevel: " + client.Account.PrivLevel + ") has given you " + value + " quickness!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                        client.Out.SendMessage("You gave " + player.Name + " " + value + " quickness successfully!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                    }
                                    break;

                                /*8*/
                                case "cha":
                                    {
                                        player.ChangeBaseStat(DOL.GS.eStat.CHR, value);
                                        player.Out.SendMessage(client.Player.Name + "(PrivLevel: " + client.Account.PrivLevel + ") has given you " + value + " charisma!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                        client.Out.SendMessage("You gave " + player.Name + " " + value + " charisma successfully!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                    }
                                    break;

                                /*all*/
                                case "all":
                                    {
                                        player.ChangeBaseStat(DOL.GS.eStat.CHR, value); //1
                                        player.ChangeBaseStat(DOL.GS.eStat.QUI, value); //2
                                        player.ChangeBaseStat(DOL.GS.eStat.INT, value); //3
                                        player.ChangeBaseStat(DOL.GS.eStat.PIE, value); //4
                                        player.ChangeBaseStat(DOL.GS.eStat.EMP, value); //5
                                        player.ChangeBaseStat(DOL.GS.eStat.CON, value); //6
                                        player.ChangeBaseStat(DOL.GS.eStat.STR, value); //7
                                        player.ChangeBaseStat(DOL.GS.eStat.DEX, value); //8
                                        player.Out.SendMessage(client.Player.Name + "(PrivLevel: " + client.Account.PrivLevel + ") has given you " + value + " to all stats!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                        client.Out.SendMessage("You gave " + player.Name + " " + value + " to all stats successfully!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                    }
                                    break;

                                default:
                                    {
                                        client.Out.SendMessage("Try using: dex, str, con, emp, int, pie, qui, cha, or all as a type of stat.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                    break;
                            }

                            player.Out.SendCharStatsUpdate();
                            player.Out.SendUpdatePlayer();
                            player.SaveIntoDatabase();

                        }
                        catch (Exception)
                        {
                            DisplaySyntax(client);
                            return;
                        }
                    }
                    break;
                #endregion
                #region friend
                case "friend":
                    {
                        GamePlayer player = client.Player.TargetObject as GamePlayer;

                        if (args.Length != 3)
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        if (player == null)
                        {
                            client.Out.SendMessage("You need a valid target!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return;
                        }

                        if (args[2] == "list")
                        {
                            string[] list = player.PlayerCharacter.SerializedFriendsList.Split(',');
                            client.Out.SendCustomTextWindow(player.Name + "'s Friend List", list);
                            return;
                        }

                        string name = string.Join(" ", args, 2, args.Length - 2);

                        int result = 0;
                        GameClient fclient = WorldMgr.GuessClientByPlayerNameAndRealm(name, 0, true, out result);
                        if (fclient != null && !GameServer.ServerRules.IsSameRealm(fclient.Player, player.Client.Player, true))
                        {
                            fclient = null;
                        }

                        if (fclient == null)
                        {
                            name = args[2];
                            if (player.Client.Player.Friends.Contains(name))
                            {
                                player.Out.SendMessage(client.Player.Name + "(PrivLevel: " + client.Account.PrivLevel + ") has removed " + player.Name + " from your friend list!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                client.Out.SendMessage("Removed " + name + " from " + player.Name + "'s friend list successfully!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                player.Client.Player.ModifyFriend(name, true);
                                player.Out.SendRemoveFriends(new string[] { name });
                                return;
                            }
                            else
                            {
                                // nothing found
                                client.Out.SendMessage("No players online with name " + name + ".", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                return;
                            }
                        }

                        switch (result)
                        {
                            case 2: // name not unique
                                client.Out.SendMessage("Character name is not unique.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            case 3: // exact match
                            case 4: // guessed name
                                if (fclient == player.Client)
                                {
                                    client.Out.SendMessage("You can't add that player to his or her own friend list!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                    return;
                                }

                                name = fclient.Player.Name;
                                if (player.Client.Player.Friends.Contains(name))
                                {
                                    player.Out.SendMessage(client.Player.Name + "(PrivLevel: " + client.Account.PrivLevel + ") has removed " + name + " from your friend list!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                    client.Out.SendMessage("Removed " + name + " from " + player.Name + "'s friend list successfully!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                    player.Client.Player.ModifyFriend(name, true);
                                    player.Out.SendRemoveFriends(new string[] { name });
                                }
                                else
                                {
                                    player.Out.SendMessage(client.Player.Name + "(PrivLevel: " + client.Account.PrivLevel + ") has added " + name + " to your friend list!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                    client.Out.SendMessage("Added " + name + " to " + player.Name + "'s friend list successfully!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                    player.Client.Player.ModifyFriend(name, false);
                                    player.Client.Out.SendAddFriends(new string[] { name });
                                }
                                return;
                        }
                        player.Out.SendUpdatePlayer();
                        player.SaveIntoDatabase();
                    }
                    break;
                #endregion
                #region respec
                case "respec":
                    {
                        GamePlayer player = client.Player.TargetObject as GamePlayer;

                        if (args.Length > 4)
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        if (player == null)
                           	player = client.Player;

						int amount = 1;
						if (args.Length == 4)
						{
							try
							{
								amount = Convert.ToInt32(args[3]);
							}
							catch
							{
								amount = 1;
							}
						}

                        switch (args[2])
                        {
                            case "line":
                                {
									player.RespecAmountSingleSkill += amount;
                                    player.Client.Out.SendMessage(client.Player.Name + "(PrivLevel: " + client.Account.PrivLevel + ") has awarded you " + amount + " single respec!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                    client.Out.SendMessage(amount + " single respec given successfully to " + player.Name + "!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                    break;
                                }
                            case "all":
                                {
									player.RespecAmountAllSkill += amount;
                                    player.Client.Out.SendMessage(client.Player.Name + "(PrivLevel: " + client.Account.PrivLevel + ") has awarded you " + amount + " full respec!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
									client.Out.SendMessage(amount + " full respec given successfully to " + player.Name + "!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                    break;
                                }
                            case "realm":
                                {
									player.RespecAmountRealmSkill += amount;
                                    player.Client.Out.SendMessage(client.Player.Name + "(PrivLevel: " + client.Account.PrivLevel + ") has awarded you " + amount + " realm respec!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
									client.Out.SendMessage(amount + " realm respec given successfully to " + player.Name + "!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                    break;
                                }
                            case "dol":
                                {
									player.RespecAmountDOL += amount;
                                    player.Client.Out.SendMessage(client.Player.Name + "(PrivLevel: " + client.Account.PrivLevel + ") has awarded you " + amount + " DOL (full) respec!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                    client.Out.SendMessage(amount + " DOL (full) respec given successfully to " + player.Name + "!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                    break;
                                }
                            case "champion":
                                {
									player.RespecAmountChampionSkill += amount;
                                    player.Client.Out.SendMessage(client.Player.Name + "(PrivLevel: " + client.Account.PrivLevel + ") has awarded you " + amount + " Champion respec!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                    client.Out.SendMessage(amount + " champion respec given successfully to " + player.Name + "!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                    break;
                                }
                            /*case "ml":
                                {
                                    //
                                    player.Client.Out.SendMessage(client.Player.Name + "(PrivLevel: " + client.Account.PrivLevel + ") has awarded you an ML respec!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                    client.Out.SendMessage("ML respec given successfully to " + player.Name + "!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                    break;
                                }*/
                        }

                        player.Client.Player.SaveIntoDatabase();
                    }
                    break;
                #endregion
                #region realm
                case "purge":
                    {
                        GamePlayer player = client.Player.TargetObject as GamePlayer;
                        bool m_hasEffect;

                        if (args.Length != 2)
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        if (player == null)
                        {
                            client.Out.SendMessage("You need a valid target!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return;
                        }

                        m_hasEffect = false;

                        lock (player.EffectList)
                        {
                            foreach (GameSpellEffect effect in player.EffectList)
                            {
                                if (!effect.SpellHandler.HasPositiveEffect)
                                {
                                    m_hasEffect = true;
                                    break;
                                }
                            }
                        }

                        if (!m_hasEffect)
                        {
                            SendResistEffect(player);
                            return;
                        }

                        lock (player.EffectList)
                        {
                            foreach (GameSpellEffect effect in player.EffectList)
                            {
                                if (!effect.SpellHandler.HasPositiveEffect)
                                {
                                    effect.Cancel(false);
                                }
                            }
                        }
                    }
                    break;
                #endregion
                #region save
                case "save":
                    {
                        GamePlayer player = client.Player.TargetObject as GamePlayer;

                        if (args.Length > 3 || args.Length < 2)
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        if (player == null && args.Length == 2)
                        {
                            client.Out.SendMessage("You need a valid target!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return;
                        }

                        if (args.Length == 2 && player != null)
                        {
                            player.Out.SendMessage(client.Player.Name + "(PrivLevel: " + client.Account.PrivLevel + ") has saved your character.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                            client.Out.SendMessage(player.Name + " saved successfully!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                            player.SaveIntoDatabase();
                        }

                        if (args.Length == 3)
                        {
                            switch (args[2])
                            {
                                case "all":
                                    {
                                        foreach (GameClient c in WorldMgr.GetAllPlayingClients())
                                        {
                                            if(c!= null && c.Player!=null) c.Player.SaveIntoDatabase();
                                        }
                                        client.Out.SendMessage("Saved all characters!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                    }
                                    break;

                                default:
                                    {
                                        DisplaySyntax(client);
                                        return;
                                    }
                            }

                        }
                    }
                    break;
                #endregion
                #region kick
                case "kick":
                    {
                        GamePlayer player = client.Player.TargetObject as GamePlayer;

                        if (args.Length > 3 || args.Length < 2)
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        if (player == null && args.Length == 2)
                        {
                            client.Out.SendMessage("You need a valid target!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return;
                        }

                        if (args.Length == 2 && player != null)
                        {
                            if (player.Client.Account.PrivLevel > 1)
                            {
                                client.Out.SendMessage("Please use /kick <name> to kick Gamemasters. This is used to prevent accidental kicks.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            player.Client.Out.SendPlayerQuit(true);
                            player.Client.Player.SaveIntoDatabase();
                            player.Client.Player.Quit(true);
                            return;
                        }

                        if (args.Length == 3)
                        {
                            switch (args[2])
                            {

                                case "all":
                                    {

                                        foreach (GameClient allplayer in WorldMgr.GetAllPlayingClients())
                                        {
                                            if (allplayer.Account.PrivLevel == 1)
                                            {
                                                allplayer.Out.SendMessage(client.Player.Name + "(PrivLevel: " + client.Account.PrivLevel + ") has kicked all players!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                                allplayer.Out.SendPlayerQuit(true);
                                                allplayer.Player.SaveIntoDatabase();
                                                allplayer.Player.Quit(true);
                                                return;
                                            }

                                        }
                                    }
                                    break;

                                default:
                                    {
                                        DisplaySyntax(client);
                                        return;
                                    }
                            }
                        }
                    }
                    break;
                #endregion
                #region rez kill
                case "rez":
                    {
                        GamePlayer player = client.Player.TargetObject as GamePlayer;

                        if (args.Length > 3 || args.Length < 2)
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        if (player == null && args.Length == 2)
                        {
                            client.Out.SendMessage("You need a valid target!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return;
                        }

                        if (args.Length == 2 && player != null)
                        {

                            if (!(player.IsAlive))
                            {

                                player.Health = player.MaxHealth;
                                player.Mana = player.MaxMana;
                                player.Endurance = player.MaxEndurance;
                                player.MoveTo(client.Player.CurrentRegionID, client.Player.X, client.Player.Y, client.Player.Z, client.Player.Heading);

                                client.Out.SendMessage("You resurrected " + player.Name + " successfully!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                //player.Out.SendMessage(client.Player.Name +" has resurrected you!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);

                                player.StopReleaseTimer();
                                player.Out.SendPlayerRevive(player);
                                player.Out.SendStatusUpdate();
                                player.Out.SendMessage("You have been resurrected by " + client.Player.GetName(0, false) + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                player.Notify(GamePlayerEvent.Revive, player);

                            }
                            else
                            {
                                client.Out.SendMessage("Player is not dead!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                return;
                            }
                        }

                        if (args.Length >= 3)
                        {

                            switch (args[2])
                            {
                                case "albs":
                                    {
                                        foreach (GameClient aplayer in WorldMgr.GetClientsOfRealm(eRealm.Albion))
                                        {
                                            if (!(aplayer.Player.IsAlive))
                                            {

                                                aplayer.Player.Health = aplayer.Player.MaxHealth;
                                                aplayer.Player.Mana = aplayer.Player.MaxMana;
                                                aplayer.Player.Endurance = aplayer.Player.MaxEndurance;
                                                aplayer.Player.MoveTo(client.Player.CurrentRegionID, client.Player.X, client.Player.Y, client.Player.Z, client.Player.Heading);

                                                aplayer.Player.StopReleaseTimer();
                                                aplayer.Player.Out.SendPlayerRevive(aplayer.Player);
                                                aplayer.Player.Out.SendStatusUpdate();
                                                aplayer.Player.Out.SendMessage("You have been resurrected by " + client.Player.GetName(0, false) + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                                aplayer.Player.Notify(GamePlayerEvent.Revive, aplayer.Player);

                                            }
                                        }
                                    }
                                    break;

                                case "hibs":
                                    {
                                        foreach (GameClient hplayer in WorldMgr.GetClientsOfRealm(eRealm.Hibernia))
                                        {
                                            if (!(hplayer.Player.IsAlive))
                                            {

                                                hplayer.Player.Health = hplayer.Player.MaxHealth;
                                                hplayer.Player.Mana = hplayer.Player.MaxMana;
                                                hplayer.Player.Endurance = hplayer.Player.MaxEndurance;
                                                hplayer.Player.MoveTo(client.Player.CurrentRegionID, client.Player.X, client.Player.Y, client.Player.Z, client.Player.Heading);

                                                hplayer.Player.StopReleaseTimer();
                                                hplayer.Player.Out.SendPlayerRevive(hplayer.Player);
                                                hplayer.Player.Out.SendStatusUpdate();
                                                hplayer.Player.Out.SendMessage("You have been resurrected by " + client.Player.GetName(0, false) + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                                hplayer.Player.Notify(GamePlayerEvent.Revive, hplayer.Player);

                                            }
                                        }
                                    }
                                    break;

                                case "mids":
                                    {
                                        foreach (GameClient mplayer in WorldMgr.GetClientsOfRealm(eRealm.Midgard))
                                        {
                                            if (!(mplayer.Player.IsAlive))
                                            {

                                                mplayer.Player.Health = mplayer.Player.MaxHealth;
                                                mplayer.Player.Mana = mplayer.Player.MaxMana;
                                                mplayer.Player.Endurance = mplayer.Player.MaxEndurance;
                                                mplayer.Player.MoveTo(client.Player.CurrentRegionID, client.Player.X, client.Player.Y, client.Player.Z, client.Player.Heading);

                                                mplayer.Player.StopReleaseTimer();
                                                mplayer.Player.Out.SendPlayerRevive(mplayer.Player);
                                                mplayer.Player.Out.SendStatusUpdate();
                                                mplayer.Player.Out.SendMessage("You have been resurrected by " + client.Player.GetName(0, false) + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                                mplayer.Player.Notify(GamePlayerEvent.Revive, mplayer.Player);

                                            }
                                        }
                                    }
                                    break;

                                case "self":
                                    {

                                        GamePlayer selfplayer = client.Player as GamePlayer;

                                        if (!(selfplayer.IsAlive))
                                        {

                                            selfplayer.Health = selfplayer.MaxHealth;
                                            selfplayer.Mana = selfplayer.MaxMana;
                                            selfplayer.Endurance = selfplayer.MaxEndurance;
                                            selfplayer.MoveTo(client.Player.CurrentRegionID, client.Player.X, client.Player.Y, client.Player.Z, client.Player.Heading);

                                            selfplayer.Out.SendMessage("You revive yourself.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);

                                            selfplayer.StopReleaseTimer();
                                            selfplayer.Out.SendPlayerRevive(selfplayer);
                                            selfplayer.Out.SendStatusUpdate();
                                            selfplayer.Notify(GamePlayerEvent.Revive, selfplayer);

                                        }
                                        else
                                        {
                                            client.Out.SendMessage("You are not dead!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                            return;
                                        }
                                    }
                                    break;

                                case "all":
                                    {

                                        foreach (GameClient allplayer in WorldMgr.GetAllPlayingClients())
                                        {
                                            if (!(allplayer.Player.IsAlive))
                                            {

                                                allplayer.Player.Health = allplayer.Player.MaxHealth;
                                                allplayer.Player.Mana = allplayer.Player.MaxMana;
                                                allplayer.Player.Endurance = allplayer.Player.MaxEndurance;
                                                allplayer.Player.MoveTo(client.Player.CurrentRegionID, client.Player.X, client.Player.Y, client.Player.Z, client.Player.Heading);

                                                allplayer.Player.StopReleaseTimer();
                                                allplayer.Player.Out.SendPlayerRevive(allplayer.Player);
                                                allplayer.Player.Out.SendStatusUpdate();
                                                allplayer.Player.Out.SendMessage("You have been resurrected by " + client.Player.GetName(0, false) + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                                allplayer.Player.Notify(GamePlayerEvent.Revive, allplayer.Player);

                                            }
                                        }
                                    }
                                    break;

                                default:
                                    {
                                        client.Out.SendMessage("SYNTAX: /player rez <albs|mids|hibs|all>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                    break;
                            }
                        }
                    }
                    break;

                case "kill":
                    {
                        GamePlayer player = client.Player.TargetObject as GamePlayer;

                        if (args.Length < 2 || args.Length > 3)
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        if (player == null && args.Length == 2)
                        {
                            client.Out.SendMessage("You need a valid target!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return;
                        }

                        if (args.Length == 2 && player != null)
                        {
                            if (player.Client.Account.PrivLevel > 1)
                            {
                                client.Out.SendMessage("This command can not be used on Gamemasters!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                return;
                            }

                            if (player.IsAlive)
                            {
                                KillPlayer(client.Player, player);
                                client.Out.SendMessage("You killed " + player.Name + " successfully!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                player.Out.SendMessage(client.Player.Name + " has killed you!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);

                            }
                            else
                            {
                                client.Out.SendMessage("Player is not alive!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                return;
                            }
                        }
                        if (args.Length < 3)
                            return;
                        switch (args[2])
                        {
                            case "albs":
                                {
                                    foreach (GameClient aplayer in WorldMgr.GetClientsOfRealm(eRealm.Albion))
                                    {
                                        if (aplayer.Player.IsAlive && aplayer.Account.PrivLevel == 1)
                                        {
                                            KillPlayer(client.Player, aplayer.Player);
                                        }
                                    }
                                }
                                break;

                            case "mids":
                                {
                                    foreach (GameClient mplayer in WorldMgr.GetClientsOfRealm(eRealm.Midgard))
                                    {
                                        if (mplayer.Player.IsAlive && mplayer.Account.PrivLevel == 1)
                                        {
                                            KillPlayer(client.Player, mplayer.Player);
                                        }
                                    }
                                }
                                break;
                            case "hibs":
                                {
                                    foreach (GameClient hplayer in WorldMgr.GetClientsOfRealm(eRealm.Hibernia))
                                    {
                                        if (hplayer.Player.IsAlive && hplayer.Account.PrivLevel == 1)
                                        {
                                            KillPlayer(client.Player, hplayer.Player);
                                        }
                                    }
                                }
                                break;

                            case "self":
                                {
                                    GamePlayer selfplayer = client.Player as GamePlayer;

                                    if (!(selfplayer.IsAlive))
                                    {
                                        client.Out.SendMessage("You are already dead. Use /player rez <self> to resurrect yourself.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                        return;
                                    }
                                    else
                                    {
                                        KillPlayer(client.Player, client.Player);
                                        client.Out.SendMessage("Good bye cruel world!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                    }
                                }
                                break;

                            case "all":
                                {
                                    foreach (GameClient allplayer in WorldMgr.GetAllPlayingClients())
                                    {
                                        if (allplayer.Player.IsAlive && allplayer.Account.PrivLevel == 1)
                                        {
                                            KillPlayer(client.Player, allplayer.Player);
                                        }
                                    }
                                }
                                break;

                            default:
                                {
                                    client.Out.SendMessage("'" + args[2] + "' is not a valid arguement.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                }
                                break;
                        }
                        /*End Switch Statement*/
                    }
                    break;
                #endregion
                #region jump
                case "jump":
                    {
                        if (args.Length < 4)
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        switch (args[2])
                        {
                            case "guild":
                                {
                                    short count = 0;

                                    foreach (GameClient pname in WorldMgr.GetAllPlayingClients())
                                    {
                                        string guild = string.Join(" ", args, 3, args.Length - 3);

                                        if (args[3] == null)
                                        {
                                            DisplaySyntax(client);
                                            return;
                                        }

                                        if (pname.Player.GuildName == guild && guild != "")
                                        {
                                            count++;
                                            pname.Player.MoveTo(client.Player.CurrentRegionID, client.Player.X, client.Player.Y, client.Player.Z, client.Player.Heading);
                                        }
                                    }

                                    client.Out.SendMessage(count + " players jumped!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                }
                                break;

                            case "group":
                                {
                                    short count = 0;

                                    foreach (GameClient pname in WorldMgr.GetAllPlayingClients())
                                    {
                                        string name = args[3];

                                        if (name == null)
                                        {
                                            DisplaySyntax(client);
                                            return;
                                        }

                                        if (name == pname.Player.Name)
                                        {
                                            foreach (GameLiving groupedplayers in pname.Player.Group.GetMembersInTheGroup())
                                            {
                                                groupedplayers.MoveTo(client.Player.CurrentRegionID, client.Player.X, client.Player.Y, client.Player.Z, client.Player.Heading);
                                                count++;
                                            }
                                        }
                                    }

                                    client.Out.SendMessage(count + " players jumped!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                }
                                break;

                            case "cg":
                                {
                                    short count = 0;

                                    foreach (GameClient pname in WorldMgr.GetAllPlayingClients())
                                    {
                                        string name = args[3];

                                        if (name == null)
                                        {
                                            DisplaySyntax(client);
                                            return;
                                        }
                                        ChatGroup cg = (ChatGroup)pname.Player.TempProperties.getObjectProperty(ChatGroup.CHATGROUP_PROPERTY, null);

                                        if (name == pname.Player.Name)
                                        {
                                            foreach (GamePlayer cgplayers in cg.Members.Keys)
                                            {
                                                cgplayers.MoveTo(client.Player.CurrentRegionID, client.Player.X, client.Player.Y, client.Player.Z, client.Player.Heading);
                                                count++;
                                            }
                                        }
                                    }

                                    client.Out.SendMessage(count + " players jumped!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                }
                                break;


                            case "bg":
                                {
                                    short count = 0;

                                    foreach (GameClient pname in WorldMgr.GetAllPlayingClients())
                                    {
                                        string name = args[3];

                                        if (name == null)
                                        {
                                            DisplaySyntax(client);
                                            return;
                                        }
                                        BattleGroup cg = (BattleGroup)pname.Player.TempProperties.getObjectProperty(BattleGroup.BATTLEGROUP_PROPERTY, null);

                                        if (name == pname.Player.Name)
                                        {
                                            foreach (GamePlayer cgplayers in cg.Members.Keys)
                                            {
                                                cgplayers.MoveTo(client.Player.CurrentRegionID, client.Player.X, client.Player.Y, client.Player.Z, client.Player.Heading);
                                                count++;
                                            }
                                        }
                                    }

                                    client.Out.SendMessage(count + " players jumped!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                }
                                break;
                            default:
                                {
                                    client.Out.SendMessage("'" + args[2] + "' is not a valid arguement.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                                }
                                break;
                        }
                    }
                    break;
                #endregion
                #region update
                case "update":
                    {
                        GamePlayer player = client.Player.TargetObject as GamePlayer;

                        if (args.Length != 2)
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        if (player == null)
                            player = client.Player;
                        
                        player.Out.SendUpdatePlayer();
                        player.Out.SendCharStatsUpdate();
                        player.Out.SendUpdatePoints();
                        player.Out.SendUpdateMaxSpeed();
                        player.Out.SendStatusUpdate();
                        player.Out.SendCharResistsUpdate();
                        client.Out.SendMessage(player.Name + " updated successfully!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                    }
                    break;
                #endregion
                #region info
                case "info":
                    {
                        GamePlayer player = client.Player.TargetObject as GamePlayer;

                        if (args.Length != 2)
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        if (player == null)
                            player = client.Player;
                        
                        Show_Info(player, client);

                    }
                    break;
                #endregion
                #region show group - effects
                case "showgroup":
                    {
                        GamePlayer player = client.Player.TargetObject as GamePlayer;

                        if (args.Length != 2)
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        if (player == null)
                        {
                            client.Out.SendMessage("You need a valid target!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return;
                        }

                        if (player.Group == null)
                        {
                            client.Out.SendMessage("Player does not have a group!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return;
                        }

                        ArrayList text = new ArrayList();

                        foreach (GamePlayer p in player.Group.GetPlayersInTheGroup())
                        {
                            text.Add(p.Name + " " + p.Level + " " + p.CharacterClass.Name);
                        }

                        client.Out.SendCustomTextWindow("Group Members", text);
                        break;
                    }
                case "showeffects":
                    {
                        GamePlayer player = client.Player.TargetObject as GamePlayer;

                        if (args.Length != 2)
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        if (player == null)
                        {
                            client.Out.SendMessage("You need a valid target!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return;
                        }

                        ArrayList text = new ArrayList();
                        foreach (IGameEffect effect in player.EffectList)
                        {
                            text.Add(effect.Name + " remaining " + effect.RemainingTime);
                        }
                        client.Out.SendCustomTextWindow("Player Effects ", text);
                        break;
                    }
                #endregion
                #region inventory
                case "inventory":
                    {
                        GamePlayer player = client.Player.TargetObject as GamePlayer;

                        if (player == null)
                        {
                            client.Out.SendMessage("You need a valid target!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return;
                        }

                        if (args.Length == 2)
                        {
                            Show_Inventory(player, client);
                        }
                        else
                        {
                            DisplaySyntax(client);
                            return;
                        }
                        break;
                    }
                #endregion

				#region allcharacters
				case "allchars":
					GamePlayer targetPlayer = client.Player.TargetObject as GamePlayer;
					GameClient targetClient = targetPlayer == null ? null : targetPlayer.Client;

					if ( args.Length > 2 )
					{
						targetClient = WorldMgr.GetClientByPlayerName( args[2], true, false );
					}

					if ( targetClient == null )
					{
						DisplaySyntax( client, args[1] );
						return;
					}
					else
					{
						string characterNames = string.Empty;

						foreach ( Character acctChar in targetClient.Account.Characters )
						{
							if ( acctChar != null )
								characterNames += acctChar.Name + " " + acctChar.LastName + "\n";
						}

						client.Out.SendMessage( characterNames, eChatType.CT_Say, eChatLoc.CL_PopupWindow );
					}

					break;
				#endregion allcharacters
			}
        }

        private void SendResistEffect(GamePlayer target)
        {
            if (target != null)
            {
                foreach (GamePlayer nearPlayer in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    nearPlayer.Out.SendSpellEffectAnimation(target, target, 7011, 0, false, 0);
                }
            }
        }

        private static void KillPlayer(GameLiving killer, GamePlayer player)
        {
            int damage = player.Health;
            if (damage > 0)
                player.TakeDamage(killer, eDamageType.Natural, damage, 0);
        }

        private void Show_Inventory(GamePlayer player, GameClient client)
        {
            ArrayList text = new ArrayList();
            text.Add(" ");
            text.Add("PLAYER INVENTORY ");
            text.Add(" ");
            text.Add("  - Name Lastname : " + player.Name + " " + player.LastName);
            text.Add("  - Realm Level Class : " + player.Realm + " " + player.Level + " " + player.CharacterClass.Name);
            text.Add("  - Inventory list: ");
            text.Add(" ");
            text.Add("  ----- WEAR");

            foreach (InventoryItem item in player.Inventory.EquippedItems)
            {
                text.Add("     [" + GlobalConstants.SlotToName(item.Item_Type) + "] " + item.Name);
            }
            text.Add(" ");
            text.Add("  ----- BAG");
            foreach (InventoryItem item in player.Inventory.AllItems)
            {
                if (item.SlotPosition >= 30) text.Add("    " + item.Name);
            }
            client.Out.SendCustomTextWindow("~*PLAYER INVENTORY LISTING*~", text);
        }

        private void Show_Info(GamePlayer player, GameClient client)
        {
            ArrayList text = new ArrayList();
            text.Add(" ");
            text.Add("PLAYER INFORMATION ");
            text.Add("  - Name Lastname : " + player.Name + " " + player.LastName);
            text.Add("  - Realm Level Class : " + player.Realm + " " + player.Level + " " + player.CharacterClass.Name);
            text.Add("  - Guild : " + player.GuildName);
            text.Add("  - XPs/RPs/BPs : " + player.Experience + " xps, " + player.RealmPoints + " rps, " + player.BountyPoints + " bps");
            text.Add("  - Craftingskill : " + player.CraftingPrimarySkill + "");
            text.Add("  - Money : " + Money.GetString(player.GetCurrentMoney()) + "");
            text.Add("  - Model ID : " + player.Model);
            text.Add("  - AFK Message: " + player.TempProperties.getProperty(GamePlayer.AFK_MESSAGE, null) + "");
            text.Add(" ");
            text.Add("ACCOUNT INFORMATION ");
            text.Add("  - Account Name & IP : " + player.Client.Account.Name + " from " + player.Client.Account.LastLoginIP);
            text.Add("  - Priv. Level : " + player.Client.Account.PrivLevel);
            text.Add(" ");
            text.Add("CHARACTER STATS ");

            ArrayList info = new ArrayList();
            String sCurrent = "";
            String sTitle = "";
            int cnt = 0;

            for (eProperty stat = eProperty.Stat_First; stat <= eProperty.Stat_Last; stat++, cnt++)
            {
                sTitle += GlobalConstants.PropertyToName(stat) + "/";
                sCurrent += player.GetModified(stat) + "/";
                if (cnt == 3)
                {
                    text.Add("  - Current stats " + sTitle + " : " + sCurrent);
                    sTitle = "";
                    sCurrent = "";
                }
            }
            text.Add("  - Current stats " + sTitle + " : " + sCurrent);

            sCurrent = "";
            sTitle = "";
            cnt = 0;
            for (eProperty res = eProperty.Resist_First; res <= eProperty.Resist_Last; res++, cnt++)
            {
                sTitle += GlobalConstants.PropertyToName(res) + "/";
                sCurrent += player.GetModified(res) + "/";
                if (cnt == 2)
                {
                    text.Add("  - Current " + sTitle + " : " + sCurrent);
                    sCurrent = "";
                    sTitle = "";
                }
                if (cnt == 5)
                {
                    text.Add("  - Current " + sTitle + " : " + sCurrent);
                    sCurrent = "";
                    sTitle = "";
                }
            }
            text.Add("  - Current " + sTitle + " : " + sCurrent);

            text.Add("  - Maximum Health : " + player.MaxHealth);
            text.Add("  - Current AF and ABS : " + player.GetModified(eProperty.ArmorFactor) + " AF, " + player.GetModified(eProperty.ArmorAbsorption) + " ABS");
            text.Add(" ");
            text.Add("SPECCING INFORMATIONS ");
            text.Add("  - Respecs availables : " + player.RespecAmountDOL + " dol, " + player.RespecAmountSingleSkill + " single, " + player.RespecAmountAllSkill + " full");
            text.Add("  - Remaining spec. points : " + player.SkillSpecialtyPoints);
            sTitle = "  - Player specialisations : ";
            sCurrent = "";
            foreach (Specialization spec in player.GetSpecList())
            {
                sCurrent += spec.Name + " = " + spec.Level + " ; ";
            }
            text.Add(sTitle + sCurrent);

            client.Out.SendCustomTextWindow("~*PLAYER & ACCOUNT INFORMATION*~", text);
        }
    }
}