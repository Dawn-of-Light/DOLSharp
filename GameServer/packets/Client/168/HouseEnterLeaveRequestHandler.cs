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
using DOL.GS.Housing;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandler(PacketHandlerType.TCP, eClientPackets.HouseEnterLeave, ClientStatus.PlayerInGame)]
	public class HouseEnterLeaveHandler : IPacketHandler
	{
		#region IPacketHandler Members

		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			int pid = packet.ReadShort();
			int housenumber = packet.ReadShort();
			int enter = packet.ReadByte();

			// house is null, return
			House house = HouseMgr.GetHouse(housenumber);
			if (house == null)
				return 1;

			new EnterLeaveHouseAction(client.Player, house, enter).Start(1);

			return 1;
		}

		#endregion

		#region Nested type: EnterLeaveHouseAction

		/// <summary>
		/// Handles house enter/leave events
		/// </summary>
		private class EnterLeaveHouseAction : RegionAction
		{
			/// <summary>
			/// The enter house flag
			/// </summary>
			private readonly int _enter;

			/// <summary>
			/// The target house
			/// </summary>
			private readonly House _house;

			/// <summary>
			/// Constructs a new EnterLeaveHouseAction
			/// </summary>
			/// <param name="actionSource">The actions source</param>
			/// <param name="house">The target house</param>
			/// <param name="enter">The enter house flag</param>
			public EnterLeaveHouseAction(GamePlayer actionSource, House house, int enter) : base(actionSource)
			{
				_house = house;
				_enter = enter;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				var player = (GamePlayer) m_actionSource;

				switch (_enter)
				{
					case 0:
						player.LeaveHouse();
						break;

					case 1:
						if (!player.IsWithinRadius(_house, WorldMgr.VISIBILITY_DISTANCE) || (player.CurrentRegionID != _house.RegionID))
						{
							ChatUtil.SendSystemMessage(player, string.Format("You are too far away to enter house {0}.", _house.HouseNumber));
							return;
						}

						// make sure player can enter
						if (_house.CanEnterHome(player))
						{
							player.CurrentHouse = _house;

							_house.Enter(player);
						}
						else
						{
							ChatUtil.SendSystemMessage(player, string.Format("You can't enter house {0}.", _house.HouseNumber));
							return;
						}

						break;
				}
			}
		}

		#endregion
	}
}