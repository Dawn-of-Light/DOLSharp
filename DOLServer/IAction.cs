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
using System.Collections;

namespace DOL.DOLServer
{
	/// <summary>
	/// Defines the interface for server actions
	/// </summary>
	public interface IAction
	{
		/// <summary>
		/// returns the name of this action
		/// </summary>
		string Name { get; }
		/// <summary>
		/// returns the syntax of this action
		/// </summary>
		string Syntax { get; }
		/// <summary>
		/// returns the description of this action
		/// </summary>
		string Description { get; }
		/// <summary>
		/// This method is called when the action should be
		/// executed
		/// </summary>
		/// <param name="parameters">The parsed command line parameters</param>
		void OnAction(Hashtable parameters);
	}
}