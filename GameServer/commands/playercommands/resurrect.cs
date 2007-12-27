using DOL.Events;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&resurrect",
		ePrivLevel.Player,
		"Used for self resurrection with Sputin's Legacy", "/resurrect")]
	public class ResurrectCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		private RegionTimer m_timer = null;

		public void OnCommand(GameClient client, string[] args)
		{
			if (client.Player.IsAlive)
			{
				DisplayMessage(client, "You're not dead!");
				return;
			}
			if (!client.Player.TempProperties.getProperty(Effects.SputinsLegacyEffect.SPUTINSLEGACYHASRES, false))
			{
				DisplayMessage(client, "You have no active Self Ressurection!");
				return;
			}
			if (m_timer != null && m_timer.IsAlive)
				return;
			m_timer = new RegionTimer(client.Player);
			m_timer.Callback = new RegionTimerCallback(OnTick);
			m_timer.Properties.setProperty("SELF_RES_PLAYER", client.Player);
			m_timer.Start(10000);
			DisplayMessage(client, "You will resurrect in 10 seconds!");
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