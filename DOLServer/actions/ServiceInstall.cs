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
using System.Text;

namespace DOL.DOLServer.Actions
{
	/// <summary>
	/// Handles service install requests of the gameserver
	/// </summary>
	public class ServiceInstall : IAction
	{
		/// <summary>
		/// returns the name of this action
		/// </summary>
		public string Name
		{
			get { return "--serviceinstall"; } 
		}

		/// <summary>
		/// returns the syntax of this action
		/// </summary>
		public string Syntax
		{
			get { return "--serviceinstall"; }
		}

		/// <summary>
		/// returns the description of this action
		/// </summary>
		public string Description
		{
			get { return "Installs DOL as system service with he given parameters"; }
		}

		public void OnAction(Hashtable parameters)
		{
			ArrayList temp = new ArrayList();
			temp.Add("/LogToConsole=false");
			StringBuilder tempString = new StringBuilder();
			foreach(DictionaryEntry entry in parameters)
			{
				if(tempString.Length > 0)
					tempString.Append(" ");
				tempString.Append(entry.Key);
				tempString.Append("=");
				tempString.Append(entry.Value);
			}
			temp.Add("commandline="+tempString.ToString());
			
			string[] commandLine = (string[]) temp.ToArray(typeof(string));

			System.Configuration.Install.AssemblyInstaller asmInstaller = new AssemblyInstaller(Assembly.GetExecutingAssembly(), commandLine);
			Hashtable rollback = new Hashtable();

			if (GameServerService.GetDOLService() != null)
			{
				Console.WriteLine("DOL service is already installed!");
				return;
			}

			Console.WriteLine("Installing DOL as system service...");
			try
			{
				asmInstaller.Install(rollback);
				asmInstaller.Commit(rollback);
			}
			catch (Exception e)
			{
				asmInstaller.Rollback(rollback);
				Console.WriteLine("Error installing as system service");
				Console.WriteLine(e.Message);
				return;
			}
			Console.WriteLine("Finished!");
		}
	}
}