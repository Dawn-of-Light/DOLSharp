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
 * Do this quest early to get the most out of the cloak, but don't give it away -
 * you might need it again later (hint). 
 *
 * The trip between Ludlow and Humberton is a lot faster if you run over the hill instead 
 * of going around it - just watch out for boulderlings.
 * 
 * Talk to Steward Willie to receieve this quest. The objective it fairly simple, leave
 * the village and hunt down a wolf pup. You'll find them right outside near the road to Camelot. 
 *
 * Although you can get the quest at level 1, the wolf will be yellow to you until level 2.
 * Take a friend along or hunt around Humberton a bit before trying the pup. 
 *
 * After killing the wolf, take the pelt back to Steward Willie, and you'll receive a Wolf Head Token.
 * Take this over the hill to Seamstress Lynnet in Ludlow, and she'll make you the Wolf Pelt Cloak.
 */

using System;
using System.Reflection;
using DOL.GS.Database;
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
	/* The first thing we do, is to declare the quest requirement
	* class linked with the new Quest. To do this, we derive 
	* from the abstract class AbstractQuestDescriptor
	*/
	public class WolfPeltCloakDescriptor : AbstractQuestDescriptor
	{
		/* This is the type of the quest class linked with 
		 * this requirement class, you must override the 
		 * base methid like that
		 */
		public override Type LinkedQuestType
		{
			get { return typeof(WolfPeltCloak); }
		}
	}

	/* The second thing we do, is to declare the class we create
	 * as Quest. We must make it persistant using attributes, to
	 * do this, we derive from the abstract class AbstractQuest
	 */
	[NHibernate.Mapping.Attributes.Subclass(NameType = typeof(WolfPeltCloak), ExtendsType = typeof(AbstractQuest))]
	public class WolfPeltCloak : BaseQuest
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
		 * We store our two mobs as static variables, since we need them
		 */

		protected const string questTitle = "Wolf Pelt Cloak";

		private static GameNPC stewardWillie = null;
		private static GameNPC lynnet = null;

		private static GameNPC don = null;

		private static CloakTemplate wolfPeltCloak = null;
		private static GenericItemTemplate wolfFur = null;
		private static GenericItemTemplate wolfHeadToken = null;

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
		 * want. We will do it the standard way here ... and make Sir Quait wail
		 * a bit about the loss of his sword! 
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

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Steward Willie", eRealm.Albion);

			/* Whops, if the npcs array length is 0 then no Sir Quait exists in
				* this users Mob Database, so we simply create one ;-)
				* else we take the existing one. And if more than one exist, we take
				* the first ...
				*/
			if (npcs.Length == 0)
			{
				stewardWillie = new GameMob();
				stewardWillie.Model = 27;
				stewardWillie.Name = "Steward Willie";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + stewardWillie.Name + ", creating him ...");
				stewardWillie.GuildName = "Part of " + questTitle;
				stewardWillie.Realm = (byte) eRealm.Albion;
				stewardWillie.RegionId = 1;
				stewardWillie.Size = 52;
				stewardWillie.Level = 35;
				stewardWillie.Position = new Point(503547, 474330, 2788);
				stewardWillie.Heading = 3163;

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					stewardWillie.SaveIntoDatabase();


				stewardWillie.AddToWorld();
			}
			else
				stewardWillie = npcs[0];

			/* Now we do the same for the Lynnet.
			*/
			npcs = WorldMgr.GetNPCsByName("Seamstress Lynnet", eRealm.Albion);
			if (npcs.Length == 0)
			{
				lynnet = new GameMob();
				lynnet.Model = 5;
				lynnet.Name = "Seamstress Lynnet";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + lynnet.Name + ", creating ...");
				lynnet.GuildName = "Part of " + questTitle;
				lynnet.Realm = (byte) eRealm.Albion; //Needs to be none, else we can't kill him ;-)
				lynnet.RegionId = 1;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 58, 30, 0);
				lynnet.Inventory = template.CloseTemplate();

//				lynnet.AddNPCEquipment((byte) eEquipmentItems.TORSO, 58, 30, 0, 0);
				lynnet.Size = 48;
				lynnet.Level = 15;
				lynnet.Position = new Point(530112, 478662, 2200);
				lynnet.Heading = 3203;

				//You don't have to store the creted mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					lynnet.SaveIntoDatabase();

				lynnet.AddToWorld();
			}
			else
				lynnet = npcs[0];

			npcs = WorldMgr.GetNPCsByName("Brother Don", eRealm.Albion);
			if (npcs.Length == 0)
			{
				don = new GameMob();
				don.Model = 34;
				don.Name = "Brother Don";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + don.Name + ", creating ...");
				don.GuildName = "Part of " + questTitle;
				don.Realm = (byte) eRealm.Albion; //Needs to be none, else we can't kill him ;-)
				don.RegionId = 1;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 58, 44, 0);
				don.Inventory = template.CloseTemplate();

//				don.AddNPCEquipment((byte) eEquipmentItems.TORSO, 58, 44, 0, 0);
				don.Size = 48;
				don.Level = 15;
				don.Position = new Point(505411, 495024, 2495);
				don.Heading = 2048;

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					don.SaveIntoDatabase();

				don.AddToWorld();
			}
			else
				don = npcs[0];

			#endregion

			#region defineItems

			wolfPeltCloak = (CloakTemplate) GameServer.Database.FindObjectByKey(typeof (CloakTemplate), "wolf_pelt_cloak");
			if (wolfPeltCloak == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Wolf Pelt Cloak, creating it ...");
				wolfPeltCloak = new CloakTemplate();
				wolfPeltCloak.Name = "Wolf Pelt Cloak";
				wolfPeltCloak.Level = 3;
				wolfPeltCloak.Weight = 3;
				wolfPeltCloak.Model = 326;
				wolfPeltCloak.Bonus = 1;

				wolfPeltCloak.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 1));
				//TODO how do you do negative numbers for magicalbonuses?
				//wolfPeltCloak.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Charisma, -1));

				wolfPeltCloak.ItemTemplateID = "wolf_pelt_cloak";
				wolfPeltCloak.Value = Money.GetMoney(0, 0, 0, 4, 3);

				wolfPeltCloak.IsDropable = true;
				wolfPeltCloak.IsSaleable = true;
				wolfPeltCloak.IsTradable = true;
				wolfPeltCloak.Color = 44;


				//You don't have to store the created wolfPeltCloak in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(wolfPeltCloak);
			}

			wolfFur = (GenericItemTemplate) GameServer.Database.FindObjectByKey(typeof (GenericItemTemplate), "wolf_fur");
			if (wolfFur == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Wolf Fur, creating it ...");
				wolfFur = new GenericItemTemplate();
				wolfFur.ItemTemplateID = "wolf_fur";
				wolfFur.Name = "Wolf Fur";
				wolfFur.Level = 1;
				wolfFur.Model = 57;
				wolfFur.IsDropable = false;
				wolfFur.IsSaleable = false;
				wolfFur.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(wolfFur);
			}

			wolfHeadToken = (GenericItemTemplate) GameServer.Database.FindObjectByKey(typeof (GenericItemTemplate), "wolf_head_token");
			if (wolfHeadToken == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Wolf Head Token, creating it ...");
				wolfHeadToken = new GenericItemTemplate();
				wolfHeadToken.ItemTemplateID = "wolf_head_token";
				wolfHeadToken.Name = "Wolf Head Token";
				wolfHeadToken.Level = 1;
				wolfHeadToken.Model = 1366;
				wolfHeadToken.IsDropable = false;
				wolfHeadToken.IsSaleable = false;
				wolfHeadToken.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(wolfHeadToken);
			}

			#endregion

			/* Now we add some hooks to the Sir Quait we found.
				* Actually, we want to know when a player interacts with him.
				* So, we hook the right-click (interact) and the whisper method
				* of Sir Quait and set the callback method to the "TalkToXXX"
				* method. This means, the "TalkToXXX" method is called whenever
				* a player right clicks on him or when he whispers to him.
				*/
			GameEventMgr.AddHandler(stewardWillie, GameLivingEvent.Interact, new DOLEventHandler(TalkToStewardWillie));
			GameEventMgr.AddHandler(stewardWillie, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToStewardWillie));

			GameEventMgr.AddHandler(don, GameLivingEvent.Interact, new DOLEventHandler(TalkToBrotherDon));
			GameEventMgr.AddHandler(don, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToBrotherDon));

			GameEventMgr.AddHandler(lynnet, GameLivingEvent.Interact, new DOLEventHandler(TalkToSeamstressLynnet));

			/* Now we bring to Ydenia the possibility to give this quest to players */
			QuestMgr.AddQuestDescriptor(stewardWillie, typeof(WolfPeltCloakDescriptor));

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
			/* If sirQuait has not been initialized, then we don't have to remove any
			 * hooks from him ;-)
			 */
			if (stewardWillie == null)
				return;

			/* Removing hooks works just as adding them but instead of 
			 * AddHandler, we call RemoveHandler, the parameters stay the same
			 */
			GameEventMgr.RemoveHandler(stewardWillie, GameObjectEvent.Interact, new DOLEventHandler(TalkToStewardWillie));
			GameEventMgr.RemoveHandler(stewardWillie, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToStewardWillie));

			GameEventMgr.RemoveHandler(don, GameLivingEvent.Interact, new DOLEventHandler(TalkToBrotherDon));
			GameEventMgr.RemoveHandler(don, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToBrotherDon));

			GameEventMgr.RemoveHandler(lynnet, GameObjectEvent.Interact, new DOLEventHandler(TalkToSeamstressLynnet));

			/* Now we remove to stewardWillie the possibility to give this quest to players */
			QuestMgr.RemoveQuestDescriptor(stewardWillie, typeof(AgainstTheGrainDescriptor));
		}

		/* This is the method we declared as callback for the hooks we set to
		 * Sir Quait. It will be called whenever a player right clicks on Sir Quait
		 * or when he whispers something to him.
		 */

		protected static void TalkToStewardWillie(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if (QuestMgr.CanGiveQuest(typeof(WolfPeltCloak), player, stewardWillie) <= 0)
				return;

			//We also check if the player is already doing the quest
			WolfPeltCloak quest = player.IsDoingQuest(typeof (WolfPeltCloak)) as WolfPeltCloak;

			stewardWillie.TurnTo(player);
			//Did the player rightclick on Sir Quait?
			if (e == GameObjectEvent.Interact)
			{
				//We check if the player is already doing the quest
				if (quest != null)
				{
					//If the player is already doing the quest, we ask if he found the fur!
					if (player.Inventory.GetFirstItemByName(wolfFur.Name, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack) != null)
						stewardWillie.SayTo(player, "Ah, well done! His Lordship will be pleased to know there is one less mongrel in the pack! Give me the fur so I can throw it with the others.");
					else if (player.Inventory.GetFirstItemByName(wolfHeadToken.Name, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack) != null)
						stewardWillie.SayTo(player, "Give the token to Seamstress Lynnet in Ludlow, she'll give ye your reward. Thank ye for your fine services to His Lordship.");
					else
						stewardWillie.SayTo(player, "Good! I know we ca'count on ye. I will reward ye for the pelt ye bring me from one of those vile beasts!");
					return;
				}
				else
				{
					stewardWillie.SayTo(player, "Aye, hello there! Have ye been sent t'help with our [problem]");
					return;
				}
			}
				// The player whispered to Sir Quait (clicked on the text inside the [])
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;

				//We also check if the player is already doing the quest
				if (quest == null)
				{
					switch (wArgs.Text)
					{
						case "problem":
							stewardWillie.SayTo(player, "What? Ye haven't heard? Hhhmm, then I wonder if ye would [like to help]");
							break;
						case "pack of wolves":
							stewardWillie.SayTo(player, "There should be some around the area of this village, take a look near the road to Camelot. Kill any wolf pups you can find, and bring me its fur.");
							break;
						case "like to help":
							stewardWillie.SayTo(player, "That's wonderful! We've been havin' a serious problem with a [pack of wolves]. His Lordship wants'em eliminated because they have been a-bothering the people here about. His Lordship has authorized me to reward those who [serve him well].");
							break;
							//If the player offered his "help", we send the quest dialog now!
						case "serve him well":
							player.Out.SendCustomDialog("Do you accept the Wolf Pelt Cloak quest?", new CustomDialogResponse(CheckPlayerAcceptQuest));
							break;
					}
				}
			}
		}

		/* This is the method we declared as callback for the hooks we set to
		 * Sir Quait. It will be called whenever a player right clicks on Sir Quait
		 * or when he whispers something to him.
		 */

		protected static void TalkToSeamstressLynnet(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if (QuestMgr.CanGiveQuest(typeof(WolfPeltCloak), player, stewardWillie) <= 0)
				return;

			//We also check if the player is already doing the quest
			WolfPeltCloak quest = player.IsDoingQuest(typeof (WolfPeltCloak)) as WolfPeltCloak;

			lynnet.TurnTo(player);
			//Did the player rightclick on Sir Quait?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					//If the player is already doing the quest, we ask if he found the sword!                    
					lynnet.SayTo(player, "I hear you have a token for me, as proof of your valuable work for his Lordship. Give it to me and I will reward you.");
				}
			}
		}

		/* This is the method we declared as callback for the hooks we set to
		 * Sir Quait. It will be called whenever a player right clicks on Sir Quait
		 * or when he whispers something to him.
		 */

		protected static void TalkToBrotherDon(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			//Did the player rightclick on Sir Quait?
			if (e == GameObjectEvent.Interact)
			{
				//If the player qualifies, we begin talking...
				if (player.Inventory.GetFirstItemByName(wolfPeltCloak.Name, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack) != null)
					don.SayTo(player, "Hail! You don't perhaps have one of those fine wolf pelt cloaks? If you no longer have need of it, we could greatly use it at the [orphanage].");
				return;
			}
				// The player whispered to Sir Quait (clicked on the text inside the [])
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				switch (wArgs.Text)
				{
					case "orphanage":
						don.SayTo(player, "Why yes, the little ones can get an awful chill during the long cold nights, so the orphanage could use a good [donation] of wolf cloaks. I would take any that you have.");
						break;
					case "donation":
						don.SayTo(player, "Do you want to donate your cloak?");  // TODO : Seems not ended
						break;
				}
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
			if (QuestMgr.CanGiveQuest(typeof(WolfPeltCloak), player, stewardWillie) <= 0)
				return;

			if (player.IsDoingQuest(typeof (WolfPeltCloak)) != null)
				return;

			if (response == 0x00)
			{
				SendReply(player, "Oh well, if you change your mind, please come back!");
			}
			else
			{
				//Check if we can add the quest
				if (!QuestMgr.GiveQuestToPlayer(typeof(WolfPeltCloak), player, stewardWillie))
					return;

				stewardWillie.SayTo(player, "Good! I know we ca'count on ye. I will reward ye for the pelt ye bring me from one of those vile beasts!");
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
						return "[Step #1] Go out into the fields to hunt a wolf pup and flay its fur.";
					case 2:
						return "[Step #2] Bring the fur back to Steward Willie in Humberton Fort.";
					case 3:
						return "[Step #3] Go to Seamstress Lynnie in Ludlow and bring her the wolf head token.";
					default:
						return "[Step #" + Step + "] No Description entered for this step!";
				}
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null)
				return;

			// brother don
			if (e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == don.Name && gArgs.Item.Name == wolfPeltCloak.Name)
				{
					don.SayTo(player, "Thank you! Your service to the church will been noted!");
					RemoveItemFromPlayer(don, wolfPeltCloak);
					don.SayTo(player, "Well done! You've helped the children get over the harsh winter.");

					//Give reward to player here ...
					player.GainExperience(200, 0, 0, true);

					return;
				}
			}

			if (player.IsDoingQuest(typeof (WolfPeltCloak)) == null)
				return;

			if (Step == 1 && e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
				if (gArgs.Target.Name.IndexOf("wolf") >= 0)
				{
					SendSystemMessage("You've killed the " + gArgs.Target.Name + " and flayed the fur from it.!");
					wolfFur.Name = gArgs.Target.GetName(1, true) + " fur";
					GiveItemToPlayer(wolfFur.CreateInstance());
					Step = 2;
					return;
				}
			}
			else if (Step == 2 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == stewardWillie.Name && gArgs.Item.Name == wolfFur.Name)
				{
					stewardWillie.TurnTo(m_questPlayer);
					stewardWillie.SayTo(m_questPlayer, "Take this token from His Lordship. If ye give it to Seamstress Lynnet in Ludlow, she'll give ye your reward. Thank ye for your fine services to His Lordship.");

					RemoveItemFromPlayer(stewardWillie, wolfFur);
					GiveItemToPlayer(stewardWillie, wolfHeadToken.CreateInstance());
					Step = 3;
					return;
				}
			}
			else if (Step == 3 && e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				if (gArgs.Target.Name == lynnet.Name && gArgs.Item.Name == wolfHeadToken.Name)
				{
					RemoveItemFromPlayer(lynnet, wolfHeadToken);
					lynnet.SayTo(player, "Well done! Here's your fine wolf pelt cloak. Wear it with pride knowing you have helped his Lordship.");
					FinishQuest();
					return;
				}
			}
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			//Give reward to player here ...

			GiveItemToPlayer(lynnet, wolfPeltCloak.CreateInstance());

			m_questPlayer.GainExperience(50, 0, 0, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 0, 30 + Util.Random(50)), "You recieve {0} for your service.");

		}

	}
}
