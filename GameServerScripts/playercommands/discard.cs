using System;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&discard",
		(uint) ePrivLevel.Player,
		"Discard card # from your hand, or discard all cards.",
		"/discard <#|all>")]
	public class DiscardCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
            if(args.Length < 2) return 0;
            if (args[1].Equals("all"))
                CardMgr.DiscardAll(client);
            else
            {
                try
                {
                    uint cardId = System.Convert.ToUInt32(args[1]);
                    CardMgr.Discard(client, cardId);
                }
                catch (Exception)
                {
                    return 0;
                }
            }
            return 1;
		}
	}
}