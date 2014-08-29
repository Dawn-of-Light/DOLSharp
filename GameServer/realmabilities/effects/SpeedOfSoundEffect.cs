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
using DOL.GS.PacketHandler;
using DOL.GS.SkillHandler;
using DOL.GS.PropertyCalc;
using DOL.Events;
using System.Collections.Generic;

namespace DOL.GS.Effects
{
	/// <summary>
	/// Effect handler for Speed of Sound
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
			living.BuffBonusMultCategory1.Set(eProperty.MaxSpeed, this, PropertyCalc.MaxSpeedCalculator.SPEED4);		
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
				CastingEventArgs cfea = args as CastingEventArgs;

				if (cfea == null || cfea.SpellHandler.Caster != player)
					return;

				//cancel if the effectowner casts a non-positive spell
				if (!cfea.SpellHandler.HasPositiveEffect)
				{
					SpeedOfSoundEffect effect = player.EffectList.GetOfType<SpeedOfSoundEffect>();
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
					case eAttackResult.HitStyle:
					case eAttackResult.HitUnstyled:
					case eAttackResult.Blocked:
					case eAttackResult.Evaded:
					case eAttackResult.Fumbled:
					case eAttackResult.Missed:
					case eAttackResult.Parried:
						SpeedOfSoundEffect effect = player.EffectList.GetOfType<SpeedOfSoundEffect>();
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
			m_owner.BuffBonusMultCategory1.Remove(eProperty.MaxSpeed, this);
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
		public override IList<string> DelveInfo
		{
			get
			{
				var delveInfoList = new List<string>();
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