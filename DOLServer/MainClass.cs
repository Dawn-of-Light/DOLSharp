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
using System.IO;
using System.Threading;
using DOL.DOLServer.Actions;

namespace DOL.DOLServer
{
	/// <summary>
	/// Just a test to run DOL in console mode
	/// </summary>
	internal class MainClass
	{
		/// <summary>
		/// Holds all the possible actions
		/// </summary>
		private static ArrayList m_actions = new ArrayList();

		/// <summary>
		/// Registers a possible action
		/// </summary>
		/// <param name="action">The action to register</param>
		private static void RegisterAction(IAction action)
		{
			if(action == null)
				throw new ArgumentException("Action can't be null!","action");

			m_actions.Add(action);
		}

		/// <summary>
		/// Displays the syntax for this exectuable
		/// </summary>
		private static void ShowSyntax()
		{
			Console.WriteLine("Syntax: DOLServer.exe {action} [param1=value1] [param2=value2] ...");
			Console.WriteLine("Possible actions:");
			foreach (IAction action in m_actions)
			{
				if (action.Syntax != null && action.Description != null)
					Console.WriteLine(String.Format("{0,-20}\t{1}", action.Syntax, action.Description));
			}
		}

		/// <summary>
		/// Searches and returns an action based on the name
		/// </summary>
		/// <param name="name">the action name</param>
		/// <returns>the action class</returns>
		private static IAction GetAction(string name)
		{
			foreach (IAction action in m_actions)
			{
				if (action.Name.Equals(name))
					return action;
			}
			return null;
		}

		/// <summary>
		/// Parses the commandline parameters
		/// </summary>
		/// <param name="args">The commandline arguments</param>
		/// <param name="actionName">The action name from the commandline</param>
		/// <param name="parameters">A hashtable with all parameters and their values</param>
		private static void ParseParameters(string[] args, out string actionName, out Hashtable parameters)
		{
			parameters = new Hashtable();
			actionName = null;

			if(!args[0].StartsWith("--"))
				throw new ArgumentException("First argument must be the action");

			actionName = args[0];
			if(args.Length == 1) return;

			for(int i = 1; i < args.Length; i++)
			{
				string arg = args[i];
				if(arg.StartsWith("--"))
				{
						throw new ArgumentException("At least two actions given and only one action allowed!");				
				}
				else if(arg.StartsWith("-"))
				{
					int valueIdx = arg.IndexOf('=');
					if(valueIdx==-1)
					{
						parameters.Add(arg, "");
						continue;
					}
					string argName = arg.Substring(0,valueIdx);
					
					string argValue = "";
					if(valueIdx+1 < arg.Length)
						argValue = arg.Substring(valueIdx+1);

					parameters.Add(argName, argValue);
				}
			}
		}

		/// <summary>
		/// Registers all possible actions
		/// </summary>
		/// <remarks>
		/// The order in which the actions are registered
		/// is the same order in which they are displayed
		/// on syntax output
		/// </remarks>
		private static void RegisterActions()
		{
			//Important action, used on service start internally
			//!!!DO NOT REMOVE!!!
			RegisterAction(new ServiceRun());
			
			//Start server in console mode
			RegisterAction(new ConsoleStart());
			//Install service action
			RegisterAction(new ServiceInstall());
			//Uninstall service action
			RegisterAction(new ServiceUninstall());
			//Start service action
			RegisterAction(new ServiceStart());
			//Stop service action
			RegisterAction(new ServiceStop());
		}

		/// <summary>
		/// The main entry into the application
		/// </summary>
		[STAThread]
		private static void Main(string[] args)
		{
		    //VaNaTiC->
			//Append the lib path to the search path for assemblies!
			#warning VaNaTiC: redirected only to ONE path, further notice is needed!
			//AppDomain.CurrentDomain.AppendPrivatePath("."+Path.DirectorySeparatorChar+"lib");
			AppDomain.CurrentDomain.SetupInformation.PrivateBinPath = "."+Path.DirectorySeparatorChar+"lib";
			//VaNaTiC<-
			Thread.CurrentThread.Name = "MAIN";
				
			RegisterActions();

			if (args.Length == 0)
			{
				args = new string[] { "--start", };
			}

			string actionName;
			Hashtable parameters;
			try
			{
				ParseParameters(args, out actionName, out parameters);
			}
			catch(ArgumentException e)
			{
				Console.WriteLine(e.Message);
				return;
			}

			IAction action = GetAction(actionName);
			if (action != null)
				action.OnAction(parameters);
			else
				ShowSyntax();
		}
	}
}
