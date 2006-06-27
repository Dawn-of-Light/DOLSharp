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

namespace DOL.GS
{
	/// <summary>
	/// GameMovingObject is a base class for boats and siege weapons.
	/// </summary>		
	public abstract class GameMovingObject : GameNPC
	{
		public GameMovingObject() : base()
		{
		}

		public virtual ushort Type()
		{
			return 0;
		}

		public override int Model
		{
			get
			{
				return base.Model;
			}
			set
			{
				base.Model = value;
				if(ObjectState==eObjectState.Active)
					BroadcastUpdate();
			}
		}
		public override bool IsWorthReward
		{
			get {return false;}
		}
		/// <summary>
		/// This methode is override to remove XP system
		/// </summary>
		/// <param name="source">the damage source</param>
		/// <param name="damageType">the damage type</param>
		/// <param name="damageAmount">the amount of damage</param>
		/// <param name="criticalAmount">the amount of critical damage</param>
		public override void TakeDamage(GameLiving source, eDamageType damageType, int damageAmount, int criticalAmount)
		{
			//Work around the XP system
			if (Alive)
			{
				ChangeHealth(source, eHealthChangeType.AttackDamage, -1 * (damageAmount + criticalAmount));
				if (!Alive)
				{
					Die(source);
				}
			}
		}
		/// <summary>
		/// Starts the power regeneration
		/// </summary>
		public override void StartPowerRegeneration()
		{
			//No regeneration for moving objects
			return;
		}
		/// <summary>
		/// Starts the endurance regeneration
		/// </summary>
		public override void StartEnduranceRegeneration()
		{
			//No regeneration for moving objects
			return;
		}
	}
}
