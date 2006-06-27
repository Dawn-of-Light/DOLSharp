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
 * 1) Travel to loc=12261,29141 Camelot Hills (Cotswold village) to speak with Madissair.
 * 2) Take his letter and travel to loc= 27717,19000 and give the poem to Serawen.
 * 3) Come back in Cotswold village and speak with Madissair to have have your reward.
 * (there is 4 way to do this quest and here is the worse one)
 */

using System;
using System.Reflection;
using DOL.AI.Brain;
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
	public class HalfOgreAllManDescriptor : AbstractQuestDescriptor
	{
		/* This is the type of the quest class linked with 
		 * this requirement class, you must override the 
		 * base methid like that
		 */
		public override Type LinkedQuestType
		{
			get { return typeof(HalfOgreAllMan); }
		}

		/* This value is used to retrieves the minimum level needed
		 *  to be able to make this quest. Override it only if you need, 
		 * the default value is 1
		 */
		public override int MinLevel
		{
			get { return 5; }
		}
	}

	/* The second thing we do, is to declare the class we create
	 * as Quest. We must make it persistant using attributes, to
	 * do this, we derive from the abstract class AbstractQuest
	 */
	[NHibernate.Mapping.Attributes.Subclass(NameType = typeof(HalfOgreAllMan), ExtendsType = typeof(AbstractQuest))]
	public class HalfOgreAllMan : BaseQuest
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
		protected const string questTitle = "Half Ogre, All Man";

		private static GameNPC madissair = null;
		private static GameNPC eileenMorton = null;
		private static GameNPC scribeVeral = null;
		private static GameNPC serawen = null;
		
		private static GenericItemTemplate sealedLovePoem = null;
		private static GenericItemTemplate beautifulRedRose = null;

		private static GenericItemTemplate adnilsMagicalOrb = null;
		/*
		2: "  (Adnil's Magical Orb dispenses advice to those seeking counsel on various matters. For advice on romance, /use the item. For advice on warfare, /use2. For general advice (yes and no questions), /interact with the item.)"
		3: " "
		4: "Magical Ability:"
		5: "  (Adnil's Magical Orb dispenses advice to those seeking counsel on various matters. For advice on romance, /use the item. For advice on warfare, /use2. For general advice (yes and no questions), /interact with the item.)"
		6: "- This spell is cast when the item is used."
		7: " "
		8: " "
		9: "Secondary Magical Ability:"
		10: "  (Adnil's Magical Orb dispenses advice to those seeking counsel on various matters. For advice on romance, /use the item. For advice on warfare, /use2. For general advice (yes and no questions), /interact with the item.)"
		11: "- This spell is cast when the item is used."
		12: " "
		13: " "
		14: "Cannot be traded to other players."
		15: "Cannot be sold to merchants."
		*/

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

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Madissair", eRealm.Albion);
			if (npcs.Length == 0)
			{
				madissair = new GameMob();
				madissair.Model = 1105;
				madissair.Name = "Madissair";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + madissair.Name + ", creating him ...");
				madissair.GuildName = "Part of " + questTitle + " Quest";
				madissair.Realm = (byte) eRealm.Albion;
				madissair.RegionId = 1;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.RightHandWeapon,8);
				template.AddNPCEquipment(eInventorySlot.FeetArmor, 133);
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 31);
				template.AddNPCEquipment(eInventorySlot.Cloak, 96);
				template.AddNPCEquipment(eInventorySlot.LegsArmor, 32);
				template.AddNPCEquipment(eInventorySlot.ArmsArmor, 33);
				madissair.Inventory = template.CloseTemplate();
				madissair.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				madissair.Size = 50;
				madissair.Level = 30;
				madissair.Position = new Point(561064, 512291, 2407);
				madissair.Heading = 721;

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					madissair.SaveIntoDatabase();

				madissair.AddToWorld();
			}
			else
				madissair = npcs[0];


			npcs = WorldMgr.GetNPCsByName("Eileen Morton", eRealm.Albion);
			if (npcs.Length == 0)
			{
				eileenMorton = new GameMob();
				eileenMorton.Model = 5;
				eileenMorton.Name = "Eileen Morton";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + eileenMorton.Name + ", creating him ...");
				eileenMorton.GuildName = "Part of " + questTitle + " Quest";
				eileenMorton.Realm = (byte) eRealm.Albion;
				eileenMorton.RegionId = 1;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 227);
				template.AddNPCEquipment(eInventorySlot.HandsArmor, 137);
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 51);
				template.AddNPCEquipment(eInventorySlot.LegsArmor, 135);
				template.AddNPCEquipment(eInventorySlot.ArmsArmor, 136);
				eileenMorton.Inventory = template.CloseTemplate();
				eileenMorton.SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);

				eileenMorton.Size = 52;
				eileenMorton.Level = 21;
				eileenMorton.Position = new Point(559495, 510743, 2496);
				eileenMorton.Heading = 716;

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					eileenMorton.SaveIntoDatabase();

				eileenMorton.AddToWorld();
			}
			else
				eileenMorton = npcs[0];


			npcs = WorldMgr.GetNPCsByName("Scribe Veral", eRealm.Albion);
			if (npcs.Length == 0)
			{
				scribeVeral = new GameMob();
				scribeVeral.Model = 9;
				scribeVeral.Name = "Scribe Veral";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + scribeVeral.Name + ", creating him ...");
				scribeVeral.GuildName = "Part of " + questTitle + " Quest";
				scribeVeral.Realm = (byte) eRealm.Albion;
				scribeVeral.RegionId = 10;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.HandsArmor, 142, 43, 0);
				template.AddNPCEquipment(eInventorySlot.FeetArmor, 143, 43, 0);
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 2230);
				scribeVeral.Inventory = template.CloseTemplate();
				scribeVeral.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				scribeVeral.Size = 50;
				scribeVeral.Level = 8;
				scribeVeral.Position = new Point(35308, 21876, 8447);
				scribeVeral.Heading = 284;

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					scribeVeral.SaveIntoDatabase();

				scribeVeral.AddToWorld();
			}
			else
				scribeVeral = npcs[0];


			npcs = WorldMgr.GetNPCsByName("Serawen", eRealm.Albion);
			if (npcs.Length == 0)
			{
				serawen = new GameMob();
				serawen.Model = 45;
				serawen.Name = "Serawen";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + serawen.Name + ", creating him ...");
				serawen.GuildName = "Part of " + questTitle + " Quest";
				serawen.Realm = (byte) eRealm.Albion;
				serawen.RegionId = 10;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 2160);
				serawen.Inventory = template.CloseTemplate();
				serawen.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				serawen.Size = 51;
				serawen.Level = 30;
				serawen.Position = new Point(35863, 27167, 8449);
				serawen.Heading = 3298;

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					serawen.SaveIntoDatabase();

				serawen.AddToWorld();
			}
			else
				serawen = npcs[0];

			#endregion
			
			#region defineItems

			// item db check
			sealedLovePoem = (GenericItemTemplate) GameServer.Database.FindObjectByKey(typeof (GenericItemTemplate), "sealed_love_poem");
			if (sealedLovePoem == null)
			{
				sealedLovePoem = new GenericItemTemplate();
				sealedLovePoem.Name = "Sealed Love Poem";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + sealedLovePoem.Name + ", creating it ...");
				sealedLovePoem.Level = 0;
				sealedLovePoem.Weight = 1;
				sealedLovePoem.Model = 498;

				sealedLovePoem.ItemTemplateID = "sealed_love_poem";

				sealedLovePoem.IsDropable = false;
				sealedLovePoem.IsSaleable = false;
				sealedLovePoem.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(sealedLovePoem);
			}

			// item db check
			beautifulRedRose = (GenericItemTemplate) GameServer.Database.FindObjectByKey(typeof (GenericItemTemplate), "beautiful_red_rose");
			if (beautifulRedRose == null)
			{
				beautifulRedRose = new GenericItemTemplate();
				beautifulRedRose.Name = "Beautiful Red Rose";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + beautifulRedRose.Name + ", creating it ...");
				beautifulRedRose.Level = 0;
				beautifulRedRose.Weight = 1;
				beautifulRedRose.Model = 618;

				beautifulRedRose.ItemTemplateID = "beautiful_red_rose";

				beautifulRedRose.IsDropable = false;
				beautifulRedRose.IsSaleable = false;
				beautifulRedRose.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(beautifulRedRose);


				// item db check
				adnilsMagicalOrb = (GenericItemTemplate) GameServer.Database.FindObjectByKey(typeof (GenericItemTemplate), "adnils_magical_orb");
				if (adnilsMagicalOrb == null)
				{
					adnilsMagicalOrb = new GenericItemTemplate();
					adnilsMagicalOrb.Name = "Adnil's Magical Orb";
					if (log.IsWarnEnabled)
						log.Warn("Could not find "+adnilsMagicalOrb.Name+", creating it ...");
					adnilsMagicalOrb.Level = 0;
					adnilsMagicalOrb.Weight = 1;
					adnilsMagicalOrb.Model = 525;

					adnilsMagicalOrb.ItemTemplateID = "adnils_magical_orb";

					adnilsMagicalOrb.IsDropable = false;
					adnilsMagicalOrb.IsSaleable = false;
					adnilsMagicalOrb.IsTradable = false;

					//You don't have to store the created item in the db if you don't want,
					//it will be recreated each time it is not found, just comment the following
					//line if you rather not modify your database
					if (SAVE_INTO_DATABASE)
						GameServer.Database.AddNewObject(adnilsMagicalOrb);
				}

				#endregion

				/* Now we add some hooks to the npc we found.
				* Actually, we want to know when a player interacts with him.
				* So, we hook the right-click (interact) and the whisper method
				* of npc and set the callback method to the "TalkToXXX"
				* method. This means, the "TalkToXXX" method is called whenever
				* a player right clicks on him or when he whispers to him.
				*/

				GameEventMgr.AddHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));
			
				GameEventMgr.AddHandler(madissair, GameObjectEvent.Interact, new DOLEventHandler(TalkToMadissair));
				GameEventMgr.AddHandler(madissair, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMadissair));

				GameEventMgr.AddHandler(eileenMorton, GameObjectEvent.Interact, new DOLEventHandler(TalkToEileenMorton));
				GameEventMgr.AddHandler(eileenMorton, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToEileenMorton));

				GameEventMgr.AddHandler(scribeVeral, GameObjectEvent.Interact, new DOLEventHandler(TalkToScribeVeral));
				GameEventMgr.AddHandler(scribeVeral, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToScribeVeral));

				GameEventMgr.AddHandler(serawen, GameObjectEvent.Interact, new DOLEventHandler(TalkToSerawen));
				GameEventMgr.AddHandler(serawen, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToSerawen));

				/* Now we bring to Ydenia the possibility to give this quest to players */
				QuestMgr.AddQuestDescriptor(madissair, typeof(HalfOgreAllManDescriptor));

				if (log.IsInfoEnabled)
					log.Info("Quest \"" + questTitle + "\" initialized");
			}
		}

		/* The following method is called automatically when this quest class
		 * is unloaded. 
		 * 
		 * Since we set hooks in the load method, it is good practice to remove
		 * those hooks again!
		 */

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded (DOLEvent e, object sender, EventArgs args)
		{
			/* If Madissair has not been initialized, then we don't have to remove any
			 * hooks from him ;-)
			 */
			if (madissair == null)
				return;

			/* Removing hooks works just as adding them but instead of 
			 * AddHandler, we call RemoveHandler, the parameters stay the same
			 */
			
			GameEventMgr.RemoveHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.RemoveHandler(madissair, GameObjectEvent.Interact, new DOLEventHandler(TalkToMadissair));
			GameEventMgr.RemoveHandler(madissair, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMadissair));
			
			GameEventMgr.RemoveHandler(eileenMorton, GameObjectEvent.Interact, new DOLEventHandler(TalkToEileenMorton));
			GameEventMgr.RemoveHandler(eileenMorton, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToEileenMorton));

			GameEventMgr.RemoveHandler(scribeVeral, GameObjectEvent.Interact, new DOLEventHandler(TalkToScribeVeral));
			GameEventMgr.RemoveHandler(scribeVeral, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToScribeVeral));

			GameEventMgr.RemoveHandler(serawen, GameObjectEvent.Interact, new DOLEventHandler(TalkToSerawen));
			GameEventMgr.RemoveHandler(serawen, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToSerawen));

			/* Now we remove to Madissair the possibility to give this quest to players */
			QuestMgr.RemoveQuestDescriptor(madissair, typeof(HalfOgreAllManDescriptor));
		}

		protected static void PlayerEnterWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			HalfOgreAllMan quest = player.IsDoingQuest(typeof (HalfOgreAllMan)) as HalfOgreAllMan;
			if (quest != null)
			{
				GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
				GameEventMgr.AddHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			}
		}

		protected static void PlayerLeftWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			HalfOgreAllMan quest = player.IsDoingQuest(typeof (HalfOgreAllMan)) as HalfOgreAllMan;
			if (quest != null)
			{
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
			}
		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToMadissair(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if (QuestMgr.CanGiveQuest(typeof(HalfOgreAllMan), player, madissair) <= 0)
				return;

			//We also check if the player is already doing the quest
			HalfOgreAllMan quest = player.IsDoingQuest(typeof (HalfOgreAllMan)) as HalfOgreAllMan;

			madissair.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					SendMessage(player, "You approach Madissair and inquire about his apparent interest in Serawen. Madissair appears to have trouble maintaining eye contact after your question and his cheeks slowly turn red.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					madissair.SayTo(player, "H-how did you know? Someone told you, didn't they? Of course they did, or else you wouldn't be asking me about it. But, how did they know? I hope I'm not that transparent. If Arliss Eadig finds out, it'll be the [end of me]!");
					return;
				}
				else
				{
					if (quest.Step == 13 || quest.Step == 16 || quest.Step == 18 || quest.Step == 19)
					{
						madissair.SayTo(player, "Ah, you're back! Did you deliver my poem to [Serawen]?");
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
						case "end of me":
							madissair.SayTo(player, "You won't tell Arliss, will you? I was lucky enough to be chosen as one of his apprentice tailors and I'd hate to be sent away because I fell in love with his daughter, [Serawen].");
							SendMessage(player, "Madissair sighs as he mentions Serawen's name.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
							break;

						case "Serawen":
							madissair.SayTo(player, "Don't think that he wouldn't! Since his wife passed away, Serawen is everything to him. He makes her stay upstairs  when the apprentices are around, and with good reason. Imagine what it would be like if every apprentice lost his [heart] to her.");
							SendMessage(player, "Madissair flashes an awkward smile.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
							break;

						case "heart":
							madissair.SayTo(player, "I just can't help it.  It's not like I was trying to get her attention. Well, ok, I never had her attention. She doesn't know how I feel. She doesn't even know who I am. I only found out about her by [accident].");
							break;

						case "accident":
							madissair.SayTo(player, "I'd forgotten that I left Arliss' shop one day with a pair of his scissors. As luck would have it, Serawen was helping her father finish up an order when I arrived with the scissors. That's when I first [saw her].");
							break;

						case "saw her":
							madissair.SayTo(player, "You probably don't believe in love at first sight, and I didn't, either, at the time.  Without speaking a word to me, Serawen changed my mind.  I kept finding reasons to stay late at the shop with the hope of seeing her [again].");
							break;

						case "again":
							madissair.SayTo(player, "I want to get to know her better, but I don't think she'd be interested in me. I'm only an apprentice tailor, after all, and a half ogre.  I've come up with an idea, but I don't think I can go [through with it].");
							SendMessage(player, "Madissair grins.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
							break;

						case "through with it":
							madissair.SayTo(player, "I can't approach her directly, but I've written a poem to express my feelings to her. The only problem is that I haven't figured out a way to deliver it anonymously.  I want her to read it and know who I am, but I'm afraid she won't like it, or me.  I'll gladly pay you if you would be willing to deliver it for me.");
							//If the player offered his help, we send the quest dialog now!
							player.Out.SendCustomDialog("Will you help Madissair \nwin Serawen's heart? \n[Level "+player.Level+"]", new CustomDialogResponse(CheckPlayerAcceptQuest));
							break;
					}
				}
				else
				{	
					switch(quest.Step)
					{
						case 1:
							switch (wArgs.Text)
							{
								case "eyes only":
									madissair.SayTo(player, "Serawen lives with her father, Arliss Eadig, in Camelot. I've heard that she likes to visit the Garden of Artorus Rex, so you may be able to find her there.  Please do not mention the errand to anyone else. Also, be on the lookout for [Geroth].");
									break;

								case "Geroth":
									madissair.SayTo(player, "I've seen her walking with him in the evening. He's one of the most promising apprentice weaponsmiths and the son of one of the Guildmasters. I don't know if he's courting her, but I wouldn't be surprised. If you see Geroth, please don't mention my name. It's bad enough that he and the other apprentice weapon- and armorsmiths tease me for my choice of profession.  'What's the matter, are you too weak to lift a hammer?' they say. Sometimes they ask if my embroidery is the secret weapon that will give Albion the edge in this miserable war. Anyway, thank you again for agreeing to help me. Please return to me after you have delivered the poem.");
									break;
							}
							break;

						case 13:
							switch (wArgs.Text)
							{
								case "Serawen":
									SendMessage(player, "You tell Madissair that Serawen loved his gift, and that she confessed she secretly had a crush on the half ogre. You deliver Serawen's invitation to help him sneak into her house, emphasizing the need for stealth.  Upon hearing the news, Madissair's eyes widen and he begins to smile, quickly agreeing to the meeting.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
									madissair.SayTo(player, "I can't believe she feels the same way! She must've thought I'm quite the poet. Perhaps I'll take up writing poetry on a more permanent basis when I'm not sewing.  Thank you for your help, Reawin. Without you, I'd be nothing more than a lovesick [half ogre].");
									break;

								case "half ogre":
									madissair.SayTo(player, "I have a friend in Camelot who is training to be a spellcrafter. Occasionally, he brings me a prototype of something that he's working on.  When I told him about my dilemma, he gave me a [magical orb] to help me decide what to do.");
									
									player.GainExperience((long)((player.ExperienceForNextLevel / (player.Level * 9)) * 1.7), 0, 0, true); //202 xp

									quest.Step = 20;
									break;
							}
							break;

						case 19:
							switch (wArgs.Text)
							{
								case "Serawen":
									SendMessage(player, "Madissair appears as though he might have had more to say, though he stops speaking abruptly. He takes a moment to compose himself and then continues.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
									madissair.SayTo(player, "So, she rejected me for Geroth? Can't she see that I'm not the sniveling coward Geroth calls me? I would treat her as a lady deserves to be treated. I'd make the finest dresses for her. She shouldn't be trapped in a [marriage] with that lying bully!");
									player.Out.SendEmoteAnimation(madissair, eEmote.Cry);
									break;

								case "marriage":
									madissair.SayTo(player, "I-I'm sorry, "+player.Name+".  I let my feelings get the better of me.  I let myself believe for too long that it was going to be just like the story of Arthur and Guinevere. I know you did your best, and without your help, I'd still love a woman who didn't [love] me.");
									break;

								case "love":
									madissair.SayTo(player, "I have a friend in Camelot who is training to be a spellcrafter. Occasionally, he brings me a prototype of something that he's working on.  When I told him about my dilemma, he gave me a [magical orb] to help me decide what to do.");
									
									player.GainExperience((long)((player.ExperienceForNextLevel / (player.Level * 9)) * 1.35), 0, 0, true); //162 xp

									quest.Step = 20;
									break;
							}
							break;

						case 16:
							switch (wArgs.Text)
							{
								case "Serawen":
									SendMessage(player, "You tell Madissair that Serawen appreciated receiving his poem, and is interested in getting to know him better. You tell Madissair that she would like to invite him to meet in the Garden of Artorus Rex, which will  provide an opportunity for a private conversation.  Upon hearing the news, Madissair's face brightens and he begins to smile.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
									madissair.SayTo(player, "I can't believe it! She must've loved my poetry, after all. Perhaps I'll write another for her in time for the meeting. I bet she'll really enjoy that.  Thank you for your help, Gwonn. Without you, I'd still be nothing more than a lovesick [half ogre].");
									break;

								case "half ogre":
									madissair.SayTo(player, "I have a friend in Camelot who is training to be a spellcrafter. Occasionally, he brings me a prototype of something that he's working on.  When I told him about my dilemma, he gave me a [magical orb] to help me decide what to do.");
									
									player.GainExperience((long)((player.ExperienceForNextLevel / (player.Level * 9)) * 1.7), 0, 0, true); //202 xp

									quest.Step = 20;
									break;
							}
							break;

						case 18:
							switch (wArgs.Text)
							{
								case "Serawen":
									SendMessage(player, "Doing your best to spare Madissair's feelings, you tell him that Serawen appreciated his thoughtfulness, but that her family is negotiating a marriage contract with Geroth, and that she seems to look forward to marrying him.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
									madissair.SayTo(player, "So, she rejected me for Geroth? Can't she see that I'm not the sniveling coward Geroth calls me? I would treat her as a lady deserves to be treated. I'd make the finest dresses for her. She shouldn't be trapped in a [marriage] with that lying bully!");
									player.Out.SendEmoteAnimation(madissair, eEmote.Cry);
									break;

								case "marriage":
									SendMessage(player, "Madissair appears as though he might have had more to say, though he stops speaking abruptly. He takes a moment to compose himself and then continues.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
									madissair.SayTo(player, "I-I'm sorry, "+player.Name+".  I let my feelings get the better of me.  I let myself believe for too long that it was going to be just like the story of Arthur and Guinevere. I know you did your best, and without your help, I'd still love a woman who didn't [love] me.");
									break;

								case "love":
									madissair.SayTo(player, "I have a friend in Camelot who is training to be a spellcrafter. Occasionally, he brings me a prototype of something that he's working on.  When I told him about my dilemma, he gave me a [magical orb] to help me decide what to do.");
									
									player.GainExperience((long)(player.ExperienceForNextLevel / (player.Level * 9)), 0, 0, true); //121 xp

									quest.Step = 20;
									break;
							}
							break;

						case 20:
							if(wArgs.Text == "magical orb")
							{
								madissair.SayTo(player, "Here it is. I'd like you to have it as a reward for all your hard work. I'm sure Adnil will craft me another one, and perhaps, in time, I won't need it to make my decisions for me.");
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

		protected static void TalkToEileenMorton(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			//We also check if the player is already doing the quest
			HalfOgreAllMan quest = player.IsDoingQuest(typeof (HalfOgreAllMan)) as HalfOgreAllMan;

			eileenMorton.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					if(quest.Step == 2 || quest.Step == 4)
					{
						eileenMorton.SayTo(player, "Hello, "+player.Name+". What can I do for you today?");
						SendMessage(player, "You tell Eileen Morton about Madissair's predicament and ask her for a rose from her flower garden.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
						eileenMorton.SayTo(player, "Madissair, eh? He seems like a good man, if a bit on the shy side. I can't say I blame him. I've seen the way the other apprentices make fun of him. No matter, I'll be happy to help him catch Serawen's eye.  He'll have his work cut out for him trying to convince Arliss Eadig to allow their courtship.  I think that Arliss has other hopes for Serawen and wants her to marry into one of the families of the other crafting Guildmasters in the [capital].");	
					}
				}
			}
				// The player whispered to NPC (clicked on the text inside the [])
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				if (quest != null)
				{
					if(quest.Step == 2 || quest.Step == 4)
					{
						switch (wArgs.Text)
						{
							case "capital":
								eileenMorton.SayTo(player, "Arliss has worked hard to make a name for his family in Camelot, and he would like to see his daughter benefit from it. Sure, I would like to see her marry for love instead of money, but that supposes that she does love [Madissair]...");
								break;

							case "Madissair":
								eileenMorton.SayTo(player, "And right now, that's a big assumption to make.  I think they would be nice together, but my opinion doesn't count.  Anyway, here's your rose. I wish you and Madissair good luck.");
								
								// give the rose    
								player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, beautifulRedRose.CreateInstance());

								if(quest.Step == 2 ) quest.Step = 3;
								else if(quest.Step == 4) quest.Step = 5;
								break;
						}
					}
				}
			}
		}

		/* This is the method we declared as callback for the hooks we set to
		 * Sir Quait. It will be called whenever a player right clicks on Sir Quait
		 * or when he whispers something to him.
		 */

		protected static void TalkToScribeVeral(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			//We also check if the player is already doing the quest
			HalfOgreAllMan quest = player.IsDoingQuest(typeof (HalfOgreAllMan)) as HalfOgreAllMan;

			scribeVeral.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					if(quest.Step == 2 || quest.Step == 3)
					{
						scribeVeral.SayTo(player, "Good day, "+player.Name+". Have you come here to commission some work, or perhaps to learn about the life of valiant King [Arthur]?");
					}
				}
			}
				// The player whispered to NPC (clicked on the text inside the [])
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				if (quest != null)
				{
					if(quest.Step == 2 || quest.Step == 3)
					{
						switch (wArgs.Text)
						{
							case "Arthur":
								SendMessage(player, "You tell Scribe Veral about Madissair and the love poem, confessing that you opened and read it, and that you don't think it will capture Serawen's heart. Out of respect for Madissair, you omit the names.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
								scribeVeral.SayTo(player, "I can't believe you opened the poem. How intrusive! You've piqued my curiousity now, "+player.Name+". Not just anyone writes poetry these days. That's normally reserved for learned scholars. Your friend must be deeply in love. Since you have already broken the seal, I don't suppose you would hand it to me and allow me to read it, would you?");
								break;

							case "paper":
								SendMessage(player, "Scribe Veral sits down at the table, taking out a piece of paper, an inkwell, and a gaudy quill.  With sweeping gestures, he begins composing the perfect love poem.  He seems oblivious to your presence as he pens each line, finishing it with a theatrical flourish. The Scribe continues writing until he nears the bottom of the sheet, then sets aside the quill, deftly folds the paper, and hands it to you.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
								scribeVeral.SayTo(player, "There we are! This is sure to make her fall madly in love with your gentleman friend.  No payment is necessary. Just knowing that I've been able to help a young man is reward enough!");

								GenericItem item = player.Inventory.GetFirstItemByName(sealedLovePoem.Name, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
								if (item != null)
								{
									item.Name = "Rewritten Love Poem";
									player.Out.SendInventorySlotsUpdate(new int[] {item.SlotPosition});
						
									if(quest.Step == 2) quest.Step = 4;
									else if(quest.Step == 3) quest.Step = 5;
								}
								break;
						}
					}
				}
			}
		}

		/* This is the method we declared as callback for the hooks we set to
		 * Sir Quait. It will be called whenever a player right clicks on Sir Quait
		 * or when he whispers something to him.
		 */

		protected static void TalkToSerawen(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			//We also check if the player is already doing the quest
			HalfOgreAllMan quest = player.IsDoingQuest(typeof (HalfOgreAllMan)) as HalfOgreAllMan;

			serawen.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					if(quest.Step == 1  || quest.Step == 3 || quest.Step == 4 || quest.Step == 5)
					{
						serawen.SayTo(player, "Hello.  I don't recognize you, "+player.CharacterClass.Name+". Are you looking for my father, [Arliss]?");
					}
				}
			}
				// The player whispered to NPC (clicked on the text inside the [])
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				if (quest != null)
				{
					if(quest.Step == 1  || quest.Step == 3 || quest.Step == 4 || quest.Step == 5)
					{
						switch (wArgs.Text)
						{
							case "Arliss":
								serawen.SayTo(player, "You're not? Then what brings [you here]?");
								SendMessage(player, "You tell Serawen that you have been sent to give her a gift from an anonymous admirer.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
								break;

							case "you here":
								serawen.SayTo(player, "A gift for me? How thoughtful! Do you have it with you?");
								if(quest.Step == 3) quest.Step = 10;
								else if(quest.Step == 5) quest.Step = 6;
								else if(quest.Step == 4) quest.Step = 7;
								else if(quest.Step == 1) quest.Step = 9;
								break;
						}
					}
					else if(quest.Step == 17) // original letter
					{
						switch (wArgs.Text)
						{
							case "man":
								serawen.SayTo(player, "I can't think of who might have sent such a thing. Most would-be suitors have the sense to identify themselves, but I can understand why the author of this, er, work might want to remain anonymous.  Is it someone that I [know]?");
								break;

							case "Geroth":
								serawen.SayTo(player, "Oh, thank goodness! For a moment, I didn't know what to think. Geroth always warned me that there are a lot of lonely men who might try this sort of thing. I'm so glad to know someone like Geroth, with both a sense of judgment and a sense of  [humor].");
								quest.Step = 18;
								break;

							case "humor":
								serawen.SayTo(player, "I think I'm going to go take a walk and speak with Geroth. He told me that he was going to ask Father for permission to marry me.  Perhaps this little joke is a sign that he's received a response. I hope it's good news. Be well, Reawin.");
								break;

							case "know":
								serawen.SayTo(player, "What is his [name]?");
								break;

							case "name":
								SendMessage(player, "You tell Serawen that her father's apprentice, Madissair, wrote her the poem and asked you to deliver it to her.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
								serawen.SayTo(player, "Madissair? I've no interest in that oafish Half Ogre. Who's ever heard of a Half Ogre tailor, anyway? Tell him that if he behaves like this again, I'll ask Father to send him away. Father has no need of apprentices with designs on his daughter.");
								SendMessage(player, "Serawen shudders with disgust as she imagines life with Madissair and then snorts in disdain at the idea of a Half Ogre tailor.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
								
								quest.Step = 19;
								break;
						}
					}
					else if(quest.Step == 15) // rose + rewrited letter
					{
						switch (wArgs.Text)
						{
							case "tale":
								serawen.SayTo(player, "Of course, Father would never approve.  It can't be cheap to commission such a poem, and if there's any hope of a future with my mystery man, I must know his identity. Can you tell me who sent you? Was it a [secret admirer] or perhaps my friend [Geroth]?");
								break;

							case "secret admirer":
								serawen.SayTo(player, "What is his [name]?");
								break;

							case "Geroth":
								serawen.SayTo(player, "Oh, thank goodness! For a moment, I didn't know what to think. Geroth always warned me that there are a lot of lonely men who might try this sort of thing. I'm so glad to know someone like Geroth, with both a sense of judgment and a sense of  [humor].");
								quest.Step = 18;
								break;

							case "humor":
								serawen.SayTo(player, "I think I'm going to go take a walk and speak with Geroth. He told me that he was going to ask Father for permission to marry me.  Perhaps this little joke is a sign that he's received a response. I hope it's good news. Be well, Reawin.");
								break;

							case "name":
								SendMessage(player, "You tell Serawen that her father's apprentice, Madissair, is the one who sent the poem. After finding out the identity of her admirer, Serawen smiles warmly.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
								serawen.SayTo(player, "Madissair? I never knew he had his eye on me, or that he was such a romantic.  Well, perhaps I underestimated him. If I could do so without Father finding out, I'd like to meet with him in the Garden of Artorus Rex. Would you [ask] him for me?");
								break;

							case "ask":
								SendMessage(player, "You agree to tell Madissair about Serawen's invitation.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
								serawen.SayTo(player, "Thank you, "+player.Name+". Please give Madissair my thanks, as well.");
								
								quest.Step = 16; // rose + rewrited
								break;
						}
					}
					else if(quest.Step == 14) // only rewrited letter
					{
						switch (wArgs.Text)
						{
							case "beautiful":
								serawen.SayTo(player, "Father will be curious about who sent it. Do you know who sent it? If it was not Geroth, I can see myself having to make a difficult choice. Was this poem sent by some mysterious, romantic [stranger] or my suitor, [Geroth]?");
								break;

							case "stranger":
								serawen.SayTo(player, "Do you [know] him?");
								break;

							case "Geroth":
								serawen.SayTo(player, "Oh, thank goodness! For a moment, I didn't know what to think. Geroth always warned me that there are a lot of lonely men who might try this sort of thing. I'm so glad to know someone like Geroth, with both a sense of judgment and a sense of  [humor].");
								quest.Step = 18;
								break;

							case "humor":
								serawen.SayTo(player, "I think I'm going to go take a walk and speak with Geroth. He told me that he was going to ask Father for permission to marry me.  Perhaps this little joke is a sign that he's received a response. I hope it's good news. Be well, Reawin.");
								break;

							case "know":
								serawen.SayTo(player, "Madissair? I've no interest in that oafish Half Ogre. Who's ever heard of a Half Ogre tailor, anyway? Tell him that if he behaves like this again, I'll ask Father to send him away. Father has no need of apprentices with designs on his daughter.");
								SendMessage(player, "Serawen shudders with disgust as she imagines life with Madissair and then snorts in disdain at the idea of a Half Ogre tailor.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
								
								quest.Step = 19;
								break;
						}
					}
					else if(quest.Step == 12) // only rose
					{
						switch (wArgs.Text)
						{
							case "voice":
								serawen.SayTo(player, "Yes, I must find out who sent this charming little poem. He's not afraid to say what he means without needlessly couching it in flowery language. Can you tell me who sent you? Was it a [secret admirer] or perhaps my friend [Geroth]?");
								break;

							case "secret admirer":
								serawen.SayTo(player, "What is his [name]?");
								break;

							case "Geroth":
								serawen.SayTo(player, "Oh, thank goodness! For a moment, I didn't know what to think. Geroth always warned me that there are a lot of lonely men who might try this sort of thing. I'm so glad to know someone like Geroth, with both a sense of judgment and a sense of  [humor].");
								quest.Step = 18;
								break;

							case "humor":
								serawen.SayTo(player, "I think I'm going to go take a walk and speak with Geroth. He told me that he was going to ask Father for permission to marry me.  Perhaps this little joke is a sign that he's received a response. I hope it's good news. Be well, Reawin.");
								break;

							case "name":
								SendMessage(player, "You tell Serawen that her father's apprentice, Madissair, is the one who sent the poem. After finding out the identity of her admirer, Serawen smiles warmly.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
								serawen.SayTo(player, "Ah, Madissair! You know, I bet he doesn't know that I've seen him watching me.  I've been watching him, too.  I've never really had the chance to get to know him very well. Father doesn't want me associating with the [apprentices].");
								break;

							case "apprentices":
								serawen.SayTo(player, "Father will never appreciate that I'm old enough to make some decisions for myself.  I admire Madissair even more for taking a risk like this, especially when he knows the consequences if he is [discovered].");
								break;

							case "discovered":
								serawen.SayTo(player, "If we're careful, it won't happen.  When I was younger, I built a rope ladder so that I could sneak out of the house at night. I haven't had to use it in awhile, but it will serve just as well to help someone get in. I'll bring some food upstairs tonight, so we can have a private meeting. Father will never know! Will you deliver my [invitation] to Madissair?");
								SendMessage(player, "You agree to deliver Serawen's invitation to Madissair.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
								
								quest.Step = 13; // only rose
								break;
						}
					}
				}
			}
		}

		protected static void PlayerUseSlot(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = (GamePlayer) sender;

			HalfOgreAllMan quest = (HalfOgreAllMan) player.IsDoingQuest(typeof (HalfOgreAllMan));
			if (quest == null)
				return;
		
			UseSlotEventArgs uArgs = (UseSlotEventArgs) args;

			GenericItem item = player.Inventory.GetItem((eInventorySlot)uArgs.Slot);
			if (item != null && item.Name == sealedLovePoem.Name)
			{
				if (quest.Step == 1)
				{
					SendMessage(player, "Using a small knife, you carefully separate the seal from one flap of the paper, allowing it to be resealed later with little evidence of tampering.  You unfold the paper, revealing a short poem in the apprentice's handwriting.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					SendMessage(player, "Our homeland's banner is red,", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					SendMessage(player, "Those little inconnu are blue,", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					SendMessage(player, "I've never been quite so happy", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					SendMessage(player, "as I feel when thinking of you.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					SendMessage(player, "Chuckling as you read the poem, you realize that Madissair's words, while sincere, may not win Serawen's heart on their own.  You quickly think up a couple of ideas to help Madissair get Serawen's attention.  In Camelot City, Scribe Veral is renowned for his poems in praise of Arthur's deeds. He might be able to help you craft a more suitable poem.  You can also seek out Eileen Morton in the tavern in Cotswold, and ask her for a red rose from her garden, to accompany whichever poem you choose to deliver.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				
					item.Name = "Unsealed Love Poem";
					player.Out.SendInventorySlotsUpdate(new int[] {item.SlotPosition});
						
					quest.Step = 2;
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
			if (QuestMgr.CanGiveQuest(typeof(HalfOgreAllMan), player, madissair) <= 0)
				return;

			if (player.IsDoingQuest(typeof (HalfOgreAllMan)) != null)
				return;

			if (response == 0x00)
			{
				SendReply(player, "Oh well, if you change your mind, please come back!");
			}
			else
			{
				//Check if we can add the quest!
				if (!QuestMgr.GiveQuestToPlayer(typeof(HalfOgreAllMan), player, madissair))
					return;

				// give the peom                
				player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, sealedLovePoem.CreateInstance());

				SendReply(player, "That's great! Let me find it for you.  Please promise that you won't break the seal on it. The contents are private, and for Serawen's [eyes only].");

				GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
				GameEventMgr.AddHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
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
						return "[Step #1] Madissair has given you the Sealed Love Poem and asked you to deliver it to Serawen in Camelot without reading it.  If you do wish to read Madissair's poem, delve the Sealed Love Poem for details on how to open it.";
					case 2:
						return "[Step #2] Madissair's poetry isn't likely to win Serawen on its own. Seek out Eileen Morton in Cotswold and ask her for one of her roses to deliver with the poem, or find Scribe Veral in Camelot and commission a better poem.";
					case 3:
						return "[Step #2] You have received a rose from Eileen Morton in Cotswold. You may now deliver it and Madissair's poem to Serawen in Camelot, or you may seek out Scribe Veral in the city and ask him to write a new poem.";
					case 4:
						return "[Step #2] Once you have received the Rewritten Love Poem from Scribe Veral, you may deliver it to Serawen in Camelot, or seek out Eileen Morton in Cotswold and ask her for a red rose from her garden.";
					case 5:
						return "[Step #3] You now have both a Rewritten Love Poem and a Beautiful Red Rose.  Find Serawen in Camelot, and speak with her before you give her the gifts. If she stops speaking with you, tell her you're not there to see her father, [Arliss].";
					case 6:
						return "[Step #4] Give Serawen the Beautiful Red Rose first.";
					case 7:
						return "[Step #4] You've told Serawen that you have a gift for her. Hand her the Rewritten Love Poem when she asks for it. If she stops speaking to you ask her about the [beautiful] poem from her admirer.";
					case 8:
						return "[Step #4] Now, give Serawen the poem. Once you have given her the poem, you must tell her whether it is from a [secret admirer] or from [Geroth].";
					case 9:
						return "[Step #2] Deliver the poem to Serawen.  She will want to know who it is from once you have given it to her.";
					case 10:
						return "[Step #4] Give Serawen the Beautiful Red Rose first.";
					case 11:
						return "[Step #4] Now, give Serawen the poem. Once you have given her the poem, you must tell her whether it is from a [secret admirer] or from [Geroth].";	
					case 12:
						return "[Step #4] You have delivered Madissair's poem and the rose.  Continue speaking with Serawen, and if she stops talking to you, tell her you've heard the [voice] of the man who sent the gift.";
					case 13: // moyen
						return "[Step #5] Return to Madissair with Serawen's thanks, and tell him that she wants to sneak him into her house for a private meeting.";
					case 14:
						return "[Step #4] You have delivered Madissair's poem. If she stops speaking to you, tell her you [know] the name of the person who sent it. After Serawen finishes speaking to you, return to Madissair in Cotswold.";
					case 15:
						return "[Step #4] You have delivered the poem and the rose to Serawen. Continue speaking with her and tell her you know the [name] of the person who sent the gift.";
					case 16: // tres bien
						return "[Step #5] Now that you have delivered the rewrited poem and the rose to Serawen, return to Madissair and let him know that [Serawen] would like to meet him in the Garden of Artorus Rex.";
					case 17:
						return "[Step #4] You have delivered Madissair's poem. If she stops speaking to you, tell her you know the [name] of the person who sent it. After Serawen finishes speaking to you, return to Madissair in Cotswold.";
					case 18: // null
						return "[Step #5] You've chosen to tell Serawen that Geroth sent the poem to her. Return to Madissair and let him know that Serawen's family is negotiating a marriage contract with Geroth's.";
					case 19: // null
						return "[Step #5] Return to Madissair and let him know that Serawen have no interest in him.";
					case 20:
						return "[Step #6] If Madissair stops talking to you, ask him about the [magical orb] he offered you.";
					default:
						return "[Step #" + Step + "] No Description entered for this step!";
				}
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof (HalfOgreAllMan)) == null)
				return;


			if (e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
				
				switch(Step)
				{
					case 2:
					case 3:
						if (gArgs.Target.Name == scribeVeral.Name && gArgs.Item.Name == sealedLovePoem.Name)
						{
							scribeVeral.Emote(eEmote.Ponder);
						
							scribeVeral.TurnTo(m_questPlayer);
							scribeVeral.SayTo(m_questPlayer, "Oh, my! This is...unusually bad. I've no doubt this young man is sincere, but we shall have to do something about this. You've come to the right place, my friend. Please excuse me while I fetch a pen and a fresh sheet of [paper].");
						}
						break;

					case 6: // rose + rewrite poem
					case 10: // only rose
						if (gArgs.Target.Name == serawen.Name && gArgs.Item.Name == beautifulRedRose.Name)
						{
							RemoveItemFromPlayer(serawen, gArgs.Item);

							serawen.TurnTo(m_questPlayer);
							serawen.SayTo(m_questPlayer, "Oh, my! That's a gorgeous rose. Did the sender give you a note, or anything identifying himself?");
							SendMessage(m_questPlayer, "Serawen accepts the flower from you, blushing.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);	
							
							if(Step == 6) Step = 8; // rose + rewrite poem
							else if(Step == 10) Step = 11; // only rose
						}
						break;

					case 7 : // only rewrited letter
						if (gArgs.Target.Name == serawen.Name && gArgs.Item.Name == sealedLovePoem.Name)
						{
							RemoveItemFromPlayer(serawen, gArgs.Item);

							serawen.TurnTo(m_questPlayer);
							SendMessage(m_questPlayer, "You hand Serawen the poem and she glances at the seal, not recognizing it. She eagerly unfolds the poem, reading quickly.  As her eyes move down the page, she begins to smile.  When she reaches the end of the page, Serawen looks up at you, still smiling with appreciation.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);	
							serawen.SayTo(m_questPlayer, "What a kind gesture! I've always hoped that I would be courted by a gallant man. Whoever sent this must have put a lot of effort into finding a talented scribe.  It really is a [beautiful] poem.");
						
							Step = 14;
						}
						break;

					case 8 : // rose + rewrited letter
						if (gArgs.Target.Name == serawen.Name && gArgs.Item.Name == sealedLovePoem.Name)
						{
							RemoveItemFromPlayer(serawen, gArgs.Item);

							serawen.TurnTo(m_questPlayer);
							SendMessage(m_questPlayer, "Serawen accepts the poem from you, inspecting the seal. She unfolds the paper eagerly, expecting it to be a note from the mysterious man who sent the rose.  When she realizes that the paper contains a poem, Serawen begins to smile.  She closes her eyes briefly and pauses to smell the rose.  After a moment, her eyes return to the paper as she reads the rest of the poem and nods approvingly.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);	
							serawen.SayTo(m_questPlayer, "This is a very beautiful poem, "+m_questPlayer.Name+".  Whoever sent it is a true gentleman. I must admit, the idea of being courted by a mystery man is exciting and sounds like something straight out of a minstrel's [tale].");
						
							Step = 15;
						}
						break;

					case 9: // letter non open
						if (gArgs.Target.Name == serawen.Name && gArgs.Item.Name == sealedLovePoem.Name)
						{
							RemoveItemFromPlayer(serawen, gArgs.Item);

							serawen.TurnTo(m_questPlayer);
							SendMessage(m_questPlayer, "You hand Serawen the poem and she glances at the seal, not recognizing it. She eagerly unfolds the poem, reading quickly.  As her eyes move down the page, the corners of her mouth begin to fall. She does not quite frown, but it is clear that she had different expectations. Serawen chuckles nervously when she reads the last of the text.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
							serawen.SayTo(m_questPlayer, "Thank you...I think. I appreciate the sentiment, but the poem is simply horrible. I don't mean to offend your employer, but I hope he didn't think he'd win my affection with this. He seems to be quite a desperate [man], or did [Geroth] put you up to this?");
							serawen.Emote(eEmote.Laugh);

							Step = 17;
						}
						break;

					case 11: // only rose
						if (gArgs.Target.Name == serawen.Name && gArgs.Item.Name == sealedLovePoem.Name)
						{
							RemoveItemFromPlayer(serawen, gArgs.Item);

							serawen.TurnTo(m_questPlayer);
							SendMessage(m_questPlayer, "Serawen accepts the poem from you, inspecting the seal. She unfolds the paper eagerly, expecting it to be a note from the mysterious man who sent the rose.  When she realizes that the paper contains a poem, Serawen begins to smile.  She closes her eyes briefly and pauses to smell the rose.  After a moment, her eyes return to the paper as she reads the rest of the poem and nods approvingly.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);	
							serawen.SayTo(m_questPlayer, "This is a very sweet gesture.  Whoever sent it might not be the best of poets, but his sincerity shines through. There aren't many who would take this risk. I think most would hire someone to write a poem instead, but then it would lack his [voice].");

							Step = 12;
						}
						break;
				}
			}
		}

		public override void FinishQuest()
		{
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...

			//Give reward to player here ...

			GiveItemToPlayer(madissair, adnilsMagicalOrb.CreateInstance());
		
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
		}
	}
}
