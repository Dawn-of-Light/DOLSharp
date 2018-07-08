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
using System.Collections.Specialized;
using System.Reflection;
using System.Linq;
using log4net;
using DOL.GS.Quests.Atlantis;
using DOL.Database;

namespace DOL.GS.Quests
{
    /// <summary>
    /// Declares the quest managed, all questDescriptor instances
    /// must be registered here to be usable
    /// This manager is used for scripted quests
    /// </summary>
    public sealed class QuestMgr
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly IDictionary QuestTypeMap = new HybridDictionary();

        public static bool Init()
        {
            // We will search our assemblies for Quests by reflection so
            // it is not neccessary anymore to register new quests with the
            // server, it is done automatically!
            foreach (Assembly assembly in ScriptMgr.GameServerScripts)
            {
                // Walk through each type in the assembly
                foreach (Type type in assembly.GetTypes())
                {
                    // Pick up a class
                    if (type.IsClass != true)
                    {
                        continue;
                    }

                    if (type.IsSubclassOf(typeof(AbstractQuest)))
                    {
                        if (Log.IsInfoEnabled)
                        {
                            Log.Info($"Registering quest: {type.FullName}");
                        }

                        RegisterQuestType(type);
                        if (type.IsSubclassOf(typeof(ArtifactQuest)))
                        {
                            Log.Info($"Initialising quest: {type.FullName}");
                            type.InvokeMember(
                                "Init",
                                BindingFlags.InvokeMethod,
                                null,
                                null,
                                new object[] { });
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Creates a new QuestBuilder with the given QuestType
        /// </summary>
        /// <param name="questType">Type of Quest this Builder is used for</param>
        /// <returns>QuestBuilder</returns>
        public static QuestBuilder GetBuilder(Type questType)
        {
            return new QuestBuilder(questType);
        }

        /// <summary>
        /// Searches for a GameLiving with the given id or name in WorldMgr
        /// </summary>
        /// <param name="identifier">ID or Name of GameLiving to resolve</param>
        /// <returns>GameLiving</returns>
        public static GameLiving ResolveLiving(object identifier)
        {
            return ResolveLiving(identifier, null, false);
        }

        /// <summary>
        /// Searches for a GameLiving with the given id or name in WorldMgr
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="defaultLiving"></param>
        /// <returns></returns>
        public static GameLiving ResolveLiving(object identifier, GameLiving defaultLiving)
        {
            return ResolveLiving(identifier, defaultLiving, false);
        }

        /// <summary>
        /// Searches for a GameLiving with the given id or name either in worldMgr or Database if lookukDB is true
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="defaultLiving"></param>
        /// <param name="lookupDb"></param>
        /// <returns></returns>
        public static GameLiving ResolveLiving(object identifier, GameLiving defaultLiving, bool lookupDb)
        {
            GameLiving living = null;

            if (identifier is string || identifier is int)
            {
                string tempId = Convert.ToString(identifier);

                // TODO: Dirty Hack this should be done better
                Mob mob = GameServer.Database.SelectObjects<Mob>("`Mob_ID` = @MobID OR `Name` = @Name", new[] { new QueryParameter("@MobID", tempId), new QueryParameter("@Name", tempId) }).FirstOrDefault();

                if (mob != null)
                {
                    GameNPC[] livings = WorldMgr.GetObjectsByName<GameNPC>(mob.Name, (eRealm)mob.Realm);

                    if (livings.Length == 1)
                    {
                        living = livings[0];
                    }
                    else if (livings.Length > 1)
                    {
                        if (Log.IsWarnEnabled)
                        {
                            Log.Warn($"Found more than one living with name :{tempId} in {(lookupDb ? "Database" : "WorldMgr")}");
                        }
                    }
                    else
                    {
                        if (Log.IsWarnEnabled)
                        {
                            Log.Warn($"Couldn\'t find GameLiving with id or name:{tempId} in {(lookupDb ? "Database" : "WorldMgr")}");
                        }
                    }
                }
            }
            else if (identifier is GameLiving)
            {
                living = (GameLiving)identifier;
            }

            // use default otherwise
            return living ?? defaultLiving;
        }

        /// <summary>
        /// Searches for a NPC with the given id or name in WorldMgr
        /// </summary>
        /// <param name="identifier">ID or Name of NPC to resolve</param>
        /// <returns>NPC</returns>
        public static GameNPC ResolveNPC(object identifier)
        {
            return ResolveNPC(identifier,null, false);
        }

        /// <summary>
        /// Searches for a NPC with the given id or name in WorldMgr
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="defaultNpc"></param>
        /// <returns></returns>
        public static GameNPC ResolveNPC(object identifier, GameNPC defaultNpc)
        {
            return ResolveNPC(identifier,defaultNpc, false);
        }

        /// <summary>
        /// Searches for a NPC with the given id or name either in worldMgr or Database if lookukDB is true
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="defaultNpc"></param>
        /// <param name="lookupDb"></param>
        /// <returns></returns>
        public static GameNPC ResolveNPC(object identifier, GameNPC defaultNpc, bool lookupDb)
        {
            GameNPC npc = null;

            if (identifier is string || identifier is int)
            {
                string tempID = Convert.ToString(identifier);

                Mob mob = GameServer.Database.SelectObjects<Mob>("`Mob_ID` = @MobID OR `Name` = @Name", new[] { new QueryParameter("@MobID", tempID), new QueryParameter("@Name", tempID) }).FirstOrDefault();

                GameNPC[] npcs = WorldMgr.GetObjectsByName<GameNPC>(mob.Name, (eRealm)mob.Realm);

                if (npcs.Length == 1)
                {
                    npc = npcs[0];
                }
                else if (npcs.Length > 1)
                {
                    if (Log.IsWarnEnabled)
                    {
                        Log.Warn($"Found more than one npc with id or name:{tempID} in WorldMgr");
                    }
                }
                else
                {
                    if (Log.IsWarnEnabled)
                    {
                        Log.Warn($"Couldn\'t find NPC with id or name:{tempID} in WorldMgr");
                    }
                }
            }
            else if (identifier is GameNPC)
            {
                npc = (GameNPC)identifier;
            }

            // use default otherwise
            if (npc == null)
            {
                npc = defaultNpc;
            }

            return npc;
        }

        /// <summary>
        /// Registers the QuestType. This needs to be done to be able to retireve and store his typeid.
        /// </summary>
        /// <param name="type"></param>
        public static void RegisterQuestType(Type type)
        {
            ushort typeId = (ushort)(QuestTypeMap.Count + 1);
            if (QuestTypeMap.Contains(typeId))
            {
                if (Log.IsErrorEnabled)
                {
                    Log.Error($"{type.FullName}: Quest with computed id of={typeId} already found.");
                }

                return;
            }

            QuestTypeMap.Add(typeId, type);
        }

        /// <summary>
        /// Returns a short id for the quest type.
        /// Used for the messages send to client to identify the questtype
        /// </summary>
        /// <param name="questType"></param>
        /// <returns></returns>
// public static ushort GetIDForQuestType(Type questType)
//        {
//            return (ushort)questType.GetHashCode();
//        }
        public static ushort GetIDForQuestType(Type questType)
        {
            IDictionaryEnumerator questEnumerator = QuestTypeMap.GetEnumerator();
            while (questEnumerator.MoveNext())
            {
                if ((Type)questEnumerator.Value == questType)
                {
                    return (ushort)questEnumerator.Key;
                }
            }

            return 0;
        }

        /// <summary>
        /// Returns a questtype for the quest id.
        /// Used for the messages send to client to identify the questtype
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public static Type GetQuestTypeForID(ushort typeId)
        {
            return (Type)QuestTypeMap[typeId];
        }

        /// <summary>
        /// Check if the npc can give the quest to the player
        /// </summary>
        /// <param name="questType">The type of the quest</param>
        /// <param name="source">The npc source</param>
        /// <param name="target">The player who search a quest</param>
        /// <returns>the number of time the quest can be done again</returns>
        public static int CanGiveQuest(Type questType, GamePlayer target, GameNPC source)
        {
            return NewMethod(questType, target, source);
        }

        private static int NewMethod(Type questType, GamePlayer target, GameNPC source)
        {
            if (source != null)
            {
                return source.CanGiveQuest(questType, target);
            }

            return 0;
        }

        /// <summary>
        /// Send the quest dialogue for a classic quest to the player
        /// </summary>
        /// <param name="questType"></param>
        /// <param name="sentence"></param>
        /// <param name="player"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool ProposeQuestToPlayer(Type questType, string sentence, GamePlayer player, GameNPC source)
        {
            if (CanGiveQuest(questType, player, source) > 0)
            {
                if (questType.IsSubclassOf(typeof(RewardQuest)))
                {
                    RewardQuest rquest = null;
                    foreach (Assembly asm in ScriptMgr.Scripts)
                    {
                        try
                        {
                            rquest = (RewardQuest)asm.CreateInstance(questType.FullName, false, BindingFlags.CreateInstance, null, new object[] { }, null, null);
                        }
                        catch (Exception e)
                        {
                            if (Log.IsErrorEnabled)
                            {
                                Log.Error("ProposeQuestToPlayer.RewardQuest", e);
                            }
                        }

                        if (rquest != null)
                        {
                            break;
                        }
                    }

                    player.Out.SendQuestOfferWindow(source, player, rquest);
                }
                else
                {
                    player.Out.SendQuestSubscribeCommand(source, GetIDForQuestType(questType), sentence);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Send the quest dialogue for a classic quest to the player
        /// </summary>
        /// <param name="questType"></param>
        /// <param name="sentence"></param>
        /// <param name="player"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool AbortQuestToPlayer(Type questType, string sentence, GamePlayer player,GameNPC source)
        {
            if (player.IsDoingQuest(questType) != null)
            {
                player.Out.SendQuestAbortCommand(source, GetIDForQuestType(questType), sentence);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Send the quest dialogue for a non npc quest to the player
        /// </summary>
        /// <param name="questType">The type of the quest</param>
        /// <param name="player">The player to give the quest</param>
        /// <returns>true if added</returns>
        public static bool GiveQuestToPlayer(Type questType, GamePlayer player)
        {
            return GiveQuestToPlayer(questType, 1, player, null);
        }

        /// <summary>
        /// Send the quest dialogue for a classic quest to the player
        /// </summary>
        /// <param name="questType">The type of the quest</param>
        /// <param name="player">The player to give the quest</param>
        /// <param name="source">The npc who give the questw</param>
        /// <returns>true if added</returns>
        public static bool GiveQuestToPlayer(Type questType, GamePlayer player, GameNPC source)
        {
            return GiveQuestToPlayer(questType, 1, player, source);
        }

        /// <summary>
        /// Send the quest dialogue for a classic quest to the player
        /// </summary>
        /// <param name="questType">The type of the quest</param>
        /// <param name="startingStep">The staring step of the quest</param>
        /// <param name="player">The player to give the quest</param>
        /// <param name="source">The npc who give the questw</param>
        /// <returns>true if added</returns>
        public static bool GiveQuestToPlayer(Type questType, byte startingStep, GamePlayer player, GameNPC source)
        {
            if (source != null && CanGiveQuest(questType, player, source) <= 0)
            {
                return false;
            }

            if (player.IsDoingQuest(questType) != null)
            {
                return false;
            }

            return source.GiveQuest(questType, player, 1);
        }
    }
}
