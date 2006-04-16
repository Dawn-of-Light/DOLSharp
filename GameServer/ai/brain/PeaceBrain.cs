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
using System.Collections;
using System.Reflection;
using System.Text;
using DOL.Events;
using DOL.GS;
using log4net;

namespace DOL.AI
{
	/// <summary>
	/// This is a brain class for all peace npc like merchand, trainer ect ...
	/// </summary>
	public class PeaceBrain : ABrain
	{
		/// <summary>
		/// PeaceBrain can't attack / be attacked and it does not need to think
		/// </summary>
		/// <returns>true if started</returns>
		public override bool Start()
		{
			return false;
		}

		/// <summary>
		/// Receives all messages of the body
		/// </summary>
		/// <param name="e">The event received</param>
		/// <param name="sender">The event sender</param>
		/// <param name="args">The event arguments</param>
		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
		}

		/// <summary>
		/// This method is called whenever the brain does some thinking
		/// </summary>
		public override void Think()
		{
		}
	}
}
