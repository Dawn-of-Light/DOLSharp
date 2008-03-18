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
 * Author:		k109
 * 
 * Date:		12/14/07	
 * Directory: /scripts/quests/hibernia/
 *
 * Description:
 *  Brief Walkthrough: 
 * 1) Talk with Richael in Mag Mell
 * 2) Find the Entrance to the Demon's Breach and then head back to her.  
 * You will receive some xp, copper and the armor of your choice.
 */
using System;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Quests.Hibernia
{
	public class ToReachTheBreach : RewardQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected const string questTitle = "To Reach the Breach";
		protected const int minimumLevel = 2;
		protected const int maximumLevel = 5;

		private static GameNPC Richael = null;
		private QuestGoal FoundBreach;

		private static GameLocation Demons_Breach = new GameLocation("Demon's Breach", 200, 354760, 486115, 5973);

		private static IArea Demons_Breach_Area = null;

		private static ItemTemplate RecruitsIntelligentBelt;
		private static ItemTemplate RecruitsMightyBelt;
		private static ItemTemplate RecruitsPiousBelt;

		public ToReachTheBreach()
			: base()
		{
			Init();
		}

		public ToReachTheBreach(GamePlayer questingPlayer)
			: this(questingPlayer, 1)
		{
		}

		public ToReachTheBreach(GamePlayer questingPlayer, int step)
			: base(questingPlayer, step)
		{
			Init();
		}

		public ToReachTheBreach(GamePlayer questingPlayer, DBQuest dbQuest)
			: base(questingPlayer, dbQuest)
		{
			Init();
		}

		private void Init()
		{
			#region defineItems
			RecruitsIntelligentBelt = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "RecruitsIntelligentBelt");
			if (RecruitsIntelligentBelt == null)
			{
				RecruitsIntelligentBelt = new ItemTemplate();
				RecruitsIntelligentBelt.Name = "Recruit's Intelligent Belt";
				RecruitsIntelligentBelt.TemplateID = "RecruitsIntelligentBelt";
				RecruitsIntelligentBelt.Level = 4;
				RecruitsIntelligentBelt.Weight = 3;
				RecruitsIntelligentBelt.Model = 597;
				RecruitsIntelligentBelt.Color = 0;
				RecruitsIntelligentBelt.Object_Type = (int)eObjectType.GenericArmor;
				RecruitsIntelligentBelt.Item_Type = (int)eEquipmentItems.WAIST;
				RecruitsIntelligentBelt.Gold = 0;
				RecruitsIntelligentBelt.Silver = 0;
				RecruitsIntelligentBelt.Copper = 10;
				RecruitsIntelligentBelt.Quality = 100;
				RecruitsIntelligentBelt.Condition = 50000;
				RecruitsIntelligentBelt.MaxCondition = 50000;
				RecruitsIntelligentBelt.Durability = 50000;
				RecruitsIntelligentBelt.MaxDurability = 50000;
				RecruitsIntelligentBelt.IsDropable = false;

				RecruitsIntelligentBelt.AddBonus((byte)eProperty.Acuity, 3);
				RecruitsIntelligentBelt.AddBonus((byte)eStat.DEX, 3);
				RecruitsIntelligentBelt.AddBonus((byte)eStat.CON, 1);
				RecruitsIntelligentBelt.AddBonus((byte)eProperty.MaxHealth, 8);

				GameServer.Database.AddNewObject(RecruitsIntelligentBelt);
			}

			RecruitsMightyBelt = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "RecruitsMightyBelt");
			if (RecruitsMightyBelt == null)
			{
				RecruitsMightyBelt = new ItemTemplate();
				RecruitsMightyBelt.Name = "Recruit's Mighty Belt";
				RecruitsMightyBelt.TemplateID = "RecruitsMightyBelt";
				RecruitsMightyBelt.Level = 4;
				RecruitsMightyBelt.Weight = 3;
				RecruitsMightyBelt.Model = 597;
				RecruitsMightyBelt.Color = 0;
				RecruitsMightyBelt.Object_Type = (int)eObjectType.GenericArmor;
				RecruitsMightyBelt.Item_Type = (int)eEquipmentItems.WAIST;
				RecruitsMightyBelt.Gold = 0;
				RecruitsMightyBelt.Silver = 0;
				RecruitsMightyBelt.Copper = 10;
				RecruitsMightyBelt.Quality = 100;
				RecruitsMightyBelt.Condition = 50000;
				RecruitsMightyBelt.MaxCondition = 50000;
				RecruitsMightyBelt.Durability = 50000;
				RecruitsMightyBelt.MaxDurability = 50000;
				RecruitsMightyBelt.IsDropable = false;

				RecruitsMightyBelt.AddBonus((byte)eStat.STR, 3);
				RecruitsMightyBelt.AddBonus((byte)eStat.DEX, 3);
				RecruitsMightyBelt.AddBonus((byte)eStat.QUI, 3);
				RecruitsMightyBelt.AddBonus((byte)eProperty.MaxHealth, 8);

				GameServer.Database.AddNewObject(RecruitsMightyBelt);
			}

			RecruitsPiousBelt = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "RecruitsPiousBelt");
			if (RecruitsPiousBelt == null)
			{
				RecruitsPiousBelt = new ItemTemplate();
				RecruitsPiousBelt.Name = "Recruit's Pious Belt";
				RecruitsPiousBelt.TemplateID = "RecruitsPiousBelt";
				RecruitsPiousBelt.Level = 4;
				RecruitsPiousBelt.Weight = 3;
				RecruitsPiousBelt.Model = 597;
				RecruitsPiousBelt.Color = 0;
				RecruitsPiousBelt.Object_Type = (int)eObjectType.GenericArmor;
				RecruitsPiousBelt.Item_Type = (int)eEquipmentItems.WAIST;
				RecruitsPiousBelt.Gold = 0;
				RecruitsPiousBelt.Silver = 0;
				RecruitsPiousBelt.Copper = 10;
				RecruitsPiousBelt.Quality = 100;
				RecruitsPiousBelt.Condition = 50000;
				RecruitsPiousBelt.MaxCondition = 50000;
				RecruitsPiousBelt.Durability = 50000;
				RecruitsPiousBelt.MaxDurability = 50000;
				RecruitsPiousBelt.IsDropable = false;

				RecruitsPiousBelt.AddBonus((byte)eProperty.Acuity, 3);
				RecruitsPiousBelt.AddBonus((byte)eProperty.AllMagicSkills, 1);
				RecruitsPiousBelt.AddBonus((byte)eStat.DEX, 3);
				RecruitsPiousBelt.AddBonus((byte)eProperty.MaxHealth, 8);

				GameServer.Database.AddNewObject(RecruitsPiousBelt);
			}
			#endregion

			Level = 3;
			QuestGiver = Richael;
			Rewards.Experience = 90;
			Rewards.MoneyPercent = 100;
			Rewards.AddOptionalItem(RecruitsIntelligentBelt);
			Rewards.AddOptionalItem(RecruitsMightyBelt);
			Rewards.AddOptionalItem(RecruitsPiousBelt);
			Rewards.ChoiceOf = 1;

			FoundBreach = AddGoal("Find the entrance to the Demon's Breach", QuestGoal.GoalType.ScoutMission, 1, null);

		}

		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			if (!ServerProperties.Properties.LOAD_QUESTS)
				return;
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initializing ...");


			#region defineNPCS

			GameNPC[] npcs = WorldMgr.GetNPCsByName("Richael", eRealm.Hibernia);

			if (npcs.Length == 0)
			{
				Richael = new GameNPC();
				Richael.Model = 377;
				Richael.Name = "Richael";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + Richael.Name + ", creating her ...");
				//k109: My preference, no guildname for quest NPCs.  Uncomment if you like that...
				//Richael.GuildName = "Part of " + questTitle + " Quest";
				Richael.Realm = eRealm.Hibernia;
				Richael.CurrentRegionID = 200;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.HandsArmor, 416, 37);       //Slot 22
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 413, 37);       //Slot 25
				template.AddNPCEquipment(eInventorySlot.FeetArmor, 417, 37);        //Slot 23
				template.AddNPCEquipment(eInventorySlot.LegsArmor, 414, 35);        //Slot 27
				template.AddNPCEquipment(eInventorySlot.Cloak, 57, 35);             //Slot 26
				template.AddNPCEquipment(eInventorySlot.ArmsArmor, 415, 37);             //Slot 28
				Richael.Inventory = template.CloseTemplate();
				Richael.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				Richael.Size = 48;
				Richael.Level = 38;
				Richael.X = 347089;
				Richael.Y = 491290;
				Richael.Z = 5247;
				Richael.Heading = 978;

				if (SAVE_INTO_DATABASE)
					Richael.SaveIntoDatabase();

				Richael.AddToWorld();
			}
			else
				Richael = npcs[0];

			#endregion
			#region defineAreas
			Demons_Breach_Area = WorldMgr.GetRegion(Demons_Breach.RegionID).AddArea(new Area.Circle("", Demons_Breach.X, Demons_Breach.Y, Demons_Breach.Z, 200));
			Demons_Breach_Area.RegisterPlayerEnter(new DOLEventHandler(PlayerEnterDemonBreachArea));
			#endregion

			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.AddHandler(Richael, GameLivingEvent.Interact, new DOLEventHandler(TalkToRichael));
			GameEventMgr.AddHandler(Richael, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToRichael));

			Richael.AddQuestToGive(typeof(ToReachTheBreach));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			if (Richael == null)
				return;

			Demons_Breach_Area.UnRegisterPlayerEnter(new DOLEventHandler(PlayerEnterDemonBreachArea));
			WorldMgr.GetRegion(Demons_Breach.RegionID).RemoveArea(Demons_Breach_Area);

			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(SubscribeQuest));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(SubscribeQuest));

			GameEventMgr.RemoveHandler(Richael, GameObjectEvent.Interact, new DOLEventHandler(TalkToRichael));
			GameEventMgr.RemoveHandler(Richael, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToRichael));

			Richael.RemoveQuestToGive(typeof(ToReachTheBreach));
		}

		protected static void TalkToRichael(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
			if (player == null)
				return;

			if (Richael.CanGiveQuest(typeof(ToReachTheBreach), player) <= 0)
				return;


			ToReachTheBreach quest = player.IsDoingQuest(typeof(ToReachTheBreach)) as ToReachTheBreach;
			Richael.TurnTo(player);

			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					quest = new ToReachTheBreach();
					quest.QuestGiver = Richael;
					quest.OfferQuest(player);
				}
				else
				{
					if (quest.Step == 1 && quest.FoundBreach.IsAchieved)
					{
						quest.ChooseRewards(player);
					}
				}
			}
		}

		/// <summary>
		/// Callback for player accept/decline action.
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		protected static void SubscribeQuest(DOLEvent e, object sender, EventArgs args)
		{
			QuestEventArgs qargs = args as QuestEventArgs;
			if (qargs == null)
				return;

			if (qargs.QuestID != QuestMgr.GetIDForQuestType(typeof(ToReachTheBreach)))
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
			if (player.IsDoingQuest(typeof(ToReachTheBreach)) != null)
				return true;

			// This checks below are only performed is player isn't doing quest already

			if (player.Level < minimumLevel || player.Level > maximumLevel)
				return false;

			return true;
		}

		private static void CheckPlayerAbortQuest(GamePlayer player, byte response)
		{
			ToReachTheBreach quest = player.IsDoingQuest(typeof(ToReachTheBreach)) as ToReachTheBreach;

			if (quest == null)
				return;

			if (response == 0x00)
			{
				SendSystemMessage(player, "Good, now go out there and finish your work!");
			}
			else
			{
				SendSystemMessage(player, "Aborting Quest " + questTitle + ". You can start over again if you want.");
				quest.AbortQuest();
			}
		}

		private static void CheckPlayerAcceptQuest(GamePlayer player, byte response)
		{
			// We recheck the qualification, because we don't talk to players
			// who are not doing the quest.

			if (Richael.CanGiveQuest(typeof(ToReachTheBreach), player) <= 0)
				return;

			if (player.IsDoingQuest(typeof(ToReachTheBreach)) != null)
				return;

			if (response == 0x00)
			{
				// Player declined, don't do anything.
			}
			else
			{
				// Player accepted, let's try to give him the quest.

				if (!Richael.GiveQuest(typeof(ToReachTheBreach), player, 1))
					return;

			}
		}

		/// <summary>
		/// The quest title.
		/// </summary>
		public override string Name
		{
			get { return questTitle; }
		}

		/// <summary>
		/// The text for individual quest steps as shown in the journal.
		/// </summary>

		public override string Description
		{
			get
			{
				switch (Step)
				{

					case 1:
						return Summary;
					default:
						return "No Queststep Description available.";
				}
			}
		}

		/// <summary>
		/// The fully-fledged story to the quest.
		/// </summary>
		public override string Story
		{
			get
			{
				String desc = "Taskmaster Sevinia is working diligently against the evil trickling into our land from the dungeons."
					+ "Recently, more evil has surfaced in the form of Demon's Breach.  The Siabran archmages of the Unseelie court meddled with the energies near Mag Mell."
					+ "Believing the area was a nexus for only two planes, they unleashed a storm of arcane energy, altering the disposition of the wildlife, animating the dead, and creating a tear into a demonic plane."
					+ "To worsen matters Midgard and Albion gained access to this plane.  The foes unleashed on Hibernia by the Siabran carelessness will do more damage then the Unseelie.\n\n"
					+ "Fortunately, our Path elders noticed this immediately, and they capped the hole, barring entry into our world.  We need a more permanent solution.  There's worry our enemies will break the ward sealing the lair.";
				return desc;
			}
		}

		/// <summary>
		/// A summary of the quest's story.
		/// </summary>
		public override string Summary
		{
			get
			{
				return "Find the entrance to the Demon's Breach. Return to Richael once you've visited the Breach.";
			}
		}

		/// <summary>
		/// Text showing upon finishing the quest.
		/// </summary>
		public override String Conclusion
		{
			get
			{
				String text = String.Format("You found your way to Demon's Breach, eh?  Mark it's location well, {0}", QuestPlayer.CharacterClass.Name);
				text += ", for your assistance may be needed there one day.  We can't afford to allow hordes of demons, Albionians or Midgardians to break free into our lands.";
				return text;
			}
		}

		/// <summary>
		/// The level of the quest as it shows in the journal.
		/// </summary>
		public override int Level
		{
			get
			{
				return 1;
			}
		}
		protected static void PlayerEnterDemonBreachArea(DOLEvent e, object sender, EventArgs args)
		{
			AreaEventArgs aargs = args as AreaEventArgs;
			GamePlayer player = aargs.GameObject as GamePlayer;

			if (player == null)
				return;
			if (player.IsDoingQuest(typeof(ToReachTheBreach)) == null)
				return;

			ToReachTheBreach quest = player.IsDoingQuest(typeof(ToReachTheBreach)) as ToReachTheBreach;

			if (quest.Step == 1 && quest.FoundBreach.IsAchieved == false)
			{
				quest.FoundBreach.Advance();
				return;
			}
		}

		public override void AbortQuest()
		{
			base.AbortQuest(); //Defined in Quest, changes the state, stores in DB etc ...

		}
	}
}
