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
	/// -------- DataQuests only support collection and single step 'interact' or 'kill' at the moment ---------
	/// 
	/// </summary>
	public class DataQuest : AbstractQuest
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected int m_step = 1;
		protected DBDataQuest m_dataQuest = null;
		protected CharacterXDataQuest m_charQuest = null;
		protected GameNPC m_startNPC = null;

		/// <summary>
		/// How does this quest start
		/// </summary>
		public enum eStartType : byte
		{
			Standard = 0,			// Talk to npc, accept quest, go through steps
			Collection = 1,			// Player turns drops into npc for xp, quest not added to player quest log, has no steps
			AutoStart = 2,			// Standard quest is auto started simply by interacting with start object
			KillComplete = 3,		// Killing the Start living grants and finished the quest, similar to One Time Drops, not logged in GamePlayer log
			InteractComplete = 4,	// Interacting with start object grants and finishes the quest, not logged in GamePlayer log
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
			InteractFinish = 5,		// Interact with the target to finish the quest
			Whisper = 6,			// Whisper to the target to advance the quest
			WhisperFinish = 7,		// Whisper to the target to finish the quest
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
		protected List<long> m_rewardXPs = new List<long>();
		protected List<long> m_rewardMoneys = new List<long>();
		byte m_numOptionalRewards = 0;
		protected List<string> m_optionalRewards = new List<string>();
		protected List<string> m_finalRewards = new List<string>();

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

			if (sourceObject != null && sourceObject is GameNPC)
			{
				m_startNPC = sourceObject as GameNPC;
			}

			ParseQuestData();
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

				lastParse = m_dataQuest.SourceText;
				if (lastParse != null)
				{
					parse1 = lastParse.Split('|');
					foreach (string str in parse1)
					{
						m_sourceTexts.Add(str);
					}
				}

				lastParse = m_dataQuest.TargetName;
				if (lastParse != null)
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
				if (lastParse != null)
				{
					parse1 = lastParse.Split('|');
					foreach (string str in parse1)
					{
						m_targetTexts.Add(str);
					}
				}


				lastParse = m_dataQuest.StepType;
				if (lastParse != null)
				{
					parse1 = lastParse.Split('|');
					foreach (string str in parse1)
					{
						m_stepTypes.Add((eStepType)Convert.ToByte(str));
					}
				}

				lastParse = m_dataQuest.StepText;
				if (lastParse != null)
				{
					parse1 = lastParse.Split('|');
					foreach (string str in parse1)
					{
						m_stepTexts.Add(str);
					}
				}

				lastParse = m_dataQuest.StepItemTemplates;
				if (lastParse != null)
				{
					parse1 = lastParse.Split('|');
					foreach (string str in parse1)
					{
						m_stepItemTemplates.Add(str);
					}
				}

				lastParse = m_dataQuest.AdvanceText;
				if (lastParse != null)
				{
					parse1 = lastParse.Split('|');
					foreach (string str in parse1)
					{
						m_advanceTexts.Add(str);
					}
				}

				lastParse = m_dataQuest.RewardMoney;
				parse1 = lastParse.Split('|');
				foreach (string str in parse1)
				{
					m_rewardMoneys.Add(Convert.ToInt64(str));
				}

				lastParse = m_dataQuest.RewardXP;
				parse1 = lastParse.Split('|');
				foreach (string str in parse1)
				{
					m_rewardXPs.Add(Convert.ToInt64(str));
				}

				lastParse = m_dataQuest.OptionalRewardItemTemplates;
				if (lastParse != null)
				{
					m_numOptionalRewards = Convert.ToByte(lastParse.Substring(0, 1));
					parse1 = lastParse.Substring(1).Split('|');
					foreach (string str in parse1)
					{
						m_optionalRewards.Add(str);
					}
				}

				lastParse = m_dataQuest.FinalRewardItemTemplates;
				if (lastParse != null)
				{
					parse1 = lastParse.Split('|');
					foreach (string str in parse1)
					{
						m_finalRewards.Add(str);
					}
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


		public virtual short Count
		{
			get { return m_charQuest.Count; }
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
					return m_dataQuest.Description;
				else
					return StepText;
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
		protected virtual CharacterXDataQuest GetCharacterQuest(GamePlayer player)
		{
			CharacterXDataQuest charQuest = GameServer.Database.SelectObject<CharacterXDataQuest>("Character_ID ='" + GameServer.Database.Escape(player.InternalID) + "' AND DataQuestID = " + ID);

			if (charQuest == null)
			{
				charQuest = new CharacterXDataQuest(player.InternalID, ID);
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

			if (StartType == eStartType.Collection)
			{
				CharacterXDataQuest charQuest = GetCharacterQuest(player);
				if (charQuest.Count >= MaxQuestCount)
				{
					return false;
				}

				return true;
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
					return m_targetTexts[Step - 1];
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
					return m_rewardXPs[Step - 1];
				}
				catch (Exception ex)
				{
					log.Error("DataQuest [" + ID + "] RewardXP error for Step " + Step, ex);
				}

				return 0;
			}
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
				// Send any target text to end this step

				if (TargetText != "")
				{
					SendMessage(m_questPlayer, TargetText, 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
				}

				eStepType nextStepType = m_stepTypes[Step];
				bool advance = true;

				// If next step requires giving the player an item then we need to check to make sure
				// player has enough inventory space to accept the item, otherwise do not advance the step

				if (nextStepType == eStepType.Deliver || 
					nextStepType == eStepType.DeliverFinish)
				{
					ItemTemplate item = GameServer.Database.FindObjectByKey<ItemTemplate>(m_stepItemTemplates[Step + 1]);
					if (item == null)
					{
						throw new Exception("Can't find ItemTemplate " + m_stepItemTemplates[Step + 1]);
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

					// Then say any source text for the new step

					if (SourceText != "")
					{
						SendMessage(m_questPlayer, SourceText, 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
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


			}
			catch (Exception ex)
			{
				log.Error("DataQuest Notify Error", ex);
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
					// This quest finishes with the interaction and is not placed in player quest log

					CharacterXDataQuest charQuest = GetCharacterQuest(player);

					if (charQuest.Count < MaxQuestCount)
					{
						if (Description != "")
						{
							SendMessage(player, Description, 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
						}

						lock (player.Inventory)
						{
							if (player.Inventory.IsSlotsFree(m_finalRewards.Count, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
							{
								foreach (string idnb in m_finalRewards)
								{
									ItemTemplate item = GameServer.Database.FindObjectByKey<ItemTemplate>(idnb);
									if (item != null)
									{
										GiveItem((obj is GameLiving ? obj as GameLiving : null), player, item, false);
									}
								}

								if (m_rewardXPs[0] > 0)
								{
									player.GainExperience(GameLiving.eXPSource.Quest, m_rewardXPs[0]);
								}

								if (m_rewardMoneys[0] > 0)
								{
									player.AddMoney(m_rewardMoneys[0], "You are awarded {0}!");
								}
							}
							else
							{
								SendMessage(player, "Your inventory does not have enough space to finish this quest!", 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
								return;
							}
						}

						charQuest.Count++;
						GameServer.Database.SaveObject(charQuest);
					}
					return;
				}

				if (StartType == eStartType.AutoStart)
				{
					CharacterXDataQuest charQuest = GetCharacterQuest(player);
					DataQuest dq = new DataQuest(player, obj, DBDataQuest, charQuest);
					dq.Step = 1;
					player.AddQuest(dq);
					if (m_sourceTexts[0] != "")
					{
						SendMessage(player, m_sourceTexts[0], 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
					}
					return;
				}

				// Standard offer quest dialog
				SendMessage(player, Description, 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
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
				CharacterXDataQuest charQuest = GetCharacterQuest(player);

				if (charQuest.Count < MaxQuestCount)
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
				CharacterXDataQuest charQuest = GetCharacterQuest(player);
				DataQuest dq = new DataQuest(player, living, DBDataQuest, charQuest);
				dq.Step = 1;
				player.AddQuest(dq);
				SendMessage(player, m_sourceTexts[0], 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
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
			//log.DebugFormat("Player {0} has quest and is interacting with {1}.", player.Name, obj.Name);

			if (TargetName == obj.Name && TargetRegion == obj.CurrentRegionID)
			{
				switch (StepType)
				{
					case eStepType.Interact:
						{
							AdvanceQuestStep(obj);
						}
						break;

					case eStepType.InteractFinish:
						{
							FinishQuest(obj);
						}
						break;
				}
			}
		}


		/// <summary>
		/// A player doing a quest has given an item to something.  All active quests check to see if they need to respond to this.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="obj"></param>
		/// <param name="item"></param>
		protected virtual void OnPlayerGiveItem(GamePlayer player, GameObject obj, InventoryItem item)
		{
			if (TargetName == obj.Name && TargetRegion == obj.CurrentRegionID && StepItemTemplate == item.Id_nb)
			{
				switch (StepType)
				{
					case eStepType.Deliver:
						{
							AdvanceQuestStep(obj);
						}
						break;

					case eStepType.DeliverFinish:
						{
							FinishQuest(obj);
						}
						break;
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

			if (TargetName == living.Name && TargetRegion == living.CurrentRegionID && AdvanceText == text)
			{
				switch (StepType)
				{
					case eStepType.Whisper:
						{
							AdvanceQuestStep(living);
						}
						break;

					case eStepType.WhisperFinish:
						{
							FinishQuest(living);
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
			log.Error("KillComplete quests not supported yet.");
			// not done
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
							AdvanceQuestStep(living);
						}
						break;

					case eStepType.KillFinish:
						{
							FinishQuest(living);
						}
						break;
				}
			}
		}





		#endregion Notification Handlers


		public override void FinishQuest()
		{
			FinishQuest(null);
		}


		/// <summary>
		/// Finish the quest and update the player quest list
		/// </summary>
		public virtual void FinishQuest(GameObject obj)
		{
			if (m_questPlayer == null || m_charQuest == null || m_charQuest.IsValid == false)
				return;

			SendMessage(m_questPlayer, TargetText, 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);

			int lastStep = Step;

			// try rewards first

			lock (m_questPlayer.Inventory)
			{
				if (m_questPlayer.Inventory.IsSlotsFree(m_finalRewards.Count, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
				{
					foreach (string idnb in m_finalRewards)
					{
						ItemTemplate item = GameServer.Database.FindObjectByKey<ItemTemplate>(idnb);
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
			SendMessage(m_questPlayer, m_targetTexts[0], 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);

			if (obj != null && obj is GameNPC)
			{
				UpdateQuestIndicator(obj as GameNPC, m_questPlayer);
			}
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
