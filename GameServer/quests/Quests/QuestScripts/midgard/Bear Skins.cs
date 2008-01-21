	
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

/*
* Author:	Lamyuras
* Date:		
*
* Notes:
*  
*/

using System;
using System.Reflection;
using DOL.Database2;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;
using DOL.GS.Quests;
using DOL.GS.Behaviour;
using DOL.GS.Behaviour.Attributes;
using DOL.AI.Brain;

	namespace DOL.GS.Quests.Midgard {
	
     /* The first thing we do, is to declare the class we create
	 * as Quest. To do this, we derive from the abstract class
	 * BaseQuest	  	 
	 */
	public class Bearskins : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		///
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/* Declare the variables we need inside our quest.
		* You can declare static variables here, which will be available in
		* ALL instance of your quest and should be initialized ONLY ONCE inside
		* the OnScriptLoaded method.
		*
		* Or declare nonstatic variables here which can be unique for each Player
		* and change through the quest journey...
		*
		*/

		protected const string questTitle = "Bear Skins";

		protected const int minimumLevel = 1;
		protected const int maximumLevel = 50;
	
	
		private static GameNPC Helen = null;
		
		private static ItemTemplate smallmaulerskin = null;
		

		// Custom Initialization Code Begin
		
		// Custom Initialization Code End

		/* 
		* Constructor
		*/
		public Bearskins() : base()
		{
		}

		public Bearskins(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public Bearskins(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public Bearskins(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
	{
	}

	[ScriptLoadedEvent]
	public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
	{
	if (!ServerProperties.Properties.LOAD_QUESTS)
		return;
	if (log.IsInfoEnabled)
		log.Info("Quest \"" + questTitle + "\" initializing ...");

	#region defineNPCs
	GameNPC[] npcs;
	
			npcs = WorldMgr.GetNPCsByName("Helen",(eRealm) 2);
			if (npcs.Length == 0)
			{			
				Helen = new DOL.GS.GameNPC();
					Helen.Model = 193;
				Helen.Name = "Helen";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Helen.Name + ", creating ...");
				Helen.GuildName = "Part of " + questTitle + " Quest";
				Helen.Realm = eRealm.Midgard;
				Helen.CurrentRegionID = 100;
				Helen.Size = 49;
				Helen.Level = 41;
				Helen.MaxSpeedBase = 191;
				Helen.Faction = FactionMgr.GetFactionByID(0);
				Helen.X = 805693;
				Helen.Y = 701160;
				Helen.Z = 4960;
				Helen.Heading = 3470;
				Helen.RespawnInterval = -1;
				Helen.BodyType = 0;
				

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 500;
				Helen.SetOwnBrain(brain);
				
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					Helen.SaveIntoDatabase();
					
				Helen.AddToWorld();
				
			}
			else 
			{
				Helen = npcs[0];
			}
		

			#endregion

			#region defineItems

		smallmaulerskin = (ItemTemplate) DatabaseLayer.Instance.SelectObject(typeof (ItemTemplate),"Id_nb", "smallmaulerskin");
			if (smallmaulerskin == null)
			{
				smallmaulerskin = new ItemTemplate();
				smallmaulerskin.Name = "Small Mauler Skin";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + smallmaulerskin.Name + ", creating it ...");
				smallmaulerskin.Level = 1;
				smallmaulerskin.Weight = 5;
				smallmaulerskin.Model = 100;
				smallmaulerskin.Object_Type = 0;
				smallmaulerskin.Item_Type = 40;
				smallmaulerskin.Id_nb = "smallmaulerskin";
				smallmaulerskin.Hand = 0;
				smallmaulerskin.Platinum = 0;
				smallmaulerskin.Gold = 0;
				smallmaulerskin.Silver = 0;
				smallmaulerskin.Copper = 0;
				smallmaulerskin.IsPickable = true;
				smallmaulerskin.IsDropable = true;
				smallmaulerskin.IsTradable = true;
				smallmaulerskin.CanDropAsLoot = false;
				smallmaulerskin.Color = 0;
				smallmaulerskin.Bonus = 35; // default bonus				
				smallmaulerskin.Bonus1 = 0;
				smallmaulerskin.Bonus1Type = (int) 0;
				smallmaulerskin.Bonus2 = 0;
				smallmaulerskin.Bonus2Type = (int) 0;
				smallmaulerskin.Bonus3 = 0;
				smallmaulerskin.Bonus3Type = (int) 0;
				smallmaulerskin.Bonus4 = 0;
				smallmaulerskin.Bonus4Type = (int) 0;
				smallmaulerskin.Bonus5 = 0;
				smallmaulerskin.Bonus5Type = (int) 0;
				smallmaulerskin.Bonus6 = 0;
				smallmaulerskin.Bonus6Type = (int) 0;
				smallmaulerskin.Bonus7 = 0;
				smallmaulerskin.Bonus7Type = (int) 0;
				smallmaulerskin.Bonus8 = 0;
				smallmaulerskin.Bonus8Type = (int) 0;
				smallmaulerskin.Bonus9 = 0;
				smallmaulerskin.Bonus9Type = (int) 0;
				smallmaulerskin.Bonus10 = 0;
				smallmaulerskin.Bonus10Type = (int) 0;
				smallmaulerskin.ExtraBonus = 0;
				smallmaulerskin.ExtraBonusType = (int) 0;
				smallmaulerskin.Effect = 0;
				smallmaulerskin.Emblem = 0;
				smallmaulerskin.Charges = 0;
				smallmaulerskin.MaxCharges = 0;
				smallmaulerskin.SpellID = 0;
				smallmaulerskin.ProcSpellID = 0;
				smallmaulerskin.Type_Damage = 0;
				smallmaulerskin.Realm = 0;
				smallmaulerskin.MaxCount = 1;
				smallmaulerskin.PackSize = 1;
				smallmaulerskin.Extension = 0;
				smallmaulerskin.Quality = 99;				
				smallmaulerskin.Condition = 100;
				smallmaulerskin.MaxCondition = 100;
				smallmaulerskin.Durability = 100;
				smallmaulerskin.MaxDurability = 100;
				smallmaulerskin.PoisonCharges = 0;
				smallmaulerskin.PoisonMaxCharges = 0;
				smallmaulerskin.PoisonSpellID = 0;
				smallmaulerskin.ProcSpellID1 = 0;
				smallmaulerskin.SpellID1 = 0;
				smallmaulerskin.MaxCharges1 = 0;
				smallmaulerskin.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(smallmaulerskin);
				}
			

			#endregion

			#region defineAreas
			
		#endregion
		
		#region defineQuestParts

		QuestBuilder builder = QuestMgr.getBuilder(typeof(Bearskins));
			QuestBehaviour a;
			a = builder.CreateBehaviour(Helen,-1);
				a.AddTrigger(eTriggerType.Interact,null,Helen);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.Bearskins),Helen);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.Bearskins),null,(eComparator)5);
			a.AddAction(eActionType.Talk,"I have lived in this village since I was a young girl. My father is a great bear hunter you know.",Helen);
			a.AddAction(eActionType.Talk,"In my years I have learned to work with all kinds of materials, specially [the skin of bears].",Helen);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Helen,-1);
				a.AddTrigger(eTriggerType.Whisper,"the skin of bears",Helen);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.Bearskins),Helen);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.Bearskins),null,(eComparator)5);
			a.AddAction(eActionType.Talk,"Hmm...A new person here to Haggerfel. Let me offer you some [advice].",Helen);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Helen,-1);
				a.AddTrigger(eTriggerType.Whisper,"advice",Helen);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.Bearskins),Helen);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.Bearskins),null,(eComparator)5);
			a.AddAction(eActionType.OfferQuest,typeof(DOL.GS.Quests.Midgard.Bearskins),"Helen has offered you the Bear skins quest.?Do you accept?");
			AddBehaviour(a);
			a = builder.CreateBehaviour(Helen,-1);
				a.AddTrigger(eTriggerType.DeclineQuest,null,typeof(DOL.GS.Quests.Midgard.Bearskins));
			a.AddAction(eActionType.Talk,"No problem. See you",Helen);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Helen,-1);
				a.AddTrigger(eTriggerType.AcceptQuest,null,typeof(DOL.GS.Quests.Midgard.Bearskins));
			a.AddAction(eActionType.Talk,"If you bring me the hide of a mauler cub I shall pay you well!",Helen);
			a.AddAction(eActionType.GiveQuest,typeof(DOL.GS.Quests.Midgard.Bearskins),Helen);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Helen,-1);
				a.AddTrigger(eTriggerType.EnemyKilled,"black mauler cub",null);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.Bearskins),1,(eComparator)3);
			a.AddAction(eActionType.GiveItem,smallmaulerskin,null);
			a.AddAction(eActionType.SetQuestStep,typeof(DOL.GS.Quests.Midgard.Bearskins),2);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Helen,-1);
				a.AddTrigger(eTriggerType.Interact,null,Helen);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.Bearskins),2,(eComparator)3);
			a.AddAction(eActionType.Talk,"You have returned! Did you bring me the skin?",Helen);
			a.AddAction(eActionType.TakeItem,smallmaulerskin,null);
			a.AddAction(eActionType.Talk,"Wonderful, wonderful, you have done well, and brought a good hide.",Helen);
			a.AddAction(eActionType.Talk,"The hide is small yet sturdy. Here is some coin for your efforts. Thank you.",Helen);
			a.AddAction(eActionType.GiveXP,10,null);
			a.AddAction(eActionType.GiveGold,105,null);
			a.AddAction(eActionType.FinishQuest,typeof(DOL.GS.Quests.Midgard.Bearskins),null);
			AddBehaviour(a);
			
			#endregion

			// Custom Scriptloaded Code Begin
			
			// Custom Scriptloaded Code End

			Helen.AddQuestToGive(typeof (Bearskins));
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{			
				
			// Custom Scriptunloaded Code Begin
			
			// Custom Scriptunloaded Code End

			

			/* If Helen has not been initialized, then we don't have to remove any
			 * hooks from him ;-)
			 */
			if (Helen == null)
				return;
			/* Now we remove the possibility to give this quest to players */			
			Helen.RemoveQuestToGive(typeof (Bearskins));
		}

		/* Now we set the quest name.
		* If we don't override the base method, then the quest
		* will have the name "UNDEFINED QUEST NAME" and we don't
		* want that, do we? ;-)
		*/

		public override string Name
		{
			get { return questTitle; }
		}

		/* Now we set the quest step descriptions.
		* If we don't override the base method, then the quest
		* description for ALL steps will be "UNDEFINDED QUEST DESCRIPTION"
		* and this isn't something nice either ;-)
		*/
		public override string Description
		{
			get
			{
			switch (Step)
			{
				
					case 1:
						return "[Step #1] Helen has experience with the hides of bears.?Bring her one she can work with.";
				
					case 2:
						return "[Step #2] Return the skin to Helen for a reward.";
				
					default:
						return " No Queststep Description available.";
				}				
			}
		}
		
		/// <summary>
		/// This method checks if a player is qualified for this quest
		/// </summary>
		/// <returns>true if qualified, false if not</returns>
		public override bool CheckQuestQualification(GamePlayer player)
		{		
			// if the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(typeof (Bearskins)) != null)
				return true;
				
			// Custom Code Begin
			
			// Custom  Code End
			
		
			if (player.Level > maximumLevel || player.Level < minimumLevel )
				return false;
		
			if (
		
			player.CharacterClass.ID != (byte) eCharacterClass.MidgardRogue && 
			player.CharacterClass.ID != (byte) eCharacterClass.Seer && 
				true) {
				return false;			
			}
		
			return true;
		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...
		}
	}
}
