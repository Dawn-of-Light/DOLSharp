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
	[PacketHandlerAttribute(PacketHandlerType.TCP,0x0B,"Handles Enter/Leave house requests")]
	public class HouseEnterLeaveHandler : IPacketHandler
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			int pid = packet.ReadShort();
			int housenumber = packet.ReadShort();
			int enter = packet.ReadByte();

			//House house = HouseMgr.GetHouse(housenumber);
			//if (house==null) {  log.Info("house not found!"); return 1; }

			//new EnterLeaveHouseAction(client.Player, house, enter).Start(1);

			return 1;
		}

		/// <summary>
		/// Handles house enter/leave events
		/// </summary>
		/*protected class EnterLeaveHouseAction : RegionAction
		{
			/// <summary>
			/// The target house
			/// </summary>
			protected readonly House m_house;
			/// <summary>
			/// The enter house flag
			/// </summary>
			protected readonly int m_enter;

			private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

			/// <summary>
			/// Constructs a new EnterLeaveHouseAction
			/// </summary>
			/// <param name="actionSource">The actions source</param>
			/// <param name="house">The target house</param>
			/// <param name="enter">The enter house flag</param>
			public EnterLeaveHouseAction(GamePlayer actionSource, House house, int enter) : base(actionSource)
			{
				//if (house == null)
				//	throw new ArgumentNullException("house");
				m_house = house;
				m_enter = enter;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				GamePlayer player = (GamePlayer)m_actionSource;

				player.CurrentHouse = m_house; //we set even if its null!

				switch(m_enter)
				{
					case 0: player.LeaveHouse(); break;

					case 1:

						if (player.Region != m_house.Region) //no "beaming" any more.
						{ 
							player.Out.SendMessage("You are too far away to enter house "+m_house.HouseNumber+".",eChatType.CT_System,eChatLoc.CL_SystemWindow);
							return; 
						} 

						m_house.Enter(player);

						break;
				}
			}
		}*/
	}
}