using System;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&discard",
		ePrivLevel.Player,
		"Discard card # from your hand, or discard all cards.",
		"/discard <#|all>")]
	public class DiscardCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 2) return;
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
					return;
                }
            }
		}
	}
}