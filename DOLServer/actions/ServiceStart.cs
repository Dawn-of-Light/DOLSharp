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
	/// Handles service start requests of the gameserver
	/// </summary>
	public class ServiceStart : IAction
	{
		/// <summary>
		/// returns the name of this action
		/// </summary>
		public string Name
		{
			get { return "--servicestart"; }
		}

		/// <summary>
		/// returns the syntax of this action
		/// </summary>
		public string Syntax
		{
			get { return "--servicestart"; }
		}

		/// <summary>
		/// returns the description of this action
		/// </summary>
		public string Description
		{
			get { return "Starts the DOL system service"; }
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
			if (svcc.Status != ServiceControllerStatus.Stopped)
			{
				Console.WriteLine("The DOL service is not stopped");
				return;
			}
			try
			{
				Console.WriteLine("Starting the DOL service...");
				svcc.Start();
				svcc.WaitForStatus(ServiceControllerStatus.StartPending, TimeSpan.FromSeconds(10));
				Console.WriteLine("Starting can take some time, please check the logfile for progress information!");
				Console.WriteLine("Finished!");
			}
			catch (InvalidOperationException e)
			{
				Console.WriteLine("Could not start the DOL service!");
				Console.WriteLine(e.Message);
			}
			catch (System.ServiceProcess.TimeoutException)
			{
				Console.WriteLine("Error starting the service, please check the logfile for further info!");
			}
		}
	}
}