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
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public JuggernautAbility(DBAbility dba, int level) : base(dba, level) { }

		int m_range = 1500;
		int m_duration = 60;
		byte m_value = 0;

		public override void Execute(GameLiving living)
		{
			GamePlayer player = living as GamePlayer;
			#region preCheck
			if (living == null)
			{
				log.Warn("Could not retrieve player in JuggernautAbilityHandler.");
				return;
			}

			if (!(living.IsAlive))
			{
				if(player != null)
					player.Out.SendMessage("You cannot use this ability while dead!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (living.IsMezzed)
			{
				if(player != null)
					player.Out.SendMessage("You cannot use this ability while mesmerized!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (living.IsStunned)
			{
				if(player != null)
					player.Out.SendMessage("You cannot use this ability while stunned!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (living.IsSitting)
			{
				if(player != null)
					player.Out.SendMessage("You cannot use this ability while sitting!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (living.ControlledBrain == null)
			{
				if(player != null)
					player.Out.SendMessage("You must have a pet controlled to use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (!living.IsWithinRadius( player.ControlledBrain.Body, m_range ))
			{
				if(player != null)
					player.Out.SendMessage("Your pet is too far away!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
            GameSpellEffect ml9=SpellHandler.FindEffectOnTarget(living.ControlledBrain.Body,"SummonMastery");
            if (ml9 != null)
            {
				if(player != null)
	                player.Out.SendMessage("Your Pet already has an ability of this type active", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                return;
            }

			#endregion

			if(ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
			{
				switch (this.Level)
				{
					case 1:
						m_value = 10;
						break;
					case 2:
						m_value = 15;
						break;
					case 3:
						m_value = 20;
						break;
					case 4:
						m_value = 25;
						break;
					case 5:
						m_value = 30;
						break;
					default:
						return;
				}
			}
			else
			{
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
			}

			new JuggernautEffect().Start(living, m_duration, m_value);

			DisableSkill(living);
		}

		public override int GetReUseDelay(int level)
		{
			return 900;
		}
	}
}
