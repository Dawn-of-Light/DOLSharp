using System;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&shuffle",
		ePrivLevel.Player,
		"Shuffle # of decks, minimum 1. Must be used before /deal.",
		"/shuffle <#>")]
	public class ShuffleCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 2)
				return;

			int numDecks;
			try
			{
				numDecks = System.Convert.ToInt32(args[1]);
			}
			catch (Exception)
			{
				return;
			}

			if (numDecks > 0)
				CardMgr.Shuffle(client, (uint)numDecks);
		}
	}
}