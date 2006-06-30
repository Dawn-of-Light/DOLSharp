// Copyright (C) 2004-2005 MySQL AB
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License version 2 as published by
// the Free Software Foundation
//
// There are special exceptions to the terms and conditions of the GPL 
// as it is applied to this software. View the full text of the 
// exception in file EXCEPTIONS in the directory of this software 
// distribution.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.IO;
using System.Security.Cryptography;
using MySql.Data.Common;

namespace MySql.Data.MySqlClient
{
	/// <summary>
	/// Summary description for Crypt.
	/// </summary>
	internal class Crypt
	{
		// private ctor to prevent creating a default one
		private Crypt()
		{
		}

/*		private void Create41Password( string password )
		{
			SHA1 sha = new SHA1CryptoServiceProvider(); 
			byte[] firstPassBytes = sha.ComputeHash( System.Text.Encoding.Default.GetBytes( password ));

			byte[] salt = packet.GetBuffer();
			byte[] input = new byte[ firstPassBytes.Length + 4 ];
			salt.CopyTo( input, 0 );
			firstPassBytes.CopyTo( input, 4 );
			byte[] outPass = new byte[100];
			byte[] secondPassBytes = sha.ComputeHash( input );

			byte[] cryptSalt = new byte[20];
			Security.ArrayCrypt( salt, 4, cryptSalt, 0, secondPassBytes, 20 );

			Security.ArrayCrypt( cryptSalt, 0, firstPassBytes, 0, firstPassBytes, 20 );

			// send the packet
			packet = CreatePacket(null);
			packet.Write( firstPassBytes, 0, 20 );
			SendPacket(packet);
		}
*/
		/// <summary>
		/// Simple XOR scramble
		/// </summary>
		/// <param name="from">Source array</param>
		/// <param name="fromIndex">Index inside source array</param>
		/// <param name="to">Destination array</param>
		/// <param name="toIndex">Index inside destination array</param>
		/// <param name="password">Password used to xor the bits</param>
		/// <param name="length">Number of bytes to scramble</param>
		static void XorScramble(byte[] from, int fromIndex, byte[] to, int toIndex, 
			byte[] password, int length) 
		{
			// make sure we were called properly
			if (fromIndex < 0 || fromIndex >= from.Length)
				throw new ArgumentException(Resources.GetString("IndexMustBeValid"), "fromIndex");
			if ((fromIndex + length) > from.Length)
				throw new ArgumentException(Resources.GetString("FromAndLengthTooBig"), "fromIndex" );
			if (from == null) 
				throw new ArgumentException(Resources.GetString("BufferCannotBeNull"), "from");
			if (to == null) 
				throw new ArgumentException(Resources.GetString("BufferCannotBeNull"), "to");
			if (toIndex < 0 || toIndex >= to.Length)
				throw new ArgumentException(Resources.GetString("IndexMustBeValid"), "toIndex" );
			if ((toIndex + length) > to.Length)
				throw new ArgumentException(Resources.GetString("IndexAndLengthTooBig"), "toIndex" );
			if (password == null || password.Length < length) 
				throw new ArgumentException(Resources.GetString("PasswordMustHaveLegalChars"), "password");
			if (length < 0) 
				throw new ArgumentException(Resources.GetString("ParameterCannotBeNegative"), "count");

			// now perform the work
			for (int i=0; i < length; i++)
				to[toIndex++] = (byte)(from[fromIndex++] ^ password[i] );
		}

		/// <summary>
		/// Generate a scrambled password for 4.1.0 using new passwords
		/// </summary>
		/// <param name="password">The password to scramble</param>
		/// <param name="seedBytes">The seedbytes used to scramble</param>
		/// <returns>Array of bytes containing the scrambled password</returns>
		public static byte[] Get410Password( string password, byte[] seedBytes )
		{
			SHA1 sha = new SHA1CryptoServiceProvider(); 

			// clean it and then digest it
			password = password.Replace(" ","").Replace("\t","");
			byte[] passBytes = System.Text.Encoding.Default.GetBytes( password );
			byte[] firstPass = sha.ComputeHash( passBytes );

			CryptoStream cs = new CryptoStream(Stream.Null, sha, CryptoStreamMode.Write);
			cs.Write( seedBytes, 0, 4 );
			cs.Write( firstPass, 0, 20 );
			cs.Close();
			byte[] secondPass = sha.Hash;

			byte[] scrambledBuff = new byte[20];
			XorScramble( seedBytes, 4, scrambledBuff, 0, secondPass, 20 );

			byte[] finalBuff = new byte[20];
			XorScramble( scrambledBuff, 0, finalBuff, 0, firstPass, 20 );

			return finalBuff;
		}

		/// <summary>
		/// Generates a proper hash for old style 4.1.0 passwords.  This would be used
		/// if a 4.1.0 server contained old 16 byte hashes.
		/// </summary>
		/// <param name="password">The password to hash</param>
		/// <param name="seedBytes">Seed bytes received from the server</param>
		/// <returns>Byte array containing the password hash</returns>
		public static byte[] GetOld410Password( string password, byte[] seedBytes )
		{
			long[] passwordHash = Hash(password);
			string passHex = String.Format(System.Globalization.CultureInfo.InvariantCulture, 
				"{0,8:X}{1,8:X}", passwordHash[0], passwordHash[1] );

			int[] salt = getSaltFromPassword(passHex);

			// compute binary password
			byte[] binaryPassword = new byte[20]; 
			int offset = 0;
			for (int i = 0; i < 2; i++) 
			{
				int val = salt[i];

				for (int t = 3; t >= 0; t--) 
				{
					binaryPassword[t + offset] = (byte) (val % 256);
					val >>= 8; /* Scroll 8 bits to get next part*/
				}

				offset += 4;
			}
			SHA1 sha = new SHA1CryptoServiceProvider(); 
			byte[] binaryHash = sha.ComputeHash( binaryPassword, 0, 8 );

			byte[] scrambledBuff = new byte[20];
			XorScramble( seedBytes, 4, scrambledBuff, 0, binaryHash, 20 );

			string scrambleString = System.Text.Encoding.Default.GetString( scrambledBuff ).Substring(0,8);

			long[] hashPass = Hash(password);
			long[] hashMessage = Hash(scrambleString);

			long max = 0x3FFFFFFFL;
			byte[] to = new byte[20];
			int msgPos = 0;
			int msgLength = scrambleString.Length;
			int toPos = 0;
			long seed1 = (hashPass[0] ^ hashMessage[0]) % max;
			long seed2 = (hashPass[1] ^ hashMessage[1]) % max;

			while (msgPos++ < msgLength) 
				to[toPos++] = (byte) (Math.Floor(rand(ref seed1, ref seed2, max) * 31) + 64);

			/* Make it harder to break */
			byte extra = (byte) (Math.Floor(rand(ref seed1, ref seed2, max) * 31));

			for (int i = 0; i < 8; i++) 
				to[i] ^= extra;

			return to;
		}

		/// <summary>
		/// Returns a byte array containing the proper encryption of the 
		/// given password/seed according to the new 4.1.1 authentication scheme.
		/// </summary>
		/// <param name="password"></param>
		/// <param name="seed"></param>
		/// <returns></returns>
		public static byte[] Get411Password( string password, string seed )
		{
			// if we have no password, then we just return 1 zero byte
			if (password.Length == 0) return new byte[1];

			SHA1 sha = new SHA1CryptoServiceProvider(); 

			byte[] firstHash = sha.ComputeHash( System.Text.Encoding.Default.GetBytes( password ) );
			byte[] secondHash = sha.ComputeHash( firstHash );
			byte[] seedBytes = System.Text.Encoding.Default.GetBytes( seed );

			CryptoStream cs = new CryptoStream(Stream.Null, sha, CryptoStreamMode.Write);
			cs.Write( seedBytes, 0, seedBytes.Length );
			cs.Write( secondHash, 0, secondHash.Length );
			cs.Close();

			byte[] finalHash = new byte[sha.Hash.Length + 1];
			finalHash[0] = 0x14;
			sha.Hash.CopyTo( finalHash, 1 );

			for (int i=1; i < finalHash.Length; i++)
				finalHash[i] = (byte)(finalHash[i] ^ firstHash[i-1]);
			return finalHash;
		}

		private static int[] getSaltFromPassword(String password) 
		{
			int[] result = new int[6];

			if (password == null || password.Length == 0)
				return result;

			int resultPos = 0; 
			int pos = 0;

			while (pos < password.Length)
			{
				int val = 0;

				for (int i = 0; i < 8; i++) 
					val = (val << 4) +  HexValue( password[pos++] );

				result[resultPos++] = val;
			}

			return result;
		}

		private static int HexValue( char c )
		{
			if (c >= 'A' && c <= 'Z') return (c - 'A') + 10;
			if (c >= 'a' && c <= 'z') return (c - 'a') + 10;
			return c - '0';
		}


		private static double rand(ref long seed1, ref long seed2, long max)
		{
			seed1 = (seed1 * 3) + seed2;
			seed1 %= max;
			seed2 = (seed1 + seed2 + 33) % max;
			return (seed1 / (double)max);
		}

		/// <summary>
		/// Encrypts a password using the MySql encryption scheme
		/// </summary>
		/// <param name="password">The password to encrypt</param>
		/// <param name="seed">The encryption seed the server gave us</param>
		/// <param name="new_ver">Indicates if we should use the old or new encryption scheme</param>
		/// <returns></returns>
		public static String EncryptPassword(String password, String seed, bool new_ver)
		{
			long max = 0x3fffffff;
			if (! new_ver)
				max = 0x01FFFFFF;
			if (password == null || password.Length == 0)
				return password;

			long[] hash_seed = Hash(seed);
			long[] hash_pass = Hash(password);

			long seed1 = (hash_seed[0]^hash_pass[0]) % max;
			long seed2 = (hash_seed[1]^hash_pass[1]) % max;
			if (! new_ver)
				seed2 = seed1 / 2;

			char[] scrambled = new char[seed.Length];
			for (int x=0; x < seed.Length; x++) 
			{
				double r = rand(ref seed1, ref seed2, max);
				scrambled[x] = (char)(Math.Floor(r*31) + 64);
			}

			if (new_ver)
			{						/* Make it harder to break */
				char extra = (char)Math.Floor( rand(ref seed1, ref seed2, max) * 31 );
				for (int x=0; x < scrambled.Length; x++)
					scrambled[x] ^= extra;
			}

			return new string(scrambled);
		}

		/// <summary>
		/// Hashes a password using the algorithm from Monty's code.
		/// The first element in the return is the result of the "old" hash.
		/// The second element is the rest of the "new" hash.
		/// </summary>
		/// <param name="P">Password to be hashed</param>
		/// <returns>Two element array containing the hashed values</returns>
		static long[] Hash(String P) 
		{
			long val1 = 1345345333;
			long val2 = 0x12345671;
			long inc  = 7;

			for (int i=0; i < P.Length; i++) 
			{
				if (P[i] == ' ' || P[i] == '\t') continue;
				long temp = (long)(0xff & P[i]);
				val1 ^= (((val1 & 63)+inc)*temp) + (val1 << 8);
				val2 += (val2 << 8) ^ val1;
				inc += temp;
			}

			long[] hash = new long[2];
			hash[0] = val1 & 0x7fffffff;
			hash[1] = val2 & 0x7fffffff;
			return hash;
		}

	}
}
