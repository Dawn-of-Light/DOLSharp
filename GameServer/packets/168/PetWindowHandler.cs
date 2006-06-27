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
using System;
using System.Reflection;
using DOL.GS;
using log4net;

namespace DOL.GS.PacketHandler.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP,0x8A,"Handles pet window commands")]
	public class PetWindowHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			if (client.Player == null) return 0;

			byte aggroState = (byte)packet.ReadByte(); 	// 1-Aggressive, 2-Deffensive, 3-Passive
			byte walkState = (byte)packet.ReadByte(); 	// 1-Follow, 2-Stay, 3-GoTarg, 4-Here
			byte command = (byte)packet.ReadByte();		// 1-Attack, 2-Release

//			packet.LogDump();
//			log.Debug(string.Format("PetWindowHandler: aggro={0} walk={1} command={2}", aggroState, walkState, command));

			new HandlePetCommandAction(client.Player, aggroState, walkState, command).Start(1);

			return 1;
		}

		/// <summary>
		/// Handles pet command actions
		/// </summary>
		protected class HandlePetCommandAction : RegionAction
		{
			/// <summary>
			/// The pet aggro state
			/// </summary>
			protected readonly int m_aggroState;
			/// <summary>
			/// The pet walk state
			/// </summary>
			protected readonly int m_walkState;
			/// <summary>
			/// The pet command
			/// </summary>
			protected readonly int m_command;

			/// <summary>
			/// Constructs a new HandlePetCommandAction
			/// </summary>
			/// <param name="actionSource">The action source</param>
			/// <param name="aggroState">The pet aggro state</param>
			/// <param name="walkState">The pet walk state</param>
			/// <param name="command">The pet command</param>
			public HandlePetCommandAction(GamePlayer actionSource, int aggroState, int walkState, int command) : base(actionSource)
			{
				m_aggroState = aggroState;
				m_walkState = walkState;
				m_command = command;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				GamePlayer player = (GamePlayer)m_actionSource;

				switch (m_aggroState)
				{
					case 0: break; // ignore
					case 1: player.CommandNpcAgressive(); break;
					case 2: player.CommandNpcDefensive(); break;
					case 3: player.CommandNpcPassive(); break;
					default:
						if (log.IsWarnEnabled)
							log.Warn("unknown aggro state "+m_aggroState+", player="+player.Name+"  version="+player.Client.Version+"  client type="+player.Client.ClientType);
						break;
				}
				switch (m_walkState)
				{
					case 0: break; // ignore
					case 1: player.CommandNpcFollow(); break;
					case 2: player.CommandNpcStay(); break;
					case 3: player.CommandNpcGoTarget(); break;
					case 4: player.CommandNpcComeHere(); break;
					default:
						if (log.IsWarnEnabled)
							log.Warn("unknown walk state "+m_walkState+", player="+player.Name+"  version="+player.Client.Version+"  client type="+player.Client.ClientType);
						break;
				}
				switch (m_command)
				{
					case 0: break; // ignore
					case 1: player.CommandNpcAttack(); break;
					case 2: player.CommandNpcRelease(); break;
					default:
						if (log.IsWarnEnabled)
							log.Warn("unknown command state "+m_command+", player="+player.Name+"  version="+player.Client.Version+"  client type="+player.Client.ClientType);
						break;
				}
			}
		}
	}
}
