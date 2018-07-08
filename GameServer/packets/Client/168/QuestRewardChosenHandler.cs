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

using DOL.Events;

namespace DOL.GS.PacketHandler.Client.v168
{
    /// <summary>
    /// Handler for quest reward dialog response.
    /// </summary>
    /// <author>Aredhel</author>
    [PacketHandler(PacketHandlerType.TCP, eClientPackets.QuestRewardChosen, "Quest Reward Choosing Handler", eClientStatus.PlayerInGame)]
    public class QuestRewardChosenHandler : IPacketHandler
    {
        public void HandlePacket(GameClient client, GSPacketIn packet)
        {
            var response = (byte)packet.ReadByte();
            if (response != 1) // confirm
            {
                return;
            }

            var countChosen = (byte)packet.ReadByte();

            var itemsChosen = new int[8];
            for (int i = 0; i < 8; ++i)
            {
                itemsChosen[i] = packet.ReadByte();
            }

            packet.ReadShort(); // unknown
            packet.ReadShort(); // unknown
            packet.ReadShort(); // unknown

            ushort questId = packet.ReadShort();
            ushort questGiverId = packet.ReadShort();

            new QuestRewardChosenAction(client.Player, countChosen, itemsChosen, questGiverId, questId).Start(1);
        }

        /// <summary>
        /// Send dialog response via Notify().
        /// </summary>
        private class QuestRewardChosenAction : RegionAction
        {
            private readonly int _countChosen;
            private readonly int[] _itemsChosen;
            private readonly int _questGiverId;
            private readonly int _questId;

            /// <summary>
            /// Constructs a new QuestRewardChosenAction.
            /// </summary>
            /// <param name="actionSource">The responding player,</param>
            /// <param name="countChosen">Number of items chosen from the dialog.</param>
            /// <param name="itemsChosen">List of items chosen from the dialog.</param>
            /// <param name="questGiverId">ID of the quest NPC.</param>
            /// <param name="questId">ID of the quest.</param>
            public QuestRewardChosenAction(GamePlayer actionSource, int countChosen, int[] itemsChosen, int questGiverId, int questId)
                : base(actionSource)
            {
                _countChosen = countChosen;
                _itemsChosen = itemsChosen;
                _questGiverId = questGiverId;
                _questId = questId;
            }

            /// <summary>
            /// Called on every timer tick
            /// </summary>
            protected override void OnTick()
            {
                var player = (GamePlayer)m_actionSource;

                player.Notify(GamePlayerEvent.QuestRewardChosen, player,
                              new QuestRewardChosenEventArgs(_questGiverId, _questId, _countChosen, _itemsChosen));
            }
        }
    }
}