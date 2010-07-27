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
using DOL.Database.Attributes;

namespace DOL.Database
{
	/// <summary>
	/// Holds all the DataQuests available
	/// </summary>
	[DataTable(TableName = "DataQuest")]
	public class DBDataQuest : DataObject
	{
		private int m_id;
		private string m_name;
		private byte m_startType;
		private string m_startName;
		private ushort m_startRegionID;
		private string m_acceptText;
		private string m_description;
		private string m_sourceName;
		private string m_sourceText;
		private string m_stepType;
		private string m_stepText;
		private string m_stepItemTemplates;
		private string m_advanceText;
		private string m_targetName;
		private string m_targetText;
		private string m_collectItemTemplate;
		private ushort m_maxCount;
		private byte m_minLevel;
		private byte m_maxLevel;
		private string m_rewardMoney;
		private string m_rewardXP;
		private string m_optionalRewardItemTemplates;
		private string m_finalRewardItemTemplates;


		public DBDataQuest()
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
		public string Name
		{
			get { return m_name; }
			set { m_name = value; Dirty = true; }
		}

		/// <summary>
		/// The start type of this quest (eStartType)
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public byte StartType
		{
			get { return m_startType; }
			set { m_startType = value; Dirty = true; }
		}

		/// <summary>
		/// The name of the object that starts this quest
		/// </summary>
		[DataElement(Varchar = 100, AllowDbNull = false)]
		public string StartName
		{
			get { return m_startName; }
			set { m_startName = value; Dirty = true; }
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
		/// The whisper text that will start this quest
		/// </summary>
		[DataElement(Varchar = 100, AllowDbNull = true)]
		public string AcceptText
		{
			get { return m_acceptText; }
			set { m_acceptText = value; Dirty = true; }
		}

		/// <summary>
		/// Description to show to start quest
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public string Description
		{
			get { return m_description; }
			set { m_description = value; Dirty = true; }
		}

		/// <summary>
		/// Who to talk to for each step
		/// Format: SourceName;RegionID|SourceName;RegionID
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public string SourceName
		{
			get { return m_sourceName; }
			set { m_sourceName = value; Dirty = true; }
		}

		/// <summary>
		/// The text for each source
		/// Format:  Step 1 Source text|Step 2 Source text
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public string SourceText
		{
			get { return m_sourceText; }
			set { m_sourceText = value; Dirty = true; }
		}

		/// <summary>
		/// Type of each step (kill, give, collect, etc)
		/// Format: Step1Type|Step2Type
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string StepType
		{
			get { return m_stepType; }
			set { m_stepType = value; Dirty = true; }
		}

		/// <summary>
		/// Description text for each step
		/// Format: Step 1 Text|Step 2 Text
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string StepText
		{
			get { return m_stepText; }
			set { m_stepText = value; Dirty = true; }
		}

		/// <summary>
		/// Items given to the player at a step
		/// Format: id_nb|idnb
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string StepItemTemplates
		{
			get { return m_stepItemTemplates; }
			set { m_stepItemTemplates = value; Dirty = true; }
		}

		/// <summary>
		/// Text required to advance to the next step
		/// Format: Step 1 Text|Step 2 Text
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string AdvanceText
		{
			get { return m_advanceText; }
			set { m_advanceText = value; Dirty = true; }
		}

		/// <summary>
		/// Name of the target for each step
		/// Format: TargetName;RegionID|TargetName;RegionID
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string TargetName
		{
			get { return m_targetName; }
			set { m_targetName = value; Dirty = true; }
		}

		/// <summary>
		/// Text for each target
		/// Format:  Step 1 Target text|Step 2 Target text| ...
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string TargetText
		{
			get { return m_targetText; }
			set { m_targetText = value; Dirty = true; }
		}

		/// <summary>
		/// ItemTemplate id_nb to be collected on each step
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
		/// Reward Money to give at each step, 0 for none
		/// Format: 111|222|0|333
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public string RewardMoney
		{
			get { return m_rewardMoney; }
			set { m_rewardMoney = value; Dirty = true; }
		}

		/// <summary>
		/// Reward XP to give at each step, 0 for none
		/// Format: 123456789|99876543|0|10000000
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public string RewardXP
		{
			get { return m_rewardXP; }
			set { m_rewardXP = value; Dirty = true; }
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


	}
}