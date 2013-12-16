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
using System.ServiceProcess;

namespace DOL.DOLServer.Actions
{
	/// <summary>
	/// Handles service stop requests of the gameserver
	/// </summary>
	public class ServiceStop : IAction
	{
		/// <summary>
		/// returns the name of this action
		/// </summary>
		public string Name
		{
			get { return "--servicestop"; }
		}

		/// <summary>
		/// returns the syntax of this action
		/// </summary>
		public string Syntax
		{
			get { return "--servicestop"; }
		}

		/// <summary>
		/// returns the description of this action
		/// </summary>
		public string Description
		{
			get { return "Stops the DOL system service"; }
		}

		public void OnAction(Hashtable parameters)
		{
			ServiceController svcc = GameServerService.GetDOLService();
			if (svcc == null)
			{
				Console.WriteLine("You have to install the service first!");
				return;
			}
			if (svcc.Status == ServiceControllerStatus.StartPending)
			{
				Console.WriteLine("Server is still starting, please check the logfile for progress information!");
				return;
			}
			if (svcc.Status != ServiceControllerStatus.Running)
			{
				Console.WriteLine("The DOL service is not running");
				return;
			}
			try
			{
				Console.WriteLine("Stopping the DOL service...");
				svcc.Stop();
				svcc.WaitForStatus(ServiceControllerStatus.Stopped);
				Console.WriteLine("Finished!");
			}
			catch (InvalidOperationException e)
			{
				Console.WriteLine("Could not stop the DOL service!");
				Console.WriteLine(e.Message);
				return;
			}
		}
	}
}