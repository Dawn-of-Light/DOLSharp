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
 * Directory: /scripts/quests/
 *
 * Description:
 * This class is the base quest class for all quest dealing with Maser Frederick:
 *
 * ImportantDelivery
 * CityOfCamelot
 * Nuisances
 * Traitor In Cotswold
 * Frontiers
 * Collection
 * IreFairyIre
 * Beginning Of War
 * Culmination
 */

using System;
using System.Reflection;
using DOL.AI.Brain;
using DOL.Database;
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

	public abstract class BaseFrederickQuest : BaseQuest
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

		/* We need to define the constructors from the base class here, else there might be problems
		 * when loading this quest...
		 */
		public BaseFrederickQuest() : base()
		{
		}

		public BaseFrederickQuest(GamePlayer questingPlayer) : base(questingPlayer)
		{
		}

		public BaseFrederickQuest(GamePlayer questingPlayer, int step) : base(questingPlayer, step)
		{
		}

		public BaseFrederickQuest(GamePlayer questingPlayer, DBQuest dbQuest) : base(questingPlayer, dbQuest)
		{
		}

		public static Type[] m_questSequel = new Type[]
			{
				typeof (ImportantDelivery), // level 1
				typeof (CityOfCamelot), // level 1
				typeof (Nuisances), // level 2
				typeof (TraitorInCotswold), // level 2
				typeof (Frontiers), // level 3
				typeof (Collection), // level 3
				typeof (IreFairyIre), // level 4
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

		public static GameNPC GetMasterFrederick()
		{
			GameNPC[] npcs = WorldMgr.GetObjectsByName<GameNPC>("Master Frederick", eRealm.Albion);

			GameNPC masterFrederick = null;

			if (npcs.Length == 0)
			{
				masterFrederick = new GameNPC();
				masterFrederick.Model = 32;
				masterFrederick.Name = "Master Frederick";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + masterFrederick.Name + ", creating him ...");
				masterFrederick.GuildName = "Part of Frederick Quests";
				masterFrederick.Realm = eRealm.Albion;
				masterFrederick.CurrentRegionID = 1;

				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.TorsoArmor, 41);
				template.AddNPCEquipment(eInventorySlot.LegsArmor, 42);
				template.AddNPCEquipment(eInventorySlot.FeetArmor, 40);
				template.AddNPCEquipment(eInventorySlot.Cloak, 91);
				template.AddNPCEquipment(eInventorySlot.RightHandWeapon, 4);
				masterFrederick.Inventory = template.CloseTemplate();
				masterFrederick.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

//				masterFrederick.AddNPCEquipment((byte) eVisibleItems.TORSO, 41, 0, 0, 0);
//				masterFrederick.AddNPCEquipment((byte) eVisibleItems.LEG, 42, 0, 0, 0);
//				masterFrederick.AddNPCEquipment((byte) eVisibleItems.BOOT, 40, 0, 0, 0);
//				masterFrederick.AddNPCEquipment((byte) eVisibleItems.CLOAK, 91, 0, 0, 0);
//				masterFrederick.AddNPCEquipment((byte) eVisibleItems.RIGHT_HAND, 4, 0, 0, 0);

				masterFrederick.Size = 50;
				masterFrederick.Level = 50;
				masterFrederick.X = 567969;
				masterFrederick.Y = 509880;
				masterFrederick.Z = 2861;
				masterFrederick.Heading = 65;

				StandardMobBrain brain = new StandardMobBrain();
				brain.AggroLevel = 0;
				brain.AggroRange = 0;
				masterFrederick.SetOwnBrain(brain);

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					masterFrederick.SaveIntoDatabase();

				masterFrederick.AddToWorld();
			}
			else
				masterFrederick = npcs[0];

			return masterFrederick;
		}

	}
}
