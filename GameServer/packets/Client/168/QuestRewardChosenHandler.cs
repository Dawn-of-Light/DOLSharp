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
using System.Collections;

using DOL.Events;
using DOL.GS.Keeps;
using DOL.GS.Housing;

namespace DOL.GS.PacketHandler.Client.v168
{
	/// <summary>
	/// Handler for quest reward dialog response.
	/// </summary>
	/// <author>Aredhel</author>
	[PacketHandler(PacketHandlerType.TCP, 0x40, "Quest reward chosen handler")]
	public class QuestRewardChosenHandler : IPacketHandler
	{
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			byte data1 = (byte)packet.ReadByte();	// unknown
			byte countChosen = (byte)packet.ReadByte();

			int[] itemsChosen = new int[8];
			for (int i = 0; i < 8; ++i)
				itemsChosen[i] = packet.ReadByte();

			ushort data2 = packet.ReadShort();	// unknown
			ushort data3 = packet.ReadShort();	// unknown
			ushort data4 = packet.ReadShort();	// unknown

			ushort questGiverID = packet.ReadShort();
			ushort questID = packet.ReadShort();

			new QuestRewardChosenAction(client.Player, countChosen, itemsChosen,
				questGiverID, questID)
				.Start(1);

			return 1;
		}

		/// <summary>
		/// Send dialog response via Notify().
		/// </summary>
		protected class QuestRewardChosenAction : RegionAction
		{
			private int m_countChosen;
			private int[] m_itemsChosen;
			private int m_questGiverID;
			private int m_questID;

			/// <summary>
			/// Constructs a new QuestRewardChosenAction.
			/// </summary>
			/// <param name="actionSource">The responding player,</param>
			/// <param name="countChosen">Number of items chosen from the dialog.</param>
			/// <param name="itemsChosen">List of items chosen from the dialog.</param>
			/// <param name="questGiverID">ID of the quest NPC.</param>
			/// <param name="questID">ID of the quest.</param>
			public QuestRewardChosenAction(GamePlayer actionSource, int countChosen, int[] itemsChosen,
				int questGiverID, int questID)
				: base(actionSource)
			{
				m_countChosen = countChosen;
				m_itemsChosen = itemsChosen;
				m_questGiverID = questGiverID;
				m_questID = questID;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				GamePlayer player = (GamePlayer)m_actionSource;

				player.Notify(GamePlayerEvent.QuestRewardChosen, player, 
					new QuestRewardChosenEventArgs(m_questGiverID, m_questID, m_countChosen,
						m_itemsChosen));
				return;
			}
		}
	}
}

