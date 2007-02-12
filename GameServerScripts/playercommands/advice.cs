using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&advice",
		(uint)ePrivLevel.Player,
		"Ask for advice from an advisor",
		"Advisors will reply via /send",
		"Please answer them via /send <Name of the Advisor>",
		"/advice - shows all advisors",
		"/advice <message>")]
	public class AdviceCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			string msg = "";
			if (args.Length >= 2)
			{
				for (int i = 1; i < args.Length; ++i)
				{
					msg += args[i] + " ";
				}
			}
			else
			{
				int total = 0;
				foreach (GameClient playerClient in WorldMgr.GetAllClients())
				{
					if (playerClient.Player == null) continue;
					if (playerClient.Player.Advisor &&
					   ((playerClient.Player.Realm == client.Player.Realm && playerClient.Player.IsAnonymous == false) ||
					   client.Account.PrivLevel > 1))
					{
						total++;
						client.Out.SendMessage(total + ")" + playerClient.Player.Name + (playerClient.Player.IsAnonymous ? " [ANON]" : ""), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}

				}
				client.Out.SendMessage("There are " + total + " advisors online!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}
			foreach (GameClient playerClient in WorldMgr.GetAllClients())
			{
				if (playerClient.Player == null) continue;
				if ((playerClient.Player.Advisor &&
					playerClient.Player.Realm == client.Player.Realm) ||
					playerClient.Account.PrivLevel > 1)
					playerClient.Out.SendMessage("[ADVICE" + getRealmString(client.Player.Realm) + "] " + client.Player.Name + ": \"" + msg + "\"", eChatType.CT_Staff, eChatLoc.CL_ChatWindow);

			}
			return 1;
		}

		public string getRealmString(byte Realm)
		{
			switch (Realm)
			{
				case 1: return " ALB";
				case 2: return " MID";
				case 3: return " HIB";
				default: return " NONE";
			}
		}
	}
}