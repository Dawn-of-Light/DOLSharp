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
 * This script creates a NPC that stands around and
 * talks to you if you rightclick on it. This
 * demonstrates a way you can use to make npcs talk
 * to players.
 */
using System;
using System.Reflection;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.GameEvents
{
	//First, declare our Event and have it implement the IGameEvent interface
	public class TalkingNPCEvent
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		//For our event we need a special npc that
		//answers to the right click (interact) of players
		//and also answers to talk.
		public class TalkingNPC : GameNPC
		{
			public TalkingNPC() : base()
			{
				//First, we set the position of this
				//npc in the constructor. You can set
				//the npc position, model etc. in the
				//StartEvent() method too if you want.
				X = 505499;
				Y = 437679;
				Z = 0;
				Heading = 0x0;
				Name = "The talking NPC";
				GuildName = "Rightclick me";
				Model = 5;
				Size = 50;
				Level = 10;
				Realm = 1;
				CurrentRegionID = 1;
			}

			//This function is the callback function that is called when
			//a player right clicks on the npc
			public override bool Interact(GamePlayer player)
			{
				if (!base.Interact(player))
					return false;

				//Now we turn the npc into the direction of the person it is
				//speaking to.
				TurnTo(player.X, player.Y);

				//We send a message to player and make it appear in a popup
				//window. Text inside the [brackets] is clickable in popup
				//windows and will generate a &whis text command!
				player.Out.SendMessage(
					"Hello " + player.Name + " do you want to have a little [chat]?",
					eChatType.CT_System, eChatLoc.CL_PopupWindow);
				return true;
			}

			//This function is the callback function that is called when
			//someone whispers something to this mob!
			public override bool WhisperReceive(GameLiving source, string str)
			{
				if (!base.WhisperReceive(source, str))
					return false;

				//If the source is no player, we return false
				if (!(source is GamePlayer))
					return false;

				//We cast our source to a GamePlayer object
				GamePlayer t = (GamePlayer) source;

				//Now we turn the npc into the direction of the person it is
				//speaking to.
				TurnTo(t.X, t.Y);

				//We test what the player whispered to the npc and
				//send a reply. The Method SendReply used here is
				//defined later in this class ... read on
				switch (str)
				{
					case "chat":
						SendReply(t,
						          "Oh, that's nice!\n I can tell you more about " +
						          	"[DOL]. [DOL] is a really cool project and their " +
						          	"[scripting] is really powerful!");
						break;
					case "scripting":
						SendReply(t,
						          "Yeah! I myself am just a [script] actually! " +
						          	"Isn't that cool? It is so [easy] to use!");
						break;
					case "DOL":
						SendReply(t,
						          "DOL? Dawn Of Light, yes yes, this server is running " +
						          	"Dawn Of Light! I am a [script] on this server, the " +
						          	"scripting language is really powerful and [easy] to use!");
						break;
					case "script":
						SendReply(t,
						          "Yes, I am a script. If you look into the /scripts/cs/gameevents directory " +
						          	"of your DOL version, you should find a script called " +
						          	"\"TalkingNPC.cs\" Yes, this is me actually! Want to [chat] some more?");
						break;
					case "easy":
						SendReply(t,
						          "Scripting is easy to use. Take a look at my [script] to " +
						          	"get a clue how easy it is!");
						break;
					default:
						break;
				}
				return true;
			}

			//This function sends some text to a player and makes it appear
			//in a popup window. We just define it here so we can use it in
			//the WhisperToMe function instead of writing the long text
			//everytime we want to send some reply!
			private void SendReply(GamePlayer target, string msg)
			{
				target.Out.SendMessage(
					msg,
					eChatType.CT_System, eChatLoc.CL_PopupWindow);
			}
		}

		private static TalkingNPC m_npc;

		//This function is implemented from the IGameEvent
		//interface and is called on serverstart when the
		//events need to be started
		[ScriptLoadedEvent]
		public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
		{
			if (!ServerProperties.Properties.LOAD_EXAMPLES)
				return;
			//Here we create an instance of our talking NPC
			m_npc = new TalkingNPC();
			//And add it to the world (the position and all
			//other relevant data is defined in the constructor
			//of the NPC
			bool good = m_npc.AddToWorld();
			if (log.IsInfoEnabled)
				log.Info("TalkingNPCEvent initialized");
		}

		//This function is implemented from the IGameEvent
		//interface and is called on when we want to stop 
		//an event
		[ScriptUnloadedEvent]
		public static void OnScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			if (!ServerProperties.Properties.LOAD_EXAMPLES)
				return;
			//To stop this event, we simply delete
			//(remove from world completly) the npc
			if (m_npc != null)
				m_npc.Delete();
		}
	}
}