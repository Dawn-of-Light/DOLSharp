using System.Collections;

using DOL.Database;
using DOL.GS;

namespace DOL.GS.SkillHandler
{
	public class PropertyChangingAbility : Ability
	{
		// property to modify
		protected eProperty[] m_property;
		public eProperty[] Properties
		{
			get { return m_property; }
		}

		public PropertyChangingAbility(DBAbility dba, int level, eProperty[] property)
			: base(dba, level)
		{
			m_property = property;
		}

		public PropertyChangingAbility(DBAbility dba, int level, eProperty property)
			: base(dba, level)
		{
			m_property = new eProperty[] { property };
		}

		/// <summary>
		/// Get the Amount of Bonus for this ability at a particular level
		/// </summary>
		/// <param name="level">The level</param>
		/// <returns>The amount</returns>
		public virtual int GetAmountForLevel(int level)
		{
			return 0;
		}

		/// <summary>
		/// The bonus amount at this abilities level
		/// </summary>
		public int Amount
		{
			get
			{
				return GetAmountForLevel(Level);
			}
		}

		/// <summary>
		/// send updates about the changes
		/// </summary>
		/// <param name="target"></param>
		public virtual void SendUpdates(GameLiving target)
		{
		}

		/// <summary>
		/// Unit for values like %
		/// </summary>
		protected virtual string ValueUnit { get { return ""; } }

		public override void Activate(GameLiving living, bool sendUpdates)
		{
			if (activeOnLiving == null)
			{
				foreach (eProperty property in m_property)
				{
					living.AbilityBonus[(int)property] += GetAmountForLevel(living.Level);
				}
				activeOnLiving = living;
				if (sendUpdates) SendUpdates(living);
			}
			else
			{
				log.Warn("ability " + Name + " already activated on " + living.Name);
			}
		}

		public override void Deactivate(GameLiving living, bool sendUpdates)
		{
			if (activeOnLiving != null)
			{
				foreach (eProperty property in m_property)
				{
					living.AbilityBonus[(int)property] -= GetAmountForLevel(Level);
				}
				if (sendUpdates) SendUpdates(living);
				activeOnLiving = null;
			}
			else
			{
				log.Warn("ability " + Name + " already deactivated on " + living.Name);
			}
		}

		public override void OnLevelChange(int oldLevel)
		{
			foreach (eProperty property in m_property)
			{
				activeOnLiving.AbilityBonus[(int)property] += GetAmountForLevel(Level) - GetAmountForLevel(oldLevel);
			}
			SendUpdates(activeOnLiving);
		}
	}

	public class StatChangingAbility : PropertyChangingAbility
	{
		public StatChangingAbility(DBAbility dba, int level, eProperty[] property)
			: base(dba, level, property)
		{

		}

		public StatChangingAbility(DBAbility dba, int level, eProperty property)
			: base(dba, level, property)
		{

		}

		/// <summary>
		/// send updates about the changes
		/// </summary>
		/// <param name="target"></param>
		public override void SendUpdates(GameLiving target)
		{
			GamePlayer player = target as GamePlayer;	// need new prop system to not worry about updates
			if (player != null)
			{
				player.Out.SendCharStatsUpdate();
				player.Out.SendUpdateWeaponAndArmorStats();
				player.UpdateEncumberance();
				player.UpdatePlayerStatus();
			}

			if (target.IsAlive)
			{
				if (target.Health < target.MaxHealth) target.StartHealthRegeneration();
				else if (target.Health > target.MaxHealth) target.Health = target.MaxHealth;

				if (target.Mana < target.MaxMana) target.StartPowerRegeneration();
				else if (target.Mana > target.MaxMana) target.Mana = target.MaxMana;

				if (target.Endurance < target.MaxEndurance) target.StartEnduranceRegeneration();
				else if (target.Endurance > target.MaxEndurance) target.Endurance = target.MaxEndurance;
			}
		}
	}
}
