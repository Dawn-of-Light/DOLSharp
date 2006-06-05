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
 *  1) Travel to loc=19105,26552 Camelot Hills to speak with Master Frederick. 
 *  2) Go to loc=12336,22623 Camelot Hills and /use the Magical Wooden Box on the Ire Fairies when they appear. 
 *  3) Take the Full Magical Wooden Box back to Master Frederick for your reward.
 */

using System;
using System.Reflection;
using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;
using log4net;
using NHibernate.Expression;
using NHibernate.Mapping.Attributes;
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
	public class NuisancesDescriptor : AbstractQuestDescriptor
	{
		/* This is the type of the quest class linked with 
		 * this requirement class, you must override the 
		 * base methid like that
		 */
		public override Type LinkedQuestType
		{
			get { return typeof(Nuisances); }
		}

		/* This value is used to retrieves the minimum level needed
		 *  to be able to make this quest. Override it only if you need, 
		 * the default value is 1
		 */
		public override int MinLevel
		{
			get { return 2; }
		}

		/* This value is used to retrieves how maximum level needed
		 * to be able to make this quest. Override it only if you need, 
		 * the default value is 50
		 */
		public override int MaxLevel
		{
			get { return 2; }
		}

		public override bool CheckQuestQualification(GamePlayer player)
		{
			// if the player is already doing the quest always return true !!!
			if (player.IsDoingQuest(LinkedQuestType) != null)
				return true;

			if (!BaseFrederickQuest.CheckPartAccessible(player, LinkedQuestType))
				return false;

			return base.CheckQuestQualification(player);
		}
	}

	/* The second thing we do, is to declare the class we create
	 * as Quest. We must make it persistant using attributes, to
	 * do this, we derive from the abstract class AbstractQuest
	 */
	[Subclass(NameType = typeof(Nuisances), ExtendsType = typeof(AbstractQuest))]
	public class Nuisances : BaseFrederickQuest
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
		protected const string questTitle = "Nuisances";

		private static GameMob masterFrederick = null;
		private static GameMob ireFairy = null;

		private static GameLocation fairyLocation = new GameLocation("Ire Fairy", 1, 561200, 505951, 2405);

		private static IArea fairyArea = null;

		private static GenericItemTemplate emptyMagicBox = null;
		private static GenericItemTemplate fullMagicBox = null;
		private static SwordTemplate recruitsShortSword = null;
		private static StaffTemplate recruitsStaff = null;

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

			#region defineNPCS

			masterFrederick = GetMasterFrederick();
			if(masterFrederick == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
				return;
			}

			ireFairy = ResearchQuestObject(typeof(GameMob), WorldMgr.GetRegion(1), eRealm.None, "Ire Fairy") as GameMob;
			if (ireFairy == null)
            {
                ireFairy = new GameMob();
                ireFairy.Model = 603;
                ireFairy.Name = "Ire Fairy";
                ireFairy.GuildName = "Part of " + questTitle + " Quest";
                ireFairy.Realm = (byte)eRealm.None;
                ireFairy.Region = WorldMgr.GetRegion(1);
                ireFairy.Size = 50;
                ireFairy.Level = 4;
				Point pos = new Point(561200 + Util.Random(-150, 150), 505951 + Util.Random(-150, 150), 2405);
            	ireFairy.Position = pos;
                ireFairy.Heading = 226;

                StandardMobBrain brain = new StandardMobBrain();
				brain.Body = ireFairy;
                brain.AggroLevel = 40;
                brain.AggroRange = 200;
                ireFairy.OwnBrain = brain;

				if(!ireFairy.AddToWorld())
				{
					if (log.IsWarnEnabled)
						log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
					return;
				}

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(ireFairy);
			}
            
			#endregion

			#region defineItems

			// item db check
			emptyMagicBox = GameServer.Database.SelectObject(typeof (GenericItemTemplate), Expression.Eq("Name", "Empty Wodden Magic Box")) as GenericItemTemplate;
			if (emptyMagicBox == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Empty Wodden Magic Box, creating it ...");
				emptyMagicBox = new GenericItemTemplate();
				emptyMagicBox.Name = "Empty Wodden Magic Box";

				emptyMagicBox.Weight = 5;
				emptyMagicBox.Model = 602;

				emptyMagicBox.IsDropable = false;
				emptyMagicBox.IsSaleable = false;
				emptyMagicBox.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(emptyMagicBox);
			}

			// item db check
			fullMagicBox = GameServer.Database.SelectObject(typeof (GenericItemTemplate), Expression.Eq("Name", "Full Wodden Magic Box")) as GenericItemTemplate;
			if (fullMagicBox == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Full Wodden Magic Box, creating it ...");
				fullMagicBox = new GenericItemTemplate();
				fullMagicBox.Name = "Full Wodden Magic Box";

				fullMagicBox.Weight = 3;
				fullMagicBox.Model = 602;

				fullMagicBox.IsDropable = false;
				fullMagicBox.IsSaleable = false;
				fullMagicBox.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(fullMagicBox);
			}

			// item db check
			recruitsShortSword = GameServer.Database.SelectObject(typeof (SwordTemplate), Expression.Eq("Name", "Recruit's Short Sword")) as SwordTemplate;
			if (recruitsShortSword == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Recruit's Short Sword, creating it ...");
				recruitsShortSword = new SwordTemplate();
				recruitsShortSword.Name = "Recruit's Short Sword";
				recruitsShortSword.Level = 4;

				recruitsShortSword.Weight = 18;
				recruitsShortSword.Model = 3; // studded Boots

				recruitsShortSword.DamagePerSecond = 23;
				recruitsShortSword.Speed = 3000;
				recruitsShortSword.DamageType = eDamageType.Slash;
				recruitsShortSword.HandNeeded = eHandNeeded.RightHand;
				recruitsShortSword.Value = 200;

				recruitsShortSword.IsDropable = true;
				recruitsShortSword.IsSaleable = true;
				recruitsShortSword.IsTradable = true;
				recruitsShortSword.Color = 45; // blue metal

				recruitsShortSword.Bonus = 1; // default bonus

				recruitsShortSword.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 3));
				recruitsShortSword.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 1));

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsShortSword);
			}

			// item db check
			recruitsStaff = GameServer.Database.SelectObject(typeof (StaffTemplate), Expression.Eq("Name", "Recruit's Staff")) as StaffTemplate;
			if (recruitsStaff == null)
			{
				recruitsStaff = new StaffTemplate();
				recruitsStaff.Name = "Recruit's Staff";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsStaff.Name + ", creating it ...");
				recruitsStaff.Level = 4;

				recruitsStaff.Weight = 45;
				recruitsStaff.Model = 442;

				recruitsStaff.DamageType = eDamageType.Crush;
				recruitsStaff.Speed = 4500;
				recruitsStaff.DamagePerSecond = 24;
				recruitsStaff.HandNeeded = eHandNeeded.TwoHands;

				recruitsStaff.Value = 200;

				recruitsStaff.IsDropable = true;
				recruitsStaff.IsSaleable = true;
				recruitsStaff.IsTradable = true;
				recruitsStaff.Color = 45; // blue metal

				recruitsStaff.Bonus = 1; // default bonus

				recruitsStaff.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Intelligence, 3));
				recruitsStaff.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 1));

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsStaff);
			}

			#endregion

			
/*
			fairyArea = fairyLocation.Region.AddArea(new Area.Circle("Fairy contamined Area", fairyLocation.Position, 1500));

            QuestBuilder builder = QuestMgr.getBuilder(typeof(Nuisances));
            BaseQuestPart a;

            builder.AddInteraction(masterFrederick,-1,eTextType.Talk,null,"My young recruit, I fear we have a growing problem on our hands. For the past several nights, citizens in Cotswold have been complaining of a constant ringing noise. It has started to keep them up at [night].");

            builder.AddInteraction(masterFrederick,2,eTextType.Talk,null,"Ah {Player}, you've returned, and none the worse for wear. Tell me, what did you find?");
            builder.AddInteraction(masterFrederick,3,eTextType.Talk,null,"Ire fairies! They're the worst! Well, now we know who has been causing these problems. You've done good work here today. It is time for a reward for your [efforts].");

            builder.AddInteraction(masterFrederick,-1,eTextType.Talk,"night","It has even begun to affect the wildlife in this area. The guards can not commit any troops to finding out the cause of this ringing sound, so the responsibility falls to you {Player}. Will you help [Cotswold]?");

            a = builder.AddInteraction(masterFrederick,-1,eTextType.None,"Cotswold",null);
                a.AddAction(eActionType.Animation, eEmote.Beg, masterFrederick);    
                a.AddAction(eActionType.OfferQuest,typeof(Nuisances), "Will you help out Cotswold and discover who or what is making this noise?");
                
            builder.AddOnQuestDecline(masterFrederick, eTextType.Talk, "Oh well, if you change your mind, please come back!");
            a = builder.AddOnQuestAccept(masterFrederick, eTextType.Talk, "This magical box will help you capture whatever is making that noise. The reports indicate that the noise is the loudest to the west-northwest, near the banks of the river. Find the source of the noise. Take this box with you. Some of the other trainers seem to think it is magical in nature. I'm not so sure. USE the box in the area that is the loudest, or where you encounter trouble. See if you can capture anything.");
                a.AddAction(eActionType.GiveItem, emptyMagicBox);

            a = builder.AddInteraction(masterFrederick,3,eTextType.Talk,"efforts","A Fighter is nothing unless he has a good weapon by his or her side. For you, a new sword is in order. Use it well. For now, I must think about what to do with these Ire Fairies, and figure out why they are here.");
			    a.AddAction(eActionType.FinishQuest,typeof(Nuisances));
                a.AddAction(eActionType.Talk,"Don't go far, I have need of your services again {Player}.");
            
            // Add Abort Possibility for quest            
            a = builder.AddInteraction(masterFrederick, 0, eTextType.None, "abort", null);                
                a.AddAction(eActionType.OfferQuestAbort,typeof(Nuisances), "Do you really want to abort this quest, \nall items gained during quest will be lost?");                        
            builder.AddOnQuestContinue(masterFrederick, eTextType.Talk, "Good {Player}, no go out there and finish your work!");
            builder.AddOnQuestAbort(masterFrederick, eTextType.Talk, "Do you really want to abandon your duties? The realm always needs willing men and women, so come back if you changed your mind.");
            

            a = builder.CreateQuestPart(ireFairy, 1,eTextType.Emote, "Ire Fairies! Quick! USE your Magical Wooden Box to capture the fairies! To USE an item, right click on the item and type /use.");                
                a.AddTrigger(eTriggerType.EnterArea,null, fairyArea);
                a.AddAction(eActionType.MonsterSpawn, ireFairy);
            AddQuestPart(a);

            a = builder.CreateQuestPart(ireFairy,1, eTextType.Emote, "As soon as the fairy gets out of your view, it quickly hides behind a tree.");                
                a.AddTrigger(eTriggerType.LeaveArea, null,fairyArea);
                a.AddAction(eActionType.MonsterUnspawn, ireFairy);
            AddQuestPart(a);

            a = builder.CreateQuestPart(ireFairy,1, eTextType.Emote, "You try to catch the ire fairy in your magical wodden box!");
                a.AddTrigger(eTriggerType.ItemUsed, null,emptyMagicBox);                
                a.AddRequirement(eRequirementType.Distance, ireFairy, 500,eComparator.Less);
                a.AddAction(eActionType.Attack, 15);
                a.AddAction(eActionType.Emote, "Quick, the ire fairy noticed your intentions, and is attacking you. Try to dodge here attacks until the magic of the box works.");
                a.AddAction(eActionType.Animation, eEmote.Bind);
                a.AddAction(eActionType.Timer,"irefairy_timer",5000);
            AddQuestPart(a);

            a = builder.CreateQuestPart(ireFairy, 1, eTextType.Emote, "You catch the ire fairy in your magical wodden box!");
                a.AddTrigger(eTriggerType.Timer, "irefairy_timer");
                a.AddRequirement(eRequirementType.Distance, ireFairy, 2000, eComparator.Less);
                a.AddRequirement(eRequirementType.InventoryItem, emptyMagicBox);
                a.AddAction(eActionType.ReplaceItem, emptyMagicBox, fullMagicBox);
                a.AddAction(eActionType.SetQuestStep, typeof(Nuisances), 2);
                a.AddAction(eActionType.MonsterUnspawn, ireFairy);
                a.AddAction(eActionType.Timer, "fullbox_timer", 10000);
            AddQuestPart(a);

            a = builder.CreateQuestPart(ireFairy, 1, eTextType.Emote, "You got too far away, try again.");
                a.AddTrigger(eTriggerType.Timer, "irefairy_timer");
                a.AddRequirement(eRequirementType.Distance, ireFairy, 2000, eComparator.Greater);
            AddQuestPart(a);

            a = builder.CreateQuestPart(ireFairy, 1, eTextType.Emote, "There is nothing within the reach of the magic box that can be cought.");
                a.AddTrigger(eTriggerType.ItemUsed,null, emptyMagicBox);
                a.AddRequirement(eRequirementType.Distance, ireFairy, 500, eComparator.Greater);
            AddQuestPart(a);
            
            a = builder.CreateQuestPart(ireFairy, 2, eTextType.Emote, "The box suddenly starts to shake wildly.");
                a.AddTrigger(eTriggerType.Timer, "fullbox_timer");
                a.AddRequirement(eRequirementType.InventoryItem,fullMagicBox);
                a.AddAction(eActionType.Dialog, "Hey big mister, please let my out. You don't know what your doing, Master Frederick will tear my wings apart... he's going to kill me ... please! I beg you let me out!!!"); 
            AddQuestPart(a);

            a = builder.CreateQuestPart(ireFairy, 2, eTextType.Emote, "You open the full magic box and release the ire fairy.");
                a.AddTrigger(eTriggerType.ItemUsed, null, fullMagicBox);                
                a.AddRequirement(eRequirementType.InventoryItem, fullMagicBox);
                a.AddAction(eActionType.ReplaceItem,fullMagicBox,emptyMagicBox);
                a.AddAction(eActionType.Animation, eEmote.Yes);
            AddQuestPart(a);


            // give the full box back to master frederick
            a = builder.AddOnGiveItem(masterFrederick, 2, fullMagicBox, eTextType.Talk, "Ah, it is quite heavy, let me take a peek.");
            a.AddAction(eActionType.Animation, eEmote.Yes);
            a.AddAction(eActionType.Emote, "Master Frederick opens the box carefully. When he sees the contents, he quickly closes it and turns his attention back to you.");
            a.AddAction(eActionType.SetQuestStep, typeof(Nuisances), 3);
            a.AddAction(eActionType.TakeItem, fullMagicBox);

            // give the empty box back to master frederick
            a = builder.AddOnGiveItem(masterFrederick, 2, emptyMagicBox, eTextType.Talk, "What's this, there's no fairy in there. Don't tell me you let yourself trick into release that *poor* fairy. You still have a lot to learn young {Player}.");
            a.AddAction(eActionType.Animation, eEmote.No);            
            a.AddAction(eActionType.SetQuestStep, typeof(Nuisances), 3);
            a.AddAction(eActionType.TakeItem, emptyMagicBox);
			 */

			/* Now we bring to masterFrederick the possibility to give this quest to players */
			QuestMgr.AddQuestDescriptor(masterFrederick, typeof(NuisancesDescriptor));

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
			
			if (masterFrederick == null)
				return;

			/* Now we remove to masterFrederick the possibility to give this quest to players */
			QuestMgr.RemoveQuestDescriptor(masterFrederick, typeof(NuisancesDescriptor));
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
						return "[Step #1] Find the area where the sound is the loudest. USE the box and see if you can capture anything. Master Frederick believes the area is to the west-northwest, in some trees near the river.";
					case 2:
						return "[Step #2] Take the Full Magical Wooden Box back to Master Frederick in Cotswold. Be sure to hand him the Full Magical Wooden Box.";
					case 3:
						return "[Step #3] Wait for Master Frederick to reward you. If he stops speaking with you at any time, ask if there is something he can give you for your [efforts].";
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
			//Give reward to player here ...            
			if (m_questPlayer.HasAbilityToUseItem(recruitsShortSword.CreateInstance() as EquipableItem))
				GiveItemToPlayer(masterFrederick, recruitsShortSword.CreateInstance());
			else
				GiveItemToPlayer(masterFrederick, recruitsStaff.CreateInstance());

			m_questPlayer.GainExperience(100, 0, 0, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 3, Util.Random(50)), "You recieve {0} as a reward.");			
		
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...
		}
	}
}
