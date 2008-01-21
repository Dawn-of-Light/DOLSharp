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
using System.Runtime.Serialization;
using System.Collections.Specialized;
using System.Reflection;
using System.Text;
using DOL.Database2;
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
	[Serializable]
    public abstract class AbstractQuest : DatabaseObject
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
        [NonSerialized]
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// The level of the quest.
		/// </summary>
		protected int m_questLevel = 1;
        /// <summary>
        /// Current Quest Step
        /// </summary>
        protected int m_step;
        /// <summary>
        /// Quest Name
        /// </summary>
        protected string m_name;
		/// <summary>
		/// The player doing the quest
		/// </summary>
        [NonSerialized]
        private GamePlayer m_secretquestplayer = null;
        protected GamePlayer m_questPlayer
        {
            get
            {
                if (m_questPlayer == null && m_questPlayerid != null)
                {
                    if (!DatabaseLayer.Instance.DatabaseObjects.TryGetValue(m_questPlayerid, m_secretquestplayer))
                        log.Error("Could not find QuestPlayer" + m_secretquestplayer + " for AbstractQuest " + ID);
                }
            }
            set
            {
                m_secretquestplayer = value;
                m_questPlayerid = value.ID;
            }
        }
        private UInt64 m_questPlayerid;
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
            m_questPlayerid = questingPlayer.ID;
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
            Save();
		}
		  
		/// <summary>
		/// Deletes this quest from the database
		/// </summary>
		public virtual void DeleteFromDatabase()
		{
            DeleteDB();
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
				m_dbQuest.CharName = QuestPlayer.Name;
			}
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
		/// <summary>
		/// This HybridDictionary holds all the custom properties of this quest
		/// </summary>
		protected readonly HybridDictionary m_customProperties = new HybridDictionary();
	}
}