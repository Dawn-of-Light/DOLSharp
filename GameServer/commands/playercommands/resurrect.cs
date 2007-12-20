using DOL.Events;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&resurrect",
		ePrivLevel.Player,
		"Used for self resurrection with Sputin's Legacy", "/resurrect")]
	public class ResurrectCommandHandler : ICommandHandler
	{
		private RegionTimer m_timer = null;

		public int OnCommand(GameClient client, string[] args)
		{
			if (client.Player.IsAlive)
			{
				client.Out.SendMessage("You're not dead!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 0;
			}
			if (!client.Player.TempProperties.getProperty(Effects.SputinsLegacyEffect.SPUTINSLEGACYHASRES, false))
			{
				client.Out.SendMessage("You have no active Self Ressurection!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 0;
			}
			if (m_timer != null && m_timer.IsAlive)
				return 0;
			m_timer = new RegionTimer(client.Player);
			m_timer.Callback = new RegionTimerCallback(OnTick);
			m_timer.Properties.setProperty("SELF_RES_PLAYER", client.Player);
			m_timer.Start(10000);
			client.Out.SendMessage("You will resurrect in 10 seconds!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			return 1;
		}

		private int OnTick(RegionTimer timer)
		{
			GamePlayer player = (GamePlayer)timer.Properties.getObjectProperty("SELF_RES_PLAYER", null);
			if (player == null)
				return 0;
			player.TempProperties.removeProperty(Effects.SputinsLegacyEffect.SPUTINSLEGACYHASRES);
			if (player.IsAlive)
				return 0;
			if (player.TempProperties.getProperty("RESSURECT_CASTER", null) != null)
				player.TempProperties.removeProperty("RESURRECT_CASTER");

			player.Health = player.MaxHealth * 10 / 100;
			player.Mana = player.MaxMana * 10 / 100;
			player.StopReleaseTimer();
			player.Out.SendPlayerRevive(player);
			player.UpdatePlayerStatus();
			player.Notify(GamePlayerEvent.Revive, player);

			return 0;

		}
	}
}