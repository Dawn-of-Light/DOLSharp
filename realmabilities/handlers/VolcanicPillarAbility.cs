using System;
using System.Collections;
using System.Reflection;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.GS.Spells;
using DOL.Events;
using DOL.Database;
namespace DOL.GS.RealmAbilities
{
	public class VolcanicPillarAbility : TimedRealmAbility
	{
		public VolcanicPillarAbility(DBAbility dba, int level) : base(dba, level) { }
		private int m_dmgValue = 0;
		private GamePlayer m_caster = null;

		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;
			m_caster = living as GamePlayer;
			if (m_caster == null)
				return;

			if (m_caster.TargetObject == null)
			{
				m_caster.Out.SendMessage("You need a target for this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				m_caster.DisableSkill(this, 3 * 1000);
				return;
			}

			if ( !m_caster.IsWithinRadius( m_caster.TargetObject, (int)( 1500 * m_caster.GetModified(eProperty.SpellRange) * 0.01 ) ) )
			{
				m_caster.Out.SendMessage(m_caster.TargetObject + " is too far away.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				return;
			}

			if(ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
			{
				switch (Level)
				{
					case 1: m_dmgValue = 200; break;
					case 2: m_dmgValue = 350; break;
					case 3: m_dmgValue = 500; break;
					case 4: m_dmgValue = 625; break;
					case 5: m_dmgValue = 750; break;
					default: return;
				}
			}
			else
			{
				switch (Level)
				{
					case 1: m_dmgValue = 200; break;
					case 2: m_dmgValue = 500; break;
					case 3: m_dmgValue = 750; break;
					default: return;
				}
			}

			foreach (GamePlayer i_player in m_caster.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
			{
				if (i_player == m_caster)
				{
					i_player.MessageToSelf("You cast " + this.Name + "!", eChatType.CT_Spell);
				}
				else
				{
					i_player.MessageFromArea(m_caster, m_caster.Name + " casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				}

				i_player.Out.SendSpellCastAnimation(m_caster, 7025, 20);
			}

			if (m_caster.RealmAbilityCastTimer != null)
			{
				m_caster.RealmAbilityCastTimer.Stop();
				m_caster.RealmAbilityCastTimer = null;
				m_caster.Out.SendMessage("You cancel your Spell!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
			}

			m_caster.RealmAbilityCastTimer = new RegionTimer(m_caster);
			m_caster.RealmAbilityCastTimer.Callback = new RegionTimerCallback(EndCast);
			m_caster.RealmAbilityCastTimer.Start(2000);
		}

		protected virtual int EndCast(RegionTimer timer)
		{
			if (m_caster.TargetObject == null)
			{
				m_caster.Out.SendMessage("You need a target for this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				m_caster.DisableSkill(this, 3 * 1000);
				return 0;
			}

            if ( !m_caster.IsWithinRadius( m_caster.TargetObject, (int)( 1500 * m_caster.GetModified( eProperty.SpellRange ) * 0.01 ) ) )
			{
				m_caster.Out.SendMessage(m_caster.TargetObject + " is too far away.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				return 0;
			}

			foreach (GamePlayer player in m_caster.TargetObject.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendSpellEffectAnimation(m_caster, (m_caster.TargetObject as GameLiving), 7025, 0, false, 1);
			}

			foreach (GameNPC mob in m_caster.TargetObject.GetNPCsInRadius(500))
			{
				if (!GameServer.ServerRules.IsAllowedToAttack(m_caster, mob, true))
					continue;

				mob.TakeDamage(m_caster, eDamageType.Heat, m_dmgValue, 0);
				m_caster.Out.SendMessage("You hit the " + mob.Name + " for " + m_dmgValue + " damage.", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
				foreach (GamePlayer player2 in m_caster.TargetObject.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					player2.Out.SendSpellEffectAnimation(m_caster, mob, 7025, 0, false, 1);
				}
			}

			foreach (GamePlayer aeplayer in m_caster.TargetObject.GetPlayersInRadius(500))
			{
				if (!GameServer.ServerRules.IsAllowedToAttack(m_caster, aeplayer, true))
					continue;

				aeplayer.TakeDamage(m_caster, eDamageType.Heat, m_dmgValue, 0);
				m_caster.Out.SendMessage("You hit " + aeplayer.Name + " for " + m_dmgValue + " damage.", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
				aeplayer.Out.SendMessage(m_caster.Name + " hits you for " + m_dmgValue + " damage.", eChatType.CT_YouWereHit, eChatLoc.CL_SystemWindow); 
				foreach (GamePlayer player3 in m_caster.TargetObject.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					player3.Out.SendSpellEffectAnimation(m_caster, aeplayer, 7025, 0, false, 1);
				}
			}

			DisableSkill(m_caster);
			timer.Stop();
			timer = null;
			return 0;
		}

		public override int GetReUseDelay(int level)
		{
			return 900;
		}
	}
}
