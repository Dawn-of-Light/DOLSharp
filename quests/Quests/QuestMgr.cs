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
using System.Text;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.Scripts;
using log4net;
using DOL.GS.Behaviour.Attributes;using DOL.GS.Behaviour;
using DOL.GS.Quests.Atlantis;

namespace DOL.GS.Quests
{				
	/// <summary>
	/// Declares the quest managed, all questDescriptor instances
	/// must be registered here to be usable
	/// </summary>
    public sealed class QuestMgr
    {  
		#region Declaration

		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Holds all the quests descriptors used in the world (unique id => descriptor)
		/// </summary>
		private static readonly IDictionary m_questDescriptors = new HybridDictionary(1);

		/// <summary>
		/// This is the last used global quest descriptor
		/// </summary>
		private static ushort LAST_USED_QUEST_DESCRIPTOR = ushort.MinValue;

		/// <summary>
		/// Holds all the npc quests descriptors used in the world (npc => list of descriptors)
		/// </summary>
		private static readonly IDictionary  m_questDescriptorsByNPC = new HybridDictionary();

        private static readonly IDictionary m_questTypeMap = new HybridDictionary();

        private static readonly IDictionary m_questActionMap = new HybridDictionary();
        private static readonly IDictionary m_questTriggerMap = new HybridDictionary();
        private static readonly IDictionary m_questRequirementMap = new HybridDictionary();

		#endregion

        public static bool Init()
        {
            //We will search our assemblies for Quests by reflection so 
            //it is not neccessary anymore to register new quests with the 
            //server, it is done automatically!
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                // Walk through each type in the assembly
                foreach (Type type in assembly.GetTypes())
                {
                    // Pick up a class
                    if (type.IsClass != true)
                        continue;
                    
                    if (type.IsSubclassOf(typeof(AbstractQuest)))
                    {
                        if (log.IsInfoEnabled)
                            log.Info("Registering quest: " + type.FullName);
                        RegisterQuestType(type);
						if (type.IsSubclassOf(typeof(ArtifactQuest)))
						{
							log.Info(String.Format("Initialising quest: {0}", type.FullName));
							type.InvokeMember("Init",
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
        

		#region Function
		
		/// <summary>
		/// Creates a new QuestBuilder with the given QuestType
		/// </summary>
		/// <param name="questType">Type of Quest this Builder is used for</param>
		/// <returns>QuestBuilder</returns>
        public static QuestBuilder getBuilder(Type questType) 
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
		/// <param name="lookupDB"></param>
		/// <returns></returns>
		public static GameLiving ResolveLiving(object identifier, GameLiving defaultLiving, bool lookupDB)
		{
			GameLiving living = null;

			if (identifier is string || identifier is int)
			{
				string tempID = Convert.ToString(identifier);

                // TODO: Dirty Hack this should be done better
				Mob mob = (Mob)GameServer.Database.SelectObject(typeof(Mob), "MobID='" + GameServer.Database.Escape(tempID) + "' OR Name='" + GameServer.Database.Escape(tempID) + "'");

                GameNPC[] livings = WorldMgr.GetNPCsByName(mob.Name,(eRealm) mob.Realm);

                if (livings.Length == 1)
                    living = livings[0];
                else if (livings.Length > 1)
                {
                    if (log.IsWarnEnabled)
                        log.Warn("Found more than one living with name :" + tempID + " in " + (lookupDB ? "Database" : "WorldMgr"));
                }
                else
                {
                    if (log.IsWarnEnabled)
                        log.Warn("Couldn't find GameLiving with id or name:" + tempID + " in " + (lookupDB ? "Database" : "WorldMgr"));
                }
			}
			else if (identifier is GameLiving)
			{
				living = (GameLiving)identifier;
			}

			// use default otherwise
			if (living == null)
				living = defaultLiving;

			return living;
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
		/// <param name="defaultNPC"></param>
		/// <returns></returns>
		public static GameNPC ResolveNPC(object identifier, GameNPC defaultNPC)
		{
			return ResolveNPC(identifier,defaultNPC, false);
		}
		/// <summary>
        /// Searches for a NPC with the given id or name either in worldMgr or Database if lookukDB is true
		/// </summary>
		/// <param name="identifier"></param>
		/// <param name="defaultNPC"></param>
		/// <param name="lookupDB"></param>
		/// <returns></returns>
		public static GameNPC ResolveNPC(object identifier, GameNPC defaultNPC, bool lookupDB)
		{
			GameNPC npc = null;

			if (identifier is string || identifier is int)
			{
				string tempID = Convert.ToString(identifier);

                Mob mob = (Mob) GameServer.Database.SelectObject(typeof(Mob), "MobID='" + GameServer.Database.Escape(tempID) + "' OR Name='" + GameServer.Database.Escape(tempID) + "'");

				GameNPC[] npcs = WorldMgr.GetNPCsByName(mob.Name,(eRealm) mob.Realm);

                if (npcs.Length == 1)
                {
                    npc = npcs[0];
                }
                else if (npcs.Length > 1)
                {
                    if (log.IsWarnEnabled)
                        log.Warn("Found more than one npc with id or name:" + tempID + " in WorldMgr");
                }
                else
                {
                    if (log.IsWarnEnabled)
                        log.Warn("Couldn't find NPC with id or name:" + tempID + " in WorldMgr");
                }
			}
			else if (identifier is GameNPC)
			{
				npc =(GameNPC) identifier;
			}

			// use default otherwise
			if (npc == null)
				npc = defaultNPC;

			return npc;			
		}
		

        /// <summary>
        /// Registers the QuestType. This needs to be done to be able to retireve and store his typeid.
        /// </summary>
        /// <param name="type"></param>
        public static void RegisterQuestType(Type type)
        {
            ushort typeId =(ushort) type.GetHashCode();
            if (m_questTypeMap.Contains(typeId))
            {
                if (log.IsErrorEnabled)
                    log.Error(type.FullName+ ": Quest with computed id of="+typeId+" already found.");
                return;
            }
            m_questTypeMap.Add(typeId, type);
        }        

        /// <summary>
        /// Returns a short id for the quest type. 
        /// Used for the messages send to client to identify the questtype
        /// </summary>
        /// <param name="questType"></param>
        /// <returns></returns>
        public static ushort GetIDForQuestType(Type questType)
        {
            return (ushort)questType.GetHashCode();
        }

        /// <summary>
        /// Returns a questtype for the quest id. 
        /// Used for the messages send to client to identify the questtype 
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public static Type GetQuestTypeForID(ushort typeId)
        {
            return (Type)m_questTypeMap[typeId];
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
			if(source != null) 
			{
                return source.CanGiveQuest(questType, target);				
			} else {			
			    return 0;
            }
		}

		/// <summary>
		/// Check if the npc can give a quest to the player
		/// </summary>
		/// <param name="source">The source to check</param>
		/// <param name="target">The player to check</param>
		/// <returns>true if yes</returns>
		public static bool CanGiveOneNewQuest(GameNPC source, GamePlayer target)
		{			
            if (source != null)
            {
                return source.CanGiveOneQuest(target);
            }
            else
            {
                return false;
            }
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
                player.Out.SendQuestSubscribeCommand(source, QuestMgr.GetIDForQuestType(questType), sentence);
                return true;
            }
            else
            {
                return false;
            }					
		}

        /// <summary>
        /// Send the quest dialogue for a classic quest to the player
        /// </summary>
        /// <param name="questType"></param>
        /// <param name="sentence"></param>
        /// <param name="player"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool AbortQuestToPlayer(Type questType, string sentence, GamePlayer player,GameNPC source )
        {
            if (player.IsDoingQuest(questType) != null)
            {
                player.Out.SendQuestAbortCommand(source, QuestMgr.GetIDForQuestType(questType), sentence);
                return true;
            }
            else
            {
                return false;
            }            
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
			if(source != null && QuestMgr.CanGiveQuest(questType, player, source as GameNPC)  <= 0)
				return false;

			if (player.IsDoingQuest(questType) != null)
				return false;

            return source.GiveQuest(questType, player, 1);			
		}

		#endregion
		
    }
    
}
