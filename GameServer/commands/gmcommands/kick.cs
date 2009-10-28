// Kick by Akira (akira@dataloggin.com)
//
//

using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&kick",
		new string[] { "&k" },
		ePrivLevel.GM,
		"GMCommands.Kick.Description",
		"GMCommands.Kick.Usage")]
	public class KickCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (string.IsNullOrEmpty(args[1]))
			    return;
			    
			GameClient clientc = WorldMgr.GetClientByPlayerName(args[1], false, false);
			
			if (clientc == null)
			{
				DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Kick.NoPlayerOnLine"));
				return;
			}

			for (int i = 0; i < 8; i++)
			{
				string message;
				if (client != null && client.Player != null)
					message = LanguageMgr.GetTranslation(clientc, "GMCommands.Kick.RemovedFromServerByGM", client.Player.Name);
				else
					message = LanguageMgr.GetTranslation(clientc, "GMCommands.Kick.RemovedFromServer");
				clientc.Out.SendMessage(message, eChatType.CT_Help, eChatLoc.CL_SystemWindow);
				clientc.Out.SendMessage(message, eChatType.CT_Help, eChatLoc.CL_ChatWindow);
			}

			clientc.Out.SendPlayerQuit(true);
			clientc.Player.SaveIntoDatabase();
			clientc.Player.Quit(true);
		}
	}
}