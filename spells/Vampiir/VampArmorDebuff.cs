using System;
using System.Collections;
using DOL.AI.Brain;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	[SpellHandlerAttribute("VampiirArmorDebuff")]
	public class VampiirArmorDebuff : SpellHandler
	{
		private static eArmorSlot[] slots = new eArmorSlot[] { eArmorSlot.HEAD, eArmorSlot.LEGS, eArmorSlot.TORSO };

		private eArmorSlot m_slot = eArmorSlot.UNKNOWN;
		public eArmorSlot Slot
		{
			get { return m_slot; }
		}

		/// <summary>
		/// called after normal spell cast is completed and effect has to be started
		/// </summary>
		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= CalculateNeededPower(target);
			base.FinishSpellCast(target);
		}


		public override void OnEffectStart(GameSpellEffect effect)
		{
			effect.Owner.StartInterruptTimer(SPELL_INTERRUPT_DURATION, AttackData.eAttackType.Spell, Caster);
			GamePlayer player = effect.Owner as GamePlayer;
			if (player == null) return;
			m_slot = slots[Util.Random(0, 2)];
			string msg = GlobalConstants.SlotToName((int)m_slot);
			MessageToCaster("You debuff " + effect.Owner.Name + "'s " + msg, eChatType.CT_Spell);
			base.OnEffectStart(effect);
		}

		// constructor
		public VampiirArmorDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}
}