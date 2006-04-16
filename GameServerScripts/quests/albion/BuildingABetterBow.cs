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
 * 1) Travel to loc=10904,27648 Camelot Hills (Cotswold Village) to speak with Elvar Ironhand
 * 2) Go to loc=20736,47872 Camelot Hills and kill skeletons until have two well-preserved bones
 * 2) Came back to Cotswold Village and give your two bones to Elvar Ironhand to have your reward
 */

using System;
using System.Reflection;
using DOL.AI.Brain;
using DOL.GS.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using NHibernate.Expression;
using log4net;
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
    public class BuildingABetterBowDescriptor : AbstractQuestDescriptor
    {
        /* This is the type of the quest class linked with 
         * this requirement class, you must override the 
         * base method like that
         */
        public override Type LinkedQuestType
        {
            get { return typeof(BuildingABetterBow); }
        }

        public override int MinLevel
        {
            get { return 6; }
        }

        /* This value is used to retrieves how maximum level needed
         * to be able to make this quest. Override it only if you need, 
         * the default value is 50
         */
        public override int MaxLevel
        {
            get { return 6; }
        }
    }
    /* The second thing we do, is to declare the class we create
     * as Quest. We must make it persistant using attributes, to
    * do this, we derive from the abstract class AbstractQuest
    */
    [NHibernate.Mapping.Attributes.Subclass(NameType = typeof(BuildingABetterBow), ExtendsType = typeof(AbstractQuest))] 
    public class BuildingABetterBow : BaseQuest
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
        protected const string questTitle = "Building a Better Bow";

        private static GameMob elvarIronhand = null;

        private static GenericItemTemplate wellPreservedBones = null;
        private static GenericItemTemplate twoWellPreservedBones = null;

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

			elvarIronhand = ResearchQuestObject(typeof(GameMob), WorldMgr.GetRegion(1), eRealm.Albion, "Elvar Ironhand") as GameMob;
			if (elvarIronhand == null)
			{
				elvarIronhand = new GameMob();
				elvarIronhand.Model = 10;
				elvarIronhand.Name = "Elvar Ironhand";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + elvarIronhand.Name + ", creating him ...");
				elvarIronhand.GuildName = "Part of " + questTitle + " Quest";
				elvarIronhand.Realm = (byte)eRealm.Albion;
				elvarIronhand.Region = WorldMgr.GetRegion(1);

				GameNpcInventory template = new GameNpcInventory();
				template.AddItem(eInventorySlot.RightHandWeapon, new NPCWeapon(12));
				elvarIronhand.Inventory = template;
				elvarIronhand.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				elvarIronhand.Size = 54;
				elvarIronhand.Level = 17;
				elvarIronhand.Position = new Point(561351, 510292, 2400);
				elvarIronhand.Heading = 3982;

				StandardMobBrain newBrain = new StandardMobBrain();
				newBrain.Body = elvarIronhand;
				newBrain.AggroLevel = 100;
				newBrain.AggroRange = 0;
				elvarIronhand.OwnBrain = newBrain;

				if(!elvarIronhand.AddToWorld())
				{
					if (log.IsWarnEnabled)
						log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
					return;
				}

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(elvarIronhand);
			}
            #endregion

            #region defineItems

            // item db check
            wellPreservedBones = GameServer.Database.SelectObject(typeof(GenericItemTemplate), Expression.Eq("Name", "Well-Preserved Bone")) as GenericItemTemplate;
            if (wellPreservedBones == null)
            {
                wellPreservedBones = new GenericItemTemplate();
                wellPreservedBones.Name = "Well-Preserved Bone";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + wellPreservedBones.Name + ", creating it ...");

                wellPreservedBones.Weight = 1;
                wellPreservedBones.Model = 497;

                wellPreservedBones.IsDropable = false;
                wellPreservedBones.IsSaleable = false;
                wellPreservedBones.IsTradable = false;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(wellPreservedBones);
            }

            // item db check
            twoWellPreservedBones = GameServer.Database.SelectObject(typeof(GenericItemTemplate), Expression.Eq("Name", "Two Well-Preserved Bones")) as GenericItemTemplate;
            if (twoWellPreservedBones == null)
            {
                twoWellPreservedBones = new GenericItemTemplate();
                twoWellPreservedBones.Name = "Two Well-Preserved Bones";
                if (log.IsWarnEnabled)
                    log.Warn("Could not find " + twoWellPreservedBones.Name + ", creating it ...");

                twoWellPreservedBones.Weight = 1;
                twoWellPreservedBones.Model = 497;

                twoWellPreservedBones.IsDropable = false;
                twoWellPreservedBones.IsSaleable = false;
                twoWellPreservedBones.IsTradable = false;

                //You don't have to store the created item in the db if you don't want,
                //it will be recreated each time it is not found, just comment the following
                //line if you rather not modify your database
                if (SAVE_INTO_DATABASE)
                    GameServer.Database.AddNewObject(twoWellPreservedBones);
            }

            #endregion


            /* Now we add some hooks to the npc we found.
			* Actually, we want to know when a player interacts with him.
			* So, we hook the right-click (interact) and the whisper method
			* of npc and set the callback method to the "TalkToXXX"
			* method. This means, the "TalkToXXX" method is called whenever
			* a player right clicks on him or when he whispers to him.
			*/

            GameEventMgr.AddHandler(elvarIronhand, GameObjectEvent.Interact, new DOLEventHandler(TalkToElvarIronhand));
            GameEventMgr.AddHandler(elvarIronhand, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToElvarIronhand));

			/* Now we add some hooks to trigger the quest dialog reponse. */
			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogResponse));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogResponse));

            /* Now we bring to Ydenia the possibility to give this quest to players */
            QuestMgr.AddQuestDescriptor(elvarIronhand, typeof(BuildingABetterBowDescriptor));

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
            /* If Elvar Ironhand has not been initialized, then we don't have to remove any
             * hooks from him ;-)
             */
            if (elvarIronhand == null)
                return;

            /* Removing hooks works just as adding them but instead of 
             * AddHandler, we call RemoveHandler, the parameters stay the same
             */

            GameEventMgr.RemoveHandler(elvarIronhand, GameObjectEvent.Interact, new DOLEventHandler(TalkToElvarIronhand));
            GameEventMgr.RemoveHandler(elvarIronhand, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToElvarIronhand));

			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogResponse));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogResponse));

            /* Now we remove to Ydenia the possibility to give this quest to players */
            QuestMgr.RemoveQuestDescriptor(elvarIronhand, typeof(BuildingABetterBowDescriptor));
        }

        /* This is the method we declared as callback for the hooks we set to
         * NPC. It will be called whenever a player right clicks on NPC
         * or when he whispers something to him.
         */

        protected static void TalkToElvarIronhand(DOLEvent e, object sender, EventArgs args)
        {
            //We get the player from the event arguments and check if he qualifies		
            GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
            if (player == null)
                return;

            if (QuestMgr.CanGiveQuest(typeof(BuildingABetterBow), player, elvarIronhand) <= 0)
                return;

            //We also check if the player is already doing the quest
            BuildingABetterBow quest = player.IsDoingQuest(typeof(BuildingABetterBow)) as BuildingABetterBow;

            elvarIronhand.TurnTo(player);
            //Did the player rightclick on NPC?
            if (e == GameObjectEvent.Interact)
            {
                if (quest == null)
                {
                    //Player is not doing the quest...
                    elvarIronhand.SayTo(player, "Hello, adventurer. Are you here in response to the notice I posted in the [tavern]?");
                    return;
                }
                else
                {
                    if (quest.Step == 4)
                    {
                        elvarIronhand.SayTo(player, "You're back! I hope you were able to retrieve those bones without much trouble. I'm already drawing up the plans for the new bow.  May I have the bones?");
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
                        case "tavern":
                            elvarIronhand.SayTo(player, "I was hoping to get someone to help me with a little side project. You see, in my spare time, I collect texts on weaponsmithing and I recently came across one about weapons used in less civilized [lands].");
                            break;
                        case "lands":
                            elvarIronhand.SayTo(player, "It seems that some nomadic tribes have perfected the art of reinforcing their bows with thin pieces of bone or horn.  The text claims that bows constructed this way shoot farther and hit harder than the ones used by our [scouts].");
                            break;

                        //If the player offered his help, we send the quest dialog now!
                        case "scouts":
                            elvarIronhand.SayTo(player, "I think combining this technique with our longbows could help give our forces the edge in the war against Midgard and Hibernia. Will you help me gather some of the materials to build a prototype?");
							QuestMgr.ProposeQuestToPlayer(typeof(BuildingABetterBow), "Will you help Elvar gather the \nmaterials for his prototype bow? \n[Levels 3-6]", player, elvarIronhand);
							break;
                    }
                }
                else
                {
                    switch (wArgs.Text)
                    {
                        case "first":
                            if (quest.Step == 1)
                            {
                                elvarIronhand.SayTo(player, "If you travel southeast from Cotswold, toward Prydwen keep, you should find some skeletons near the bend in the river.  Return to me when you've gathered two well-preserved bones from them.");
                                quest.ChangeQuestStep(2);
                            }
                            break;

                        case "technique":
                            if (quest.Step == 5)
                            {
                                elvarIronhand.SayTo(player, "Thank you for your help, " + player.CharacterClass.Name + ". Here's a bit of copper for your time. Keep your eyes open for a good source of horn in case the bone prototype doesn't work out.");
                                quest.FinishQuest();
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

			if (gArgs != null && gArgs.QuestType.Equals(typeof(BuildingABetterBow)))
			{
				GamePlayer player = gArgs.Player;
				if (player == null) return;

				if (e == GamePlayerEvent.AcceptQuest)
				{
					if (QuestMgr.GiveQuestToPlayer(typeof(BuildingABetterBow), player, gArgs.Source as GameNPC))
					{
						player.Out.SendMessage("Wonderful! It's going to be difficult to get ahold of the right kind of horn, so I think it would be best to experiment with bone [first].", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
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
                        return "[Step #1] Continue speaking with Elvar Ironhand about the new bow he's building.  Ask him what you should do [first].";
                    case 2:
                        return "[Step #2] Travel southeast from Cotswold, toward Prydwen keep. You should see skeletons near the graveyard where the river turns east.  Kill the skeletons until you have gathered two Well-Preserved Bones.";
                    case 3:
                        return "[Step #3] You still need one more Well-Preserved Bone.  Continue killing the skeletons west of Prydwen Keep, near the graveyard.";
                    case 4:
                        return "[Step #4] Now that you have gathered both bones, return to Elvar Ironhand in Cotswold and speak to him.";
                    case 5:
                        return "[Step #5] Continue speaking to Elvar about his experimental [technique] for building a composite bow.";
                    default:
                        return "[Step #" + Step + "] No Description entered for this step!";
                }
            }
        }

        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = sender as GamePlayer;

            if (player == null || player.IsDoingQuest(typeof(BuildingABetterBow)) == null)
                return;

            if (e == GameLivingEvent.EnemyKilled)
            {
                if (Step == 2)
                {
                    EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
                    if (gArgs.Target.Name == "skeleton")
                    {
                        if (Util.Chance(50))
                        {
                            player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0x00, 0x00, 0x00, 0x00, eDialogType.Ok, true, "You select a well-preserved bone from the \nremains and place it in your pack. \nYour journal has been updated.");
                            GiveItemToPlayer(CreateQuestItem(wellPreservedBones));
                            ChangeQuestStep(3);
                        }
                        return;
                    }
                }
                else if (Step == 3)
                {
                    EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
                    if (gArgs.Target.Name == "skeleton")
                    {
                        if (Util.Chance(50))
                        {
							RemoveItemFromPlayer(player.Inventory.GetFirstItemByName(wellPreservedBones.Name, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack));
							player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0x00, 0x00, 0x00, 0x00, eDialogType.Ok, true, "You select a well-preserved bone from the \nremains and place it in your pack. \nYour journal has been updated.");
                            GiveItemToPlayer(CreateQuestItem(twoWellPreservedBones));
                            ChangeQuestStep(4);
                        }
                        return;
                    }
                }
            }
            else if (e == GamePlayerEvent.GiveItem)
            {
                if (Step == 4)
                {
                    GiveItemEventArgs gArgs = (GiveItemEventArgs)args;
                    if (gArgs.Target == elvarIronhand && gArgs.Item.QuestName == Name && gArgs.Item.Name == twoWellPreservedBones.Name)
                    {
                        RemoveItemFromPlayer(elvarIronhand, gArgs.Item);

                        elvarIronhand.TurnTo(m_questPlayer);
                        elvarIronhand.SayTo(m_questPlayer, "Hmm...These look a bit more brittle than I was expecting.  I suspect I may end up using horn for the final prototype, after all.  No matter, I'm sure I'll end up making several bows before I start demonstrating the new [technique].");
                        ChangeQuestStep(5);
                    }
                }
            }
        }

        public override void FinishQuest()
        {
            //Give reward to player here ...
            m_questPlayer.GainExperience(145, 0, 0, true);
            m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 0, 50), "You are awarded 50 copper!");

			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...
        }
    }
}
