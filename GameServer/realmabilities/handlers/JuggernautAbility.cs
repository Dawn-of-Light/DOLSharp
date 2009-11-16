using System.Reflection;
using System.Collections;
using DOL.Database;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.GS.Spells;

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
			if (player.ControlledNpcBrain == null)
			{
				player.Out.SendMessage("You must have a pet controlled to use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (!player.IsWithinRadius( player.ControlledNpcBrain.Body, m_range ))
			{
				player.Out.SendMessage("Your pet is too far away!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
            GameSpellEffect ml9=SpellHandler.FindEffectOnTarget(player.ControlledNpcBrain.Body,"SummonMastery");
            if (ml9 != null)
            {
                player.Out.SendMessage("Your Pet already has an ability of this type active", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
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
			return 900;
		}
	}
}
