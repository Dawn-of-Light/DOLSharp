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
using DOL.GS;

namespace DOL.Events
{
	/// <summary>
	/// Holds the arguments for the Whisper event of GameLivings
	/// </summary>
	public class WhisperEventArgs : SayEventArgs
	{
		private GameLiving target;
		/// <summary>
		/// Constructs a new WhisperEventArgs
		/// </summary>
		/// <param name="target">the target of the whisper</param>
		/// <param name="text">the text being whispered</param>
		public WhisperEventArgs(GameLiving target, string text) : base(text)
		{
			this.target = target;
		}

		/// <summary>
		/// Gets the target of the whisper
		/// </summary>
		public GameLiving Target
		{
			get { return target; }
		}
	}
}
