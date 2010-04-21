using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS.Effects;
using log4net;
using System.Reflection;
using DOL.GS.Atlantis;
using DOL.Database;
using DOL.Language;
using DOL.GS.Spells;

namespace DOL.GS.Atlantis
{
    /// <summary>
    /// Handles ArtifactEncounter functions like granting credit.
    /// </summary>
    public static class EncounterMgr
    {
        public static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Grant Credit
        /// <summary>
        /// Grants credit for the encounter to the killer/player and
        /// optionally his group or battlegroup mates.
        /// </summary>
        public static void GrantEncounterCredit(GameObject killer, bool group, bool battlegroup, string artifactid)
        {
            List<GamePlayer> creditPlayerList = new List<GamePlayer>();
            // if controlled NPC - do checks for owner instead
            if (killer is GameNPC)
            {
                IControlledBrain controlled = ((GameNPC)killer).Brain as IControlledBrain;
                if (controlled != null)
                {
                    killer = controlled.GetPlayerOwner();
                }
            }
            GamePlayer player = killer as GamePlayer;
            //add the killer player to the list.
            if (!creditPlayerList.Contains(player))
            {
                creditPlayerList.Add(player);
            }

            //if killing player has a group, let's add those players to the list to receive credit.
            if (player.Group != null && group)
            {

                //player is grouped, let's add the group to the list to recieve credit.
                foreach (GamePlayer groupplayer in (player.Group.GetPlayersInTheGroup()))
                {
                    if (!creditPlayerList.Contains(groupplayer))
                    {
                        //only add players are near enough that they would have earned XP from the kill.
                        if (groupplayer.IsWithinRadius(killer, WorldMgr.MAX_EXPFORKILL_DISTANCE))
                        {
                            creditPlayerList.Add(groupplayer);
                        }
                    }
                }
            }

            //if killing player has a battlegroup, let's add those players to the list to receive credit.
            if (player.isInBG && battlegroup)
            {

                BattleGroup bg = (BattleGroup)player.TempProperties.getProperty<object>(BattleGroup.BATTLEGROUP_PROPERTY, null);
                HybridDictionary bgplayers = bg.Members;
                foreach (GamePlayer eachplayer in bgplayers.Keys)
                {
                    if (!creditPlayerList.Contains(eachplayer))
                    {
                        //only add players who are near enough that they would have earned XP from the kill.
                        if (eachplayer.IsWithinRadius(killer, WorldMgr.MAX_EXPFORKILL_DISTANCE))
                        {
                            creditPlayerList.Add(eachplayer);
                        }
                    }
                }
            }

            //List should now contain the killer, plus optionally the players in his group, and optionally his battlegroup
            if (creditPlayerList.Count > 0)
            {
                foreach (GamePlayer creditplayer in creditPlayerList)
                {
                    creditplayer.Out.SendMessage("The " + artifactid + " encounter has been completed, you should have received credit, if you were eligible.", eChatType.CT_Important, eChatLoc.CL_ChatWindow);
                    ArtifactMgr.GrantArtifactCredit(creditplayer, artifactid);
                }
            }
            return;
        }
        #endregion Grant Credit

        #region Broadcast Message
        /// <summary>
        /// Broadcasts a message to players within saydistance from this object
        /// </summary>
        /// <param name="obj">The object that says the message</param>
        /// <param name="msg">The message that the object says</param>
        /// <param name="IsSaying">Is this object saying the message if false the message will drop the 'objname says' part</param>
        public static void BroadcastMsg(GameObject obj, string msg, bool IsSaying)
        {
            foreach (GamePlayer bPlayer in obj.GetPlayersInRadius((ushort)WorldMgr.SAY_DISTANCE))
            {
                if (IsSaying) { bPlayer.Out.SendMessage(obj.Name + " says \"" + msg + "\"", eChatType.CT_Broadcast, eChatLoc.CL_ChatWindow); }
                else { bPlayer.Out.SendMessage(msg, eChatType.CT_Broadcast, eChatLoc.CL_ChatWindow); }
            }
            return;
        }

        /// <summary>
        /// Broadcasts a message to players within saydistance from this object
        /// </summary>
        /// <param name="obj">The object that says the message</param>
        /// <param name="msg">The message that the object says</param>
        /// <param name="chattype">The chattype of the message</param>
        /// <param name="IsSaying">Is this object saying the message if false the message will drop the 'objname says' part</param>
        public static void BroadcastMsg(GameObject obj, string msg, eChatType chattype, bool IsSaying)
        {
            foreach (GamePlayer bPlayer in obj.GetPlayersInRadius((ushort)WorldMgr.SAY_DISTANCE))
            {
                if (IsSaying) { bPlayer.Out.SendMessage(obj.Name + " says \"" + msg + "\"", chattype, eChatLoc.CL_ChatWindow); }
                else { bPlayer.Out.SendMessage(msg, chattype, eChatLoc.CL_ChatWindow); }
            }
            return;
        }

        /// <summary>
        /// Broadcasts a message to players within (whatever) distance from this object
        /// </summary>
        /// <param name="obj">The object that says the message</param>
        /// <param name="msg">The message that the object says</param>
        /// <param name="distance">The distance which the message is heard</param>
        /// <param name="IsSaying">Is this object saying the message if false the message will drop the 'npcname says' part</param>
        public static void BroadcastMsg (GameObject obj, string msg, int distance, bool IsSaying)
        {
            foreach (GamePlayer bPlayer in obj.GetPlayersInRadius((ushort)distance))
            {
                if (IsSaying) { bPlayer.Out.SendMessage(obj.Name + " says \"" + msg + "\"", eChatType.CT_Broadcast, eChatLoc.CL_ChatWindow); }
                else { bPlayer.Out.SendMessage(msg, eChatType.CT_Broadcast, eChatLoc.CL_ChatWindow); }
            }
            return;
        }

        /// <summary>
        /// Broadcasts a message to players within (whatever) distance from this object
        /// </summary>
        /// <param name="obj">The object that says the message</param>
        /// <param name="msg">The message that the object says</param>
        /// <param name="distance">The distance which the message is heard</param>
        /// <param name="chattype">The chattype of the message</param>
        /// <param name="IsSaying">Is this object saying the message if false the message will drop the 'objname says' part</param>
        public static void BroadcastMsg(GameObject obj, string msg, int distance, eChatType chattype, bool IsSaying)
        {
            foreach (GamePlayer bPlayer in obj.GetPlayersInRadius((ushort)distance))
            {
                if (IsSaying) { bPlayer.Out.SendMessage(obj.Name + " says \"" + msg + "\"", chattype, eChatLoc.CL_ChatWindow); }
                else { bPlayer.Out.SendMessage(msg, chattype, eChatLoc.CL_ChatWindow); }
            }
            return;
        }
        #endregion Broadcast Message

    }
}