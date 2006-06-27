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
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace DOL.DOLServer
{
	/// <summary>
	/// Zusammenfassung für GameServerServiceInstaller.
	/// </summary>
	[RunInstaller(true)]
	public class GameServerServiceInstaller : Installer
	{
		private ServiceInstaller m_gameServerServiceInstaller;
		private ServiceProcessInstaller m_gameServerServiceProcessInstaller;

		public GameServerServiceInstaller()
		{
			// Instantiate installers for process and services.
			m_gameServerServiceProcessInstaller = new ServiceProcessInstaller();
			m_gameServerServiceProcessInstaller.Account = ServiceAccount.LocalSystem;

			m_gameServerServiceInstaller = new ServiceInstaller();
			m_gameServerServiceInstaller.StartType = ServiceStartMode.Manual;
			m_gameServerServiceInstaller.ServiceName = "DOL";

			Installers.Add(m_gameServerServiceProcessInstaller);
			Installers.Add(m_gameServerServiceInstaller);
		}

		public override void Install(System.Collections.IDictionary stateSaver)
		{
			Context.Parameters["assemblyPath"] += " --SERVICERUN " + Context.Parameters["commandline"];
			base.Install(stateSaver);
		}

	}
}