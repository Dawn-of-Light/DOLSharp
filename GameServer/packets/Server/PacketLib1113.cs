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
    [PacketLib(1113, GameClient.eClientVersion.Version1113)]
    public class PacketLib1113 : PacketLib1112
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Constructs a new PacketLib for Client Version 1.112
        /// </summary>
        /// <param name="client">the gameclient this lib is associated with</param>
        public PacketLib1113(GameClient client)
            : base(client)
        {

        }

		public override void SendPlayerTitles()
		{
			IList titles = m_gameClient.Player.Titles;
			
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.DetailWindow)))
			{

				pak.WriteByte(1); // new in 1.75
				pak.WriteByte(0); // new in 1.81
				pak.WritePascalString("Player Statistics"); //window caption
	
				byte line = 1;
				foreach (string str in m_gameClient.Player.FormatStatistics())
				{
					pak.WriteByte(line++);
					pak.WritePascalString(str);
				}
	
				pak.WriteByte(200);
				long titlesCountPos = pak.Position;
				pak.WriteByte((byte)titles.Count);
				line = 0;
				
				foreach (IPlayerTitle title in titles)
				{
					pak.WriteByte(line++);
					pak.WritePascalString(title.GetDescription(m_gameClient.Player));
				}
				
				long titlesLen = (pak.Position - titlesCountPos - 1); // include titles count
				if (titlesLen > byte.MaxValue)
					log.WarnFormat("Titles block is too long! {0} (player: {1})", titlesLen, m_gameClient.Player);
				
				//Trailing Zero!
				pak.WriteByte(0);
				SendTCP(pak);
			}
		}
    }
}
