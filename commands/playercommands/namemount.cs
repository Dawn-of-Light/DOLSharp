using System;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[CmdAttribute("&namemount", ePrivLevel.Player,"Name your hourse","/namemount")]
	public class NameHorseCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (client.Player == null || args.Length < 2)
				return;
			string horseName = args[1];
			if (!client.Player.HasHorse)
				return;
			client.Player.ActiveHorse.Name = horseName;
		}
	}
}