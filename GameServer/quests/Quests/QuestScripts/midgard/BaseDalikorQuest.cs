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
 * Directory: /scripts/quests/midgard
 *
 * Description:
 * This class is the base quest class for all quest dealing with Dalikor:
 *
 * ImportantDelivery
 * CityOfJordheim
 * Nuisances
 * Traitor In Mularn
 * Frontiers
 * Collection
 * Stolen Eggs
 * Beginning Of War
 * Culmination
 */

using System;
using System.Reflection;
using DOL.AI.Brain;
using DOL.Database2;
using DOL.GS.PacketHandler;
using log4net;
/* I suggest you declare yourself some namespaces for your quests
 * Like: DOL.GS.Quests.Albion
 *       DOL.GS.Quests.Midgard
 *       DOL.GS.Quests.Hibernia
 * Also this is the name that will show up in the database as QuestName
 * so setting good values here will result in easier to read and cleaner
 * GS Code
 */

namespace DOL.GS.Quests.Midgard
{
	/* The first thing we do, is to declare the class we create
	 * as Quest. To do this, we derive from the abstract class
	 * AbstractQuest
	 * 	 
	 */

	public abstract class BaseDalikorQuest : BaseQuest
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

		protected static GameLocation locationDalikor = new GameLocation("Dalikor", 100, 100, 41211, 50221, 5018, 242);

		/* We need to define the constructors from the base class here, else there might be problems
		 * when loading this quest...
		 */

		public BaseDalikorQuest() : base()
		{
		}

		public BaseDalikorQuest(GamePlayer questingPlayer) : base(questingPlayer)
		{
		}

		public BaseDalikorQuest(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public BaseDalikorQuest(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
		{
		}

		public static Type[] m_questSequel = new Type[]
			{
				typeof (ImportantDelivery), // level 1
				typeof (CityOfJordheim), // level 1
				typeof (Nuisances), // level 2
				typeof (TraitorInMularn), // level 2
				typeof (Frontiers), // level 3
				typeof (Collection), // level 3
				typeof (StolenEggs), // level 4
				typeof (BeginningOfWar), // level 4
				typeof (Culmination) // level 5
			};

		/// <summary>
		/// Array of all Types of Quest that belong to this Sequel, in the order they are intendet to play.
		/// </summary>
		public static Type[] QuestSequel
		{
			get { return m_questSequel; }
		}

		/// <summary>
		/// Checks wether player is allowed to to do Quest of type questtype with current configuration of quests.
		/// At the moment it is only checked wether player isn't still doing a quest thats before the given quest in the sequel
		/// 
		/// Eg. If player is doing Collection and hasn't finished it yet, he will not be allowed to do IreFrairyIre.
		/// </summary>
		/// <param name="player">player to do quest</param>
		/// <param name="questType">Type of quest</param>
		/// <returns>true if allowed else false</returns>
		public static bool CheckPartAccessible(GamePlayer player, Type questType)
		{
			int currentIndex = -1;
			for (int i = 0; i < QuestSequel.Length; i++)
			{
				if (questType == QuestSequel[i])
					currentIndex = i;
			}

			if (currentIndex == -1)
				throw new ArgumentException("Type " + questType + " is not part of given Sequel.");

			for (int i = 0; i < currentIndex; i++)
			{
				// player is still doing on of the parts before current part...
				// parts belonging to one sequel should not be done overlapping.
				if (player.IsDoingQuest(QuestSequel[i]) != null)
					return false;
			}

			// player isn't doing any of the parts before given part.
			return true;
		}

		public static GameNPC GetDalikor()
		{
			GameNPC[] npcs = WorldMgr.GetNPCsByName("Dalikor", eRealm.Midgard);

			GameNPC dalikor = null;

			if (npcs.Length == 0)
			{
				dalikor = new GameNPC();
				dalikor.Model = 159;
				dalikor.Name = "Dalikor";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + dalikor.Name + ", creating him ...");
				dalikor.GuildName = "Part of Dalikor Quests";
				dalikor.Realm = eRealm.Midgard;
				dalikor.CurrentRegionID = locationDalikor.RegionID;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 348);
				template.AddNPCEquipment(eInventorySlot.LegsArmor, 349);
				template.AddNPCEquipment(eInventorySlot.ArmsArmor, 350);
				template.AddNPCEquipment(eInventorySlot.HandsArmor, 351);
				template.AddNPCEquipment(eInventorySlot.FeetArmor, 352);
				template.AddNPCEquipment(eInventorySlot.RightHandWeapon, 4);
				dalikor.Inventory = template.CloseTemplate();
				dalikor.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

//				dalikor.AddNPCEquipment((byte) eEquipmentItems.TORSO, 348, 0, 0, 0);
//				dalikor.AddNPCEquipment((byte) eEquipmentItems.LEGS, 349, 0, 0, 0);
//				dalikor.AddNPCEquipment((byte) eEquipmentItems.ARMS, 350, 0, 0, 0);
//				dalikor.AddNPCEquipment((byte) eEquipmentItems.HAND, 351, 0, 0, 0);
//				dalikor.AddNPCEquipment((byte) eEquipmentItems.FEET, 352, 0, 0, 0);
//				dalikor.AddNPCEquipment((byte) eVisibleItems.RIGHT_HAND, 4, 0, 0, 0);

				dalikor.Size = 50;
				dalikor.Level = 50;
				dalikor.X = locationDalikor.X;
				dalikor.Y = locationDalikor.Y;
				dalikor.Z = locationDalikor.Z;
				dalikor.Heading = locationDalikor.Heading;

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				dalikor.SetOwnBrain(brain);

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					dalikor.SaveIntoDatabase();

				dalikor.AddToWorld();
			}
			else
				dalikor = npcs[0];

			return dalikor;
		}

	}
}
