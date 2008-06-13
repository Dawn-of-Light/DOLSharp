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
using System.Text;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Spell handler for unbreakable speed decreasing spells
	/// </summary>
	[SpellHandler("UnbreakableSpeedDecrease")]
	public class UnbreakableSpeedDecreaseSpellHandler : ImmunityEffectSpellHandler
	{
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			if (target.HasAbility(Abilities.CCImmunity)||target.HasAbility(Abilities.RootImmunity))
			{
				MessageToCaster(target.Name + " is immune to this effect!", eChatType.CT_SpellResisted);
				return;
			}
			if (target.EffectList.GetOfType(typeof(ChargeEffect)) != null)
			{
				MessageToCaster(target.Name + " is moving to fast for this spell to have any effect!", eChatType.CT_SpellResisted);
				return;
			}

			base.ApplyEffectOnTarget(target, effectiveness);
		}

		/// <summary>
		/// When an applied effect starts,
		/// duration spells only
		/// </summary>
		/// <param name="effect"></param>
		public override void OnEffectStart(GameSpellEffect effect)
		{
			base.OnEffectStart(effect);
			effect.Owner.BuffBonusMultCategory1.Set((int)eProperty.MaxSpeed, effect, 1.0-Spell.Value*0.01);

			SendUpdates(effect.Owner);

			MessageToLiving(effect.Owner, Spell.Message1, eChatType.CT_Spell);
			Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, true)), eChatType.CT_Spell, effect.Owner);

			RestoreSpeedTimer timer = new RestoreSpeedTimer(effect);
			effect.Owner.TempProperties.setProperty(effect, timer);
			timer.Interval = 650;
			timer.Start(1 + (effect.Duration >> 1));

			effect.Owner.StartInterruptTimer(SPELL_INTERRUPT_DURATION, AttackData.eAttackType.Spell, Caster);
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
			base.OnEffectExpires(effect,noMessages);

			GameTimer timer = (GameTimer)effect.Owner.TempProperties.getObjectProperty(effect, null);
			effect.Owner.TempProperties.removeProperty(effect);
			if(timer!=null) timer.Stop();

			effect.Owner.BuffBonusMultCategory1.Remove((int)eProperty.MaxSpeed, effect);

			SendUpdates(effect.Owner);

			MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
			Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, true)), eChatType.CT_SpellExpires, effect.Owner);

			return 60000;
		}

		/// <summary>
		/// Calculates the effect duration in milliseconds
		/// </summary>
		/// <param name="target">The effect target</param>
		/// <param name="effectiveness">The effect effectiveness</param>
		/// <returns>The effect duration in milliseconds</returns>
		protected override int CalculateEffectDuration(GameLiving target, double effectiveness)
		{
			double duration = base.CalculateEffectDuration(target, effectiveness);
			duration *= target.GetModified(eProperty.SpeedDecreaseDuration) * 0.01;

			if (duration < 1)
				duration = 1;
			else if (duration > (Spell.Duration * 4))
				duration = (Spell.Duration * 4);
			return (int)duration;
		}

		/// <summary>
		/// Sends updates on effect start/stop
		/// </summary>
		/// <param name="owner"></param>
		protected static void SendUpdates(GameLiving owner)
		{
			if (owner.IsMezzed || owner.IsStunned)
				return;

			GamePlayer player = owner as GamePlayer;
			if (player != null)
				player.Out.SendUpdateMaxSpeed();

			GameNPC npc = owner as GameNPC;
			if (npc != null)
			{
				int maxSpeed = npc.MaxSpeed;
				if (npc.CurrentSpeed > maxSpeed)
					npc.CurrentSpeed = maxSpeed;
			}
		}

		/// <summary>
		/// Slowly restores the livings speed
		/// </summary>
		private sealed class RestoreSpeedTimer : GameTimer
		{
			/// <summary>
			/// The speed changing effect
			/// </summary>
			private readonly GameSpellEffect m_effect;

			/// <summary>
			/// Constructs a new RestoreSpeedTimer
			/// </summary>
			/// <param name="effect">The speed changing effect</param>
			public RestoreSpeedTimer(GameSpellEffect effect) : base(effect.Owner.CurrentRegion.TimeManager)
			{
				m_effect = effect;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				GameSpellEffect effect = m_effect;

				double factor = 2.0 - (effect.Duration - effect.RemainingTime)/(double)(effect.Duration>>1);
				if (factor < 0) factor = 0;
				else if (factor > 1) factor = 1;

				effect.Owner.BuffBonusMultCategory1.Set((int)eProperty.MaxSpeed, effect, 1.0 - effect.Spell.Value*factor*0.01);

				SendUpdates(effect.Owner);

				if (factor <= 0)
					Stop();
			}


			/// <summary>
			/// Returns short information about the timer
			/// </summary>
			/// <returns>Short info about the timer</returns>
			public override string ToString()
			{
				return new StringBuilder(base.ToString())
					.Append(" SpeedDecreaseEffect: (").Append(m_effect.ToString()).Append(')')
					.ToString();
			}
		}

		/// <summary>
		/// Constructs a new UnbreakableSpeedDecreaseSpellHandler
		/// </summary>
		/// <param name="caster"></param>
		/// <param name="spell"></param>
		/// <param name="line"></param>
		public UnbreakableSpeedDecreaseSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line)
		{
		}
	}
}
