namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&held",
		(uint) ePrivLevel.Player,
		"List the cards in your hand. Use 'held g' to display faceup cards held by group members.",
		"/held <g>")]
	public class HeldCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
            if (args.Length == 2 && client.Player.PlayerGroup != null)
                foreach (GamePlayer Groupee in client.Player.PlayerGroup.GetPlayersInTheGroup())
                    if(Groupee != client.Player) CardMgr.Held(client, Groupee.Client);
            else
                CardMgr.Held(client, client);
            return 1;
		}
	}
}