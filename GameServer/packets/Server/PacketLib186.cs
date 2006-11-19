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
using log4net;
using DOL.GS.Quests;
using System.Reflection;

namespace DOL.GS.PacketHandler
{
	[PacketLib(186, GameClient.eClientVersion.Version186)]
	public class PacketLib186 : PacketLib185
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Constructs a new PacketLib for Version 1.86 clients
		/// </summary>
		/// <param name="client">the gameclient this lib is associated with</param>
		public PacketLib186(GameClient client)
			: base(client)
		{
		}

		/// <summary>
		/// The bow prepare animation
		/// </summary>
		public override int BowPrepare
		{
			get { return 16000; }
		}

		/// <summary>
		/// The bow shoot animation
		/// </summary>
		public override int BowShoot
		{
			get { return 16003; }
		}

		public override void SendCombatAnimation(GameObject attacker, GameObject defender, ushort weaponID, ushort shieldID, int style, byte stance, byte result, byte targetHealthPercent)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.CombatAnimation));
			if (attacker != null)
				pak.WriteShort((ushort)attacker.ObjectID);
			else
				pak.WriteShort(0x00);
			if (defender != null)
				pak.WriteShort((ushort)defender.ObjectID);
			else
				pak.WriteShort(0x00);
			pak.WriteShort(weaponID);
			pak.WriteShort(shieldID);
			pak.WriteShortLowEndian((ushort)style);
			pak.WriteByte(stance);
			pak.WriteByte(result);
			pak.WriteByte(targetHealthPercent);
			pak.WriteByte(0);//unk
			SendTCP(pak);
		}

	}
}
