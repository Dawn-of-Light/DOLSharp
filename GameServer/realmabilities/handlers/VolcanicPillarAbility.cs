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
		private int dmgValue = 0;
		private GamePlayer caster = null;

		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;
			caster = living as GamePlayer;
			if (caster == null)
				return;

			if (caster.TargetObject == null)
			{
				caster.Out.SendMessage("You need a target for this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				caster.DisableSkill(this, 3 * 1000);
				return;
			}

			if ( !caster.IsWithinRadius( caster.TargetObject, (int)( 1500 * caster.GetModified(eProperty.SpellRange) * 0.01 ) ) )
			{
				caster.Out.SendMessage(caster.TargetObject + " is too far away.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				return;
			}

			switch (Level)
			{
				case 1: dmgValue = 200; break;
				case 2: dmgValue = 500; break;
				case 3: dmgValue = 750; break;
				default: return;
			}

			foreach (GamePlayer i_player in caster.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
			{
				if (i_player == caster) i_player.Out.SendMessage("You cast " + this.Name + "!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				else i_player.Out.SendMessage(caster.Name + " casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);

				i_player.Out.SendSpellCastAnimation(caster, 7025, 20);
			}

			if (caster.RealmAbilityCastTimer != null)
			{
				caster.RealmAbilityCastTimer.Stop();
				caster.RealmAbilityCastTimer = null;
				caster.Out.SendMessage("You cancel your Spell!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
			}

			caster.RealmAbilityCastTimer = new RegionTimer(caster);
			caster.RealmAbilityCastTimer.Callback = new RegionTimerCallback(EndCast);
			caster.RealmAbilityCastTimer.Start(2000);
		}

		protected virtual int EndCast(RegionTimer timer)
		{
			if (caster.TargetObject == null)
			{
				caster.Out.SendMessage("You need a target for this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				caster.DisableSkill(this, 3 * 1000);
				return 0;
			}

            if ( !caster.IsWithinRadius( caster.TargetObject, (int)( 1500 * caster.GetModified( eProperty.SpellRange ) * 0.01 ) ) )
			{
				caster.Out.SendMessage(caster.TargetObject + " is too far away.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				return 0;
			}

			foreach (GamePlayer player in caster.TargetObject.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendSpellEffectAnimation(caster, (caster.TargetObject as GameLiving), 7025, 0, false, 1);
			}

			foreach (GameNPC mob in caster.TargetObject.GetNPCsInRadius(500))
			{
				if (!GameServer.ServerRules.IsAllowedToAttack(caster, mob, true))
					continue;

				mob.TakeDamage(caster, eDamageType.Heat, dmgValue, 0);
				caster.Out.SendMessage("You hit the " + mob.Name + " for " + dmgValue + " damage.", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
				foreach (GamePlayer player2 in caster.TargetObject.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					player2.Out.SendSpellEffectAnimation(caster, mob, 7025, 0, false, 1);
				}
			}

			foreach (GamePlayer aeplayer in caster.TargetObject.GetPlayersInRadius(500))
			{
				if (!GameServer.ServerRules.IsAllowedToAttack(caster, aeplayer, true))
					continue;

				aeplayer.TakeDamage(caster, eDamageType.Heat, dmgValue, 0);
				caster.Out.SendMessage("You hit " + aeplayer.Name + " for " + dmgValue + " damage.", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
				aeplayer.Out.SendMessage(caster.Name + " hits you for " + dmgValue + " damage.", eChatType.CT_YouWereHit, eChatLoc.CL_SystemWindow); 
				foreach (GamePlayer player3 in caster.TargetObject.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					player3.Out.SendSpellEffectAnimation(caster, aeplayer, 7025, 0, false, 1);
				}
			}

			DisableSkill(caster);
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
