using System;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;

namespace DOL.GS.Spells
{
    [SpellHandlerAttribute("ShatterIllusions")]
	public class ShatterIllusions : SpellHandler
	{
        //Shatter Illusions 
        //(returns the enemy from their shapeshift forms 
        //causing 200 body damage to the enemy. Range: 1500) 
        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            AttackData ad = CalculateDamageToTarget(target, effectiveness);
            base.OnDirectEffect(target, effectiveness);
            foreach (GameSpellEffect effect in target.EffectList.GetAllOfType(typeof(GameSpellEffect)))
            {
                if (effect.SpellHandler.Spell.SpellType.Equals("ShadesOfMist") ||
                    effect.SpellHandler.Spell.SpellType.Equals("TraitorsDaggerProc") ||
                    effect.SpellHandler.Spell.SpellType.Equals("DreamMorph") ||
                    effect.SpellHandler.Spell.SpellType.Equals("DreamGroupMorph") ||
                    effect.SpellHandler.Spell.SpellType.Equals("MaddeningScalars") ||
                    effect.SpellHandler.Spell.SpellType.Equals("AtlantisTabletMorph") ||
                    effect.SpellHandler.Spell.SpellType.Equals("AlvarusMorph"))
                {
                    ad.Damage = (int)Spell.Damage;
                    effect.Cancel(false);
                    SendEffectAnimation(target, 0, false, 1);
                    SendDamageMessages(ad);
                    DamageTarget(ad);
                    return;
                }
            }
        }
        public virtual void DamageTarget(AttackData ad)
        {
            ad.AttackResult = GameLiving.eAttackResult.HitUnstyled;
            ad.Target.OnAttackedByEnemy(ad);
            ad.Attacker.DealDamage(ad);
            foreach (GamePlayer player in ad.Attacker.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                player.Out.SendCombatAnimation(null, ad.Target, 0, 0, 0, 0, 0x0A, ad.Target.HealthPercent);
            }
        }
        public ShatterIllusions(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}
}