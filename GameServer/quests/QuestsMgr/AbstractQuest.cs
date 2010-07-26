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
using System.Collections.Specialized;
using System.Reflection;
using System.Text;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.Language;
using log4net;

namespace DOL.GS.Quests
{
	/// <summary>
	/// Declares the abstract quest class from which all user created
	/// quests must derive!
	/// </summary>
	public abstract class AbstractQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// The level of the quest.
		/// </summary>
		protected int m_questLevel = 1;

		/// <summary>
		/// The player doing the quest
		/// </summary>
		protected GamePlayer m_questPlayer = null;

		/// <summary>
		/// The player who is being offered this quest
		/// </summary>
		protected GamePlayer m_offerPlayer = null;

		/// <summary>
		/// The quest database object, storing the information for the player
		/// and the quest. Eg. QuestStep etc.
		/// </summary>
		private DBQuest m_dbQuest = null;

		/// <summary>
		/// List of all QuestParts that can be fired on notify method of quest.
		/// </summary>
		protected static IList questParts = null;

		/// <summary>
		/// The various /commands supported by quests
		/// </summary>
		public enum eQuestCommand
		{
			None,
			Search
		}

		private List<QuestSearchArea> m_searchAreas = new List<QuestSearchArea>();

		/// <summary>
		/// Constructs a new empty Quest
		/// </summary>		
		public AbstractQuest()
		{
		}

		/// <summary>
		/// Constructs a new Quest
		/// </summary>
		/// <param name="questingPlayer">The player doing this quest</param>
		public AbstractQuest(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		/// <summary>
		/// Constructs a new Quest
		/// </summary>
		/// <param name="questingPlayer">The player doing this quest</param>
		/// <param name="step">The current step the player is on</param>
		public AbstractQuest(GamePlayer questingPlayer,int step)
		{
			m_questPlayer = questingPlayer;
			
			DBQuest dbQuest = new DBQuest();
			dbQuest.Character_ID = questingPlayer.InternalID;
			dbQuest.Name = GetType().FullName;
			dbQuest.Step = step;
			m_dbQuest = dbQuest;
			SaveIntoDatabase();
		}

		/// <summary>
		/// Constructs a new Quest from a database Object
		/// </summary>
		/// <param name="questingPlayer">The player doing the quest</param>
		/// <param name="dbQuest">The database object</param>
		public AbstractQuest(GamePlayer questingPlayer, DBQuest dbQuest)
		{
			m_questPlayer = questingPlayer;
			m_dbQuest = dbQuest;
			ParseCustomProperties();
			SaveIntoDatabase();
		}

		/// <summary>
		/// Loads a quest from the databaseobject and assigns it to a player
		/// </summary>
		/// <param name="targetPlayer">Player to assign the loaded quest</param>
		/// <param name="dbQuest">Quest to load</param>
		/// <returns>The created quest</returns>
		public static AbstractQuest LoadFromDatabase(GamePlayer targetPlayer, DBQuest dbQuest)
		{
			Type questType = null;
			foreach (Assembly asm in ScriptMgr.Scripts)
			{
				questType = asm.GetType(dbQuest.Name);
				if (questType != null)
					break;
			}
			if(questType==null)
				questType = Assembly.GetAssembly(typeof(GameServer)).GetType(dbQuest.Name);
			if(questType==null)
			{
				if (log.IsErrorEnabled)
					log.Error("Could not find quest: "+dbQuest.Name+"!");
				return null;
			}
			return (AbstractQuest)Activator.CreateInstance(questType, new object[] { targetPlayer, dbQuest });
		}

		/// <summary>
		/// Saves this quest into the database
		/// </summary>
		public virtual void SaveIntoDatabase()
		{
			if(m_dbQuest.IsValid)
				GameServer.Database.SaveObject(m_dbQuest);
			else
				GameServer.Database.AddObject(m_dbQuest);
		}
		  
		/// <summary>
		/// Deletes this quest from the database
		/// </summary>
		public virtual void DeleteFromDatabase()
		{
			if(!m_dbQuest.IsValid) return;

			DBQuest dbQuest = (DBQuest) GameServer.Database.FindObjectByKey<DBQuest>(m_dbQuest.ObjectId);
			if(dbQuest!=null)
				GameServer.Database.DeleteObject(dbQuest);
		}

		/// <summary>
		/// Retrieves how much time player can do the quest
		/// </summary>
		public virtual int MaxQuestCount
		{
			get { return 1; }
		}

		/// <summary>
		/// Gets or sets the player doing the quest
		/// </summary>
		public GamePlayer QuestPlayer
		{
			get	{ return m_questPlayer; }
			set	
			{ 
				m_questPlayer = value;
				m_dbQuest.Character_ID = QuestPlayer.InternalID;
			}
		}

		public GamePlayer OfferPlayer
		{
			get { return m_offerPlayer; }
			set { m_offerPlayer = value; }
		}

		/// <summary>
		/// Retrieves the name of the quest
		/// </summary>
		public virtual string Name
		{
			get { return "QUEST NAME UNDEFINED!"; }
		}

		/// <summary>
		/// Retrieves the description for the current quest step
		/// </summary>
		public virtual string Description
		{
			get { return "QUEST DESCRIPTION UNDEFINED!"; }
		}

		/// <summary>
		/// Retrieves the minimum level for this quest.
		/// </summary>
		public virtual int Level
		{
			get { return m_questLevel; }
			set 
			{ 
				if (value >= 1 && value <= 50)
					m_questLevel = value; 
			}
		}

		/// <summary>
		/// Gets or Sets the current step of the quest.
		/// Changing the Quest Step will propably change the 
		/// description and also update the player quest list and
		/// store the changes in the database!
		/// </summary>
		public virtual int Step
		{
			get { return m_dbQuest.Step; }
			set 
			{ 
				m_dbQuest.Step = value; 
				SaveIntoDatabase();
				m_questPlayer.Out.SendQuestUpdate(this);
			}
		}

		/// <summary>
		/// Is this player doing this quest
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public virtual bool IsDoingQuest(AbstractQuest checkQuest)
		{
			return Step != -1; // by default a simple check of this quest step
		}

		/// <summary>
		/// This method needs to be implemented in each quest.
		/// This method checks if a player qualifies for this quest
		/// </summary>
		/// <returns>true if qualified, false if not</returns>
		public abstract bool CheckQuestQualification(GamePlayer player);

		/// <summary>
		/// Called to finish the quest.
		/// Should be overridden and some rewards given etc.
		/// </summary>
		public virtual void FinishQuest()
		{
			Step = -1; // -1 indicates finished or aborted quests etc, they won't show up in the list
			m_questPlayer.Out.SendMessage(String.Format(LanguageMgr.GetTranslation(m_questPlayer.Client, "AbstractQuest.FinishQuest.Completed", Name)), eChatType.CT_ScreenCenter, eChatLoc.CL_SystemWindow);

			// move quest from active list to finished list...
			m_questPlayer.QuestList.Remove(this);

			if (m_questPlayer.HasFinishedQuest(this.GetType()) == 0)
				m_questPlayer.QuestListFinished.Add(this);

			m_questPlayer.Out.SendQuestListUpdate();
		}

		/// <summary>
		/// Called to abort the quest and remove it from the database!
		/// </summary>
		public virtual void AbortQuest()
		{
			Step = -1;
			m_questPlayer.QuestList.Remove(this);
			DeleteFromDatabase();
			m_questPlayer.Out.SendQuestListUpdate();
            m_questPlayer.Out.SendMessage(LanguageMgr.GetTranslation(m_questPlayer.Client, "AbstractQuest.AbortQuest"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            //Todo: quest giver should again "SendNPCsQuestEffect"
        }

		/// <summary>
		/// This method needs to be implemented in each quest.
		/// It is the core of the quest. The global event hook of the GamePlayer.
		/// This method will be called whenever a GamePlayer with this quest
		/// fires ANY event!
		/// </summary>
		/// <param name="e">The event type</param>
		/// <param name="sender">The sender of the event</param>
		/// <param name="args">The event arguments</param>
		public abstract void Notify(DOLEvent e, object sender, EventArgs args);

		/// <summary>
		/// Called when this player has acquired the quest.
		/// </summary>
		/// <param name="player"></param>
		public virtual void OnQuestAssigned(GamePlayer player)
		{
			player.Out.SendMessage(String.Format(LanguageMgr.GetTranslation(player.Client, "AbstractQuest.OnQuestAssigned.GetQuest", Name)), eChatType.CT_Group, eChatLoc.CL_ChatWindow);

		}


		#region Quest Commands

		protected eQuestCommand m_currentCommand = eQuestCommand.None;

		protected void AddSearchArea(QuestSearchArea searchArea)
		{
			if (m_searchAreas.Contains(searchArea) == false)
			{
				m_searchAreas.Add(searchArea);
			}
		}

		public virtual bool Command(GamePlayer player, eQuestCommand command)
		{
			if (m_searchAreas == null || m_searchAreas.Count == 0)
				return false;

			if (player == null || command == eQuestCommand.None)
				return false;

			if (command == eQuestCommand.Search)
			{
				foreach (AbstractArea area in player.CurrentAreas)
				{
					if (area is QuestSearchArea)
					{
						QuestSearchArea questArea = area as QuestSearchArea;

						if (questArea != null && questArea.Step == Step)
						{
							foreach (QuestSearchArea searchArea in m_searchAreas)
							{
								if (searchArea == questArea)
								{
									StartQuestActionTimer(player, command, questArea.SearchSeconds);
									return true;
								}
							}
						}
					}
				}
			}

			return false;
		}

		public virtual void StartQuestActionTimer(GamePlayer player, eQuestCommand command, int seconds)
		{
			if (player.QuestActionTimer == null)
			{
				m_currentCommand = command;
				AddActionHandlers(player);

				// Live progress dialog is labeled 'Area Action' but I dediced to make label more specific - tolakram
				QuestPlayer.Out.SendTimerWindow(Enum.GetName(typeof(eQuestCommand), command), seconds);
				QuestPlayer.QuestActionTimer = new RegionTimer(player);
				QuestPlayer.QuestActionTimer.Callback = new RegionTimerCallback(QuestActionCallback);
				QuestPlayer.QuestActionTimer.Start(seconds * 1000);
			}
			else
			{
				// error message about another action in progress
			}
		}

		protected virtual int QuestActionCallback(RegionTimer timer)
		{
			RemoveActionHandlers(QuestPlayer);

			QuestPlayer.Out.SendCloseTimerWindow();
			QuestPlayer.QuestActionTimer.Stop();
			QuestPlayer.QuestActionTimer = null;
			QuestCommandCompleted(m_currentCommand);
			m_currentCommand = eQuestCommand.None;
			return 0;
		}


		protected void AddActionHandlers(GamePlayer player)
		{
			if (player != null)
			{
				GameEventMgr.AddHandler(player, GamePlayerEvent.Moving, new DOLEventHandler(InterruptAction));
				GameEventMgr.AddHandler(player, GamePlayerEvent.AttackedByEnemy, new DOLEventHandler(InterruptAction));
				GameEventMgr.AddHandler(player, GamePlayerEvent.Dying, new DOLEventHandler(InterruptAction));
				GameEventMgr.AddHandler(player, GamePlayerEvent.AttackFinished, new DOLEventHandler(InterruptAction));
			}
		}

		protected void RemoveActionHandlers(GamePlayer player)
		{
			if (player != null)
			{
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.Moving, new DOLEventHandler(InterruptAction));
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.AttackedByEnemy, new DOLEventHandler(InterruptAction));
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.Dying, new DOLEventHandler(InterruptAction));
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.AttackFinished, new DOLEventHandler(InterruptAction));
			}
		}


		protected void InterruptAction(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player != null)
			{
				if (m_currentCommand != eQuestCommand.None)
				{
					player.Out.SendMessage("Your " + Enum.GetName(typeof(eQuestCommand), m_currentCommand).ToLower() + " is interrupted!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
				}
				RemoveActionHandlers(player);
				player.Out.SendCloseTimerWindow();
				player.QuestActionTimer.Stop();
				player.QuestActionTimer = null;
				m_currentCommand = eQuestCommand.None;
			}
		}

		protected virtual void QuestCommandCompleted(eQuestCommand command)
		{
			// override this to do whatever needs to be done when the command is completed
			// Typically this would be: give player an item and advance the step

			QuestPlayer.Out.SendMessage("Error, command completed handler not overriden for quest!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
		}


		#endregion Quest Commands

		#region Items

		protected static void RemoveItem(GamePlayer player, ItemTemplate itemTemplate)
		{
			RemoveItem(null, player, itemTemplate, true);
		}

		protected static void RemoveItem(GamePlayer player, ItemTemplate itemTemplate, bool notify)
		{
			RemoveItem(null, player, itemTemplate, notify);
		}

		protected static void RemoveItem(GameLiving target, GamePlayer player, ItemTemplate itemTemplate)
		{
			RemoveItem(target, player, itemTemplate, true);
		}

		protected static void ReplaceItem(GamePlayer target, ItemTemplate itemTemplateOut, ItemTemplate itemTemplateIn)
		{
			target.Inventory.BeginChanges();
			RemoveItem(target, itemTemplateOut, false);
			GiveItem(target, itemTemplateIn);
			target.Inventory.CommitChanges();
		}

		protected static void RemoveItem(GameLiving target, GamePlayer player, ItemTemplate itemTemplate, bool notify)
		{
			if (itemTemplate == null)
			{
				log.Error("itemtemplate is null in RemoveItem:" + Environment.StackTrace);
				return;
			}
			lock (player.Inventory)
			{
				InventoryItem item = player.Inventory.GetFirstItemByID(itemTemplate.Id_nb, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
				if (item != null)
				{
					player.Inventory.RemoveItem(item);
					if (target != null)
					{
						player.Out.SendMessage("You give the " + itemTemplate.Name + " to " + target.GetName(0, false), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
				}
				else if (notify)
				{
					player.Out.SendMessage("You cannot remove the \"" + itemTemplate.Name + "\" because you don't have it.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}
		}

		protected static void RemoveItem(GameObject target, GamePlayer player, InventoryItem item, bool notify)
		{
			if (item == null)
			{
				log.Error("item is null in RemoveItem:" + Environment.StackTrace);
				return;
			}
			lock (player.Inventory)
			{
				if (item != null)
				{
					player.Inventory.RemoveItem(item);
					if (target != null)
					{
						player.Out.SendMessage("You give the " + item.Name + " to " + target.GetName(0, false), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
				}
				else if (notify)
				{
					player.Out.SendMessage("You cannot remove the \"" + item.Name + "\" because you don't have it.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}
		}

		protected static int RemoveAllItem(GameLiving target, GamePlayer player, ItemTemplate itemTemplate, bool notify)
		{
			int itemsRemoved = 0;

			if (itemTemplate == null)
			{
				log.Error("itemtemplate is null in RemoveItem:" + Environment.StackTrace);
				return 0;
			}
			lock (player.Inventory)
			{
				InventoryItem item = player.Inventory.GetFirstItemByID(itemTemplate.Id_nb, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);

				while (item != null)
				{
					player.Inventory.RemoveItem(item);
					itemsRemoved++;
					item = player.Inventory.GetFirstItemByID(itemTemplate.Id_nb, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
				}

				if (notify)
				{
					if (itemsRemoved == 0)
					{
						player.Out.SendMessage("You cannot remove the \"" + itemTemplate.Name + "\" because you don't have it.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					else if (target != null)
					{
						if (itemTemplate.Name.EndsWith("s"))
						{
							player.Out.SendMessage("You give the " + itemTemplate.Name + " to " + target.Name, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						else
						{
							player.Out.SendMessage("You give the " + itemTemplate.Name + "'s to " + target.Name, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
					}
				}
			}

			return itemsRemoved;
		}
		#endregion

		public static Queue m_sayTimerQueue = new Queue();
		public static Queue m_sayObjectQueue = new Queue();
		public static Queue m_sayMessageQueue = new Queue();
		public static Queue m_sayChatTypeQueue = new Queue();
		public static Queue m_sayChatLocQueue = new Queue();

		protected static int MakeSaySequence(RegionTimer callingTimer)
		{
			m_sayTimerQueue.Dequeue();
			GamePlayer player = (GamePlayer)m_sayObjectQueue.Dequeue();
			String message = (String)m_sayMessageQueue.Dequeue();
			eChatType chatType = (eChatType)m_sayChatTypeQueue.Dequeue();
			eChatLoc chatLoc = (eChatLoc)m_sayChatLocQueue.Dequeue();

			player.Out.SendMessage(message, chatType, chatLoc);

			return 0;
		}


		protected void SendSystemMessage(String msg)
		{
			SendSystemMessage(m_questPlayer, msg);
		}

		protected void SendEmoteMessage(String msg)
		{
			SendEmoteMessage(m_questPlayer, msg, 0);
		}

		protected static void SendSystemMessage(GamePlayer player, String msg)
		{
			SendEmoteMessage(player, msg, 0);
		}

		protected static void SendSystemMessage(GamePlayer player, String msg, uint delay)
		{
			SendMessage(player, msg, delay, eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		protected static void SendEmoteMessage(GamePlayer player, String msg)
		{
			SendEmoteMessage(player, msg, 0);
		}

		protected static void SendEmoteMessage(GamePlayer player, String msg, uint delay)
		{
			SendMessage(player, msg, delay, eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
		}

		protected static void SendReply(GamePlayer player, String msg)
		{
			SendMessage(player, msg, 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
		}

		protected static void SendMessage(GamePlayer player, String msg, uint delay, eChatType chatType, eChatLoc chatLoc)
		{
			if (delay == 0)
				player.Out.SendMessage(msg, chatType, chatLoc);
			else
			{
				m_sayMessageQueue.Enqueue(msg);
				m_sayObjectQueue.Enqueue(player);
				m_sayChatLocQueue.Enqueue(chatLoc);
				m_sayChatTypeQueue.Enqueue(chatType);
				m_sayTimerQueue.Enqueue(new RegionTimer(player, new RegionTimerCallback(MakeSaySequence), (int)delay * 100));
			}
		}

		protected static bool TryGiveItem(GamePlayer player, ItemTemplate itemTemplate)
		{
			return GiveItem(null, player, itemTemplate, false);
		}

		protected static bool TryGiveItem(GameLiving source, GamePlayer player, ItemTemplate itemTemplate)
		{
			return GiveItem(source, player, itemTemplate, false);
		}

		protected static bool GiveItem(GamePlayer player, ItemTemplate itemTemplate)
		{
			return GiveItem(null, player, itemTemplate, true);
		}

		protected static bool GiveItem(GamePlayer player, ItemTemplate itemTemplate, bool canDrop)
		{
			return GiveItem(null, player, itemTemplate, canDrop);
		}

		protected static bool GiveItem(GameLiving source, GamePlayer player, ItemTemplate itemTemplate)
		{
			return GiveItem(source, player, itemTemplate, true);
		}

		protected static bool GiveItem(GameLiving source, GamePlayer player, ItemTemplate itemTemplate, bool canDrop)
		{
			InventoryItem item = null;

			if (itemTemplate is ItemUnique)
			{
				GameServer.Database.AddObject(itemTemplate as ItemUnique);
				item = GameInventoryItem.Create<ItemUnique>(itemTemplate as ItemUnique);
			}
			else
			{
				item = GameInventoryItem.Create<ItemTemplate>(itemTemplate);
			}

			if (!player.ReceiveItem(source, item))
			{
				if (canDrop)
				{
					player.CreateItemOnTheGround(item);
					player.Out.SendMessage(String.Format("Your backpack is full, {0} is dropped on the ground.", itemTemplate.Name), eChatType.CT_Important, eChatLoc.CL_PopupWindow);
				}
				else
				{
					player.Out.SendMessage("Your backpack is full!", eChatType.CT_Important, eChatLoc.CL_PopupWindow);
					return false;
				}
			}

			return true;
		}

		protected static ItemTemplate CreateTicketTo(String destination, String ticket_Id)
		{
			ItemTemplate ticket = GameServer.Database.FindObjectByKey<ItemTemplate>(GameServer.Database.Escape(ticket_Id.ToLower()));
			if (ticket == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + destination + ", creating it ...");

				ticket = new ItemTemplate();
				ticket.Name = "ticket to " + destination;

				ticket.Id_nb = ticket_Id.ToLower();

				ticket.Model = 499;

				ticket.Object_Type = (int)eObjectType.GenericItem;
				ticket.Item_Type = 40;

				ticket.IsPickable = true;
				ticket.IsDropable = true;

				ticket.Price = Money.GetMoney(0, 0, 0, 5, 3);

				ticket.PackSize = 1;
				ticket.Weight = 0;

				GameServer.Database.AddObject(ticket);
			}
			return ticket;
		}


		#region Custom Properties


		/// <summary>
		/// This HybridDictionary holds all the custom properties of this quest
		/// </summary>
		protected readonly HybridDictionary m_customProperties = new HybridDictionary();


		/// <summary>
		/// This method parses the custom properties string of the m_dbQuest
		/// into the HybridDictionary for easier use and access
		/// </summary>
		public void ParseCustomProperties()
		{
			if(m_dbQuest.CustomPropertiesString == null)
				return;

			lock(m_customProperties)
			{
				m_customProperties.Clear();
				string[] properties = m_dbQuest.CustomPropertiesString.Split(';');
				foreach(string property in properties)
				if(property.Length>0)
				{
					string[] values = property.Split('=');
					m_customProperties[values[0]] = values[1];
				}
			}
		}

		/// <summary>
		/// This method sets a custom Property to a specific value
		/// </summary>
		/// <param name="key">The name of the property</param>
		/// <param name="value">The value of the property</param>
		public void SetCustomProperty(string key, string value)
		{
			if(key==null)
				throw new ArgumentNullException("key");
			if(value==null)
				throw new ArgumentNullException("value");

			//Make the string safe
			key = key.Replace(';',',');
			key = key.Replace('=','-');
			value = value.Replace(';',',');
			value = value.Replace('=','-');
			lock(m_customProperties)
			{
				m_customProperties[key]=value;
			}
			SaveCustomProperties();
		}

		/// <summary>
		/// Saves the custom properties into the database
		/// </summary>
		protected void SaveCustomProperties()
		{
			StringBuilder builder = new StringBuilder();
			lock(m_customProperties)
			{
				foreach(string hKey in m_customProperties.Keys)
				{
					builder.Append(hKey);
					builder.Append("=");
					builder.Append(m_customProperties[hKey]);
					builder.Append(";");
				}
			}
			m_dbQuest.CustomPropertiesString = builder.ToString();
			SaveIntoDatabase();
		}
		
		/// <summary>
		/// Removes a custom property from the database
		/// </summary>
		/// <param name="key">The key name of the property</param>
		public void RemoveCustomProperty(string key)
		{
			if(key==null)
				throw new ArgumentNullException("key");

			lock(m_customProperties)
			{
				m_customProperties.Remove(key);
			}
			SaveCustomProperties();
		}

		/// <summary>
		/// This method retrieves a custom property from the database
		/// </summary>
		/// <param name="key">The property key</param>
		/// <returns>The property value</returns>
		public string GetCustomProperty(string key)
		{
			if(key==null)
				throw new ArgumentNullException("key");

			return (string)m_customProperties[key];
		}

		#endregion
	}
}