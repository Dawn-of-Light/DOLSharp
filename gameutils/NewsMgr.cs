using System;

using DOL.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS
{
	public enum eNewsType : byte
	{ 
		RvRGlobal = 0,
		RvRLocal = 1,
		PvE = 2,
	}

	public class NewsMgr
	{
		public static void CreateNews(string message, eRealm realm, eNewsType type, bool sendMessage)
		{
			if (sendMessage)
			{
				foreach (GameClient client in WorldMgr.GetAllClients())
				{
					if (client.Player == null)
						continue;
					if ((client.Account.PrivLevel != 1 || realm == eRealm.None) || client.Player.Realm == realm)
					{
						client.Out.SendMessage(message, eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
				}
			}

			DBNews news = new DBNews();
			news.Type = (byte)type;
			news.Realm = (byte)realm;
			news.Text = message;
			GameServer.Database.AddNewObject(news);
		}

		public static void DisplayNews(GameClient client)
		{
			// N,chanel(0/1/2),index(0-4),string time,\"news\"

			for (int type = 0; type <= 2; type++)
			{
				int index = 0;
				string realm = "";
				//we can see all captures
				if (type > 0)
					realm = " AND (`Realm` = '0' OR `Realm` = '" + client.Player.Realm + "')";

				DBNews[] newsList = (DBNews[])GameServer.Database.SelectObjects(typeof(DBNews), "`Type` = '" + type + "'" + realm + " ORDER BY `CreationDate` DESC LIMIT 5");

				foreach (DBNews news in newsList)
				{
					if (index > 4)
						break;

					client.Out.SendMessage(string.Format("N,{0},{1},{2},\"{3}\"", news.Type, index++, RetElapsedTime(news.CreationDate), news.Text), eChatType.CT_SocialInterface, eChatLoc.CL_SystemWindow);
				}
			}
		}

		private static string RetElapsedTime(DateTime dt)
		{
			TimeSpan playerEnterGame = DateTime.Now.Subtract(dt);
			string newsTime;
			if (playerEnterGame.Days > 0)
				newsTime = playerEnterGame.Days.ToString() + " day" + ((playerEnterGame.Days > 1) ? "s" : "");
			else if (playerEnterGame.Hours > 0)
				newsTime = playerEnterGame.Hours.ToString() + " hour" + ((playerEnterGame.Hours > 1) ? "s" : "");
			else if (playerEnterGame.Minutes > 0)
				newsTime = playerEnterGame.Minutes.ToString() + " minute" + ((playerEnterGame.Minutes > 1) ? "s" : "");
			else
				newsTime = playerEnterGame.Seconds.ToString() + " second" + ((playerEnterGame.Seconds > 1) ? "s" : "");
			return newsTime;
		}
	}
}