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
 * 1) Travel to loc=11305,30802 Camelot Hills (Cotswold Village) to speak with Eowyln Astos
 * 2) Go to loc=12320,43841 Camelot Hills and kill some river sprite until Sephucoth pop
 * 2) Kill Sephucoth to have his heart and comme back to Eowyln Astos to give it
 * 3) Go to loc=25268,36339 and kill some large skeleton to drop a polished bone
 * 4) Come back to Camelot Hills and give the bone to Eowyln Astos to have your reward
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

	public class HeartOfSephucoth : BaseQuest
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
		protected const string questTitle = "Heart of Sephucoth";
		protected const int minimumLevel = 7;
		protected const int maximumLevel = 14;

		private static GameNPC eowylnAstos = null;
		private static GameNPC sephucoth = null;
		
		private static ItemTemplate sephucothsHeart = null;
		private static ItemTemplate polishedBone = null;

		private static ItemTemplate fieryCrystalPendant = null;
		
		
		/* We need to define the constructors from the base class here, else there might be problems
		 * when loading this quest...
		 */
		public HeartOfSephucoth() : base()
		{
		}

		public HeartOfSephucoth(GamePlayer questingPlayer) : this(questingPlayer, 1)
		{
		}

		public HeartOfSephucoth(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public HeartOfSephucoth(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
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
			if (!ServerProperties.Properties.LOAD_QUESTS)
				return;
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

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Eowyln Astos", eRealm.Albion);

			/* Whops, if the npcs array length is 0 then no npc exists in
				* this users Mob Database, so we simply create one ;-)
				* else we take the existing one. And if more than one exist, we take
				* the first ...
				*/
			if (npcs.Length == 0)
			{
				eowylnAstos = new GameNPC();
				eowylnAstos.Model = 35;
				eowylnAstos.Name = "Eowyln Astos";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + eowylnAstos.Name + ", creating him ...");
				eowylnAstos.GuildName = "Part of " + questTitle + " Quest";
				eowylnAstos.Realm = eRealm.Albion;
				eowylnAstos.CurrentRegionID = 1;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 58, 40);
				eowylnAstos.Inventory = template.CloseTemplate();
				eowylnAstos.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				eowylnAstos.Size = 54;
				eowylnAstos.Level = 17;
				eowylnAstos.X = 559680;
				eowylnAstos.Y = 513793;
				eowylnAstos.Z = 2619;
				eowylnAstos.Heading = 3185;

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database

				if (SAVE_INTO_DATABASE)
					eowylnAstos.SaveIntoDatabase();

				eowylnAstos.AddToWorld();
			}
			else
				eowylnAstos = npcs[0];

			#endregion

			#region defineItems

			// item db check
			sephucothsHeart = GameServer.Database.FindObjectByKey<ItemTemplate>("sephucoths_heart");
			if (sephucothsHeart == null)
			{
				sephucothsHeart = new ItemTemplate();
				sephucothsHeart.Name = "Sephucoth's Heart";
				if (log.IsWarnEnabled)
					log.Warn("Could not find "+sephucothsHeart.Name+", creating it ...");
				sephucothsHeart.Level = 0;
				sephucothsHeart.Weight = 0;
				sephucothsHeart.Model = 595;

				sephucothsHeart.Object_Type = (int) eObjectType.GenericItem;
				sephucothsHeart.Id_nb = "sephucoths_heart";
				sephucothsHeart.Price = 0;
				sephucothsHeart.IsPickable = false;
				sephucothsHeart.IsDropable = false;
				
				sephucothsHeart.Quality = 100;
				sephucothsHeart.Condition = 1000;
				sephucothsHeart.MaxCondition = 1000;
				sephucothsHeart.Durability = 1000;
				sephucothsHeart.MaxDurability = 1000;


				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				
					GameServer.Database.AddObject(sephucothsHeart);
			}

			// item db check
			polishedBone = GameServer.Database.FindObjectByKey<ItemTemplate>("polished_bone");
			if (polishedBone == null)
			{
				polishedBone = new ItemTemplate();
				polishedBone.Name = "Polished Bone";
				if (log.IsWarnEnabled)
					log.Warn("Could not find "+polishedBone.Name+", creating it ...");
				polishedBone.Level = 0;
				polishedBone.Weight = 15;
				polishedBone.Model = 497;

				polishedBone.Object_Type = (int) eObjectType.GenericItem;
				polishedBone.Id_nb = "polished_bone";
				polishedBone.Price = 0;
				polishedBone.IsPickable = false;
				polishedBone.IsDropable = false;
				
				polishedBone.Quality = 100;
				polishedBone.Condition = 1000;
				polishedBone.MaxCondition = 1000;
				polishedBone.Durability = 1000;
				polishedBone.MaxDurability = 1000;


				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				
					GameServer.Database.AddObject(polishedBone);
			}

			// item db check
			fieryCrystalPendant = GameServer.Database.FindObjectByKey<ItemTemplate>("fiery_crystal_pendant");
			if (fieryCrystalPendant == null)
			{
				fieryCrystalPendant = new ItemTemplate();
				fieryCrystalPendant.Name = "Fiery Crystal Pendant";
				if (log.IsWarnEnabled)
					log.Warn("Could not find "+fieryCrystalPendant.Name+", creating it ...");
			
				fieryCrystalPendant.Level = 8;
				fieryCrystalPendant.Weight = 8;
				fieryCrystalPendant.Model = 101;
			
				fieryCrystalPendant.Object_Type = (int) eObjectType.Magical;
				fieryCrystalPendant.Item_Type = (int) eEquipmentItems.NECK;
				fieryCrystalPendant.Id_nb = "fiery_crystal_pendant";
				fieryCrystalPendant.Price = Money.GetMoney(0,0,0,0,30);
				fieryCrystalPendant.IsPickable = true;
				fieryCrystalPendant.IsDropable = true;

				fieryCrystalPendant.Bonus1 = 1;
				fieryCrystalPendant.Bonus1Type = (int)eProperty.Skill_Fire;
				fieryCrystalPendant.Bonus2 = 1;
				fieryCrystalPendant.Bonus2Type = (int)eProperty.Intelligence;
			
				fieryCrystalPendant.Quality = 100;
				fieryCrystalPendant.Condition = 1000;
				fieryCrystalPendant.MaxCondition = 1000;
				fieryCrystalPendant.Durability = 1000;
				fieryCrystalPendant.MaxDurability = 1000;


				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				
					GameServer.Database.AddObject(fieryCrystalPendant);
			}

			#endregion


			/* Now we add some hooks to the npc we found.
			* Actually, we want to know when a player interacts with him.
			* So, we hook the right-click (interact) and the whisper method
			* of npc and set the callback method to the "TalkToXXX"
			* method. This means, the "TalkToXXX" method is called whenever
			* a player right clicks on him or when he whispers to him.
			*/

			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));
			
			GameEventMgr.AddHandler(eowylnAstos, GameLivingEvent.Interact, new DOLEventHandler(TalkToEowylnAstos));
			GameEventMgr.AddHandler(eowylnAstos, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToEowylnAstos));

			/* Now we bring to Yetta Fletcher the possibility to give this quest to players */
			eowylnAstos.AddQuestToGive(typeof (HeartOfSephucoth));

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
			/* If Yetta Fletcher has not been initialized, then we don't have to remove any
			 * hooks from him ;-)
			 */
			if (eowylnAstos == null)
				return;

			/* Removing hooks works just as adding them but instead of 
			 * AddHandler, we call RemoveHandler, the parameters stay the same
			 */

			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(eowylnAstos, GameLivingEvent.Interact, new DOLEventHandler(TalkToEowylnAstos));
			GameEventMgr.RemoveHandler(eowylnAstos, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToEowylnAstos));

			/* Now we remove to Yetta Fletcher the possibility to give this quest to players */
			eowylnAstos.RemoveQuestToGive(typeof (HeartOfSephucoth));
		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToEowylnAstos(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if(eowylnAstos.CanGiveQuest(typeof (HeartOfSephucoth), player)  <= 0)
				return;

			//We also check if the player is already doing the quest
			HeartOfSephucoth quest = player.IsDoingQuest(typeof (HeartOfSephucoth)) as HeartOfSephucoth;

			eowylnAstos.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					eowylnAstos.SayTo(player, "Hail traveler! I may have a bit of [profitable information] for you!");
					return;
				}
				else
				{
					switch(quest.Step)
					{
						case 1:	
							eowylnAstos.SayTo(player, "You must seek out the monster Sephucoth! Slay it and bring me its heart!");
							break;

						case 2:
							eowylnAstos.SayTo(player, "Hand to me the heart needed for this construct.");
							break;

						case 4:
							eowylnAstos.SayTo(player, "Hand to me the polished bone needed for this construct.");
							break;
					}
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
						case "profitable information":
							eowylnAstos.SayTo(player, "I have learned how to [fashion a pendant] of immense value.");
							break;

						case "fashion a pendant":
							eowylnAstos.SayTo(player, "I can do so, but I would require the heart from a [terrible beast].");
							break;
						
							//If the player offered his help, we send the quest dialog now!
						case "terrible beast":
							player.Out.SendQuestSubscribeCommand(eowylnAstos, QuestMgr.GetIDForQuestType(typeof(HeartOfSephucoth)), "Do you accept the \nHeart of Sephucoth quest? \n[Levels 7-10]");
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "abort":
							player.Out.SendCustomDialog("Do you really want to abort this quest, \nall items gained during quest will be lost?", new CustomDialogResponse(CheckPlayerAbortQuest));
							break;
					}
				}
			}
		}

		protected static void SubscribeQuest(DOLEvent e, object sender, EventArgs args)
		{
			QuestEventArgs qargs = args as QuestEventArgs;
			if (qargs == null)
				return;

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(HeartOfSephucoth)))
				return;

			if (e == GamePlayerEvent.AcceptQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x01);
			else if (e == GamePlayerEvent.DeclineQuest)
				CheckPlayerAcceptQuest(qargs.Player, 0x00);
		}

		/// <summary>
		/// This method checks if a player qualifies for this quest
		/// </summary>
		/// <returns>true if qualified, false if not</returns>
		public override bool CheckQuestQualification(GamePlayer player)
		{
			// if the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(typeof (HeartOfSephucoth)) != null)
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
			HeartOfSephucoth quest = player.IsDoingQuest(typeof (HeartOfSephucoth)) as HeartOfSephucoth;

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
			if(eowylnAstos.CanGiveQuest(typeof (HeartOfSephucoth), player)  <= 0)
				return;

			if (player.IsDoingQuest(typeof (HeartOfSephucoth)) != null)
				return;

			if (response == 0x00)
			{
				SendReply(player, "Oh well, if you change your mind, please come back!");
			}
			else
			{
				//Check if we can add the quest!
				if (!eowylnAstos.GiveQuest(typeof (HeartOfSephucoth), player, 1))
					return;

				SendReply(player, "Aye, the locals have given it the name Sephucoth. Return its heart to me and I will instruct you further.");
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
						return "[Step #1] Slay the river sprite Sephucoth and return to Eowyln Astos with his heart.";
					case 2:
						return "[Step #2] Give Sephucoth's heart to Eowyln.";
					case 3:
						return "[Step #3] Retrieve a piece of polished bone from a large skeleton.";
					case 4:
						return "[Step #4] Return the polished bone to Eowyln.";
				}
				return base.Description;
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof (HeartOfSephucoth)) == null)
				return;

			if (e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
				if(Step == 1)
				{
					if (gArgs.Target.Name == "river sprite")
					{
						if (Util.Chance(25))
						{
							if(sephucoth == null)
							{
								sephucoth = new GameNPC();
								sephucoth.Model = 136;
								sephucoth.Name = "Sephucoth";
								sephucoth.Realm = eRealm.None;
								sephucoth.CurrentRegionID = 1;

								sephucoth.Size = 55;
								sephucoth.Level = 7;
								sephucoth.X = 560836;
								sephucoth.Y = 527260;
								sephucoth.Z = 2082;
								sephucoth.Heading = 1480;

								StandardMobBrain brain = new StandardMobBrain();  // set a brain witch find a lot mob friend to attack the player
								sephucoth.SetOwnBrain(brain);					  // so this mob must be abble to cast 

								sephucoth.RespawnInterval = 0; // don't respawn when killed

								sephucoth.AddToWorld();
							}
						}
					}
					else if (gArgs.Target.Name == "Sephucoth")
					{
						GiveItem(gArgs.Target, player, sephucothsHeart);
						if(sephucoth != null) { sephucoth = null; }

						Step = 2;
					}
				}
				else if(Step == 3)
				{
					if (gArgs.Target.Name == "large skeleton")
					{
						if (Util.Chance(50))
						{
							GiveItem(gArgs.Target, player, polishedBone);
							Step = 4;
						}
					}
				}
			}
			else if (e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == eowylnAstos.Name)
				{
					if(gArgs.Item.Id_nb == sephucothsHeart.Id_nb && Step == 2)
					{
						RemoveItem(eowylnAstos, m_questPlayer, sephucothsHeart);

						eowylnAstos.TurnTo(m_questPlayer);
						eowylnAstos.SayTo(m_questPlayer, "You have done well traveler! I will still require one final object to complete the pendant. Seek out a large skeleton and bring from it a piece of polished bone! Return this to me and I shall finish your pendant.");
						Step = 3;
					}
					else if(gArgs.Item.Id_nb == polishedBone.Id_nb && Step == 4)
					{
						RemoveItem(eowylnAstos, m_questPlayer, polishedBone);

						eowylnAstos.TurnTo(m_questPlayer);
						eowylnAstos.SayTo(m_questPlayer, "Eowyln draws two items before her. Gathering her strength, she shouts.");
						
						new RegionTimer(eowylnAstos, new RegionTimerCallback(BuildNecklace), 5000);
					}
				}
			}
		}

		protected virtual int BuildNecklace(RegionTimer callingTimer)
		{
			m_questPlayer.Out.SendEmoteAnimation(eowylnAstos, eEmote.Yes);
			SendMessage(m_questPlayer, "Eowyln carefully fashions a delicate necklace about the crystal and smiles.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
							
			eowylnAstos.SayTo(m_questPlayer, "Here is your pendant. I thank you for allowing me the opportunity to create such a great item!");
		
			GiveItem(eowylnAstos, m_questPlayer, fieryCrystalPendant);
							
			FinishQuest();
			return 0;
		}
						

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			RemoveItem(m_questPlayer, sephucothsHeart, false);
			RemoveItem(m_questPlayer, polishedBone, false);
		}
	}
}
	
