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
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
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
	/* The first thing we do, is to declare the class we create
	 * as Quest. To do this, we derive from the abstract class
	 * AbstractQuest
	 * 	 
	 */

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
		protected const int minimumLevel = 3;
		protected const int maximumLevel = 6;

		private static GameNPC elvarIronhand = null;

		private static ItemTemplate wellPreservedBones= null;
		private static ItemTemplate twoWellPreservedBones= null;
		
		
		/* We need to define the constructors from the base class here, else there might be problems
		 * when loading this quest...
		 */
		public BuildingABetterBow() : base()
		{
		}

		public BuildingABetterBow(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public BuildingABetterBow(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public BuildingABetterBow(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
		{
		}

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

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Elvar Ironhand", eRealm.Albion);

			/* Whops, if the npcs array length is 0 then no npc exists in
				* this users Mob Database, so we simply create one ;-)
				* else we take the existing one. And if more than one exist, we take
				* the first ...
				*/
			if (npcs.Length == 0)
			{
				elvarIronhand = new GameMob();
				elvarIronhand.Model = 10;
				elvarIronhand.Name = "Elvar Ironhand";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + elvarIronhand.Name + ", creating him ...");
				elvarIronhand.GuildName = "Part of " + questTitle + " Quest";
				elvarIronhand.Realm = (byte) eRealm.Albion;
				elvarIronhand.CurrentRegionID = 1;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.RightHandWeapon, 12);
				elvarIronhand.Inventory = template.CloseTemplate();
				elvarIronhand.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				elvarIronhand.Size = 54;
				elvarIronhand.Level = 17;
				elvarIronhand.X = 561351;
				elvarIronhand.Y = 510292;
				elvarIronhand.Z = 2400;
				elvarIronhand.Heading = 3982;

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database

				if (SAVE_INTO_DATABASE)
					elvarIronhand.SaveIntoDatabase();

				elvarIronhand.AddToWorld();
			}
			else
				elvarIronhand = npcs[0];

			#endregion

			#region defineItems

			// item db check
			wellPreservedBones = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "well_preserved_bone");
			if (wellPreservedBones == null)
			{
				wellPreservedBones = new ItemTemplate();
				wellPreservedBones.Name = "Well-Preserved Bone";
				if (log.IsWarnEnabled)
					log.Warn("Could not find "+wellPreservedBones.Name+", creating it ...");
				
				wellPreservedBones.Level = 0;
				wellPreservedBones.Weight = 1;
				wellPreservedBones.Model = 497;
				
				wellPreservedBones.Object_Type = (int) eObjectType.GenericItem;
				wellPreservedBones.Id_nb = "well_preserved_bone";
				wellPreservedBones.Gold = 0;
				wellPreservedBones.Silver = 0;
				wellPreservedBones.Copper = 0;
				wellPreservedBones.IsPickable = false;
				wellPreservedBones.IsDropable = false;
				
				wellPreservedBones.Quality = 100;
				wellPreservedBones.MaxQuality = 100;
				wellPreservedBones.Condition = 1000;
				wellPreservedBones.MaxCondition = 1000;
				wellPreservedBones.Durability = 1000;
				wellPreservedBones.MaxDurability = 1000;


				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(wellPreservedBones);
			}

			// item db check
			twoWellPreservedBones = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "two_well_preserved_bones");
			if (twoWellPreservedBones == null)
			{
				twoWellPreservedBones = new ItemTemplate();
				twoWellPreservedBones.Name = "Two Well-Preserved Bones";
				if (log.IsWarnEnabled)
					log.Warn("Could not find "+twoWellPreservedBones.Name+", creating it ...");
				
				twoWellPreservedBones.Level = 0;
				twoWellPreservedBones.Weight = 1;
				twoWellPreservedBones.Model = 497;
				
				twoWellPreservedBones.Object_Type = (int) eObjectType.GenericItem;
				twoWellPreservedBones.Id_nb = "two_well_preserved_bones";
				twoWellPreservedBones.Gold = 0;
				twoWellPreservedBones.Silver = 0;
				twoWellPreservedBones.Copper = 0;
				twoWellPreservedBones.IsPickable = false;
				twoWellPreservedBones.IsDropable = false;
				
				twoWellPreservedBones.Quality = 100;
				twoWellPreservedBones.MaxQuality = 100;
				twoWellPreservedBones.Condition = 1000;
				twoWellPreservedBones.MaxCondition = 1000;
				twoWellPreservedBones.Durability = 1000;
				twoWellPreservedBones.MaxDurability = 1000;


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
			
			GameEventMgr.AddHandler(elvarIronhand, GameLivingEvent.Interact, new DOLEventHandler(TalkToElvarIronhand));
			GameEventMgr.AddHandler(elvarIronhand, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToElvarIronhand));

			/* Now we bring to Ydenia the possibility to give this quest to players */
			elvarIronhand.AddQuestToGive(typeof (BuildingABetterBow));

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
			
			GameEventMgr.RemoveHandler(elvarIronhand, GameLivingEvent.Interact, new DOLEventHandler(TalkToElvarIronhand));
			GameEventMgr.RemoveHandler(elvarIronhand, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToElvarIronhand));

			/* Now we remove to Ydenia the possibility to give this quest to players */
			elvarIronhand.RemoveQuestToGive(typeof (BuildingABetterBow));
		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToElvarIronhand(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(elvarIronhand.CanGiveQuest(typeof (BuildingABetterBow), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			BuildingABetterBow quest = player.IsDoingQuest(typeof (BuildingABetterBow)) as BuildingABetterBow;

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
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
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
							player.Out.SendCustomDialog("Will you help Elvar gather the \nmaterials for his prototype bow? \n[Levels 3-6]", new CustomDialogResponse(CheckPlayerAcceptQuest));
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "first":
							if(quest.Step == 1)
							{
								elvarIronhand.SayTo(player, "If you travel southeast from Cotswold, toward Prydwen keep, you should find some skeletons near the bend in the river.  Return to me when you've gathered two well-preserved bones from them.");
								quest.Step = 2;
							}
							break;

						case "technique":
							if(quest.Step == 5)
							{
								elvarIronhand.SayTo(player, "Thank you for your help, "+player.CharacterClass.Name+". Here's a bit of copper for your time. Keep your eyes open for a good source of horn in case the bone prototype doesn't work out.");
								quest.FinishQuest();
							}
							break;	

						case "abort":
							player.Out.SendCustomDialog("Do you really want to abort this quest, \nall items gained during quest will be lost?", new CustomDialogResponse(CheckPlayerAbortQuest));
							break;
					}
				}
			}
		}

		/// <summary>
		/// This method checks if a player qualifies for this quest
		/// </summary>
		/// <returns>true if qualified, false if not</returns>
		public override bool CheckQuestQualification(GamePlayer player)
		{
			// if the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(typeof (BuildingABetterBow)) != null)
				return true;

			// This checks below are only performed is player isn't doing quest already

			if (player.Level < minimumLevel || player.Level > maximumLevel)
				return false;

			return true;
		}

		
		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest offer dialog. We check if he accepts or
		 * declines here...
		 */

		private static void CheckPlayerAbortQuest(GamePlayer player, byte response)
		{
			BuildingABetterBow quest = player.IsDoingQuest(typeof (BuildingABetterBow)) as BuildingABetterBow;

			if (quest == null)
				return;

			if (response == 0x00)
			{
				SendSystemMessage(player, "Good, no go out there and finish your work!");
			}
			else
			{
				SendSystemMessage(player, "Aborting Quest " + questTitle + ". You can start over again if you want.");
				quest.AbortQuest();
			}
		}

		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest offer dialog. We check if he accepts or
		 * declines here...
		 */

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			//We recheck the qualification, because we don't talk to players
			//who are not doing the quest
			if(elvarIronhand.CanGiveQuest(typeof (BuildingABetterBow), player)  <= 0)
				return;

			if (player.IsDoingQuest(typeof (BuildingABetterBow)) != null)
				return;

			if (response == 0x00)
			{
				SendReply(player, "Oh well, if you change your mind, please come back!");
			}
			else
			{
				//Check if we can add the quest!
				if (!elvarIronhand.GiveQuest(typeof (BuildingABetterBow), player, 1))
					return;

				SendReply(player, "Wonderful! It's going to be difficult to get ahold of the right kind of horn, so I think it would be best to experiment with bone [first].");
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
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof (BuildingABetterBow)) == null)
				return;

			if (e == GameLivingEvent.EnemyKilled)
			{
				if(Step == 2)
				{
					EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
					if (gArgs.Target.Name == "skeleton")
					{
						if (Util.Chance(50))
						{
							player.Out.SendSimpleWarningDialog("You select a well-preserved bone from the \nremains and place it in your pack. \nYour journal has been updated.");
							GiveItem(gArgs.Target, player, wellPreservedBones);
							Step = 3;
						}
						return;
					}
				}
				else if(Step == 3)
				{
					EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
					if (gArgs.Target.Name == "skeleton")
					{
						if (Util.Chance(50))
						{
							player.Out.SendSimpleWarningDialog("You select a well-preserved bone from the \nremains and place it in your pack. \nYour journal has been updated.");
							ReplaceItem(player, wellPreservedBones, twoWellPreservedBones);
							Step = 4;
						}
						return;
					}
				}
			}
			else if (e == GamePlayerEvent.GiveItem)
			{
				if(Step == 4)
				{
					GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
					if (gArgs.Target.Name == elvarIronhand.Name && gArgs.Item.Id_nb == twoWellPreservedBones.Id_nb)
					{
						RemoveItem(elvarIronhand, m_questPlayer, twoWellPreservedBones);

						elvarIronhand.TurnTo(m_questPlayer);
						elvarIronhand.SayTo(m_questPlayer, "Hmm...These look a bit more brittle than I was expecting.  I suspect I may end up using horn for the final prototype, after all.  No matter, I'm sure I'll end up making several bows before I start demonstrating the new [technique].");
						Step = 5;
					}
				}
			}
		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, wellPreservedBones, false);
			RemoveItem(m_questPlayer, twoWellPreservedBones, false);
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			//Give reward to player here ...
			m_questPlayer.GainExperience(145, 0, 0, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 0, 50), "You are awarded 50 copper!");
		}
	}
}
