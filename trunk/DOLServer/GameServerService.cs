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
using System.Reflection;
using System.IO;
using System.ServiceProcess;
using DOL.GS;

namespace DOL.DOLServer
{
	/// <summary>
	/// DOL System Service
	/// </summary>
	public class GameServerService : ServiceBase
	{
		public GameServerService()
		{
			this.ServiceName = "DOL";
			this.AutoLog = false;
			this.CanHandlePowerEvent = false;
			this.CanPauseAndContinue = false;
			this.CanShutdown = true;
			this.CanStop = true;
		}

		private static bool StartServer()
		{
			//TODO parse args for -config parameter!
			FileInfo dolserver = new FileInfo(Assembly.GetExecutingAssembly().Location);
			Directory.SetCurrentDirectory(dolserver.DirectoryName);
			FileInfo configFile = new FileInfo("./config/serverconfig.xml");
			GameServerConfiguration config = new GameServerConfiguration();
			if(configFile.Exists)
			{
				config.LoadFromXMLFile(configFile);
			}
			else
			{
				if(!configFile.Directory.Exists)
					configFile.Directory.Create();
				config.SaveToXMLFile(configFile);
			}

			GameServer.CreateInstance(config);

			return GameServer.Instance.Start();
		}

		private static void StopServer()
		{
			GameServer.Instance.Stop();
		}

		protected override void OnStart(string[] args)
		{
			if (!StartServer())
				throw new ApplicationException("Failed to start server!");
		}

		protected override void OnStop()
		{
			StopServer();
		}

		protected override void OnShutdown()
		{
			StopServer();
		}

		/// <summary>
		/// Gets the DOL service from the service list
		/// </summary>
		/// <returns></returns>
		public static ServiceController GetDOLService()
		{
			foreach (ServiceController svcc in ServiceController.GetServices())
			{
				if (svcc.ServiceName.ToLower().Equals("dol"))
					return svcc;
			}
			return null;
		}
	}
}