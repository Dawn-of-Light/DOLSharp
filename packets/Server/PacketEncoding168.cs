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

		public enum eEncryptionState
		{
			NotEncrypted = 0,
			RSAEncrypted = 1,
			PseudoRC4Encrypted = 2
		}

		protected eEncryptionState	m_encryptionState;
		protected byte[]						m_sbox = null;

		public PacketEncoding168()
		{
			m_encryptionState = eEncryptionState.NotEncrypted;
			m_sbox = new byte[256];
		}

		/// <summary>
		/// Gets or sets the SBox for this encoding
		/// </summary>
		public byte[] SBox
	  {
	    get { return m_sbox; }
			set { m_sbox = value; }
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
		public byte[] DecryptPacket(byte[] content, bool udpPacket)
		{
			/* No Cryptlib currently!
			if(m_encryptionState == eEncryptionState.RSAEncrypted)
			{
				byte[] output = new byte[content.Length];
				UInt32 outLen = CryptLib168.DecodeMythicRSAPacket(content,(UInt32)content.Length,output,(UInt32)output.Length);
				if(outLen==0)
				{
					if (log.IsErrorEnabled)
						log.Error("Failed to decrypt RSA packet!");
					return content;
				}
				byte[] newPacket = new byte[outLen];
				Array.Copy(output,0,newPacket,0,outLen);
				return newPacket;
			}
			if(m_encryptionState == eEncryptionState.PseudoRC4Encrypted && m_sbox != null)			
			{
			  CryptLib168.DecodeMythicRC4Packet(content,m_sbox);
				return content;
			}
			*/
			return content;
		}
		
		/// <summary>
		/// Encrypts a 1.68 packet
		/// </summary>
		/// <param name="content">the content to encrypt</param>
		/// <param name="udpPacket">true if the packet is an udp packet</param>
		/// <returns>the encrypted packet</returns>
		public byte[] EncryptPacket(byte[] content, bool udpPacket)
		{
			/* No Cryptlib currently!
			if(m_encryptionState == eEncryptionState.PseudoRC4Encrypted && m_sbox != null)			
			{
			  CryptLib168.EncodeMythicRC4Packet(content,m_sbox,udpPacket);
				return content;
			}
			*/
			return content;
		}
	}
}
