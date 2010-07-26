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
			Standard = 0,		// Talk to npc, accept quest, go through steps
			Collection = 1,		// player turns drops into npc for xp, quest not added to player quest log
			AutoStart = 2,		// quest is auto started simply by interacting with start object
			KillComplete = 3,	// Killing the Start NPC grants and finished the quest, similar to One Time Drops
		}

		/// <summary>
		/// The type of each quest step
		/// </summary>
		public enum eStepType : byte
		{
			Kill = 0,			// Kill the target to advance the quest
			KillFinish = 1,		// Killing the target finishes the quest and gives the reward
			Deliver = 2,		// Deliver an item to the target to advance the quest
			DeliverFinish = 3,	// Deliver an item to the target to finish the quest
			Interact = 4,		// Interact with the target to advance the step
			InteractFinish = 5,	// Interact with the target to finish the quest
			Whisper = 6,		// Whisper to the target to advance the quest
			WhisperFinish = 7,	// Whisper to the target to finish the quest
		}

		protected List<ushort> m_sourceRegions = new List<ushort>();
		protected List<string> m_sourceNames = new List<string>();
		protected List<string> m_sourceTexts = new List<string>();
		protected List<ushort> m_targetRegions = new List<ushort>();
		protected List<string> m_targetNames = new List<string>();
		protected List<string> m_targetTexts = new List<string>();
		protected List<eStepType> m_stepTypes = new List<eStepType>();
		protected List<string> m_stepTexts = new List<string>();
		protected List<string> m_advanceTexts = new List<string>();
		protected List<long> m_rewardXPs = new List<long>();
		protected List<long> m_rewardMoneys = new List<long>();
		protected List<string> m_finalRewards = new List<string>();

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
				parse1 = lastParse.Split('|');
				foreach (string str in parse1)
				{
					m_sourceTexts.Add(str);
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
					return m_stepTexts[Step - 1];
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

		/// <summary>
		/// Can this player do this quest
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override bool CheckQuestQualification(GamePlayer player)
		{
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
				}

				// Interact when already doing quest
				if (e == GamePlayerEvent.InteractWith)
				{
					GamePlayer p = sender as GamePlayer;
					InteractWithEventArgs a = args as InteractWithEventArgs;

					//log.DebugFormat("DataQuest Interact: Player {0} is interacting with {1}", p.Name, a.Target.Name);
					OnPlayerInteract(p, a.Target);
				}

				// Player is giving an item to something
				if (e == GamePlayerEvent.GiveItem)
				{
					GiveItemEventArgs a = args as GiveItemEventArgs;

					//log.DebugFormat("DataQuest: GiveItem {0} receives {1} from {2}", a.Target.Name, a.Item.Name, a.Source.Name);
					OnGiveItem(a.Source, a.Target, a.Item);
				}

				// Living is receiving an item, should a quest react to this
				if (e == GamePlayerEvent.ReceiveItem)
				{
					ReceiveItemEventArgs a = args as ReceiveItemEventArgs;
					GamePlayer p = a.Source as GamePlayer;

					if (p != null)
					{
						//log.DebugFormat("DataQuest: ReceiveItem {0} receives {1} from {2}", a.Target.Name, a.Item.Name, a.Source.Name);
						OnItemReceived(p, a.Target, a.Item);
					}
				}

				// Whisper
				if (e == GamePlayerEvent.WhisperReceive)
				{
					WhisperReceiveEventArgs a = args as WhisperReceiveEventArgs;
					GamePlayer p = a.Source as GamePlayer;

					if (p != null)
					{
						//log.DebugFormat("DataQuest: WhisperReceived {0} receives whisper {1} from {2}", a.Target.Name, a.Text, a.Source.Name);
						OnWhisperReceived(p, a.Target, a.Text);
					}
				}

			}
			catch (Exception ex)
			{
				log.Error("DataQuest Notify Error", ex);
			}
		}

		#endregion Notify


		/// <summary>
		/// A player has interacted with an object that has a DataQuest.
		/// Check to see if we can offer this quest to the player and display the text
		/// </summary>
		/// <param name="player"></param>
		/// <param name="obj"></param>
		public virtual void CheckOfferQuest(GamePlayer player, GameObject obj)
		{
			// Can we offer this quest to the player?
			if (CheckQuestQualification(player))
			{
				SendMessage(player, Description, 0, eChatType.CT_System, eChatLoc.CL_PopupWindow);
			}
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
		/// A player with this quest has interacted with an object.
		/// See if this object is part of the quest and respond accordingly
		/// </summary>
		/// <param name="player"></param>
		/// <param name="obj"></param>
		public virtual void OnPlayerInteract(GamePlayer player, GameObject obj)
		{
			//log.DebugFormat("Player {0} has quest and is interacting with {1}.", player.Name, obj.Name);

			if (m_targetNames[Step - 1] == obj.Name && m_targetRegions[Step - 1] == obj.CurrentRegionID)
			{
				if (m_stepTypes[Step - 1] == eStepType.InteractFinish)
				{
					FinishQuest(obj);
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
		public virtual void OnItemReceived(GamePlayer player, GameObject obj, InventoryItem item)
		{
			if (m_questPlayer == null)
			{
				// checking the quests we can offer to see if this is a collection quest or if the item starts a quest

				// check to see if this object has a collection quest and if so accept the item and generate the reward
				// collection quests do not go into a players quest list

				//log.DebugFormat("Checking collection quests: '{0}' of type '{1}', wants item '{2}'", Name, (eStartType)DBDataQuest.StartType, DBDataQuest.CollectItemTemplate == null ? "" : DBDataQuest.CollectItemTemplate);

				if (DBDataQuest.StartType == (byte)eStartType.Collection && item.Id_nb == DBDataQuest.CollectItemTemplate)
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

				return;
			}
		}


		/// <summary>
		/// A player doing a quest has given an item to something.  All active quests check to see if they need to respond to this.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="obj"></param>
		/// <param name="item"></param>
		public virtual void OnGiveItem(GamePlayer player, GameObject obj, InventoryItem item)
		{
		}


		/// <summary>
		/// A player has whispered to a GameLiving
		/// The player is either starting this quest, or doing this quest
		/// </summary>
		/// <param name="player"></param>
		/// <param name="living"></param>
		/// <param name="text"></param>
		public virtual void OnWhisperReceived(GamePlayer player, GameLiving living, string text)
		{
			// Player may want to start this quest
			if (m_questPlayer == null)
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

				return;
			}

			// Player is doing this quest

		}


		protected virtual CharacterXDataQuest GetCharacterQuest(GamePlayer player)
		{
			CharacterXDataQuest charQuest = GameServer.Database.SelectObject<CharacterXDataQuest>("Character_ID ='" + GameServer.Database.Escape(player.InternalID) + "' AND DataQuestID = " + ID);

			if (charQuest == null)
			{
				charQuest = new CharacterXDataQuest(player.InternalID, ID);
				GameServer.Database.AddObject(charQuest);
			}

			return charQuest;
		}

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
