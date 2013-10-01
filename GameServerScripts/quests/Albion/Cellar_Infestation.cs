	
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

	namespace DOL.GS.Quests.Albion {
	
     /* The first thing we do, is to declare the class we create
	 * as Quest. To do this, we derive from the abstract class
	 * BaseQuest	  	 
	 */
	public class Cellarinfestation : BaseQuest
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

		protected const string questTitle = "Cellar Infestation";

		protected const int minimumLevel = 1;
		protected const int maximumLevel = 4;
	
	
		private static GameNPC MistressLaws = null;
		
		private static GameNPC YlaineBarrett = null;
		
		private static ItemTemplate slimyswampgooskin = null;
		

		// Custom Initialization Code Begin
		
		// Custom Initialization Code End

		/* 
		* Constructor
		*/
		public Cellarinfestation() : base()
		{
		}

		public Cellarinfestation(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public Cellarinfestation(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public Cellarinfestation(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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
	
			npcs = WorldMgr.GetNPCsByName("Mistress Laws",(eRealm) 1);
			if (npcs.Length == 0)
			{
				if (!WorldMgr.GetRegion(51).IsDisabled)
				{
				MistressLaws = new DOL.GS.GameNPC();
					MistressLaws.Model = 68;
				MistressLaws.Name = "Mistress Laws";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + MistressLaws.Name + ", creating ...");
				MistressLaws.GuildName = "Part of " + questTitle + " Quest";
				MistressLaws.Realm = eRealm.Albion;
				MistressLaws.CurrentRegionID = 51;
				MistressLaws.Size = 52;
				MistressLaws.Level = 40;
				MistressLaws.MaxSpeedBase = 191;
				MistressLaws.Faction = FactionMgr.GetFactionByID(0);
				MistressLaws.X = 536859;
				MistressLaws.Y = 548403;
				MistressLaws.Z = 4800;
				MistressLaws.Heading = 1035;
				MistressLaws.RespawnInterval = -1;
				MistressLaws.BodyType = 0;
				

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 500;
				MistressLaws.SetOwnBrain(brain);
				
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					MistressLaws.SaveIntoDatabase();
					
				MistressLaws.AddToWorld();
				
				}
			}
			else 
			{
				MistressLaws = npcs[0];
			}
		
			npcs = WorldMgr.GetNPCsByName("Ylaine Barrett",(eRealm) 1);
			if (npcs.Length == 0)
			{
				if (!WorldMgr.GetRegion(51).IsDisabled)
				{
				YlaineBarrett = new DOL.GS.GameMerchant();
					YlaineBarrett.Model = 87;
				YlaineBarrett.Name = "Ylaine Barrett";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + YlaineBarrett.Name + ", creating ...");
				YlaineBarrett.GuildName = "Part of " + questTitle + " Quest";
				YlaineBarrett.Realm = eRealm.Albion;
				YlaineBarrett.CurrentRegionID = 51;
				YlaineBarrett.Size = 50;
				YlaineBarrett.Level = 40;
				YlaineBarrett.MaxSpeedBase = 191;
				YlaineBarrett.Faction = FactionMgr.GetFactionByID(0);
				YlaineBarrett.X = 522790;
				YlaineBarrett.Y = 542142;
				YlaineBarrett.Z = 3230;
				YlaineBarrett.Heading = 1661;
				YlaineBarrett.RespawnInterval = -1;
				YlaineBarrett.BodyType = 0;
				

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 500;
				YlaineBarrett.SetOwnBrain(brain);
				
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					YlaineBarrett.SaveIntoDatabase();
					
				YlaineBarrett.AddToWorld();
				
				}
			}
			else 
			{
				YlaineBarrett = npcs[0];
			}
		

			#endregion

			#region defineItems

		slimyswampgooskin = GameServer.Database.FindObjectByKey<ItemTemplate>("slimyswampgooskin");
			if (slimyswampgooskin == null)
			{
				slimyswampgooskin = new ItemTemplate();
				slimyswampgooskin.Name = "Slimy Swamp Goo Skin";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + slimyswampgooskin.Name + ", creating it ...");
				slimyswampgooskin.Level = 1;
				slimyswampgooskin.Weight = 1;
				slimyswampgooskin.Model = 100;
				slimyswampgooskin.Object_Type = 0;
				slimyswampgooskin.Item_Type = -1;
				slimyswampgooskin.Id_nb = "slimyswampgooskin";
				slimyswampgooskin.Hand = 0;
				slimyswampgooskin.Price = 0;
				slimyswampgooskin.IsPickable = true;
				slimyswampgooskin.IsDropable = true;
				slimyswampgooskin.IsTradable = true;
				slimyswampgooskin.CanDropAsLoot = false;
				slimyswampgooskin.Color = 0;
				slimyswampgooskin.Bonus = 0; // default bonus				
				slimyswampgooskin.Bonus1 = 0;
				slimyswampgooskin.Bonus1Type = (int) 0;
				slimyswampgooskin.Bonus2 = 0;
				slimyswampgooskin.Bonus2Type = (int) 0;
				slimyswampgooskin.Bonus3 = 0;
				slimyswampgooskin.Bonus3Type = (int) 0;
				slimyswampgooskin.Bonus4 = 0;
				slimyswampgooskin.Bonus4Type = (int) 0;
				slimyswampgooskin.Bonus5 = 0;
				slimyswampgooskin.Bonus5Type = (int) 0;
				slimyswampgooskin.Bonus6 = 0;
				slimyswampgooskin.Bonus6Type = (int) 0;
				slimyswampgooskin.Bonus7 = 0;
				slimyswampgooskin.Bonus7Type = (int) 0;
				slimyswampgooskin.Bonus8 = 0;
				slimyswampgooskin.Bonus8Type = (int) 0;
				slimyswampgooskin.Bonus9 = 0;
				slimyswampgooskin.Bonus9Type = (int) 0;
				slimyswampgooskin.Bonus10 = 0;
				slimyswampgooskin.Bonus10Type = (int) 0;
				slimyswampgooskin.ExtraBonus = 0;
				slimyswampgooskin.ExtraBonusType = (int) 0;
				slimyswampgooskin.Effect = 0;
				slimyswampgooskin.Emblem = 0;
				slimyswampgooskin.Charges = 0;
				slimyswampgooskin.MaxCharges = 0;
				slimyswampgooskin.SpellID = 0;
				slimyswampgooskin.ProcSpellID = 0;
				slimyswampgooskin.Type_Damage = 0;
				slimyswampgooskin.Realm = 0;
				slimyswampgooskin.MaxCount = 1;
				slimyswampgooskin.PackSize = 1;
				slimyswampgooskin.Extension = 0;
				slimyswampgooskin.Quality = 100;				
				slimyswampgooskin.Condition = 100;
				slimyswampgooskin.MaxCondition = 100;
				slimyswampgooskin.Durability = 100;
				slimyswampgooskin.MaxDurability = 100;
				slimyswampgooskin.PoisonCharges = 0;
				slimyswampgooskin.PoisonMaxCharges = 0;
				slimyswampgooskin.PoisonSpellID = 0;
				slimyswampgooskin.ProcSpellID1 = 0;
				slimyswampgooskin.SpellID1 = 0;
				slimyswampgooskin.MaxCharges1 = 0;
				slimyswampgooskin.Charges1 = 0;
				
				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				
					GameServer.Database.AddObject(slimyswampgooskin);
				}
			

			#endregion

			#region defineAreas
			
		#endregion
		
		#region defineQuestParts

		QuestBuilder builder = QuestMgr.getBuilder(typeof(Cellarinfestation));
			QuestBehaviour a;
			a = builder.CreateBehaviour(MistressLaws,-1);
				a.AddTrigger(eTriggerType.Interact,null,MistressLaws);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Albion.Cellarinfestation),MistressLaws);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Albion.Cellarinfestation),null,(eComparator)5);
			a.AddAction(eActionType.Talk,"You have barely begun training; yet already there is a need for you to lend your strength for the protection of this town. I know some of the locals are suspicious of the Inconnu, well, that is not to be helped. They are a mysterious people, and their sudden appearance one stormy night, well, that causes a large amount of [suspicion], as you can imagine, They are here to help us, I do believe that, and so must you. But some of the locals, they are not so sure.",MistressLaws);
			AddBehaviour(a);
			a = builder.CreateBehaviour(MistressLaws,-1);
				a.AddTrigger(eTriggerType.Whisper,"suspicion",MistressLaws);
			a.AddRequirement(eRequirementType.QuestGivable,typeof(DOL.GS.Quests.Albion.Cellarinfestation),MistressLaws);
			a.AddRequirement(eRequirementType.QuestPending,typeof(DOL.GS.Quests.Albion.Cellarinfestation),null,(eComparator)5);
			a.AddAction(eActionType.Talk,"They have seen the Drakoran ruin their lands, they have witnessed Morgana's terrible magic first hand, so they cannot be expected to trust these new, powerful allies. I expect you to treat the Inconnu as allies, and to try and avoid the idle gossip of the local townsfolk. Saying that, I must send you to one of the loudest of the rabble. Ylaine Barrett. The food cellar has been swarming with swamp goo, and she wants someone to go out and clear the immediate area of swamp goo. Don't worry, I don't expect you to kill them all, just do what you can.",MistressLaws);
			a.AddAction(eActionType.OfferQuest,typeof(DOL.GS.Quests.Albion.Cellarinfestation),"Will you aid Ylaine Barrett in her attempt to rid the cellars of swamp goo?");
			AddBehaviour(a);
			a = builder.CreateBehaviour(MistressLaws,-1);
				a.AddTrigger(eTriggerType.DeclineQuest,null,typeof(DOL.GS.Quests.Albion.Cellarinfestation));
			a.AddAction(eActionType.Talk,"No problem. See you.",MistressLaws);
			AddBehaviour(a);
			a = builder.CreateBehaviour(MistressLaws,-1);
				a.AddTrigger(eTriggerType.AcceptQuest,null,typeof(DOL.GS.Quests.Albion.Cellarinfestation));
			a.AddAction(eActionType.Talk,"Right, I expected you would do so. Now, go talk to Ylaine Barrett.",MistressLaws);
			a.AddAction(eActionType.GiveQuest,typeof(DOL.GS.Quests.Albion.Cellarinfestation),MistressLaws);
			AddBehaviour(a);
			a = builder.CreateBehaviour(YlaineBarrett,-1);
				a.AddTrigger(eTriggerType.Interact,null,YlaineBarrett);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Albion.Cellarinfestation),1,(eComparator)3);
			a.AddAction(eActionType.Talk,"Ah. G'day. And who might you be?",YlaineBarrett);
			a.AddAction(eActionType.Message,"Tell Ylaine Barrett your name.",(eTextType)2);
			a.AddAction(eActionType.Talk,"I see, you're a fighter. Has Mistress Laws, Master Reginald, or Delore sent you here to help me out? Why don't they just send you out after the goo? That's all I really want. You know, I don't remember that there were that many of these strange creatures crawling around the village [before].",YlaineBarrett);
			AddBehaviour(a);
			a = builder.CreateBehaviour(YlaineBarrett,-1);
				a.AddTrigger(eTriggerType.Whisper,"before",YlaineBarrett);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Albion.Cellarinfestation),1,(eComparator)3);
			a.AddAction(eActionType.Talk,"Yes, well, before, well, before they came. Don't you remember? That night? All day long, everything's fine. Set itself uop for a fine next day, as well! The night, it was clear. Not a sign of a storm in sight! Why, no one prepared for it! Then, in the darkest part, suddenly, the storm starts! We were woken from the noise of it! It was awful, but we didn't dare go outside! Some of the men, they did, but they came back pale as a corpse! Were shaken to their core! And some of these being the fightin' men! Best of the town! Aye, but the [swamp goo].",YlaineBarrett);
			AddBehaviour(a);
			a = builder.CreateBehaviour(YlaineBarrett,-1);
				a.AddTrigger(eTriggerType.Whisper,"swamp goo",YlaineBarrett);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Albion.Cellarinfestation),1,(eComparator)3);
			a.AddAction(eActionType.Talk,"They've gotten into the food cellar! The slimy things were creeping all over that place! I've gotten rid of most of the swamp goo, but I know there's more just outside town working their way to my food stores. I think you could do a small favor for us all and get rid of some. Oh and while you are at it, the skin of the swamp goo, I've some uses for it. I'll pay you for a swamp goo skin. Just go and get me one of their skins, and I'll be satisfied with ya.",YlaineBarrett);
			a.AddAction(eActionType.IncQuestStep,typeof(DOL.GS.Quests.Albion.Cellarinfestation),null);
			AddBehaviour(a);
			a = builder.CreateBehaviour(MistressLaws,-1);
				a.AddTrigger(eTriggerType.EnemyKilled,"swamp goo",null);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Albion.Cellarinfestation),2,(eComparator)3);
			a.AddAction(eActionType.GiveItem,slimyswampgooskin,null);
			a.AddAction(eActionType.IncQuestStep,typeof(DOL.GS.Quests.Albion.Cellarinfestation),null);
			AddBehaviour(a);
			a = builder.CreateBehaviour(YlaineBarrett,-1);
				a.AddTrigger(eTriggerType.Interact,null,YlaineBarrett);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Albion.Cellarinfestation),3,(eComparator)3);
			a.AddAction(eActionType.Talk,"Oh, you're back! Did you see how many there were? They are a nuisance, aren't they! Well, have you got me the skin?",YlaineBarrett);
			AddBehaviour(a);
			a = builder.CreateBehaviour(YlaineBarrett,-1);
				a.AddTrigger(eTriggerType.GiveItem,YlaineBarrett,slimyswampgooskin);
			a.AddRequirement(eRequirementType.QuestStep,typeof(DOL.GS.Quests.Albion.Cellarinfestation),3,(eComparator)3);
			a.AddAction(eActionType.Talk,"Ah, a good and slimy one! It's odd, these creatures, they've got some strangeness about them, but they've got their uses, as well. Thank you, then. Here, I promised I'd pay ya, and I will!",YlaineBarrett);
			a.AddAction(eActionType.GiveGold,330,null);
			a.AddAction(eActionType.GiveXP,11,null);
			a.AddAction(eActionType.FinishQuest,typeof(DOL.GS.Quests.Albion.Cellarinfestation),null);
			AddBehaviour(a);
			
			#endregion

			// Custom Scriptloaded Code Begin
			
			// Custom Scriptloaded Code End
			if (MistressLaws!=null) {
				MistressLaws.AddQuestToGive(typeof (Cellarinfestation));
			}
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{			
				
			// Custom Scriptunloaded Code Begin
			
			// Custom Scriptunloaded Code End

			

			/* If MistressLaws has not been initialized, then we don't have to remove any
			 * hooks from him ;-)
			 */
			if (MistressLaws == null)
				return;
			/* Now we remove the possibility to give this quest to players */			
			MistressLaws.RemoveQuestToGive(typeof (Cellarinfestation));
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
						return "[Step #1] Speak to Ylaine Barrett about the swamp goo in the cellar. Be sure to let her know your name when she asks. She's in Gothwaite Harbor.";
				
					case 2:
						return "[Step #2] Hunt the swamp goo, and return a skin to Ylaine.";
				
					case 3:
						return "[Step #3] Return the skin to Ylaine Barrett.";
				
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
			if (player.IsDoingQuest(typeof (Cellarinfestation)) != null)
				return true;
				
			// Custom Code Begin
			
			// Custom  Code End
			
		
			if (player.Level > maximumLevel || player.Level < minimumLevel )
				return false;
		
			if (
		
			player.CharacterClass.ID != (byte) eCharacterClass.Acolyte && 
			player.CharacterClass.ID != (byte) eCharacterClass.AlbionRogue && 
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
