
using System;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;
using System.Text;
using DOL.AI.Brain;

namespace DOL.GS.Spells
{
	public abstract class HereticImmunityEffectSpellHandler : HereticPiercingMagic
	{
		/// <summary>
		/// called when spell effect has to be started and applied to targets
		/// </summary>
		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= CalculateNeededPower(target);
			base.FinishSpellCast(target);
		}

		/// <summary>
        /// Determines wether this spell is better than given one
		/// </summary>
		/// <param name="oldeffect"></param>
		/// <param name="neweffect"></param>
		/// <returns></returns>
		public override bool IsNewEffectBetter(GameSpellEffect oldeffect, GameSpellEffect neweffect)
		{
			if (oldeffect.Owner is GamePlayer) return false; //no overwrite for players
			return base.IsNewEffectBetter(oldeffect, neweffect);
		}
		/// <summary>
		/// Apply effect on target or do spell action if non duration spell
		/// </summary>
		/// <param name="target">target that gets the effect</param>
		/// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			if (target.Realm == 0 || Caster.Realm == 0)
			{
				target.LastAttackedByEnemyTickPvE = target.CurrentRegion.Time;
				Caster.LastAttackTickPvE = Caster.CurrentRegion.Time;
			}
			else
			{
				target.LastAttackedByEnemyTickPvP = target.CurrentRegion.Time;
				Caster.LastAttackTickPvP = Caster.CurrentRegion.Time;
			}
            if (target.HasAbility(Abilities.CCImmunity))
            {
                MessageToCaster(target.Name + " is immune to this effect!", eChatType.CT_SpellResisted);
                return;
            }
            if (target.TempProperties.getProperty("Charging", false))
            {
                MessageToCaster(target.Name + " is moving to fast for this spell to have any effect!", eChatType.CT_SpellResisted);
                return;
            }
			base.ApplyEffectOnTarget(target, effectiveness);

			if (Spell.CastTime > 0) 
			{
				target.StartInterruptTimer(SPELL_INTERRUPT_DURATION, AttackData.eAttackType.Spell, Caster);
			}
			if(target is GameNPC) 
			{
				GameNPC npc = (GameNPC)target;
				IOldAggressiveBrain aggroBrain = npc.Brain as IOldAggressiveBrain;
				if (aggroBrain != null)
					aggroBrain.AddToAggroList(Caster, 1);
			}
		}

//		/// <summary>
//		/// Calculates effect duration in ticks
//		/// </summary>
//		/// <param name="target"></param>
//		/// <param name="effectiveness"></param>
//		/// <returns></returns>
//		protected override int CalculateEffectDuration(GameLiving target, double effectiveness)
//		{
//			// http://support.darkageofcamelot.com/kb/article.php?id=423
//			// Patch Notes: Version 1.52
//			// The duration is 100% at the middle of the area, and it tails off to 50%
//			// duration at the edges. This does NOT change the way area effect spells
//			// work against monsters, only realm enemies (i.e. enemy players and enemy realm guards).
//			int duration = base.CalculateEffectDuration(target, effectiveness);
//			if (target is GamePlayer == false)
//				return duration;
//			duration *= (int)(0.5 + 0.5*effectiveness);
//			duration -= (int)(duration * target.GetResist(Spell.DamageType) * 0.01);
//
//			if (duration < 1) duration = 1;
//			else if (duration > (Spell.Duration << 5)) duration = (Spell.Duration << 5); // duration is in seconds, mult by 32
//			return duration;
//		}

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
		/// Creates the corresponding spell effect for the spell
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		/// <returns></returns>
		protected override GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
		{
			return new GameSpellAndImmunityEffect(this, (int)CalculateEffectDuration(target, effectiveness), 0, effectiveness);
		}

		// constructor
		public HereticImmunityEffectSpellHandler(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) {}
	}

	[SpellHandler("HereticSpeedDecrease")]
	public class HereticSpeedDecreaseSpellHandler : HereticImmunityEffectSpellHandler
	{
		private readonly object TIMER_PROPERTY;
		private const string EFFECT_PROPERTY = "Effect";

		public override void OnEffectStart(GameSpellEffect effect)
		{
			base.OnEffectStart(effect);
			effect.Owner.BuffBonusMultCategory1.Set((int)eProperty.MaxSpeed, effect, 1.0-Spell.Value*0.01);

			SendUpdates(effect.Owner);

			MessageToLiving(effect.Owner, Spell.Message1, eChatType.CT_Spell);
			Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, true)), eChatType.CT_Spell, effect.Owner);

			RestoreSpeedTimer timer = new RestoreSpeedTimer(effect);
			effect.Owner.TempProperties.setProperty(effect, timer);

			//REVOIR
			timer.Interval = 650;
			timer.Start(1 + (effect.Duration >> 1));

			effect.Owner.StartInterruptTimer(SPELL_INTERRUPT_DURATION, AttackData.eAttackType.Spell, Caster);
		}


		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			base.OnEffectExpires(effect,noMessages);

			GameTimer timer = (GameTimer)effect.Owner.TempProperties.getObjectProperty(effect, null);
			effect.Owner.TempProperties.removeProperty(effect);
			timer.Stop();

			effect.Owner.BuffBonusMultCategory1.Remove((int)eProperty.MaxSpeed, effect);

			SendUpdates(effect.Owner);

			MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
			Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, true)), eChatType.CT_SpellExpires, effect.Owner);

			return 60000;
		}


		protected virtual void OnAttacked(DOLEvent e, object sender, EventArgs arguments)
		{
			AttackedByEnemyEventArgs attackArgs = arguments as AttackedByEnemyEventArgs;
			GameLiving living = sender as GameLiving;
			if (attackArgs == null) return;
			if (living == null) return;
			GameSpellEffect effect = FindEffectOnTarget(living, this);
			if (attackArgs.AttackData.Damage > 0)
			{
							if (effect != null)
								effect.Cancel(false);
			}
            if (attackArgs.AttackData.SpellHandler is StyleBleeding || attackArgs.AttackData.SpellHandler is DoTSpellHandler || attackArgs.AttackData.SpellHandler is HereticDoTSpellHandler)
            {
                GameSpellEffect affect = FindEffectOnTarget(living, this);
                if (affect != null)
                    affect.Cancel(false);
            }
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

	
		public HereticSpeedDecreaseSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line)
		{
			TIMER_PROPERTY = this;
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
	}
}
