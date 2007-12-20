using System;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[CmdAttribute("&summon", ePrivLevel.Player,"Summon horse","/summon")]
	public class SummonHorseCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (client.Player == null)
				return 1;
			try
			{
				if (args.Length > 1 && Convert.ToInt16(args[1]) == 0)
					client.Player.IsOnHorse = false;
			}
			catch
			{
				client.Player.Out.SendMessage("Incorrect format of the command", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			finally
			{
				if (client.Account.PrivLevel > 1 && client.Player.IsOnHorse == false)
					client.Player.IsOnHorse = true;
			}
			return 1;
		}
	}
}