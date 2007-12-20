	
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
	public class Amorasaidmistyc : BaseQuest
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

		protected const string questTitle = "Amora's Aid for Mistyc";

		protected const int minimumLevel = 1;
		protected const int maximumLevel = 50;
	
	
		private static GameNPC Lycla = null;
		
		private static GameNPC Amora = null;
		
		private static GameNPC Kari = null;
		
		private static ItemTemplate snakevenom = null;
		

		// Custom Initialization Code Begin
		
		// Custom Initialization Code End

		/* 
		* Constructor
		*/
		public Amorasaidmistyc() : base()
		{
		}

		public Amorasaidmistyc(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public Amorasaidmistyc(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public Amorasaidmistyc(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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
	
			npcs = WorldMgr.GetNPCsByName("Lycla",(eRealm) 2);
			if (npcs.Length == 0)
			{			
				Lycla = new DOL.GS.GameNPC();
					Lycla.Model = 178;
				Lycla.Name = "Lycla";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Lycla.Name + ", creating ...");
				Lycla.GuildName = "Part of " + questTitle + " Quest";
				Lycla.Realm = (byte) 2;
				Lycla.CurrentRegionID = 100;
				Lycla.Size = 48;
				Lycla.Level = 50;
				Lycla.MaxSpeedBase = 191;
				Lycla.Faction = FactionMgr.GetFactionByID(0);
				Lycla.X = 749032;
				Lycla.Y = 814613;
				Lycla.Z = 4408;
				Lycla.Heading = 170;
				Lycla.RespawnInterval = -1;
				Lycla.BodyType = 0;
				

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 500;
				Lycla.SetOwnBrain(brain);
				
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					Lycla.SaveIntoDatabase();
					
				Lycla.AddToWorld();
				
			}
			else 
			{
				Lycla = npcs[0];
			}
		
			npcs = WorldMgr.GetNPCsByName("Amora",(eRealm) 2);
			if (npcs.Length == 0)
			{			
				Amora = new DOL.GS.GameNPC();
					Amora.Model = 216;
				Amora.Name = "Amora";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Amora.Name + ", creating ...");
				Amora.GuildName = "Part of " + questTitle + " Quest";
				Amora.Realm = (byte) 2;
				Amora.CurrentRegionID = 100;
				Amora.Size = 49;
				Amora.Level = 28;
				Amora.MaxSpeedBase = 191;
				Amora.Faction = FactionMgr.GetFactionByID(0);
				Amora.X = 747714;
				Amora.Y = 814910;
				Amora.Z = 4636;
				Amora.Heading = 3456;
				Amora.RespawnInterval = -1;
				Amora.BodyType = 0;
				

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 500;
				Amora.SetOwnBrain(brain);
				
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					Amora.SaveIntoDatabase();
					
				Amora.AddToWorld();
				
			}
			else 
			{
				Amora = npcs[0];
			}
		
			npcs = WorldMgr.GetNPCsByName("Kari",(eRealm) 2);
			if (npcs.Length == 0)
			{			
				Kari = new GameHealer();
					Kari.Model = 216;
				Kari.Name = "Kari";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Kari.Name + ", creating ...");
				Kari.GuildName = "Part of " + questTitle + " Quest";
				Kari.Realm = (byte) 2;
				Kari.CurrentRegionID = 100;
				Kari.Size = 51;
				Kari.Level = 20;
				Kari.MaxSpeedBase = 191;
				Kari.Faction = FactionMgr.GetFactionByID(0);
				Kari.X = 749114;
				Kari.Y = 814019;
				Kari.Z = 4408;
				Kari.Heading = 3595;
				Kari.RespawnInterval = -1;
				Kari.BodyType = 0;
				

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 500;
				Kari.SetOwnBrain(brain);
				
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					Kari.SaveIntoDatabase();
					
				Kari.AddToWorld();
				
			}
			else 
			{
				Kari = npcs[0];
			}
		

			#endregion

			#region defineItems

		snakevenom = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "snakevenom");
			if (snakevenom == null)
			{
				snakevenom = new ItemTemplate();
				snakevenom.Name = "Snake Venom";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + snakevenom.Name + ", creating it ...");
				snakevenom.Level = 1;
				snakevenom.Weight = 1;
				snakevenom.Model = 488;
				snakevenom.Object_Type = 46;
				snakevenom.Item_Type = -1;
				snakevenom.Id_nb = "snakevenom";
				snakevenom.Hand = 0;
				snakevenom.Platinum = 0;
				snakevenom.Gold = 0;
				snakevenom.Silver = 0;
				snakevenom.Copper = 0;
				snakevenom.IsPickable = true;
				snakevenom.IsDropable = true;
				snakevenom.IsTradable = true;
				snakevenom.CanDropAsLoot = true;
				snakevenom.Color = 0;
				snakevenom.Bonus = 0; // default bonus				
				snakevenom.Bonus1 = 0;
				snakevenom.Bonus1Type = (int) 0;
				snakevenom.Bonus2 = 0;
				snakevenom.Bonus2Type = (int) 0;
				snakevenom.Bonus3 = 0;
				snakevenom.Bonus3Type = (int) 0;
				snakevenom.Bonus4 = 0;
				snakevenom.Bonus4Type = (int) 0;
				snakevenom.Bonus5 = 0;
				snakevenom.Bonus5Type = (int) 0;
				snakevenom.Bonus6 = 0;
				snakevenom.Bonus6Type = (int) 0;
				snakevenom.Bonus7 = 0;
				snakevenom.Bonus7Type = (int) 0;
				snakevenom.Bonus8 = 0;
				snakevenom.Bonus8Type = (int) 0;
				snakevenom.Bonus9 = 0;
				snakevenom.Bonus9Type = (int) 0;
				snakevenom.Bonus10 = 0;
				snakevenom.Bonus10Type = (int) 0;
				snakevenom.ExtraBonus = 0;
				snakevenom.ExtraBonusType = (int) 0;
				snakevenom.Effect = 0;
				snakevenom.Emblem = 0;
				snakevenom.Charges = 0;
				snakevenom.MaxCharges = 0;
				snakevenom.SpellID = 0;
				snakevenom.ProcSpellID = 0;
				snakevenom.Type_Damage = 0;
				snakevenom.Realm = 0;
				snakevenom.MaxCount = 1;
				snakevenom.PackSize = 1;
				snakevenom.Extension = 0;
				snakevenom.Quality = 100;				
				snakevenom.Condition = 100;
				snakevenom.MaxCondition = 100;
				snakevenom.Durability = 100;
				snakevenom.MaxDurability = 100;
				snakevenom.PoisonCharges = 0;
				snakevenom.PoisonMaxCharges = 0;
				snakevenom.PoisonSpellID = 0;
				snakevenom.ProcSpellID1 = 0;
				snakevenom.SpellID1 = 0;
				snakevenom.MaxCharges1 = 0;
				snakevenom.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(snakevenom);
				}
			

			#endregion

			#region defineAreas
			
		#endregion
		
		#region defineQuestParts

		QuestBuilder builder = QuestMgr.getBuilder(typeof(Amorasaidmistyc));
			QuestBehaviour a;
			a = builder.CreateBehaviour(Lycla,-1);
				a.AddTrigger(eTriggerType.Interact,null,Lycla);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.Amorasaidmistyc),Lycla);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.Amorasaidmistyc),null,(eComparator)5);
			a.AddAction(eActionType.Talk,"The lady Amora here in Fort Atla has been here for several weeks because she is [unable] to travel.",Lycla);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Lycla,-1);
				a.AddTrigger(eTriggerType.Whisper,"unable",Lycla);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.Amorasaidmistyc),Lycla);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.Amorasaidmistyc),null,(eComparator)5);
			a.AddAction(eActionType.Talk,"Her daughter, Magnild has fallen [ill].",Lycla);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Lycla,-1);
				a.AddTrigger(eTriggerType.Whisper,"ill",Lycla);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.Amorasaidmistyc),Lycla);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Midgard.Amorasaidmistyc),null,(eComparator)5);
			a.AddAction(eActionType.Talk,"They have been staying in one of the homes here in town to keep the poor girl warm. Please help them, they are in great need.",Lycla);
			a.AddAction(eActionType.OfferQuest,typeof(DOL.GS.Quests.Midgard.Amorasaidmistyc),"The Lady Amora needs your help in curing her sick daughter.");
			AddBehaviour(a);
			a = builder.CreateBehaviour(Lycla,-1);
				a.AddTrigger(eTriggerType.DeclineQuest,null,typeof(DOL.GS.Quests.Midgard.Amorasaidmistyc));
			a.AddAction(eActionType.Talk,"No problem. See you.",Lycla);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Lycla,-1);
				a.AddTrigger(eTriggerType.AcceptQuest,null,typeof(DOL.GS.Quests.Midgard.Amorasaidmistyc));
			a.AddAction(eActionType.GiveQuest,typeof(DOL.GS.Quests.Midgard.Amorasaidmistyc),Lycla);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Amora,-1);
				a.AddTrigger(eTriggerType.Interact,null,Amora);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.Amorasaidmistyc),1,(eComparator)3);
			a.AddAction(eActionType.Talk,"Be still Magnild, all will be alright soon and you will be well once again.",Amora);
			a.AddAction(eActionType.Talk,"Hello stranger, may I ask your [name]?",Amora);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Amora,-1);
				a.AddTrigger(eTriggerType.Whisper,"name",Amora);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.Amorasaidmistyc),1,(eComparator)3);
			a.AddAction(eActionType.Talk,"It is a pleasure to meet you. I am so glad you have come to aid me and [Magnild].",Amora);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Amora,-1);
				a.AddTrigger(eTriggerType.Whisper,"Magnild",Amora);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.Amorasaidmistyc),1,(eComparator)3);
			a.AddAction(eActionType.Talk,"Yes, my poor darling Magnild was bitten by a [water snake] while traveling into town. Unfortunately, the bite has not healed and she is not feeling very well at all.",Amora);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Amora,-1);
				a.AddTrigger(eTriggerType.Whisper,"water snake",Amora);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.Amorasaidmistyc),1,(eComparator)3);
			a.AddAction(eActionType.Talk,"Oh, it seems the venom of the snake was very powerful. I have sent word to t he healer and she has told me she needs five vials of [snake venom] to cure the child.",Amora);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Amora,-1);
				a.AddTrigger(eTriggerType.Whisper,"snake venom",Amora);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.Amorasaidmistyc),1,(eComparator)3);
			a.AddAction(eActionType.Talk,"I do not feel right leaving Magnild [here alone] this ill, so I fear I can't hunt the snakes myself.",Amora);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Amora,-1);
				a.AddTrigger(eTriggerType.Whisper,"here alone",Amora);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.Amorasaidmistyc),1,(eComparator)3);
			a.AddAction(eActionType.Talk,"Please go outside the city and [collect] the five vials of venom and take them to Kari, the healer.",Amora);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Amora,-1);
				a.AddTrigger(eTriggerType.Whisper,"collect",Amora);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.Amorasaidmistyc),1,(eComparator)3);
			a.AddAction(eActionType.Talk,"I am indebted to you always if you do the for me.",Amora);
			a.AddAction(eActionType.SetQuestStep,typeof(DOL.GS.Quests.Midgard.Amorasaidmistyc),2);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Lycla,-1);
				a.AddTrigger(eTriggerType.EnemyKilled,"water snake",null);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.Amorasaidmistyc),2,(eComparator)3);
			a.AddRequirement(eRequirementType.InventoryItem,snakevenom,4,(eComparator)1);
			a.AddAction(eActionType.GiveItem,snakevenom,null);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Lycla,-1);
				a.AddTrigger(eTriggerType.EnemyKilled,"water snake",null);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.Amorasaidmistyc),2,(eComparator)3);
			a.AddRequirement(eRequirementType.InventoryItem,snakevenom,4,(eComparator)3);
			a.AddAction(eActionType.SetQuestStep,typeof(DOL.GS.Quests.Midgard.Amorasaidmistyc),3);
			a.AddAction(eActionType.GiveItem,snakevenom,null);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Kari,-1);
				a.AddTrigger(eTriggerType.Interact,null,Kari);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.Amorasaidmistyc),3,(eComparator)3);
			a.AddAction(eActionType.Talk,"Ahh, you look as if you have been having a rough time. Have you brought me the vials of snake venom?",Kari);
			a.AddAction(eActionType.TakeItem,snakevenom,5);
			a.AddAction(eActionType.Talk,"\"I shall begin [work] at once in making the potion to cure the sick girl, hopefully all my training will not fail me.",Kari);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Kari,-1);
				a.AddTrigger(eTriggerType.Whisper,"work",Kari);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.Amorasaidmistyc),3,(eComparator)3);
			a.AddAction(eActionType.Talk,"You have aided in this situation beyond what is expected. I thnk you.",Kari);
			a.AddAction(eActionType.GiveXP,10,null);
			a.AddAction(eActionType.GiveGold,42,null);
			a.AddAction(eActionType.FinishQuest,typeof(DOL.GS.Quests.Midgard.Amorasaidmistyc),null);
			AddBehaviour(a);
			
			#endregion

			// Custom Scriptloaded Code Begin
			
			// Custom Scriptloaded Code End

			Lycla.AddQuestToGive(typeof (Amorasaidmistyc));
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{			
				
			// Custom Scriptunloaded Code Begin
			
			// Custom Scriptunloaded Code End

			

			/* If Lycla has not been initialized, then we don't have to remove any
			 * hooks from him ;-)
			 */
			if (Lycla == null)
				return;
			/* Now we remove the possibility to give this quest to players */			
			Lycla.RemoveQuestToGive(typeof (Amorasaidmistyc));
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
						return "[Step #1] The lady Amora an her daughter need your assistance, Seek Amora out for details";
				
					case 2:
						return "[Step #2] The venom of the Water Snakes must be secured to cure Magnild, five vials are needed.";
				
					case 3:
						return "[Step #3] Give the vials of snake venom to Kari so that she may brew a cure.";
				
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
			if (player.IsDoingQuest(typeof (Amorasaidmistyc)) != null)
				return true;
				
			// Custom Code Begin
			
			// Custom  Code End
			
		
			if (player.Level > maximumLevel || player.Level < minimumLevel )
				return false;
		
			if (
		
			player.CharacterClass.ID != (byte) eCharacterClass.Mystic && 
			player.CharacterClass.ID != (byte) eCharacterClass.Runemaster && 
			player.CharacterClass.ID != (byte) eCharacterClass.Spiritmaster && 
			player.CharacterClass.ID != (byte) eCharacterClass.Warlock && 
			player.CharacterClass.ID != (byte) eCharacterClass.Bonedancer && 
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
