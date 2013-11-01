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
using System.Threading.Tasks;

namespace DOL.GS.PacketHandler.Client.v168
{
	/// <summary>
	/// Handles player target changes
	/// </summary>
	[PacketHandler(PacketHandlerType.TCP, eClientPackets.PlayerTarget, ClientStatus.PlayerInGame)]
	public class PlayerTargetHandler : IPacketHandler
	{
		#region IPacketHandler Members

		/// <summary>
		/// Handles every received packet
		/// </summary>
		/// <param name="client">The client that sent the packet</param>
		/// <param name="packet">The received packet data</param>
		/// <returns></returns>
		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			ushort targetID = packet.ReadShort();
			ushort flags = packet.ReadShort();

			/*
			 * 0x8000 = 'examine' bit
			 * 0x4000 = LOS1 bit; is 0 if no LOS
			 * 0x2000 = LOS2 bit; is 0 if no LOS
			 * 0x0001 = players attack mode bit (not targets!)
			 */
			Task.Run(() => {
			         	
						GameObject myTarget = client.Player.CurrentRegion.GetObject(targetID);
						bool targetInView = (flags & (0x4000 | 0x2000)) != 0;
						bool examineTarget = (flags & 0x8000) != 0;
		
						client.Player.TargetObject = myTarget;
						client.Player.TargetInView = targetInView;
							
						if (myTarget != null)
						{
							// Send target message text only if 'examine' bit is set.
							if (examineTarget)
							{
								foreach (string message in myTarget.GetExamineMessages(client.Player))
								{
									client.Player.Out.SendMessage(message, eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
								}
							}
							
							// Then no LOS message; not sure which bit to use so use both :)
							// should be sent if targeted is using group panel to change the target					
							if (!targetInView)
							{
								client.Player.Out.SendMessage("Target is not in view.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
		
							client.Player.Out.SendObjectUpdate(myTarget);
							
						}
		
						if (client.Player.IsPraying)
						{
							GameGravestone gravestone = myTarget as GameGravestone;
							if (gravestone == null || !gravestone.InternalID.Equals(client.Player.InternalID))
							{
								client.Player.Out.SendMessage("You are no longer targetting your grave. Your prayers fail.", eChatType.CT_System,
								                       eChatLoc.CL_SystemWindow);
								client.Player.PrayTimerStop();
							}
						}
		
						GameEventMgr.Notify(GamePlayerEvent.ChangeTarget, client.Player, null);
						
						if(myTarget != null && ServerProperties.Properties.LOSMGR_ENABLE)
						{
							client.Player.CurrentRegion.LosCheckManager.UpdateCacheFromTargeting(client.Player, myTarget, targetInView);
								
						}
					} );
		
		}

		#endregion
	}
}