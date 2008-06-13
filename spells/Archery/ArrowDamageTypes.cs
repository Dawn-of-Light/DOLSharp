//Andraste v2.0 - Vico

using System;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;

namespace DOL.GS.Spells
{
    [SpellHandler("ArrowDamageTypes")]
    public class ArrowDamageTypes : SpellHandler
	{
        public ArrowDamageTypes(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}