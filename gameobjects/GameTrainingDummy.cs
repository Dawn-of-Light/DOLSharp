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
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using DOL.AI;
using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;
using DOL.Language;
using DOL.GS.Effects;
using DOL.GS.Movement;
using DOL.GS.Quests;
using DOL.GS.PacketHandler;
using DOL.GS.Spells;
using DOL.GS.Utils;
using DOL.GS.Housing;
using DOL.GS.RealmAbilities;
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
			get
			{
				return base.Health;
			}
			set
			{
				base.Health = Math.Max(1, value);
			
				if (HealthPercent < 10)
				{
					base.Health = MaxHealth / 10;
				}
			}
		}

		/// <summary>
		/// Training Dummies are always alive
		/// </summary>
		public override bool IsAlive
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Training Dummies never attack
		/// </summary>
		/// <param name="ad"></param>
		public override void OnAttackedByEnemy(AttackData ad)
		{
			// put the dummy in combat mode to reduce regen times
			LastAttackedByEnemyTickPvE = CurrentRegion.Time;
			return;
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