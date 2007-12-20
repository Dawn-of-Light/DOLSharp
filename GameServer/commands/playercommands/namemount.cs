using System;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[CmdAttribute("&namemount", ePrivLevel.Player,"Name your hourse","/namemount")]
	public class NameHorseCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (client.Player == null || args.Length < 2)
				return 1;
			string horseName = args[1];
			if (!client.Player.HasHorse)
				return 1;
			client.Player.ActiveHorse.Name = horseName;
			return 1;
		}
	}
}