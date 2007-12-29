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
 * This is a really simple example of game events
 * This script creates a NPC (a spider) that stands 
 * around and when a player right-clicks on it will
 * run to the player and keep following this player
 * like a dog. The speed of the spider is based on
 * the distance to the player, the further away the
 * spider is, the faster it will run :-)
 */
using System;
using System.Reflection;
using System.Timers;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.GameEvents
{
	//First, declare our Event and have it implement the IGameEvent interface
	public class FollowingNPCEvent
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		//This is the NPC that will follow the player around
		public class FollowingNPC : GameNPC
		{
			//The NPC will remember what player it is following
			private GamePlayer m_playerToFollow;

			//Creates a new spider
			public FollowingNPC() : base()
			{
				//First, we set the position of this
				//npc in the constructor. You can set
				//the npc position, model etc. in the
				//StartEvent() method too if you want.
				X = 505599;
				Y = 437679;
				Z = 0;
				Heading = 0x0;
				Name = "Ugly Spider";
				GuildName = "Rightclick me";
				Model = 132;
				Size = 30;
				Level = 10;
				Realm = eRealm.Albion;
				CurrentRegionID = 1;

				//At the beginning, the spider isn't following anyone
				m_playerToFollow = null;

				//Now we set the timer callback to a function we defined
				//Inside this function we will check for player movement
				//and position changes. It will be called every 1,5 seconds
				m_myFollowTimer.Elapsed += new ElapsedEventHandler(CalculateWalkToSpot);
			}

			//This function will be called when some player 
			//right-clicks on the small spider
			public override bool Interact(GamePlayer player)
			{
				if (!base.Interact(player))
					return false;
				//If the same player rightclicks the mob, we ignore it
				if (m_playerToFollow == player)
					return false;
				//We set the player we will follow
				m_playerToFollow = player;
				//We send some nice message to the player we will follow
				player.Out.SendMessage(Name + " will follow you now!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
				//We make the mob say some stupid stuff :-)
				Say("Ha ha ha! I will follow " + player.Name + " now!!!!");
				return true;
			}

			//This function is a timercallback function that 
			//is called every 1,5 seconds
			protected void CalculateWalkToSpot(object sender, ElapsedEventArgs args)
			{
				//If we have no player to follow, we just return
				if (m_playerToFollow == null)
					return;
				//If the player has moved to another region, we stop following
				if (m_playerToFollow.CurrentRegionID != CurrentRegionID)
				{
					//Reset the player
					m_playerToFollow = null;
					//Stop our movement
					StopMoving();
				}

				//Calculate the difference between our position and the players position
				float diffx = (long) m_playerToFollow.X - X;
				float diffy = (long) m_playerToFollow.Y - Y;

				//Calculate the distance to the player
				float distance = (float) Math.Sqrt(diffx*diffx + diffy*diffy);

				//If the player walks away too far, then we will stop following
				if (distance > 3000)
				{
					//Reset the player
					m_playerToFollow = null;
					//Stop our movement
					StopMoving();
				}
				//If the distance to the player is less than or equal to 75 we return
				if (distance < 75)
					return;

				//Calculate the offset to the player we will be walking to
				//Our spot will be 50 coordinates from the player, so we
				//calculate how much x and how much y we need to subtract
				//from the player to get the right x and y to walk to 
				diffx = (diffx/distance)*50;
				diffy = (diffy/distance)*50;

				//Subtract the offset from the players position to get
				//our target position
				int newX = (int) (m_playerToFollow.X - diffx);
				int newY = (int) (m_playerToFollow.Y - diffy);

				//Our speed is based on the distance to the player
				//We will walk faster to the player if the player
				//is further away and slower if it is close
				ushort speed = (ushort) (distance/5);
				//But we have a base minimum speed
				if (speed < 50)
					speed = 50;

				//Make the mob walk to the new spot
				WalkTo(newX, newY, 0, speed);
			}
		}

		//The NPC will check with a Timer for player movement
		//changes. This timmer will make the NPC start moving
		//stop moving or change direction.
		private static Timer m_myFollowTimer;

		private static FollowingNPC m_npc;
		//This function is implemented from the IGameEvent
		//interface and is called on serverstart when the
		//events need to be started
		[ScriptLoadedEvent]
		public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
		{
			if (!ServerProperties.Properties.LOAD_EXAMPLES)
				return;
			m_myFollowTimer = new Timer(1500);
			m_myFollowTimer.AutoReset = true;
			m_myFollowTimer.Start();

			//Create an instance of the following spider
			m_npc = new FollowingNPC();
			//And add it to the world (the position and all
			//other relevant data is defined in the constructor
			//of the NPC
			bool good = m_npc.AddToWorld();
			if (log.IsInfoEnabled)
				if (log.IsInfoEnabled)
					log.Info("FollowingNPCEvent initialized");
		}

		//This function is implemented from the IGameEvent
		//interface and is called on when we want to stop 
		//an event
		[ScriptUnloadedEvent]
		public static void OnScriptUnload(DOLEvent e, object sender, EventArgs args)
		{
			if (!ServerProperties.Properties.LOAD_EXAMPLES)
				return;
			//To stop this event, we simply delete
			//(remove from world completly) the npc
			if (m_npc != null)
				m_npc.Delete();
			if (m_myFollowTimer != null)
			{
				m_myFollowTimer.Stop();
				m_myFollowTimer.Close();
				m_myFollowTimer = null;
			}
		}
	}
}