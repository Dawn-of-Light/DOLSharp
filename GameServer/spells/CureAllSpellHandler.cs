using System;
using System.Collections.Generic;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	/// <summary>
	///
	/// </summary>
	[SpellHandlerAttribute("CureAll")]
	public class CureAllSpellHandler : RemoveSpellEffectHandler
	{
		// constructor
		public CureAllSpellHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{
			m_spellTypesToRemove = new List<string>();
			m_spellTypesToRemove.Add("DamageOverTime");
			m_spellTypesToRemove.Add("Nearsight");
			m_spellTypesToRemove.Add("Disease");
            m_spellTypesToRemove.Add("StyleBleeding");
		}
	}
}