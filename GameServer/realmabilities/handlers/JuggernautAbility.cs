using System.Reflection;
using System.Collections;
using DOL.Database;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;

namespace DOL.GS.RealmAbilities
{
	public class JuggernautAbility : TimedRealmAbility
	{
		public JuggernautAbility(DBAbility dba, int level) : base(dba, level) { }

		int m_range = 1500;
		int m_duration = 60;
		byte m_value = 0;

		public override void Execute(GameLiving living)
		{
			GamePlayer player = living as GamePlayer;
			#region preCheck
			if (player == null)
			{
				log.Warn("Could not retrieve player in JuggernautAbilityHandler.");
				return;
			}

			if (!(player.IsAlive))
			{
				player.Out.SendMessage("You cannot use this ability while dead!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (player.IsMezzed)
			{
				player.Out.SendMessage("You cannot use this ability while mesmerized!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (player.IsStunned)
			{
				player.Out.SendMessage("You cannot use this ability while stunned!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (player.IsSitting)
			{
				player.Out.SendMessage("You cannot use this ability while sitting!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (player.ControlledNpc == null)
			{
				player.Out.SendMessage("You must have a pet controlled to use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (!player.TargetInView)
			{
				player.Out.SendMessage("Your target is not in view!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (!WorldMgr.CheckDistance(player.TargetObject, player, m_range))
			{
				player.Out.SendMessage("Your target is too far away!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}


			#endregion

			switch (this.Level)
			{
				case 1:
					m_value = 10;
					break;
				case 2:
					m_value = 20;
					break;
				case 3:
					m_value = 30;
					break;
				default:
					return;
			}

			new JuggernautEffect().Start(player, m_duration, m_value);

			DisableSkill(player);
		}

		public override int GetReUseDelay(int level)
		{
			return 900 * 1000;
		}
	}
}
