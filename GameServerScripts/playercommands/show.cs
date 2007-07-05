namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&show",
		(uint) ePrivLevel.Player,
		"Show all your cards to the other players (all cards become 'up').",
		"/show")]
	public class ShowCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
            CardMgr.Show(client);
            return 1;
		}
	}
}