using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Threading;
using System.Xml;
using DOL.GS;

using DOL.Config;

using log4net;

namespace DOL.Mail
{
	/// <summary>
	/// Contains functions to compress with the gzip algorithm.
	/// </summary>
	public class GZipCompress
	{
		/// <summary>
		/// Compresses files with the gzip algorithm. 
		/// Compressed files will have a path equal to the source file path + ".gz".
		/// </summary>
		/// <param name="filesPathsList">An array containing paths of the files to compress.</param>
		/// <returns>True if files have been successfully compressed.</returns>
		public static bool compressMultipleFiles(Queue filesPathsList)
		{
			foreach (string file in filesPathsList)
			{
				//let's not try and compress already compressed files
				if (file.Contains(".gz"))
					continue;
				Console.WriteLine(file);
				if (!compressFile(file, file + ".gz"))
					return false;
			}
			return true;
		}

		/// <summary>
		/// Compresses a file with the gzip algorithm.
		/// (If you want to keep the correct file name in the archive you must set the dest
		/// path value equals to source + ".gz")
		/// </summary>
		/// <param name="source">The source file path.</param>
		/// <param name="dest">The destination file path.</param>
		/// <returns>True if the file have been successfully compressed.</returns>
		public static bool compressFile(string source, string dest)
		{
			try
			{
				// Stream to read the file
				FileStream fstream = new FileStream(source, FileMode.Open, FileAccess.Read);

				// We store the complete file into a buffer
				byte[] buf = new byte[fstream.Length];
				fstream.Read(buf, 0, buf.Length);
				fstream.Close();

				// Stream to write the compressed file
				fstream = new FileStream(dest, FileMode.Create);

				// File compression (fsream is automatically closed)
				GZipStream zipStream = new GZipStream(fstream, CompressionMode.Compress, false);
				zipStream.Write(buf, 0, buf.Length);
				zipStream.Close();

				return true;
			}
			catch (Exception e)
			{
				MailMgr.Logger.Error(e);
				return false;
			}
		}
	}

	public class MailMgr
	{
		private static string m_username = "";
		private static string m_password = "";
		private static string m_emailAddress = "";
		private static string m_smtpServer = "";
		private static bool m_enable = false;
		private static bool m_ssl = false;

		private static int m_mailFrequency = 5 * 60 * 1000;

		private static Queue m_mailQueue = new Queue();
		private static volatile Timer m_timer = null;

		private static SmtpClient SmtpClient = null;

		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		public static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static bool Init()
		{
			XMLConfigFile xmlConfig = new XMLConfigFile();
			FileInfo file = new FileInfo("./config/MailConfig.xml");

			try
			{
				if (file.Exists)
				{
					xmlConfig = XMLConfigFile.ParseXMLFile(file);
					m_enable = xmlConfig["Enable"].GetBoolean(false);
					if (m_enable)
					{
						m_username = xmlConfig["Username"].GetString("");
						m_password = xmlConfig["Password"].GetString("");
						m_emailAddress = xmlConfig["EmailAddress"].GetString("");
						m_smtpServer = xmlConfig["SMTPServer"].GetString("");
						m_ssl = xmlConfig["SSL"].GetBoolean(false);
						Logger.Info("Mail enabled");
					}
					else
					{
						Logger.Info("Mail disabled");
					}
				}
				else
				{
					//create and add default values
					xmlConfig["Username"].Set("");
					xmlConfig["Password"].Set("");
					xmlConfig["EmailAddress"].Set("");
					xmlConfig["SMTPServer"].Set("");
					xmlConfig["SSL"].Set(false);
					xmlConfig["Enable"].Set(false);
					file.Refresh();
					xmlConfig.Save(file);
				}
			}
			catch (Exception e)
			{
				Logger.Error(e);
				return false;
			}

			SmtpClient = new SmtpClient(m_smtpServer);
			SmtpClient.UseDefaultCredentials = false;
			SmtpClient.EnableSsl = m_ssl;
			SmtpClient.Credentials = new NetworkCredential(m_username, m_password);

			if (DOL.GS.ServerProperties.Properties.LOG_EMAIL_ADDRESSES != "")
				SendLogs(DOL.GS.ServerProperties.Properties.LOG_EMAIL_ADDRESSES);

			if (m_enable)
			{
				//create callback thread for the send mail queue
				m_timer = new Timer(new TimerCallback(RunTask), null, m_mailFrequency, 0);
			}

			return true;
		}

		public static bool SendMail(string to, string subject, string message, string fromName, string fromAddress)
		{
			if (m_enable == false)
			{
				Logger.Info("SendMail called but sending mail is not enabled.");
				return false;
			}
			try
			{
				MailMessage mail = new MailMessage();

				foreach (string str in to.SplitCSV())
					mail.To.Add(str);
				mail.Subject = subject;


				mail.From = new MailAddress(fromAddress, fromName);
				mail.IsBodyHtml = true;
				mail.Body = message;

				mail.BodyEncoding = System.Text.Encoding.ASCII;
				mail.SubjectEncoding = System.Text.Encoding.ASCII;

				lock (m_mailQueue.SyncRoot)
				{
					m_mailQueue.Enqueue(mail);
				}
			}
			catch (Exception e)
			{
				Logger.Error(e);
				return false;
			}
			return true;
		}

		/// <summary>
		/// Parses the Log config file and extracts all archived logs.
		/// </summary>
		/// <returns>A Queue made of strings containing logs urls.</returns>
		public static Queue GetArchivedLogsUrls()
		{
			FileInfo info = new FileInfo(GameServer.Instance.Configuration.LogConfigFile);
			Queue dirList = new Queue();

			try
			{
				if (info.Exists)
				{
					Queue archiveList = new Queue(); // List made of all archived logs urls.

					// We parse the xml file to find urls of logs.
					XmlTextReader configFileReader = new XmlTextReader(info.OpenRead());
					configFileReader.WhitespaceHandling = WhitespaceHandling.None;
					while (configFileReader.Read())
					{
						if (configFileReader.LocalName == "file")
						{
							// First attribute of node file is the url of a Log.
							configFileReader.MoveToFirstAttribute();
							info = new FileInfo(configFileReader.ReadContentAsString());

							// Foreach file in the Log directory, we will return all logs with
							// the same name + an extension added when archived.
							foreach (string file in Directory.GetFiles(info.DirectoryName, info.Name + "*"))
							{
								// Only if this file is accessible we add it to the list we will return.
								if (file != info.FullName)
									archiveList.Enqueue(file);
							}
						}
					}
					return archiveList;
				}
				else
				{
					// We return the default value.
					Queue archiveList = new Queue(); // List made of all archived logs urls.
					archiveList.Enqueue("./logs/Cheats.Log");
					archiveList.Enqueue("./logs/GMActions.Log");
					archiveList.Enqueue("./logs/Error.Log");
					archiveList.Enqueue("./logs/GameServer.Log");
					return archiveList;
				}
			}
			catch (Exception e)
			{
				Logger.Error(e);
				return dirList;
			}
		}

		/// <summary>
		/// Sends all logs to a given email address. MailMgr must be initialized.
		/// </summary>
		/// <param name="to">The destination email address.</param>
		/// <returns>True if files have been successfully sent.</returns>
		public static bool SendLogs(string to)
		{
			if (m_enable == false)
			{
				// TODO: Maybe one or both can be removed depending on what you want to print 
				Logger.Error("SendLogs called but sending mail is not enabled.");
				return false;
			}
			try
			{
				// Look for all archived logs to send.
				Queue archivedLogsUrls = new Queue(GetArchivedLogsUrls());

				if (archivedLogsUrls.Count > 0)
				{
					// Compress logs before attaching them.
					Logger.Info("Compressing logs...");
					if (!GZipCompress.compressMultipleFiles(archivedLogsUrls))
					{
						Logger.Error("Cannot compress files properly. Email not sent.");
						return false;
					}

					// Build the mail.
					MailMessage mail = new MailMessage();

					foreach (string str in to.SplitCSV())
						if (!String.IsNullOrEmpty(str.Trim())) mail.To.Add(str);
					mail.Subject = "[ Logs ] " + DateTime.Now.ToString();
					mail.From = new MailAddress(m_emailAddress, GameServer.Instance.Configuration.ServerName);
					mail.IsBodyHtml = true;
					mail.Body = ""; // Add the mail core here if needed
					mail.BodyEncoding = System.Text.Encoding.ASCII;
					mail.SubjectEncoding = System.Text.Encoding.ASCII;

					foreach (string file in archivedLogsUrls)
					{
						// Attach each compressed Log with correct name.
						FileInfo info = new FileInfo(file + ".gz");
						Attachment a = new Attachment(file + ".gz");
						a.Name = info.Name;
						mail.Attachments.Add(a);
					}

					// Send the email.
					Logger.Info("Sending logs to: " + to);
					SmtpClient.Send(mail);

					// Free ressources: *.gz before deleting them.
					mail.Attachments.Dispose();

					// Delete archives and logs.
					// Any exception above will avoid deleting logs if they are not correctly sent.
					foreach (string file in archivedLogsUrls)
					{
						File.Delete(file);
						File.Delete(file + ".gz");
					}
				}
				else
				{
					Logger.Info("No Log to send.");
				}
			}
			catch (Exception e)
			{
				Logger.Error(e);
				return false;
			}
			return true;
		}

		private static void RunTask(object state)
		{
			MailMessage mailMessage = null;
			Logger.Info("Starting mail queue");
			lock (m_mailQueue.SyncRoot)
			{
				while (m_mailQueue.Count > 0)
				{
					try
					{
						int messageCount = m_mailQueue.Count;
						Logger.Info(String.Format("{0} mail message{1} in queue", messageCount,
								(messageCount > 1) ? "s" : ""));
						mailMessage = (MailMessage)m_mailQueue.Dequeue();
					}
					catch (Exception e)
					{
						Logger.Error("Couldn't retrieve message from queue: " + e.ToString());
						break;
					}
					if (mailMessage != null)
					{
						try
						{
							SmtpClient.Send(mailMessage);
						}
						catch (Exception e)
						{
							Logger.Error("Couldn't send message, requeueing");
							Logger.Error(e.ToString());
							m_mailQueue.Enqueue(mailMessage);
							break;
						}
					}
				}
			}
			Logger.Info("Ending mail queue");
			m_timer.Change(m_mailFrequency, Timeout.Infinite);
		}
	}
}