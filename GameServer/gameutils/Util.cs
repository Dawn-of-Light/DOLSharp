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
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Security.Cryptography;
using System.Linq;
using log4net;
using System.Reflection;

using DOL.GS.Utils;
using Microsoft.Diagnostics.Runtime;
using DOL.GS.Geometry;

namespace DOL.GS
{
	/// <summary>
	/// Generic purpose utility collection
	/// </summary>
	public class Util
	{
		private static Util soleInstance = new Util();

		public static void LoadTestDouble(Util testDouble) { soleInstance = testDouble; }

		protected virtual double RandomDoubleImpl()
		{
			return RandomGen.NextDouble();
		}

        protected virtual int RandomImpl(int min, int max)
        {
            return RandomGen.Next(min, max + 1);
        }

		#region Random
		/// <summary>
		/// Holds the random number generator instance
		/// </summary>
		[ThreadStatic]
		private static Random m_random = null;
		
		/// <summary>
		/// Gets the random number generator
		/// </summary>
		public static Random RandomGen
		{
			get
			{
				if (m_random == null)
				{
					m_random = new Random();
				}

				return m_random;
			}
		}

		private static readonly RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();
		
		/// <summary>
		/// Get a Crypto Strength Random Int
		/// </summary>
		/// <returns></returns>
		public static int CryptoNextInt()
		{
		    byte[] buffer = new byte[4];
		
		    randomNumberGenerator.GetBytes(buffer);
		    return BitConverter.ToInt32(buffer, 0) & 0x7FFFFFFF; 
		}
		
		/// <summary>
		/// Generates a Crypto Strength random number between 0..max inclusive 0 AND exclusive max
		/// </summary>
		/// <param name="max"></param>
		/// <returns></returns>		
		public static int CryptoNextInt(int maxValue)
		{
			return CryptoNextInt(0, maxValue);
		}
		
		/// <summary>
		/// Generates a Crypto Strength random number between min..max inclusive min AND exclusive max
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>		
		public static int CryptoNextInt(int minValue, int maxValue)
		{
			if (minValue == maxValue)
				return minValue;
			
			if (minValue > maxValue)
			{
				int swap = minValue;
				minValue = maxValue;
				maxValue = swap;
			}
			
			long diff = maxValue - minValue;
			byte[] buffer = new byte[4];
			
			// to prevent endless loop
			int counter = 0;
			
			while (true)
			{
				counter++;
				randomNumberGenerator.GetBytes(buffer);
				uint rand = BitConverter.ToUInt32(buffer, 0);
				long max = (1 + (long)int.MaxValue);
				
				long remainder = max % diff;
				
				// very low chance of getting an endless loop
				if (rand < max - remainder || counter > 10)
				{
					return (int)(minValue + (rand % diff));
				}
			}
		}
		
		/// <summary>
		/// Generates a Crypto Strength random number between 0.0 and 1.0.
		/// </summary>
		/// <returns>
		/// A double-precision floating point number greater than
		/// or equal to 0.0, and less than 1.0.
		/// </returns>		
		public static double CryptoNextDouble()
		{
			byte[] buffer = new byte[4];
			randomNumberGenerator.GetBytes(buffer);
			uint rand = BitConverter.ToUInt32(buffer, 0);
			return rand / (1.0 + uint.MaxValue);
		}
		
		public static bool RandomBool()
		{
			return Random(1) == 0;
		}
		
		/// <summary>
		/// Generates a random number between 0..max inclusive 0 AND max
		/// </summary>
		/// <param name="max"></param>
		/// <returns></returns>
		public static int Random(int max)
		{
			return RandomGen.Next(max + 1);
		}

		/// <summary>
		/// Generates a random number between min..max inclusive min AND max
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public static int Random(int min, int max)
		{
            return soleInstance.RandomImpl(min, max);
		}

		/// <summary>
		/// Generates a random number between 0.0 and 1.0.
		/// </summary>
		/// <returns>
		/// A double-precision floating point number greater than
		/// or equal to 0.0, and less than 1.0.
		/// </returns>
		public static double RandomDouble()
		{
			return soleInstance.RandomDoubleImpl();
		}

		/// <summary>
		/// returns in chancePercent% cases true
		/// </summary>
		/// <param name="chancePercent">0 .. 100</param>
		/// <returns></returns>
		public static bool Chance(int chancePercent)
		{
			return chancePercent >= Random(1, 100);
		}

		/// <summary>
		/// returns in chancePercent% cases true
		/// </summary>
		/// <param name="chancePercent">0.0 .. 1.0</param>
		/// <returns></returns>
		public static bool ChanceDouble(double chancePercent)
		{
			return chancePercent > RandomDouble();
		}
		
		#endregion
		
		#region stringMethod
		const char primarySeparator = ';';
		const char secondarySeparator = '-';

		/// <summary>
		/// Parse a string in CSV mode with separator ';'
		/// </summary>
		/// <param name="str">the string to parse</param>
		/// <param name="rangeCheck">the ranges are burst and put into the list</param>
		/// <returns>a List of strings with the values parsed</returns>
		public static List<string> SplitCSV (string str, bool rangeCheck = false)
		{
			
			if (str==null) return null;
			
			// simple parsing on priSep
			var resultat = str.Split(new char[]{primarySeparator}, StringSplitOptions.RemoveEmptyEntries).ToList();
			if (!rangeCheck)
				return resultat;
			
			// advanced parsing with range handling
			List<string> advancedResultat = new List<string>();
			foreach(var currentResultat in resultat)
			{
				if (currentResultat.Contains('-'))
				{
					int from =0;
					int to =0;
					
					if (int.TryParse(currentResultat.Split(secondarySeparator)[0], out from) && int.TryParse(currentResultat.Split(secondarySeparator)[1], out to))
					{
						if (from > to)
						{
							int tmp = to;
							to = from;
							from = tmp;
						}
						
						for (int i=from; i<=to; i++)
							advancedResultat.Add(i.ToString());
					}
				}
				else
					advancedResultat.Add(currentResultat);
			}
			return advancedResultat;
		}
		
		/// <summary>
		/// Make a sentence, first letter uppercase and replace all parameters
		/// </summary>
		/// <param name="message"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static string MakeSentence(string message, params string[] args)
		{
			if (string.IsNullOrEmpty(message))
				return message;

			string res = string.Format(message, args);
			if (res.Length > 0 && char.IsLower(res[0]))
			{
				res = char.ToUpper(res[0]) + res.Substring(1);
			}

			return res;
		}

		/// <summary>
		/// Checks wether string is empty.
		/// empty means either null or ""
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static bool IsEmpty(string str, bool zeroMeansEmpty = false)
		{
			if (string.IsNullOrEmpty(str))
				return true;
			
			// various common db troubles
			string currentStr = str.ToLower();
			if (currentStr == "null" ||currentStr == "\r\n" || currentStr == "\n")
				return true;
			
			if (zeroMeansEmpty && currentStr.Trim() == "0")
				return true;

			return false;
		}
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
		/// <summary>
		/// Extract keyword from a sentence that is started by a specific word
		/// </summary>
		/// <param name="str"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static IList<string> ContainsKey(string str, string startKey, params string[] args)
		{
			if (str.Trim().ToLower().StartsWith(startKey.ToLower()))
			{
				List<string> results = new List<string>(args.Length+1);
				results.Add(startKey);
				
				if (args != null)
				{
					// reduce string
					string rem = str.Trim().Substring(startKey.Length).Trim().ToLower();
					
					// search for keyword
					foreach(string keyW in args)
					{
						string keyWord = keyW;
						
						int index = rem.IndexOf(keyWord.ToLower());
						
						// if found
						if (index != -1)
						{
							results.Add(keyWord);
							
							// remove all found keyword.
							rem = rem.Replace(keyWord.ToLower(), string.Empty).Trim();
						}
					}
				}
				
				return results;
				
			}
			else if (args != null)
			{
				// search for keywords at begining of text
				foreach(string keyW in args)
				{
					string keyWord = keyW;
					
					if (str.Trim().ToLower().StartsWith(keyWord.ToLower()))
					{
						List<string> result = new List<string>(1);
						result.Add(keyWord);
						return result;
					}
				}
			}
			
			return new List<string>();
		}
		#endregion

		public static string GetFormattedStackTraceFrom(Thread targetThread)
        {
			var sb = new StringBuilder();
			try
			{
				var dt = DataTarget.AttachToProcess(Process.GetCurrentProcess().Id, false);
				var rt = dt.ClrVersions.Single().CreateRuntime();
				ClrThread clrThread = null;
				foreach (var t in rt.Threads)
				{
					if (t.ManagedThreadId == targetThread.ManagedThreadId)
					{
						clrThread = t;
						break;
					}
				}
				foreach (var frame in clrThread.EnumerateStackTrace())
				{
					var method = frame.Method;
					if (method != null)
					{
						sb.AppendLine($"   at {method.Signature}");
					}
				}
			}
			catch (Exception e)
			{
				return e.StackTrace;
			}
			return sb.ToString();
		}

		public static string FormatTime(long seconds)
		{
			var str = new StringBuilder(10);

			long minutes = seconds/60;
			if (minutes > 0)
			{
				str.Append(minutes)
					.Append(":")
					.Append((seconds - (minutes*60)).ToString("D2"))
					.Append(" min");
			}
			else
				str.Append(seconds)
					.Append(" sec");

			return str.ToString();
		}

        [Obsolete("This is going to be removed.")]
		public static bool IsNearValue(int valueToHave, int compareToCompare, ushort tolerance)
		{
			return FastMath.Abs(valueToHave - compareToCompare) <= FastMath.Abs(tolerance);
		}

        [Obsolete("This is going to be removed.")]
		public static bool IsNearDistance(int xH, int yH, int zH, int xC, int yC, int zC, ushort tolerance)
		{
			return IsNearValue(xH, xC, tolerance) && IsNearValue(yH, yC, tolerance) && IsNearValue(zH, zC, tolerance);
		}
		
		#region Collection Utils

		/// <summary>
		/// Implementation of a List Shuffle for Generics.
		/// This can help for Loot Randomizing.
		/// </summary>
		/// <param name="list"></param>
		public static void Shuffle<T>(IList<T> list)  
		{  
		    int n = list.Count; 
		    while (n > 1)
		    {
				n--;
				int k = Random(n);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
		    }
		}
		
		/// <summary>
		/// Helper For List Appending.
		/// </summary>
		/// <param name="list"></param>
		/// <param name="addList"></param>
		public static void AddRange<T>(IList<T> list, IList<T> addList)
		{
			foreach (T item in addList)
			{
				list.Add(item);
			}
		}

		/// <summary>
		/// Foreach Helper
		/// </summary>
		/// <param name="array"></param>
		/// <param name="action"></param>
        public static void ForEach<T>(IEnumerable<T> array, Action<T> action)
        {
            foreach (var cur in array)
                action(cur);
        }
        
        #endregion
	}
}