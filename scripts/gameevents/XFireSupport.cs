using System;
using System.Reflection;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.Scripts;

namespace DOL.GS.GameEvents
{
	public class XFirePlayerEnterExit
	{
		private const string XFire_Property_Flag = "XFire_Property_Flag";

		[GameServerStartedEvent]
		public static void OnServerStart(DOLEvent e, object sender, EventArgs arguments)
		{
			GameEventMgr.AddHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEntered));
			GameEventMgr.AddHandler(GamePlayerEvent.Quit, new DOLEventHandler(PlayerQuit));
		}

		[GameServerStoppedEvent]
		public static void OnServerStop(DOLEvent e, object sender, EventArgs arguments)
		{
			GameEventMgr.RemoveHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEntered));
			GameEventMgr.RemoveHandler(GamePlayerEvent.Quit, new DOLEventHandler(PlayerQuit));
		}

		private static void PlayerEntered(DOLEvent e, object sender, EventArgs arguments)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null) return;
			//			if (player.IsAnonymous) return; TODO check /anon and xfire
			byte flag = 0;
			if (player.PlayerCharacter.ShowXFireInfo)
				flag = 1;
			player.Out.SendXFireInfo(flag);
		}

		private static void PlayerQuit(DOLEvent e, object sender, EventArgs arguments)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null) return;

			player.Out.SendXFireInfo(0);
		}
	}

namespace DOL.GS
{
	namespace Scripts
	{
		[CmdAttribute("&xfire", (uint) ePrivLevel.Player, "Xfire support", "/xfire <on|off>")]
		public class CheckXFireCommandHandler : ICommandHandler
		{
			public int OnCommand(GameClient client, string[] args)
			{
				if (client.Player == null || args.Length < 2)
					return 1;
				byte flag = 0;
				if (args[1].ToLower().Equals("on"))
				{
					client.Player.PlayerCharacter.ShowXFireInfo = true;
					client.Out.SendMessage("Your XFire flag is ON. Your character data will be sent to the XFire service ( if you have XFire installed ). Use '/xfire off' to disable sending character data to the XFire service.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					flag = 1;
				}
				else if (args[1].ToLower().Equals("off"))
				{
					client.Player.PlayerCharacter.ShowXFireInfo = false;
					client.Out.SendMessage("Your XFire flag is OFF. TODO correct message.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
				}
				else
					return 1;// not have clue what message for wrong params or help
				client.Player.Out.SendXFireInfo(flag);
				return 1;
			}
		}
	}
}
}
