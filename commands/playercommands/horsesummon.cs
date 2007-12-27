using System;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[CmdAttribute("&summon", ePrivLevel.Player,"Summon horse","/summon")]
	public class SummonHorseCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (client.Player == null)
				return;
			try
			{
				if (args.Length > 1 && Convert.ToInt16(args[1]) == 0)
					client.Player.IsOnHorse = false;
			}
			catch
			{
				DisplayMessage(client, "Incorrect format of the command");
			}
			finally
			{
				if (client.Account.PrivLevel > 1 && client.Player.IsOnHorse == false)
					client.Player.IsOnHorse = true;
			}
		}
	}
}