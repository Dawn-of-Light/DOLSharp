	
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
using DOL.Database;
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
	public class Learnthehunt : BaseQuest
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

		protected const string questTitle = "Learn the hunt";

		protected const int minimumLevel = 1;
		protected const int maximumLevel = 5;
	
	
		private static GameNPC Aegan = null;
		
		private static ItemTemplate clawofblackmauler = null;
		

		// Custom Initialization Code Begin
		
		// Custom Initialization Code End

		/* 
		* Constructor
		*/
		public Learnthehunt() : base()
		{
		}

		public Learnthehunt(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public Learnthehunt(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public Learnthehunt(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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
	
			npcs = WorldMgr.GetNPCsByName("Aegan",(eRealm) 2);
			if (npcs.Length == 0)
			{			
				Aegan = new DOL.GS.GameNPC();
					Aegan.Model = 232;
				Aegan.Name = "Aegan";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Aegan.Name + ", creating ...");
				Aegan.GuildName = "Part of " + questTitle + " Quest";
				Aegan.Realm = (byte) 2;
				Aegan.CurrentRegionID = 100;
				Aegan.Size = 51;
				Aegan.Level = 41;
				Aegan.MaxSpeedBase = 191;
				Aegan.Faction = FactionMgr.GetFactionByID(0);
				Aegan.X = 805398;
				Aegan.Y = 725829;
				Aegan.Z = 4700;
				Aegan.Heading = 3595;
				Aegan.RespawnInterval = -1;
				Aegan.BodyType = 0;
				

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 500;
				Aegan.SetOwnBrain(brain);
				
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					Aegan.SaveIntoDatabase();
					
				Aegan.AddToWorld();
				
			}
			else 
			{
				Aegan = npcs[0];
			}
		

			#endregion

			#region defineItems

		clawofblackmauler = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "clawofblackmauler");
			if (clawofblackmauler == null)
			{
				clawofblackmauler = new ItemTemplate();
				clawofblackmauler.Name = "Claw of Black Mauler";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + clawofblackmauler.Name + ", creating it ...");
				clawofblackmauler.Level = 50;
				clawofblackmauler.Weight = 5;
				clawofblackmauler.Model = 1;
				clawofblackmauler.Object_Type = 0;
				clawofblackmauler.Item_Type = 40;
				clawofblackmauler.Id_nb = "clawofblackmauler";
				clawofblackmauler.Hand = 0;
				clawofblackmauler.Platinum = 0;
				clawofblackmauler.Gold = 0;
				clawofblackmauler.Silver = 0;
				clawofblackmauler.Copper = 0;
				clawofblackmauler.IsPickable = true;
				clawofblackmauler.IsDropable = true;
				clawofblackmauler.IsTradable = true;
				clawofblackmauler.CanDropAsLoot = false;
				clawofblackmauler.Color = 0;
				clawofblackmauler.Bonus = 35; // default bonus				
				clawofblackmauler.Bonus1 = 0;
				clawofblackmauler.Bonus1Type = (int) 0;
				clawofblackmauler.Bonus2 = 0;
				clawofblackmauler.Bonus2Type = (int) 0;
				clawofblackmauler.Bonus3 = 0;
				clawofblackmauler.Bonus3Type = (int) 0;
				clawofblackmauler.Bonus4 = 0;
				clawofblackmauler.Bonus4Type = (int) 0;
				clawofblackmauler.Bonus5 = 0;
				clawofblackmauler.Bonus5Type = (int) 0;
				clawofblackmauler.Bonus6 = 0;
				clawofblackmauler.Bonus6Type = (int) 0;
				clawofblackmauler.Bonus7 = 0;
				clawofblackmauler.Bonus7Type = (int) 0;
				clawofblackmauler.Bonus8 = 0;
				clawofblackmauler.Bonus8Type = (int) 0;
				clawofblackmauler.Bonus9 = 0;
				clawofblackmauler.Bonus9Type = (int) 0;
				clawofblackmauler.Bonus10 = 0;
				clawofblackmauler.Bonus10Type = (int) 0;
				clawofblackmauler.ExtraBonus = 0;
				clawofblackmauler.ExtraBonusType = (int) 0;
				clawofblackmauler.Effect = 0;
				clawofblackmauler.Emblem = 0;
				clawofblackmauler.Charges = 0;
				clawofblackmauler.MaxCharges = 0;
				clawofblackmauler.SpellID = 0;
				clawofblackmauler.ProcSpellID = 0;
				clawofblackmauler.Type_Damage = 0;
				clawofblackmauler.Realm = 0;
				clawofblackmauler.MaxCount = 1;
				clawofblackmauler.PackSize = 1;
				clawofblackmauler.Extension = 0;
				clawofblackmauler.Quality = 99;				
				clawofblackmauler.Condition = 100;
				clawofblackmauler.MaxCondition = 100;
				clawofblackmauler.Durability = 100;
				clawofblackmauler.MaxDurability = 100;
				clawofblackmauler.PoisonCharges = 0;
				clawofblackmauler.PoisonMaxCharges = 0;
				clawofblackmauler.PoisonSpellID = 0;
				clawofblackmauler.ProcSpellID1 = 0;
				clawofblackmauler.SpellID1 = 0;
				clawofblackmauler.MaxCharges1 = 0;
				clawofblackmauler.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(clawofblackmauler);
				}
			

			#endregion

			#region defineAreas
			
		#endregion
		
		#region defineQuestParts

		QuestBuilder builder = QuestMgr.getBuilder(typeof(Learnthehunt));
			QuestBehaviour a;
			a = builder.CreateBehaviour(Aegan,-1);
				a.AddTrigger(eTriggerType.Interact,null,Aegan);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.Learnthehunt),Aegan);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.Learnthehunt),null,(eComparator)5);
			a.AddAction(eActionType.Talk,"Greetings, are you [worthy of the hunt]?",Aegan);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Aegan,-1);
				a.AddTrigger(eTriggerType.Whisper,"worthy of the hunt",Aegan);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.Learnthehunt),Aegan);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.Learnthehunt),null,(eComparator)5);
			a.AddAction(eActionType.Talk,"Oh ho! Another bright young come to help Midgard fend off her enemies, eh? Wonderful, that's what I say! Wonderful! With the rise of the Albion and Hibernia armies,Midgard will need all the she can get!",Aegan);
			a.AddAction(eActionType.Talk,"I have spent a great deal of my life hunting the [maulers] of this region",Aegan);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Aegan,-1);
				a.AddTrigger(eTriggerType.Whisper,"maulers",Aegan);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.Learnthehunt),Aegan);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.Learnthehunt),null,(eComparator)5);
			a.AddAction(eActionType.Talk,"Bring me the claws of a mauler cub and I shall reward you",Aegan);
			a.AddAction(eActionType.OfferQuest,typeof(DOL.GS.Quests.Midgard.Learnthehunt),"Accept Learn the Hunt quest?");
			AddBehaviour(a);
			a = builder.CreateBehaviour(Aegan,-1);
				a.AddTrigger(eTriggerType.DeclineQuest,null,typeof(DOL.GS.Quests.Midgard.Learnthehunt));
			a.AddAction(eActionType.Talk,"No problem. See you.",Aegan);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Aegan,-1);
				a.AddTrigger(eTriggerType.AcceptQuest,null,typeof(DOL.GS.Quests.Midgard.Learnthehunt));
			a.AddAction(eActionType.GiveQuest,typeof(DOL.GS.Quests.Midgard.Learnthehunt),Aegan);
			a.AddAction(eActionType.SetQuestStep,typeof(DOL.GS.Quests.Midgard.Learnthehunt),2);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Aegan,-1);
				a.AddTrigger(eTriggerType.EnemyKilled,"black mauler cub",null);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.Learnthehunt),1,(eComparator)2);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.Learnthehunt),7,(eComparator)1);
			a.AddAction(eActionType.GiveItem,clawofblackmauler,null);
			a.AddAction(eActionType.IncQuestStep,typeof(DOL.GS.Quests.Midgard.Learnthehunt),null);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Aegan,-1);
				a.AddTrigger(eTriggerType.Interact,null,Aegan);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.Learnthehunt),7,(eComparator)3);
			a.AddAction(eActionType.Talk,"You are doing well! Continue to pass me the bear claws!",Aegan);
			a.AddAction(eActionType.Talk,"You have proven yourself well youngster! I hope the coin serves you well.",Aegan);
			a.AddAction(eActionType.GiveXP,20,null);
			a.AddAction(eActionType.GiveGold,100,null);
			a.AddAction(eActionType.FinishQuest,typeof(DOL.GS.Quests.Midgard.Learnthehunt),null);
			a.AddAction(eActionType.TakeItem,clawofblackmauler,5);
			AddBehaviour(a);
			
			#endregion

			// Custom Scriptloaded Code Begin
			
			// Custom Scriptloaded Code End

			Aegan.AddQuestToGive(typeof (Learnthehunt));
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{			
				
			// Custom Scriptunloaded Code Begin
			
			// Custom Scriptunloaded Code End

			

			/* If Aegan has not been initialized, then we don't have to remove any
			 * hooks from him ;-)
			 */
			if (Aegan == null)
				return;
			/* Now we remove the possibility to give this quest to players */			
			Aegan.RemoveQuestToGive(typeof (Learnthehunt));
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
						return "[Step #1] Hunt black mauler cubs for their claws";
				
					case 2:
						return "[Step #2] Kill more mauler cubs (total of 5)";
				
					case 3:
						return "[Step #3] Kill more mauler cubs (total of 5)";
				
					case 4:
						return "[Step #4] Kill more mauler cubs (total of 5)";
				
					case 5:
						return "[Step #5] Kill more mauler cubs (total of 5)";
				
					case 6:
						return "[Step #6] Kill more mauler cubs (total of 5)";
				
					case 7:
						return "[Step #7] You have proven yourself a true hunter. Return to Aegan with all five bear claws.";
				
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
			if (player.IsDoingQuest(typeof (Learnthehunt)) != null)
				return true;
				
			// Custom Code Begin
			
			// Custom  Code End
			
		
			if (player.Level > maximumLevel || player.Level < minimumLevel )
				return false;
		
			if (
		
			player.CharacterClass.ID != (byte) eCharacterClass.Mystic && 
			player.CharacterClass.ID != (byte) eCharacterClass.Viking && 
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
