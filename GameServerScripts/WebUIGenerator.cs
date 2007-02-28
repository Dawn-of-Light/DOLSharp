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
using System.Reflection;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Scripts
{
	/// <summary>
	/// Summary description for DefWebUIGenerator. This is a self contained script within the scripts assembly.
	/// </summary>
	public class WebUIGenerator
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		[ScriptLoadedEvent]
		public static void OnScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			//Uncomment the following line to enable the WebUI
			//Start();
		}

		[ScriptUnloadedEvent]
		public static void OnScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			//Uncomment the following line to enable the WebUI
			//Stop();
		}

		/// <summary>
		/// Generates the DOL web ui
		/// </summary>
		public class WebUIDir
		{
			public string m_path;
			public string[] m_files;
			public ArrayList m_dirs = new ArrayList();
		}

		private static System.Text.StringBuilder m_js = null;
		private static System.Timers.Timer m_timer = null;

		/// <summary>
		/// Parses a directory for all source files
		/// </summary>
		/// <param name="parent"></param>
		private static void ParseDirectory(WebUIDir parent)
		{
			string[] dirs = Directory.GetDirectories(parent.m_path);
			IEnumerator iter = dirs.GetEnumerator();
			while (iter.MoveNext())
			{
				WebUIDir dir = new WebUIDir();
				dir.m_path = (string) (iter.Current);
				ParseDirectory(dir);
				parent.m_dirs.Add(dir);
			}

			parent.m_files = Directory.GetFiles(parent.m_path, "*.*");
		}

		/// <summary>
		/// Copies all files from .\webui\template to .\webui\generated
		/// </summary>
		/// <param name="dir">Parent directory</param>
		private static void CopyFromTemplate(WebUIDir dir)
		{
			string path = dir.m_path.Replace("."+Path.DirectorySeparatorChar+"webui"+Path.DirectorySeparatorChar+"template", "."+Path.DirectorySeparatorChar+"webui"+Path.DirectorySeparatorChar+"generated");

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			foreach (WebUIDir d in dir.m_dirs)
			{
				CopyFromTemplate(d);
			}

			foreach (string s in dir.m_files)
			{
				FileInfo fi = new FileInfo(s);
				string fpath = s.Replace("."+Path.DirectorySeparatorChar+"webui"+Path.DirectorySeparatorChar+"template", "."+Path.DirectorySeparatorChar+"webui"+Path.DirectorySeparatorChar+"generated");

				if (fi.Extension.IndexOf("html") != -1 || fi.Extension.IndexOf("htm") != -1)
				{
					GenerateJS(s);
				}
				else
				{
					fi.CopyTo(fpath, true);
				}
			}
		}

		/// <summary>
		/// Insert the javascript stuff into an html file
		/// </summary>
		/// <param name="fname">The template file name</param>
		private static void GenerateJS(string fname)
		{
			string path = fname.Replace(Path.DirectorySeparatorChar+"webui"+Path.DirectorySeparatorChar+"template", Path.DirectorySeparatorChar+"webui"+Path.DirectorySeparatorChar+"generated");
			string buf = "";

			using (StreamReader rdr = File.OpenText(fname))
			{
				buf = rdr.ReadToEnd();

				rdr.Close();
			}

			int pos = buf.IndexOf("<head>");

			if (pos == -1)
			{
				throw new Exception("Invalid HTML file, could not locate <head> tag");
			}
			else
			{
				pos += "<head>".Length;
			}

			buf = buf.Insert(pos, m_js.ToString());

			using (StreamWriter wrtr = File.CreateText(path))
			{
				wrtr.WriteLine(buf);
				wrtr.Flush();
				wrtr.Close();
			}
		}

		/// <summary>
		/// Builds the javascript block
		/// </summary>
		private static void InitJS()
		{
			m_js = new System.Text.StringBuilder();
			StreamWriter nl = new StreamWriter(new MemoryStream());

			m_js.Append(nl.NewLine);
			m_js.Append("<script type=\"text/javascript\">");
			m_js.Append(nl.NewLine);
			m_js.Append(nl.NewLine);

			m_js.AppendFormat("var dateTime = \"{0}\"", DateTime.Now.ToString());
			m_js.Append(nl.NewLine);

			m_js.AppendFormat("var srvrName = \"{0}\"", GameServer.Instance.Configuration.ServerName);
			m_js.Append(nl.NewLine);

			int gm = 0;
			int admin = 0;
			foreach (GameClient client in WorldMgr.GetAllClients()) {
				if (client.Account.PrivLevel == (int)ePrivLevel.GM) gm++;
				if (client.Account.PrivLevel == (int)ePrivLevel.Admin) admin++;
			}

			m_js.AppendFormat("var numClientsConnected = {0}", GameServer.Instance.ClientCount);
			m_js.Append(nl.NewLine);

			m_js.AppendFormat("var numGMsConnected = {0}", gm);
			m_js.Append(nl.NewLine);

			m_js.AppendFormat("var numAdminsConnected = {0}", admin);
			m_js.Append(nl.NewLine);

			m_js.AppendFormat("var numAccts = {0}", GameServer.Database.GetObjectCount(typeof (DOL.Database.Account)));
			m_js.Append(nl.NewLine);

			m_js.AppendFormat("var numMobs = {0}", GameServer.Database.GetObjectCount(typeof (DOL.Database.Mob)));
			m_js.Append(nl.NewLine);

			m_js.AppendFormat("var numInvItems = {0}", GameServer.Database.GetObjectCount(typeof (DOL.Database.InventoryItem)));
			m_js.Append(nl.NewLine);

			m_js.AppendFormat("var numPlrChars = {0}", GameServer.Database.GetObjectCount(typeof (DOL.Database.Character)));
			m_js.Append(nl.NewLine);

			m_js.AppendFormat("var numMerchantItems = {0}", GameServer.Database.GetObjectCount(typeof (DOL.Database.MerchantItem)));
			m_js.Append(nl.NewLine);

			m_js.AppendFormat("var numItemTemplates = {0}", GameServer.Database.GetObjectCount(typeof (DOL.Database.ItemTemplate)));
			m_js.Append(nl.NewLine);

			m_js.AppendFormat("var numWorldObjects = {0}", GameServer.Database.GetObjectCount(typeof (DOL.Database.WorldObject)));
			m_js.Append(nl.NewLine);

			m_js.AppendFormat("var srvrType = \"{0}\"", GameServer.Instance.Configuration.ServerType.ToString());
			m_js.Append(nl.NewLine);

			m_js.AppendFormat("var aac = \"{0}\"", GameServer.Instance.Configuration.AutoAccountCreation ? "enabled" : "disabled");
			m_js.Append(nl.NewLine);

			m_js.AppendFormat("var srvrStatus = \"{0}\"", GameServer.Instance.ServerStatus.ToString());
			m_js.Append(nl.NewLine);
			m_js.Append(nl.NewLine);

			//begin function
			m_js.Append("function WritePlrTable()");
			m_js.Append(nl.NewLine);
			m_js.Append("{");
			m_js.Append(nl.NewLine);

			//begin table
			m_js.Append("document.write(\"<table width=\\\"100%\\\" border=\\\"0\\\" cellpadding=\\\"4\\\">\")");
			m_js.Append(nl.NewLine);

			//first row
			m_js.Append("document.write(\"<tr>\")");
			m_js.Append(nl.NewLine);

			//name column
			m_js.Append("document.write(\"<td bgcolor=\\\"#333333\\\">\")");
			m_js.Append(nl.NewLine);
			m_js.Append("document.write(\"Name\")");
			m_js.Append(nl.NewLine);
			m_js.Append("document.write(\"</td>\")");
			m_js.Append(nl.NewLine);
			m_js.Append(nl.NewLine);

			//last name column
			m_js.Append("document.write(\"<td bgcolor=\\\"#333333\\\">\")");
			m_js.Append(nl.NewLine);
			m_js.Append("document.write(\"Last Name\")");
			m_js.Append(nl.NewLine);
			m_js.Append("document.write(\"</td>\")");
			m_js.Append(nl.NewLine);
			m_js.Append(nl.NewLine);

			//Class
			m_js.Append("document.write(\"<td bgcolor=\\\"#333333\\\">\")");
			m_js.Append(nl.NewLine);
			m_js.Append("document.write(\"Class\")");
			m_js.Append(nl.NewLine);
			m_js.Append("document.write(\"</td>\")");
			m_js.Append(nl.NewLine);
			m_js.Append(nl.NewLine);

			//Race
			m_js.Append("document.write(\"<td bgcolor=\\\"#333333\\\">\")");
			m_js.Append(nl.NewLine);
			m_js.Append("document.write(\"Race\")");
			m_js.Append(nl.NewLine);
			m_js.Append("document.write(\"</td>\")");
			m_js.Append(nl.NewLine);
			m_js.Append(nl.NewLine);

			//Guild
			m_js.Append("document.write(\"<td bgcolor=\\\"#333333\\\">\")");
			m_js.Append(nl.NewLine);
			m_js.Append("document.write(\"Guild\")");
			m_js.Append(nl.NewLine);
			m_js.Append("document.write(\"</td>\")");
			m_js.Append(nl.NewLine);
			m_js.Append(nl.NewLine);

			//Level
			m_js.Append("document.write(\"<td bgcolor=\\\"#333333\\\">\")");
			m_js.Append(nl.NewLine);
			m_js.Append("document.write(\"Level\")");
			m_js.Append(nl.NewLine);
			m_js.Append("document.write(\"</td>\")");
			m_js.Append(nl.NewLine);
			m_js.Append(nl.NewLine);

			//Alive
			m_js.Append("document.write(\"<td bgcolor=\\\"#333333\\\">\")");
			m_js.Append(nl.NewLine);
			m_js.Append("document.write(\"Alive\")");
			m_js.Append(nl.NewLine);
			m_js.Append("document.write(\"</td>\")");
			m_js.Append(nl.NewLine);
			m_js.Append(nl.NewLine);

			//Realm
			m_js.Append("document.write(\"<td bgcolor=\\\"#333333\\\">\")");
			m_js.Append(nl.NewLine);
			m_js.Append("document.write(\"Realm\")");
			m_js.Append(nl.NewLine);
			m_js.Append("document.write(\"</td>\")");
			m_js.Append(nl.NewLine);
			m_js.Append(nl.NewLine);

			//Current Region
			m_js.Append("document.write(\"<td bgcolor=\\\"#333333\\\">\")");
			m_js.Append(nl.NewLine);
			m_js.Append("document.write(\"Current Region\")");
			m_js.Append(nl.NewLine);
			m_js.Append("document.write(\"</td>\")");
			m_js.Append(nl.NewLine);
			m_js.Append(nl.NewLine);

			//X
			m_js.Append("document.write(\"<td bgcolor=\\\"#333333\\\">\")");
			m_js.Append(nl.NewLine);
			m_js.Append("document.write(\"X\")");
			m_js.Append(nl.NewLine);
			m_js.Append("document.write(\"</td>\")");
			m_js.Append(nl.NewLine);
			m_js.Append(nl.NewLine);

			//Y
			m_js.Append("document.write(\"<td bgcolor=\\\"#333333\\\">\")");
			m_js.Append(nl.NewLine);
			m_js.Append("document.write(\"Y\")");
			m_js.Append(nl.NewLine);
			m_js.Append("document.write(\"</td>\")");
			m_js.Append(nl.NewLine);
			m_js.Append(nl.NewLine);

			//end row
			m_js.Append("document.write(\"</tr>\")");
			m_js.Append(nl.NewLine);
			m_js.Append(nl.NewLine);

			foreach (GameClient client in WorldMgr.GetAllPlayingClients())
			{
				GamePlayer plr = client.Player;

				m_js.Append("document.write(\"<tr>\")");
				m_js.Append(nl.NewLine);

				//name column
				m_js.Append("document.write(\"<td bgcolor=\\\"#333333\\\">\")");
				m_js.Append(nl.NewLine);
				m_js.AppendFormat("document.write(\"{0}\")", plr.Name);
				m_js.Append(nl.NewLine);
				m_js.Append("document.write(\"</td>\")");
				m_js.Append(nl.NewLine);
				m_js.Append(nl.NewLine);

				//last name column
				m_js.Append("document.write(\"<td bgcolor=\\\"#333333\\\">\")");
				m_js.Append(nl.NewLine);
				m_js.AppendFormat("document.write(\"{0}\")", plr.LastName);
				m_js.Append(nl.NewLine);
				m_js.Append("document.write(\"</td>\")");
				m_js.Append(nl.NewLine);
				m_js.Append(nl.NewLine);

				//Class
				m_js.Append("document.write(\"<td bgcolor=\\\"#333333\\\">\")");
				m_js.Append(nl.NewLine);
				m_js.AppendFormat("document.write(\"{0}\")", plr.CharacterClass.Name);
				m_js.Append(nl.NewLine);
				m_js.Append("document.write(\"</td>\")");
				m_js.Append(nl.NewLine);
				m_js.Append(nl.NewLine);

				//Race
				m_js.Append("document.write(\"<td bgcolor=\\\"#333333\\\">\")");
				m_js.Append(nl.NewLine);
				m_js.AppendFormat("document.write(\"{0}\")", plr.RaceName);
				m_js.Append(nl.NewLine);
				m_js.Append("document.write(\"</td>\")");
				m_js.Append(nl.NewLine);
				m_js.Append(nl.NewLine);

				//Guild
				m_js.Append("document.write(\"<td bgcolor=\\\"#333333\\\">\")");
				m_js.Append(nl.NewLine);
				m_js.AppendFormat("document.write(\"{0}\")", plr.GuildName);
				m_js.Append(nl.NewLine);
				m_js.Append("document.write(\"</td>\")");
				m_js.Append(nl.NewLine);
				m_js.Append(nl.NewLine);

				//Level
				m_js.Append("document.write(\"<td align=\\\"center\\\" bgcolor=\\\"#333333\\\">\")");
				m_js.Append(nl.NewLine);
				m_js.AppendFormat("document.write(\"{0}\")", plr.Level);
				m_js.Append(nl.NewLine);
				m_js.Append("document.write(\"</td>\")");
				m_js.Append(nl.NewLine);
				m_js.Append(nl.NewLine);

				//Alive
				m_js.Append("document.write(\"<td bgcolor=\\\"#333333\\\">\")");
				m_js.Append(nl.NewLine);
				m_js.AppendFormat("document.write(\"{0}\")", plr.IsAlive ? "yes" : "no");
				m_js.Append(nl.NewLine);
				m_js.Append("document.write(\"</td>\")");
				m_js.Append(nl.NewLine);
				m_js.Append(nl.NewLine);

				//Realm
				m_js.Append("document.write(\"<td bgcolor=\\\"#333333\\\">\")");
				m_js.Append(nl.NewLine);
				m_js.AppendFormat("document.write(\"{0}\")", ((eRealm) plr.Realm).ToString());
				m_js.Append(nl.NewLine);
				m_js.Append("document.write(\"</td>\")");
				m_js.Append(nl.NewLine);
				m_js.Append(nl.NewLine);

				//Current Region
				m_js.Append("document.write(\"<td bgcolor=\\\"#333333\\\">\")");
				m_js.Append(nl.NewLine);
				m_js.AppendFormat("document.write(\"{0}\")", plr.CurrentRegion.Description);
				m_js.Append(nl.NewLine);
				m_js.Append("document.write(\"</td>\")");
				m_js.Append(nl.NewLine);
				m_js.Append(nl.NewLine);

				//X
				m_js.Append("document.write(\"<td align=\\\"center\\\" bgcolor=\\\"#333333\\\">\")");
				m_js.Append(nl.NewLine);
				m_js.AppendFormat("document.write(\"{0}\")", plr.X);
				m_js.Append(nl.NewLine);
				m_js.Append("document.write(\"</td>\")");
				m_js.Append(nl.NewLine);
				m_js.Append(nl.NewLine);

				//Y
				m_js.Append("document.write(\"<td align=\\\"center\\\" bgcolor=\\\"#333333\\\">\")");
				m_js.Append(nl.NewLine);
				m_js.AppendFormat("document.write(\"{0}\")", plr.Y);
				m_js.Append(nl.NewLine);
				m_js.Append("document.write(\"</td>\")");
				m_js.Append(nl.NewLine);
				m_js.Append(nl.NewLine);

				m_js.Append("document.write(\"</tr>\")");
				m_js.Append(nl.NewLine);
				m_js.Append(nl.NewLine);
			}

			m_js.Append("document.write(\"</table>\")");
			m_js.Append(nl.NewLine);

			m_js.Append("}");
			m_js.Append(nl.NewLine);

			m_js.Append("</script>");
			m_js.Append(nl.NewLine);
		}

		/// <summary>
		/// Reads in the template and generates the appropriate html
		/// </summary>
		public static void Generate()
		{
			try
			{
				InitJS();

				WebUIDir root = new WebUIDir();
				root.m_path = "."+Path.DirectorySeparatorChar+"webui"+Path.DirectorySeparatorChar+"template";

				if (!Directory.Exists("."+Path.DirectorySeparatorChar+"webui"))
				{
					Directory.CreateDirectory("."+Path.DirectorySeparatorChar+"webui");
				}

				ParseDirectory(root);

				CopyFromTemplate(root);

				if (log.IsInfoEnabled)
					if (log.IsInfoEnabled)
						log.Info("WebUI Generation initialized!");
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("WebUI Generation", e);
			}
		}

		/// <summary>
		/// Starts the timer to generate the web ui
		/// </summary>
		public static void Start()
		{
			if (m_timer != null)
			{
				Stop();
			}

			m_timer = new System.Timers.Timer(60000.0); //1 minute
			m_timer.Elapsed += new System.Timers.ElapsedEventHandler(m_timer_Elapsed);
			m_timer.AutoReset = true;
			m_timer.Start();

			if (log.IsInfoEnabled)
				if (log.IsInfoEnabled)
					log.Info("Web UI generation started...");
		}

		/// <summary>
		/// Stops the timer that generates the web ui
		/// </summary>
		public static void Stop()
		{
			if (m_timer != null)
			{
				m_timer.Stop();
				m_timer.Close();
				m_timer.Elapsed -= new System.Timers.ElapsedEventHandler(m_timer_Elapsed);
				m_timer = null;
			}

			Generate();

			if (log.IsInfoEnabled)
				if (log.IsInfoEnabled)
					log.Info("Web UI generation stopped...");
		}

		/// <summary>
		/// The timer proc that generates the web ui every X milliseconds
		/// </summary>
		/// <param name="sender">Caller of this function</param>
		/// <param name="e">Info about the timer</param>
		private static void m_timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			Generate();
		}
	}
}