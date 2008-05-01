	
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
* Author:	Gandulf
* Date:		2007-04-13T18:23:43+02:00
*
* Notes:
*  DOL Server example quest (Help Sir Quait) rewritten using the Quest Designer
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

	namespace DOL.GS.Quests.Examples {
	
     /* The first thing we do, is to declare the class we create
	 * as Quest. To do this, we derive from the abstract class
	 * BaseQuest	  	 
	 */
	public class HelpSirQuait : BaseQuest
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

		protected const string questTitle = "Help Sir Quait to find his sword";

		protected const int minimumLevel = 1;
		protected const int maximumLevel = 1;
	
	
		private static GameNPC SirQuait = null;
		
		private static GameNPC EvilThiefoftheShadowclan = null;
		
		private static ItemTemplate SirQuaitsSword = null;
		

		// Custom Initialization Code Begin
		
		// Custom Initialization Code End

		/* 
		* Constructor
		*/
		public HelpSirQuait() : base()
		{
		}

		public HelpSirQuait(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public HelpSirQuait(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public HelpSirQuait(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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
	
		npcs = WorldMgr.GetNPCsByName("Sir Quait",(eRealm) 1);
		if (npcs.Length == 0)
		{			
			SirQuait = new DOL.GS.GameNPC();
				SirQuait.Model = 40;
			SirQuait.Name = "Sir Quait";
			if (log.IsWarnEnabled)
				log.Warn("Could not find " + SirQuait.Name + ", creating ...");
			SirQuait.GuildName = "Part of " + questTitle + " Quest";
			SirQuait.Realm = eRealm.Albion;
			SirQuait.CurrentRegionID = 1;
			SirQuait.Size = 50;
			SirQuait.Level = 10;
			SirQuait.MaxSpeedBase = 100;
			SirQuait.Faction = FactionMgr.GetFactionByID(0);
			SirQuait.X = 531971;
			SirQuait.Y = 478955;
			SirQuait.Z = 0;
			SirQuait.Heading = 3570;
			SirQuait.RespawnInterval = 0;
			SirQuait.BodyType = 0;
			

			StandardMobBrain brain = new StandardMobBrain();
			brain.AggroLevel = 0;
			brain.AggroRange = 0;
			SirQuait.SetOwnBrain(brain);
			
			//You don't have to store the created mob in the db if you don't want,
			//it will be recreated each time it is not found, just comment the following
			//line if you rather not modify your database
			if (SAVE_INTO_DATABASE)
				SirQuait.SaveIntoDatabase();
				
			SirQuait.AddToWorld();
			
		}
		else 
		{
			SirQuait = npcs[0];
		}
	
		npcs = WorldMgr.GetNPCsByName("Evil Thief of the Shadowclan",(eRealm) 0);
		if (npcs.Length == 0)
		{			
			EvilThiefoftheShadowclan = new DOL.GS.GameNPC();
				EvilThiefoftheShadowclan.Model = 55;
			EvilThiefoftheShadowclan.Name = "Evil Thief of the Shadowclan";
			if (log.IsWarnEnabled)
				log.Warn("Could not find " + EvilThiefoftheShadowclan.Name + ", creating ...");
			EvilThiefoftheShadowclan.GuildName = "Part of " + questTitle + " Quest";
			EvilThiefoftheShadowclan.Realm = eRealm.None;
			EvilThiefoftheShadowclan.CurrentRegionID = 1;
			EvilThiefoftheShadowclan.Size = 50;
			EvilThiefoftheShadowclan.Level = 1;
			EvilThiefoftheShadowclan.MaxSpeedBase = 100;
			EvilThiefoftheShadowclan.Faction = FactionMgr.GetFactionByID(0);
			EvilThiefoftheShadowclan.X = 532571;
			EvilThiefoftheShadowclan.Y = 479055;
			EvilThiefoftheShadowclan.Z = 0;
			EvilThiefoftheShadowclan.Heading = 3570;
			EvilThiefoftheShadowclan.RespawnInterval = 0;
			EvilThiefoftheShadowclan.BodyType = 0;
			

			StandardMobBrain brain = new StandardMobBrain();
			brain.AggroLevel = 0;
			brain.AggroRange = 0;
			EvilThiefoftheShadowclan.SetOwnBrain(brain);
			
			//You don't have to store the created mob in the db if you don't want,
			//it will be recreated each time it is not found, just comment the following
			//line if you rather not modify your database
			if (SAVE_INTO_DATABASE)
				EvilThiefoftheShadowclan.SaveIntoDatabase();
				
			EvilThiefoftheShadowclan.AddToWorld();
			
		}
		else 
		{
			EvilThiefoftheShadowclan = npcs[0];
		}
	

		#endregion

		#region defineItems

	    SirQuaitsSword = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "SirQuaitsSword");
		if (SirQuaitsSword == null)
		{
			SirQuaitsSword = new ItemTemplate();
			SirQuaitsSword.Name = "Sir Quait's Sword";
			if (log.IsWarnEnabled)
				log.Warn("Could not find " + SirQuaitsSword.Name + ", creating it ...");
			SirQuaitsSword.Level = 1;
			SirQuaitsSword.Weight = 0;
			SirQuaitsSword.Model = 847;
			SirQuaitsSword.Object_Type = 0;
			SirQuaitsSword.Item_Type = 0;
			SirQuaitsSword.Id_nb = "SirQuaitsSword";
			SirQuaitsSword.Hand = 0;
			SirQuaitsSword.Platinum = 0;
			SirQuaitsSword.Gold = 0;
			SirQuaitsSword.Silver = 0;
			SirQuaitsSword.Copper = 0;
			SirQuaitsSword.IsPickable = false;
			SirQuaitsSword.IsDropable = false;
			SirQuaitsSword.IsTradable = true;
			SirQuaitsSword.CanDropAsLoot = true;
			SirQuaitsSword.Color = 0;
			SirQuaitsSword.Bonus = 0; // default bonus				
			SirQuaitsSword.Bonus1 = 0;
			SirQuaitsSword.Bonus1Type = (int) 0;
			SirQuaitsSword.Bonus2 = 0;
			SirQuaitsSword.Bonus2Type = (int) 0;
			SirQuaitsSword.Bonus3 = 0;
			SirQuaitsSword.Bonus3Type = (int) 0;
			SirQuaitsSword.Bonus4 = 0;
			SirQuaitsSword.Bonus4Type = (int) 0;
			SirQuaitsSword.Bonus5 = 0;
			SirQuaitsSword.Bonus5Type = (int) 0;
			SirQuaitsSword.Bonus6 = 0;
			SirQuaitsSword.Bonus6Type = (int) 0;
			SirQuaitsSword.Bonus7 = 0;
			SirQuaitsSword.Bonus7Type = (int) 0;
			SirQuaitsSword.Bonus8 = 0;
			SirQuaitsSword.Bonus8Type = (int) 0;
			SirQuaitsSword.Bonus9 = 0;
			SirQuaitsSword.Bonus9Type = (int) 0;
			SirQuaitsSword.Bonus10 = 0;
			SirQuaitsSword.Bonus10Type = (int) 0;
			SirQuaitsSword.ExtraBonus = 0;
			SirQuaitsSword.ExtraBonusType = (int) 0;
			SirQuaitsSword.Effect = 0;
			SirQuaitsSword.Emblem = 0;
			SirQuaitsSword.Charges = 0;
			SirQuaitsSword.MaxCharges = 0;
			SirQuaitsSword.SpellID = 0;
			SirQuaitsSword.ProcSpellID = 0;
			SirQuaitsSword.Type_Damage = 0;
			SirQuaitsSword.Realm = 0;
			SirQuaitsSword.MaxCount = 1;
			SirQuaitsSword.PackSize = 1;
			SirQuaitsSword.Extension = 0;
			SirQuaitsSword.Quality = 1;				
			SirQuaitsSword.Condition = 1;
			SirQuaitsSword.MaxCondition = 1;
			SirQuaitsSword.Durability = 1;
			SirQuaitsSword.MaxDurability = 1;
			SirQuaitsSword.PoisonCharges = 0;
			SirQuaitsSword.PoisonMaxCharges = 0;
			SirQuaitsSword.PoisonSpellID = 0;
			SirQuaitsSword.ProcSpellID1 = 0;
			SirQuaitsSword.SpellID1 = 0;
			SirQuaitsSword.MaxCharges1 = 0;
			SirQuaitsSword.Charges1 = 0;
			
			//You don't have to store the created item in the db if you don't want,
			//it will be recreated each time it is not found, just comment the following
			//line if you rather not modify your database
			if (SAVE_INTO_DATABASE)
				GameServer.Database.AddNewObject(SirQuaitsSword);
			}
		

		    #endregion

		    #region defineAreas
    		
	        #endregion
    	
		    #region defineQuestParts

		    QuestBuilder builder = QuestMgr.getBuilder(typeof(HelpSirQuait));
			    QuestBehaviour a;
			    a = builder.CreateBehaviour(SirQuait,-1);
				    a.AddTrigger(eTriggerType.Interact,null,SirQuait);
			    a.AddRequirement(eRequirementType.QuestPending,typeof(HelpSirQuait),null,(eComparator)5);
			    a.AddAction(eActionType.Talk,"Hello adventurer, an [evil thief] has stolen my [sword], can [you help me] get it back?",SirQuait);
			    AddBehaviour(a);
			    a = builder.CreateBehaviour(SirQuait,-1);
				    a.AddTrigger(eTriggerType.Whisper,"sword",SirQuait);
			    a.AddRequirement(eRequirementType.QuestPending,typeof(HelpSirQuait),null,(eComparator)5);
			    a.AddAction(eActionType.Talk,"I really need it and if [you help me], I will give you a little reward!",SirQuait);
			    AddBehaviour(a);
			    a = builder.CreateBehaviour(SirQuait,-1);
				    a.AddTrigger(eTriggerType.Whisper,"sword",SirQuait);
			    a.AddRequirement(eRequirementType.QuestPending,typeof(HelpSirQuait),null);
			    a.AddAction(eActionType.Talk,"I really need it I am so glad you are helping me!",SirQuait);
			    AddBehaviour(a);
			    a = builder.CreateBehaviour(SirQuait,-1);
				    a.AddTrigger(eTriggerType.Whisper,"evil thief",SirQuait);
			    a.AddRequirement(eRequirementType.QuestPending,typeof(HelpSirQuait),null,(eComparator)5);
			    a.AddAction(eActionType.Talk,"This evil thief of the shadowclan bastard, he stole it! Kill him and get my [sword] back to me!",SirQuait);
			    AddBehaviour(a);
			    a = builder.CreateBehaviour(SirQuait,-1);
				    a.AddTrigger(eTriggerType.Whisper,"you help me",SirQuait);
			    a.AddRequirement(eRequirementType.QuestPending,typeof(HelpSirQuait),null,(eComparator)5);
			    a.AddRequirement(eRequirementType.QuestGivable,typeof(HelpSirQuait),SirQuait);
			    a.AddAction(eActionType.OfferQuest,typeof(HelpSirQuait),"Do you want to help Sir Quait?");
			    AddBehaviour(a);
			    a = builder.CreateBehaviour(SirQuait,-1);
				    a.AddTrigger(eTriggerType.Whisper,"find",SirQuait);
			    a.AddRequirement(eRequirementType.QuestPending,typeof(HelpSirQuait),null);
			    a.AddAction(eActionType.Talk,"Really!? Please give it to me!",SirQuait);
			    AddBehaviour(a);
			    a = builder.CreateBehaviour(SirQuait,-1);
				    a.AddTrigger(eTriggerType.Interact,null,SirQuait);
			    a.AddRequirement(eRequirementType.QuestPending,typeof(HelpSirQuait),null);
			    a.AddAction(eActionType.Talk,"Did you [find] my [sword]?",SirQuait);
			    AddBehaviour(a);
			    a = builder.CreateBehaviour(SirQuait,-1);
				    a.AddTrigger(eTriggerType.DeclineQuest,null,typeof(HelpSirQuait));
			    a.AddRequirement(eRequirementType.QuestGivable,typeof(HelpSirQuait),SirQuait);
			    a.AddAction(eActionType.Talk,"Oh well, if you change your mind, please come back!",SirQuait);
			    AddBehaviour(a);
			    a = builder.CreateBehaviour(SirQuait,-1);
				    a.AddTrigger(eTriggerType.AcceptQuest,null,typeof(HelpSirQuait));
			    a.AddRequirement(eRequirementType.QuestGivable,typeof(HelpSirQuait),SirQuait);
			    a.AddAction(eActionType.Talk,"Thank you! Please bring the [sword] back to me!",SirQuait);
			    a.AddAction(eActionType.GiveQuest,typeof(HelpSirQuait),SirQuait);
			    AddBehaviour(a);
			    a = builder.CreateBehaviour(EvilThiefoftheShadowclan,-1);
				    a.AddTrigger(eTriggerType.EnemyKilled,null,EvilThiefoftheShadowclan);
			    a.AddRequirement(eRequirementType.QuestStep,typeof(HelpSirQuait),1,(eComparator)3);
			    a.AddAction(eActionType.Message,"You defeated the evil thief and quickly pick up Sir Quait's sword!",(eTextType)1);
			    a.AddAction(eActionType.GiveItem,SirQuaitsSword,null);
			    AddBehaviour(a);
			    a = builder.CreateBehaviour(SirQuait,-1);
				    a.AddTrigger(eTriggerType.GiveItem,SirQuait,SirQuaitsSword);
			    a.AddRequirement(eRequirementType.QuestStep,typeof(HelpSirQuait),1,(eComparator)3);
			    a.AddAction(eActionType.Message,"Sir Quait thanks you for bringing back his holy sword!",(eTextType)1);
			    a.AddAction(eActionType.TakeItem,SirQuaitsSword,null);
			    a.AddAction(eActionType.FinishQuest,typeof(HelpSirQuait),null);
			    AddBehaviour(a);
    			
			#endregion

			// Custom Scriptloaded Code Begin
    			
			// Custom Scriptloaded Code End

			SirQuait.AddQuestToGive(typeof (HelpSirQuait));
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{			
				
			// Custom Scriptunloaded Code Begin
			
			// Custom Scriptunloaded Code End

			

			/* If SirQuait has not been initialized, then we don't have to remove any
			 * hooks from him ;-)
			 */
			if (SirQuait == null)
				return;
			/* Now we remove the possibility to give this quest to players */			
			SirQuait.RemoveQuestToGive(typeof (HelpSirQuait));
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
						return "[Step #1] Find the evil thief and get the magic sword from him!";
				
					case 2:
						return "[Step #2] Bring back Sir Quait's magic sword to receive your reward!";
				
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
			if (player.IsDoingQuest(typeof (HelpSirQuait)) != null)
				return true;
				
			// Custom Code Begin
			
			// Custom  Code End
			
		
			if (player.Level > maximumLevel || player.Level < minimumLevel )
				return false;
		
			if (
		
			player.CharacterClass.ID != (byte) eCharacterClass.Fighter && 
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
