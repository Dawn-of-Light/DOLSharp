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
using DOL.GS.Utils;
using System.Linq;

namespace DOL.GS
{
	/// <summary>
	/// Generic purpose utility collection
	/// </summary>
	public static class Util
	{
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
					m_random = new Random((int)DateTime.Now.Ticks);
				}

				return m_random;
			}
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
			return RandomGen.Next(min, max + 1);
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
			return RandomGen.NextDouble();
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
			return chancePercent > RandomGen.NextDouble();
		}

		/// <summary>
		/// Parse a string in CSV mode: handle ';'
		/// In case of int value
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static List<string> SplitCSV (this string str, bool rangeCheck = false)
		{
			char primarySeparator = ';';
			char secondarySeparator = '-';
			
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
			return (string.IsNullOrEmpty(str) || str.ToLower() == "null" || zeroMeansEmpty?str.Trim()=="0":false);			
		}

		/// <summary>
		/// Gets the stacktrace of a thread
		/// </summary>
		/// <remarks>
		/// The use of the deprecated Suspend and Resume methods is necessary to get the StackTrace.
		/// Suspend/Resume are not being used for thread synchronization (very bad).
		/// It may be possible to get the StackTrace some other way, but this works for now
		/// So, the related warning is disabled
		/// </remarks>
		/// <param name="thread">Thread</param>
		/// <returns>The thread's stacktrace</returns>
		public static StackTrace GetThreadStack(Thread thread)
		{
			#pragma warning disable 0618
			thread.Suspend();
			StackTrace trace;

			try
			{
				trace = new StackTrace(thread, true);
			}
			finally
			{
				thread.Resume();
			}
			#pragma warning restore 0618
			
			return trace;
		}

		/// <summary>
		/// Formats the stacktrace
		/// </summary>
		/// <param name="trace">The stacktrace to format</param>
		/// <returns>The fromatted string of stacktrace object</returns>
		public static string FormatStackTrace(StackTrace trace)
		{
			var str = new StringBuilder(128);

			if (trace == null)
			{
				str.Append("(null)");
			}
			else
			{
				for (int i = 0; i < trace.FrameCount; i++)
				{
					StackFrame frame = trace.GetFrame(i);
					Type declType = frame.GetMethod().DeclaringType;
					str.Append("   at ")
						.Append(declType == null ? "(null)" : declType.FullName).Append('.')
						.Append(frame.GetMethod().Name).Append(" in ")
						.Append(frame.GetFileName())
						.Append("  line:").Append(frame.GetFileLineNumber())
						.Append(" col:").Append(frame.GetFileColumnNumber())
						.Append("\n");
				}
			}

			return str.ToString();
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

		/// <summary>
		/// [Ganrod] Nidel: Check if between two values are near with tolerance.
		/// </summary>
		/// <param name="valueToHave"></param>
		/// <param name="compareToCompare"></param>
		/// <param name="tolerance"></param>
		/// <returns></returns>
		public static bool IsNearValue(int valueToHave, int compareToCompare, ushort tolerance)
		{
			return FastMath.Abs(valueToHave - compareToCompare) <= FastMath.Abs(tolerance);
		}

		/// <summary>
		/// [Ganrod] Nidel: Check if between two distances are near with tolerance.
		/// </summary>
		/// <param name="xH">X coord value to have</param>
		/// <param name="yH">Y coord value to have</param>
		/// <param name="zH">Z coord value to have</param>
		/// <param name="xC">X coord value to compare</param>
		/// <param name="yC">Y coord value to compare</param>
		/// <param name="zC">Z coord value to compare</param>
		/// <param name="tolerance">Tolerance distance between two coords</param>
		/// <returns></returns>
		public static bool IsNearDistance(int xH, int yH, int zH, int xC, int yC, int zC, ushort tolerance)
		{
			return IsNearValue(xH, xC, tolerance) && IsNearValue(yH, yC, tolerance) && IsNearValue(zH, zC, tolerance);
		}

		public static void AddRange<T>(this IList<T> list, IList<T> addList)
		{
			foreach (T item in addList)
			{
				list.Add(item);
			}
		}
	}
}