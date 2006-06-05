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
 * Author:		Doulbousiouf
 * Date:			
 * Directory: /scripts/quests/albion/
 *
 * Description:
 *  Brief Walkthrough: 
 * 1) Travel to loc=13417,28914 Camelot Hills (Cotswold Village) to speak with Liridia the Minstrel
 * 2) Go to loc=15075,25906 Camelot Hills and speak with Farmer Asma.
 * 3) Come back to Liridia the Minstrel and you will have your reward. 
 */

using System;
using System.Reflection;
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;
using NHibernate.Mapping.Attributes;
using DOL.Database;
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
    public class AgainstTheGrainDescriptor : AbstractQuestDescriptor
    {
        /* This is the type of the quest class linked with 
         * this requirement class, you must override the 
         * base method like that
         */
        public override Type LinkedQuestType
        {
            get { return typeof(AgainstTheGrain); }
        }

        /* This value is used to retrieves how maximum level needed
         * to be able to make this quest. Override it only if you need, 
         * the default value is 50
         */
        public override int MaxLevel
        {
            get { return 4; }
        }
    }


    /* The second thing we do, is to declare the class we create
     * as Quest. We must make it persistant using attributes, to
     * do this, we derive from the abstract class AbstractQuest
     */
    [Subclass(NameType = typeof(AgainstTheGrain), ExtendsType = typeof(AbstractQuest))] 
	public class AgainstTheGrain : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /* Declare the variables we need inside our quest.
         * You can declare static variables here, which will be available in 
         * ALL instance of your quest and should be initialized ONLY ONCE inside
         * the OnScriptLoaded method.
         */

        private static GameNPC laridiaTheMinstrel = null;
        private static GameNPC farmerAsma = null;
        public const string questName = "Against The Grain";

        /* The following method is called automatically when this quest class
         * is loaded. You might notice that this method is the same as in standard
         * game events. And yes, quests basically are game events for single players
         * 
         * To make this method automatically load, we have to declare it static
         * and give it the [ScriptLoadedEvent] attribute. 
         * 
         * Inside this method we initialize the quest. This is neccessary if we 
         * want to set the quest hooks to the NPCs. 
         */

		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
            if (log.IsInfoEnabled)
                log.Info("Quest \"" + questName + "\" initializing ...");

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

			/* If laridiaTheMinstrel == null then no Laridia exists in
				* this users Mob Database, so we simply create one ;-)
				* else we take the existing one.
				*/
			laridiaTheMinstrel = ResearchQuestObject(typeof(GameMob), WorldMgr.GetRegion(1), eRealm.Albion, "Laridia the Minstrel") as GameMob;
			if (laridiaTheMinstrel == null)
			{
				laridiaTheMinstrel = new GameMob();
				laridiaTheMinstrel.Model = 38;
				laridiaTheMinstrel.Name = "Laridia the Minstrel";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + laridiaTheMinstrel.Name + ", creating him ...");
				laridiaTheMinstrel.GuildName = "Part of " + questName+ " Quest";
				laridiaTheMinstrel.Realm = (byte) eRealm.Albion;
				laridiaTheMinstrel.Region = WorldMgr.GetRegion(1);

				GameNpcInventory template = new GameNpcInventory();
				template.AddItem(eInventorySlot.HandsArmor, new NPCArmor(137, 9, 0));
				template.AddItem(eInventorySlot.FeetArmor, new NPCArmor(138, 9, 0));
				template.AddItem(eInventorySlot.TorsoArmor, new NPCArmor(134, 9, 0));
				template.AddItem(eInventorySlot.Cloak, new NPCEquipment(96, 72));
				template.AddItem(eInventorySlot.LegsArmor, new NPCArmor(140, 43, 0));
				template.AddItem(eInventorySlot.ArmsArmor, new NPCArmor(141, 43, 0));
				laridiaTheMinstrel.Inventory = template;

				laridiaTheMinstrel.Size = 49;
				laridiaTheMinstrel.Level = 25;
				laridiaTheMinstrel.Position = new Point(562280, 512243, 2448);
				laridiaTheMinstrel.Heading = 3049;

				StandardMobBrain newBrain = new StandardMobBrain();
				newBrain.Body = laridiaTheMinstrel;
				newBrain.AggroLevel = 100;
				newBrain.AggroRange = 0;
				laridiaTheMinstrel.OwnBrain = newBrain;

				if(!laridiaTheMinstrel.AddToWorld())
				{
					if (log.IsWarnEnabled)
						log.Warn("Quest "+questName+" abort because a needed region is not in use in this server!");
					return;
				}

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database

				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(laridiaTheMinstrel);
			}

			
			farmerAsma = ResearchQuestObject(typeof(GameMob), WorldMgr.GetRegion(1), eRealm.Albion, "Farmer Asma") as GameMob;
			if (farmerAsma == null)
			{
				farmerAsma = new GameMob();
				farmerAsma.Model = 82;
				farmerAsma.Name = "Farmer Asma";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + farmerAsma.Name + ", creating him ...");
				farmerAsma.GuildName = "Part of " + questName + " Quest";
				farmerAsma.Realm = (byte) eRealm.Albion;
				farmerAsma.Region = WorldMgr.GetRegion(1);

				GameNpcInventory template = new GameNpcInventory();
				template.AddItem(eInventorySlot.TorsoArmor, new NPCArmor(31, 0, 0));
				template.AddItem(eInventorySlot.Cloak, new NPCEquipment(57, 0));
				template.AddItem(eInventorySlot.LegsArmor, new NPCArmor(32, 0, 0));
				template.AddItem(eInventorySlot.ArmsArmor, new NPCArmor(33, 0, 0));
				farmerAsma.Inventory = template;
				
				farmerAsma.Size = 50;
				farmerAsma.Level = 35;
				farmerAsma.Position = new Point(563939, 509234, 2744);
				farmerAsma.Heading = 21;

				StandardMobBrain newBrain = new StandardMobBrain();
				newBrain.Body = farmerAsma;
				newBrain.AggroLevel = 100;
				newBrain.AggroRange = 0;
				farmerAsma.OwnBrain = newBrain;

				if(!farmerAsma.AddToWorld())
				{
					if (log.IsWarnEnabled)
						log.Warn("Quest "+questName+" abort because a needed region is not in use in this server!");
					return;
				}

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(farmerAsma);
			}

			#endregion

			/* Now we add some hooks to the npc we found.
			* Actually, we want to know when a player interacts with him.
			* So, we hook the right-click (interact) and the whisper method
			* of npc and set the callback method to the "TalkToXXX"
			* method. This means, the "TalkToXXX" method is called whenever
			* a player right clicks on him or when he whispers to him.
			*/
			
			GameEventMgr.AddHandler(laridiaTheMinstrel, GameObjectEvent.Interact, new DOLEventHandler(TalkToLaridiaTheMinstrel));
			GameEventMgr.AddHandler(laridiaTheMinstrel, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToLaridiaTheMinstrel));

			GameEventMgr.AddHandler(farmerAsma, GameObjectEvent.Interact, new DOLEventHandler(TalkToFarmerAsma));
			GameEventMgr.AddHandler(farmerAsma, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToFarmerAsma));

            /* Now we add some hooks to trigger the quest dialog reponse. */
            GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogResponse));
            GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogResponse));

            /* Now we bring to SirQuait the possibility to give this quest to players,*/
            QuestMgr.AddQuestDescriptor(laridiaTheMinstrel, typeof(AgainstTheGrainDescriptor));

            if (log.IsInfoEnabled)
                log.Info("Quest \"" + questName + "\" initialized");
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
			if (laridiaTheMinstrel == null)
				return;

			/* Removing hooks works just as adding them but instead of 
			 * AddHandler, we call RemoveHandler, the parameters stay the same
			 */
			GameEventMgr.RemoveHandler(laridiaTheMinstrel, GameObjectEvent.Interact, new DOLEventHandler(TalkToLaridiaTheMinstrel));
			GameEventMgr.RemoveHandler(laridiaTheMinstrel, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToLaridiaTheMinstrel));

			GameEventMgr.RemoveHandler(farmerAsma, GameObjectEvent.Interact, new DOLEventHandler(TalkToFarmerAsma));
			GameEventMgr.RemoveHandler(farmerAsma, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToFarmerAsma));
			
			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogResponse));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogResponse));

			/* Now we remove to Ydenia the possibility to give this quest to players */
            QuestMgr.RemoveQuestDescriptor(laridiaTheMinstrel, typeof(AgainstTheGrainDescriptor));
		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToLaridiaTheMinstrel(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

            if (QuestMgr.CanGiveQuest(typeof(AgainstTheGrain), player, laridiaTheMinstrel) <= 0)
				return;

			//We also check if the player is already doing the quest
			AgainstTheGrain quest = player.IsDoingQuest(typeof (AgainstTheGrain)) as AgainstTheGrain;

			laridiaTheMinstrel.TurnTo(player.Position);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					laridiaTheMinstrel.SayTo(player, "Good night, friend. Have you spent much time traveling our [realm]?");
					return;
				}
				else
				{
					if (quest.Step == 2)
					{
						SendMessage(player, "You tell Laridia that the rumor of the displaced farmer is true, and that Farmer Asma is searching for a way to reestablish herself.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow); 
						laridiaTheMinstrel.SayTo(player, "I had a feeling this rumor might turn out to be true. It angers me that Farmer Asma didn't enter into consideration when the pig herders were looking for a field.  We'll have to find a way to help her get back on her [feet].");
					}
					return;
				}
			}
				// The player whispered to NPC (clicked on the text inside the [])
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				if (quest == null)
				{
					//Do some small talk :)
					switch (wArgs.Text)
					{
						case "realm":
							laridiaTheMinstrel.SayTo(player, "If you haven't, then you should take an excursion. I've never been the kind of person that can spend her entire life in one place. I relish change and new experiences! If I had more time, I'd share my stories with you, but I'm [occupied] at the moment.");
							break;
						case "occupied":
							laridiaTheMinstrel.SayTo(player, "I heard a disturbing rumor yesterday.  It seems that a farmer with a longstanding lease near Vetusta Abbey was recently evicted from the land.  Fields where she used to grow barley are now serving as a site for the debut of a new [sport].");
							break;
						case "sport":
							laridiaTheMinstrel.SayTo(player, "Yes, it seems that 'pig herding' is taking Albion by storm.  I'm always interested in new amusements, but I don't think that it's right for these things to cost people their livelihoods.  Do you have time to investigate this rumor for me?");
							//If the player offered his help, we send the quest dialog now!
                            QuestMgr.ProposeQuestToPlayer(typeof(AgainstTheGrain), "Will you investigate Minstrel\nLaridia's story?\n[Level 1-4]?", player, laridiaTheMinstrel);
							break;
					}
				}
				else
				{	
					switch (wArgs.Text)
					{
						case "feet":
							if(quest.Step == 2)
							{
								laridiaTheMinstrel.SayTo(player, "I'm going to see if I can think of a way to help Farmer Asma.  You might want to check back with her later, too, as she may have her own ideas.  In the meantime, here's a bit of coin for your help.");
								quest.FinishQuest();
							}
							break;
					}
				}
			}
		}

		/* This is the method we declared as callback for the hooks we set to
		 * Sir Quait. It will be called whenever a player right clicks on Sir Quait
		 * or when he whispers something to him.
		 */

		protected static void TalkToFarmerAsma(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			//We also check if the player is already doing the quest
			AgainstTheGrain quest = player.IsDoingQuest(typeof (AgainstTheGrain)) as AgainstTheGrain;

			farmerAsma.TurnTo(player.Position);
			
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null && quest.Step == 1)
				{
					//Player is doing the quest...
					farmerAsma.SayTo(player, "Hello, adventurer.  Pardon me if I do not seem enthusiastic about meeting you, but a terrible thing has just [happened].");
					return;
				}
			}	

			// The player whispered to NPC (clicked on the text inside the [])
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				if (quest != null)
				{
					switch (wArgs.Text)
					{
						case "happened":
							SendMessage(player, "You start to ask Farmer Asma if she's the person Minstrel Laridia was talking about, the farmer continues speaking before you can organize your thoughts.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
							farmerAsma.SayTo(player, "You see, I used to lease land close to Vetusta Abbey, where I raised barley.  Being a farmer never made me wealthy, but it was satisfying. I never had any problems with the Church or my neighbors, so I was shocked when the Abbey asked me to [leave].");
							break;

						case "leave":
							farmerAsma.SayTo(player, "They said they needed the land for something else, but they never mentioned the details. I packed up and left, coming to Cotswold in search of a new plot to lease. Days later, I learned the Church had given the land to people promoting a new [sport].");
							break;

						case "sport":
							farmerAsma.SayTo(player, "A game, can you believe it? I was curious, so I attended a match. When I could see through the crowds of garishly-dressed gentry, it looked to me like a bunch of men chasing pigs around with sticks! I don't understand, but it's quite [popular].");
							break;

						case "popular":
							farmerAsma.SayTo(player, "I can remember playing something similar in my childhood on the farm, but suddenly, it's become a bona fide sport. I just...well, I don't understand why they had to take away my farm for a silly [diversion].");
							player.Out.SendEmoteAnimation(farmerAsma, eEmote.MidgardFrenzy);
							break;

						case "diversion":
							farmerAsma.SayTo(player, "I'm sure it will fade from popularity within a few months. All fads do, and then where will I be? Most likey, I'll still be trying to reestablish myself elsewhere. Well, I need to get back to what I was doing. Thank you for lending a sympathetic ear.");
							quest.ChangeQuestStep(2);
							break;
					}
				}
			}
		}

		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest offer dialog. We check if he accepts or
		 * declines here...
		 */

        protected static void QuestDialogResponse(DOLEvent e, object sender, EventArgs args)
		{
            QuestEventArgs gArgs = args as QuestEventArgs;

            if (gArgs != null && gArgs.QuestType.Equals(typeof(AgainstTheGrain)))
            {
                GamePlayer player = gArgs.Player;
                if (player == null) return;

                if (e == GamePlayerEvent.AcceptQuest)
                {
                    if (QuestMgr.GiveQuestToPlayer(typeof(AgainstTheGrain), player, gArgs.Source as GameNPC))
                    {
                        player.Out.SendMessage("Thank you for agreeing to help me.  The man I spoke with would only say that the farmer was staying in a small camp to the northest of Cotswold. See if you can locate her and verify her story.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    }
                }
                else if (e == GamePlayerEvent.DeclineQuest)
                {
                    player.Out.SendMessage("Oh well, if you change your mind, please come back!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                }
            }
		}

		/* Now we set the quest name.
		 * If we don't override the base method, then the quest
		 * will have the name "UNDEFINED QUEST NAME" and we don't
		 * want that, do we? ;-)
		 */

		public override string Name
		{
            get { return questName; }
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
						return "[Step #1] Find Farmer Asma in the small camp to the northeast of Cotswold. The camp should be near the portal to Shrouded Isles. Right click on Asma to begin a conversation with her.";
					case 2:
						return "[Step #2] Now that you've spoken to Farmer Asma and learned that Laridia's story is true, return to Minstrel Laridia in Cotswold and speak with her.";
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
			m_questPlayer.GainExperience(10 + (m_questPlayer.Level * 5), 0, 0, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 0, 25 + m_questPlayer.Level), "You are awarded "+(25+m_questPlayer.Level)+" copper!");
			
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...
		}
	}
}
