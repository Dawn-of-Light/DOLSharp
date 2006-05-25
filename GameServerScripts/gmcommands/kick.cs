// Kick by Akira (akira@dataloggin.com)
//
//			

using DOL.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
	  "&kick",
	  new string[] { "&k" },
	  (int)ePrivLevel.GM,
		"Kicks the named player off the server!",
		 "/kick <name>")]
	public class KickCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			GameClient clientc = WorldMgr.GetClientByPlayerName(args[1], true);
			if (clientc == null)
			{
				client.Out.SendMessage("No one with that name is online to Kick!", eChatType.CT_Help, eChatLoc.CL_SystemWindow);
				return 0;
			}
			for (int i = 0; i < 8; i++)
			{
				clientc.Out.SendMessage("You have been removed from the server by GM " + client.Player.Name + "!", eChatType.CT_Help, eChatLoc.CL_SystemWindow);
				clientc.Out.SendMessage("You have been removed from the server by GM " + client.Player.Name + "!", eChatType.CT_Help, eChatLoc.CL_ChatWindow);
			}

			clientc.Out.SendPlayerQuit(true);
			clientc.Player.SaveIntoDatabase();
			clientc.Player.Quit(true);
			return 0;
		}
	}
}