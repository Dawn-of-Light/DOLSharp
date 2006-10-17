using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Threading;

using DOL.Config;

using log4net;

namespace DOL.Mail
{
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
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
						log.Info("Mail enabled");
					}
					else
					{
						log.Info("Mail disabled");
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
				log.Error(e);
				return false;
			}

			SmtpClient = new SmtpClient(m_smtpServer);
			SmtpClient.UseDefaultCredentials = false;
			SmtpClient.EnableSsl = m_ssl;
			SmtpClient.Credentials = new NetworkCredential(m_username, m_password);

			if (m_enable)
			{
				//create callback thread for the send mail queue
				m_timer = new Timer(new TimerCallback(RunTask), null, m_mailFrequency, 0);
			}

			return true;
		}

		public static bool SendMail(string to, string subject, string message, string name)
		{
			if (m_enable == false)
			{
				log.Info("SendMail called but sending mail is not enabled.");
				return false;
			}
			try
			{
				MailMessage mail = new MailMessage();

				mail.To.Add(to);
				mail.Subject = subject;


				mail.From = new MailAddress(m_emailAddress, name);
				mail.IsBodyHtml = true;
				mail.Body = message;

				mail.BodyEncoding = System.Text.Encoding.ASCII;
				mail.SubjectEncoding = System.Text.Encoding.ASCII;

				lock (m_mailQueue.SyncRoot)
				{
					m_mailQueue.Enqueue(mail);
				}
			}
			catch (Exception)
			{
				return false;
			}
			return true;
		}

		private static void RunTask(object state)
		{
			MailMessage mailMessage = null;
			log.Info("Starting mail queue");
			lock (m_mailQueue.SyncRoot)
			{
				while (m_mailQueue.Count > 0)
				{
					try
					{
						int messageCount = m_mailQueue.Count;
						log.Info(String.Format("{0} mail message{1} in queue", messageCount,
								(messageCount > 1) ? "s" : ""));
						mailMessage = (MailMessage)m_mailQueue.Dequeue();
					}
					catch (Exception e)
					{
						log.Error("Couldn't retrieve message from queue: " + e.ToString());
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
							log.Error("Couldn't send message, requeueing");
							log.Error(e.ToString());
							m_mailQueue.Enqueue(mailMessage);
							break;
						}
					}
				}
			}
			log.Info("Ending mail queue");
			m_timer.Change(m_mailFrequency, Timeout.Infinite);
		}
	}
}