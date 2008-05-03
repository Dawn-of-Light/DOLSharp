	
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
	public class essenceoflife : BaseQuest
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

		protected const string questTitle = "Essence of Life";

		protected const int minimumLevel = 1;
		protected const int maximumLevel = 4;
	
	
		private static GameNPC AmbientRatStatua = null;
		
		private static GameNPC Ballach = null;
		
		private static ItemTemplate enchantedflask = null;
		
		private static ItemTemplate Flaskofetherealessence = null;
		

		// Custom Initialization Code Begin
		
		// Custom Initialization Code End

		/* 
		* Constructor
		*/
		public essenceoflife() : base()
		{
		}

		public essenceoflife(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public essenceoflife(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public essenceoflife(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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
	
			npcs = WorldMgr.GetNPCsByName("Ambient Rat Statua",(eRealm) 0);
			if (npcs.Length == 0)
			{			
				AmbientRatStatua = new DOL.GS.GameNPC();
					AmbientRatStatua.Model = 1673;
				AmbientRatStatua.Name = "Ambient Rat Statua";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + AmbientRatStatua.Name + ", creating ...");
				AmbientRatStatua.GuildName = "Part of " + questTitle + " Quest";
				AmbientRatStatua.Realm = eRealm.None;
				AmbientRatStatua.CurrentRegionID = 229;
				AmbientRatStatua.Size = 1;
				AmbientRatStatua.Level = 1;
				AmbientRatStatua.MaxSpeedBase = 0;
				AmbientRatStatua.X = 40887;
				AmbientRatStatua.Y = 39276;
				AmbientRatStatua.Z = 17040;
				AmbientRatStatua.Heading = 0;
				AmbientRatStatua.RespawnInterval = -1;
				AmbientRatStatua.BodyType = 0;
				

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				AmbientRatStatua.SetOwnBrain(brain);
				
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					AmbientRatStatua.SaveIntoDatabase();
					
				AmbientRatStatua.AddToWorld();
				
			}
			else 
			{
				AmbientRatStatua = npcs[0];
			}
		
			npcs = WorldMgr.GetNPCsByName("Ballach",(eRealm) 2);
			if (npcs.Length == 0)
			{			
				Ballach = new DOL.GS.GameNPC();
					Ballach.Model = 225;
				Ballach.Name = "Ballach";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Ballach.Name + ", creating ...");
				Ballach.GuildName = "Part of " + questTitle + " Quest";
				Ballach.Realm = eRealm.Midgard;
				Ballach.CurrentRegionID = 243;
				Ballach.Size = 48;
				Ballach.Level = 20;
				Ballach.MaxSpeedBase = 0;
				Ballach.Faction = FactionMgr.GetFactionByID(0);
				Ballach.X = 27723;
				Ballach.Y = 39184;
				Ballach.Z = 20156;
				Ballach.Heading = 2070;
				Ballach.RespawnInterval = -1;
				Ballach.BodyType = 0;
				

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 500;
				Ballach.SetOwnBrain(brain);
				
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					Ballach.SaveIntoDatabase();
					
				Ballach.AddToWorld();
				
			}
			else 
			{
				Ballach = npcs[0];
			}
		

			#endregion

			#region defineItems

		enchantedflask = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "enchantedflask");
			if (enchantedflask == null)
			{
				enchantedflask = new ItemTemplate();
				enchantedflask.Name = "Enchanted Flask";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + enchantedflask.Name + ", creating it ...");
				enchantedflask.Level = 1;
				enchantedflask.Weight = 5;
				enchantedflask.Model = 99;
				enchantedflask.Object_Type = 0;
				enchantedflask.Item_Type = 40;
				enchantedflask.Id_nb = "enchantedflask";
				enchantedflask.Hand = 0;
				enchantedflask.Platinum = 0;
				enchantedflask.Gold = 0;
				enchantedflask.Silver = 0;
				enchantedflask.Copper = 0;
				enchantedflask.IsPickable = true;
				enchantedflask.IsDropable = true;
				enchantedflask.IsTradable = false;
				enchantedflask.CanDropAsLoot = false;
				enchantedflask.Color = 0;
				enchantedflask.Bonus = 35; // default bonus				
				enchantedflask.Bonus1 = 0;
				enchantedflask.Bonus1Type = (int) 0;
				enchantedflask.Bonus2 = 0;
				enchantedflask.Bonus2Type = (int) 0;
				enchantedflask.Bonus3 = 0;
				enchantedflask.Bonus3Type = (int) 0;
				enchantedflask.Bonus4 = 0;
				enchantedflask.Bonus4Type = (int) 0;
				enchantedflask.Bonus5 = 0;
				enchantedflask.Bonus5Type = (int) 0;
				enchantedflask.Bonus6 = 0;
				enchantedflask.Bonus6Type = (int) 0;
				enchantedflask.Bonus7 = 0;
				enchantedflask.Bonus7Type = (int) 0;
				enchantedflask.Bonus8 = 0;
				enchantedflask.Bonus8Type = (int) 0;
				enchantedflask.Bonus9 = 0;
				enchantedflask.Bonus9Type = (int) 0;
				enchantedflask.Bonus10 = 0;
				enchantedflask.Bonus10Type = (int) 0;
				enchantedflask.ExtraBonus = 0;
				enchantedflask.ExtraBonusType = (int) 0;
				enchantedflask.Effect = 0;
				enchantedflask.Emblem = 0;
				enchantedflask.Charges = 0;
				enchantedflask.MaxCharges = 0;
				enchantedflask.SpellID = 0;
				enchantedflask.ProcSpellID = 0;
				enchantedflask.Type_Damage = 0;
				enchantedflask.Realm = 2;
				enchantedflask.MaxCount = 1;
				enchantedflask.PackSize = 1;
				enchantedflask.Extension = 0;
				enchantedflask.Quality = 99;				
				enchantedflask.Condition = 100;
				enchantedflask.MaxCondition = 100;
				enchantedflask.Durability = 100;
				enchantedflask.MaxDurability = 100;
				enchantedflask.PoisonCharges = 0;
				enchantedflask.PoisonMaxCharges = 0;
				enchantedflask.PoisonSpellID = 0;
				enchantedflask.ProcSpellID1 = 0;
				enchantedflask.SpellID1 = 0;
				enchantedflask.MaxCharges1 = 0;
				enchantedflask.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(enchantedflask);
				}
			Flaskofetherealessence = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "Flaskofetherealessence");
			if (Flaskofetherealessence == null)
			{
				Flaskofetherealessence = new ItemTemplate();
				Flaskofetherealessence.Name = "Flask of Ethereal Essence";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Flaskofetherealessence.Name + ", creating it ...");
				Flaskofetherealessence.Level = 1;
				Flaskofetherealessence.Weight = 5;
				Flaskofetherealessence.Model = 99;
				Flaskofetherealessence.Object_Type = 0;
				Flaskofetherealessence.Item_Type = 40;
				Flaskofetherealessence.Id_nb = "Flaskofetherealessence";
				Flaskofetherealessence.Hand = 0;
				Flaskofetherealessence.Platinum = 0;
				Flaskofetherealessence.Gold = 0;
				Flaskofetherealessence.Silver = 0;
				Flaskofetherealessence.Copper = 0;
				Flaskofetherealessence.IsPickable = true;
				Flaskofetherealessence.IsDropable = true;
				Flaskofetherealessence.IsTradable = false;
				Flaskofetherealessence.CanDropAsLoot = false;
				Flaskofetherealessence.Color = 0;
				Flaskofetherealessence.Bonus = 35; // default bonus				
				Flaskofetherealessence.Bonus1 = 0;
				Flaskofetherealessence.Bonus1Type = (int) 0;
				Flaskofetherealessence.Bonus2 = 0;
				Flaskofetherealessence.Bonus2Type = (int) 0;
				Flaskofetherealessence.Bonus3 = 0;
				Flaskofetherealessence.Bonus3Type = (int) 0;
				Flaskofetherealessence.Bonus4 = 0;
				Flaskofetherealessence.Bonus4Type = (int) 0;
				Flaskofetherealessence.Bonus5 = 0;
				Flaskofetherealessence.Bonus5Type = (int) 0;
				Flaskofetherealessence.Bonus6 = 0;
				Flaskofetherealessence.Bonus6Type = (int) 0;
				Flaskofetherealessence.Bonus7 = 0;
				Flaskofetherealessence.Bonus7Type = (int) 0;
				Flaskofetherealessence.Bonus8 = 0;
				Flaskofetherealessence.Bonus8Type = (int) 0;
				Flaskofetherealessence.Bonus9 = 0;
				Flaskofetherealessence.Bonus9Type = (int) 0;
				Flaskofetherealessence.Bonus10 = 0;
				Flaskofetherealessence.Bonus10Type = (int) 0;
				Flaskofetherealessence.ExtraBonus = 0;
				Flaskofetherealessence.ExtraBonusType = (int) 0;
				Flaskofetherealessence.Effect = 0;
				Flaskofetherealessence.Emblem = 0;
				Flaskofetherealessence.Charges = 0;
				Flaskofetherealessence.MaxCharges = 0;
				Flaskofetherealessence.SpellID = 0;
				Flaskofetherealessence.ProcSpellID = 0;
				Flaskofetherealessence.Type_Damage = 0;
				Flaskofetherealessence.Realm = 2;
				Flaskofetherealessence.MaxCount = 1;
				Flaskofetherealessence.PackSize = 1;
				Flaskofetherealessence.Extension = 0;
				Flaskofetherealessence.Quality = 99;				
				Flaskofetherealessence.Condition = 100;
				Flaskofetherealessence.MaxCondition = 100;
				Flaskofetherealessence.Durability = 100;
				Flaskofetherealessence.MaxDurability = 100;
				Flaskofetherealessence.PoisonCharges = 0;
				Flaskofetherealessence.PoisonMaxCharges = 0;
				Flaskofetherealessence.PoisonSpellID = 0;
				Flaskofetherealessence.ProcSpellID1 = 0;
				Flaskofetherealessence.SpellID1 = 0;
				Flaskofetherealessence.MaxCharges1 = 0;
				Flaskofetherealessence.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(Flaskofetherealessence);
				}
			

			#endregion

			#region defineAreas
			
		#endregion
		
		#region defineQuestParts

		QuestBuilder builder = QuestMgr.getBuilder(typeof(essenceoflife));
			QuestBehaviour a;
			a = builder.CreateBehaviour(Ballach,-1);
				a.AddTrigger(eTriggerType.Interact,null,Ballach);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.essenceoflife),Ballach);
			a.AddAction(eActionType.Talk,"Greetings Guardian. I have some [business] that needs tended to. There is some coin involved in it for you.",Ballach);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Ballach,-1);
				a.AddTrigger(eTriggerType.Whisper,"business",Ballach);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Midgard.essenceoflife),Ballach);
			a.AddAction(eActionType.Talk,"I am running low on Essence of Life. Do you think you could retrieve some for me?",Ballach);
			a.AddAction(eActionType.OfferQuest,typeof(DOL.GS.Quests.Midgard.essenceoflife),"Will you aid Ballach and retrieve some Essence of Life? [Levels 1-4]");
			AddBehaviour(a);
			a = builder.CreateBehaviour(Ballach,-1);
				a.AddTrigger(eTriggerType.DeclineQuest,null,typeof(DOL.GS.Quests.Midgard.essenceoflife));
			a.AddAction(eActionType.Talk,"No problem. See you.",Ballach);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Ballach,-1);
				a.AddTrigger(eTriggerType.AcceptQuest,null,typeof(DOL.GS.Quests.Midgard.essenceoflife));
			a.AddAction(eActionType.GiveQuest,typeof(DOL.GS.Quests.Midgard.essenceoflife),Ballach);
			a.AddAction(eActionType.Talk,"Thank you very much for offering to help. Here, take this enchanted flask. You will need to find a grave in the Burial Grounds. When you find one, use the flask and capture the essence of life that rests in the buried persons remains.",Ballach);
			a.AddAction(eActionType.GiveItem,enchantedflask,Ballach);
			AddBehaviour(a);
			a = builder.CreateBehaviour(AmbientRatStatua,-1);
				a.AddTrigger(eTriggerType.ItemUsed,null,enchantedflask);
			a.AddRequirement(eRequirementType.Distance,AmbientRatStatua,1000,(eComparator)1);
			a.AddAction(eActionType.Timer,"flask",4000);
			a.AddAction(eActionType.Message,"You fill the flask",(eTextType)1);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Ballach,-1);
				a.AddTrigger(eTriggerType.Timer,"flask",null);
			a.AddAction(eActionType.ReplaceItem,enchantedflask,Flaskofetherealessence);
			a.AddAction(eActionType.IncQuestStep,typeof(DOL.GS.Quests.Midgard.essenceoflife),null);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Ballach,-1);
				a.AddTrigger(eTriggerType.Interact,null,Ballach);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Midgard.essenceoflife),2,(eComparator)3);
			a.AddAction(eActionType.Talk,"Did you collect the essence for me? If so, hand me the flask.",Ballach);
			AddBehaviour(a);
			a = builder.CreateBehaviour(Ballach,-1);
				a.AddTrigger(eTriggerType.GiveItem,Ballach,Flaskofetherealessence);
			a.AddAction(eActionType.Talk,"Thank you for taking on this task for me. You have replenished my supply. Here, take this small monetary reward.",Ballach);
			a.AddAction(eActionType.GiveXP,60,null);
			a.AddAction(eActionType.GiveGold,18,null);
			a.AddAction(eActionType.FinishQuest,typeof(DOL.GS.Quests.Midgard.essenceoflife),null);
			AddBehaviour(a);
			
			#endregion

			// Custom Scriptloaded Code Begin
			
			// Custom Scriptloaded Code End

			Ballach.AddQuestToGive(typeof (essenceoflife));
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{			
				
			// Custom Scriptunloaded Code Begin
			
			// Custom Scriptunloaded Code End

			

			/* If Ballach has not been initialized, then we don't have to remove any
			 * hooks from him ;-)
			 */
			if (Ballach == null)
				return;
			/* Now we remove the possibility to give this quest to players */			
			Ballach.RemoveQuestToGive(typeof (essenceoflife));
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
						return "[Step #1] Take the enchanted flask into the Burial Grounds and find a grave in one of the side chambers off of the tunnel up to Jordheim. USE the enchanted flask to collect the essence of life";
				
					case 2:
						return "[Step #2] Return to Ballach and hand him the flask for your reward.";
				
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
			if (player.IsDoingQuest(typeof (essenceoflife)) != null)
				return true;
				
			// Custom Code Begin
			
			// Custom  Code End
			
		
			if (player.Level > maximumLevel || player.Level < minimumLevel )
				return false;
		
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
