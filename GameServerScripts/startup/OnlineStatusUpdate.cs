/* 
 * This script is for pinging your player count to a website. Take the time to edit some of the fields
 * 
 * Author: Dawn of Light
 * Orginal Date: 31th December 2016
 * Released by: Shauzia
 * Based from: ServerListUpdate.cs 
 * Credits: Shauzia/Aceofspades
 */

using System;
using System.Net;
using System.Text;
using System.Threading;
using System.IO;
using DOL.GS;
using DOL.Events;
using log4net;
using System.Reflection;

namespace DOL.GS.GameEvents
{
	public class OnlineStatusUpdate
	{
		/// <summary>
        /// Gets the DAoCPortal UserName to authenticate with - Add your server username
		/// </summary>
		protected static string UserName = "";

		/// <summary>
		/// Gets the DAoCPortal Password to authenticate with - Add your server password
		/// </summary>
		protected static string Password = "";

        /// <summary>
        /// Gets player count - Don't edit this one.
        protected static string ClientCount = WorldMgr.GetAllPlayingClientsCount().ToString();
        /// </summary>
       

        /// <summary>
        /// Gets the URL from your ftp server - Edit the URL
        /// Example: http://www.bifrostgaming.com/daocbifrost/serverinfo.php
        /// </summary>
		protected static string UpdateUrl = UrlEncode("http://www.your-website-here.com/serverinfo.php?action=submit&totalclients=" + ClientCount);


		//!!!!!!!!!!!!!!!!!!!!DO NOT EDIT BELOW THIS LINE!!!!!!!!!!!!!!!!!!!!!!!!!!!
		#region Code

		/// <summary>
		/// The Script Version
		/// </summary>
		protected static double ScriptVersion = 2.1;

		/// <summary>
		/// creates the thread which is used to update the portal's entry.
		/// </summary>
		protected static Thread m_thread;

		/// <summary>
		/// creates the timer which will set the interval on which the portal list will be updated
		/// </summary>
		protected static Timer m_timer;

		/// <summary>
		/// Interval between server updates in miliseconds
		/// </summary>
		protected const int UPDATE_INTERVAL = 3 * 60 * 1000;

		/// <summary>
		/// Sets up our logger instance
		/// </summary>
		protected static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// This method is called when the script is loaded.
		/// </summary>
		[ScriptLoadedEvent]
		public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
		{
			Init();
		}
		
		/// <summary>
		/// This method is called when the scripts are unloaded. 
		/// </summary>
		[ScriptUnloadedEvent]
		public static void OnScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			Stop();
		}

		/// <summary>
		/// Initializes the DAoCPortal Update Manager
		/// </summary>
		/// <returns>returns true if successful or if the username is not supplied</returns>
		public static bool Init()
		{
			string[] args = ValidateArgs();
			if (args != null)
			{
				if (log.IsErrorEnabled)
				{
					log.Error("Login/URL Error: Validating Arguments Failed! The server's player count will not be added/updated on the list!");
                    log.Error("Login/URL Error: The arguments that failed validation are:");
					foreach (string arg in args)
					{
						if (arg != null)
							log.Error(arg);
					}
				}
				Stop();
				return false;
			}
			m_timer = new Timer(new TimerCallback(StartListThread), m_timer, 0, UPDATE_INTERVAL);
			if (m_timer == null)
			{
				if (log.IsErrorEnabled)
					log.Error("Update timer failed to start. Stopping!");
				return false;
			}
			return true;
		}

		/// <summary>
		/// Starts the thread which updates the portal. This runs on a seperate thread due 
		/// to the time it can take to complete a WebClient operation.
		/// </summary>
		/// <param name="timer"></param>
		private static void StartListThread(object timer)
		{
			m_thread = new Thread(new ThreadStart(StartList));
			m_thread.Start();
		}

		/// <summary>
		/// This method defines and formats the various strings to be used
		/// </summary>
		private static void StartList()
		{
    
            string Updater = UpdateUrl;

			log.Info("Your Website's Online Status has been Updated!");
			
		}

		/// <summary>
		/// This Method creates the web client and updates the list using the values
		/// provided in the updateurl
		/// </summary>
		/// <param name="updateurl">A pre-formatted URI used to send params to the web server</param>
		/// <returns>returns true if successful</returns>
		private static bool ListUpdater(string updateurl)
		{
			try
			{
				WebClient webclient = new WebClient();
				Byte[] contentBuffer = webclient.DownloadData(updateurl);
				string result = Encoding.ASCII.GetString(contentBuffer).ToLower();
				if (result.IndexOf("success") != -1)
				{
					return true;
				}
				else
				{
					result = result.Replace("<br>", "\r\n");
					if (log.IsErrorEnabled)
						log.Error(result);
					return false;
				}
			}
			catch (Exception ex)
			{
				if (log.IsErrorEnabled)
					log.Error("An Error Occured: " + ex.Message);
				return false;
			}
		}

		/// <summary>
		/// Stops the update process.
		/// </summary>
		/// <returns>true always</returns>
		public static bool Stop()
		{
			try
			{
				if (m_thread != null)
					m_thread = null;

				if (m_timer != null)
					m_timer = null;
				return true;
			}
			catch (Exception ex)
			{
				if (log.IsErrorEnabled)
					log.Error("An error occured: \r\n" + ex.ToString());
				return false;
			}

		}

		/// <summary>
		/// Validates the required elements of the list update.
		/// </summary>
		/// <returns>returns an array of values to indicate what failed the validation check</returns>
		public static string[] ValidateArgs()
		{
			string[] failed = new string[6];
			int count = 0;
			if (UserName == "" || UserName == "null")
			{
				failed[count] = "Username";
				count++;
			}
			if (Password == "" || Password == "null")
			{
				failed[count] = "Password";
				count++;
			}
			if (count == 0)
			{
				return null;
			}
			return failed;
		}

		/// <summary>
		/// This method makes the URL friendly (replaces spaces and other html characters
		/// with their url friendly counterparts.
		/// </summary>
		/// <param name="Url">url to be formatted</param>
		/// <returns>friendly url</returns>
		public static string UrlEncode(string Url)
		{
			string newUrl = String.Empty;
			newUrl = Url.Replace(" ", "%20");
			return newUrl;
		}
		#endregion
	}
}
