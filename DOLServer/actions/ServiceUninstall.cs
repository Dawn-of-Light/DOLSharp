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
using System.Configuration.Install;
using System.Reflection;

namespace DOL.DOLServer.Actions
{
	/// <summary>
	/// Handles service uninstall requests of the gameserver
	/// </summary>
	public class ServiceUninstall : IAction
	{
		/// <summary>
		/// returns the name of this action
		/// </summary>
		public string Name
		{
			get { return "--serviceuninstall"; }
		}

		/// <summary>
		/// returns the syntax of this action
		/// </summary>
		public string Syntax
		{
			get { return "--serviceuninstall"; }
		}

		/// <summary>
		/// returns the description of this action
		/// </summary>
		public string Description
		{
			get { return "Uninstalls the DOL system service"; }
		}

		public void OnAction(Hashtable parameters)
		{
			System.Configuration.Install.AssemblyInstaller asmInstaller = new AssemblyInstaller(Assembly.GetExecutingAssembly(), new string[1] {"/LogToConsole=false"});
			Hashtable rollback = new Hashtable();
			if (GameServerService.GetDOLService() == null)
			{
				Console.WriteLine("No service named \"DOL\" found!");
				return;
			}
			Console.WriteLine("Uninstalling DOL system service...");
			try
			{
				asmInstaller.Uninstall(rollback);
			}
			catch (Exception e)
			{
				Console.WriteLine("Error uninstalling system service");
				Console.WriteLine(e.Message);
				return;
			}
			Console.WriteLine("Finished!");
		}
	}
}