using System;

namespace DOL.GS.Spells
{
	[Obsolete("HereticDamageSpeedDecreaseLOP will be removed. Use RampingDamageFocus instead!")]
	[SpellHandler("HereticDamageSpeedDecreaseLOP")]
	public class HereticDamageSpeedDecreaseLostOnPulse : RampingDamageFocus
	{
		public HereticDamageSpeedDecreaseLostOnPulse(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}
