using System;
using System.Collections;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;
 
namespace DOL.GS.Spells
{
	/// <summary>
	/// Summary description for RangeShield.
	/// </summary>
	[SpellHandlerAttribute("RangeShield")]
	public class RangeShield : BladeturnSpellHandler 
	{
        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);
            GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttacked));
        }
        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            GameEventMgr.RemoveHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttacked));
            return base.OnEffectExpires(effect, noMessages);
        }
        protected virtual void OnAttacked(DOLEvent e, object sender, EventArgs arguments)
        {
            AttackedByEnemyEventArgs attackArgs = arguments as AttackedByEnemyEventArgs;
            GameLiving living = sender as GameLiving;
            if (attackArgs == null) return;
            if (living == null) return;
            double value = 0;
            switch (attackArgs.AttackData.AttackType)
            {
                case AttackData.eAttackType.Ranged:
                    value = Spell.Value * .01;
                    attackArgs.AttackData.Damage *= (int)value;
                    break;
                case AttackData.eAttackType.Spell:
                    if (attackArgs.AttackData.SpellHandler.Spell.SpellType == "Archery")
                    {
                        value = Spell.Value * .01;
                        attackArgs.AttackData.Damage *= (int)value;
                    }
                    break;
            }
        }
		public RangeShield(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}
