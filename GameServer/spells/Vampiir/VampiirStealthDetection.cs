using System;
using System.Collections;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	[SpellHandlerAttribute("VampiirStealthDetection")]
	public class VampiirStealthDetection : SpellHandler
	{
		public VampiirStealthDetection(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= PowerCost(target);
			base.FinishSpellCast(target);
		}

		public override void OnEffectStart(GameSpellEffect effect)
		{

			effect.Owner.BaseBuffBonusCategory[(int)eProperty.Skill_Stealth]+=(int)m_spell.Value;
			base.OnEffectStart(effect);
	//		effect.Owner.BuffBonusCategory1[(int)eProperty.StealthRange] += (int)m_spell.Value;
		}


		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
	//		effect.Owner.BuffBonusCategory1[(int)eProperty.StealthRange] -= (int)m_spell.Value;
			effect.Owner.BaseBuffBonusCategory[(int)eProperty.Skill_Stealth]-=(int)m_spell.Value;
			return base.OnEffectExpires(effect, noMessages);
		}


	}
}























