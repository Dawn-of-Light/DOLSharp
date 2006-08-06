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
using System;
using System.Reflection;
using DOL.Database;
using DOL.GS.Scripts;
using log4net;

namespace DOL.GS.ServerProperties
{
	/// <summary>
	/// The server property manager class
	/// </summary>
	public class ServerPropertyMgr
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Initialise the properties
		/// </summary>
		/// <returns>true if successful</returns>
		public static bool Init()
		{
			if (log.IsInfoEnabled)
				log.Info("ServerPropertyMgr initialising server properties");

			foreach (Type t in ScriptMgr.GetDerivedClasses(typeof(IServerProperty)))
			{
				if (t.IsAbstract) continue;

				IServerProperty property;
				try
				{
					object[] objs = t.GetCustomAttributes(false);
					foreach (ServerPropertyAttribute attrib in objs)
					{
						if (log.IsInfoEnabled)
							log.Info("ServerPropertyMgr: ServerProperty - '" + attrib.Key + "' - (" + attrib.Description + ") default value:" + attrib.DefaultValue);
						property = (IServerProperty)Activator.CreateInstance(t, new object[]{attrib});
						property.Load();
					}
				}
				catch(Exception e)
				{
					log.Error(e);
				}
			}

			return true;
		}
	}
}
