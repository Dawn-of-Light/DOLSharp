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
	public class PacketEncoding168 : IPacketEncoding
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected eEncryptionState m_encryptionState;
		protected byte[] _sbox = null;

		public PacketEncoding168()
		{
			m_encryptionState = eEncryptionState.NotEncrypted;
			_sbox = new byte[256];
		}

		/// <summary>
		/// Gets or sets the SBox for this encoding
		/// </summary>
		public byte[] SBox
		{
			get { return _sbox; }
			set { _sbox = value; }
		}

		/// <summary>
		/// Gets or sets the Encryption State of this encoding
		/// </summary>
		public eEncryptionState EncryptionState
		{
			get { return m_encryptionState; }
			set { m_encryptionState = value; }
		}

		/// <summary>
		/// Decrypts a 1.68 packet
		/// </summary>
		/// <param name="content">the content to be decrypted</param>
		/// <param name="udpPacket">true if the packet an udp packet</param>
		/// <returns>the decrypted packet</returns>
		public byte[] DecryptPacket(byte[] buf, bool udpPacket)
		{
			if (buf == null)
				return null;
			if (_sbox == null || m_encryptionState == eEncryptionState.NotEncrypted)
				return buf;
			byte[] tmpsbox = new byte[_sbox.Length];
			Array.Copy(_sbox, 0, tmpsbox, 0, _sbox.Length);
			byte i = 0;
			byte j = 0;
			ushort len = (ushort)((buf[0] << 8) | buf[1] + 10); //+10 byte for packet#,session,param,code,checksum
			int k;
			for (k = (len / 2) + 2; k < len + 2; k++)
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
			for (k = 2; k < (len / 2) + 2; k++)
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
			log.Debug($"Decrypted {buf.Length} bytes (udp: {udpPacket})");
			return buf;
		}

		/// <summary>
		/// Encrypts a 1.68 packet
		/// </summary>
		/// <param name="content">the content to encrypt</param>
		/// <param name="udpPacket">true if the packet is an udp packet</param>
		/// <returns>the encrypted packet</returns>
		public byte[] EncryptPacket(byte[] buf, bool udpPacket)
		{
			if (buf == null)
				return null;
			if (_sbox == null || m_encryptionState == eEncryptionState.NotEncrypted)
				return buf;
			byte[] tmpsbox = new byte[_sbox.Length];
			Array.Copy(_sbox, 0, tmpsbox, 0, _sbox.Length);
			byte i = 0;
			byte j = 0;
			ushort len = (ushort)((buf[0] << 8) | buf[1]);
			len += 1; // +1 byte for packet code
			if (udpPacket)
				len += 2; //+2 byte for packet-count

			int k;
			for (k = (len / 2) + 2; k < len + 2; k++)
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
			for (k = 2; k < (len / 2) + 2; k++)
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
			log.Debug($"Encrypted {buf.Length} bytes (udp: {udpPacket})");
			return buf;
		}
	}
}
