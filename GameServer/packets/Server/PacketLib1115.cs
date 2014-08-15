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
using DOL.GS.PlayerTitles;
using log4net;


namespace DOL.GS.PacketHandler
{
    [PacketLib(1115, GameClient.eClientVersion.Version1115)]
    public class PacketLib1115 : PacketLib1114
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Constructs a new PacketLib for Client Version 1.115
        /// </summary>
        /// <param name="client">the gameclient this lib is associated with</param>
        public PacketLib1115(GameClient client)
            : base(client)
        {

        }
        
        /// <summary>
        /// New Iem Packet Data in v1.115
        /// </summary>
        /// <param name="pak"></param>
        /// <param name="item"></param>
        protected override void WriteItemData(GSTCPPacketOut pak, InventoryItem item) {
        	
        	if (item == null)
			{
				pak.Fill(0x00, 23); // 1.115 +1 short in front of item Data
				return;
			}
        	
        	// FIXME: Unknown short, seems to have some importance in displaying "Bonus Level" in Tool Tip.
        	// More research needed, this doesn't break 1.115 Client...
        	pak.WriteShort(0);
        	
        	// TODO : Override for PacketLib 1.112 needed for this to work (See Darwin/Eden 1.112 implementation)
        	base.WriteItemData(pak, item);
        }
    }
}
