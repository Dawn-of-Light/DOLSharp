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

// Tolakram, July 2010 - This represents a data driven quest that can be added and removed at runtime.  

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;
using DOL.Language;
using DOL.GS.Behaviour;
using DOL.GS.PacketHandler;
using log4net;


namespace DOL.GS.Quests
{

	/// <summary>
	/// This represents a data driven quest
	/// DataQuests are defined in the database instead of a script.
	/// 
	/// Each Quest should have a complete set of startup parameters.  All of these are non serialized.
	/// 
	/// Name, StartType, StartName, StartRegionID, AcceptText and Description.  These determine who offers the quest and what text is displayed
	/// to the player considering accepting the quest.
	/// 
	/// Once a quest is started each step behaves in a set order.  
	/// 
	/// Source -> Target -> Advance to Next Step 
	/// 
	/// Step 1 is considered the first step, and for each step
	/// Source is considered who started the Step and Target is considered who ends the Step.   For each Step the following columns must
	/// have serialized values, separated by | for each Step, including Step 1.  If the last value for any step is empty then the string should
	/// end with a double pipe |
	/// 
	/// SourceName;RegionID - both the SourceName and TargetName fields must be in the format Name;RegionID|Name;RegionID...
	/// 
	/// SourceText - what the source says to you when beginning a step.  If a source starting the next step has no text then an empty 
	/// string can be provided using || with nothing between the pipes.
	/// 
	/// StepType - The type of step from eStepType
	/// 
	/// StepText - The text for the step that appears in the players quest journal
	/// 
	/// StepItemTemplates - Any items that need to be given to the player at the start of the step.  If no items are given to a player
	/// at any of the steps then this can be null, otherwise it must have values for each step.  Empty values || are ok.
	/// 
	/// AdvanceText - The text needed, if any, to advance this step.  If no step requires advance text then this can be null, otherwise
	/// text must be provided for every step.  Empty values || are ok.
	/// 
	/// TargetName;RegionID - in addition the name can be a location (X,Y,Z,Radius) if used by a step that requires a location.  For example:
	/// 10000,10000,5000,300;1 would be X = 10000, Y = 10000, Z = 5000, Radius = 300 in Region 1
	/// 
	/// TargetText - Text shown to player when current step ends.
	/// 
	/// CollectItemTemplate - Item that needs to be collected to end the current step.  If no items are ever collected this can be kept null,
	/// otherwise it needs an entry for each step.  Empty values || are ok.
	/// 
	/// MaxCount, MinLevel, MaxLevel - Single values to determine who can do quest.  All must be provided.
	/// 
	/// RewardMoney - Serialized list of money rewarded for each step.  All steps must have a value, 0 is ok.
	/// 
	/// RewardXP - Serialized list of XP rewarded each step.  All steps must have a value, 0 is ok.
	/// 
	/// OptionalRewardItemTemplates - A serialized list of optional rewards to be presented to the player at the end of a Reward quest.
	/// The first value must be a number from 0 to 8 followed by the item list.  ex: 2id_nb|id_nb  For quests without optional rewards
	/// this field can be null.
	/// 
	/// FinalRewardItemTemplates - A serialized list of rewards to be given to the player at the end of a quest.  For quests without
	/// rewards this field can be null.
	/// 
	/// FinishText - The text to show the player once the quest has completed.
	/// 
	/// QuestDependency - If this quest is dependent on other quests being done first then the name(s) of those quests should be here.
	/// This can be null if no dependencies.
	/// 
	/// AllowedClasses - Player classes that can get this quest
	/// 
	/// ClassType - Any class of type IDataQuestStep which is called on each quest step and when the quest is finished.
	/// 
	/// </summary>
	public class DataQuest : AbstractQuest
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected int m_step = 1;
		protected DBDataQuest m_dataQuest = null;
		protected CharacterXDataQuest m_charQuest = null;
		protected GameObject m_startObject = null;
		protected GameNPC m_startNPC = null;
		protected IDataQuestStep m_customQuestStep = null;

		/// <summary>
		/// In order to avoid conflicts with scripted quests data quest ID's are added to this number when sending a quest ID to the client
		/// </summary>
		public const ushort DATAQUEST_CLIENTOFFSET = 32767;


		/// <summary>
		/// How does this quest start
		/// </summary>
		public enum eStartType : byte
		{
			Standard = 0,			// Talk to npc, accept quest, go through steps
			Collection = 1,			// Player turns drops into npc for xp, quest not added to player quest log, has no steps
			AutoStart = 2,			// Standard quest is auto started simply by interacting with start object
			KillComplete = 3,		// Killing the Start living grants and finished the quest, similar to One Time Drops
			InteractComplete = 4,	// Interacting with start object grants and finishes the quest
			RewardQuest = 200,		// A reward quest, where reward dialog is given to player on quest offer and complete.  
			Unknown = 255
		}

		/// <summary>
		/// The type of each quest step
		/// All quests with steps must end in a Finish step
		/// </summary>
		public enum eStepType : byte
		{
			Kill = 0,				// Kill the target to advance the quest
			KillFinish = 1,			// Killing the target finishes the quest and gives the reward
			Deliver = 2,			// Deliver an item to the target to advance the quest
			DeliverFinish = 3,		// Deliver an item to the target to finish the quest
			Interact = 4,			// Interact with the target to advance the step
			InteractFinish = 5,		// Interact with the target to finish the quest.  This is required to end a RewardQuest
			Whisper = 6,			// Whisper to the target to advance the quest
			WhisperFinish = 7,		// Whisper to the target to finish the quest
			Search = 8,				// Search in a specified location
			SearchFinish = 9,		// Search in a specified location to finish the quest
			Unknown = 255
		}

		protected List<ushort> m_sourceRegions = new List<ushort>();
		protected List<string> m_sourceNames = new List<string>();
		protected List<string> m_sourceTexts = new List<string>();
		protected List<ushort> m_targetRegions = new List<ushort>();
		protected List<string> m_targetNames = new List<string>();
		protected List<string> m_targetTexts = new List<string>();
		protected List<eStepType> m_stepTypes = new List<eStepType>();
		protected List<string> m_stepTexts = new List<string>();
		protected List<string> m_stepItemTemplates = new List<string>();
		protected List<string> m_advanceTexts = new List<string>();
		protected List<string> m_collectItems = new List<string>();
		protected List<long> m_rewardXPs = new List<long>();
		protected List<long> m_rewardMoneys = new List<long>();
		byte m_numOptionalRewardsChoice = 1;
		protected List<ItemTemplate> m_optionalRewards = new List<ItemTemplate>();
		protected List<ItemTemplate> m_optionalRewardChoice = new List<ItemTemplate>();
		protected List<ItemTemplate> m_finalRewards = new List<ItemTemplate>();
		protected List<string> m_questDependencies = new List<string>();
		protected List<byte> m_allowedClasses = new List<byte>();
		string classType = "";

		#region Construction

		/// <summary>
		/// Create an empty Quest
		/// </summary>
		public DataQuest()
			: base()
		{
		}

		/// <summary>
		/// This version of a DataQuest is used to give the quest to a player
		/// </summary>
		/// <param name="dbQuest"></param>
		public DataQuest(DBDataQuest dataQuest)
		{
			m_questPlayer = null;
			m_step = 1;
			m_dataQuest = dataQuest;
			ParseQuestData();
		}

		/// <summary>
		/// Dataquest that belongs to a player
		/// </summary>
		/// <param name="questingPlayer"></param>
		/// <param name="dataQuest"></param>
		/// <param name="charQuest"></param>
		public DataQuest(GamePlayer questingPlayer, DBDataQuest dataQuest, CharacterXDataQuest charQuest)
			: this(questingPlayer, null, dataQuest, charQuest)
		{
		}

		/// <summary>
		/// This is a dataquest that belongs to a player
		/// </summary>
		/// <param name="questingPlayer"></param>
		/// <param name="dbQuest"></param>
		/// <param name="charQuest"></param>
		public DataQuest(GamePlayer questingPlayer, GameObject sourceObject, DBDataQuest dataQuest, CharacterXDataQuest charQuest)
		{
			m_questPlayer = questingPlayer;
			m_step = 1;
			m_dataQuest = dataQuest;
			m_charQuest = charQuest;

			if (sourceObject != null)
			{
				if (sourceObject is GameNPC)
				{
					m_startNPC = sourceObject as GameNPC;
				}

				m_startObject = sourceObject;
			}

			ParseQuestData();
		}

		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(RewardQuestNotify));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(RewardQuestNotify));
		}


		#endregion Construction

		#region Parse Quest Data

		/// <summary>
		/// Split the quest strings into individual step data
		/// It's important to remember that there must be an entry, even if empty, for each column for each step.
		/// For example; something|||something for a 4 part quest
		/// </summary>
		protected void ParseQuestData()
		{
			if (m_dataQuest == null)
				return;

			string lastParse = "";

			try
			{
				string[] parse1;

				lastParse = m_dataQuest.SourceName;
				if (string.IsNullOrEmpty(lastParse) == false)
				{
					parse1 = lastParse.Split('|');
					foreach (string str in parse1)
					{
						if (str == string.Empty)
						{
							// if there's not npc for this step then empty is ok
							m_sourceNames.Add("");
							m_sourceRegions.Add(0);
						}
						else
						{
							string[] parse2 = str.Split(';');
							m_sourceNames.Add(parse2[0]);
							m_sourceRegions.Add(Convert.ToUInt16(parse2[1]));
						}
					}
				}

				lastParse = m_dataQuest.SourceText;
				if (string.IsNullOrEmpty(lastParse) == false)
				{
					parse1 = lastParse.Split('|');
					foreach (string str in parse1)
					{
						m_sourceTexts.Add(str);
					}
				}

				lastParse = m_dataQuest.TargetName;
				if (string.IsNullOrEmpty(lastParse) == false)
				{
					parse1 = lastParse.Split('|');
					foreach (string str in parse1)
					{
						if (str == string.Empty)
						{
							// if there's not npc for this step then empty is ok
							m_targetNames.Add("");
							m_targetRegions.Add(0);
						}
						else
						{
							string[] parse2 = str.Split(';');
							m_targetNames.Add(parse2[0]);
							m_targetRegions.Add(Convert.ToUInt16(parse2[1]));
						}
					}
				}

				lastParse = m_dataQuest.TargetText;
				if (string.IsNullOrEmpty(lastParse) == false)
				{
					parse1 = lastParse.Split('|');
					foreach (string str in parse1)
					{
						m_targetTexts.Add(str);
					}
				}


				lastParse = m_dataQuest.StepType;
				if (string.IsNullOrEmpty(lastParse) == false)
				{
					parse1 = lastParse.Split('|');
					foreach (string str in parse1)
					{
						m_stepTypes.Add((eStepType)Convert.ToByte(str));
					}
				}

				lastParse = m_dataQuest.StepText;
				if (string.IsNullOrEmpty(lastParse) == false)
				{
					parse1 = lastParse.Split('|');
					foreach (string str in parse1)
					{
						m_stepTexts.Add(str);
					}
				}

				lastParse = m_dataQuest.StepItemTemplates;
				if (string.IsNullOrEmpty(lastParse) == false)
				{
					parse1 = lastParse.Split('|');
					foreach (string str in parse1)
					{
						m_stepItemTemplates.Add(str);
					}
				}

				lastParse = m_dataQuest.AdvanceText;
				if (string.IsNullOrEmpty(lastParse) == false)
				{
					parse1 = lastParse.Split('|');
					foreach (string str in parse1)
					{
						m_advanceTexts.Add(str);
					}
				}

				lastParse = m_dataQuest.CollectItemTemplate;
				if (string.IsNullOrEmpty(lastParse) == false)
				{
					parse1 = lastParse.Split('|');
					foreach (string str in parse1)
					{
						m_collectItems.Add(str);
					}
				}

				lastParse = m_dataQuest.RewardMoney;
				if (string.IsNullOrEmpty(lastParse) == false)
				{
					parse1 = lastParse.Split('|');
					foreach (string str in parse1)
					{
						m_rewardMoneys.Add(Convert.ToInt64(str));
					}
				}

				lastParse = m_dataQuest.RewardXP;
				if (string.IsNullOrEmpty(lastParse) == false)
				{
					parse1 = lastParse.Split('|');
					foreach (string str in parse1)
					{
						m_rewardXPs.Add(Convert.ToInt64(str));
					}
				}

				lastParse = m_dataQuest.OptionalRewardItemTemplates;
				if (string.IsNullOrEmpty(lastParse) == false)
				{
					m_numOptionalRewardsChoice = Convert.ToByte(lastParse.Substring(0, 1));
					parse1 = lastParse.Substring(1).Split('|');
					foreach (string str in parse1)
					{
						ItemTemplate item = GameServer.Database.FindObjectByKey<ItemTemplate>(str);
						if (item != null)
						{
							m_optionalRewards.Add(item);
						}
						else
						{
							log.ErrorFormat("DataQuest: Optional reward ItemTemplate not found: {0}", str);
						}
					}
				}

				lastParse = m_dataQuest.FinalRewardItemTemplates;
				if (string.IsNullOrEmpty(lastParse) == false)
				{
					parse1 = lastParse.Split('|');
					foreach (string str in parse1)
					{
						ItemTemplate item = GameServer.Database.FindObjectByKey<ItemTemplate>(str);
						if (item != null)
						{
							m_finalRewards.Add(item);
						}
						else
						{
							log.ErrorFormat("DataQuest: Final reward ItemTemplate not found: {0}", str);
						}
					}
				}

				lastParse = m_dataQuest.QuestDependency;
				if (string.IsNullOrEmpty(lastParse) == false)
				{
					parse1 = lastParse.Split('|');
					foreach (string str in parse1)
					{
						if (str != "")
						{
							m_questDependencies.Add(str);
						}
					}
				}

				lastParse = m_dataQuest.AllowedClasses;
				if (string.IsNullOrEmpty(lastParse) == false)
				{
					parse1 = lastParse.Split('|');
					foreach (string str in parse1)
					{
						m_allowedClasses.Add(Convert.ToByte(str));
					}
				}

				lastParse = m_dataQuest.ClassType;
				if (string.IsNullOrEmpty(lastParse) == false)
				{
					classType = lastParse;
				}
			}

			catch (Exception ex)
			{
				log.Error("Error parsing quest data for " + m_dataQuest.Name + " (" + m_dataQuest.ID + "), last string to parse = '" + lastParse + "'.", ex);
			}
		}

		#endregion Parse Quest Data

		#region Properties

		/// <summary>
		/// Name of this quest to show in quest log
		/// </summary>
		public override string Name
		{
			get
			{
				return m_dataQuest.Name;
			}
		}


		/// <summary>
		/// How does this quest start?
		/// </summary>
		public virtual eStartType StartType
		{
			get { return (eStartType)m_dataQuest.StartType; }
		}

		/// <summary>
		/// What object started this quest
		/// </summary>
		public virtual GameObject StartObject
		{
			get { return m_startObject; }
			set { m_startObject = value; }
		}

		/// <summary>
		/// List of final rewards for this quest
		/// </summary>
		public virtual List<ItemTemplate> FinalRewards
		{
			get { return m_finalRewards; }
		}

		/// <summary>
		/// How many optional items can the player choose
		/// </summary>
		public virtual byte NumOptionalRewardsChoice
		{
			get { return m_numOptionalRewardsChoice; }
			set { m_numOptionalRewardsChoice = value; }
		}

		/// <summary>
		/// List of optional rewards for this quest
		/// </summary>
		public virtual List<ItemTemplate> OptionalRewards
		{
			get { return m_optionalRewards; }
		}

		public virtual List<ItemTemplate> OptionalRewardsChoice
		{
			get { return m_optionalRewardChoice; }
		}

		/// <summary>
		/// Final text to display to player when quest is finished
		/// </summary>
		public virtual string FinishText
		{
			get { return m_dataQuest.FinishText; }
		}

		/// <summary>
		/// The DBDataQuest for this quest
		/// </summary>
		public virtual DBDataQuest DBDataQuest
		{
			get { return m_dataQuest; }
		}


		/// <summary>
		/// The CharacterXDataQuest entry for the player doing this quest
		/// </summary>
		public virtual CharacterXDataQuest CharDataQuest
		{
			get { return m_charQuest; }
		}

		/// <summary>
		/// The unique ID for this quest
		/// </summary>
		public virtual int ID
		{
			get { return m_dataQuest.ID; }
		}

		/// <summary>
		/// Unique quest ID to send to the client
		/// </summary>
		public virtual ushort ClientQuestID
		{
			get { return (ushort)(m_dataQuest.ID + DATAQUEST_CLIENTOFFSET); }
		}


		/// <summary>
		/// Minimum level this quest can be done
		/// </summary>
		public override int Level
		{
			get	{ return m_dataQuest.MinLevel; }
		}


		/// <summary>
		/// Max level that this quest can be done
		/// </summary>
		public virtual int MaxLevel
		{
			get { return m_dataQuest.MaxLevel; }
		}

		/// <summary>
		/// Text of every step in this quest
		/// </summary>
		public virtual List<string> StepTexts
		{
			get { return m_stepTexts; }
		}


		public virtual short Count
		{
			get 
			{
				if (m_charQuest != null)
				{
					return m_charQuest.Count;
				}

				try
				{
					log.ErrorFormat("m_charQuest null for quest {0}:{1} on character {2}", ID, Name, QuestPlayer == null ? "null" : QuestPlayer.Name);
				}
				catch
				{
					log.ErrorFormat("Error retrieving count for DataQuest {0}", ID);
				}

				return 0; 
			}
			set
			{
				short oldCount = m_charQuest.Count;
				m_charQuest.Count = value;
				if (m_charQuest.Count != oldCount)
				{
					GameServer.Database.SaveObject(m_charQuest);
				}
			}
		}
		
		/// <summary>
		/// Maximum number of times this quest can be done
		/// </summary>
		public override int MaxQuestCount
		{
			get
			{
				return m_dataQuest.MaxCount;
			}
		}

		/// <summary>
		/// Description of this quest to show in quest log
		/// </summary>
		public override string Description
		{
			get
			{
				if (Step == 0)
				{
					if (m_dataQuest.Description == null)
					{
						return "";
					}
					else
					{
						return m_dataQuest.Description;
					}
				}
				else
				{
					return StepText;
				}
			}
		}

		/// <summary>
		/// The Story to display if this is a Reward Quest
		/// </summary>
		public virtual string Story
		{
			get
			{
				if (m_sourceTexts.Count > 0)
				{
					return m_sourceTexts[0];
				}
				else
				{
					return "SourceTexts[0] undefined!";
				}
			}
		}

		/// <summary>
		/// What quest step is the player on
		/// Generic Quests only support a single step
		/// </summary>
		public override int Step
		{
			get
			{
				if (m_charQuest == null)
				{
					return 0;
				}

				return m_charQuest.Step;
			}
			set
			{
				if (m_charQuest != null)
				{
					int oldStep = m_charQuest.Step;
					m_charQuest.Step = (short)value;
					if (m_charQuest.Step != oldStep)
					{
						GameServer.Database.SaveObject(m_charQuest);
					}
				}
			}
		}

		#endregion Properties

		#region Utility

		/// <summary>
		/// Get or create the CharacterXDataQuest for this player
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		protected virtual CharacterXDataQuest GetCharacterQuest(GamePlayer player, bool create)
		{
			CharacterXDataQuest charQuest = GameServer.Database.SelectObject<CharacterXDataQuest>("Character_ID ='" + GameServer.Database.Escape(player.InternalID) + "' AND DataQuestID = " + ID);

			if (charQuest == null && create)
			{
				charQuest = new CharacterXDataQuest(player.InternalID, ID);
				charQuest.Count = 0;
				charQuest.Step = 0;
				GameServer.Database.AddObject(charQuest);
			}

			return charQuest;
		}

		/// <summary>
		/// Static version used for reward quest accepts
		/// </summary>
		/// <param name="id"></param>
		/// <param name="player"></param>
		/// <param name="create"></param>
		/// <returns></returns>
		protected static CharacterXDataQuest GetCharacterQuest(int id, GamePlayer player, bool create)
		{
			CharacterXDataQuest charQuest = GameServer.Database.SelectObject<CharacterXDataQuest>("Character_ID ='" + GameServer.Database.Escape(player.InternalID) + "' AND DataQuestID = " + id);

			if (charQuest == null && create)
			{
				charQuest = new CharacterXDataQuest(player.InternalID, id);
				charQuest.Count = 0;
				charQuest.Step = 0;
				GameServer.Database.AddObject(charQuest);
			}

			return charQuest;
		}

		/// <summary>
		/// Can this player do this quest
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override bool CheckQuestQualification(GamePlayer player)
		{
			if (player.Level < DBDataQuest.MinLevel || player.Level > DBDataQuest.MaxLevel)
				return false;

			if (m_allowedClasses.Count > 0)
			{
				if (m_allowedClasses.Contains((byte)player.CharacterClass.ID) == false)
				{
					return false;
				}
			}

			if (ExecuteCustomQuestStep(player, 0, eStepCheckType.Qualification) == false)
				return false;


			if (StartType == eStartType.Collection)
			{
				CharacterXDataQuest charQuest = GetCharacterQuest(player, false);
				if (charQuest != null && charQuest.Count >= MaxQuestCount)
				{
					return false;
				}

				return true;
			}

			lock (player.QuestList)
			{
				foreach (AbstractQuest q in player.QuestList)
				{
					if (q is DataQuest && (q as DataQuest).ID == ID)
					{
						return false;  // player is currently doing this quest
					}
				}
			}

			lock (player.QuestListFinished)
			{
				foreach (AbstractQuest q in player.QuestListFinished)
				{
					if (q is DataQuest && (q as DataQuest).ID == ID)
					{
						if (q.IsDoingQuest(q) == true || (q as DataQuest).Count >= MaxQuestCount)
						{
							return false; // player has done this quest the max number of times
						}
					}
				}

				// check to see if this quest requires another to be done first
				if (m_questDependencies.Count > 0)
				{
					int numFound = 0;

					foreach (string str in m_questDependencies)
					{
						foreach (AbstractQuest q in player.QuestListFinished)
						{
							if (q is DataQuest && (q as DataQuest).Name.ToLower() == str.ToLower())
							{
								numFound++;
								break;
							}
						}
					}

					if (numFound < m_questDependencies.Count)
					{
						return false;
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Is the player currently doing this quest
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public override bool IsDoingQuest(AbstractQuest checkQuest)
		{
			if (checkQuest is DataQuest && (checkQuest as DataQuest).ID == ID)
			{
				return Step > 0;
			}

			return false;
		}


		/// <summary>
		/// Update the quest indicator
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="player"></param>
		public virtual void UpdateQuestIndicator(GameNPC npc, GamePlayer player)
		{
			player.Out.SendNPCsQuestEffect(npc, npc.ShowQuestIndicator(player));
		}



		/// <summary>
		/// Source name for the current step
		/// </summary>
		protected string SourceName
		{
			get
			{
				try
				{
					return m_sourceNames[Step - 1];
				}
				catch (Exception ex)
				{
					log.Error("DataQuest [" + ID + "] SourceName error for Step " + Step, ex);
				}

				return "";
			}
		}

		/// <summary>
		/// Source region for the current step
		/// </summary>
		protected ushort SourceRegion
		{
			get
			{
				try
				{
					return m_sourceRegions[Step - 1];
				}
				catch (Exception ex)
				{
					log.Error("DataQuest [" + ID + "] SourceRegion error for Step " + Step, ex);
				}

				return 0;
			}
		}

		/// <summary>
		/// Source text for the current step
		/// </summary>
		protected string SourceText
		{
			get
			{
				try
				{
					return m_sourceTexts[Step - 1];
				}
				catch (Exception ex)
				{
					log.Error("DataQuest [" + ID + "] SourceText error for Step " + Step, ex);
				}

				return "Error retrieving source text for step " + Step;
			}
		}

		/// <summary>
		/// Target name for the current step
		/// </summary>
		protected string TargetName
		{
			get
			{
				try
				{
					return m_targetNames[Step - 1];
				}
				catch (Exception ex)
				{
					log.Error("DataQuest [" + ID + "] TargetName error for Step " + Step, ex);
				}

				return "";
			}
		}

		/// <summary>
		/// Target region for the current step
		/// </summary>
		protected ushort TargetRegion
		{
			get
			{
				try
				{
					return m_targetRegions[Step - 1];
				}
				catch (Exception ex)
				{
					log.Error("DataQuest [" + ID + "] TargetRegion error for Step " + Step, ex);
				}

				return 0;
			}
		}

		/// <summary>
		/// Target text for the current step
		/// </summary>
		protected string TargetText
		{
			get
			{
				try
				{
					if (m_targetTexts.Count > 0)
					{
						return m_targetTexts[Step - 1];
					}
					else
					{
						return string.Empty;
					}
				}
				catch (Exception ex)
				{
					log.Error("DataQuest [" + ID + "] TargetText error for Step " + Step, ex);
				}

				return "Error retrieving target text for step " + Step;
			}
		}

		/// <summary>
		/// Current step type
		/// </summary>
		protected eStepType StepType
		{
			get
			{
				try
				{
					return m_stepTypes[Step - 1];
				}
				catch (Exception ex)
				{
					log.Error("DataQuest [" + ID + "] StepType error for Step " + Step, ex);
				}

				return eStepType.Unknown;
			}
		}

		/// <summary>
		/// Step description to show in quest log for the current step
		/// </summary>
		protected string StepText
		{
			get
			{
				try
				{
					return m_stepTexts[Step - 1];
				}
				catch (Exception ex)
				{
					log.Error("DataQuest [" + ID + "] StepText error for Step " + Step, ex);
				}

				return "Error retrieving step text for step " + Step;
			}
		}

		/// <summary>
		/// An item template to give to the player for this step
		/// </summary>
		protected string StepItemTemplate
		{
			get
			{
				try
				{
					return m_stepItemTemplates[Step - 1];
				}
				catch (Exception ex)
				{
					log.Error("DataQuest [" + ID + "] StepItemTemplate error for Step " + Step, ex);
				}

				return "";
			}
		}


		/// <summary>
		/// Text needed to advance the step or end the quest for the current step
		/// </summary>
		protected string AdvanceText
		{
			get
			{
				try
				{
					return m_advanceTexts[Step - 1];
				}
				catch (Exception ex)
				{
					log.Error("DataQuest [" + ID + "] AdvanceText error for Step " + Step, ex);
				}

				return "";
			}
		}


		/// <summary>
		/// Any money reward for the current step
		/// </summary>
		protected long RewardMoney
		{
			get
			{
				try
				{
					if (m_rewardMoneys.Count == 0)
					{
						return 0;
					}

					return m_rewardMoneys[Step - 1];
				}
				catch (Exception ex)
				{
					log.Error("DataQuest [" + ID + "] RewardMoney error for Step " + Step, ex);
				}

				return 0;
			}
		}


		/// <summary>
		/// Any xp reward for the current step
		/// </summary>
		protected long RewardXP
		{
			get
			{
				try
				{
					if (m_rewardXPs.Count == 0)
					{
						return 0;
					}

					return m_rewardXPs[Step - 1];
				}
				catch (Exception ex)
				{
					log.Error("DataQuest [" + ID + "] RewardXP error for Step " + Step, ex);
				}

				return 0;
			}
		}

		protected virtual bool ExecuteCustomQuestStep(GamePlayer player, int step, eStepCheckType stepCheckType)
		{
			bool canContinue = true;

			if (string.IsNullOrEmpty(classType) == false)
			{
				if (m_customQuestStep == null)
				{
					foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
					{
						if (assembly.GetType(classType) != null)
						{
							try
							{
								m_customQuestStep = assembly.CreateInstance(classType, false, BindingFlags.CreateInstance, null, new object[] { }, null, null) as IDataQuestStep;
							}
							catch (Exception)
							{
							}

							break;
						}
					}
				}

				if (m_customQuestStep == null)
				{
					log.ErrorFormat("Failed to construct custom DataQuest step of ClassType {0}!  Quest will continue anyway.", classType);
				}
			}

			if (m_customQuestStep != null)
			{
				canContinue = m_customQuestStep.Execute(this, player, step, stepCheckType);
			}

			return canContinue;
		}


		/// <summary>
		/// Try to advance the quest step, doing any actions required to start the next step
		/// </summary>
		/// <param name="obj">The object that is advancing the step</param>
		/// <returns></returns>
		protected virtual bool AdvanceQuestStep(GameObject obj)
		{
			try
			{
				eStepType nextStepType = m_stepTypes[Step];
				bool advance = false;

				if (ExecuteCustomQuestStep(QuestPlayer, Step, eStepCheckType.Step))
				{
					advance = true;

					// If next step requires giving the player an item then we need to check to make sure
					// player has enough inventory space to accept the item, otherwise do not advance the step

					if (nextStepType == eStepType.Deliver || nextStepType == eStepType.DeliverFinish)
					{
						ItemTemplate item = GameServer.Database.FindObjectByKey<ItemTemplate>(m_stepItemTemplates[Step]);
						if (item == null)
						{
							throw new Exception("Can't find ItemTemplate " + m_stepItemTemplates[Step]);
						}

						if (obj != null && obj is GameLiving)
						{
							advance = GiveItem(obj as GameLiving, m_questPlayer, item, false);
						}
						else
						{
							advance = GiveItem(m_questPlayer, item, false);
						}
					}
				}

				if (advance)
				{
					// Since we can advance first give any rewards for the current step

					if (RewardMoney > 0)
					{
						m_questPlayer.AddMoney(RewardMoney, "You are awarded {0}!");
					}

					if (RewardXP > 0)
					{
						m_questPlayer.GainExperience(GameLiving.eXPSource.Quest, RewardXP);
					}

					// Then advance step

					Step++;
					m_questPlayer.Out.SendQuestListUpdate();

					// Then say any source text for the new step

					if (string.IsNullOrEmpty(SourceText) == false)
					{
						if (obj.Realm == eRealm.None)
						{
							SendMessage(m_questPlayer, SourceText, 0, eChatType.CT_Say, eChatLoc.CL_ChatWindow);
						}
						else
						{
							SendMessage(m_questPlayer, SourceText, 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
						}
					}

					return true;
				}
			}
			catch (Exception ex)
			{
				log.Error("DataQuest [" + ID + "] AdvanceQuestStep error when advancing from Step " + Step, ex);
			}

			return false;
		}


		#endregion Utility


		#region Notify

		/// <summary>
		/// Notify is sent to all quests in the players active quest list
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			// log.DebugFormat("DataQuest: Notify {0}, m_questPlayer {1}", e.Name, m_questPlayer == null ? "null" : m_questPlayer.Name);

			try
			{
				// Interact to check quest offer
				if (e == GameObjectEvent.Interact)
				{
					InteractEventArgs a = args as InteractEventArgs;
					GameObject o = sender as GameObject;
					GamePlayer p = a.Source as GamePlayer;

					if (p != null && o != null)
					{
						//log.DebugFormat("DataQuest CheckOffer: Player {0} is interacting with {1}", p.Name, o.Name);
						CheckOfferQuest(p, o);
					}

					return;
				}

				// Interact when already doing quest
				if (e == GamePlayerEvent.InteractWith)
				{
					GamePlayer p = sender as GamePlayer;
					InteractWithEventArgs a = args as InteractWithEventArgs;

					//log.DebugFormat("DataQuest Interact: Player {0} is interacting with {1}", p.Name, a.Target.Name);
					OnPlayerInteract(p, a.Target);

					return;
				}

				// Player is giving an item to something
				if (e == GamePlayerEvent.GiveItem)
				{
					GiveItemEventArgs a = args as GiveItemEventArgs;

					//log.DebugFormat("DataQuest: GiveItem {0} receives {1} from {2}", a.Target.Name, a.Item.Name, a.Source.Name);
					OnPlayerGiveItem(a.Source, a.Target, a.Item);

					return;
				}

				// Living is receiving an item, should a quest react to this
				if (e == GamePlayerEvent.ReceiveItem)
				{
					ReceiveItemEventArgs a = args as ReceiveItemEventArgs;
					GamePlayer p = a.Source as GamePlayer;

					if (p != null)
					{
						//log.DebugFormat("DataQuest: ReceiveItem {0} receives {1} from {2}", a.Target.Name, a.Item.Name, a.Source.Name);
						OnNPCReceiveItem(p, a.Target, a.Item);
					}

					return;
				}

				// Whisper 
				if (e == GamePlayerEvent.WhisperReceive)
				{
					WhisperReceiveEventArgs a = args as WhisperReceiveEventArgs;
					GamePlayer p = a.Source as GamePlayer;

					if (p != null)
					{
						//log.DebugFormat("DataQuest: WhisperReceived {0} receives whisper {1} from {2}", a.Target.Name, a.Text, a.Source.Name);
						OnNPCReceiveWhisper(p, a.Target, a.Text);
					}

					return;
				}

				if (e == GamePlayerEvent.Whisper)
				{
					WhisperEventArgs a = args as WhisperEventArgs;
					GamePlayer p = sender as GamePlayer;

					if (p != null)
					{
						OnPlayerWhisper(p, a.Target, a.Text);
					}
				}

				// NPC is dying, check for KillComplete quests
				if (e == GameLivingEvent.Dying)
				{
					DyingEventArgs a = args as DyingEventArgs;
					GameLiving dying = sender as GameLiving;
					GameObject killer = a.Killer;
					List<GamePlayer> playerKillers = a.PlayerKillers;

					OnLivingIsDying(dying, killer, playerKillers);

					return;
				}

				// Enemy of player with quest was killed, check quests and steps
				if (e == GamePlayerEvent.EnemyKilled)
				{
					EnemyKilledEventArgs a = args as EnemyKilledEventArgs;
					GamePlayer player = sender as GamePlayer;
					GameLiving killed = a.Target;

					OnEnemyKilled(player, killed);

					return;
				}

				// Player is trying to finish a Reward Quest
				if (e == GamePlayerEvent.QuestRewardChosen)
				{
					QuestRewardChosenEventArgs rewardArgs = args as QuestRewardChosenEventArgs;
					if (rewardArgs == null)
						return;

					// Check if this particular quest has been finished.

					if (ClientQuestID != rewardArgs.QuestID)
						return;

					m_optionalRewardChoice.Clear();

					for (int reward = 0; reward < rewardArgs.CountChosen; ++reward)
					{
						m_optionalRewardChoice.Add(OptionalRewards[rewardArgs.ItemsChosen[reward]]);
					}

					if (NumOptionalRewardsChoice > 0 && rewardArgs.CountChosen <= 0)
					{
						QuestPlayer.Out.SendMessage(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "RewardQuest.Notify"), eChatType.CT_System, eChatLoc.CL_ChatWindow);
						return;
					}

					FinishQuest(null, false);
					return;
				}


			}
			catch (Exception ex)
			{
				log.Error("DataQuest Notify Error", ex);
			}
		}

		public static void RewardQuestNotify(DOLEvent e, object sender, EventArgs args)
		{
			// Reward Quest accept
			if (e == GamePlayerEvent.AcceptQuest)
			{
				QuestEventArgs qargs = args as QuestEventArgs;
				if (qargs == null)
					return;

				GamePlayer player = qargs.Player;
				GameLiving giver = qargs.Source;

				foreach (DBDataQuest quest in GameObject.DataQuestCache)
				{
					if ((quest.ID + DATAQUEST_CLIENTOFFSET) == qargs.QuestID)
					{
						CharacterXDataQuest charQuest = GetCharacterQuest(quest.ID, player, true);
						DataQuest dq = new DataQuest(player, giver, quest, charQuest);
						dq.Step = 1;
						player.AddQuest(dq);
						if (giver is GameNPC)
						{
							player.Out.SendNPCsQuestEffect(giver as GameNPC, (giver as GameNPC).ShowQuestIndicator(player));
						}
						player.Out.SendSoundEffect(7, 0, 0, 0, 0, 0);
						break;
					}
				}

				return;
			}
		}

		#endregion Notify


		#region Notification Handlers

		/// <summary>
		/// A player has interacted with an object that has a DataQuest.
		/// Check to see if we can offer this quest to the player and display the text
		/// </summary>
		/// <param name="player"></param>
		/// <param name="obj"></param>
		protected virtual void CheckOfferQuest(GamePlayer player, GameObject obj)
		{
			// Can we offer this quest to the player?
			if (CheckQuestQualification(player))
			{
				if (StartType == eStartType.InteractComplete)
				{
					// This quest finishes with the interaction

					CharacterXDataQuest charQuest = GetCharacterQuest(player, true);

					if (charQuest.Count < MaxQuestCount)
					{
						if (ExecuteCustomQuestStep(player, 0, eStepCheckType.Finish))
						{
							if (Description.Trim() != "")
							{
								SendMessage(player, Description, 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
							}

							if (m_finalRewards.Count > 0)
							{
								lock (player.Inventory)
								{
									if (player.Inventory.IsSlotsFree(m_finalRewards.Count, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
									{
										foreach (ItemTemplate item in m_finalRewards)
										{
											if (item != null)
											{
												GiveItem((obj is GameLiving ? obj as GameLiving : null), player, item, false);
											}
										}
									}
									else
									{
										SendMessage(player, "Your inventory does not have enough space to finish this quest!", 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
										return;
									}
								}
							}

							if (m_rewardXPs.Count > 0 && m_rewardXPs[0] > 0)
							{
								player.GainExperience(GameLiving.eXPSource.Quest, m_rewardXPs[0]);
							}

							if (m_rewardMoneys.Count > 0 && m_rewardMoneys[0] > 0)
							{
								player.AddMoney(m_rewardMoneys[0], "You are awarded {0}!");
							}

							charQuest.Count++;
							GameServer.Database.SaveObject(charQuest);

							bool add = true;
							lock (player.QuestListFinished)
							{
								foreach (AbstractQuest q in player.QuestListFinished)
								{
									if (q is DataQuest && (q as DataQuest).ID == ID)
									{
										add = false;
										break;
									}
								}
							}

							if (add)
							{
								player.QuestListFinished.Add(this);
							}

							player.Out.SendQuestListUpdate();

							player.Out.SendMessage(String.Format(LanguageMgr.GetTranslation(player.Client, "AbstractQuest.FinishQuest.Completed", Name)), eChatType.CT_ScreenCenter, eChatLoc.CL_SystemWindow);
							player.Out.SendMessage(String.Format(LanguageMgr.GetTranslation(player.Client, "AbstractQuest.FinishQuest.Completed", Name)), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
						}
					}
					return;
				}

				if (StartType == eStartType.AutoStart)
				{
					CharacterXDataQuest charQuest = GetCharacterQuest(player, true);
					DataQuest dq = new DataQuest(player, obj, DBDataQuest, charQuest);
					dq.Step = 1;
					player.AddQuest(dq);
					if (m_sourceTexts[0] != "")
					{
						SendMessage(player, m_sourceTexts[0], 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
					}
					if (obj is GameNPC)
					{
						UpdateQuestIndicator(obj as GameNPC, player);
					}
					return;
				}

				// Send offer quest dialog
				if (StartType == eStartType.RewardQuest)
				{
					GameNPC offerNPC = obj as GameNPC;
					if (offerNPC != null)
					{
						// Note: If the offer is handled by the custom step then it should return false to prevent a double offer
						if (ExecuteCustomQuestStep(player, 0, eStepCheckType.Offer))
						{
							player.Out.SendQuestOfferWindow(offerNPC, player, this);
						}
					}
				}
				else
				{
					SendMessage(player, Description, 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
				}
			}
		}


		/// <summary>
		/// Check quests offered to see if receiving an item should be processed
		/// Used for Collection and Item Start quest types
		/// </summary>
		/// <param name="player"></param>
		/// <param name="obj"></param>
		/// <param name="item"></param>
		protected virtual void CheckOfferedQuestReceiveItem(GamePlayer player, GameObject obj, InventoryItem item)
		{
			// checking the quests we can offer to see if this is a collection quest or if the item starts a quest
			//log.DebugFormat("Checking collection quests: '{0}' of type '{1}', wants item '{2}'", Name, (eStartType)DBDataQuest.StartType, DBDataQuest.CollectItemTemplate == null ? "" : DBDataQuest.CollectItemTemplate);

			// check to see if this object has a collection quest and if so accept the item and generate the reward
			// collection quests do not go into the GamePlayer quest lists
			if (StartType == eStartType.Collection && item.Id_nb == DBDataQuest.CollectItemTemplate)
			{
				CharacterXDataQuest charQuest = GetCharacterQuest(player, true);

				if (charQuest.Count < MaxQuestCount)
				{
					if (item.Count == 1)
					{
						RemoveItem(obj, player, item, false);
						charQuest.Count++;
						charQuest.Step = 0;
						GameServer.Database.SaveObject(charQuest);
						long rewardXP = 0;
						if (long.TryParse(DBDataQuest.RewardXP, out rewardXP))
						{
							player.GainExperience(GameLiving.eXPSource.Quest, rewardXP);
						}
						SendMessage(player, m_sourceTexts[0], 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
					}
					else
					{
						SendMessage(player, "You need to unstack these first.", 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
					}
				}
			}
		}


		/// <summary>
		/// Check offered quests to see if whisper should be processed
		/// </summary>
		/// <param name="player"></param>
		/// <param name="living"></param>
		/// <param name="text"></param>
		protected virtual void CheckOfferedQuestWhisper(GamePlayer player, GameLiving living, string text)
		{
			//log.DebugFormat("Checking accept quest: '{0}' ID: {1} of type '{2}', key word '{3}', is qualified {4}", Name, ID, (eStartType)DBDataQuest.StartType, DBDataQuest.AcceptText, CheckQuestQualification(player));

			if (CheckQuestQualification(player) && DBDataQuest.StartType == (byte)eStartType.Standard && DBDataQuest.AcceptText == text)
			{
				//log.DebugFormat("Adding quest {0} to player {1}", Name, player.Name);
				CharacterXDataQuest charQuest = GetCharacterQuest(player, true);
				DataQuest dq = new DataQuest(player, living, DBDataQuest, charQuest);
				dq.Step = 1;
				player.AddQuest(dq);
				SendMessage(player, m_sourceTexts[0], 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
				if (living is GameNPC)
				{
					UpdateQuestIndicator(living as GameNPC, player);
				}
			}
		}


		/// <summary>
		/// A player with this quest has interacted with an object.
		/// See if this object is part of the quest and respond accordingly
		/// </summary>
		/// <param name="player"></param>
		/// <param name="obj"></param>
		protected virtual void OnPlayerInteract(GamePlayer player, GameObject obj)
		{
			if (TargetName == obj.Name && TargetRegion == obj.CurrentRegionID)
			{
				switch (StepType)
				{
					case eStepType.Interact:
						{
							if (string.IsNullOrEmpty(TargetText) == false)
							{
								SendMessage(m_questPlayer, TargetText, 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
							}

							AdvanceQuestStep(obj);
						}
						break;

					case eStepType.InteractFinish:
						{
							if (StartType == eStartType.RewardQuest)
							{
								GameNPC finishNPC = obj as GameNPC;
								if (finishNPC != null)
								{
									// Custom step can modify rewards here.  Should return false if it sends the reward window
									if (ExecuteCustomQuestStep(player, 0, eStepCheckType.Finish))
									{
										player.Out.SendQuestRewardWindow(finishNPC, player, this);
									}
								}
								else
								{
									log.ErrorFormat("DataQuest Finish is RewardQuest but object {0} is not an NPC!", obj.Name);
								}
							}
							else
							{
								FinishQuest(obj, true);
							}
						}
						break;

					default:
						{
							if (string.IsNullOrEmpty(TargetText) == false)
							{
								SendMessage(m_questPlayer, TargetText, 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
							}
						}
						break;
				}
			}
		}


		/// <summary>
		/// A player doing this quest has given an item to something.  All active quests check to see if they need to respond to this.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="obj"></param>
		/// <param name="item"></param>
		protected virtual void OnPlayerGiveItem(GamePlayer player, GameObject obj, InventoryItem item)
		{
			if (item == null || item.OwnerID == null || m_collectItems.Count == 0)
				return;

			//log.DebugFormat("player giving item {0}, looking for item {1}", item.Id_nb, m_collectItems[Step - 1]);

			if (TargetName == obj.Name && TargetRegion == obj.CurrentRegionID && m_collectItems[Step - 1] == item.Id_nb)
			{
				RemoveItem(obj, player, item, true);

				// if we turn over the item then advance the step

				if (item.OwnerID == null)
				{
					switch (StepType)
					{
						case eStepType.Deliver:
							{
								if (string.IsNullOrEmpty(TargetText) == false)
								{
									if (obj.Realm == eRealm.None)
									{
										// mobs and other non realm objects send chat text and not popup text.
										SendMessage(m_questPlayer, TargetText, 0, eChatType.CT_Say, eChatLoc.CL_ChatWindow);
									}
									else
									{
										SendMessage(m_questPlayer, TargetText, 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
									}
								}

								AdvanceQuestStep(obj);
							}
							break;

						case eStepType.DeliverFinish:
							{
								FinishQuest(obj, true);
							}
							break;
					}
				}
			}
		}


		/// <summary>
		/// A player has given an item to an object
		/// See if this object is part of the quest and respond accordingly
		/// </summary>
		/// <param name="player"></param>
		/// <param name="obj"></param>
		/// <param name="item"></param>
		protected virtual void OnNPCReceiveItem(GamePlayer player, GameObject obj, InventoryItem item)
		{
			if (m_questPlayer == null)
			{
				// Player may want to start this quest
				CheckOfferedQuestReceiveItem(player, obj, item);
				return;
			}
		}

		/// <summary>
		/// A player has whispered to a GameLiving
		/// The player is either starting this quest, or doing this quest
		/// </summary>
		/// <param name="player"></param>
		/// <param name="living"></param>
		/// <param name="text"></param>
		protected virtual void OnNPCReceiveWhisper(GamePlayer player, GameLiving living, string text)
		{
			if (m_questPlayer == null)
			{
				// Player may want to start this quest
				CheckOfferedQuestWhisper(player, living, text);
				return;
			}
		}

		/// <summary>
		/// A player doing this quest whispers something to a living
		/// </summary>
		/// <param name="p"></param>
		/// <param name="living"></param>
		/// <param name="text"></param>
		public virtual void OnPlayerWhisper(GamePlayer p, GameObject obj, string text)
		{
			//log.DebugFormat("Whisper {0}, listening for {1}, on step type {2}", text, AdvanceText, m_stepTypes[Step - 1]);

			if (TargetName == obj.Name && TargetRegion == obj.CurrentRegionID && AdvanceText == text)
			{
				switch (StepType)
				{
					case eStepType.Whisper:
						{
							AdvanceQuestStep(obj);
						}
						break;

					case eStepType.WhisperFinish:
						{
							FinishQuest(obj, true);
						}
						break;
				}
			}
		}

		/// <summary>
		/// The living offering a dataquest is dying.  Do we have any kill quests we need to activate?
		/// </summary>
		/// <param name="dying"></param>
		/// <param name="killer"></param>
		/// <param name="playerKillers"></param>
		protected virtual void OnLivingIsDying(GameLiving dying, GameObject killer, List<GamePlayer> playerKillers)
		{
			if (StartType == eStartType.KillComplete)
			{
				if (playerKillers == null)
				{
					GamePlayer player = killer as GamePlayer;

					if (player == null)
					{
						GameNPC npc = killer as GameNPC;
						if (npc != null)
						{
							if (npc.Brain != null && npc.Brain is IControlledBrain)
							{
								player = (npc.Brain as IControlledBrain).GetPlayerOwner();
							}
						}
					}

					if (player == null)
						return;

					playerKillers = new List<GamePlayer>();
					playerKillers.Add(player);
				}

				foreach (GamePlayer player in playerKillers)
				{
					if (CheckQuestQualification(player))
					{
						CharacterXDataQuest charQuest = GetCharacterQuest(player, true);

						if (charQuest.Count < MaxQuestCount)
						{
							if (ExecuteCustomQuestStep(player, 0, eStepCheckType.Finish))
							{
								if (Description.Trim() != "")
								{
									SendMessage(player, Description, 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
								}

								if (m_finalRewards.Count > 0)
								{
									lock (player.Inventory)
									{
										if (player.Inventory.IsSlotsFree(m_finalRewards.Count, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
										{
											foreach (ItemTemplate item in m_finalRewards)
											{
												if (item != null)
												{
													GiveItem(dying, player, item, false);
												}
											}
										}
										else
										{
											SendMessage(player, "Your inventory does not have enough space to finish this quest!", 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
											return;
										}
									}
								}

								if (m_rewardXPs.Count > 0 && m_rewardXPs[0] > 0)
								{
									player.GainExperience(GameLiving.eXPSource.Quest, m_rewardXPs[0]);
								}

								if (m_rewardMoneys.Count > 0 && m_rewardMoneys[0] > 0)
								{
									player.AddMoney(m_rewardMoneys[0], "You are awarded {0}!");
								}

								charQuest.Count++;
								GameServer.Database.SaveObject(charQuest);

								bool add = true;
								lock (player.QuestListFinished)
								{
									foreach (AbstractQuest q in player.QuestListFinished)
									{
										if (q is DataQuest && (q as DataQuest).ID == ID)
										{
											add = false;
											break;
										}
									}
								}

								if (add)
								{
									player.QuestListFinished.Add(this);
								}

								player.Out.SendQuestListUpdate();

								player.Out.SendMessage(String.Format(LanguageMgr.GetTranslation(player.Client, "AbstractQuest.FinishQuest.Completed", Name)), eChatType.CT_ScreenCenter, eChatLoc.CL_SystemWindow);
								player.Out.SendMessage(String.Format(LanguageMgr.GetTranslation(player.Client, "AbstractQuest.FinishQuest.Completed", Name)), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
							}
						}
					}
				}
			}
		}


		/// <summary>
		/// Enemy of a player with a dataquest is killed, check for quest advancement
		/// </summary>
		/// <param name="player"></param>
		/// <param name="enemy"></param>
		protected virtual void OnEnemyKilled(GamePlayer player, GameLiving living)
		{
			if (TargetName == living.Name && TargetRegion == living.CurrentRegionID)
			{
				switch (StepType)
				{
					case eStepType.Kill:
						{
							if (string.IsNullOrEmpty(TargetText) == false)
							{
								if (living.Realm == eRealm.None)
								{
									// mobs and other non realm objects send chat text and not popup text.
									SendMessage(m_questPlayer, TargetText, 0, eChatType.CT_Say, eChatLoc.CL_ChatWindow);
								}
								else
								{
									SendMessage(m_questPlayer, TargetText, 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
								}
							}
							AdvanceQuestStep(living);
						}
						break;

					case eStepType.KillFinish:
						{
							FinishQuest(living, true);
						}
						break;
				}
			}
		}





		#endregion Notification Handlers


		public override void FinishQuest()
		{
			FinishQuest(null, true);
		}


		/// <summary>
		/// Finish the quest and update the player quest list
		/// </summary>
		public virtual void FinishQuest(GameObject obj, bool checkCustomStep)
		{
			if (m_questPlayer == null || m_charQuest == null || m_charQuest.IsValid == false)
				return;

			int lastStep = Step;

			if (checkCustomStep && ExecuteCustomQuestStep(QuestPlayer, Step, eStepCheckType.Finish) == false)
				return;

			// try rewards first

			lock (m_questPlayer.Inventory)
			{
				if (m_questPlayer.Inventory.IsSlotsFree(m_finalRewards.Count + m_optionalRewardChoice.Count, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
				{
					foreach (ItemTemplate item in m_finalRewards)
					{
						if (item != null)
						{
							GiveItem(m_questPlayer, item);
						}
					}

					foreach (ItemTemplate item in m_optionalRewardChoice)
					{
						if (item != null)
						{
							GiveItem(m_questPlayer, item);
						}
					}

					if (m_rewardXPs.Count > 0)
					{
						long rewardXP = m_rewardXPs[lastStep - 1];
						m_questPlayer.GainExperience(GameLiving.eXPSource.Quest, rewardXP);
					}

					if (m_rewardMoneys.Count > 0)
					{
						long rewardMoney = m_rewardMoneys[lastStep - 1];
						m_questPlayer.AddMoney(rewardMoney, "You are awarded {0}!");
					}
				}
				else
				{
					SendMessage(m_questPlayer, "Your inventory does not have enough space to finish this quest!", 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
					return;
				}
			}

			m_charQuest.Step = 0;
			m_charQuest.Count++;
			GameServer.Database.SaveObject(m_charQuest);

			m_questPlayer.Out.SendMessage(String.Format(LanguageMgr.GetTranslation(m_questPlayer.Client, "AbstractQuest.FinishQuest.Completed", Name)), eChatType.CT_ScreenCenter, eChatLoc.CL_SystemWindow);
			m_questPlayer.Out.SendMessage(String.Format(LanguageMgr.GetTranslation(m_questPlayer.Client, "AbstractQuest.FinishQuest.Completed", Name)), eChatType.CT_Important, eChatLoc.CL_SystemWindow);

			// Remove this quest from the players active quest list and either
			// Add or update the quest in the players finished list

			m_questPlayer.QuestList.Remove(this);

			bool add = true;
			lock (m_questPlayer.QuestListFinished)
			{
				foreach (AbstractQuest q in m_questPlayer.QuestListFinished)
				{
					if (q is DataQuest && (q as DataQuest).ID == ID)
					{
						(q as DataQuest).CharDataQuest.Step = 0;
						(q as DataQuest).CharDataQuest.Count++;
						add = false;
						break;
					}
				}
			}

			if (add)
			{
				m_questPlayer.QuestListFinished.Add(this);
			}

			m_questPlayer.Out.SendQuestListUpdate();

			if (StartType == eStartType.RewardQuest)
			{
				m_questPlayer.Out.SendSoundEffect(11, 0, 0, 0, 0, 0);
			}
			else if (m_dataQuest.FinishText != null && m_dataQuest.FinishText != "")
			{
				if (obj != null && obj.Realm == eRealm.None)
				{
					// mobs and other non realm objects send chat text and not popup text.
					SendMessage(m_questPlayer, m_dataQuest.FinishText, 0, eChatType.CT_Say, eChatLoc.CL_ChatWindow);
				}
				else
				{
					SendMessage(m_questPlayer, m_dataQuest.FinishText, 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
				}
			}

			if (obj != null && obj is GameNPC)
			{
				UpdateQuestIndicator(obj as GameNPC, m_questPlayer);
			}

			if (m_startNPC != null)
			{
				UpdateQuestIndicator(m_startNPC, m_questPlayer);
			}

			foreach (GameNPC npc in m_questPlayer.GetNPCsInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				UpdateQuestIndicator(npc, m_questPlayer);
			}
		}


		/// <summary>
		/// Replace special characters in an item string
		/// Supported parsing:
		/// %c = character class
		/// </summary>
		/// <param name="idnb"></param>
		/// <param name="player"></param>
		/// <returns></returns>
		protected virtual string ParseItemString(string idnb, GamePlayer player)
		{
			string parsed = idnb;

			parsed = parsed.Replace("%c", ((eCharacterClass)player.CharacterClass.ID).ToString());

			return parsed;
		}



		/// <summary>
		/// Called to abort the quest and remove it from the database!
		/// </summary>
		public override void AbortQuest()
		{
			if (m_questPlayer == null || m_charQuest == null || m_charQuest.IsValid == false) return;

			if (m_questPlayer.QuestList.Contains(this))
			{
				m_questPlayer.QuestList.Remove(this);
			}

			if (m_charQuest.Count == 0)
			{
				if (m_questPlayer.QuestListFinished.Contains(this))
				{
					m_questPlayer.QuestListFinished.Remove(this);
				}

				DeleteFromDatabase();
			}

			m_questPlayer.Out.SendQuestListUpdate();
			m_questPlayer.Out.SendMessage(LanguageMgr.GetTranslation(m_questPlayer.Client, "AbstractQuest.AbortQuest"), eChatType.CT_System, eChatLoc.CL_SystemWindow);

			if (m_startNPC != null)
			{
				UpdateQuestIndicator(m_startNPC, m_questPlayer);
			}
		}

		/// <summary>
		/// Saves this quest into the database
		/// </summary>
		public override void SaveIntoDatabase()
		{
			// Not applicable for data quests
		}

		/// <summary>
		/// Quest aborted, deleting from player
		/// </summary>
		public override void DeleteFromDatabase()
		{
			if (m_charQuest == null || m_charQuest.IsValid == false) return;

			CharacterXDataQuest charQuest = GameServer.Database.FindObjectByKey<CharacterXDataQuest>(m_charQuest.ID);
			if (charQuest != null)
			{
				GameServer.Database.DeleteObject(charQuest);
			}
		}

	}
}
