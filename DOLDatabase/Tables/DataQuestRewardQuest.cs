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

using DOL.Database.Attributes;

namespace DOL.Database
{
	/// <summary>
	/// Holds all the DQRewardquest types for DataQuests available
	/// </summary>
	[DataTable(TableName = "RewardQuest")]
	
	public class DBRewardQuest : DataObject
	{
		private int m_id;
		private string m_questName;		
		private string m_startNPC;
		private ushort m_startRegionID;
		private string m_storyText;
		private string m_summary;
        private string m_acceptText;
		private string m_questGoals;
		private string m_goalType;		
		private string m_goalRepeatNo;
		private string m_goaltargetName;
		private string m_goaltargetText;
		private int m_stepCount;
		private string m_finishNPC; // maybe dont need		
		private string m_goaladvanceText;		
		private string m_collectItemTemplate;
		private ushort m_maxCount;
		private byte m_minLevel;
		private byte m_maxLevel;
		private long m_rewardMoney;
		private long m_rewardXP;
		private long m_rewardCLXP;
		private long m_rewardRP;
		private long m_rewardBP;
		private string m_optionalRewardItemTemplates;
		private string m_finalRewardItemTemplates;
		private string m_finishText;
		private string m_questDependency;
		private string m_allowedClasses;
		private string m_classType;
		private string m_xOffset;
		private string m_yOffset;
		private string m_zoneID;		
		
		public DBRewardQuest()
		{
		}

		[PrimaryKey(AutoIncrement = true)]
		public int ID
		{
			get { return m_id; }
			set { m_id = value; }
		}

        /// <summary>
        /// The name of this quest
        /// </summary>
        [DataElement(Varchar = 255, AllowDbNull = false)]
		public string QuestName
		{
			get { return m_questName; }
			set { m_questName = value; Dirty = true; }
		}		

		/// <summary>
		/// The name of the object that starts this quest
		/// </summary>
		[DataElement(Varchar = 100, AllowDbNull = false)]
		public string StartNPC
		{
			get { return m_startNPC; }
			set { m_startNPC = value; Dirty = true; }
		}

		/// <summary>
		/// The region id where this quest starts
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public ushort StartRegionID
		{
			get { return m_startRegionID; }
			set { m_startRegionID = value; Dirty = true; }
		}

		/// <summary>
		/// The quest story shown to player upon being offered the quest
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string StoryText
		{
			get { return m_storyText; }
			set { m_storyText = value; Dirty = true; }
		}

		/// <summary>
		/// Summary of the quest shown in journal
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string Summary
		{
			get { return m_summary; }
			set { m_summary = value; Dirty = true; }
		}

        /// <summary>
        /// Additional text displayed to the player upon accepting the quest
        /// </summary>
        [DataElement(AllowDbNull = true)]
        public string AcceptText
        {
            get { return m_acceptText; }
            set { m_acceptText = value; Dirty = true; }
        }

        /// <summary>
        /// Goal description and what step it is given. 
        /// Format: kill two bandits;1
        /// </summary>
        [DataElement(AllowDbNull = true)]
		public string QuestGoals
		{
			get { return m_questGoals; }
			set { m_questGoals = value; Dirty = true; }
		}
		
		/// <summary>
		/// Type of each step (kill, give, collect, etc)		
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string GoalType
		{
			get { return m_goalType; }
			set { m_goalType = value; Dirty = true; }
		}
		
		/// <summary>
		/// how many times goal must be repeated to be achieved
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string GoalRepeatNo
		{
			get { return m_goalRepeatNo; }
			set { m_goalRepeatNo = value; Dirty = true; }
		}
		
		/// <summary>
		/// Name of the target for each step		
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string GoalTargetName
		{
			get { return m_goaltargetName; }
			set { m_goaltargetName = value; Dirty = true; }
		}

		/// <summary>
		/// Text a target will say to player 		
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string GoalTargetText
		{
			get { return m_goaltargetText; }
			set { m_goaltargetText = value; Dirty = true; }
		}
		
		/// <summary>
		/// How many steps in this quest? used to display goals at certain stages of quest
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int StepCount
		{
			get { return m_stepCount; }
			set { m_stepCount = value; Dirty = true; }
		}
		
		/// <summary>
		/// The NPC name who finishes the quest
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string FinishNPC
		{
			get { return m_finishNPC; }
			set { m_finishNPC = value; Dirty = true; }
		}
		
		/// <summary>
		/// Text required to advance to the next step
		/// Format: Step 1 Text|Step 2 Text
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string AdvanceText
		{
			get { return m_goaladvanceText; }
			set { m_goaladvanceText = value; Dirty = true; }
		}		

		/// <summary>
		/// ItemTemplate id_nb to be collected to finish the current step
		/// Format: id_nb|id_nb||  steps with no item to collect should be blank
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string CollectItemTemplate
		{
			get { return m_collectItemTemplate; }
			set { m_collectItemTemplate = value; Dirty = true; }
		}

		/// <summary>
		/// Max number of times a player can do this quest
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public ushort MaxCount
		{
			get { return m_maxCount; }
			set { m_maxCount = value; Dirty = true; }
		}

		/// <summary>
		/// Minimum level a player has to be to start this quest
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public byte MinLevel
		{
			get { return m_minLevel; }
			set { m_minLevel = value; Dirty = true; }
		}

		/// <summary>
		/// Max level a player can be and still do this quest
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public byte MaxLevel
		{
			get { return m_maxLevel; }
			set { m_maxLevel = value; Dirty = true; }
		}

		/// <summary>
		/// Reward Money to give at quest completion, 0 for none		
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public long RewardMoney
		{
			get { return m_rewardMoney; }
			set { m_rewardMoney = value; Dirty = true; }
		}

        /// <summary>
        /// Reward XP to give at quest completion, 0 for none        
        /// </summary>
        [DataElement(AllowDbNull = true)]
		public long RewardXP
		{
			get { return m_rewardXP; }
			set { m_rewardXP = value; Dirty = true; }
		}

        /// <summary>
        /// Reward CLXP to give at quest completion, 0 for none        
        /// </summary>
        [DataElement(AllowDbNull = true)]
		public long RewardCLXP
		{
			get { return m_rewardCLXP; }
			set { m_rewardCLXP = value; Dirty = true; }
		}

        /// <summary>
        /// Reward RP to give at quest completion, 0 for none        
        /// </summary>
        [DataElement(AllowDbNull = true)]
		public long RewardRP
		{
			get { return m_rewardRP; }
			set { m_rewardRP = value; Dirty = true; }
		}

        /// <summary>
        /// Reward BP to give at quest completion, 0 for none        
        /// </summary>
        [DataElement(AllowDbNull = true)]
		public long RewardBP
		{
			get { return m_rewardBP; }
			set { m_rewardBP = value; Dirty = true; }
		}

		/// <summary>
		/// The ItemTemplate id_nb(s) to give as a optional rewards
		/// Format:  #id_nb1|id_nb2 with first character being the number of choices
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string OptionalRewardItemTemplates
		{
			get { return m_optionalRewardItemTemplates; }
			set { m_optionalRewardItemTemplates = value; Dirty = true; }
		}

		/// <summary>
		/// The ItemTemplate id_nb(s) to give as a final reward
		/// Format:  id_nb1|id_nb2
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string FinalRewardItemTemplates
		{
			get { return m_finalRewardItemTemplates; }
			set { m_finalRewardItemTemplates = value; Dirty = true; }
		}

		/// <summary>
		/// Text to show the user once the quest is finished.
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string FinishText
		{
			get { return m_finishText; }
			set { m_finishText = value; Dirty = true; }
		}

		/// <summary>
		/// The name or names of other quests that need to be done before this quest can be offered.
		/// Name Quest One|Name Quest Two... Can be null if no dependency
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string QuestDependency
		{
			get { return m_questDependency; }
			set { m_questDependency = value; Dirty = true; }
		}

		/// <summary>
		/// Player classes that can do this quest.  Null for all.
		/// </summary>
		[DataElement(AllowDbNull = true, Varchar = 200)]
		public string AllowedClasses
		{
			get { return m_allowedClasses; }
			set { m_allowedClasses = value; Dirty = true; }
		}

		/// <summary>
		/// Code that can be used for various quest activities		
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string ClassType
		{
			get { return m_classType; }
			set { m_classType = value; Dirty = true; }
		}
		
		//the following is used for adding the quest marker on the map // patch 0026
		/// <summary>
		/// Xloc of a goal target. Used to display red dot on map
		/// Value is from the /loc command
        /// Format: value1|value2|value3
        /// can be 0 for the interactFinish goal
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string XOffset
		{
			get { return m_xOffset; }
			set { m_xOffset = value; Dirty = true; }
		}
        /// <summary>
        /// Yloc of a goal target. Used to display red dot on map
        /// Value is from the /loc command
        /// Format: value1|value2|value3
        /// can be 0 for the interactFinish goal
        /// </summary>
        [DataElement(AllowDbNull = true)]
		public string YOffset
		{
			get { return m_yOffset; }
			set { m_yOffset = value; Dirty = true; }
		}
        /// <summary>
        /// ZoneID (not RegionID!) of a goal target. Used to display red dot on map        
        /// Format: value1|value2|value3
        /// can be 0 for the interactFinish goal
        /// </summary>
        [DataElement(AllowDbNull = true)]
		public string ZoneID
		{
			get { return m_zoneID; }
			set { m_zoneID = value; Dirty = true; }
		}		
	}
}
