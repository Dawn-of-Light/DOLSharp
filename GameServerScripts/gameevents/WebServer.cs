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

using Nancy;
using Nancy.Hosting.Self;
using log4net;

using DOL.Events;

namespace DOL.GS.Web
{
	/// <summary>
	/// WebServer Script Initializing Nancy Self Hosting.
	/// </summary>
	public class WebServer
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Define Self Hosted Web Server
		/// </summary>
		static NancyHost m_embeddedWebServer = null;
		
		[GameServerStartedEvent]
		public static void OnServerStart(DOLEvent e, object sender, EventArgs arguments)
		{
			if (m_embeddedWebServer == null)
			{
				try
				{
					m_embeddedWebServer = new NancyHost(new Uri("http://localhost:10200"));
					m_embeddedWebServer.Start();
					
				}
				catch (Exception ex)
				{
					if (log.IsErrorEnabled)
						log.ErrorFormat("Error While Starting Embedded Nancy Web Server : {0}", ex);
				}
			}
		}
		
		[GameServerStoppedEvent]
		public static void OnServerStop(DOLEvent e, object sender, EventArgs arguments)
		{
			if (m_embeddedWebServer != null)
			{
				try
				{
					m_embeddedWebServer.Stop();
				}
				catch (Exception ex)
				{
					if (log.IsErrorEnabled)
						log.ErrorFormat("Error While Stopping Embedded Nancy Web Server : {0}", ex);
				}
			}
		}
		
		public WebServer()
		{
		}
	}
	
	/// <summary>
	/// Demo Page
	/// </summary>
    public class HelloModule : NancyModule
    {
        public HelloModule()
        {
        	Get["/"] = parameters => string.Format("<pre>Current Game Time: {0}\tCurrent Player Count: {1}</pre>", WorldMgr.GetCurrentGameTime(), WorldMgr.GetAllPlayingClientsCount());
        }
    }
}
