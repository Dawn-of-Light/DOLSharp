using System;
using System.Collections;
using DOL.GS.PacketHandler;
using DOL.GS.SkillHandler;
using DOL.GS.PropertyCalc;
using DOL.Events;

namespace DOL.GS.Effects
{
	/// <summary>
	/// Effect handler for Barrier Of Fortitude
	/// </summary> 
	public class SpeedOfSoundEffect : TimedEffect, IGameEffect
	{
		public SpeedOfSoundEffect(int duration)
			: base(duration)
		{ }

		DOLEventHandler m_attackFinished = new DOLEventHandler(AttackFinished);


		/// <summary>
		/// Called when effect is to be started
		/// </summary>
		/// <param name="living">The living to start the effect for</param>
		public override void Start(GameLiving living)
		{
			base.Start(living);
			living.TempProperties.setProperty("Charging", true);
			GameEventMgr.AddHandler(living, GameLivingEvent.AttackFinished, m_attackFinished);
			GameEventMgr.AddHandler(living, GameLivingEvent.CastFinished, m_attackFinished);
			living.BuffBonusMultCategory1.Set((int)eProperty.MaxSpeed, this, PropertyCalc.MaxSpeedCalculator.SPEED4);		
			if (living is GamePlayer)
				(living as GamePlayer).Out.SendUpdateMaxSpeed();
		}

		/// <summary>
		/// Called when the effectowner attacked an enemy
		/// </summary>
		/// <param name="e">The event which was raised</param>
		/// <param name="sender">Sender of the event</param>
		/// <param name="args">EventArgs associated with the event</param>
		private static void AttackFinished(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = (GamePlayer)sender;
			if (e == GameLivingEvent.CastFinished)
			{
				CastStartingEventArgs cfea = args as CastStartingEventArgs;

				if (cfea.SpellHandler.Caster != player)
					return;

				//cancel if the effectowner casts a non-positive spell
				if (!cfea.SpellHandler.HasPositiveEffect)
				{
					SpeedOfSoundEffect effect = (SpeedOfSoundEffect)player.EffectList.GetOfType(typeof(SpeedOfSoundEffect));
					if (effect != null)
						effect.Cancel(false);
				}
			}
			else if (e == GameLivingEvent.AttackFinished)
			{
				AttackFinishedEventArgs afargs = args as AttackFinishedEventArgs;
				if (afargs == null)
					return;

				if (afargs.AttackData.Attacker != player)
					return;

				switch (afargs.AttackData.AttackResult)
				{
					case GameLiving.eAttackResult.HitStyle:
					case GameLiving.eAttackResult.HitUnstyled:
					case GameLiving.eAttackResult.Blocked:
					case GameLiving.eAttackResult.Evaded:
					case GameLiving.eAttackResult.Fumbled:
					case GameLiving.eAttackResult.Missed:
					case GameLiving.eAttackResult.Parried:
						SpeedOfSoundEffect effect = (SpeedOfSoundEffect)player.EffectList.GetOfType(typeof(SpeedOfSoundEffect));
						if (effect != null)
							effect.Cancel(false);
						break;
				}
			}
		}

		public override void Stop()
		{
			base.Stop();
			m_owner.TempProperties.removeProperty("Charging");
			m_owner.BuffBonusMultCategory1.Remove((int)eProperty.MaxSpeed, this);
			if (m_owner is GamePlayer)
				(m_owner as GamePlayer).Out.SendUpdateMaxSpeed();
			GameEventMgr.RemoveHandler(m_owner, GameLivingEvent.AttackFinished, m_attackFinished);
			GameEventMgr.RemoveHandler(m_owner, GameLivingEvent.CastFinished, m_attackFinished);
		}


		/// <summary>
		/// Name of the effect
		/// </summary>
		public override string Name
		{
			get
			{
				return "Speed of Sound";
			}
		}

		/// <summary>
		/// Icon ID
		/// </summary>
		public override UInt16 Icon
		{
			get
			{
				return 3020;
			}
		}

		/// <summary>
		/// Delve information
		/// </summary>
		public override IList DelveInfo
		{
			get
			{
				IList delveInfoList = new ArrayList(10);
				delveInfoList.Add("Gives immunity to stun/snare/root and mesmerize spells and provides unbreakeable speed.");
				delveInfoList.Add(" ");

				int seconds = (int)(RemainingTime / 1000);
				if (seconds > 0)
				{
					delveInfoList.Add(" ");
					delveInfoList.Add("- " + seconds + " seconds remaining.");
				}

				return delveInfoList;
			}
		}
	}
}