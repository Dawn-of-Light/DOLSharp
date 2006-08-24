using System;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[CmdAttribute("&summon", (uint)ePrivLevel.Player,"Summon horse","/summon")]
	public class SummonHorseCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (client.Player == null)
				return 1;
			if (args.Length > 1 && Convert.ToInt16(args[1]) == 0)
				client.Player.IsOnHorse = false;
			else if (client.Account.PrivLevel > 1)
				client.Player.IsOnHorse = true;
			return 1;
		}
	}
}