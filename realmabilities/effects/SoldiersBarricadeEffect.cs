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
	public class SoldiersBarricadeEffect : StaticEffect, IGameEffect
	{
		private const String m_delveString = "Grants the group an absorption bonus to all forms of damage (Does not stack with Barrier of Fortitude or Bedazzling Aura).";
		private GamePlayer m_player;
		private Int32 m_effectDuration;
		private RegionTimer m_expireTimer;
		private int m_value;

		/// <summary>
		/// Default constructor for AmelioratingMelodiesEffect
		/// </summary>
		public SoldiersBarricadeEffect()
		{

		}

		/// <summary>
		/// Called when effect is to be started
		/// </summary>
		/// <param name="player">The player to start the effect for</param>
		/// <param name="duration">The effectduration in secounds</param>
		/// <param name="value">The percentage additional value for all magic resis</param>
		public void Start(GamePlayer player, int duration, int value)
		{
			m_player = player;
			m_effectDuration = duration;
			m_value = value;

			if (player.TempProperties.getProperty(RealmAbilities.BarrierOfFortitudeAbility.BofBaSb, false))
				return;

			StartTimers();

			GameEventMgr.AddHandler(m_player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));

			m_player.AbilityBonus[(int)eProperty.Resist_Body] += m_value;
			m_player.AbilityBonus[(int)eProperty.Resist_Cold] += m_value;
			m_player.AbilityBonus[(int)eProperty.Resist_Energy] += m_value;
			m_player.AbilityBonus[(int)eProperty.Resist_Heat] += m_value;
			m_player.AbilityBonus[(int)eProperty.Resist_Matter] += m_value;
			m_player.AbilityBonus[(int)eProperty.Resist_Spirit] += m_value;
			m_player.AbilityBonus[(int)eProperty.Resist_Crush] += m_value;
			m_player.AbilityBonus[(int)eProperty.Resist_Slash] += m_value;
			m_player.AbilityBonus[(int)eProperty.Resist_Thrust] += m_value;
			m_player.Out.SendCharResistsUpdate();
			m_player.EffectList.Add(this);
			player.TempProperties.setProperty(RealmAbilities.BarrierOfFortitudeAbility.BofBaSb, true);
		}

		/// <summary>
		/// Called when a player leaves the game
		/// </summary>
		/// <param name="e">The event which was raised</param>
		/// <param name="sender">Sender of the event</param>
		/// <param name="args">EventArgs associated with the event</param>
		private static void PlayerLeftWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = (GamePlayer)sender;

			SoldiersBarricadeEffect SBEffect = (SoldiersBarricadeEffect)player.EffectList.GetOfType(typeof(SoldiersBarricadeEffect));
			if (SBEffect != null)
			{
				SBEffect.Cancel(false);
			}
		}

		/// <summary>
		/// Called when effect is to be cancelled
		/// </summary>
		/// <param name="playerCancel">Whether or not effect is player cancelled</param>
		public override void Cancel(bool playerCancel)
		{

			StopTimers();
			m_player.AbilityBonus[(int)eProperty.Resist_Body] -= m_value;
			m_player.AbilityBonus[(int)eProperty.Resist_Cold] -= m_value;
			m_player.AbilityBonus[(int)eProperty.Resist_Energy] -= m_value;
			m_player.AbilityBonus[(int)eProperty.Resist_Heat] -= m_value;
			m_player.AbilityBonus[(int)eProperty.Resist_Matter] -= m_value;
			m_player.AbilityBonus[(int)eProperty.Resist_Spirit] -= m_value;
			m_player.AbilityBonus[(int)eProperty.Resist_Crush] -= m_value;
			m_player.AbilityBonus[(int)eProperty.Resist_Slash] -= m_value;
			m_player.AbilityBonus[(int)eProperty.Resist_Thrust] -= m_value;
			m_player.Out.SendCharResistsUpdate();
			m_player.EffectList.Remove(this);
			GameEventMgr.RemoveHandler(m_player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
			m_player.TempProperties.removeProperty(RealmAbilities.BarrierOfFortitudeAbility.BofBaSb);

		}

		/// <summary>
		/// Starts the timers for this effect
		/// </summary>
		private void StartTimers()
		{
			StopTimers();
			m_expireTimer = new RegionTimer(m_player, new RegionTimerCallback(ExpireCallback), m_effectDuration * 1000);
		}

		/// <summary>
		/// Stops the timers for this effect
		/// </summary>
		private void StopTimers()
		{

			if (m_expireTimer != null)
			{
				m_expireTimer.Stop();
				m_expireTimer = null;
			}
		}

		/// <summary>
		/// The callback for when the effect expires
		/// </summary>
		/// <param name="timer">The ObjectTimerCallback object</param>
		private int ExpireCallback(RegionTimer timer)
		{
			Cancel(false);

			return 0;
		}


		/// <summary>
		/// Name of the effect
		/// </summary>
		public override string Name
		{
			get
			{
				return "Soldier's Barricade";
			}
		}

		/// <summary>
		/// Remaining time of the effect in milliseconds
		/// </summary>
		public override Int32 RemainingTime
		{
			get
			{
				RegionTimer timer = m_expireTimer;
				if (timer == null || !timer.IsAlive)
					return 0;
				return timer.TimeUntilElapsed;
			}
		}

		/// <summary>
		/// Icon ID
		/// </summary>
		public override UInt16 Icon
		{
			get
			{
				return 3014;
			}
		}

		//VaNaTiC->
		/*
		/// <summary>
		/// Unique ID for identification in the effect list
		/// </summary>
		public UInt16 InternalID
		{
			get
			{
				return m_id;
			}
			set
			{
				m_id = value;
			}
		}*/
		//VaNaTiC<-

		/// <summary>
		/// Delve information
		/// </summary>
		public override IList DelveInfo
		{
			get
			{
				IList delveInfoList = new ArrayList(10);
				delveInfoList.Add(m_delveString);
				delveInfoList.Add(" ");
				delveInfoList.Add("Value: " + m_value + "%");

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
