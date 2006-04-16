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
using System.Reflection;
using System.Text;
using DOL.Events;

namespace DOL.GS
{
	/// <summary>
	/// This class holds all information that
	/// EVERY base living in the game world needs!
	/// </summary>
	public abstract class GameLivingBase : PersistantGameObject
	{
		#region Level / EffectiveLevel / Health / MaxHealth / HealthPercent / Alive

		/// <summary>
		/// The level of the Object
		/// </summary>
		protected byte m_Level;

		/// <summary>
		/// Gets or Sets the current level of the Object
		/// </summary>
		public virtual byte Level
		{
			get { return m_Level; }
			set { m_Level = value; }
		}

		/// <summary>
		/// Gets or Sets the effective level of the Object
		/// </summary>
		public virtual int EffectiveLevel
		{
			get { return Level; }
			set {}
		}

		/// <summary>
		/// Health of the object
		/// </summary>
		protected int m_health;

		/// <summary>
		/// Gets/sets the object health
		/// </summary>
		public virtual int Health
		{
			get { return m_health; }
			set { m_health = value; }
		}

		/// <summary>
		/// Gets the max health of this living base
		/// </summary>
		public abstract int MaxHealth
		{
			get;
		}

		/// <summary>
		/// Gets the Health in percent 0..100
		/// </summary>
		public virtual byte HealthPercent
		{
			get
			{
				int maxHealth = MaxHealth;
				return (byte) (maxHealth <= 0 ? 0 : Health * 100 / maxHealth);
			}
		}

		/// <summary>
		/// returns if this living is alive
		/// </summary>
		public virtual bool Alive
		{
			get { return Health > 0; }
		}

		#endregion

		#region ChangeHealth / Die

		/// <summary>
		/// Changes the health
		/// </summary>
		/// <param name="changeSource">the source that inited the changes</param>
		/// <param name="changeAmount">the change amount (can be > 0)</param>
		/// <param name="generateAggro">if changeAmount > 0 does it generate agggro ?</param>
		/// <returns>the amount really changed</returns>
		public virtual int ChangeHealth(GameLiving changeSource, int changeAmount, bool generateAggro)
		{
			int oldHealth = Health;
			Health = Math.Min(Health + changeAmount, MaxHealth); //cap heal to max health
			Health = Math.Max(Health, 0); //min heal is 0
			if(oldHealth > 0 && Health <= 0)
			{
				Die(changeSource);
			}
			return Health - oldHealth;
		}

		/// <summary>
		/// Called when this living dies
		/// </summary>
		public virtual void Die(GameLiving killer)
		{
			Notify(GameLivingBaseEvent.Dying, this, new DyingEventArgs(killer));
		}

		/// <summary>
		/// This method is called whenever this living
		/// should take damage from some source
		/// (no update packet are send)
		/// </summary>
		/// <param name="source">the damage source</param>
		/// <param name="damageType">the damage type</param>
		/// <param name="damageAmount">the amount of damage</param>
		/// <param name="criticalAmount">the amount of critical damage</param>
		public virtual void TakeDamage(GameLiving source, eDamageType damageType, int damageAmount, int criticalAmount)
		{
			Notify(GameLivingBaseEvent.TakeDamage, this, new TakeDamageEventArgs(source, damageType, damageAmount, criticalAmount));
			ChangeHealth(source, -1 * (damageAmount+criticalAmount), false);
		}
		#endregion

		#region ConLevel/DurLevel

		/// <summary>
		/// Calculate con-level against other object
		/// &lt;=-3 = grey
		/// -2 = green
		/// -1 = blue
		/// 0 = yellow (same level)
		/// 1 = orange
		/// 2 = red
		/// &gt;=3 = violet
		/// </summary>
		/// <returns>conlevel</returns>
		public double GetConLevel(GameLivingBase compare)
		{
			return GetConLevel(EffectiveLevel, compare.EffectiveLevel);
		}

		/// <summary>
		/// Calculate con-level against other compareLevel
		/// &lt;=-3 = grey
		/// -2 = green
		/// -1 = blue  (compareLevel is 1 con lower)
		/// 0 = yellow (same level)
		/// 1 = orange (compareLevel is 1 con higher)
		/// 2 = red
		/// &gt;=3 = violet
		/// </summary>
		/// <returns>conlevel</returns>
		public static double GetConLevel(int level, int compareLevel)
		{
			int constep = Math.Max(1, (level+9)/10);
			double stepping = 1.0 / constep;
			int leveldiff = level - compareLevel + 1;
			return 1.0 - leveldiff * stepping;
		}

		/// <summary>
		/// Checks whether object is grey con to this living
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public bool IsObjectGreyCon(GameLivingBase obj)
		{
			int sourceLevel = EffectiveLevel;
			if(sourceLevel < GameLiving.NoXPForLevel.Length)
			{
				//if target level is less or equals to level that is grey to source
				if(obj.EffectiveLevel <= GameLiving.NoXPForLevel[sourceLevel])
					return true;
			}
			else
			{
				if(GetConLevel(obj) <= -3)
					return true;
			}
			return false;
		}

		#endregion

		#region ArmorFactor/ArmorAbsorb/Resist/OnattackedByEnemy
		/// <summary>
		/// calculate item armor factor
		/// </summary>
		/// <param name="slot"></param>
		/// <returns></returns>
		public abstract double GetArmorAF(eArmorSlot slot);

		/// <summary>
		/// Calculates armor absorb level
		/// </summary>
		/// <param name="slot"></param>
		/// <returns></returns>
		public abstract double GetArmorAbsorb(eArmorSlot slot);

		/// <summary>
		/// gets the resistance value by damage types
		/// </summary>
		/// <param name="damageType"></param>
		/// <returns></returns>
		public abstract int GetResist(eDamageType damageType);

		/// <summary>
		/// Returns the result of an enemy attack,
		/// yes this means WE decide if an enemy hits us or not :-)
		/// </summary>
		/// <param name="ad">AttackData</param>
		/// <param name="weapon">the weapon used for attack</param>
		/// <returns>the result of the attack</returns>
		public abstract GameLiving.eAttackResult CalculateEnemyAttackResult(AttackData ad, Weapon weapon);
		
		/// <summary>
		/// This method is called at the end of the attack sequence to
		/// notify objects if they have been attacked/hit by an attack
		/// </summary>
		/// <param name="ad">information about the attack</param>
		public virtual void OnAttackedByEnemy(AttackData ad, GameObject originalTarget)
		{
			Notify(GameLivingBaseEvent.AttackedByEnemy, this, new AttackedByEnemyEventArgs(ad) );
		}
		#endregion

		/// <summary>
		/// Returns the string representation of the object
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return new StringBuilder(base.ToString())
				.Append(" level=").Append(Level)
				.Append(" effectiveLevel=").Append(EffectiveLevel)
				.Append(" health=").Append(Health)
				.Append(" maxHealth=").Append(MaxHealth)
				.ToString();
		}
	}
}