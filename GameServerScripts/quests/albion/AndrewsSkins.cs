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
 * 1) Travel to loc=10725,27708 Camelot Hills (Cotswold Village) to speak with Andrew Wyatt
 * 2) Go to loc=29164,22747 City of Camelot and give the bundle of bear skins to Geor Nadren. 
 * 3) Go to loc=28607,22593 City of Camelot and ask some threads to Ver Nuren (/whisper thread) (you gain some xp and money).
 * 4) Can back to Geor Nadren and give it a thread (you gain some xp and money).
 * 5) Can back to Andrew Wyatt at Camelot Hills (Cotswold Village) (/whisper finished) to have your reward.
 */

using System;
using System.Reflection;
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS.PacketHandler;
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
    public class AndrewsSkinsDescriptor : AbstractQuestDescriptor
    {
        /* This is the type of the quest class linked with 
         * this requirement class, you must override the 
         * base methid like that
         */
        public override Type LinkedQuestType
        {
            get { return typeof(AndrewsSkins); }
        }

        /* This value is used to retrieves the minimum level needed
         *  to be able to make this quest. Override it only if you need, 
         * the default value is 1
         */
        public override int MinLevel
        {
            get { return 3; }
        }

        /* This value is used to retrieves how maximum level needed
         * to be able to make this quest. Override it only if you need, 
         * the default value is 50
         */
        public override int MaxLevel
        {
            get { return 5; }
        }
    }

    /* The second thing we do, is to declare the class we create
     * as Quest. We must make it persistant using attributes, to
     * do this, we derive from the abstract class AbstractQuest
     */
    [Subclass(NameType = typeof(AndrewsSkins), ExtendsType = typeof(AbstractQuest))]
    public class AndrewsSkins : BaseQuest
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /* Declare the variables we need inside our quest.
         * You can declare static variables here, which will be available in 
         * ALL instance of your quest and should be initialized ONLY ONCE inside
         * the OnScriptLoaded method
         */

        protected const string questTitle = "Andrew's Skins";

        private static GameMob andrewWyatt = null;
        private static GameMob georNadren = null;
        private static GameMob verNuren = null;

        private static GenericItemTemplate bundleOfBearSkins = null;
        private static GenericItemTemplate spoolOfLeatherworkingThread = null;

        private static NecklaceTemplate chokerOfTheBear = null;

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

			/* If andrewWyatt == null then no Andrew Wyatt exists in
				* this users Mob Database, so we simply create one ;-)
				* else we take the existing one.
				*/
           	andrewWyatt = ResearchQuestObject(typeof(GameMob), WorldMgr.GetRegion(1), eRealm.Albion, "Andrew Wyatt") as GameMob;
            if (andrewWyatt == null)
            {
                andrewWyatt = new GameMob();
                andrewWyatt.Model = 80;
                andrewWyatt.Name = "Andrew Wyatt";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + andrewWyatt.Name + ", creating him ...");
                andrewWyatt.GuildName = "Part of " + questTitle + " Quest";
                andrewWyatt.Realm = (byte)eRealm.Albion;
				andrewWyatt.Region = WorldMgr.GetRegion(1);

				GameNpcInventory template = new GameNpcInventory();
				template.AddItem(eInventorySlot.HandsArmor, new NPCArmor(80));
				template.AddItem(eInventorySlot.FeetArmor, new NPCArmor(54));
				template.AddItem(eInventorySlot.TorsoArmor, new NPCArmor(51));
				template.AddItem(eInventorySlot.LegsArmor, new NPCArmor(52));
				andrewWyatt.Inventory = template;
				
                andrewWyatt.Size = 48;
                andrewWyatt.Level = 30;
                andrewWyatt.Position = new Point(559590, 511039, 2488);
                andrewWyatt.Heading = 1524;

				StandardMobBrain newBrain = new StandardMobBrain();
				newBrain.Body = andrewWyatt;
				newBrain.AggroLevel = 100;
				newBrain.AggroRange = 0;
				andrewWyatt.OwnBrain = newBrain;

				if(!andrewWyatt.AddToWorld())
				{
					if (log.IsWarnEnabled)
						log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
					return;
				}

                //You don't have to store the created mob in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database

                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(andrewWyatt);
            }
           

			georNadren = ResearchQuestObject(typeof(GameMob), WorldMgr.GetRegion(10), eRealm.Albion, "Geor Nadren") as GameMob;
			if (georNadren == null)
            {
                if (log.IsWarnEnabled)
                    log.Warn("Could not find Geor Nadren, creating him ...");
                georNadren = new GameMob();
                georNadren.Model = 9;
                georNadren.Name = "Geor Nadren";
                georNadren.GuildName = "Part of " + questTitle + " Quest";
                georNadren.Realm = (byte)eRealm.Albion;
				georNadren.Region =  WorldMgr.GetRegion(10);

				GameNpcInventory template = new GameNpcInventory();
				template.AddItem(eInventorySlot.HandsArmor, new NPCArmor(39));
				template.AddItem(eInventorySlot.FeetArmor, new NPCArmor(40));
				template.AddItem(eInventorySlot.TorsoArmor, new NPCArmor(36));
				template.AddItem(eInventorySlot.LegsArmor, new NPCArmor(37));
				template.AddItem(eInventorySlot.ArmsArmor, new NPCArmor(38));
				georNadren.Inventory = template;
				
                georNadren.Size = 51;
                georNadren.Level = 8;
                georNadren.Position = new Point(37355, 30943, 8002);
                georNadren.Heading = 3231;

				StandardMobBrain newBrain = new StandardMobBrain();
				newBrain.Body = georNadren;
				newBrain.AggroLevel = 100;
				newBrain.AggroRange = 0;
				georNadren.OwnBrain = newBrain;

				if(!georNadren.AddToWorld())
				{
					if (log.IsWarnEnabled)
						log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
					return;
				}

                //You don't have to store the created mob in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(georNadren);
			}

			verNuren =  ResearchQuestObject(typeof(GameMob), WorldMgr.GetRegion(10), eRealm.Albion, "Ver Nuren") as GameMob;
			if (verNuren == null)
			{
                if (log.IsWarnEnabled)
                    log.Warn("Could not find Ver Nuren, creating him ...");
                verNuren = new GameMob();
                verNuren.Model = 9;
                verNuren.Name = "Ver Nuren";
                verNuren.GuildName = "Part of " + questTitle + " Quest";
                verNuren.Realm = (byte)eRealm.Albion;
				verNuren.Region =  WorldMgr.GetRegion(10);

				GameNpcInventory template = new GameNpcInventory();
				template.AddItem(eInventorySlot.LeftHandWeapon, new NPCWeapon(61));
				template.AddItem(eInventorySlot.HandsArmor, new NPCArmor(39));
				template.AddItem(eInventorySlot.FeetArmor, new NPCArmor(40));
				template.AddItem(eInventorySlot.TorsoArmor, new NPCArmor(36));
				template.AddItem(eInventorySlot.LegsArmor, new NPCArmor(37));
				template.AddItem(eInventorySlot.ArmsArmor, new NPCArmor(38));
				verNuren.Inventory = template;
				verNuren.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

                verNuren.Size = 51;
                verNuren.Level = 8;
                verNuren.Position = new Point(36799, 30786, 8010);
                verNuren.Heading = 625;

				StandardMobBrain newBrain = new StandardMobBrain();
				newBrain.Body = verNuren;
				newBrain.AggroLevel = 100;
				newBrain.AggroRange = 0;
				verNuren.OwnBrain = newBrain;

				if(!verNuren.AddToWorld())
				{
					if (log.IsWarnEnabled)
						log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
					return;
				}

                //You don't have to store the created mob in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(verNuren);
			}
     

            #endregion

            #region defineItems

            // item db check
            bundleOfBearSkins = GameServer.Database.SelectObject(typeof(GenericItemTemplate), Expression.Eq("Name", "Bundle of Bear Skins")) as GenericItemTemplate;
            if (bundleOfBearSkins == null)
            {
                bundleOfBearSkins = new GenericItemTemplate();
                bundleOfBearSkins.Name = "Bundle of Bear Skins";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + bundleOfBearSkins.Name + ", creating it ...");
                bundleOfBearSkins.Model = 100;
                bundleOfBearSkins.IsDropable = false;
                bundleOfBearSkins.IsSaleable = false;
                bundleOfBearSkins.IsTradable = false;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(bundleOfBearSkins);
            }

            // item db check
			spoolOfLeatherworkingThread = GameServer.Database.SelectObject(typeof(GenericItemTemplate), Expression.Eq("Name", "Spool of Leatherworking Thread")) as GenericItemTemplate;
			if (spoolOfLeatherworkingThread == null)
            {
                spoolOfLeatherworkingThread = new GenericItemTemplate();
                spoolOfLeatherworkingThread.Name = "Spool of Leatherworking Thread";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + spoolOfLeatherworkingThread.Name + ", creating it ...");
                spoolOfLeatherworkingThread.Model = 537;
                spoolOfLeatherworkingThread.IsDropable = false;
                spoolOfLeatherworkingThread.IsSaleable = false;
                spoolOfLeatherworkingThread.IsTradable = false;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(spoolOfLeatherworkingThread);
            }

            // item db check
            chokerOfTheBear = GameServer.Database.SelectObject(typeof(NecklaceTemplate), Expression.Eq("Name", "Choker of the Bear")) as NecklaceTemplate;
            if (chokerOfTheBear == null)
            {
                chokerOfTheBear = new NecklaceTemplate();
                chokerOfTheBear.Name = "Choker of the Bear";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + chokerOfTheBear.Name + ", creating it ...");

                chokerOfTheBear.Level = 5;
                chokerOfTheBear.Weight = 6;
                chokerOfTheBear.Model = 101;
                chokerOfTheBear.Value = 30;

                chokerOfTheBear.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 4));
                chokerOfTheBear.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Thrust, 1));

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(chokerOfTheBear);
            }

            #endregion

            /* Now we add some hooks to the npc we found.
			* Actually, we want to know when a player interacts with him.
			* So, we hook the right-click (interact) and the whisper method
			* of npc and set the callback method to the "TalkToXXX"
			* method. This means, the "TalkToXXX" method is called whenever
			* a player right clicks on him or when he whispers to him.
			*/

            GameEventMgr.AddHandler(andrewWyatt, GameObjectEvent.Interact, new DOLEventHandler(TalkToAndrewWyatt));
            GameEventMgr.AddHandler(andrewWyatt, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToAndrewWyatt));

            GameEventMgr.AddHandler(georNadren, GameObjectEvent.Interact, new DOLEventHandler(TalkToGeorNadren));
            GameEventMgr.AddHandler(georNadren, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToGeorNadren));

            GameEventMgr.AddHandler(verNuren, GameObjectEvent.Interact, new DOLEventHandler(TalkToVerNuren));
            GameEventMgr.AddHandler(verNuren, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToVerNuren));

            /* Now we add some hooks to trigger the quest dialog reponse. */
            GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogResponse));
            GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogResponse));

            /* Now we bring to Ydenia the possibility to give this quest to players */
            QuestMgr.AddQuestDescriptor(andrewWyatt, typeof(AndrewsSkinsDescriptor));

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
            /* If Andrew Wyatt has not been initialized, then we don't have to remove any
             * hooks from him ;-)
             */
            if (andrewWyatt == null)
                return;

            /* Removing hooks works just as adding them but instead of 
             * AddHandler, we call RemoveHandler, the parameters stay the same
             */
            GameEventMgr.RemoveHandler(andrewWyatt, GameObjectEvent.Interact, new DOLEventHandler(TalkToAndrewWyatt));
            GameEventMgr.RemoveHandler(andrewWyatt, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToAndrewWyatt));

            GameEventMgr.RemoveHandler(georNadren, GameObjectEvent.Interact, new DOLEventHandler(TalkToGeorNadren));
            GameEventMgr.RemoveHandler(georNadren, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToGeorNadren));

            GameEventMgr.RemoveHandler(verNuren, GameObjectEvent.Interact, new DOLEventHandler(TalkToVerNuren));
            GameEventMgr.RemoveHandler(verNuren, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToVerNuren));

			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogResponse));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogResponse));

            /* Now we remove to Ydenia the possibility to give this quest to players */
            QuestMgr.RemoveQuestDescriptor(andrewWyatt, typeof(AgainstTheGrainDescriptor));
        }

        /* This is the method we declared as callback for the hooks we set to
         * NPC. It will be called whenever a player right clicks on NPC
         * or when he whispers something to him.
         */

        protected static void TalkToAndrewWyatt(DOLEvent e, object sender, EventArgs args)
        {
            //We get the player from the event arguments and check if he qualifies		
            GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
            if (player == null)
                return;

            if (QuestMgr.CanGiveQuest(typeof(AndrewsSkins), player, andrewWyatt) <= 0)
                return;

            //We also check if the player is already doing the quest
            AndrewsSkins quest = player.IsDoingQuest(typeof(AndrewsSkins)) as AndrewsSkins;

            andrewWyatt.TurnTo(player.Position);
            //Did the player rightclick on NPC?
            if (e == GameObjectEvent.Interact)
            {
                if (quest == null)
                {
                    //Player is not doing the quest...
                    andrewWyatt.SayTo(player, "Greetings friend.  I am Andrew Wyatt, local hunter in these parts.  You must be a fresh, young adventurer, aren't you?  Well then, I might have an [errand] for you to run.");
                    return;
                }
                else
                {
                    if (quest.Step == 4)
                    {
                        andrewWyatt.SayTo(player, "Ah, back so soon friend?  Well then, I take it you [finished] my errand?");
                    }
                    return;
                }
            }
            // The player whispered to NPC (clicked on the text inside the [])
            else if (e == GameLivingEvent.WhisperReceive)
            {
                WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs)args;
                if (quest == null)
                {
                    //Do some small talk :)
                    switch (wArgs.Text)
                    {
                        case "errand":
                            andrewWyatt.SayTo(player, "Aye friend.  I hunt all manner of creatures, but my specialty is bears.  I hunt every type of bear out there.  I use every part of them too.  I need to run my next shipment of skins to Geor Nadren in [Camelot].");
                            break;
                        case "Camelot":
                            andrewWyatt.SayTo(player, "I'll tell you what.  If you take my skins to him, I'll set you up with a little reward for your troubles.  I'm enjoying my time here in the tavern, and I'd like to stay a little longer.  What do you say?  Are you [up] for it or not?");
                            break;

                        //If the player offered his help, we send the quest dialog now!
                        case "up":
                            QuestMgr.ProposeQuestToPlayer(typeof(AndrewsSkins), "Will you deliver these skins to \nGeor Nadren in Camelot City?\n[Level " + player.Level + "]?", player, andrewWyatt);
                            break;
                    }
                }
                else
                {
                    switch (wArgs.Text)
                    {
                        case "finished":
                            if (quest.Step == 4)
                            {
                                andrewWyatt.SayTo(player, "I knew I could count on you.  All right then, as promised, a small reward for your troubles.  Use it well, and good luck " + player.Name + ".  Perhaps I'll have some other work for you in the future.");
                                quest.FinishQuest();
                            }
                            break;
                    }
                }
            }
        }

        /* This is the method we declared as callback for the hooks we set to
         * NPC. It will be called whenever a player right clicks on NPC
         * or when he whispers something to him.
         */

        protected static void TalkToGeorNadren(DOLEvent e, object sender, EventArgs args)
        {
            //We get the player from the event arguments and check if he qualifies		
            GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
            if (player == null)
                return;

            //We also check if the player is already doing the quest
            AndrewsSkins quest = player.IsDoingQuest(typeof(AndrewsSkins)) as AndrewsSkins;

            georNadren.TurnTo(player.Position);
            //Did the player rightclick on NPC?
            if (e == GameObjectEvent.Interact)
            {
                if (quest != null)
                {
                    switch (quest.Step)
                    {
                        case 1:
                            georNadren.SayTo(player, "Hello there friend.  How may I be of service today?");
                            break;

                        case 3:
                            georNadren.SayTo(player, "Welcome back friend.  Did you get the thread I need?");
                            break;
                    }
                }
            }
            // The player whispered to NPC (clicked on the text inside the [])
            else if (e == GameLivingEvent.WhisperReceive)
            {
                WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs)args;
                if (quest != null)
                {
                    switch (wArgs.Text)
                    {
                        case "errand":
                            if (quest.Step == 2)
                            {
                                georNadren.SayTo(player, "I need a few spools of thread in order to continue making the armor I sell.  Ver Nuren's wife makes the thread I use.  Go ask him if he has any for me.  Come back to me when you have some.");
                            }
                            break;
                    }
                }
            }
        }

        /* This is the method we declared as callback for the hooks we set to
         * NPC. It will be called whenever a player right clicks on NPC
         * or when he whispers something to him.
         */

        protected static void TalkToVerNuren(DOLEvent e, object sender, EventArgs args)
        {
            //We get the player from the event arguments and check if he qualifies		
            GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
            if (player == null)
                return;

            //We also check if the player is already doing the quest
            AndrewsSkins quest = player.IsDoingQuest(typeof(AndrewsSkins)) as AndrewsSkins;

            verNuren.TurnTo(player.Position);
            //Did the player rightclick on NPC?
            if (e == GameObjectEvent.Interact)
            {
                if (quest != null)
                {
                    if (quest.Step < 3)
                    {
                        //Player is doing the quest...
                        verNuren.SayTo(player, "Greetings to you traveler.  Is there something I can help you with today?");
                        return;
                    }
                }
            }
            // The player whispered to NPC (clicked on the text inside the [] or with /whisper)
            else if (e == GameLivingEvent.WhisperReceive)
            {
                WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs)args;
                if (quest != null)
                {
                    switch (wArgs.Text)
                    {
                        case "thread":
                            if (quest.Step == 2)
                            {
                                verNuren.SayTo(player, "Oh yes.  I know what you're talking about.  Geor told me the other day that he was out of the thread he needs to make his armor.  No worries.  I have some right here.  Now please, take this straight to Geor.");
                                GiveItemToPlayer(verNuren, CreateQuestItem(spoolOfLeatherworkingThread, quest), player);
								
                                player.GainExperience(40, 0, 0, true);
                                player.AddMoney(Money.GetMoney(0, 0, 0, 3, Util.Random(50)), "You are awarded 3 silver and some copper!");

                                quest.ChangeQuestStep(3);
                            }
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

            if (gArgs != null && gArgs.QuestType.Equals(typeof(AndrewsSkins)))
            {
                GamePlayer player = gArgs.Player;
                if (player == null) return;

                if (e == GamePlayerEvent.AcceptQuest)
                {
                    if (QuestMgr.GiveQuestToPlayer(typeof(AndrewsSkins), player, gArgs.Source as GameNPC))
                    {
                        // give letter        
                        GiveItemToPlayer(gArgs.Source, CreateQuestItem(bundleOfBearSkins, player.IsDoingQuest(typeof(AndrewsSkins))), player);
                        SendReply(player, "Great, I knew you'd help me out.  All right friend, here are the skins.  Take them to Geor Nadren inside Camelot.  He'll be happy to see you.");
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
                        return "[Step #1] Take the skins that Andrew gave you to Geor Nadren in Camelot City.";
                    case 2:
                        return "[Step #2] Ver Nuren in Camelot City has some of the thread that Geor needs to make his armor.  Find him and ask if he has any [thread].";
                    case 3:
                        return "[Step #3] Take the spools of thread back to Geor in Camelot City.";
                    case 4:
                        return "[Step #4] Return to Andrew Wyatt in Cotswold Village.  Tell him you have [finished] his errand.";
                    default:
                        return "[Step #" + Step + "] No Description entered for this step!";
                }
            }
        }

        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = sender as GamePlayer;

            if (player == null || player.IsDoingQuest(typeof(AndrewsSkins)) == null)
                return;


            if (e == GamePlayerEvent.GiveItem)
            {
                if (Step == 1)
                {
                    GiveItemEventArgs gArgs = (GiveItemEventArgs)args;
                    if (gArgs.Target.Name == georNadren.Name && gArgs.Item.Name == bundleOfBearSkins.Name)
                    {
                        RemoveItemFromPlayer(georNadren, gArgs.Item);

                        georNadren.TurnTo(m_questPlayer.Position);
                        georNadren.SayTo(m_questPlayer, "Ah!  These must be the skins I've been waiting for from Andrew.  My, my, these are very high quality.  He's a very skilled hunter indeed, with a fine eye for good pelts.  Excellent!  I have but one more [errand] I need for you to run for me.");

                        ChangeQuestStep(2);
                        return;
                    }
                }
                else if (Step == 3)
                {
                    GiveItemEventArgs gArgs = (GiveItemEventArgs)args;
                    if (gArgs.Target.Name == georNadren.Name && gArgs.Item.Name== spoolOfLeatherworkingThread.Name)
                    {
                        RemoveItemFromPlayer(georNadren, gArgs.Item);

                        georNadren.TurnTo(m_questPlayer.Position);
                        georNadren.SayTo(m_questPlayer, "Excellent!  Why, there is enough here to make several suits of armor.  Thank you friend!  Now, I think you need to return to Andrew in Cotswold and let him know I received the skins.  Thank you again, and good journeys to you " + m_questPlayer.Name + ".");

                        m_questPlayer.GainExperience(40, 0, 0, true);
                        m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 2, Util.Random(50)), "You are awarded 2 silver and some copper!");

                        ChangeQuestStep(4);
                        return;
                    }
                }


            }
        }

        public override void FinishQuest()
        {
            //Give reward to player here ...
            GiveItemToPlayer(andrewWyatt, chokerOfTheBear.CreateInstance());

            m_questPlayer.GainExperience(80, 0, 0, true);
            m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 4, Util.Random(50)), "You are awarded 4 silver and some copper!");
       
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...
		}
    }
}
