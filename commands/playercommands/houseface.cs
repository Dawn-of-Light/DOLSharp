using DOL.GS;
using DOL.GS.Housing;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[CmdAttribute(
	  "&houseface",
	  ePrivLevel.Player,
	  "Points to the specified guildhouse of the guild noted, or the lot number noted in the command. /houseface alone will point to one's personal home.")]
	public class HouseFaceCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			int housenumber = 0;
			if (args.Length > 1)
			{
				try
				{
					housenumber = int.Parse(args[1]);
				}
				catch
				{
					DisplaySyntax(client);
					return;
				}
			}
			else HouseMgr.GetHouseNumberByPlayer(client.Player);

			if (housenumber == 0)
			{
				DisplayMessage(client, "No house found.");
				return;
			}

			House house = HouseMgr.GetHouse(housenumber);

			ushort direction = client.Player.GetHeading(house);
			client.Player.Heading = direction;
			client.Out.SendPlayerJump(true);
			DisplayMessage(client, "You face house " + housenumber);
		}
	}
}