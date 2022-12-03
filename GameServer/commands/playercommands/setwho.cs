using System;
using DOL.GS.PacketHandler;
using DOL.Language;

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
				DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Setwho.Specify"));
				return;
			}

			if (args[1].ToLower() == "class")
				client.Player.ClassNameFlag = true;
			else if (args[1].ToLower() == "trade")
			{
				if (client.Player.CraftingPrimarySkill == eCraftingSkill.NoCrafting)
				{
					DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Setwho.Profession"));
					return;
				}

				client.Player.ClassNameFlag= false;
			}
			else
			{
				DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Setwho.Specify"));
				return;
			}

			if (client.Player.ClassNameFlag)
				DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Setwho.CraftingOff"));
			else
				DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Setwho.CraftingOn"));
		}
	}
}