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
using DOL.AI.Brain;
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
			private GamePlayer m_playerToFollow = null;

			//Creates a new spider
			public FollowingNPC() : base()
			{
				//First, we set the position of this
				//npc in the constructor. You can set
				//the npc position, model etc. in the
				//StartEvent() method too if you want.
				Position = new Point(505599, 437679, 0);
				Heading = 0x0;
				Name = "Ugly Spider";
				GuildName = "Rightclick me";
				Model = 132;
				Size = 30;
				Level = 10;
				Realm = 1;
				Region = WorldMgr.GetRegion(1);
				StandardMobBrain newBrain = new StandardMobBrain();
				newBrain.Body = this;
				newBrain.AggroLevel = 0;
				newBrain.AggroRange = 0;
				OwnBrain = newBrain;
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

				Follow(m_playerToFollow, 150, 2000);
				return true;
			}
		}

		//This function is implemented from the IGameEvent
		//interface and is called on serverstart when the
		//events need to be started
		[ScriptLoadedEvent]
		public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
		{
			//Create an instance of the following spider
			FollowingNPC m_npc = new FollowingNPC();
			//And add it to the world (the position and all
			//other relevant data is defined in the constructor
			//of the NPC
			m_npc.AddToWorld();
			if (log.IsInfoEnabled)
				log.Info("FollowingNPCEvent initialized");
		}
	}
}