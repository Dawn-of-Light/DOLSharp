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
using System.Reflection;
using log4net;

namespace DOL.GS.PacketHandler
{

	/// <summary>
	/// Handles the encoding and decoding of Mythic packets for 1.68
	/// </summary>
	public class PacketEncoding1110 : IPacketEncoding
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected byte[] _sbox;
		protected byte[] _commonKey;

		public PacketEncoding1110()
		{
			EncryptionState = eEncryptionState.NotEncrypted;
			_sbox = new byte[256];
		}

		/// <summary>
		/// Gets or sets the SBox for this encoding
		/// </summary>
		public byte[] SBox
		{
			get { return _sbox; }
			set
			{
				_commonKey = value;
				_sbox = GenerateSBox(value);
			}
		}

		/// <summary>
		/// Gets or sets the Encryption State of this encoding
		/// </summary>
		public eEncryptionState EncryptionState { get; set; }

		/// <summary>
		/// Decrypts a 1.68 packet
		/// </summary>
		/// <param name="content">the content to be decrypted</param>
		/// <param name="udpPacket">true if the packet an udp packet</param>
		/// <returns>the decrypted packet</returns>
		public byte[] DecryptPacket(byte[] buf, int offset, bool udpPacket)
		{
			if (buf == null)
				return null;
			if (_sbox == null || EncryptionState == eEncryptionState.NotEncrypted)
				return buf;
			byte[] tmpsbox = new byte[_sbox.Length];
			Array.Copy(_sbox, 0, tmpsbox, 0, _sbox.Length);
			byte i = 0;
			byte j = 0;
			ushort len = (ushort)((buf[offset] << 8) | buf[offset + 1] + 10); //+10 byte for packet#,session,param,code,checksum
			offset += 2;
			int k;
			for (k = (len / 2) + offset; k < len + offset; k++)
			{
				i++;
				byte tmp = tmpsbox[i];
				j += tmp;
				tmpsbox[i] = tmpsbox[j];
				tmpsbox[j] = tmp;
				byte xorKey = tmpsbox[(byte)(tmpsbox[i] + tmpsbox[j])];
				buf[k] ^= xorKey;
				j += buf[k];
			}
			for (k = offset; k < (len / 2) + offset; k++)
			{
				i++;
				byte tmp = tmpsbox[i];
				j += tmp;
				tmpsbox[i] = tmpsbox[j];
				tmpsbox[j] = tmp;
				byte xorKey = tmpsbox[(byte)(tmpsbox[i] + tmpsbox[j])];
				buf[k] ^= xorKey;
				j += buf[k];
			}
			log.Debug($"Decrypted {len}/{buf.Length} bytes (udp: {udpPacket})");
			return buf;
		}

		/// <summary>
		/// Encrypts a 1.68 packet
		/// </summary>
		/// <param name="content">the content to encrypt</param>
		/// <param name="udpPacket">true if the packet is an udp packet</param>
		/// <returns>the encrypted packet</returns>
		public byte[] EncryptPacket(byte[] buf, int offset, bool udpPacket)
		{
			if (buf == null)
				return null;
			if (_sbox == null || EncryptionState == eEncryptionState.NotEncrypted)
				return buf;
			byte[] tmpsbox = new byte[_sbox.Length];
			Array.Copy(_sbox, 0, tmpsbox, 0, _sbox.Length);
			byte i = 0;
			byte j = 0;
			ushort len = (ushort)((buf[offset] << 8) | buf[offset + 1]);
			offset += 2;
			len += 1; // +1 byte for packet code
			if (udpPacket)
				len += 2; //+2 byte for packet-count

			int k;
			for (k = (len / 2) + offset; k < len + offset; k++)
			{
				i++;
				byte tmp = tmpsbox[i];
				j += tmp;
				tmpsbox[i] = tmpsbox[j];
				tmpsbox[j] = tmp;
				byte xorKey = tmpsbox[(byte)(tmpsbox[i] + tmpsbox[j])];
				j += buf[k];
				buf[k] ^= xorKey;
			}
			for (k = offset; k < (len / 2) + offset; k++)
			{
				i++;
				byte tmp = tmpsbox[i];
				j += tmp;
				tmpsbox[i] = tmpsbox[j];
				tmpsbox[j] = tmp;
				byte xorKey = tmpsbox[(byte)(tmpsbox[i] + tmpsbox[j])];
				j += buf[k];
				buf[k] ^= xorKey;
			}
			log.Debug($"Encrypted {len}/{buf.Length} bytes (udp: {udpPacket})");
			return buf;
		}

		/// <summary>
		///   Generates the SBox from the common key
		/// </summary>
		private static byte[] GenerateSBox(byte[] commonKey) {
			// Generate the S-Box from the common key (Code borrowed from DAoC Logger)
			var sbox = new byte[256];
			int x, y;
			for (x = 0; x < sbox.Length; x++) {
				sbox[x] = (byte) x;
			}

			for (x = y = 0; x < 256; x++) {
				y = (y + sbox[x] + commonKey[x % commonKey.Length]) & 255;
				var tmp = sbox[x];
				sbox[x] = sbox[y];
				sbox[y] = tmp;
			}

			// Dump the S-Box
			//if (_log.IsDebugEnabled) {
			//    _log.Debug(Marshal.ToHexDump("Common-Key", commonKey));
			//    _log.Debug(Marshal.ToHexDump("S-Box", sbox));
			//}

			return sbox;
		}
	}
}
