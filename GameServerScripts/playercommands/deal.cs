using DOL.GS;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&deal",
		(uint) ePrivLevel.Player,
		"Deal one card to a player in your group. Cards must first be prepared with /shuffle.",
		"/deal <name> <u/d>")]
	public class DealCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{           
            if(args.Length < 3) return 0;
            bool up = false;
            GameClient friendClient = WorldMgr.GetClientByPlayerName(args[1], true, true);
            if (args[2][0] == 'u') up = true;
            else if (args[2][0] == 'd') up = false;
            else return 0;
            CardMgr.Deal(client, friendClient, up);
            return 1;
		}
	}
}