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
	[PacketHandlerAttribute(PacketHandlerType.TCP, eClientPackets.PetWindow, "Handle Pet Window Command", eClientStatus.PlayerInGame)]
	public class PetWindowHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			var aggroState = (byte) packet.ReadByte(); // 1-Aggressive, 2-Deffensive, 3-Passive
			var walkState = (byte) packet.ReadByte(); // 1-Follow, 2-Stay, 3-GoTarg, 4-Here
			var command = (byte) packet.ReadByte(); // 1-Attack, 2-Release

			//[Ganrod] Nidel: Animist can removed his TurretFnF without MainPet.
			if (client.Player.TargetObject != null && command == 2 && client.Player.ControlledBrain == null &&
			    client.Player.CharacterClass.ID == (int) eCharacterClass.Animist)
			{
				var turret = client.Player.TargetObject as TurretPet;
				if (turret != null && turret.Brain is TurretFNFBrain && client.Player.IsControlledNPC(turret))
				{
					//release
					new HandlePetCommandAction(client.Player, 0, 0, 2).Start(1);
					return;
				}
			}

			//[Ganrod] Nidel: Call only if player has controllednpc
			if (client.Player.ControlledBrain != null)
			{
				new HandlePetCommandAction(client.Player, aggroState, walkState, command).Start(1);
				return;
			}
		}

		/// <summary>
		/// Handles pet command actions
		/// </summary>
		protected class HandlePetCommandAction : RegionAction
		{
			/// <summary>
			/// The pet aggro state
			/// </summary>
			protected readonly int _aggroState;

			/// <summary>
			/// The pet command
			/// </summary>
			protected readonly int _command;

			/// <summary>
			/// The pet walk state
			/// </summary>
			protected readonly int _walkState;

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
				_aggroState = aggroState;
				_walkState = walkState;
				_command = command;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				var player = (GamePlayer)m_actionSource;

				switch (_aggroState)
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
						Log.Warn($"unknown aggro state {_aggroState}, player={player.Name}  version={player.Client.Version}  client type={player.Client.ClientType}");
						break;
				}
				switch (_walkState)
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
						Log.Warn($"unknown walk state {_walkState}, player={player.Name}  version={player.Client.Version}  client type={player.Client.ClientType}");
						break;
				}
				switch (_command)
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
						Log.Warn($"unknown command state {_command}, player={player.Name}  version={player.Client.Version}  client type={player.Client.ClientType}");
						break;
				}
			}
		}
	}
}