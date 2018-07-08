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
using DOL.GS.Behaviour;
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
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The level of the quest.
        /// </summary>
        private int _questLevel = 1;

        /// <summary>
        /// The player doing the quest
        /// </summary>
        private GamePlayer _questPlayer;

        /// <summary>
        /// The quest database object, storing the information for the player
        /// and the quest. Eg. QuestStep etc.
        /// </summary>
        private readonly DBQuest _dbQuest;

        /// <summary>
        /// List of all QuestParts that can be fired on notify method of quest.
        /// </summary>
        protected static IList QuestParts = null;

        /// <summary>
        /// The various /commands supported by quests
        /// </summary>
        public enum eQuestCommand
        {
            None,
            Search,
            SearchStart,
        }

        private readonly List<QuestSearchArea> _searchAreas = new List<QuestSearchArea>();

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
            _questPlayer = questingPlayer;

            DBQuest dbQuest = new DBQuest
            {
                Character_ID = questingPlayer.QuestPlayerID,
                Name = GetType().FullName,
                Step = step
            };

            _dbQuest = dbQuest;
            SaveIntoDatabase();
        }

        /// <summary>
        /// Constructs a new Quest from a database Object
        /// </summary>
        /// <param name="questingPlayer">The player doing the quest</param>
        /// <param name="dbQuest">The database object</param>
        public AbstractQuest(GamePlayer questingPlayer, DBQuest dbQuest)
        {
            _questPlayer = questingPlayer;
            _dbQuest = dbQuest;
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
                {
                    break;
                }
            }

            if (questType == null)
            {
                questType = Assembly.GetAssembly(typeof(GameServer)).GetType(dbQuest.Name);
            }

            if (questType == null)
            {
                if (Log.IsErrorEnabled)
                {
                    Log.Error($"Could not find quest: {dbQuest.Name}!");
                }

                return null;
            }

            return (AbstractQuest)Activator.CreateInstance(questType, targetPlayer, dbQuest);
        }

        /// <summary>
        /// Saves this quest into the database
        /// </summary>
        public virtual void SaveIntoDatabase()
        {
            if (_dbQuest.IsPersisted)
            {
                GameServer.Database.SaveObject(_dbQuest);
            }
            else
            {
                GameServer.Database.AddObject(_dbQuest);
            }
        }

        /// <summary>
        /// Deletes this quest from the database
        /// </summary>
        public virtual void DeleteFromDatabase()
        {
            if (!_dbQuest.IsPersisted)
            {
                return;
            }

            DBQuest dbQuest = GameServer.Database.FindObjectByKey<DBQuest>(_dbQuest.ObjectId);
            if (dbQuest != null)
            {
                GameServer.Database.DeleteObject(dbQuest);
            }
        }

        /// <summary>
        /// Retrieves how much time player can do the quest
        /// </summary>
        public virtual int MaxQuestCount { get; } = 1;

        /// <summary>
        /// Gets or sets the player doing the quest
        /// </summary>
        public GamePlayer QuestPlayer
        {
            get => _questPlayer;
            set
            {
                _questPlayer = value;
                _dbQuest.Character_ID = QuestPlayer.QuestPlayerID;
            }
        }

        public GamePlayer OfferPlayer { get; set; }

        /// <summary>
        /// Retrieves the name of the quest
        /// </summary>
        public virtual string Name => "QUEST NAME UNDEFINED!";

        /// <summary>
        /// Retrieves the description for the current quest step
        /// </summary>
        public virtual string Description => "QUEST DESCRIPTION UNDEFINED!";

        /// <summary>
        /// Retrieves the minimum level for this quest.
        /// </summary>
        public virtual int Level
        {
            get => _questLevel;
            set
            {
                if (value >= 1 && value <= 50)
                {
                    _questLevel = value;
                }
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
            get => _dbQuest.Step;
            set
            {
                _dbQuest.Step = value;
                SaveIntoDatabase();
                _questPlayer.Out.SendQuestUpdate(this);
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
            _questPlayer.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(_questPlayer.Client, "AbstractQuest.FinishQuest.Completed", Name)), eChatType.CT_ScreenCenter, eChatLoc.CL_SystemWindow);

            // move quest from active list to finished list...
            _questPlayer.QuestList.Remove(this);

            if (_questPlayer.HasFinishedQuest(GetType()) == 0)
            {
                _questPlayer.QuestListFinished.Add(this);
            }

            _questPlayer.Out.SendQuestListUpdate();
        }

        /// <summary>
        /// Called to abort the quest and remove it from the database!
        /// </summary>
        public virtual void AbortQuest()
        {
            Step = -1;
            _questPlayer.QuestList.Remove(this);
            DeleteFromDatabase();
            _questPlayer.Out.SendQuestListUpdate();
            _questPlayer.Out.SendMessage(LanguageMgr.GetTranslation(_questPlayer.Client, "AbstractQuest.AbortQuest"), eChatType.CT_System, eChatLoc.CL_SystemWindow);

            // Todo: quest giver should again "SendNPCsQuestEffect"
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
            player.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractQuest.OnQuestAssigned.GetQuest", Name)), eChatType.CT_System, eChatLoc.CL_ChatWindow);
        }

        private eQuestCommand _currentCommand = eQuestCommand.None;

        protected void AddSearchArea(QuestSearchArea searchArea)
        {
            if (_searchAreas.Contains(searchArea) == false)
            {
                _searchAreas.Add(searchArea);
            }
        }

        public virtual bool Command(GamePlayer player, eQuestCommand command, AbstractArea area = null)
        {
            if (_searchAreas == null || _searchAreas.Count == 0)
            {
                return false;
            }

            if (player == null || command == eQuestCommand.None)
            {
                return false;
            }

            if (command == eQuestCommand.Search)
            {
                foreach (var area1 in player.CurrentAreas)
                {
                    var playerArea = (AbstractArea) area1;
                    if (playerArea is QuestSearchArea questArea && questArea.Step == Step)
                    {
                        foreach (QuestSearchArea searchArea in _searchAreas)
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

            return false;
        }

        /// <summary>
        /// Starts the progress bar for a command.  Override QuestCommandCompleted to handle this command.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="command"></param>
        /// <param name="seconds"></param>
        /// <param name="label">optional label to display above progress bar</param>
        public virtual void StartQuestActionTimer(GamePlayer player, eQuestCommand command, int seconds, string label = "")
        {
            if (player.QuestActionTimer == null)
            {
                _currentCommand = command;
                AddActionHandlers(player);

                if (label == string.Empty)
                {
                    // Live progress dialog is labeled 'Area Action' but I decided to make label more specific - tolakram
                    label = Enum.GetName(typeof(eQuestCommand), command);
                }

                player.Out.SendTimerWindow(label, seconds);
                player.QuestActionTimer = new RegionTimer(player)
                {
                    Callback = new RegionTimerCallback(QuestActionCallback)
                };

                player.QuestActionTimer.Start(seconds * 1000);
            }
        }

        protected virtual int QuestActionCallback(RegionTimer timer)
        {
            if (timer.Owner is GamePlayer player)
            {
                RemoveActionHandlers(player);

                player.Out.SendCloseTimerWindow();
                player.QuestActionTimer.Stop();
                player.QuestActionTimer = null;
                QuestCommandCompleted(_currentCommand, player);
            }

            _currentCommand = eQuestCommand.None;
            return 0;
        }

        protected void AddActionHandlers(GamePlayer player)
        {
            if (player != null)
            {
                GameEventMgr.AddHandler(player, GameLivingEvent.Moving, new DOLEventHandler(InterruptAction));
                GameEventMgr.AddHandler(player, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(InterruptAction));
                GameEventMgr.AddHandler(player, GameLivingEvent.Dying, new DOLEventHandler(InterruptAction));
                GameEventMgr.AddHandler(player, GameLivingEvent.AttackFinished, new DOLEventHandler(InterruptAction));
            }
        }

        protected void RemoveActionHandlers(GamePlayer player)
        {
            if (player != null)
            {
                GameEventMgr.RemoveHandler(player, GameLivingEvent.Moving, new DOLEventHandler(InterruptAction));
                GameEventMgr.RemoveHandler(player, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(InterruptAction));
                GameEventMgr.RemoveHandler(player, GameLivingEvent.Dying, new DOLEventHandler(InterruptAction));
                GameEventMgr.RemoveHandler(player, GameLivingEvent.AttackFinished, new DOLEventHandler(InterruptAction));
            }
        }

        protected void InterruptAction(DOLEvent e, object sender, EventArgs args)
        {
            if (sender is GamePlayer player)
            {
                if (_currentCommand != eQuestCommand.None)
                {
                    string commandName = Enum.GetName(typeof(eQuestCommand), _currentCommand)?.ToLower();
                    if (_currentCommand == eQuestCommand.SearchStart)
                    {
                        commandName = Enum.GetName(typeof(eQuestCommand), eQuestCommand.Search)?.ToLower();
                    }

                    player.Out.SendMessage($"Your {commandName} is interrupted!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                }

                RemoveActionHandlers(player);
                player.Out.SendCloseTimerWindow();
                player.QuestActionTimer.Stop();
                player.QuestActionTimer = null;
                _currentCommand = eQuestCommand.None;
            }
        }

        /// <summary>
        /// Override this to respond to the completion of a quest command, such as /search
        /// </summary>
        /// <param name="command"></param>
        protected virtual void QuestCommandCompleted(eQuestCommand command, GamePlayer player)
        {
            // override this to do whatever needs to be done when the command is completed
            // Typically this would be: give player an item and advance the step
            QuestPlayer.Out.SendMessage("Error, command completed handler not overriden for quest!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
        }

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
                Log.Error("itemtemplate is null in RemoveItem:" + Environment.StackTrace);
                return;
            }

            lock (player.Inventory)
            {
                InventoryItem item = player.Inventory.GetFirstItemByID(itemTemplate.Id_nb, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
                if (item != null)
                {
                    player.Inventory.RemoveItem(item);
                    InventoryLogging.LogInventoryAction(player, target, eInventoryActionType.Quest, item.Template, item.Count);
                    if (target != null)
                    {
                        player.Out.SendMessage($"You give the {itemTemplate.Name} to {target.GetName(0, false)}", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
                }
                else if (notify)
                {
                    player.Out.SendMessage($"You cannot remove the \"{itemTemplate.Name}\" because you don\'t have it.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
            }
        }

        protected static void RemoveItem(GameObject target, GamePlayer player, InventoryItem item, bool notify)
        {
            if (item == null)
            {
                Log.Error("item is null in RemoveItem:" + Environment.StackTrace);
                return;
            }

            lock (player.Inventory)
            {
                player.Inventory.RemoveItem(item);
                InventoryLogging.LogInventoryAction(player, target, eInventoryActionType.Quest, item.Template, item.Count);
                if (target != null)
                {
                    player.Out.SendMessage($"You give the {item.Name} to {target.GetName(0, false)}", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
            }
        }

        protected static int RemoveAllItem(GameLiving target, GamePlayer player, ItemTemplate itemTemplate, bool notify)
        {
            int itemsRemoved = 0;

            if (itemTemplate == null)
            {
                Log.Error("itemtemplate is null in RemoveItem:" + Environment.StackTrace);
                return 0;
            }

            lock (player.Inventory)
            {
                InventoryItem item = player.Inventory.GetFirstItemByID(itemTemplate.Id_nb, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);

                while (item != null)
                {
                    player.Inventory.RemoveItem(item);
                    InventoryLogging.LogInventoryAction(player, target, eInventoryActionType.Quest, item.Template, item.Count);
                    itemsRemoved++;
                    item = player.Inventory.GetFirstItemByID(itemTemplate.Id_nb, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
                }

                if (notify)
                {
                    if (itemsRemoved == 0)
                    {
                        player.Out.SendMessage($"You cannot remove the \"{itemTemplate.Name}\" because you don\'t have it.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
                    else if (target != null)
                    {
                        var message = itemTemplate.Name.EndsWith("s")
                                ? $"You give the {itemTemplate.Name} to {target.Name}"
                                : $"You give the {itemTemplate.Name}\'s to {target.Name}";

                        player.Out.SendMessage(message, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
                }
            }

            return itemsRemoved;
        }

        private static readonly Queue SayTimerQueue = new Queue();
        private static readonly Queue SayObjectQueue = new Queue();
        private static readonly Queue SayMessageQueue = new Queue();
        private static readonly Queue SayChatTypeQueue = new Queue();
        private static readonly Queue SayChatLocQueue = new Queue();

        protected static int MakeSaySequence(RegionTimer callingTimer)
        {
            SayTimerQueue.Dequeue();
            GamePlayer player = (GamePlayer)SayObjectQueue.Dequeue();
            string message = (string)SayMessageQueue.Dequeue();
            eChatType chatType = (eChatType)SayChatTypeQueue.Dequeue();
            eChatLoc chatLoc = (eChatLoc)SayChatLocQueue.Dequeue();

            player.Out.SendMessage(message, chatType, chatLoc);

            return 0;
        }

        protected void SendSystemMessage(string msg)
        {
            SendSystemMessage(_questPlayer, msg);
        }

        protected void SendEmoteMessage(string msg)
        {
            SendEmoteMessage(_questPlayer, msg, 0);
        }

        protected static void SendSystemMessage(GamePlayer player, string msg)
        {
            SendEmoteMessage(player, msg, 0);
        }

        protected static void SendSystemMessage(GamePlayer player, string msg, uint delay)
        {
            SendMessage(player, msg, delay, eChatType.CT_System, eChatLoc.CL_SystemWindow);
        }

        protected static void SendEmoteMessage(GamePlayer player, string msg)
        {
            SendEmoteMessage(player, msg, 0);
        }

        protected static void SendEmoteMessage(GamePlayer player, string msg, uint delay)
        {
            SendMessage(player, msg, delay, eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
        }

        protected static void SendReply(GamePlayer player, string msg)
        {
            SendMessage(player, msg, 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
        }

        /// <summary>
        /// Send a message to player.  You can use &lt;Player&gt;, &lt;Class&gt;, &lt;Race&gt;, &lt;Guild&gt;, &lt;RealmTitle&gt;, &lt;Title&gt; to have text replaced with actual player values.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="msg"></param>
        /// <param name="delay"></param>
        /// <param name="chatType"></param>
        /// <param name="chatLoc"></param>
        protected static void SendMessage(GamePlayer player, string msg, uint delay, eChatType chatType, eChatLoc chatLoc)
        {
            msg = BehaviourUtils.GetPersonalizedMessage(msg, player);

            if (delay == 0)
            {
                player.Out.SendMessage(msg, chatType, chatLoc);
            }
            else
            {
                SayMessageQueue.Enqueue(msg);
                SayObjectQueue.Enqueue(player);
                SayChatLocQueue.Enqueue(chatLoc);
                SayChatTypeQueue.Enqueue(chatType);
                SayTimerQueue.Enqueue(new RegionTimer(player, new RegionTimerCallback(MakeSaySequence), (int)delay * 100));
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
            InventoryItem item;

            if (itemTemplate is ItemUnique unique)
            {
                GameServer.Database.AddObject(unique);
                item = GameInventoryItem.Create(unique);
            }
            else
            {
                item = GameInventoryItem.Create(itemTemplate);
            }

            if (!player.ReceiveItem(source, item))
            {
                if (canDrop)
                {
                    player.CreateItemOnTheGround(item);
                    player.Out.SendMessage($"Your backpack is full, {itemTemplate.Name} is dropped on the ground.", eChatType.CT_Important, eChatLoc.CL_PopupWindow);
                }
                else
                {
                    player.Out.SendMessage("Your backpack is full!", eChatType.CT_Important, eChatLoc.CL_PopupWindow);
                    return false;
                }
            }

            return true;
        }

        protected static ItemTemplate CreateTicketTo(string destination, string ticketId)
        {
            ItemTemplate ticket = GameServer.Database.FindObjectByKey<ItemTemplate>(GameServer.Database.Escape(ticketId.ToLower()));
            if (ticket == null)
            {
                if (Log.IsWarnEnabled)
                {
                    Log.Warn($"Could not find {destination}, creating it ...");
                }

                ticket = new ItemTemplate
                {
                    Name = $"ticket to {destination}",
                    Id_nb = ticketId.ToLower(),
                    Model = 499,
                    Object_Type = (int) eObjectType.GenericItem,
                    Item_Type = 40,
                    IsPickable = true,
                    IsDropable = true,
                    Price = Money.GetMoney(0, 0, 0, 5, 3),
                    PackSize = 1,
                    Weight = 0
                };

                GameServer.Database.AddObject(ticket);
            }

            return ticket;
        }

        /// <summary>
        /// This HybridDictionary holds all the custom properties of this quest
        /// </summary>
        private readonly HybridDictionary _customProperties = new HybridDictionary();

        /// <summary>
        /// This method parses the custom properties string of the m_dbQuest
        /// into the HybridDictionary for easier use and access
        /// </summary>
        public void ParseCustomProperties()
        {
            if (_dbQuest.CustomPropertiesString == null)
            {
                return;
            }

            lock (_customProperties)
            {
                _customProperties.Clear();
                foreach (string property in _dbQuest.CustomPropertiesString.SplitCSV())
                {
                    if (property.Length > 0)
                    {
                        string[] values = property.Split('=');
                        _customProperties[values[0]] = values[1];
                    }
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
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            // Make the string safe
            key = key.Replace(';',',');
            key = key.Replace('=','-');
            value = value.Replace(';',',');
            value = value.Replace('=','-');
            lock (_customProperties)
            {
                _customProperties[key] = value;
            }

            SaveCustomProperties();
        }

        /// <summary>
        /// Saves the custom properties into the database
        /// </summary>
        protected void SaveCustomProperties()
        {
            StringBuilder builder = new StringBuilder();
            lock (_customProperties)
            {
                foreach (string hKey in _customProperties.Keys)
                {
                    builder.Append(hKey);
                    builder.Append("=");
                    builder.Append(_customProperties[hKey]);
                    builder.Append(";");
                }
            }

            _dbQuest.CustomPropertiesString = builder.ToString();
            SaveIntoDatabase();
        }

        /// <summary>
        /// Removes a custom property from the database
        /// </summary>
        /// <param name="key">The key name of the property</param>
        public void RemoveCustomProperty(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            lock (_customProperties)
            {
                _customProperties.Remove(key);
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
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            lock (_customProperties)
            {
                return (string)_customProperties[key];
            }
        }
    }
}