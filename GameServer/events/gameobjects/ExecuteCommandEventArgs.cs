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
using DOL.Database;
using DOL.GS;

namespace DOL.Events
{
	/// <summary>
	/// Holds the arguments for the Execute Command Event.
	/// </summary>
	public class ExecuteCommandEventArgs : EventArgs
	{

		private GamePlayer source;
		private ScriptMgr.GameCommand command;
		private string[] pars;

		/// <summary>
		/// Constructs a new ExecuteCommandEventArgs
		/// </summary>
		/// <param name="source">the source that executed the command</param>
		/// <param name="command">the command which was executed.</param>
		/// <param name="pars">the pars given!</param>
		public ExecuteCommandEventArgs(GamePlayer source, ScriptMgr.GameCommand command, string[] pars)
		{
			this.source = source;
			this.command = command;
			this.pars = pars;
		}

		/// <summary>
		/// Gets the GamePlayer source who executed the command.
		/// </summary>
		public GamePlayer Source
		{
			get { return source; }
		}
		
		/// <summary>
		/// Gets the Command which was executed by the source player !
		/// </summary>
		public ScriptMgr.GameCommand Command
		{
			get { return command; }
		}

		/// <summary>
		/// Gets the Command Parameters!
		/// </summary>
		public string[] Parameters
		{
			get { return pars; }
		}
	}
}
