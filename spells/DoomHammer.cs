//Eden - Darwin

using System;
using System.Reflection;
using DOL.Events;
using DOL.GS;
using DOL.GS.PacketHandler;
using log4net;
using System.Collections;
using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.Spells;

namespace DOL.GS.Spells
{
	[SpellHandlerAttribute("DoomHammer")]
	public class DoomHammerSpellHandler : DirectDamageSpellHandler
	{
		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			if(Caster.IsDisarmed)
			{
				MessageToCaster("You are disarmed and can't use this spell!",eChatType.CT_SpellResisted);
				return false;
			}
			return base.CheckBeginCast(selectedTarget);
		}
		public override double CalculateDamageBase() { return Spell.Damage; }
		public override void ApplyEffectOnTarget(GameLiving target,double effectiveness)
		{
			GamePlayer player=target as GamePlayer;
			base.ApplyEffectOnTarget(Caster,effectiveness);
			Caster.StopAttack();
            Caster.DisarmedTime = Caster.CurrentRegion.Time + Spell.Duration;
			foreach (GamePlayer visPlayer in Caster.GetPlayersInRadius((ushort)WorldMgr.VISIBILITY_DISTANCE))
				visPlayer.Out.SendCombatAnimation(Caster, target, 0x0000, 0x0000, (ushort)408, 0, 0x00, target.HealthPercent);
			if(Spell.ResurrectMana>0) foreach (GamePlayer visPlayer in target.GetPlayersInRadius((ushort)WorldMgr.VISIBILITY_DISTANCE))
				visPlayer.Out.SendSpellEffectAnimation(Caster, target, (ushort)Spell.ResurrectMana, 0, false, 0x01);
			
			if((Spell.Duration>0&&Spell.Target!="Area")||Spell.Concentration>0) OnDirectEffect(target,effectiveness);
		}
		public override int OnEffectExpires(GameSpellEffect effect,bool noMessages)
		{
			//Caster.IsDisarmed=false;
			return base.OnEffectExpires(effect,noMessages);
		}
		public DoomHammerSpellHandler(GameLiving caster,Spell spell,SpellLine line) : base(caster,spell,line) {}
	}
}