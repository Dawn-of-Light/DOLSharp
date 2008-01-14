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
using DOL.GS;

namespace DOL.Events
{
	/// <summary>
	/// This class holds all possible GameLiving events.
	/// Only constants defined here!
	/// </summary>
	public class GameLivingEvent : GameObjectEvent
	{
		/// <summary>
		/// Constructs a new GameLivingEvent
		/// </summary>
		/// <param name="name">the event name</param>
		protected GameLivingEvent(string name)
			: base(name)
		{
		}

		/// <summary>
		/// Tests if this event is valid for the specified object
		/// </summary>
		/// <param name="o">The object for which the event wants to be registered</param>
		/// <returns>true if valid, false if not</returns>
		public override bool IsValidFor(object o)
		{
			return o is GameLiving;
		}

		/// <summary>
		/// The SayReceive event is fired whenever the living receives a say
		/// <seealso cref="SayReceiveEventArgs"/>
		/// </summary>
		public static readonly GameLivingEvent SayReceive = new GameLivingEvent("GameLiving.SayReceive");
		/// <summary>
		/// The Say event is fired whenever the living says something
		/// <seealso cref="SayEventArgs"/>
		/// </summary>
		public static readonly GameLivingEvent Say = new GameLivingEvent("GameLiving.Say");
		/// <summary>
		/// The YellReceive event is fired whenever the living receives a yell
		/// <seealso cref="YellReceiveEventArgs"/>
		/// </summary>
		public static readonly GameLivingEvent YellReceive = new GameLivingEvent("GameLiving.YellReceive");
		/// <summary>
		/// The Yell event is fired whenever the living yells something
		/// <seealso cref="YellEventArgs"/>
		/// </summary>
		public static readonly GameLivingEvent Yell = new GameLivingEvent("GameLiving.Yell");
		/// <summary>
		/// The WhisperReceive event is fired whenever the living receives a whisper
		/// <seealso cref="WhisperReceiveEventArgs"/>
		/// </summary>
		public static readonly GameLivingEvent WhisperReceive = new GameLivingEvent("GameLiving.WhisperReceive");
		/// <summary>
		/// The Whisper event is fired whenever the living whispers something
		/// <seealso cref="WhisperEventArgs"/>
		/// </summary>
		public static readonly GameLivingEvent Whisper = new GameLivingEvent("GameLiving.Whisper");
		/// <summary>
		/// The Hit event is fired whenever the living is hit by gameliving
		/// </summary>
		public static readonly GameLivingEvent AttackedByEnemy = new GameLivingEvent("GameLiving.AttackedByEnemy");
		/// <summary>
		/// The AttackedEnemy event is fired whenever the living makes attack
		/// </summary>
		public static readonly GameLivingEvent AttackFinished = new GameLivingEvent("GameLiving.AttackedEnemy");
		/// <summary>
		/// The EnemyKilled event is fired whenever the living kill something
		/// </summary>
		public static readonly GameLivingEvent EnemyKilled = new GameLivingEvent("GameLiving.EnemyKilled");
		/// <summary>
		/// The GainedExperience event is fired whenever the living gains experience
		/// </summary>
		public static readonly GameLivingEvent GainedExperience = new GameLivingEvent("GameLiving.GainedExperience");
		/// <summary>
		/// The GainRealmPoints event is fired whenever the living gains realm points
		/// </summary>
		public static readonly GameLivingEvent GainedRealmPoints = new GameLivingEvent("GameLiving.GainedRealmPoints");
		public static readonly GameLivingEvent GainedBountyPoints = new GameLivingEvent("GameLiving.GainedBountyPoints");
		/// <summary>
		/// The Dying event is fired just before the living died
		/// </summary>
		public static readonly GameLivingEvent Dying = new GameLivingEvent("GameLiving.Dying");
		/// <summary>
		/// The Moving event is fired whenever the living moves
		/// </summary>
		public static readonly GameLivingEvent Moving = new GameLivingEvent("GameLiving.Moving");
		/// <summary>
		/// The EnemyHealed event is fired whenever the living's enemy is healed
		/// <seealso cref="EnemyHealedEventArgs"/>
		/// </summary>
		public static readonly GameLivingEvent EnemyHealed = new GameLivingEvent("GameLiving.EnemyHealed");
		/// <summary>
		/// The Timer event is fired whenever an previously added timer has finished
		/// Used within questsystem
		/// </summary>
		public static readonly GameLivingEvent Timer = new GameLivingEvent("GameLiving.Timer");
		/// <summary>
		/// The CastSpell event is fired whenever the living casts a spell
		/// </summary>
		public static readonly GameLivingEvent CastSpell = new GameLivingEvent("GameLiving.CastSpell");
		/// <summary>
		/// The StartSpell event is fired whenever the living start a spell
		/// </summary>
		public static readonly GameLivingEvent StartSpell = new GameLivingEvent("GameLiving.StartSpell");
		/// <summary>
		/// The CastFinished event is fired whenever the living finishes casting a spell
		/// </summary>
		public static readonly GameLivingEvent CastFinished = new GameLivingEvent("GameLiving.CastFinished");
		/// <summary>
		/// The CastFailed event is fired whenever the GameLiving's cast failed
		/// </summary>
		public static readonly GameLivingEvent CastFailed = new GameLivingEvent("GameLiving.CastFailed");
		/// <summary>
		/// The CastFailed event is fired whenever the GameLiving's cast succeeded
		/// </summary>
		public static readonly GameLivingEvent CastSucceeded = new GameLivingEvent("GameLiving.CastSucceeded");
		/// <summary>
		/// The HealthChanged event is fired whenever the GameLiving's health is changed
		/// </summary>
		public static readonly GameLivingEvent HealthChanged = new GameLivingEvent("GameLiving.HealthChanged");
		/// <summary>
		/// The PetReleased event is fired whenever the player commands to release controlled NPC
		/// </summary>
		public static readonly GameLivingEvent PetReleased = new GameLivingEvent("GamePlayer.PetReleased");
	}
}
