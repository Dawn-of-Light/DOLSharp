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
using System.Runtime.InteropServices;

namespace DOL.GS
{
	public class CryptLib168
	{
		[DllImport("CryptLib168.dll", CharSet=CharSet.Ansi)]
		public static extern bool GenerateRSAKey();

		[DllImport("CryptLib168.dll", CharSet=CharSet.Ansi)]
		public static extern bool ImportRSAKey(Byte[] externalKey, UInt32 keyLen);

		[DllImport("CryptLib168.dll", CharSet=CharSet.Ansi)]
		public static extern UInt32 ExportRSAKey(Byte[] key, UInt32 maxKeySize, bool withPrivateKey);

		[DllImport("CryptLib168.dll", CharSet=CharSet.Ansi)]
		public static extern UInt32 EncodeMythicRSAPacket(Byte[] inMessage, UInt32 inMessageLen, Byte[] outMessage, UInt32 outMessageLen);

		[DllImport("CryptLib168.dll", CharSet=CharSet.Ansi)]
		public static extern UInt32 DecodeMythicRSAPacket(Byte[] inMessage, UInt32 inMessageLen, Byte[] outMessage, UInt32 outMessageLen);
		
		[DllImport("CryptLib168.dll", CharSet=CharSet.Ansi)]
		public static extern void EncodeMythicRC4Packet(Byte[] packet, Byte[] sbox, bool udpPacket);

		[DllImport("CryptLib168.dll", CharSet=CharSet.Ansi)]
		public static extern void DecodeMythicRC4Packet(Byte[] packet, Byte[] sbox);
	}
 
}

