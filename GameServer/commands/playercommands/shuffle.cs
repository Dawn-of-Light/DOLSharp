using System;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&shuffle",
		ePrivLevel.Player,
		"Shuffle # of decks, minimum 1. Must be used before /deal.",
		"/shuffle <#>")]
	public class ShuffleCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
            if (args.Length < 2) return 0;
            int numDecks;
            try
            {
                numDecks = System.Convert.ToInt32(args[1]);
            }
            catch (Exception)
            {
                return 0;
            }
            if(numDecks > 0)
                CardMgr.Shuffle(client, (uint)numDecks);
            return 1;
		}
	}
}