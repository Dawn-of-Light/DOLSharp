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
using System.Reflection;
using DOL.AI.Brain;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Training Dummy: Can't move, fight back, or die
	/// </summary>
	public class GameTrainingDummy : GameNPC
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public GameTrainingDummy() : base()
		{
			m_maxSpeedBase = 0;
			SetOwnBrain(new BlankBrain());
		}


		/// <summary>
		/// Training Dummies never drop below 10% health
		/// </summary>
		public override int Health
		{
			get{ return base.MaxHealth;}
			set {}
		}

		/// <summary>
		/// Training Dummies are always alive
		/// </summary>
		public override bool IsAlive
		{
			get{ return true;}
		}

		/// <summary>
		/// Training Dummies never attack
		/// </summary>
		/// <param name="ad"></param>
		public override void OnAttackedByEnemy(AttackData ad)
		{
			if (ad.IsHit)
			{
				if (ad.Attacker.Realm == 0 || this.Realm == 0)
				{
					LastAttackedByEnemyTickPvE = CurrentRegion.Time;
					ad.Attacker.LastAttackTickPvE = CurrentRegion.Time;
				}
				else
				{
					LastAttackedByEnemyTickPvP = CurrentRegion.Time;
					ad.Attacker.LastAttackTickPvP = CurrentRegion.Time;
				}
			}
		}

		/// <summary>
		/// Interacting with a training dummy does nothing
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override bool Interact(GamePlayer player)
		{
			return true;
		}
	}
}