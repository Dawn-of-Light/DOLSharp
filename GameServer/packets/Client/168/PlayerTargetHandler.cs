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
    /// Handles player target changes
    /// </summary>
    [PacketHandler(PacketHandlerType.TCP, eClientPackets.PlayerTarget, "Handle Player Target Change.", eClientStatus.PlayerInGame)]
    public class PlayerTargetHandler : IPacketHandler
    {
        /// <summary>
        /// Handles every received packet
        /// </summary>
        /// <param name="client">The client that sent the packet</param>
        /// <param name="packet">The received packet data</param>
        /// <returns></returns>
        public void HandlePacket(GameClient client, GSPacketIn packet)
        {
            ushort targetId = packet.ReadShort();
            ushort flags = packet.ReadShort();

            /*
             * 0x8000 = 'examine' bit
             * 0x4000 = LOS1 bit; is 0 if no LOS
             * 0x2000 = LOS2 bit; is 0 if no LOS
             * 0x0001 = players attack mode bit (not targets!)
             */

            new ChangeTargetAction(client.Player, targetId, (flags & (0x4000 | 0x2000)) != 0, (flags & 0x8000) != 0).Start(1);
        }

        /// <summary>
        /// Changes players target
        /// </summary>
        private class ChangeTargetAction : RegionAction
        {
            /// <summary>
            /// The 'examine target' bit
            /// </summary>
            private readonly bool _examineTarget;

            /// <summary>
            /// The new target OID
            /// </summary>
            private readonly int _newTargetId;

            /// <summary>
            /// The 'target in view' flag
            /// </summary>
            private readonly bool _targetInView;

            /// <summary>
            /// Constructs a new TargetChangeAction
            /// </summary>
            /// <param name="actionSource">The action source</param>
            /// <param name="newTargetId">The new target OID</param>
            /// <param name="targetInView">The target LOS bit</param>
            /// <param name="examineTarget">The 'examine target' bit</param>
            public ChangeTargetAction(GamePlayer actionSource, int newTargetId, bool targetInView, bool examineTarget)
                : base(actionSource)
            {
                _newTargetId = newTargetId;
                _targetInView = targetInView;
                _examineTarget = examineTarget;
            }

            /// <summary>
            /// Called on every timer tick
            /// </summary>
            protected override void OnTick()
            {
                var player = (GamePlayer)m_actionSource;

                GameObject myTarget = player.CurrentRegion.GetObject((ushort)_newTargetId);
                player.TargetObject = myTarget;
                player.TargetInView = _targetInView;

                if (myTarget != null)
                {
                    // Send target message text only if 'examine' bit is set.
                    if (_examineTarget)
                    {
                        foreach (string message in myTarget.GetExamineMessages(player))
                        {
                            player.Out.SendMessage(message, eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                        }
                    }

                    // Then no LOS message; not sure which bit to use so use both :)
                    // should be sent if targeted is using group panel to change the target
                    if (!_targetInView)
                    {
                        player.Out.SendMessage("Target is not in view.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }

                    if (!(myTarget is GamePlayer))
                    {
                        player.Out.SendObjectUpdate(myTarget);
                    }
                }

                if (player.IsPraying)
                {
                    if (!(myTarget is GameGravestone gravestone) || !gravestone.InternalID.Equals(player.InternalID))
                    {
                        player.Out.SendMessage("You are no longer targetting your grave. Your prayers fail.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        player.PrayTimerStop();
                    }
                }

                GameEventMgr.Notify(GamePlayerEvent.ChangeTarget, player, null);
            }
        }
    }
}