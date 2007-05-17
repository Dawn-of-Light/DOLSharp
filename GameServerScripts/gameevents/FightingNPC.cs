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
 * Author:	SmallHorse & Crystalö
 * Date:	20.11.2003
 * This script should be put in /scripts/gameevents directory.
 * This event simulates a guard-trainer and some of his trainees.
 * They all come with basic equipment and do some attack as well
 * as some defense styles. Note: This is just a fight-simulation,
 * no real combat, hp-loss or anything, just the clanging of swords.
 */
using System;
using System.Reflection;
using System.Timers;
using DOL.Events;
using log4net;

namespace DOL.GS.GameEvents
{
	//Declare our event class
	public class FightingNPCEvent
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		//Declare our fightingNPC class which derives from GameNPC (and inherits all it's functionality)
		public class FightingNPC : GameNPC
		{
			//Empty constructor, sets the default parameters for this NPC
			public FightingNPC() : base()
			{
				Z = 0;
				Heading = 0x0;
				Model = 40;
				Size = 50;
				Level = 10;
				Realm = 1;
				CurrentRegionID = 1;
				GuildName = "DOLTopia";
			}

			//If someone interacts (rightclicks on the guard this function is called)
			public override bool Interact(GamePlayer player)
			{
				if (!base.Interact(player))
					return false;
				Say("Can't you see I am busy " + player.Name + ", please be quiet!");
				return true;
			}
		}

		//Declare all the variables that will be used in the scope
		//of this event

		//We have one guard-master that will train all his trainees
		private static FightingNPC m_guardMaster;
		//We have an array of guard-trainees that will try to beat the master
		private static FightingNPC[] m_guardTrainee;
		//We need some randomness
		private static Random m_rnd;
		//And we need a timer to time our styles
		private static Timer m_fightTimer;


		//This function will be called to initialized the event
		//It needs to be declared in EVERY game-event and is used
		//to start things up.
		[ScriptLoadedEvent]
		public static void OnScriptsCompiled(DOLEvent e, object sender, EventArgs args)
		{
			if (!ServerProperties.Properties.LOAD_EXAMPLES)
				return;
			//We set a new random class
			m_rnd = new Random();

			//We create our guardmaster-trainer
			m_guardMaster = new FightingNPC();
			m_guardMaster.X = 531771;
			m_guardMaster.Y = 478755;
			m_guardMaster.Heading = 3570;
			m_guardMaster.Name = "Master Guard Trainer";
			//Now we add some nice equipment to the guard-trainer
			GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
			template.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 843, 43, 13);
			template.AddNPCEquipment(eInventorySlot.Cloak, 57, 65);
			template.AddNPCEquipment(eInventorySlot.TorsoArmor, 46);
			template.AddNPCEquipment(eInventorySlot.LegsArmor, 47);
			template.AddNPCEquipment(eInventorySlot.ArmsArmor, 48);
			template.AddNPCEquipment(eInventorySlot.HandsArmor, 49);
			template.AddNPCEquipment(eInventorySlot.FeetArmor, 50);
			m_guardMaster.Inventory = template.CloseTemplate();
			m_guardMaster.SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);

			/*m_guardMaster.AddNPCEquipment((byte)eInventorySlot.TwoHandWeapon,843,43,13,0);
			m_guardMaster.AddNPCEquipment((byte)eInventorySlot.Cloak,57,65,0,0);
			m_guardMaster.AddNPCEquipment((byte)eInventorySlot.TorsoArmor,46,0,0,0);
			m_guardMaster.AddNPCEquipment((byte)eInventorySlot.LegsArmor,47,0,0,0);
			m_guardMaster.AddNPCEquipment((byte)eInventorySlot.ArmsArmor,48,0,0,0);
			m_guardMaster.AddNPCEquipment((byte)eInventorySlot.HandsArmor,49,0,0,0);
			m_guardMaster.AddNPCEquipment((byte)eInventorySlot.FeetArmor,50,0,0,0);
			m_guardMaster.ActiveWeaponSlot=GameLiving.eActiveWeaponSlot.TwoHanded;*/

			//Now we declare 5 guard trainees that will fight the master and
			//stand in a circle around him.
			m_guardTrainee = new FightingNPC[5];

			// same inventory template for all
			template = new GameNpcInventoryTemplate();
			template.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 843);
			template.AddNPCEquipment(eInventorySlot.Cloak, 57, 32);
			template.AddNPCEquipment(eInventorySlot.TorsoArmor, 36);
			template.AddNPCEquipment(eInventorySlot.LegsArmor, 37);
			template.AddNPCEquipment(eInventorySlot.ArmsArmor, 38);
			template.AddNPCEquipment(eInventorySlot.HandsArmor, 39);
			template.AddNPCEquipment(eInventorySlot.FeetArmor, 40);
			template = template.CloseTemplate();

			for (int i = 0; i < 5; i++)
			{
				//Create a trainee
				m_guardTrainee[i] = new FightingNPC();
				m_guardTrainee[i].Name = "Guard Trainee";
				//Add some equipment to our trainee
				m_guardTrainee[i].Inventory = template;
				m_guardTrainee[i].SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);

				/*m_guardTrainee[i].AddNPCEquipment((byte)eVisibleItems.TWO_HANDED,843,0,0,0);
				m_guardTrainee[i].AddNPCEquipment((byte)eVisibleItems.CLOAK,57,32,0,0);
				m_guardTrainee[i].AddNPCEquipment((byte)eVisibleItems.TORSO,36,0,0,0);
				m_guardTrainee[i].AddNPCEquipment((byte)eVisibleItems.LEG,37,0,0,0);
				m_guardTrainee[i].AddNPCEquipment((byte)eVisibleItems.ARMS,38,0,0,0);
				m_guardTrainee[i].AddNPCEquipment((byte)eVisibleItems.HAND,39,0,0,0);
				m_guardTrainee[i].AddNPCEquipment((byte)eVisibleItems.BOOT,40,0,0,0);
				m_guardTrainee[i].ActiveWeaponSlot=GameLiving.eActiveWeaponSlot.TwoHanded;*/

				//Our trainee will face the master in combat position
				m_guardTrainee[i].TargetObject = m_guardMaster;
			}

			//We set the position, model and size of our trainees
			m_guardTrainee[0].X = m_guardMaster.X - 70;
			m_guardTrainee[0].Y = m_guardMaster.Y - 66;
			m_guardTrainee[0].Heading = m_guardTrainee[0].GetHeadingToSpot(m_guardMaster.X, m_guardMaster.Y);
			m_guardTrainee[0].Model = (ushort) m_rnd.Next(32, 55);
			m_guardTrainee[0].Size = (byte) (45 + m_rnd.Next(10));

			m_guardTrainee[1].X = m_guardMaster.X + 76;
			m_guardTrainee[1].Y = m_guardMaster.Y - 29;
			m_guardTrainee[1].Heading = m_guardTrainee[1].GetHeadingToSpot(m_guardMaster.X, m_guardMaster.Y);
			;
			m_guardTrainee[1].Model = (ushort) m_rnd.Next(32, 55);
			m_guardTrainee[1].Size = (byte) (45 + m_rnd.Next(10));

			m_guardTrainee[2].X = m_guardMaster.X - 110;
			m_guardTrainee[2].Y = m_guardMaster.Y + 22;
			m_guardTrainee[2].Heading = m_guardTrainee[2].GetHeadingToSpot(m_guardMaster.X, m_guardMaster.Y);
			;
			m_guardTrainee[2].Model = (ushort) m_rnd.Next(32, 55);
			m_guardTrainee[2].Size = (byte) (45 + m_rnd.Next(10));

			m_guardTrainee[3].X = m_guardMaster.X - 34;
			m_guardTrainee[3].Y = m_guardMaster.Y + 88;
			m_guardTrainee[3].Heading = m_guardTrainee[3].GetHeadingToSpot(m_guardMaster.X, m_guardMaster.Y);
			;
			m_guardTrainee[3].Model = (ushort) m_rnd.Next(32, 55);
			m_guardTrainee[3].Size = (byte) (45 + m_rnd.Next(10));

			m_guardTrainee[4].X = m_guardMaster.X + 52;
			m_guardTrainee[4].Y = m_guardMaster.Y + 64;
			m_guardTrainee[4].Heading = m_guardTrainee[4].GetHeadingToSpot(m_guardMaster.X, m_guardMaster.Y);
			;
			m_guardTrainee[4].Model = (ushort) m_rnd.Next(32, 55);
			m_guardTrainee[4].Size = (byte) (45 + m_rnd.Next(10));

			//Now we try to add the master and all trainees to the world
			bool good = true;
			if (!m_guardMaster.AddToWorld())
				good = false;
			for (int i = 0; i < 5; i++)
				if (!m_guardTrainee[i].AddToWorld())
					good = false;

			//We start our fight timer that will make the guards
			//act each second
			m_fightTimer = new Timer(1000);
			m_fightTimer.AutoReset = true;
			m_fightTimer.Elapsed += new ElapsedEventHandler(MakeAttackSequence);
			m_fightTimer.Start();

			if (log.IsInfoEnabled)
				if (log.IsInfoEnabled)
					log.Info("FightingNPCEvent initialized: "+good);
		}

		//This function is a timercallback function that 
		//is called every second
		protected static void MakeAttackSequence(object sender, ElapsedEventArgs args)
		{
			GameObject attacker;
			GameObject defender;
			ushort weapon = 0x13;
			ushort shield = 0;
			int attackValue = 0;
			int defenseValue = 0;

			//We need some random attack and defense animation
			attackValue = m_rnd.Next(45);
			defenseValue = m_rnd.Next(2);

			if (defenseValue == 0)
				defenseValue = 11;

			//We randomly set an attacker
			int currentAttacker = m_rnd.Next(8);

			//Is the attacker the master?
			if (currentAttacker > 4)
			{
				attacker = m_guardMaster;
				//If the attacker is our master-trainer, the defender is one of the trainees
				defender = m_guardTrainee[m_rnd.Next(5)];
				//The master trainer always hits!
				defenseValue = 11;
				//We set the attack-target of our master to the defending trainee
				m_guardMaster.TargetObject = defender;
			}
			else
			{
				//Our attacker is one of the trainees and the defender is the master
				attacker = m_guardTrainee[currentAttacker];
				defender = m_guardMaster;
				//We set attack-target of the master to the trainee that is attacking him
				//this makes the master face his attacker at the time of the attack (eg. spin around)
				m_guardMaster.TargetObject = m_guardTrainee[currentAttacker];
			}

			//Now we send the combat-animation to all players that are viewing
			//the combat scene			
			foreach (GamePlayer player in m_guardMaster.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				player.Out.SendCombatAnimation(attacker, defender, weapon, shield, attackValue, 0, (byte) defenseValue, 100);
		}

		//This function is called whenever the event is stopped
		//It should be used to clean up!
		[ScriptUnloadedEvent]
		public static void OnScriptUnload(DOLEvent e, object sender, EventArgs args)
		{
			if (!ServerProperties.Properties.LOAD_EXAMPLES)
				return;
			//We stop our timer ... no more attacks
			if (m_fightTimer != null) {
				m_fightTimer.Stop();
				m_fightTimer.Close();	
			}				

			//We delete our master and his trainees from the world
			if (m_guardMaster != null)
				m_guardMaster.Delete();
			if (m_guardTrainee != null)
				for (int i = 0; i < 5; i++)
					if (m_guardTrainee[i] != null)
						m_guardTrainee[i].Delete();
		}
	}
}