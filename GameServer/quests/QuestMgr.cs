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
using NHibernate.Expression;
using DOL.GS.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.Scripts;
using log4net;

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

		#endregion

		#region Function

		#region ?
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
		/// <param name="identifier">ID or Name of GameLiving to resolve</param>
		/// <returns>GameLiving</returns>
		public static GameLiving ResolveLiving(object identifier, GameLiving defaultLiving)
		{
			return ResolveLiving(identifier, defaultLiving, false);
		}
		/// <summary>
		/// Searches for a GameLiving with the given id or name either in worldMgr or Database if lookukDB is true
		/// </summary>
		/// <param name="identifier">ID or name of GameLiving</param>
		/// <param name="lookupDB">Search in DB or existing GameLivings in worldMgr</param>
		/// <returns>GameLiving</returns>
		public static GameLiving ResolveLiving(object identifier, GameLiving defaultLiving, bool lookupDB)
		{
			/*GameLiving living = null;

			if (identifier is string || identifier is int)
			{
				string tempID = Convert.ToString(identifier);

				if (lookupDB)
				{
					living = (GameLiving)GameServer.Database.FindObjectByKey(typeof(GameLiving), tempID);
					if (living == null)
						living = (GameLiving)GameServer.Database.SelectObject(typeof(GameLiving), Expression.Eq("Name", tempID));
				}
				else
				{
					living = (GameLiving)WorldMgr.GetObjectByInternalID(tempID);
					if (living == null)
						living = WorldMgr.GetNPCByName(tempID);
				}
				if (living == null)
					log.Warn("Couldn't find GameLiving with id or name:" + tempID + " in " + (lookupDB ? "Database" : "WorldMgr"));
			}
			else if (identifier is GameLiving)
			{
				living = (GameLiving)identifier;
			}

			// use default otherwise
			if (living == null)
				living = defaultLiving;

			return living;*/
			return null;
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
		/// <param name="identifier">ID or Name of NPC to resolve</param>
		/// <returns>NPC</returns>
		public static GameNPC ResolveNPC(object identifier, GameNPC defaultNPC)
		{
			return ResolveNPC(identifier,defaultNPC, false);
		}
		/// <summary>
		/// Searches for a NPC with the given id or name either in worldMgr or Database if lookukDB is true
		/// </summary>
		/// <param name="identifier">ID or name of NPC</param>
		/// <param name="lookupDB">Search in DB or existing NPCs in worldMgr</param>
		/// <returns>NPC</returns>
		public static GameNPC ResolveNPC(object identifier, GameNPC defaultNPC, bool lookupDB)
		{
			/*GameNPC npc = null;

			if (identifier is string || identifier is int)
			{
				string tempID = Convert.ToString(identifier);

				if (lookupDB)
				{
					npc = (GameNPC)GameServer.Database.FindObjectByKey(typeof(GameNPC), tempID);
					if (npc == null)
						npc = (GameNPC)GameServer.Database.SelectObject(typeof(GameNPC), Expression.Eq("Name", tempID));
				}
				else
				{
					npc = WorldMgr.GetObjectByInternalID(tempID) as GameNPC;
					if (npc == null)
						npc = WorldMgr.GetNPCByName(tempID);
				}
				if (npc == null)
					log.Warn("Couldn't find NPC with id or name:" + tempID + " in " + (lookupDB ? "Database" : "WorldMgr"));
			}
			else if (identifier is GameNPC)
			{
				npc =(GameNPC) identifier;
			}

			// use default otherwise
			if (npc == null)
				npc = defaultNPC;

			return npc;*/
			return null;
		}
		#endregion

		/// <summary>
		/// Adds a global quest descriptor
		/// </summary>
		/// <param name="questDescriptorType">The quest requirement type to add</param>
		public static bool AddQuestDescriptor(Type questDescriptorType)
		{
			return AddQuestDescriptor(null, questDescriptorType);
		}

		/// <summary>
		/// Adds a quest descriptor to a npc
		/// </summary>
		/// <param name="questDescriptorType">The quest requirement type to add</param>
		public static bool AddQuestDescriptor(GameNPC target, Type questDescriptorType)
		{
			lock(m_questDescriptors.SyncRoot)
			{
				foreach(AbstractQuestDescriptor desc in m_questDescriptors.Values)
				{
					if(desc.GetType().Equals(questDescriptorType))
						return false;
				}
			}

			LAST_USED_QUEST_DESCRIPTOR ++;
			AbstractQuestDescriptor newQuestDescr = (AbstractQuestDescriptor) Activator.CreateInstance(questDescriptorType);
			newQuestDescr.DescriptorUniqueID = LAST_USED_QUEST_DESCRIPTOR;
			m_questDescriptors.Add(newQuestDescr.DescriptorUniqueID, newQuestDescr);
			
			if(target != null)
			{
				lock(m_questDescriptorsByNPC.SyncRoot)
				{
					IList npcDescriptorsList = (IList)m_questDescriptorsByNPC[target];
					if(npcDescriptorsList == null)
					{	
						npcDescriptorsList = new ArrayList(1);
						m_questDescriptorsByNPC.Add(target, npcDescriptorsList);
					}
					else
					{
						foreach(AbstractQuestDescriptor desc in npcDescriptorsList)
						{
							if(desc.GetType().Equals(questDescriptorType))
								return false;
						}
					}

					npcDescriptorsList.Add(newQuestDescr);	
				}
			}
			return true;
		}

		/// <summary>
		/// Remove a global quest descriptor
		/// </summary>
		/// <param name="questDescriptorType">The quest requirement type to remove</param>
		public static void RemoveQuestDescriptor(Type questDescriptorType)
		{
			RemoveQuestDescriptor(null, questDescriptorType);
		}

		/// <summary>
		/// Remove a quest descriptor from a npc
		/// </summary>
		/// <param name="questDescriptorType">The quest requirement type to remove</param>
		public static void RemoveQuestDescriptor(GameNPC target, Type questDescriptorType)
		{
			lock(m_questDescriptors.SyncRoot)
			{
				foreach(AbstractQuestDescriptor desc in m_questDescriptors.Values)
				{
					if(desc.GetType().Equals(questDescriptorType))
					{
						m_questDescriptors.Remove(desc.DescriptorUniqueID);
						break;
					}
				}
			}
			
			if(target != null)
			{
				lock(m_questDescriptorsByNPC.SyncRoot)
				{
					IList npcDescriptorsList = (IList)m_questDescriptorsByNPC[target];
					if(npcDescriptorsList != null)
					{
						foreach(AbstractQuestDescriptor desc in npcDescriptorsList)
						{
							if(desc.GetType().Equals(questDescriptorType))
							{
								npcDescriptorsList.Remove(desc);
								break;
							}
						}
					}
				}
			}
		}

		
		/// <summary>
		/// Get the quest descriptor with the given id for the given npc
		/// </summary>
		/// <param name="uniqueID">The unique id of the description<param>
		public static AbstractQuestDescriptor GetQuestDescriptor(ushort uniqueID)
		{
			return m_questDescriptors[uniqueID] as AbstractQuestDescriptor;
		}

		/// <summary>
		/// Check if the player can do the global quest
		/// </summary>
		/// <param name="questType">The type of the quest</param>
		/// <param name="target">The player who search a quest</param>
		/// <returns>the number of time the quest can be done again</returns>
		public static int CanGiveQuest(Type questType, GamePlayer target)
		{
			return CanGiveQuest(questType, target, null);
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
			AbstractQuestDescriptor finalDesc = null;
			
			if(source != null) 
			{
				lock(m_questDescriptorsByNPC.SyncRoot)
				{
					IList allQuestDescriptors = (IList)m_questDescriptorsByNPC[source];
					foreach(AbstractQuestDescriptor desc in allQuestDescriptors)
					{
						if(desc.LinkedQuestType.Equals(questType) && desc.CheckQuestQualification(target))
						{
							finalDesc = desc;
							break;
						}
					}
				}
			}
			else
			{
				lock(m_questDescriptors.SyncRoot)
				{
					foreach(AbstractQuestDescriptor desc in m_questDescriptors.Values)
					{
						if(desc.LinkedQuestType.Equals(questType) && desc.CheckQuestQualification(target))
						{
							finalDesc = desc;
							break;
						}
					}
				}
			}

			if(finalDesc != null)
			{
				return finalDesc.MaxQuestCount - target.HasFinishedQuest(questType);
			}

			return 0;
		}

		/// <summary>
		/// Check if the npc can give a quest to the player
		/// </summary>
		/// <param name="source">The source to check</param>
		/// <param name="target">The player to check</param>
		/// <returns>true if yes</returns>
		public static bool CanGiveOneNewQuest(GameNPC source, GamePlayer target)
		{
			IList allQuestDescriptors = (IList)m_questDescriptorsByNPC[source];
			if(allQuestDescriptors != null)
			{
				lock(allQuestDescriptors.SyncRoot)
				{
					foreach(AbstractQuestDescriptor desc in allQuestDescriptors)
					{
						if(desc.CheckQuestQualification(target) && target.IsDoingQuest(desc.LinkedQuestType) == null)
							return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Send the quest dialogue for a global quest to the player
		/// </summary>
		/// <param name="questType">The type of the quest</param>
		/// <param name="target">The player to ask</param>
		/// <param name="sentence">The sentence to show</param>
		public static bool ProposeQuestToPlayer(Type questType, string sentence, GamePlayer target)
		{
			return ProposeQuestToPlayer(questType, sentence, target, null);
		}

		/// <summary>
		/// Send the quest dialogue for a classic quest to the player
		/// </summary>
		/// <param name="questType">The type of the quest</param>
		/// <param name="source">The npc source</param>
		/// <param name="target">The player to ask</param>
		/// <param name="sentence">The sentence to show</param>
		public static bool ProposeQuestToPlayer(Type questType, string sentence, GamePlayer target, GameNPC source)
		{
			AbstractQuestDescriptor finalDesc = null;
					
			if(source != null) 
			{
				lock(m_questDescriptorsByNPC.SyncRoot)
				{
					IList allQuestDescriptors = (IList)m_questDescriptorsByNPC[source];
					foreach(AbstractQuestDescriptor desc in allQuestDescriptors)
					{
						if(desc.LinkedQuestType.Equals(questType) && desc.CheckQuestQualification(target))
						{
							finalDesc = desc;
							break;
						}
					}
				}
			}
			else
			{
				lock(m_questDescriptors.SyncRoot)
				{
					foreach(AbstractQuestDescriptor desc in m_questDescriptors.Values)
					{
						if(desc.LinkedQuestType.Equals(questType) && desc.CheckQuestQualification(target))
						{
							finalDesc = desc;
							break;
						}
					}
				}
			}

			if(finalDesc != null)
			{
				target.Out.SendDialogBox(eDialogCode.QuestSuscribe, finalDesc.DescriptorUniqueID, (ushort)(source != null ? source.ObjectID : 0x00) , 0x00 /*m_data3 accept decline quest*/, 0x00, eDialogType.YesNo, true, sentence);
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
			if(source != null && QuestMgr.CanGiveQuest(questType, player, source as GameNPC)  <= 0)
				return false;

			if (player.IsDoingQuest(questType) != null)
				return false;

			AbstractQuest quest = (AbstractQuest)Activator.CreateInstance(questType);
			quest.QuestPlayer = player;
			quest.Step = startingStep;

			player.AddQuest(quest);

			//Update all npc in the visibility range
			foreach (GameNPC npc in player.GetInRadius(typeof(GameNPC), WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendNPCsQuestEffect(npc, QuestMgr.CanGiveOneNewQuest(npc, player));
			}

			return true;
		}

		#endregion
		
    }
}
