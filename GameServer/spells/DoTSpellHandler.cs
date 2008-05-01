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
using System;
using System.Collections;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Damage Over Time spell handler
	/// </summary>
	[SpellHandlerAttribute("DamageOverTime")]
	public class DoTSpellHandler : SpellHandler
	{
		/// <summary>
		/// Execute damage over time spell
		/// </summary>
		/// <param name="target"></param>
		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= CalculateNeededPower(target);
			base.FinishSpellCast(target);
		}

		public override double GetLevelModFactor()
		{
			return 0;
		}

		/// <summary>
		/// There is no area variance for dots
		/// </summary>
		/// <param name="distance"></param>
		/// <param name="radius"></param>
		/// <returns></returns>
		protected override double CalculateAreaVariance(int distance, int radius)
		{
			return 0;
		}

		/// <summary>
		/// Determines wether this spell is compatible with given spell
		/// and therefore overwritable by better versions
		/// spells that are overwritable cannot stack
		/// </summary>
		/// <param name="compare"></param>
		/// <returns></returns>
		public override bool IsOverwritable(GameSpellEffect compare)
		{
			return Spell.SpellType == compare.Spell.SpellType && Spell.DamageType == compare.Spell.DamageType && SpellLine.IsBaseLine == compare.SpellHandler.SpellLine.IsBaseLine;
		}

		/// <summary>
		/// Calculates damage to target with resist chance and stores it in ad
		/// </summary>
		/// <param name="target">spell target</param>
		/// <param name="effectiveness">value from 0..1 to modify damage</param>
		/// <returns>attack data</returns>
		public override AttackData CalculateDamageToTarget(GameLiving target, double effectiveness)
		{
			AttackData ad = base.CalculateDamageToTarget(target, effectiveness);
            if (this.SpellLine.KeyName == GlobalSpellsLines.Mundane_Poisons)
            {
                RealmAbilities.L3RAPropertyEnhancer ra = Caster.GetAbility(typeof(RealmAbilities.ViperAbility)) as RealmAbilities.L3RAPropertyEnhancer;
                if (ra != null)
                    ad.Damage *= 1 + (ra.Amount / 100);
            }
            			
			GameSpellEffect iWarLordEffect = SpellHandler.FindEffectOnTarget(target, "CleansingAura");
			if (iWarLordEffect != null)
				ad.Damage *= (int)(1.00 - (iWarLordEffect.Spell.Value * 0.01));
                       
            ad.CriticalDamage = 0;
			return ad;
		}

		/// <summary>
		/// Calculates min damage variance %
		/// </summary>
		/// <param name="target">spell target</param>
		/// <param name="min">returns min variance</param>
		/// <param name="max">returns max variance</param>
		public override void CalculateDamageVariance(GameLiving target, out double min, out double max)
		{
			int speclevel = 1;
			if (m_caster is GamePlayer)
			{
				speclevel = ((GamePlayer)m_caster).GetModifiedSpecLevel(m_spellLine.Spec);
			}
			min = 1;
			max = 1;

			if (target.Level > 0)
			{
				min = 0.75 + (speclevel - 1) / (double)target.Level * 0.5;
			}

			if (speclevel - 1 > target.Level)
			{
				double overspecBonus = (speclevel - 1 - target.Level) * 0.005;
				min += overspecBonus;
				max += overspecBonus;
			}

			if (min > max) min = max;
			if (min < 0) min = 0;
		}

		/// <summary>
		/// Sends damage text messages but makes no damage
		/// </summary>
		/// <param name="ad"></param>
		public override void SendDamageMessages(AttackData ad)
		{
			if (Spell.Name.StartsWith("Proc"))
			{
				MessageToCaster(String.Format("You hit {0} for {1} damage!",
					ad.Target.GetName(0, false), ad.Damage), eChatType.CT_YouHit);
			}
			else
			{
				MessageToCaster(String.Format("Your {0} hits {1} for {2} damage!",
					Spell.Name, ad.Target.GetName(0, false), ad.Damage), eChatType.CT_YouHit);
			}
			if (ad.CriticalDamage > 0)
				MessageToCaster(String.Format("Your {0} critically hits {1} for an additional {2} damage!",
					Spell.Name, ad.Target.GetName(0, false), ad.CriticalDamage), eChatType.CT_YouHit);

			//			if (ad.Damage > 0)
			//			{
			//				string modmessage = "";
			//				if (ad.Modifier > 0) modmessage = " (+"+ad.Modifier+")";
			//				if (ad.Modifier < 0) modmessage = " ("+ad.Modifier+")";
			//				MessageToCaster("You hit "+ad.Target.GetName(0, false)+" for " + ad.Damage + " damage!", eChatType.CT_Spell);
			//			}
			//			else
			//			{
			//				MessageToCaster("You hit "+ad.Target.GetName(0, false)+" for " + ad.Damage + " damage!", eChatType.CT_Spell);
			//				MessageToCaster(ad.Target.GetName(0, true) + " resists the effect!", eChatType.CT_SpellResisted);
			//				MessageToLiving(ad.Target, "You resist the effect!", eChatType.CT_SpellResisted);
			//			}
		}

		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			base.ApplyEffectOnTarget(target, effectiveness);
			target.StartInterruptTimer(SPELL_INTERRUPT_DURATION, AttackData.eAttackType.Spell, Caster);
		}


		protected override GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
		{
			// damage is not reduced with distance
            return new GameSpellEffect(this, m_spell.Duration, m_spellLine.IsBaseLine ? 2500 : 2000, effectiveness);
		}

		public override void OnEffectStart(GameSpellEffect effect)
		{
			SendEffectAnimation(effect.Owner, 0, false, 1);
		}

		public override void OnEffectPulse(GameSpellEffect effect)
		{
			base.OnEffectPulse(effect);
			// An acidic cloud surrounds you!
			MessageToLiving(effect.Owner, Spell.Message1, eChatType.CT_Spell);
			// {0} is surrounded by an acidic cloud!
			Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, false)), eChatType.CT_YouHit, effect.Owner);
			OnDirectEffect(effect.Owner, effect.Effectiveness);
		}

		/// <summary>
		/// When an applied effect expires.
		/// Duration spells only.
		/// </summary>
		/// <param name="effect">The expired effect</param>
		/// <param name="noMessages">true, when no messages should be sent to player and surrounding</param>
		/// <returns>immunity duration in milliseconds</returns>
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			base.OnEffectExpires(effect, noMessages);
			if (!noMessages)
			{
				// The acidic mist around you dissipates.
				MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
				// The acidic mist around {0} dissipates.
				Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, false)), eChatType.CT_SpellExpires, effect.Owner);
			}
			return 0;
		}

		public override void OnDirectEffect(GameLiving target, double effectiveness)
		{
			if (target == null) return;
			if (!target.IsAlive || target.ObjectState != GameLiving.eObjectState.Active) return;

			// no interrupts on DoT direct effect
			// calc damage
			AttackData ad = CalculateDamageToTarget(target, effectiveness);
			SendDamageMessages(ad);
			DamageTarget(ad, false);
		}

		public override double CalculateDamageBase()
		{
			double spellDamage = Spell.Damage;
			GamePlayer player = null;
			if (m_caster is GamePlayer)
				player = m_caster as GamePlayer;
			if (player != null && player.CharacterClass.ManaStat != eStat.UNDEFINED)
			{
				int manaStatValue = player.GetModified((eProperty)player.CharacterClass.ManaStat);
				spellDamage *= (manaStatValue + 200) / 275.0;
				if (spellDamage < 0)
					spellDamage = 0;
			}
			else if (m_caster is GameNPC)
			{
				int manaStatValue = m_caster.GetModified(eProperty.Intelligence);
				spellDamage *= (manaStatValue + 200) / 275.0;
				if (spellDamage < 0)
					spellDamage = 0;
			}
			return spellDamage;
		}

		// constructor
		public DoTSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}
}
