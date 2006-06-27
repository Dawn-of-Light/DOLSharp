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
using DOL.GS;

namespace DOL.Events
{
	/// <summary>
	/// Holds the arguments for the SayReceive event of GameLivings
	/// </summary>
	public class SayReceiveEventArgs : SourceEventArgs
	{
		
		private GameLiving target;
		private string text;

		/// <summary>
		/// Constructs a new SayReceiveEventArgs
		/// </summary>
		/// <param name="source">the source that is saying something</param>
		/// <param name="target">the target that listened to the say</param>
		/// <param name="text">the text being said</param>
		public SayReceiveEventArgs(GameLiving source, GameLiving target,  string text) : base(source)
		{			
			this.target = target;
			this.text = text;
		}		
		
		/// <summary>
		/// Gets the GameLiving target who listened to the say
		/// </summary>
		public GameLiving Target
		{
			get { return target; }
		}

		/// <summary>
		/// Gets the text being said
		/// </summary>
		public string Text
		{
			get { return text; }
		}
	}
}
