using System;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&advisor",
		(uint)ePrivLevel.Player,
		"Toggles Advisor status",
		"/advisor")]
	public class AdvisorCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			client.Player.Advisor = !client.Player.Advisor;
			if (client.Player.Advisor)
				client.Out.SendMessage("You will now receive messages from the advice channel", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			else
				client.Out.SendMessage("You will no longer receive messages from the advice channel", eChatType.CT_System, eChatLoc.CL_SystemWindow);

			return 1;
		}
	}
}