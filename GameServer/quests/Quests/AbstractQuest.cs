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

		private List<SearchLocation> m_searchLocations = new List<SearchLocation>();

		protected class SearchLocation : GameLocation
		{
			public const int DEFAULT_SEARCH_SECONDS = 5;
			public const int DEFAULT_SEARCH_RADIUS = 150;

			protected QuestSearchArea m_area;

			/// <summary>
			/// Create a /search location for this quest.
			/// This will create an area for searching and popup a message to the player when they are 
			/// in the area to be /searched
			/// </summary>
			/// <param name="questType"></param>
			/// <param name="step"></param>
			/// <param name="name"></param>
			/// <param name="regionId"></param>
			/// <param name="x"></param>
			/// <param name="y"></param>
			/// <param name="z"></param>
			public SearchLocation(Type questType, int step, string name, ushort regionId, int x, int y, int z)
				: base(name, regionId, x, y, z)
			{
				CreateSearchLocation(questType, step, DEFAULT_SEARCH_SECONDS, name, regionId, x, y, z, DEFAULT_SEARCH_RADIUS);
			}

			/// <summary>
			/// Create a /search location for this quest.
			/// This will create an area for searching and popup a message to the player when they are 
			/// in the area to be /searched. Optionally provide the number of seconds to search and the radius of the search area.
			/// </summary>
			/// <param name="questType"></param>
			/// <param name="step"></param>
			/// <param name="searchSeconds"></param>
			/// <param name="name"></param>
			/// <param name="regionId"></param>
			/// <param name="x"></param>
			/// <param name="y"></param>
			/// <param name="z"></param>
			/// <param name="radius"></param>
			public SearchLocation(Type questType, int step, int searchSeconds, string name, ushort regionId, int x, int y, int z, int radius)
				: base(name, regionId, x, y, z)
			{
				CreateSearchLocation(questType, step, searchSeconds, name, regionId, x, y, z, radius);
			}

			protected void CreateSearchLocation(Type questType, int step, int searchSeconds, string name, ushort regionId, int x, int y, int z, int radius)
			{
				m_area = new QuestSearchArea(questType, step, searchSeconds, name, x, y, z, radius);
				m_area.DisplayMessage = false;

				if (WorldMgr.GetRegion(regionId) != null)
				{
					WorldMgr.GetRegion(regionId).AddArea(m_area);
				}
			}

			public QuestSearchArea SearchArea
			{
				get { return m_area; }
			}
		}

		protected class QuestSearchArea : Area.Circle
		{
			Type m_questType;
			int m_validStep;
			int m_searchSeconds;
			string m_popupText = "";
			
	        public QuestSearchArea(Type questType, int validStep, int searchSeconds, string desc, int x, int y, int z, int radius)
	            : base(desc, x, y, z, radius)
	        {
				m_questType = questType;
				m_validStep = validStep;
				m_searchSeconds = searchSeconds;
				m_popupText = desc;
				DisplayMessage = false;
			}

			public override void OnPlayerEnter(GamePlayer player)
			{
				// popup a dialog telling the player they should search here
				if (player.IsDoingQuest(m_questType) != null && player.IsDoingQuest(m_questType).Step == m_validStep)
				{
					player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0, 0, 0, 0, eDialogType.Ok, true, m_popupText);
				}
			}

			public Type QuestType
			{
				get { return m_questType; }
			}

			public int Step
			{
				get { return m_validStep; }
			}

			public int SearchSeconds
			{
				get { return m_searchSeconds; }
			}
		}


		/// <summary>
		/// Constructs a new empty Quest
		/// </summary>		
		public AbstractQuest()
		{
			//m_dbQuest = new DBQuest();
			//m_dbQuest.Name = GetType().FullName;
			//m_dbQuest.Step = 1;
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

		protected void AddSearchLocation(SearchLocation location)
		{
			if (m_searchLocations.Contains(location) == false)
			{
				m_searchLocations.Add(location);
			}
		}

		public virtual bool Command(GamePlayer player, eQuestCommand command)
		{
			if (m_searchLocations == null || m_searchLocations.Count == 0)
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
							foreach (SearchLocation location in m_searchLocations)
							{
								if (location.SearchArea == questArea)
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
				AddSearchActionHandlers(player);

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
			RemoveSearchActionHandlers(QuestPlayer);

			QuestPlayer.Out.SendCloseTimerWindow();
			QuestPlayer.QuestActionTimer.Stop();
			QuestPlayer.QuestActionTimer = null;
			QuestCommandCompleted(m_currentCommand);
			m_currentCommand = eQuestCommand.None;
			return 0;
		}


		protected void AddSearchActionHandlers(GamePlayer player)
		{
			if (player != null)
			{
				GameEventMgr.AddHandler(player, GamePlayerEvent.Moving, new DOLEventHandler(InterruptSearch));
				GameEventMgr.AddHandler(player, GamePlayerEvent.AttackedByEnemy, new DOLEventHandler(InterruptSearch));
				GameEventMgr.AddHandler(player, GamePlayerEvent.Dying, new DOLEventHandler(InterruptSearch));
				GameEventMgr.AddHandler(player, GamePlayerEvent.AttackFinished, new DOLEventHandler(InterruptSearch));
			}
		}

		protected void RemoveSearchActionHandlers(GamePlayer player)
		{
			if (player != null)
			{
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.Moving, new DOLEventHandler(InterruptSearch));
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.AttackedByEnemy, new DOLEventHandler(InterruptSearch));
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.Dying, new DOLEventHandler(InterruptSearch));
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.AttackFinished, new DOLEventHandler(InterruptSearch));
			}
		}


		protected void InterruptSearch(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player != null)
			{
				player.Out.SendMessage("Your search is interrupted!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
				RemoveSearchActionHandlers(player);
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