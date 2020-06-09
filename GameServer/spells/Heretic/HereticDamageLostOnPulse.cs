using System;

namespace DOL.GS.Spells
{
    [Obsolete("HereticDoTLostOnPulse will be removed. Use RampingDamageFocus instead!")]
    [SpellHandler("HereticDoTLostOnPulse")]
    public class HereticDoTLostOnPulse : RampingDamageFocus
    {
        public HereticDoTLostOnPulse(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) { }
    }
}
