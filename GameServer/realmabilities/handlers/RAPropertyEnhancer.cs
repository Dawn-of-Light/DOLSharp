using System;
using System.Collections;
using System.Reflection;
using DOL.Database2;
using DOL.GS.PropertyCalc;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class RAPropertyEnhancer : L5RealmAbility
	{
		// property to modify
		eProperty[] m_property;

		public RAPropertyEnhancer(DBAbility dba, int level, eProperty[] property)
			: base(dba, level)
		{
			m_property = property;
		}

		public RAPropertyEnhancer(DBAbility dba, int level, eProperty property)
			: base(dba, level)
		{
			m_property = new eProperty[] { property };
		}

		public override System.Collections.IList DelveInfo
		{
			get
			{
				ArrayList list = new ArrayList();
				list.Add(m_description);
				list.Add("");
				for (int i = 1; i <= MaxLevel; i++)
				{
					list.Add("Level " + i + ": Amount: " + GetAmountForLevel(i) + ValueUnit);
				}
				return list;
			}
		}

		/// <summary>
		/// Get the Amount of Bonus for this RA at a particular level
		/// </summary>
		/// <param name="level">The level</param>
		/// <returns>The amount</returns>
		public virtual int GetAmountForLevel(int level)
		{
			return 0;
		}

		/// <summary>
		/// The bonus amount at this RA's level
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
		protected virtual void SendUpdates(GameLiving target)
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
					living.AbilityBonus[(int)property] += GetAmountForLevel(Level);
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

	public abstract class L3RAPropertyEnhancer : RAPropertyEnhancer
	{
		public L3RAPropertyEnhancer(DBAbility dba, int level, eProperty property)
			: base(dba, level, property)
		{
		}

		public L3RAPropertyEnhancer(DBAbility dba, int level, eProperty[] properties)
			: base(dba, level, properties)
		{ 
		}

		public override int CostForUpgrade(int level)
		{
			return (level + 1) * 5;
		}

		public override int MaxLevel
		{
			get
			{
				return 3;
			}
		}
	}
}