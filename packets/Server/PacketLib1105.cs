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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using DOL.Database;
using DOL.GS.RealmAbilities;
using log4net;

namespace DOL.GS.PacketHandler
{
	[PacketLib(1105, GameClient.eClientVersion.Version1105)]
	public class PacketLib1105 : PacketLib1104
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Constructs a new PacketLib for Client Version 1.105
		/// </summary>
		/// <param name="client">the gameclient this lib is associated with</param>
		public PacketLib1105(GameClient client)
			: base(client)
		{
		}
		
		public override void SendTrainerWindow()
		{
			// TODO: 1.105 Trainer window handling
			m_gameClient.Out.SendMessage("The new trainer window for client 1.105 has not been implemented yet.  To train use /trainline <LineName> <Amount> or roll back your client to an earlier version.", eChatType.CT_Staff, eChatLoc.CL_SystemWindow);
		}
	}
}
