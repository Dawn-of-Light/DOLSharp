using System;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&setwho",
		ePrivLevel.Player,
		"Set your class or trade for /who output",
		"/setwho class | trade")]
	public class SetWhoCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (IsSpammingCommand(client.Player, "setwho"))
				return;

			if (args.Length < 2)
			{
				DisplayMessage(client, "You need to specify if you want to change to class or trade");
				return;
			}

			if (args[1].ToLower() == "class")
				client.Player.ClassNameFlag = true;
			else if (args[1].ToLower() == "trade")
			{
				if (client.Player.CraftingPrimarySkill == eCraftingSkill.NoCrafting)
				{
					DisplayMessage(client, "You need a profession to enable it in for who messages");
					return;
				}

				client.Player.ClassNameFlag= false;
			}
			else
			{
				DisplayMessage(client, "You need to specify if you want to change to class or trade");
				return;
			}

			if (client.Player.ClassNameFlag)
				DisplayMessage(client, "/who will no longer show your crafting title");
			else
				DisplayMessage(client, "/who will now show your crafting title");
		}
	}
}