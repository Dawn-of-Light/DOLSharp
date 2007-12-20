using System;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&setwho",
		ePrivLevel.Player,
		"Set your class or trade for /who output",
		"/setwho class | trade")]
	public class SetWhoCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 2)
			{
				client.Out.SendMessage("You need to specify if you want to change to class or trade", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}
			if (args[1].ToLower() == "class")
				client.Player.ClassNameFlag = true;
			else if (args[1].ToLower() == "trade")
			{
				if (client.Player.CraftingPrimarySkill == eCraftingSkill.NoCrafting)
				{
					client.Out.SendMessage("You need a profession to enable it in for who messages", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 1;
				}
				client.Player.ClassNameFlag= false;
			}
			else
			{
				client.Out.SendMessage("You need to specify if you want to change to class or trade", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}

			if (client.Player.ClassNameFlag)
				client.Out.SendMessage("/who will no longer show your crafting title", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			else
				client.Out.SendMessage("/who will now show your crafting title", eChatType.CT_System, eChatLoc.CL_SystemWindow);


			return 1;
		}
	}
}