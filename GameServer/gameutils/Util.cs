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
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace DOL.GS
{
	/// <summary>
	/// Generic purpose utility collection
	/// </summary>
	public sealed class Util
	{
		/// <summary>
		/// Holds the random number generator instance
		/// </summary>
		[ThreadStatic]
		private static Random m_random;

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
		/// Gets the random number generator
		/// </summary>
		public static Random RandomGen
		{
			get
			{
				Random rnd = m_random;
				if (rnd == null)
					m_random = rnd = new Random((int)DateTime.Now.Ticks);
				return rnd;
			}
		}

		/// <summary>
		/// Make a sentence, first letter uppercase and replace all parameters
		/// </summary>
		/// <param name="message"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static string MakeSentence(string message, params string[] args)
		{
			if (message == null || message.Length == 0) return message;
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
		public static bool IsEmpty(string str)
		{
			return (str == null || str.Length == 0 || str.ToLower() == "null");
		}

		/// <summary>
		/// Gets the stacktrace of a thread
		/// </summary>
		/// <param name="thread">Thread</param>
		/// <returns>The thread's stacktrace</returns>
		public static StackTrace GetThreadStack(Thread thread)
		{
			thread.Suspend();
			StackTrace trace = new StackTrace(thread, true);
			thread.Resume();
			return trace;
		}

		/// <summary>
		/// Formats the stacktrace
		/// </summary>
		/// <param name="trace">The stacktrace to format</param>
		/// <returns>The fromatted string of stacktrace object</returns>
		public static string FormatStackTrace(StackTrace trace)
		{
			StringBuilder str = new StringBuilder(128);
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
			StringBuilder str = new StringBuilder(10);
			long minutes = seconds / 60;
			if (minutes > 0)
			{
				str.Append(minutes)
					.Append(":")
					.Append((seconds - (minutes * 60)).ToString("D2"))
					.Append(" min");
			}
			else
				str.Append(seconds)
					.Append(" sec");
			return str.ToString();
		}
	}
}
