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
using System.Reflection;
using System.Text;
using System.Threading;
using DOL.GS;
using log4net;

namespace DOL.AI
{
	/// <summary>
	/// <p>This class is the base brain of all npc's that only stay active when players are close
	/// <p>This class defines the base for a brain that activates itself when players get close to
	/// it's body and becomes dormat again after a certain amount of time when no players are close
	/// to it's body anymore.
	/// <p>Useful to save CPU for MANY mobs that have no players in range, they will stay dormant.
	/// </summary>
	public abstract class APlayerVicinityBrain : ABrain
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// constructor of this brain
		/// </summary>
		public APlayerVicinityBrain() : base()
		{
		}

		/// <summary>
		/// Returns the string representation of the APlayerVicinityBrain
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return base.ToString() + ", noPlayersStopCountdown=" + noPlayersStopCountdown.ToString();
		}

		/// <summary>
		/// The number of ticks this brain stays active while no player
		/// is in the vicinity.
		/// </summary>
		protected int noPlayersStopCountdown;

		/// <summary>
		/// The number of milliseconds this brain will stay active even when no player is close
		/// This abstract class always returns 45 Seconds
		/// </summary>
		protected virtual int NoPlayersStopDelay
		{
			set
			{
			}
			get { return 45000; }
		}

		/// <summary>
		/// Starts the brain thinking and resets the inactivity countdown
		/// </summary>
		/// <returns>true if started</returns>
		public override bool Start()
		{
			if (!Body.IsVisibleToPlayers)
				return false;
			Interlocked.Exchange(ref noPlayersStopCountdown, (int)(NoPlayersStopDelay/ThinkInterval));
			return base.Start();
		}

		/// <summary>
		/// Called whenever the brain should do some thinking.
		/// We check if there is at least one player around and nothing
		/// bad has happened. If so, we shutdown our brain.
		/// </summary>
		/// <param name="callingTimer"></param>
		protected override int BrainTimerCallback(RegionTimer callingTimer)
		{
			if (Interlocked.Decrement(ref noPlayersStopCountdown) <= 0)
			{
				//Stop the brain timer
				Stop();
				return 0;
			}
			return base.BrainTimerCallback(callingTimer);
		}
	}
}