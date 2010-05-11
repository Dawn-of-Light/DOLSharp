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

using System.Reflection;
using System.Collections.Generic;
using log4net;
using DOL.GS.PacketHandler;
using DOL.Language;
using DOL.GS.Appeal;

namespace DOL.GS.Commands
{

    [CmdAttribute(
        "&gmappeal",
        new string[] { "&gmhelp" },
        ePrivLevel.GM,
        "Commands for server staff to assist players with their Appeals.",
        "/gmappeal view <player name> - Views the appeal of a specific player.",
        "/gmappeal list - Lists all the current Appeals from online players only, in a window.",
        "/gmappeal listall - Will list Appeals of both offline and online players, in a window.",
        "/gmappeal assist <player name> - Take ownership of the player's appeal and lets other staff know you are helping this player.",
        "/gmappeal jumpto - Will jump you to the player you are currently assisting (must use /gmappeal assist first).",
        "/gmappeal jumpback - Will jump you back to where you were after you've helped the player (must use /gmappeal jumpto first).",
        "/gmappeal close <player name> - Closes the appeal and removes it from the queue.",
        "/gmappeal closeoffline <player name> - Closes an appeal of a player who is not online.",
        "/gmappeal release <player name> - Releases ownership of the player's appeal so someone else can help them.",
        "/gmappeal mute - Toggles receiving appeal notices, for yourself, for this session.",
        "/gmappeal commands - Lists all the commands in a pop up window.")]

    public class GMAppealCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        public void OnCommand(GameClient client, string[] args)
        {
            if (ServerProperties.Properties.DISABLE_APPEALSYSTEM)
            {
                AppealMgr.MessageToClient(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.SystemDisabled"));
                return;
            }

            if (args.Length < 2)
            {
                DisplaySyntax(client);
                return;
            }

            switch (args[1])
            {

                #region gmappeal assist
                case "assist":
                    {
                        if (args.Length < 3)
                        {
                            DisplaySyntax(client);
                            return;
                        }
                        int result = 0;
                        string targetName = args[2];
                        GameClient targetClient = WorldMgr.GuessClientByPlayerNameAndRealm(targetName, 0, false, out result);
                        switch (result)
                        {
                            case 2: // name not unique
                                AppealMgr.MessageToClient(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.NameNotUnique"));
                                return;
                            case 3: // exact match
                            case 4: // guessed name
                                break;
                        }

                        if (targetClient == null)
                        {

                            // nothing found
                            AppealMgr.MessageToClient(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.PlayerNotFound", targetName));
                            return;
                        }

                        DBAppeal appeal = AppealMgr.GetAppealByPlayerName(targetClient.Player.Name);
                        if (appeal != null)
                        {
                            if (appeal.Status != "Being Helped")
                            {
                                AppealMgr.ChangeStatus(client.Player.Name, targetClient.Player, appeal, "Being Helped");
                                string message = LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.RandMessage" + Util.Random(4), targetClient.Player.Name);
                                client.Player.TempProperties.setProperty("AppealAssist", targetClient.Player);
                                client.Player.SendPrivateMessage(targetClient.Player, message);
                                targetClient.Out.SendPlaySound(eSoundType.Craft, 0x04);
                                return;
                            }
                            else
                            {
                                AppealMgr.MessageToClient(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.BeingHelped"));
                                break;
                            }
                        }
                        AppealMgr.MessageToClient(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.DoesntHaveAppeal"));
                        break;
                    }
                #endregion gmappeal assist
                #region gmappeal view

                case "view":
                    {
                        if (args.Length < 3)
                        {
                            DisplaySyntax(client);
                            return;
                        }
                        int result = 0;
                        string targetName = args[2];
                        GameClient targetClient = WorldMgr.GuessClientByPlayerNameAndRealm(targetName, 0, false, out result);
                        switch (result)
                        {

                            case 2: // name not unique
                                AppealMgr.MessageToClient(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.NameNotUnique"));
                                return;
                            case 3: // exact match
                            case 4: // guessed name
                                break;
                        }
                        if (targetClient == null)
                        {

                            // nothing found
                            AppealMgr.MessageToClient(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.PlayerNotFound", targetName));
                            return;
                        }
                        DBAppeal appeal = AppealMgr.GetAppealByPlayerName(targetClient.Player.Name);
                        if (appeal != null)
                        {
                            //Let's view it.
                            List<string> msg = new List<string>();
                            msg.Add("[Appeal]: " + appeal.Name + ", [Status]: " + appeal.Status + ", [Priority]: " + appeal.SeverityToName + " [Issue]: " + appeal.Text + ", [Time]: " + appeal.Timestamp + ".\n");
                            msg.Add("To assist them with the appeal use /gmappeal assist <player name>.\n");
                            msg.Add("To jump yourself to the player use /gmappeal jumpto.\n");
                            msg.Add("For a full list of possible commands, use /gmappeal (with no arguments)");
                            client.Out.SendCustomTextWindow("Viewing " + appeal.Name + "'s Appeal", msg);
                            return;
                        }
                        AppealMgr.MessageToClient(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.DoesntHaveAppeal"));
                        break;
                    }
                #endregion gmappeal view
                #region gmappeal release
                case "release":
                    {
                        if (args.Length < 3)
                        {
                            DisplaySyntax(client);
                            return;
                        }
                        int result = 0;
                        string targetName = args[2];
                        GameClient targetClient = WorldMgr.GuessClientByPlayerNameAndRealm(targetName, 0, false, out result);
                        switch (result)
                        {
                            case 2: // name not unique
                                AppealMgr.MessageToClient(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.NameNotUnique"));
                                return;
                            case 3: // exact match
                            case 4: // guessed name
                                break;
                        }

                        if (targetClient == null)
                        {

                            // nothing found
                            AppealMgr.MessageToClient(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.PlayerNotFound", targetName));
                            return;
                        }

                        DBAppeal appeal = AppealMgr.GetAppealByPlayerName(targetClient.Player.Name);
                        if (appeal != null)
                        {
                            if (appeal.Status == "Being Helped")
                            {
                                AppealMgr.ChangeStatus(client.Player.Name, targetClient.Player, appeal, "Open");
                                client.Player.TempProperties.removeProperty("AppealAssist");
                                return;
                            }
                            else
                            {
                                AppealMgr.MessageToClient(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.NotBeingHelped"));
                                return;
                            }
                        }
                        AppealMgr.MessageToClient(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.DoesntHaveAppeal"));
                        return;
                    }
                #endregion gmappeal release
                #region gmappeal list
                case "list":
                case "listall":
                    {

                        int low = 0;
                        int med = 0;
                        int high = 0;
                        int crit = 0;
                        string caption;
                        IList<DBAppeal> appeallist;
                        List<string> msg = new List<string>();

                        if (args[1] == "listall")
                        {
                            caption = "Offline and Online Player Appeals";
                            appeallist = AppealMgr.GetAllAppealsOffline();
                        }
                        else
                        {
                            caption = "Online Player Appeals";
                            appeallist = AppealMgr.GetAllAppeals();
                        }

                        if (appeallist.Count < 1 || appeallist == null)
                        {
                            AppealMgr.MessageToClient(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.NoAppealsinQueue"));
                            return;
                        }

                        foreach (DBAppeal a in appeallist)
                        {
                            switch (a.Severity)
                            {
                                case (int)AppealMgr.eSeverity.Low:
                                    low++;
                                    break;
                                case (int)AppealMgr.eSeverity.Medium:
                                    med++;
                                    break;
                                case (int)AppealMgr.eSeverity.High:
                                    high++;
                                    break;
                                case (int)AppealMgr.eSeverity.Critical:
                                    crit++;
                                    break;
                            }
                        }
                        int total = appeallist.Count;
                        msg.Add(LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.CurrentStaffAvailable", AppealMgr.StaffList.Count, total) + "\n");
                        msg.Add("Appeals ordered by severity: ");
                        msg.Add("Critical:" + crit + ", High:" + high + " Med:" + med + ", Low:" + low + ".\n");
                        if (crit > 0)
                        {
                            msg.Add("Critical priority appeals:\n");
                            foreach (DBAppeal a in appeallist)
                            {
                                if (a.Severity == (int)AppealMgr.eSeverity.Critical)
                                {
                                    msg.Add("[Name]: " + a.Name + ", [Status]: " + a.Status + ", [Priority]: " + a.SeverityToName + " [Issue]: " + a.Text + ", [Time]: " + a.Timestamp + ".\n");
                                }
                            }
                        }
                        if (high > 0)
                        {
                            msg.Add("High priority appeals:\n");
                            foreach (DBAppeal a in appeallist)
                            {
                                if (a.Severity == (int)AppealMgr.eSeverity.High)
                                {
                                    msg.Add("[Name]: " + a.Name + ", [Status]: " + a.Status + ", [Priority]: " + a.SeverityToName + ", [Issue]: " + a.Text + ", [Time]: " + a.Timestamp + ".\n");
                                }
                            }
                        }
                        if (med > 0)
                        {
                            msg.Add("Medium priority Appeals:\n");
                            foreach (DBAppeal a in appeallist)
                            {
                                if (a.Severity == (int)AppealMgr.eSeverity.Medium)
                                {
                                    msg.Add("[Name]: " + a.Name + ", [Status]: " + a.Status + ", [Priority]: " + a.SeverityToName + ", [Issue]: " + a.Text + ", [Time]: " + a.Timestamp + ".\n");
                                }
                            }
                        }
                        if (low > 0)
                        {
                            msg.Add("Low priority appeals:\n");
                            foreach (DBAppeal a in appeallist)
                            {
                                if (a.Severity == (int)AppealMgr.eSeverity.Low)
                                {
                                    msg.Add("[Name]: " + a.Name + ", [Status]: " + a.Status + ", [Priority]: " + a.SeverityToName + ", [Issue]: " + a.Text + ", [Time]: " + a.Timestamp + ".\n");
                                }
                            }
                        }
                        client.Out.SendCustomTextWindow(caption, msg);
                    }

                    break;
                #endregion gmappeal list
                #region gmappeal close
                case "close":
                    {
                        if (args.Length < 3)
                        {
                            DisplaySyntax(client);
                            return;
                        }
                        int result = 0;
                        string targetName = args[2];
                        GameClient targetClient = WorldMgr.GuessClientByPlayerNameAndRealm(targetName, 0, false, out result);
                        switch (result)
                        {
                            case 2: // name not unique
                                AppealMgr.MessageToClient(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.NameNotUnique"));
                                return;
                            case 3: // exact match
                            case 4: // guessed name
                                break;
                        }

                        if (targetClient == null)
                        {

                            // nothing found
                            AppealMgr.MessageToClient(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.PlayerNotFound", targetName));
                            return;
                        }

                        DBAppeal appeal = AppealMgr.GetAppealByPlayerName(targetClient.Player.Name);
                        if (appeal == null)
                        {
                            AppealMgr.MessageToClient(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.DoesntHaveAppeal"));
                            return;
                        }
                        AppealMgr.CloseAppeal(client.Player.Name, targetClient.Player, appeal);
                        client.Player.TempProperties.removeProperty("AppealAssist");
                        return;
                    }

                #endregion gmappeal close
                #region gmappeal closeoffline
                case "closeoffline":
                    {
                        if (args.Length < 3)
                        {
                            DisplaySyntax(client);
                            return;
                        }
                        string targetName = args[2];
                        DBAppeal appeal = AppealMgr.GetAppealByPlayerName(targetName);
                        if (appeal == null)
                        {
                            AppealMgr.MessageToClient(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.CantFindAppeal"));
                            return;
                        }
                        AppealMgr.CloseAppeal(client.Player.Name, appeal);

                        //just incase the player is actually online let's check so we can handle it properly
                        string targetNameTwo = args[2];
                        int resultTwo = 0;
                        GameClient targetClient = WorldMgr.GuessClientByPlayerNameAndRealm(targetNameTwo, 0, false, out resultTwo);
                        switch (resultTwo)
                        {
                            case 2: // name not unique
                                AppealMgr.MessageToClient(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.NameNotUnique"));
                                return;
                            case 3: // exact match
                            case 4: // guessed name
                                break;
                        }
                        if (targetClient == null)
                        {

                            // player isn't online so we're fine.
                            return;
                        }
                        else
                        {
                            //cleaning up the player since he really was online.
                            AppealMgr.MessageToClient(targetClient, LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.StaffClosedYourAppeal", client.Player.Name));
                            targetClient.Out.SendPlaySound(eSoundType.Craft, 0x02);
                            targetClient.Player.TempProperties.setProperty("HasPendingAppeal", false);
                        }
                        return;
                    }

                #endregion gmappeal closeoffline
                #region gmappeal jumpto
                case "jumpto":
                    {
                        try
                        {
                            GamePlayer p = client.Player.TempProperties.getProperty<GamePlayer>("AppealAssist");
                            if (p.ObjectState == GameObject.eObjectState.Active)
                            {
                                GameLocation oldlocation = new GameLocation("old", client.Player.CurrentRegionID, client.Player.X, client.Player.Y, client.Player.Z);
                                client.Player.TempProperties.setProperty("AppealJumpOld", oldlocation);
                                client.Player.MoveTo(p.CurrentRegionID, p.X, p.Y, p.Z, p.Heading);
                            }
                            break;
                        }
                        catch
                        {
                            AppealMgr.MessageToClient(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.MustBeAssisting"));
                            break;
                        }
                    }
                case "jumpback":
                    {
                        GameLocation jumpback = client.Player.TempProperties.getProperty<GameLocation>("AppealJumpOld");

                        if (jumpback != null)
                        {
                            client.Player.MoveTo(jumpback);
                            //client.Player.TempProperties.removeProperty("AppealJumpOld");
                            break;
                        }
                        else
                        {
                            AppealMgr.MessageToClient(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.NoLocationToJump"));
                        }
                        break;
                    }

                #endregion gmappeal jumpto
                #region gmappeal mute
                case "mute":
                    {
                        bool mute = client.Player.TempProperties.getProperty<bool>("AppealMute");
                        if (mute == false)
                        {
                            client.Player.TempProperties.setProperty("AppealMute", true);
                            AppealMgr.MessageToClient(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.NoLongerReceiveMsg"));
                            AppealMgr.MessageToClient(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.UseCmdTurnBackOn"));
                            AppealMgr.StaffList.Remove(client.Player);
                        }
                        else
                        {
                            client.Player.TempProperties.setProperty("AppealMute", false);
                            AppealMgr.MessageToClient(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.NowReceiveMsg"));
                            AppealMgr.MessageToClient(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Appeal.UseCmdTurnBackOff"));
                            AppealMgr.StaffList.Add(client.Player);
                        }

                        break;
                    }
                #endregion gmappeal mute
                #region gmappeal commands
                case "commands":
                case "cmds":
                case "help":
                    //List all the commands in a pop up window
                    List<string> helpmsg = new List<string>();
                    helpmsg.Add("Commands for server staff to assist players with their Appeals.");
                    helpmsg.Add("/gmappeal view <player name> - Views the appeal of a specific player.");
                    helpmsg.Add("/gmappeal list - Lists all the current Appeals from online players only, in a window.");
                    helpmsg.Add("/gmappeal listall - Will list Appeals of both offline and online players, in a window.");
                    helpmsg.Add("/gmappeal assist <player name> - Take ownership of the player's appeal and lets other staff know you are helping this player.");
                    helpmsg.Add("/gmappeal jumpto - Will jump you to the player you are currently assisting (must use /gmappeal assist first).");
                    helpmsg.Add("/gmappeal jumpback - Will jump you back to where you were after you've helped the player (must use /gmappeal jumpto first).");
                    helpmsg.Add("/gmappeal close <player name> - Closes the appeal and removes it from the queue.");
                    helpmsg.Add("/gmappeal closeoffline <player name> - Closes an appeal of a player who is not online.");
                    helpmsg.Add("/gmappeal release <player name> - Releases ownership of the player's appeal so someone else can help them.");
                    helpmsg.Add("/gmappeal mute - Toggles receiving appeal notices, for yourself, for this session.");
                    client.Out.SendCustomTextWindow("/gmappeal commands list", helpmsg);
                    break;
                #endregion gmappeal commands
                default:
                    {
                        DisplaySyntax(client);
                        return;
                    }
            }
            return;
        }
    }
}