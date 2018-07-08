// Andraste v2.0 - Vico

namespace DOL.GS.Spells
{
    [SpellHandler("ArrowDamageTypes")]
    public class ArrowDamageTypes : SpellHandler
    {
        /// <summary>
        /// Does this spell break stealth on finish?
        /// </summary>
        public override bool UnstealthCasterOnFinish => false;

        /// <summary>
        /// Does this spell break stealth on start?
        /// </summary>
        public override bool UnstealthCasterOnStart => false;

        public ArrowDamageTypes(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}