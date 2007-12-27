using DOL.GS;
using DOL.GS.Housing;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS.Commands
{
	[CmdAttribute(
	  "&boot",
	  ePrivLevel.Player,
	   "Kicks a player out of your house",
		 "Useage: /boot [playername]")]
	public class BootCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			House house = HouseMgr.GetHouseByPlayer(client.Player);
			if (house == null)
			{
				DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Boot.NoHouseError"));
				return;
			}

			if (!client.Player.InHouse || client.Player.CurrentHouse != house)
			{
				DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Boot.InHouseError"));
				return;
			}

			foreach (GamePlayer player in house.GetAllPlayersInHouse())
			{
				if (player != client.Player && player.Name.IndexOf(args[1]) > -1)
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Boot.YouRemoved", client.Player.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					player.LeaveHouse();
					return;
				}
			}

			client.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Boot.NoOneOnline"), eChatType.CT_Help, eChatLoc.CL_SystemWindow);
		}
	}
}