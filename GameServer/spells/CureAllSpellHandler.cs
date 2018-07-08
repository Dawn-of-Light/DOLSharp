using System.Collections.Generic;

namespace DOL.GS.Spells
{
    /// <summary>
    ///
    /// </summary>
    [SpellHandler("CureAll")]
    public class CureAllSpellHandler : RemoveSpellEffectHandler
    {
        // constructor
        public CureAllSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            SpellTypesToRemove = new List<string>
            {
                "DamageOverTime",
                "Nearsight",
                "Silence",
                "Disease",
                "StyleBleeding"
            };
        }
    }
}