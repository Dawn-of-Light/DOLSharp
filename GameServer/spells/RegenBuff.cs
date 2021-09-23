/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
using System.Collections;
using System.Collections.Specialized;
using DOL.GS.Effects;
using DOL.GS.PropertyCalc;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	[SpellHandler("HealthRegenBuff")]
	public class HealthRegenSpellHandler : PropertyChangingSpell
	{
		public override eBuffBonusCategory BonusCategory1 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eProperty Property1 { get { return eProperty.HealthRegenerationRate; } }

		public HealthRegenSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}

		public override string ShortDescription => $"Target regenerates {Spell.Value} extra health.";
	}

	[SpellHandler("PowerRegenBuff")]
	public class PowerRegenSpellHandler : PropertyChangingSpell
	{
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
            if (target is GamePlayer && (((GamePlayer)target).CharacterClass.ID == (int)eCharacterClass.Vampiir
                || ((GamePlayer)target).CharacterClass.ID == (int)eCharacterClass.MaulerAlb
                || ((GamePlayer)target).CharacterClass.ID == (int)eCharacterClass.MaulerMid
                || ((GamePlayer)target).CharacterClass.ID == (int)eCharacterClass.MaulerHib)) { MessageToCaster("This spell has no effect on this class!", eChatType.CT_Spell); return; }
			base.ApplyEffectOnTarget(target, effectiveness);
		}
		public override eBuffBonusCategory BonusCategory1 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eProperty Property1 { get { return eProperty.PowerRegenerationRate; } }

		public PowerRegenSpellHandler(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) {}

		public override string ShortDescription => $"Target regenerates {Spell.Value} extra power.";

	}

	[SpellHandler("EnduranceRegenBuff")]
	public class EnduranceRegenSpellHandler : PropertyChangingSpell
	{
		public override eBuffBonusCategory BonusCategory1 { get { return eBuffBonusCategory.BaseBuff; } }
		public override eProperty Property1 { get { return eProperty.EnduranceRegenerationRate; } }

		private const int CONC_MAX_RANGE = 1500;

		private const int RANGE_CHECK_INTERVAL = 5000;

		private ListDictionary m_concEffects;

		public override void FinishSpellCast(GameLiving target)
		{
			if (Spell.Concentration > 0)
			{
				m_concEffects = new ListDictionary();
				RangeCheckAction rangeCheck = new RangeCheckAction(Caster, this);
				rangeCheck.Interval = RANGE_CHECK_INTERVAL;
				rangeCheck.Start(RANGE_CHECK_INTERVAL);
			}
			base.FinishSpellCast(target);
		}

		public override bool StartSpell(GameLiving target)
		{
			// paladin chants seem special
			if (SpellLine.Spec == Specs.Chants)
				SendEffectAnimation(Caster, 0, true, 1);

			return base.StartSpell(target);
		}

		public override void OnEffectStart(GameSpellEffect effect)
		{
			base.OnEffectStart(effect);
			if (Spell.Concentration > 0) {
				lock (m_concEffects) {
					m_concEffects.Add(effect, effect); // effects are enabled at start
				}
			}
		}

		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			if (Spell.Concentration > 0) {
				lock (m_concEffects) {
					EnableEffect(effect); // restore disabled effect before it is completely canceled
					m_concEffects.Remove(effect);
				}
			}
			return base.OnEffectExpires(effect, noMessages);
		}

		private void EnableEffect(GameSpellEffect effect)
		{
			if (m_concEffects[effect] != null) return; // already enabled
			m_concEffects[effect] = effect;
			IPropertyIndexer bonuscat = GetBonusCategory(effect.Owner, BonusCategory1);
			bonuscat[(int)Property1] += (int)(Spell.Value * effect.Effectiveness);
		}

		private void DisableEffect(GameSpellEffect effect)
		{
			if (m_concEffects[effect] == null) return; // already disabled
			m_concEffects[effect] = null;
			IPropertyIndexer bonuscat = GetBonusCategory(effect.Owner, BonusCategory1);
			bonuscat[(int)Property1] -= (int)(Spell.Value * effect.Effectiveness);
		}

		private sealed class RangeCheckAction : RegionAction
		{
			private readonly EnduranceRegenSpellHandler m_handler;

			public RangeCheckAction(GameLiving actionSource, EnduranceRegenSpellHandler handler) : base(actionSource)
			{
				m_handler = handler;
			}

			protected override void OnTick()
			{
				IDictionary effects = m_handler.m_concEffects;
				GameLiving caster = (GameLiving)m_actionSource;
				lock (effects)
				{
					if (effects.Count <= 0)
					{
						Stop(); // all effects were canceled, stop the timer
						return;
					}

					ArrayList disableEffects = null;
					ArrayList enableEffects = null;
					foreach (DictionaryEntry de in effects)
					{
						GameSpellEffect effect = (GameSpellEffect)de.Key;
						GameLiving effectOwner = effect.Owner;
						if (caster == effectOwner) continue;
//						DOLConsole.WriteLine(Caster.Name+" to "+effectOwner.Name+" range="+range);

						if (!caster.IsWithinRadius(effectOwner, CONC_MAX_RANGE))
						{
							if (de.Value != null)
							{
								// owner is out of range and effect is still active, disable it
								if (disableEffects == null)
									disableEffects = new ArrayList(1);
								disableEffects.Add(effect);
							}
						}
						else if (de.Value == null)
						{
							// owner entered the range and effect was disabled, enable it now
							if (enableEffects == null)
								enableEffects = new ArrayList(1);
							enableEffects.Add(effect);
						}
					}

					if (enableEffects != null)
						foreach (GameSpellEffect fx in enableEffects)
							m_handler.EnableEffect(fx);

					if (disableEffects != null)
						foreach (GameSpellEffect fx in disableEffects)
							m_handler.DisableEffect(fx);
				}
			}
		}

		public EnduranceRegenSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}

		public override string ShortDescription => $"Target regenerates {Spell.Value} extra fatigue.";
	}
}
