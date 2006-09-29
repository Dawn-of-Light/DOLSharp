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
 * Author:	SmallHorse
 * Date:	15.10.2003
 * This is a more complex example of game events
 * This script creates 4 race horses that stand 
 * around and wait for their riders. If a horse is
 * mounted it automatically walks to the startline of
 * the race and waits for 1-3 more players to join the
 * race. If 2-4 horses are waiting on the startline,
 * the race will start. The horses will race along a
 * predefined racetrack and their speed is increased
 * and decreased by a small random value at each
 * pathpoint. The race goes in a circle. When the
 * horses arrive at the start again, the winnder is
 * announced and the riders dismounted!
 */
using System;
using System.Reflection;
using System.Timers;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.GameEvents
{
	//First we declare our event class, this class must implement the
	//IGameEvent interface to be recognized as event
	public class HorseRace
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		//--------------------------------------------------------------------
		// START OF RACEHORSE CLASS inside Event Class
		//--------------------------------------------------------------------
		//For our horserace event we need some racehorses of course!
		//Here we declare the racehorse class. Since race horses are
		//mobs our base class is GameMob
		protected class RaceHorse : GameMob
		{
			//Our horse will run along a certain path and this
			//variable will hold the current position
			protected int m_currentPathPoint;

			//The originalX,Y,Heading is the place where the horse is 
			//originally positioned. This is used to make the horse 
			//walk back to the startposition after the race
			protected int m_originalX;
			protected int m_originalY;
			protected ushort m_originalHeading;

			//This enum holds all possible states a racehorse can have
			public enum RaceHorseState
			{
				Grazing, //The horse is currently grazing
				WalkingToStart, //The horse is walking to the startline
				WaitingForRaceStart, //The horse is waiting for the race to start
				Racing, //The horse is racing
				WalkingToGrazing, //The horse is walking back to the grazing spot
				Finished //The horse has finished the race
			}

			//This will hold our current horse state
			protected RaceHorseState m_horseState;
			//This will store which horse we are
			//Eg. the first horse to enter the race is 0
			//the second one is 1 ... etc.h
			protected int m_horseNum;

			//The constructor of the racehorse takes the grazing position
			//and the HorseRace event as parameters
			public RaceHorse(int origX, int origY, ushort origHeading) : base()
			{
				m_originalX = origX;
				m_originalY = origY;
				m_originalHeading = origHeading;

				CurrentRegionID = 1;
				Level = 10;
				Realm = 0;
				X = m_originalX;
				Y = m_originalY;
				Z = 0;
				Heading = m_originalHeading;
				GuildName = "Race Horse";
			}

			public override int MAX_PASSENGERS
			{
				get
				{
					return 1;
				}
			}

			public override int SLOT_OFFSET
			{
				get
				{
					return 0;
				}
			}

			//Property HorseState
			public RaceHorseState HorseState
			{
				get { return m_horseState; }
				set { m_horseState = value; }
			}

			//Called when someone rightclicks on our racehorse
			public override bool Interact(GamePlayer player)
			{
				if (!base.Interact(player))
					return false;
				if (Riders.Length >= 1)
					return false;

				player.MountSteed(this, false);
				return true;
			}

			//Called when a player wants to mount our racehorse
			//if it returns true mount is allowed if it returns
			//false then the mounting is not allowed
			public override bool RiderMount(GamePlayer rider, bool forced)
			{
				bool good = base.RiderMount(rider, forced);
				if (!good)
					return good;

				//If the race has started already, we don't allow
				//the player to mount the racehorse
				if (!forced && IsRaceStarted())
				{
					rider.Out.SendMessage("Sorry, the race has started already! Wait until it is over!",
					                      eChatType.CT_System,
					                      eChatLoc.CL_SystemWindow);
					//Return false -> don't allow the mount
					return false;
				}

				//We print out a nice message to the other players that are
				//standing around the horse and to the player that is mounting
				foreach (GamePlayer player in GetPlayersInRadius(1500))
					if (player != rider)
					{
						player.Out.SendMessage(
							player.Name + " jumps onto " + Name + "'s back and get's ready for the race!",
							eChatType.CT_System,
							eChatLoc.CL_SystemWindow);
					}
					else
					{
						player.Out.SendMessage(
							"You jump onto " + Name + "'s back and get ready for the race!",
							eChatType.CT_System,
							eChatLoc.CL_SystemWindow);
					}

				//We register this horse in our event and get back
				//our startnumber (0-3)
				m_horseNum = RegisterRacingHorse(this);

				//Now we fetch our startposition
				int startX, startY;
				GetStartPoint(out startX, out startY);

				//Make the mob and it's rider walk to our startposition!
				WalkTo(startX, startY, 0, 250);
				//Set the horse state correctly
				HorseState = RaceHorseState.WalkingToStart;
				//Return true -> allow the mounting
				return true;
			}

			//Called when our rider wants to dismount
			//If it returns true, the dismounting is allowed
			//else the dismounting is prevented
			public override bool RiderDismount(bool forced, GamePlayer player)
			{
				//If the race has started and our horse is racing we 
				//prevent the dismounting and print a message
				if (!forced && player != null && IsRaceStarted() && HorseState == RaceHorseState.Racing)
				{
					player.Out.SendMessage(
						"You can't dismount during the race! Do you want to break all your bones?",
						eChatType.CT_System,
						eChatLoc.CL_SystemWindow);
					//return false -> prevent dismounting
					return false;
				}

				//Unregister our horse and free the startposition
				UnregisterRacingHorse(this);

				//Walk back to our grazing spot if the rider dismounts
				WalkTo(m_originalX, m_originalY, 0, 75);
				//Set our horsestate correctly
				m_horseState = RaceHorseState.WalkingToGrazing;
				//Return true -> allow the dismounting
				return base.RiderDismount(forced, player);
			}

			public override void Notify(DOLEvent e, object sender, EventArgs args)
			{
				base.Notify(e, sender, args);

				if (e == GameNPCEvent.ArriveAtTarget)
				{
					if (HorseState == RaceHorseState.WalkingToStart)
					{
						//We are at the first pathpoint
						m_currentPathPoint = 0;

						//Get the next point we will be racing to
						int x, y;
						GetNextPoint(m_currentPathPoint, out x, out y);

						//Turn the horse towards the next point on the path
						TurnTo(x, y);

						//We are waiting for the race to start now!
						m_horseState = RaceHorseState.WaitingForRaceStart;
					}
					else if (HorseState == RaceHorseState.WalkingToGrazing)
					{
						//We set our heading to our original heading
						Heading = m_originalHeading;
						//We broadcast the update to the players around us
						BroadcastUpdate();
						//Set the horse state back to grazing
						m_horseState = RaceHorseState.Grazing;
					}
				}
				else if (e == GameNPCEvent.CloseToTarget)
				{
					if (HorseState == RaceHorseState.Racing)
					{
						if (m_currentPathPoint >= (racePoints.Length/2))
						{
							//Our state is finished now
							m_horseState = RaceHorseState.Finished;

							//We register the finish in the raceevent
							HorseFinished(this);
						}
						else
						{
							//We set our new speed with a small random
							//variance up and down
							Random rnd = new Random();
							int speed = CurrentSpeed + rnd.Next(20) - 10;
							CurrentSpeed = (ushort) speed;

							//If our speed drops below 500 we set it to 500
							//if it get's higher than 550 we set it to 550
							if (CurrentSpeed < 500)
								CurrentSpeed = 500;
							else if (CurrentSpeed > 550)
								CurrentSpeed = 550;

							//We get our next pathpoint and
							int nextX, nextY;
							GetNextPoint(m_currentPathPoint, out nextX, out nextY);
							m_currentPathPoint++;

							WalkTo(nextX, nextY, 0, CurrentSpeed);
						}
					}
				}
			}

			//This function will be called by our raceevent when
			//the race is started
			public void StartRace()
			{
				//Set our state to racing
				m_horseState = RaceHorseState.Racing;

				//We are at the first startpoint
				m_currentPathPoint = 0;

				//We fetch the next pathpoint
				int x, y;
				GetNextPoint(m_currentPathPoint, out x, out y);

				//Now our current point is 1
				m_currentPathPoint++;
				//Walk to the target spot ... our speed is 500+random 20
				Random rnd = new Random();
				WalkTo(x, y, 0, (ushort) (500 + rnd.Next(20)));
			}

			//This function is used to return the next pathpoint for a horse
			//given the current pathpoint. The horses will race parallel to each
			//other, depending on their startnumber
			protected void GetNextPoint(int curPoint, out int x, out int y)
			{
				//We get the vector from our current point to the next point and turn
				//it by 90 degrees so it is normal to our vector to the next point
				float normvx = racePoints[curPoint + 1, 1] - racePoints[curPoint, 1]; //DIFFY
				float normvy = -(racePoints[curPoint + 1, 0] - racePoints[curPoint, 0]); //-DIFFX

				//We now get the unit vector of our normal vector
				float len = (float) Math.Sqrt(normvx*normvx + normvy*normvy);
				normvx /= len;
				normvy /= len;


				//We calculate the offset of the next point now based
				//on our startnumber
				float offx = 0;
				float offy = 0;

				switch (m_horseNum)
				{
					default:
						offx += normvx*15;
						offy += normvy*15;
						break;
					case 1:
						offx -= normvx*15;
						offy -= normvy*15;
						break;
					case 2:
						offx += normvx*45;
						offy += normvy*45;
						break;
					case 3:
						offx -= normvx*45;
						offy -= normvy*45;
						break;
				}

				//return the next point with the correct offset
				x = (int) (racePoints[curPoint + 1, 0] + offx);
				y = (int) (racePoints[curPoint + 1, 1] + offy);
			}

			//Get the startpoint of our horse based on the startnumber
			//Works exactly like the getnextpoint function
			protected void GetStartPoint(out int x, out int y)
			{
				float normvx = racePoints[1, 1] - racePoints[0, 1]; //DIFFY
				float normvy = -(racePoints[1, 0] - racePoints[0, 0]); //-DIFFX

				float len = (float) Math.Sqrt(normvx*normvx + normvy*normvy);
				normvx /= len;
				normvy /= len;

				float offx = 0;
				float offy = 0;


				switch (m_horseNum)
				{
					default:
						offx += normvx*15;
						offy += normvy*15;
						break;
					case 1:
						offx -= normvx*15;
						offy -= normvy*15;
						break;
					case 2:
						offx += normvx*45;
						offy += normvy*45;
						break;
					case 3:
						offx -= normvx*45;
						offy -= normvy*45;
						break;
				}

				x = (int) (racePoints[0, 0] + offx);
				y = (int) (racePoints[0, 1] + offy);
			}
		}

		//--------------------------------------------------------------------
		// END OF RACEHORSE CLASS inside Event Class
		//--------------------------------------------------------------------


		//This variable will hold all our race horses
		protected static RaceHorse[] m_horses;
		//This variable will hold if our race has been started or not
		protected static bool m_raceStarted;
		//This variable will hold all our riders
		protected static RaceHorse[] m_racingHorses;
		//This variable will hold our current ranking
		protected static int m_currentRanking;
		//This variable will hold a timer that will check for the
		//race start each 30 seconds
		protected static Timer m_raceStartTimer;

		//This function is implemented from the IGameEvent
		//interface and is called on serverstart when the
		//events need to be started
		[ScriptLoadedEvent]
		public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
		{
			//Declare our variables
			m_horses = new RaceHorse[4];
			m_racingHorses = new RaceHorse[4];
			m_raceStarted = false;
			m_currentRanking = 0;

			//Now declare all our horses and their initial position
			m_horses[0] = new RaceHorse(504406, 436722, 6082);
			m_horses[0].Name = "Blacky";
			m_horses[0].Model = 450;
			m_horses[0].Size = 50;

			m_horses[1] = new RaceHorse(504499, 436679, 7484);
			m_horses[1].Name = "Speedy";
			m_horses[1].Model = 449;
			m_horses[1].Size = 40;

			m_horses[2] = new RaceHorse(504504, 436830, 6778);
			m_horses[2].Name = "Lucky";
			m_horses[2].Model = 448;
			m_horses[2].Size = 50;

			m_horses[3] = new RaceHorse(504416, 436618, 1568);
			m_horses[3].Name = "Binky";
			m_horses[3].Model = 447;
			m_horses[3].Size = 50;

			//Add all horses to the world
			bool good = true;
			if (!m_horses[0].AddToWorld()
				|| !m_horses[1].AddToWorld()
				|| !m_horses[2].AddToWorld()
				|| !m_horses[3].AddToWorld())
				good = false;

			//Declare the timer to check for the racestart
			m_raceStartTimer = new Timer(30000);
			m_raceStartTimer.Elapsed += new ElapsedEventHandler(RaceStartTest);
			m_raceStartTimer.AutoReset = true;
			m_raceStartTimer.Start();

			if (log.IsInfoEnabled)
				if (log.IsInfoEnabled)
					log.Info("HorseRace Event initialized: "+good);
		}

		//This function is implemented from the IGameEvent
		//interface and is called on when we want to stop 
		//an event
		[ScriptUnloadedEvent]
		public static void OnScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			//Stop the timer
			if (m_raceStartTimer != null)
				m_raceStartTimer.Close();

			if (m_horses == null)
				return;

			//If we have riders on our horse, dismount them
			for (int i = 0; i < m_horses.Length; i++)
				if (m_horses[i] != null)
				{
					foreach (GamePlayer player in m_horses[i].Riders)
					{
						player.DismountSteed(true);
					}
					m_horses[i].Delete();
				}
		}

		//Called every 30 seconds to test if we can start the race
		protected static void RaceStartTest(object sender, ElapsedEventArgs e)
		{
			//if the race has already started return.
			if (m_raceStarted)
				return;
			int wts = 0;
			int ws = 0;
			lock (m_horses)
			{
				//Get the number of horses on start and walking to start
				foreach (RaceHorse horse in m_horses)
					if (horse.HorseState == RaceHorse.RaceHorseState.WalkingToStart)
					{
						wts++;
					}
					else if (horse.HorseState == RaceHorse.RaceHorseState.WaitingForRaceStart)
					{
						ws++;
					}
				//If at least 2 horses are waiting to start and no
				//other horse is walking towards the start, then 
				//start the race!
				if (wts == 0 && ws > 1)
				{
					m_raceStarted = true;
					foreach (RaceHorse horse in m_horses)
						if (horse.HorseState == RaceHorse.RaceHorseState.WaitingForRaceStart)
							horse.StartRace();
				}

			}
		}

		//Returns true if the race is started
		public static bool IsRaceStarted()
		{
			return m_raceStarted;
		}

		//Registers a racing horse and returns the startnumber
		protected static int RegisterRacingHorse(RaceHorse horse)
		{
			lock (m_horses)
			{
				for (int i = 0; i < 4; i++)
					if (m_racingHorses[i] == null)
					{
						m_racingHorses[i] = horse;
						return i;
					}
				return -1;
			}
		}


		//Deregister a racing horse and frees the startnumber
		protected static int UnregisterRacingHorse(RaceHorse horse)
		{
			for (int i = 0; i < 4; i++)
				if (m_racingHorses[i] == horse)
				{
					m_racingHorses[i] = null;
					return i;
				}
			return -1;
		}

		//Called from a horse when it has finished the race!
		protected static void HorseFinished(RaceHorse horse)
		{
			lock (m_horses)
			{
				foreach (GamePlayer ply in horse.Riders)
				{
					//Dismount the rider
					ply.DismountSteed(true);
					//Unregister the horse from the race and free
					//the startnumber
					UnregisterRacingHorse(horse);
					//If a player was sitting on the horse
					//(could be that player /quit while on horse)
					if (ply != null)
					{
						//increase the ranking and output message
						m_currentRanking++;
						foreach (GamePlayer player in horse.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
							player.Out.SendMessage(
								ply.Name + " has finished the horse race and came in at " + m_currentRanking + ". position!",
								eChatType.CT_Broadcast,
								eChatLoc.CL_SystemWindow);
					}


					//Check if all horses are back in finish yet
					int count = 0;
					for (int i = 0; i < 4; i++)
					{
						if (m_horses[i].HorseState != RaceHorse.RaceHorseState.Racing)
							count++;
					}

					//If all horses have finished, reset the race
					if (count == 4)
					{
						m_raceStarted = false;
						m_currentRanking = 0;
					}
				}
			}
		}

		//This array holds the path the horses will race
		//in the form of x,y
		public static uint[,] racePoints = new uint[,]
			{
				{505424, 435868},
				{505744, 436584},
				{506069, 437313},
				{506394, 438041},
				{506679, 438705},
				{506890, 439433},
				{507198, 440165},
				{507578, 440865},
				{507980, 441450},
				{508490, 442042},
				{509027, 442623},
				{509522, 443142},
				{510073, 443702},
				{510632, 444262},
				{511201, 444821},
				{511777, 445368},
				{512373, 445896},
				{513009, 446372},
				{513649, 446849},
				{514295, 447317},
				{514940, 447784},
				{515587, 448253},
				{516234, 448721},
				{516802, 449132},
				{517467, 449568},
				{518136, 449956},
				{518831, 450294},
				{519558, 450554},
				{520330, 450728},
				{521122, 450719},
				{521901, 450552},
				{522634, 450238},
				{523355, 449900},
				{524076, 449561},
				{524710, 449264},
				{525436, 448946},
				{526155, 448605},
				{526799, 448327},
				{527508, 447986},
				{528206, 447646},
				{528900, 447310},
				{529604, 446983},
				{530318, 446658},
				{531045, 446327},
				{531771, 445997},
				{532483, 445638},
				{533128, 445179},
				{533728, 444652},
				{534285, 444085},
				{534751, 443562},
				{535260, 442952},
				{535756, 442333},
				{536201, 441674},
				{536643, 441017},
				{537072, 440351},
				{537423, 439640},
				{537677, 438984},
				{537891, 438222},
				{538033, 437439},
				{538117, 436649},
				{538143, 435855},
				{538104, 435062},
				{537976, 434279},
				{537806, 433503},
				{537546, 432753},
				{537096, 432103},
				{536518, 431566},
				{535822, 431171},
				{535106, 430822},
				{534361, 430543},
				{533588, 430360},
				{532791, 430285},
				{532001, 430221},
				{531208, 430157},
				{530417, 430068},
				{529723, 429970},
				{528931, 429858},
				{528243, 429728},
				{527487, 429479},
				{526758, 429159},
				{526030, 428838},
				{525299, 428517},
				{524569, 428196},
				{523840, 427876},
				{523110, 427555},
				{522379, 427234},
				{521654, 426915},
				{520902, 426651},
				{520136, 426450},
				{519364, 426261},
				{518587, 426082},
				{517798, 425980},
				{517009, 425884},
				{516221, 425787},
				{515430, 425691},
				{514639, 425594},
				{513849, 425498},
				{513058, 425430},
				{512271, 425492},
				{511488, 425634},
				{510723, 425855},
				{510019, 426222},
				{509365, 426675},
				{508720, 427145},
				{508079, 427612},
				{507434, 428083},
				{506790, 428554},
				{506149, 429021},
				{505508, 429489},
				{504867, 429957},
				{504245, 430451},
				{503750, 431062},
				{503676, 431849},
				{503865, 432620},
				{504143, 433361},
				{504419, 434093},
				{504692, 434820}
			};
	}
}