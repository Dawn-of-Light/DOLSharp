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
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Style bleeding effect spell handler
	/// </summary>
	[SpellHandler("StyleBleeding")]
	public class StyleBleeding : SpellHandler
	{
		/// <summary>
		/// The property name for bleed value
		/// </summary>
		protected const string BLEED_VALUE_PROPERTY = "BleedValue";

		/// <summary>
		/// When an applied effect starts
		/// duration spells only
		/// </summary>
		/// <param name="effect"></param>
		public override void OnEffectStart(GameSpellEffect effect)
		{
			base.OnEffectStart(effect);
			effect.Owner.TempProperties.setProperty(BLEED_VALUE_PROPERTY, (int)Spell.Damage + (int)Spell.Damage * Util.Random(25) / 100 );  // + random max 25%
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
			effect.Owner.TempProperties.removeProperty(BLEED_VALUE_PROPERTY);
			return 0;
		}

		/// <summary>
		/// When an applied effect pulses
		/// duration spells only
		/// </summary>
		/// <param name="effect"></param>
		public override void OnEffectPulse(GameSpellEffect effect)
		{
			base.OnEffectPulse(effect);

			MessageToLiving(effect.Owner, Spell.Message1, eChatType.CT_YouWereHit);
			Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, false)), eChatType.CT_YouHit, effect.Owner);

			int bleedValue = effect.Owner.TempProperties.getIntProperty(BLEED_VALUE_PROPERTY, 0);

			AttackData ad = new AttackData();
			ad.Attacker = Caster;
			ad.Target = effect.Owner;
			ad.AttackType = AttackData.eAttackType.Spell;
			ad.Modifier = bleedValue * ad.Target.GetResist(Spell.DamageType) / -100;
			ad.Damage = bleedValue + ad.Modifier;
			ad.DamageType = Spell.DamageType;
			ad.AttackResult = GameLiving.eAttackResult.HitUnstyled;

			SendDamageMessages(ad);

			// attacker must be null, attack result is 0x0A
			foreach(GamePlayer player in ad.Target.GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE)) 
			{
				player.Out.SendCombatAnimation(null, ad.Target, 0, 0, 0, 0, 0x0A, ad.Target.HealthPercent);
			}
			// send animation before dealing damage else dead livings show no animation
			ad.Target.OnAttackedByEnemy(ad, ad.Target);
			ad.Attacker.DealDamage(ad);

			if (--bleedValue <= 0 || !effect.Owner.Alive)
				effect.Cancel(false);
			else effect.Owner.TempProperties.setProperty(BLEED_VALUE_PROPERTY, bleedValue);
		}

		/// <summary>
		/// Creates the corresponding spell effect for the spell
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		/// <returns></returns>
		protected override GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
		{
			return new GameSpellEffect(this, CalculateEffectDuration(target, effectiveness), Spell.Frequency, effectiveness);
		}

		/// <summary>
		/// Calculates the effect duration in milliseconds
		/// </summary>
		/// <param name="target">The effect target</param>
		/// <param name="effectiveness">The effect effectiveness</param>
		/// <returns>The effect duration in milliseconds</returns>
		protected override int CalculateEffectDuration(GameLiving target, double effectiveness)
		{
			return Spell.Duration;
		}

		/// <summary>
		/// Calculates chance of spell getting resisted
		/// </summary>
		/// <param name="target">the target of the spell</param>
		/// <returns>chance that spell will be resisted for specific target</returns>
		public override int CalculateSpellResistChance(GameLiving target)
		{
			return 0;
		}

		/// <summary>
		/// Determines wether this spell is better than given one
		/// </summary>
		/// <param name="oldeffect"></param>
		/// <returns>true if this spell is better version than compare spell</returns>
		public override bool IsNewEffectBetter(GameSpellEffect oldeffect, GameSpellEffect neweffect)
		{
			return oldeffect.Spell.SpellType == neweffect.Spell.SpellType;
		}

		// constructor
		public StyleBleeding(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}
