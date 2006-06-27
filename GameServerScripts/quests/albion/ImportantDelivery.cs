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
 * Author:		Gandulf Kohlweiss
 * Date:			
 * Directory: /scripts/quests/albion/
 *
 * Description:
 *  Brief Walkthrough: 
 * 1) Travel to loc=19105,26552 Camelot Hills to speak with Master Frederick 
 * 2) Go to Vuloch, loc=4264,30058 Camelot Hills, and hand him the Ticket to Ludlow from Master Frederick. 
 * 3) Give Apprentice Dunan, loc=48300,45549 Black Mountains, the Sack of Supplies from Master Frederick. 
 * 4) Go to Yaren and hand him the Ticket to Bombard from Apprentice Dunan. 
 * 5) Give Bombard, Crate of Vegetables from Apprentice Dunan, chat with him to receive your reward.
 */

using System;
using System.Reflection;
using DOL.GS.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;
using DOL.GS.Quests;
/* I suggest you declare yourself some namespaces for your quests
 * Like: DOL.GS.Quests.Albion
 *       DOL.GS.Quests.Midgard
 *       DOL.GS.Quests.Hibernia
 * Also this is the name that will show up in the database as QuestName
 * so setting good values here will result in easier to read and cleaner
 * Database Code
 */

namespace DOL.GS.Quests.Albion
{
	/* The first thing we do, is to declare the quest requirement
	* class linked with the new Quest. To do this, we derive 
	* from the abstract class AbstractQuestDescriptor
	*/
	public class ImportantDeliveryDescriptor : AbstractQuestDescriptor
	{
		/* This is the type of the quest class linked with 
		 * this requirement class, you must override the 
		 * base methid like that
		 */
		public override Type LinkedQuestType
		{
			get { return typeof(ImportantDelivery); }
		}

		/* This value is used to retrieves how maximum level needed
		 * to be able to make this quest. Override it only if you need, 
		 * the default value is 50
		 */
		public override int MaxLevel
		{
			get { return 1; }
		}

		public override bool CheckQuestQualification(GamePlayer player)
		{
			if (!BaseFrederickQuest.CheckPartAccessible(player, typeof(ImportantDelivery)))
				return false;

			return base.CheckQuestQualification(player);
		}
	}

	/* The second thing we do, is to declare the class we create
	 * as Quest. We must make it persistant using attributes, to
	 * do this, we derive from the abstract class AbstractQuest
	 */
	[NHibernate.Mapping.Attributes.Subclass(NameType = typeof(ImportantDelivery), ExtendsType = typeof(AbstractQuest))]
	public class ImportantDelivery : BaseFrederickQuest
	{
		/// <summary>
		/// Defines a logger for this class.
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

		protected const string questTitle = "Important Delivery";

		private static GameNPC masterFrederick = null;
		private static GameNPC dunan = null;
		private static GameNPC bombard = null;
		private static GameNPC vuloch = null;
		private static GameNPC yaren = null;

		private static TravelTicketTemplate ticketToLudlow = null;
		private static TravelTicketTemplate ticketToBombard = null;
		private static GenericItemTemplate sackOfSupplies = null;
		private static GenericItemTemplate crateOfVegetables = null;
		private static CloakTemplate recruitsCloak = null;

		/* The following method is called automatically when this quest class
		 * is loaded. You might notice that this method is the same as in standard
		 * game events. And yes, quests basically are game events for single players
		 * 
		 * To make this method automatically load, we have to declare it static
		 * and give it the [ScriptLoadedEvent] attribute. 
		 *
		 * Inside this method we initialize the quest. This is neccessary if we 
		 * want to set the quest hooks to the NPCs.
		 * 
		 * If you want, you can however add a quest to the player from ANY place
		 * inside your code, from events, from custom items, from anywhere you
		 * want. 
		 */

		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initializing ...");
			/* First thing we do in here is to search for the NPCs inside
			* the world who comes from the certain Realm. If we find a the players,
			* this means we don't have to create a new one.
			* 
			* NOTE: You can do anything you want in this method, you don't have
			* to search for NPC's ... you could create a custom item, place it
			* on the ground and if a player picks it up, he will get the quest!
			* Just examples, do anything you like and feel comfortable with :)
			*/

			#region defineNPCs

			masterFrederick = GetMasterFrederick();

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Apprentice Dunan", eRealm.Albion);
			if (npcs.Length == 0)
			{
				dunan = new GameMob();
				dunan.Model = 49;
				dunan.Name = "Apprentice Dunan";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + dunan.Name + ", creating ...");
				dunan.GuildName = "Part of " + questTitle + " Quest";
				dunan.Realm = (byte) eRealm.Albion;
				dunan.RegionId = 1;
				dunan.Size = 49;
				dunan.Level = 21;
				dunan.Position = new Point(531663, 479785, 2200);
				dunan.Heading = 1579;
				dunan.EquipmentTemplateID = "1707754";
				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					dunan.SaveIntoDatabase();
				dunan.AddToWorld();
			}
			else
				dunan = npcs[0];

			npcs = WorldMgr.GetNPCsByName("Bombard", eRealm.Albion);
			if (npcs.Length == 0)
			{
				bombard = new GameStableMaster();
				bombard.Model = 8;
				bombard.Name = "Bombard";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + bombard.Name + ", creating ...");
				bombard.GuildName = "Stable Master";
				bombard.Realm = (byte) eRealm.Albion;
				bombard.RegionId = 1;
				bombard.Size = 49;
				bombard.Level = 4;
				bombard.Position = new Point(515718, 496739, 3352);
				bombard.Heading = 2500;

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					bombard.SaveIntoDatabase();
				bombard.AddToWorld();
			}
			else
				bombard = npcs[0];

			npcs = WorldMgr.GetNPCsByName("Vuloch", eRealm.Albion);
			if (npcs.Length == 0)
			{
				vuloch = new GameStableMaster();
				vuloch.Model = 86;
				vuloch.Name = "Vuloch";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + vuloch.Name + ", creating ...");
				vuloch.GuildName = "Stable Master";
				vuloch.Realm = (byte) eRealm.Albion;
				vuloch.RegionId = 1;
				vuloch.Size = 50;
				vuloch.Level = 4;
				vuloch.Position = new Point(553089, 513380, 2896);
				vuloch.Heading = 2139;

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					vuloch.SaveIntoDatabase();

				vuloch.AddToWorld();
			}
			else
				vuloch = npcs[0];

			npcs = WorldMgr.GetNPCsByName("Yaren", eRealm.Albion);
			if (npcs.Length == 0)
			{
				yaren = new GameStableMaster();
				yaren.Model = 79;
				yaren.Name = "Yaren";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + yaren.Name + ", creating ...");
				yaren.GuildName = "Stable Master";
				yaren.Realm = (byte) eRealm.Albion;
				yaren.RegionId = 1;
				yaren.Size = 48;
				yaren.Level = 4;
				yaren.Position = new Point(529638, 478091, 2200);
				yaren.Heading = 3160;
				yaren.EquipmentTemplateID = "11701347";

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					yaren.SaveIntoDatabase();
				yaren.AddToWorld();
			}
			else
				yaren = npcs[0];

			#endregion

			#region defineItems

			ticketToLudlow = CreateTicketTo("Ludlow");
			ticketToBombard = CreateTicketTo("North Camelot Gates");


            sackOfSupplies = (GenericItemTemplate) GameServer.Database.FindObjectByKey(typeof (GenericItemTemplate), "sack_of_supplies");
            if (sackOfSupplies == null)
            {
	            sackOfSupplies = new GenericItemTemplate();
	            sackOfSupplies.Name = "Sack of Supplies";
	            if (log.IsWarnEnabled)
		            log.Warn("Could not find " + sackOfSupplies.Name + " , creating it ...");

	            sackOfSupplies.Weight = 10;
	            sackOfSupplies.Model = 488;

	            sackOfSupplies.ItemTemplateID = "sack_of_supplies";

				sackOfSupplies.IsDropable = false;
				sackOfSupplies.IsSaleable = false;
				sackOfSupplies.IsTradable = false;

	            //You don't have to store the created item in the db if you don't want,
	            //it will be recreated each time it is not found, just comment the following
	            //line if you rather not modify your database
	            if (SAVE_INTO_DATABASE)
		            GameServer.Database.AddNewObject(sackOfSupplies);
            }

			crateOfVegetables = (GenericItemTemplate) GameServer.Database.FindObjectByKey(typeof (GenericItemTemplate), "crate_of_vegetables");
			if (crateOfVegetables == null)
			{
				crateOfVegetables = new GenericItemTemplate();
				crateOfVegetables.Name = "Crate of Vegetables";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + crateOfVegetables.Name + " , creating it ...");

				crateOfVegetables.Weight = 15;
				crateOfVegetables.Model = 602;

				crateOfVegetables.ItemTemplateID = "crate_of_vegetables";

				crateOfVegetables.IsDropable = false;
				crateOfVegetables.IsSaleable = false;
				crateOfVegetables.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(crateOfVegetables);
			}

			// item db check
			recruitsCloak = (CloakTemplate) GameServer.Database.FindObjectByKey(typeof (CloakTemplate), "recruits_cloak");
			if (recruitsCloak == null)
			{
				recruitsCloak = new CloakTemplate();
				recruitsCloak.Name = "Recruit's Cloak";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsCloak.Name + ", creating it ...");
				recruitsCloak.Level = 3;

				recruitsCloak.Weight = 3;
				recruitsCloak.Model = 57; // studded Boots                

				recruitsCloak.ItemTemplateID = "recruits_cloak";
				recruitsCloak.Value = 100;

				recruitsCloak.IsDropable = true;
				recruitsCloak.IsSaleable = true;
				recruitsCloak.IsTradable = true;
				recruitsCloak.Color = 36;

				recruitsCloak.Bonus = 1; // default bonus

				recruitsCloak.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 1));
				recruitsCloak.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Slash, 1));

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsCloak);
			}

			#endregion

			/*
            #region defineConversation
            QuestBuilder builder = QuestMgr.getBuilder(typeof(ImportantDelivery));

            BaseQuestPart a;

            // Master Frederick
            builder.AddInteraction(masterFrederick, -1, eTextType.Talk, null, "Greetings to you my young friend. I am Master Frederick. I'm here to help you find your way around this vast realm. In the process, you'll have the ability to earn weapons, armor, coin and some levels. Would you like to start [training] now?");
            builder.AddInteraction(masterFrederick, -1, eTextType.Talk, "training", "I thought you would. What I am here to do is to guide you through your first few seasons, until I feel you're confident and skilled enough to make it on your own in Albion. If you aren't properly trained, then what good are you to the realm? None, of course. Now, I will start your training off by asking you a simple quesion, whether or not you wish to [proceed] with your training. A dialogue box will pop up. Either press the Accept or Decline button.");

            // Offer quest to player
            a = builder.AddInteraction(masterFrederick,-1,eTextType.None,"proceed",null);
                a.AddAction(eActionType.OfferQuest, typeof(ImportantDelivery), "Are you ready to begin your training?");            
            builder.AddOnQuestDecline(masterFrederick, eTextType.Talk, "Oh well, if you change your mind, please come back!");                            
            builder.AddOnQuestAccept(masterFrederick, eTextType.Talk, "Congratulations! You are now one step closer to understanding the world of Camelot! During this phase of your training, I will be sending you to different parts of the realm to deliver much needed supplies to various citizens. You will need to check your QUEST JOURNAL from time to time to see what you need to accomplish next on your quest. You can access the quest journal from the COMMAND button on your [character sheet].");                
                            
            // do some smalltalk
            builder.AddInteraction(masterFrederick, 1, eTextType.Talk, "character sheet", "Your character sheet houses all of your character's information, such as attributes, weapon skill, base class and profession. If at any time you want to see your character's statistics, press the far left icon on the menu bar (it looks like a person with a circle around them) for more [information].");
            a = builder.AddInteraction(masterFrederick, 1, eTextType.Talk, "information", "I know this all seems a little overwhelming, but I have a special item here that will make this transition a smooth one. Please, take this [journal].");
                a.AddAction(eActionType.IncQuestStep,typeof(ImportantDelivery));
            
            builder.AddInteraction(masterFrederick, 2, eTextType.Talk, "journal", "This journal will help you from time to time while you are doing various tasks for me. I like to call it a smart journal. It was made by one of the sorcerers at the Academy for new recruits like you. It will help to [expedite] your training.");
            builder.AddInteraction(masterFrederick, 2, eTextType.Talk, "expedite", "Now that I've given you a small introduction to the world of Albion, let's get started with your first task. I need for you to deliver this package of supplies to Apprentice Dunan in Ludlow. Don't worry, I have a special [horse ticket] for you.");

            // give player items for dunan and ticket
            a = builder.AddInteraction(masterFrederick, 2, eTextType.Talk, "horse ticket", "All you need to do is take this horse ticket to Vuloch near the gates of Camelot. Hand him the ticket and you'll be on your way to Ludlow. Be swift my young recruit. Time is of the essence.");
                a.AddAction(eActionType.GiveItem,ticketToLudlow);
                a.AddAction(eActionType.GiveItem,sackOfSupplies);
                a.AddAction(eActionType.IncQuestStep,typeof(ImportantDelivery));

            builder.AddInteraction(masterFrederick, 2, eTextType.Talk, null, "This journal will help you from time to time while you are doing various tasks for me. I like to call it a smart journal. It was made by one of the sorcerers at the Academy for new recruits like you. It will help to [expedite] your training.");
            builder.AddInteraction(masterFrederick, 3, eTextType.Talk, null, "All you need to do is take this horse ticket to Vuloch near the gates of Camelot. Hand him the ticket and you'll be on your way to Ludlow. Be swift my young recruit. Time is of the essence.");
            
            // Add Abort Possibility for quest            
            a = builder.AddInteraction(masterFrederick, 0, eTextType.None, "abort", null);                
                a.AddAction(eActionType.OfferQuestAbort,typeof(ImportantDelivery), "Do you really want to abort this quest, \nall items gained during quest will be lost?");                        
            builder.AddOnQuestContinue(masterFrederick, eTextType.Talk, "Good, no go out there and finish your work!");
            builder.AddOnQuestAbort(masterFrederick, eTextType.Talk, "Do you really want to abandon your duties? The realm always needs willing men and women, so come back if you changed your mind.");

            // Goto vuloch and give him the ticket
            a = builder.AddOnGiveItem(vuloch, 3, ticketToLudlow, eTextType.Emote, "You give Vuloch the item");
                a.AddAction(eActionType.IncQuestStep, typeof(ImportantDelivery));
    
            // Talk to Dunan 
            builder.AddInteraction(dunan, 3,5, eTextType.Talk, null, "Greetings traveler. I've not seen you around here before. You must be a new recruit. Well then, is there something I can help you with?");                

            // Give supplies to dunan
            a = builder.AddOnGiveItem(dunan, 3, 4, sackOfSupplies, eTextType.Talk, "Oh, I see. Yes, from Master Frederick. We've been waiting for these supplies for a while. It's good to have them. I don't suppose you're up for one more [errand], are you?");
                a.AddAction(eActionType.SetQuestStep, typeof(ImportantDelivery), 5);
                a.AddAction(eActionType.TakeItem, sackOfSupplies);

            a = builder.CreateQuestPart(dunan, eTextType.Talk, "Oh, I see. Yes, from Master Frederick. We've been waiting for these supplies for a while. It's good to have them. I don't suppose you're up for one more [errand], are you?");
                a.AddRequirement(eRequirementType.QuestStep, typeof(ImportantDelivery), 2, eComparator.Greater);
                a.AddRequirement(eRequirementType.QuestStep, typeof(ImportantDelivery), 5, eComparator.Less);
                a.AddTrigger(eTriggerType.GiveItem,null,sackOfSupplies);
                a.AddAction(eActionType.SetQuestStep, typeof(ImportantDelivery), 5);
                a.AddAction(eActionType.TakeItem, sackOfSupplies);
            AddQuestPart(a);

            builder.AddInteraction(dunan, 5, eTextType.Talk, null, "Oh, I see. Yes, from Master Frederick. We've been waiting for these supplies for a while. It's good to have them. I don't suppose you're up for one more [errand], are you?");
            
            // More work to do, give player items for bombard and ticket 
            a = builder.AddInteraction(dunan, 5, eTextType.Talk, "errand", "I need for you to deliver this crate of vegetables to Stable Master Bombard at the Camelot Gates. Don't worry, I'll give you a ticket so you don't have to run there. Thank you my friend. Be swift so the vegetables don't rot.");
                a.AddAction(eActionType.GiveItem,ticketToBombard);
                a.AddAction(eActionType.GiveItem,crateOfVegetables);
                a.AddAction(eActionType.IncQuestStep,typeof(ImportantDelivery));

            // Goto Yaren and give him the ticket
            a = builder.AddOnGiveItem(yaren, 6, ticketToBombard, eTextType.None, null);
                a.AddAction(eActionType.IncQuestStep, typeof(ImportantDelivery));

            // Bombard
            builder.AddInteraction(bombard, 6,7, eTextType.Talk, null, "Welcome to my stable friend. What can I do for you today?");                      
            
            // Give crate to bombard
            a = builder.AddOnGiveItem(bombard,6,7,crateOfVegetables, eTextType.Talk, "Ah, the vegetables I've been waiting for from Dunan. Thank you for delivering them to me. I couldn't find anyone to look after my stable so I could go and get them. Let me see, I think a [reward] is in order for your hard work.");                
                a.AddAction(eActionType.SetQuestStep, typeof(ImportantDelivery), 8);
                a.AddAction(eActionType.TakeItem, crateOfVegetables);

            // Finish Quest and recive reward
            a = builder.AddInteraction(bombard, 8, eTextType.Talk, "reward", "Ah, here we are. I know it isn't much, but I got it in a trade a while ago, and I don't have much use for it. I'm sure you can put it to use though, can't you? Let me know if you're in need of anything else. I have a few errands I need run.");
                a.AddAction(eActionType.FinishQuest, typeof(ImportantDelivery));
            a = builder.AddInteraction(bombard, 8, eTextType.Talk, null, "Ah, here we are. I know it isn't much, but I got it in a trade a while ago, and I don't have much use for it. I'm sure you can put it to use though, can't you? Let me know if you're in need of anything else. I have a few errands I need run.");
                a.AddAction(eActionType.FinishQuest, typeof(ImportantDelivery));
            
            #endregion

			*/

			/* Now we bring to Ydenia the possibility to give this quest to players */
			QuestMgr.AddQuestDescriptor(masterFrederick, typeof(ImportantDeliveryDescriptor));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");

		}

		/* The following method is called automatically when this quest class
		 * is unloaded. 
		 * 
		 * Since we set hooks in the load method, it is good practice to remove
		 * those hooks again!
		 */

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
            //remove quest from npc
			QuestMgr.RemoveQuestDescriptor(masterFrederick, typeof(ImportantDeliveryDescriptor));
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
						return "[Step #1] Listen to Master Frederick describe your training. If he stops speaking with you, ask him about you [character sheet].";
					case 2:
						return "[Step #2] Listen to Master Frederick as he explains your journal. If he stops speaking with you, ask him how he can [expedite] your journeys.";
					case 3:
						return "[Step #3] Find Stable Master Vuloch near the Camelot Gates. Make your way west-southwest towards Camelot. If you forget your directions, you can always speak with Master Frederick again.";
					case 4:
						return "[Step #4] Hand your Sack of Supplies to Apprentice Dunan just as you did with the horse ticket for Vuloch.";
					case 5:
						return "[Step #5] Listen to what Dunan has to say. If he stops speaking with you, ask him if there is an [errand] he needs you to run for him.";
					case 6:
						return "[Step #6] Take the Ticket Dunan gave you to Stable Master Yaren at the northwest end of Ludlow. Hand him the ticket so you can get to Stable Master Bombard.";
					case 7:
						return "[Step #7] Now that you are in Bombard's stable, you must hand him the Crate of Vegetables. If you prefer, you may right click to interact with him first.";
					case 8:
						return "[Step #8] Talk to Bombard to recieve your reward for your hard work.";
					default:
						return "[Step #" + Step + "] No Description entered for this step!";
				}
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			//Give reward to player here ...            
			GiveItemToPlayer(bombard, recruitsCloak.CreateInstance());

			m_questPlayer.GainExperience(12, 0, 0, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 1, Util.Random(50)), "You recieve {0} as a reward.");
		}
	}
}
