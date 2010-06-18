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
using System.Reflection;
using DOL.AI.Brain;
using log4net;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandler(PacketHandlerType.TCP, eClientPackets.PetWindow, ClientStatus.PlayerInGame)]
	public class PetWindowHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region IPacketHandler Members

		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			var aggroState = (byte) packet.ReadByte(); // 1-Aggressive, 2-Deffensive, 3-Passive
			var walkState = (byte) packet.ReadByte(); // 1-Follow, 2-Stay, 3-GoTarg, 4-Here
			var command = (byte) packet.ReadByte(); // 1-Attack, 2-Release

			//[Ganrod] Nidel: Animist can removed his TurretFnF without MainPet.
			if (client.Player.TargetObject != null && command == 2 && client.Player.ControlledNpcBrain == null &&
			    client.Player.CharacterClass.ID == (int) eCharacterClass.Animist)
			{
				var turret = client.Player.TargetObject as TurretPet;
				if (turret != null && turret.Brain is TurretFNFBrain && client.Player.IsControlledNPC(turret))
				{
					//release
					new HandlePetCommandAction(client.Player, 0, 0, 2).Start(1);
					return 1;
				}
			}

			//[Ganrod] Nidel: Call only if player has controllednpc
			if (client.Player.ControlledNpcBrain != null)
			{
				new HandlePetCommandAction(client.Player, aggroState, walkState, command).Start(1);
				return 1;
			}

			return 0;
		}

		#endregion

		#region Nested type: HandlePetCommandAction

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
			/// The pet command
			/// </summary>
			protected readonly int m_command;

			/// <summary>
			/// The pet walk state
			/// </summary>
			protected readonly int m_walkState;

			/// <summary>
			/// Constructs a new HandlePetCommandAction
			/// </summary>
			/// <param name="actionSource">The action source</param>
			/// <param name="aggroState">The pet aggro state</param>
			/// <param name="walkState">The pet walk state</param>
			/// <param name="command">The pet command</param>
			public HandlePetCommandAction(GamePlayer actionSource, int aggroState, int walkState, int command)
				: base(actionSource)
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
				var player = (GamePlayer) m_actionSource;

				switch (m_aggroState)
				{
					case 0:
						break; // ignore
					case 1:
						player.CommandNpcAgressive();
						break;
					case 2:
						player.CommandNpcDefensive();
						break;
					case 3:
						player.CommandNpcPassive();
						break;
					default:
						if (Log.IsWarnEnabled)
							Log.Warn("unknown aggro state " + m_aggroState + ", player=" + player.Name + "  version=" + player.Client.Version +
							         "  client type=" + player.Client.ClientType);
						break;
				}
				switch (m_walkState)
				{
					case 0:
						break; // ignore
					case 1:
						player.CommandNpcFollow();
						break;
					case 2:
						player.CommandNpcStay();
						break;
					case 3:
						player.CommandNpcGoTarget();
						break;
					case 4:
						player.CommandNpcComeHere();
						break;
					default:
						if (Log.IsWarnEnabled)
							Log.Warn("unknown walk state " + m_walkState + ", player=" + player.Name + "  version=" + player.Client.Version +
							         "  client type=" + player.Client.ClientType);
						break;
				}
				switch (m_command)
				{
					case 0:
						break; // ignore
					case 1:
						player.CommandNpcAttack();
						break;
					case 2:
						player.CommandNpcRelease();
						break;
					default:
						if (Log.IsWarnEnabled)
							Log.Warn("unknown command state " + m_command + ", player=" + player.Name + "  version=" + player.Client.Version +
							         "  client type=" + player.Client.ClientType);
						break;
				}
			}
		}

		#endregion
	}
}