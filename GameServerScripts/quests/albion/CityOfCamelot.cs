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
 *  Brief Walkthrough: 
 *  1) Bombard can be found at loc=32479,62585 Black Mountains South. 
 *  2) Zone into Camelot and /USE the Assistant Necklace from Bombard. 
 *  3) Talk with the Camelot Assistant that appeared from using the necklace. Click on [vault keeper] option when it is given. 
 *  4) Hand Lord Urqhart the Scroll for Vault Keeper Urqhart from Bombard and then the Small Chest of Coins. 
 *  5) Click on the Camelot Assistant to teleport back to the main gate. 
 *  6) Go back to Bombard and hand him the Receipt for Bombard from Urqhart, chat with him a bit more for your reward.
 */

using System;
using System.Reflection;
using DOL.AI.Brain;
using DOL.GS.Database;
using DOL.Events;
using DOL.GS.Movement;
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
    public class CityOfCamelotDescriptor : AbstractQuestDescriptor
    {
        /* This is the type of the quest class linked with 
         * this requirement class, you must override the 
         * base methid like that
         */
        public override Type LinkedQuestType
        {
            get { return typeof(CityOfCamelot); }
        }

		/* This value is used to retrieves how maximum level needed
		 * to be able to make this quest. Override it only if you need, 
		 * the default value is 50
		 */
		public override int MaxLevel
		{
			get { return 1; }
		}

        public override bool CheckQuestQualification(GamePlayer player)
        {
			// if the player is already doing the quest always return true !!!
			if (player.IsDoingQuest(LinkedQuestType) != null)
				return true;

            // This checks below are only performed is player isn't doing quest already
            if (player.HasFinishedQuest(typeof(ImportantDelivery)) == 0)
                return false;

            if (!BaseFrederickQuest.CheckPartAccessible(player, LinkedQuestType))
                return false;

            return base.CheckQuestQualification(player);
        }
    }

    /* The second thing we do, is to declare the class we create
     * as Quest. We must make it persistant using attributes, to
     * do this, we derive from the abstract class AbstractQuest
     */
	[NHibernate.Mapping.Attributes.Subclass(NameType = typeof(CityOfCamelot), ExtendsType = typeof(AbstractQuest))]
	public class CityOfCamelot : BaseFrederickQuest
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

		protected const string questTitle = "City of Camelot";

		protected static GameLocation armsManTrainer = new GameLocation("Armsman Trainer", 10, 30985, 31520, 8216, 0);
		protected static GameLocation reaverTrainer = new GameLocation("Reaver Trainer", 10, 32545, 26900, 7830, 0);
		protected static GameLocation paladinTrainer = new GameLocation("Paladin Trainer", 10, 38920, 27845, 8636, 0);
		protected static GameLocation mercenaryTrainer = new GameLocation("Mercenary Trainer", 10, 32826, 26831, 7995, 0);

		protected static GameLocation vaultKeeper = new GameLocation("Vault Keeper", 10, 35408, 23830, 8751, 0);
		protected static GameLocation mainGates = new GameLocation("Main Gates", 10, 40069, 26463, 8255, 0);

		private static GameMob masterFrederick = null;
		private static GameMob lordUrqhart = null;
		private static GameMob bombard = null;

		private GameNPC assistant = null;
		private RegionTimer assistantTimer = null;

		private static TravelTicketTemplate ticketToCotswold = null;
		private static GenericItemTemplate scrollUrqhart = null;
		private static GenericItemTemplate receiptBombard = null;
		private static GenericItemTemplate letterFrederick = null;
		private static NecklaceTemplate assistantNecklace = null;
		private static GenericItemTemplate chestOfCoins = null;
		private static ShieldTemplate recruitsRoundShield = null;
		private static BracerTemplate recruitsBracer = null;

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

			#region defineNPCs

			masterFrederick = GetMasterFrederick();
			if(masterFrederick == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
				return;
			}

			lordUrqhart = ResearchQuestObject(typeof(GameMob), WorldMgr.GetRegion(10), eRealm.Albion, "Lord Urqhart") as GameMob;
			if (lordUrqhart == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Lord Urqhart, creating him ...");
				lordUrqhart = new GameMob();
				lordUrqhart.Model = 79;
				lordUrqhart.Name = "Lord Urqhart";
				lordUrqhart.GuildName = "Vault Keeper";
				lordUrqhart.Realm = (byte) eRealm.Albion;
				lordUrqhart.Region = WorldMgr.GetRegion(10);
				lordUrqhart.Size = 49;
				lordUrqhart.Level = 50;
				lordUrqhart.Position = new Point(35500, 23857, 8751);
				lordUrqhart.Heading = 603;
				
				StandardMobBrain newBrain = new StandardMobBrain();
				newBrain.Body = lordUrqhart;
				newBrain.AggroLevel = 100;
				newBrain.AggroRange = 0;
				lordUrqhart.OwnBrain = newBrain;

				if(!lordUrqhart.AddToWorld())
				{
					if (log.IsWarnEnabled)
						log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
					return;
				}

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(lordUrqhart);
			}


			bombard = ResearchQuestObject(typeof(GameMob), WorldMgr.GetRegion(1), eRealm.Albion, "Bombard") as GameMob;
			if (bombard == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Bombard, creating her ...");
				bombard = new GameStableMaster();
				bombard.Model = 8;
				bombard.Name = "Bombard";
				bombard.GuildName = "Stable Master";
				bombard.Realm = (byte) eRealm.Albion;
				bombard.Region = WorldMgr.GetRegion(1);
				bombard.Size = 49;
				bombard.Level = 4;
				bombard.Position = new Point(515718, 496739, 3352);
				bombard.Heading = 2500;

				StandardMobBrain newBrain = new StandardMobBrain();
				newBrain.Body = bombard;
				newBrain.AggroLevel = 100;
				newBrain.AggroRange = 0;
				bombard.OwnBrain = newBrain;

				if(!bombard.AddToWorld())
				{
					if (log.IsWarnEnabled)
						log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
					return;
				}

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(bombard);
			}

			#endregion

			#region define horse Paths

			PathPoint newPoint = null;
			PathPoint lastPoint = null;

			TripPath pathToCotswold = new TripPath();
			pathToCotswold.PathID = -20;
			pathToCotswold.Region = WorldMgr.GetRegion(1);
			pathToCotswold.SteedModel = 413;
			pathToCotswold.SteedName = "horse";

			if(!PathMgr.AddPath(pathToCotswold))
			{
				if (log.IsWarnEnabled)
					log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
				return;
			}
			
			#region all pathToCamelotHills points

			newPoint = new PathPoint();
			newPoint.Position = new Point(515765, 496737, 3352);
			newPoint.Speed = 0;

			pathToCotswold.StartingPoint = newPoint;

			lastPoint = newPoint;
		
			newPoint = new PathPoint();
			newPoint.Position = new Point(515809,496751,3352);
			newPoint.Speed = 14;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(516181,496820,3352);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(516639,496814,3352);
			newPoint.Speed = 70;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(516748,496701,3352);
			newPoint.Speed = 128;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(516832,496649,3352);
			newPoint.Speed = 128;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(517002,496529,3352);
			newPoint.Speed = 67;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(517006,496423,3373);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(516987,495759,3352);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(516957,492076,3349);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(516915,487275,3188);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(516894,486775,3184);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(516772,483642,3182);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(516820,481497,2459);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(516802,480791,2407);
			newPoint.Speed = 194;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(516609,480776,2406);
			newPoint.Speed = 135;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(516537,480597,2408);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(515098,477707,2332);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(514965,477192,2363);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(515265,474872,2823);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(516038,472574,2925);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(518045,471473,2381);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(519125,471862,2378);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(520702,472715,2438);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(525002,475074,2234);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(525423,475435,2288);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(527691,477396,2382);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(529057,479349,2243);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(532185,483818,2315);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(532922,484886,2236);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(534495,487604,2220);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(535038,488202,2220);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(535725,488871,2232);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(539423,491056,1957);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(539875,491262,1924);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(543418,491164,1968);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(544549,491001,1910);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(548126,490498,2167);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(549887,490673,2152);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(551798,490853,2210);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(553123,490859,2213);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(555350,490681,2189);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(557276,490532,2145);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(560519,490761,2172);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(562735,490944,2214);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(565041,491555,2256);
			newPoint.Speed = 191;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(565391,491685,2274);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(565551,491996,2264);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(566813,494410,2302);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(568438,496197,2308);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(569185,497057,2304);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(570289,501228,2143);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(567906,505921,2207);
			newPoint.Speed = 600;

			lastPoint.NextPoint = newPoint;
			lastPoint = newPoint;

			newPoint = new PathPoint();
			newPoint.Position = new Point(567376,507118,2248);
			newPoint.Speed = 600;

			#endregion

			#endregion

			#region defineItems

			// ------------- First traver ticket -----------------
			ticketToCotswold = new TravelTicketTemplate();
			ticketToCotswold.Name = "Ticket to Cotswold";
			if (log.IsWarnEnabled)
				log.Warn("Creating ticket " + ticketToCotswold.Name + " ...");

			ticketToCotswold.Weight = 0;
			ticketToCotswold.Model = 499;
			ticketToCotswold.Realm = eRealm.Albion;
			ticketToCotswold.Value = Money.GetMoney(0, 0, 0, 5, 0);

			ticketToCotswold.IsDropable = true;
			ticketToCotswold.IsSaleable = true;
			ticketToCotswold.IsTradable = true;

			ticketToCotswold.TripPathID = pathToCotswold.PathID;

			scrollUrqhart = GameServer.Database.SelectObject(typeof(GenericItemTemplate), Expression.Eq("Name", "Scroll for Vault Keeper Urqhart")) as GenericItemTemplate;
			if (scrollUrqhart == null)
			{
				scrollUrqhart = new GenericItemTemplate();
				scrollUrqhart.Name = "Scroll for Vault Keeper Urqhart";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + scrollUrqhart.Name + " , creating it ...");

				scrollUrqhart.Weight = 3;
				scrollUrqhart.Model = 498;

				scrollUrqhart.IsDropable = false;
				scrollUrqhart.IsSaleable = false;
				scrollUrqhart.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(scrollUrqhart);
			}


			receiptBombard = GameServer.Database.SelectObject(typeof (GenericItemTemplate), Expression.Eq("Name", "Receipt for Bombard")) as GenericItemTemplate;
			if (receiptBombard == null)
			{
				receiptBombard = new GenericItemTemplate();
				receiptBombard.Name = "Receipt for Bombard";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + receiptBombard.Name + " , creating it ...");

				receiptBombard.Weight = 3;
				receiptBombard.Model = 498;

				receiptBombard.IsDropable = false;
				receiptBombard.IsSaleable = false;
				receiptBombard.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(receiptBombard);
			}

			chestOfCoins = GameServer.Database.SelectObject(typeof (GenericItemTemplate), Expression.Eq("Name", "Small Chest of Coins")) as GenericItemTemplate;
			if (chestOfCoins == null)
			{
				chestOfCoins = new GenericItemTemplate();
				chestOfCoins.Name = "Small Chest of Coins";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + chestOfCoins.Name + " , creating it ...");

				chestOfCoins.Weight = 15;
				chestOfCoins.Model = 602;

				chestOfCoins.IsDropable = false;
				chestOfCoins.IsSaleable = false;
				chestOfCoins.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(chestOfCoins);
			}

			letterFrederick = GameServer.Database.SelectObject(typeof (GenericItemTemplate), Expression.Eq("Name", "Letter for Frederick")) as GenericItemTemplate;
			if (letterFrederick == null)
			{
				letterFrederick = new GenericItemTemplate();
				letterFrederick.Name = "Letter for Frederick";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + letterFrederick.Name + " , creating it ...");

				letterFrederick.Weight = 3;
				letterFrederick.Model = 498;

				letterFrederick.IsDropable = false;
				letterFrederick.IsSaleable = false;
				letterFrederick.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(letterFrederick);
			}

			assistantNecklace = GameServer.Database.SelectObject(typeof (NecklaceTemplate), Expression.Eq("Name", "Assistant's Necklace")) as NecklaceTemplate;
			if (assistantNecklace == null)
			{
				assistantNecklace = new NecklaceTemplate();
				assistantNecklace.Name = "Assistant's Necklace";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + assistantNecklace.Name + " , creating it ...");

				assistantNecklace.Weight = 3;
				assistantNecklace.Model = 101;

				assistantNecklace.IsDropable = false;
				assistantNecklace.IsSaleable = false;
				assistantNecklace.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(assistantNecklace);
			}

			// item db check
			recruitsRoundShield = GameServer.Database.SelectObject(typeof (ShieldTemplate), Expression.Eq("Name", "Recruit's Round Shield")) as ShieldTemplate;
			if (recruitsRoundShield == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find Recruit's Round Shield, creating it ...");
				recruitsRoundShield = new ShieldTemplate();
				recruitsRoundShield.Name = "Recruit's Round Shield";
				recruitsRoundShield.Level = 4;

				recruitsRoundShield.Weight = 31;
				recruitsRoundShield.Model = 59; // studded Boots                

				recruitsRoundShield.Value = 400;

				recruitsRoundShield.IsDropable = true;
				recruitsRoundShield.IsSaleable = true;
				recruitsRoundShield.IsTradable = true;

				recruitsRoundShield.Color = 36;

				recruitsRoundShield.Size = eShieldSize.Small;
				recruitsRoundShield.HandNeeded = eHandNeeded.LeftHand;

				recruitsRoundShield.Speed = 1000;
				recruitsRoundShield.DamagePerSecond = 1;

				recruitsRoundShield.Bonus = 1; // default bonus

				recruitsRoundShield.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 1));
				recruitsRoundShield.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Body, 1));

				recruitsRoundShield.Quality = 100;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsRoundShield);
			}

			// item db check
			recruitsBracer = GameServer.Database.SelectObject(typeof (BracerTemplate), Expression.Eq("Name", "Recruit's Silver Bracer")) as BracerTemplate;
			if (recruitsBracer == null)
			{
				recruitsBracer = new BracerTemplate();
				recruitsBracer.Name = "Recruit's Silver Bracer";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + recruitsBracer.Name + ", creating it ...");
				recruitsBracer.Level = 4;

				recruitsBracer.Weight = 10;
				recruitsBracer.Model = 130;

				recruitsBracer.Value = 400;

				recruitsBracer.IsDropable = true;
				recruitsBracer.IsSaleable = true;
				recruitsBracer.IsTradable = true;

				recruitsBracer.Bonus = 1; // default bonus

				recruitsBracer.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 8));
				recruitsBracer.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Resist_Crush, 1));

				recruitsBracer.Quality = 100;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(recruitsBracer);
			}

			#endregion

			/* Now we add some hooks to the npc we found.
			* Actually, we want to know when a player interacts with him.
			* So, we hook the right-click (interact) and the whisper method
			* of npc and set the callback method to the "TalkToXXX"
			* method. This means, the "TalkToXXX" method is called whenever
			* a player right clicks on him or when he whispers to him.
			*/
			//We want to be notified whenever a player enters the world            
			GameEventMgr.AddHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.AddHandler(masterFrederick, GameObjectEvent.Interact, new DOLEventHandler(TalkToMasterFrederick));
			GameEventMgr.AddHandler(masterFrederick, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterFrederick));

			GameEventMgr.AddHandler(bombard, GameObjectEvent.Interact, new DOLEventHandler(TalkToBombard));
			GameEventMgr.AddHandler(bombard, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToBombard));

			GameEventMgr.AddHandler(lordUrqhart, GameObjectEvent.Interact, new DOLEventHandler(TalkToUrqhart));
			GameEventMgr.AddHandler(lordUrqhart, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToUrqhart));

			/* Now we add some hooks to trigger the quest dialog reponse. */
			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogResponse));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogResponse));

			/* Now we bring to Ydenia the possibility to give this quest to players */
			QuestMgr.AddQuestDescriptor(bombard, typeof(CityOfCamelotDescriptor));

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
			if (masterFrederick == null)
				return;

			/* Removing hooks works just as adding them but instead of 
			 * AddHandler, we call RemoveHandler, the parameters stay the same
			 */
			GameEventMgr.RemoveHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.RemoveHandler(masterFrederick, GameObjectEvent.Interact, new DOLEventHandler(TalkToMasterFrederick));
			GameEventMgr.RemoveHandler(masterFrederick, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToMasterFrederick));

			GameEventMgr.RemoveHandler(bombard, GameObjectEvent.Interact, new DOLEventHandler(TalkToBombard));
			GameEventMgr.RemoveHandler(bombard, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToBombard));

			GameEventMgr.RemoveHandler(lordUrqhart, GameObjectEvent.Interact, new DOLEventHandler(TalkToUrqhart));
			GameEventMgr.RemoveHandler(lordUrqhart, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToUrqhart));

			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogResponse));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogResponse));

			/* Now we remove to bombard the possibility to give this quest to players */
			QuestMgr.RemoveQuestDescriptor(bombard, typeof(CityOfCamelotDescriptor));
		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToMasterFrederick(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if (QuestMgr.CanGiveQuest(typeof(CityOfCamelot), player, bombard) <= 0)
				return;

			//We also check if the player is already doing the quest
			CityOfCamelot quest = player.IsDoingQuest(typeof (CityOfCamelot)) as CityOfCamelot;

			masterFrederick.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					//Player is not doing the quest...
					if (quest.Step == 9)
					{
						masterFrederick.SayTo(player, "This note puts you in high regard "+player.Name+". I'm glad to see that you are so willing to serve Albion and so ready to help her citizens. For your selfless acts of servitude, I have this [reward] for you.");
						quest.ChangeQuestStep(10);
					}
					return;
				}
			}
				// The player whispered to NPC (clicked on the text inside the [])
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				if (quest != null)
				{
					//Do some small talk :)
					switch (wArgs.Text)
					{
						case "reward":
							masterFrederick.SayTo(player, "You have shown that you are all that a recruit can hope to be. This token of my esteem and gratitude is but a fraction of the gratitude Albion will show you once you are a mighty warrior. Do not go far my young friend. I have more for you to do.");
							if (quest.Step == 10)
							{
								quest.FinishQuest();
								masterFrederick.SayTo(player, "Don't wander too far my young recruit. There are more things for you to accomplish. Visit me when you have reached your second season.");
							}
							break;
					}
				}

			}
		}

		protected static void TalkToUrqhart(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if (QuestMgr.CanGiveQuest(typeof(CityOfCamelot), player, bombard) <= 0)
				return;

			//We also check if the player is already doing the quest
			CityOfCamelot quest = player.IsDoingQuest(typeof (CityOfCamelot)) as CityOfCamelot;

			lordUrqhart.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null)
				{
					if (quest.Step < 6)
					{
						lordUrqhart.SayTo(player, "Greetings friend. How may I be of assistance to you today?");
					}
					else if (quest.Step == 6)
					{
						lordUrqhart.SayTo(player, "Ah, I am now [done].");
					}

				}
				return;
			}
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				if (quest != null)
				{
					//Do some small talk :)
					switch (wArgs.Text)
					{
						case "done":
							lordUrqhart.SayTo(player, "Alright, here you are my friend, as promised. Please be sure to return it to Bombard. Have a nice day!");
							if (quest.Step == 6)
							{
								player.ReceiveItem(lordUrqhart, receiptBombard.CreateInstance());
								quest.ChangeQuestStep(7);
							}
							break;
					}
				}
			}
		}

		protected static void TalkToBombard(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if (QuestMgr.CanGiveQuest(typeof(CityOfCamelot), player, bombard) <= 0)
				return;

			//We also check if the player is already doing the quest
			CityOfCamelot quest = player.IsDoingQuest(typeof (CityOfCamelot)) as CityOfCamelot;

			bombard.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					bombard.SayTo(player, "I wanted to thank you again for delivering these vegetables to me. My horses and my family will be able to eat well these next few days. However, I am in need of yet another [favor], if you are so inclined.");
				}
				return;
			}
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				if (quest == null)
				{
					//Do some small talk :)
					switch (wArgs.Text)
					{
						case "favor":
							bombard.SayTo(player, "I have here a chest of coins that I need taken to the Vault Keeper inside of Camelot City. Since my stable is fairly busy, I don't have time to leave it, and by the time I do get a moment, the banker has gone home for the day. It's a simple [errand], really.");
							break;
						case "errand":
							bombard.SayTo(player, "I trust that you won't steal from me, so what do you say? Will you [do this] for me or not?");
							break;
						case "do this":
							QuestMgr.ProposeQuestToPlayer(typeof(CityOfCamelot), "Will you take these coins to Vault Keeper Urqhart in Camelot?", player, bombard);
							break;
					}
				}
				else
				{
					//Do some small talk :)
					switch (wArgs.Text)
					{
						case "necklace":
							bombard.SayTo(player, "The necklace was made by my wife, a Cabalist with the Academy. She is currently helping out with a few things in Avalon Marsh, or else she could take the money to the vault keeper, hehe. Anyhow, you'll need to USE the necklace once you're inside Camelot City. I'm sure your journal there will be able to help you out. Don't worry, you'll get the hang of it. Be sure to give the Vault Keeper the scroll. Good luck, and thank you again.");
							break;
					}
				}
			}
		}

		protected static void TalkToAssistant(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if (QuestMgr.CanGiveQuest(typeof(CityOfCamelot), player, bombard) <= 0)
				return;

			//We also check if the player is already doing the quest
			CityOfCamelot quest = player.IsDoingQuest(typeof (CityOfCamelot)) as CityOfCamelot;

			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest != null && quest.assistant == sender)
				{
					quest.assistant.TurnTo(player);
					quest.assistant.SayTo(player, "Greetings to you. I am here to assist you on your journey through Camelot. Please listen [carefully] to my instructions.");
					if (quest.Step == 2)
					{
						quest.ChangeQuestStep(3);
					}
				}
				return;
			}
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				if (quest != null && quest.assistant == sender)
				{
					quest.assistant.TurnTo(player);
					//Do some small talk :)
					switch (wArgs.Text)
					{
						case "carefully":
							quest.assistant.SayTo(player, "I have a limited time duration. I will only be around for about an hour (2 real life minutes), so please use me wisely. I can teleport you to various [trainers] within Camelot.");
							break;
						case "trainers":
							quest.assistant.SayTo(player, "I know that you have been given the task of delivering Bombard's coins to the vault keeper. I can teleport you there as well. All you need to do is make a [choice].");
							break;
						case "choice":
							quest.assistant.SayTo(player, "Where would you like to go to? The [paladin] trainer; the [armsman] trainer; the [reaver] trainer; the [mercenary] trainer; to the [vault keeper] or back tp the [main gates]?");
							break;

						case "paladin":
							quest.TeleportTo(paladinTrainer);
							break;
						case "armsman":
							quest.TeleportTo(armsManTrainer);
							break;
						case "reaver":
							quest.TeleportTo(reaverTrainer);
							break;
						case "mercenary":
							quest.TeleportTo(mercenaryTrainer);
							break;
						case "main gates":
							quest.TeleportTo(mainGates);
							break;
						case "vault keeper":
							if (quest.Step == 3)
							{
								quest.ChangeQuestStep(4);
							}
							quest.TeleportTo(vaultKeeper);
							break;

						case "go away":
							quest.assistantTimer.Start(1);
							break;

					}
				}
			}
		}

		/**
		 * Convinient Method for teleporintg with assistant 
		 */

		protected void TeleportTo(GameLocation target)
		{
			TeleportTo(m_questPlayer, assistant, target, 5);
			TeleportTo(assistant, assistant, target, 25, 50);
		}


		protected static void PlayerEnterWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			CityOfCamelot quest = player.IsDoingQuest(typeof (CityOfCamelot)) as CityOfCamelot;
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

			CityOfCamelot quest = player.IsDoingQuest(typeof (CityOfCamelot)) as CityOfCamelot;
			if (quest != null)
			{
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
				// remorph player back...
				if (quest.assistantTimer != null)
					quest.assistantTimer.Start(1);

			}
		}

		protected static void PlayerUseSlot(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			// assistant works only in camelot...
			if (player.Region.RegionID != 10)
				return;

			CityOfCamelot quest = (CityOfCamelot) player.IsDoingQuest(typeof (CityOfCamelot));
			if (quest == null)
				return;

			UseSlotEventArgs uArgs = (UseSlotEventArgs) args;

			GenericItem item = player.Inventory.GetItem((eInventorySlot)uArgs.Slot) as GenericItem;
			if (item != null && item.Name == assistantNecklace.Name)
			{
				foreach (GamePlayer visPlayer in player.GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
				{
					visPlayer.Out.SendSpellCastAnimation(player, 1, 20);
				}

				new RegionTimer(player, new RegionTimerCallback(quest.CreateAssistant), 2000);

				if (quest.Step == 1)
				{
					quest.ChangeQuestStep(2);
				}
			}
		}

		protected virtual int DeleteAssistant(RegionTimer callingTimer)
		{
			if (assistant != null)
			{
				assistant.RemoveFromWorld();

				GameEventMgr.RemoveHandler(assistant, GameObjectEvent.Interact, new DOLEventHandler(TalkToAssistant));
				GameEventMgr.RemoveHandler(assistant, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToAssistant));
			}
			return 0;
		}

		protected virtual int CreateAssistant(RegionTimer timer)
		{
			if (assistant != null && assistant.ObjectState == eObjectState.Active)
			{
				Point pos = m_questPlayer.Position;
				pos.X += 50;
				pos.Y += 30;
				assistant.MoveTo(m_questPlayer.Region, pos, (ushort)m_questPlayer.Heading);
			}
			else
			{
				assistant = new GameMob();
				assistant.Model = 951;
				assistant.Name = m_questPlayer.Name + "'s Assistant";
				assistant.GuildName = "Part of " + questTitle + " Quest";
				assistant.Realm = m_questPlayer.Realm;
				assistant.Region = m_questPlayer.Region;
				assistant.Size = 25;
				assistant.Level = 5;
				Point pos = m_questPlayer.Position;
				pos.X += 50;
				pos.Y += 50;
				assistant.Position = pos;
				assistant.Heading = m_questPlayer.Heading;

				assistant.AddToWorld();

				GameEventMgr.AddHandler(assistant, GameObjectEvent.Interact, new DOLEventHandler(TalkToAssistant));
				GameEventMgr.AddHandler(assistant, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToAssistant));
			}

			foreach (GamePlayer visPlayer in assistant.GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
			{
				visPlayer.Out.SendEmoteAnimation(assistant, eEmote.Bind);
			}

			if (assistantTimer != null)
			{
				assistantTimer.Stop();
			}
			assistantTimer = new RegionTimer(assistant, new RegionTimerCallback(DeleteAssistant), 120000);

			return 0;
		}

		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest offer dialog. We check if he accepts or
		 * declines here...
		 */
		protected static void QuestDialogResponse(DOLEvent e, object sender, EventArgs args)
		{
			QuestEventArgs gArgs = args as QuestEventArgs;

			if (gArgs != null && gArgs.QuestType.Equals(typeof(CityOfCamelot)))
			{
				GamePlayer player = gArgs.Player;
				if (player == null) return;

				if (e == GamePlayerEvent.AcceptQuest)
				{
					if (QuestMgr.GiveQuestToPlayer(typeof(CityOfCamelot), player, gArgs.Source as GameNPC))
					{
						bombard.SayTo(player, "Oh thank you "+player.Name+". This means a lot to me. Here, take this chest of coins, this scroll and this [necklace].");

						AbstractQuest quest = player.IsDoingQuest(typeof (CityOfCamelot));
						GiveItemToPlayer(bombard, CreateQuestItem(assistantNecklace, quest), player);
						GiveItemToPlayer(bombard, CreateQuestItem(chestOfCoins, quest), player);
						GiveItemToPlayer(bombard, CreateQuestItem(scrollUrqhart, quest), player);
						player.GainExperience(7, 0, 0, true);

						GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
						GameEventMgr.AddHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
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
						return "[Step #1] Bombard has given you a Chest of Coins he needs taken to the vault keeper. Make your way inside the gates of Camelot and /USE the item Bombard gave you. The gates of Camelot are between the two large stone soldiers near Bombard.";
					case 2:
						return "[Step #2] You have successfully USED the necklace. Speak with the Camelot Assistant for further instructions.";
					case 3:
						return "[Step #3] Chose the destination you wish to know more about. If you do not need the Assistant, tell him to [go away] If you go straight to the Vault Keeper, be sure to hand him the scroll from Bombard.";
					case 4:
						return "[Step #4] Speak with Lord Urqhart. Be sure to give him the note Bombard gave you.";
					case 5:
						return "[Step #5] Hand the Small Chest of Coins over to Lord Urqhart.";
					case 6:
						return "[Step #6] Wait for the Vault Keeper to finish counting the coins. If he stops speaking with you, ask him if he is [done] counting the coins.";
					case 7:
						return "[Step #7] Take the Receipt to Bombard outside the Camelot Gates. Ask your Camelot Assistant to transport you back to the [main gates].";
					case 8:
						return "[Step #8] Take the horse back to Cotswold. Be sure to give Master Frederick the note from Bombard.";
					case 9:
						return "[Step #9] Wait for Master Frederick to finish reading the note from Bombard. If he stops speaking with you, ask him if he is [done] with the letter.";
					case 10:
						return "[Step #10] Wait for Master Frederick to reward you. If your trainer stops speaking with you at any time, ask him if there is a [reward] for your efforts.";
					default:
						return "[Step #" + Step + "] No Description entered for this step!";
				}
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player==null || player.IsDoingQuest(typeof (CityOfCamelot)) == null)
				return;

			if(e == GamePlayerEvent.GiveItem)
			{
				if (Step <= 4)
				{
					GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
					if (gArgs.Target == lordUrqhart && gArgs.Item.QuestName == Name && gArgs.Item.Name == scrollUrqhart.Name)
					{
						lordUrqhart.SayTo(player, "Ah, a note from Bombard. Excellent. I see he wishes to make a deposit. Alright then, just hand me the chest please.");
						RemoveItemFromPlayer(lordUrqhart, gArgs.Item);
						ChangeQuestStep(5);
						return;
					}
				}

				if (Step == 5)
				{
					GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
					if (gArgs.Target == lordUrqhart && gArgs.Item.QuestName == Name && gArgs.Item.Name == chestOfCoins.Name)
					{
						lordUrqhart.SayTo(player, "My this is heavy. Business must be booming for him! Well, one moment and I will write you a receipt to take back to him.");
						RemoveItemFromPlayer(lordUrqhart, gArgs.Item);
						ChangeQuestStep(6);
						return;
					}
				}

				if (Step == 7)
				{
					GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
					if (gArgs.Target == bombard && gArgs.Item.QuestName == Name && gArgs.Item.Name == receiptBombard.Name)
					{
						bombard.SayTo(player, "Ah, fantastic. I'm glad to know my money is now in a safe place. Thank you so much for doing that for me, and I hope the trip into Camelot was informative for you. Here, take this letter back to Master Frederick in Cotswold. I want for him to know what a fantastic job you did for me by delivering these vegetables from Ludlow, and for taking care of some business in Camelot for me. Thank you again "+player.Name+". I hope we speak again soon.");

						RemoveItemFromPlayer(bombard, gArgs.Item);
						RemoveItemFromPlayer(bombard, player.Inventory.GetFirstItemByName(assistantNecklace.Name, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack));

						GiveItemToPlayer(bombard, CreateQuestItem(ticketToCotswold));
						GiveItemToPlayer(bombard, CreateQuestItem(letterFrederick));

						player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0x00, 0x00, 0x00, 0x00, eDialogType.Ok, true, "You receive a horse ticket. \nGive it to Bombard to go to Cotswold.");
                            
						ChangeQuestStep(8);
						return;
					}
				}

				if (Step == 8)
				{
					GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
					if (gArgs.Target == masterFrederick && gArgs.Item.Name == letterFrederick.Name)
					{
						RemoveItemFromPlayer(masterFrederick, gArgs.Item);
						
						masterFrederick.SayTo(player, "Ah, from Bombard. Let me see what is says. One moment please.");
						SendSystemMessage(player, "Master Frederick reads the note from Bombard carefully.");
						player.Out.SendEmoteAnimation(masterFrederick, eEmote.Ponder);

						ChangeQuestStep(9);
						return;
					}
				}
			}
		}

		public override void FinishQuest()
		{
			//Give reward to player here ...            
			if (m_questPlayer.HasAbilityToUseItem(recruitsRoundShield.CreateInstance() as Shield))
				GiveItemToPlayer(masterFrederick, recruitsRoundShield.CreateInstance());
			else
				GiveItemToPlayer(masterFrederick, recruitsBracer.CreateInstance());

			m_questPlayer.GainExperience(26, 0, 0, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 2, Util.Random(50)), "You recieve {0} as a reward.");

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));

			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...
		}
	}
}
