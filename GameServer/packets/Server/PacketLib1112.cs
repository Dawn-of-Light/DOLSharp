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
#define NOENCRYPTION
using System;
using System.Reflection;
using DOL.Database;
using System.Collections;
using System.Collections.Generic;
using log4net;


namespace DOL.GS.PacketHandler
{
    [PacketLib(1112, GameClient.eClientVersion.Version1112)]
    public class PacketLib1112 : PacketLib1111
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Constructs a new PacketLib for Client Version 1.112
        /// </summary>
        /// <param name="client">the gameclient this lib is associated with</param>
        public PacketLib1112(GameClient client)
            : base(client)
        {

        }
        
        /// <summary>
		/// This is used to build a server side "Position Object"
		/// Usually Position Packet Should only be relayed
		/// The only purpose of this method is refreshing postion when there is Lag
		/// </summary>
		/// <param name="player"></param>
		public override void SendPlayerForgedPosition(GamePlayer player)
		{
			using (GSUDPPacketOut pak = new GSUDPPacketOut(GetPacketCode(eServerPackets.PlayerPosition)))
			{
				// PID
				pak.WriteShort((ushort)player.ObjectID);
				
				// Write Speed
				if (player.Steed != null && player.Steed.ObjectState == GameObject.eObjectState.Active)
				{
					player.Heading = player.Steed.Heading;
					pak.WriteShort(0x1800);
				}
				else
				{
					short rSpeed = player.CurrentSpeed;
					if (player.IsIncapacitated)
						rSpeed = 0;
					
					ushort content;
					if (rSpeed < 0)
					{
						content = (ushort)((Math.Abs(rSpeed) > 511 ? 511 : Math.Abs(rSpeed)) + 0x200);
					}
					else
					{
						content = (ushort)(rSpeed > 511 ? 511 : rSpeed);
					}
					
					if (!player.IsAlive)
					{
						content += 5 << 10;
					}
					else
					{
						ushort state = 0;
						
						if (player.IsSwimming)
							state = 1;
						
						if (player.IsClimbing)
							state = 7;
						
						if (player.IsSitting)
							state = 4;
						
						content += (ushort)(state << 10);
					}
					
					content += (ushort)(player.IsStrafing ? 1 << 13 : 0 << 13);
					
					pak.WriteShort(content);
				}

				// Get Off Corrd
				int offX = player.X - player.CurrentZone.XOffset;
				int offY = player.Y - player.CurrentZone.YOffset;

				pak.WriteShort((ushort)player.Z);
				pak.WriteShort((ushort)offX);
				pak.WriteShort((ushort)offY);
				
				// Write Zone
				pak.WriteShort(player.CurrentZone.ZoneSkinID);

				// Copy Heading && Falling or Write Steed
				if (player.Steed != null && player.Steed.ObjectState == GameObject.eObjectState.Active)
				{
					pak.WriteShort((ushort)player.Steed.ObjectID);
					pak.WriteShort((ushort)player.Steed.RiderSlot(player));
				}
				else
				{
					// Set Player always on ground, this is an "anti lag" packet
					ushort contenthead = (ushort)(player.Heading + (true ? 0x1000 : 0));
					pak.WriteShort(contenthead);
					// No Fall Speed.
					pak.WriteShort(0);
				}
				
				// Write Flags
				byte flagcontent = 0;
			
				if (player.IsDiving)
				{
					flagcontent += 0x04;
				}
				
				if (player.IsWireframe)
				{
					flagcontent += 0x01;
				}
				
				if (player.IsStealthed)
				{
					flagcontent += 0x02;
				}
				
				if (player.IsTorchLighted)
				{
					flagcontent += 0x80;
				}
				
				pak.WriteByte(flagcontent);

				// Write health + Attack
				byte healthcontent = (byte)(player.HealthPercent + (player.AttackState ? 0x80 : 0));
			
				pak.WriteByte(healthcontent);
				
				// Send Remaining
				pak.WriteByte(player.ManaPercent);
				pak.WriteByte(player.EndurancePercent);
				pak.WriteByte((byte)(player.RPFlag ? 1 : 0));
				pak.WriteByte(0);

				SendUDP(pak);	
			}
			
			// Update Cache
			m_gameClient.GameObjectUpdateArray[new Tuple<ushort, ushort>(player.CurrentRegionID, (ushort)player.ObjectID)] = GameTimer.GetTickCount();
		}
    }
}
