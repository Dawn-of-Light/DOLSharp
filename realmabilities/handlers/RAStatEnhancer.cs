using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DOL.Database;
using DOL.Language;


namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// 
	/// </summary>
	public class RAStatEnhancer : L5RealmAbility
	{
		eProperty m_property = eProperty.Undefined;

		public eProperty Property
		{
			get { return m_property; }
		}

		public RAStatEnhancer(DBAbility dba, int level, eProperty property)
			: base(dba, level)
		{
			m_property = property;
		}

		public override IList<string> DelveInfo
		{
			get
			{
				var list = new List<string>();
				list.Add(m_description);
				list.Add("");
				for (int i = 1; i <= MaxLevel; i++)
				{
                    list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "RAStatEnhancer.DelveInfo.Info1", i, GetAmountForLevel(i)));
                }
				return list;
			}
		}

		public virtual int GetAmountForLevel(int level)
		{
			if (level < 1) return 0;
			switch (level)
			{
				case 1: return 4;
				case 2: return 12;
				case 3: return 22;
				case 4: return 34;
				case 5: return 48;
				default: return 48;
			}
		}

		/// <summary>
		/// send updates about the changes
		/// </summary>
		/// <param name="target"></param>
		public virtual void SendUpdates(GameLiving target)
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

		public override void Activate(GameLiving living, bool sendUpdates)
		{
			if (activeOnLiving == null)
			{
				living.AbilityBonus[(int)m_property] += GetAmountForLevel(Level);
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
				living.AbilityBonus[(int)m_property] -= GetAmountForLevel(Level);
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
			activeOnLiving.AbilityBonus[(int)m_property] += GetAmountForLevel(Level) - GetAmountForLevel(oldLevel);
			SendUpdates(activeOnLiving);
		}
	}

	public class RAStrengthEnhancer : RAStatEnhancer
	{
		public RAStrengthEnhancer(DBAbility dba, int level) : base(dba, level, eProperty.Strength) { }
	}

	public class RAConstitutionEnhancer : RAStatEnhancer
	{
		public RAConstitutionEnhancer(DBAbility dba, int level) : base(dba, level, eProperty.Constitution) { }
	}

	public class RAQuicknessEnhancer : RAStatEnhancer
	{
		public RAQuicknessEnhancer(DBAbility dba, int level) : base(dba, level, eProperty.Quickness) { }
	}

	public class RADexterityEnhancer : RAStatEnhancer
	{
		public RADexterityEnhancer(DBAbility dba, int level) : base(dba, level, eProperty.Dexterity) { }
	}

	public class RAAcuityEnhancer : RAStatEnhancer
	{
		public RAAcuityEnhancer(DBAbility dba, int level) : base(dba, level, eProperty.Acuity) { }
	}

	public class RAMaxManaEnhancer : RAStatEnhancer
	{
		public RAMaxManaEnhancer(DBAbility dba, int level) : base(dba, level, eProperty.MaxMana) { }
	}

	public class RAMaxHealthEnhancer : RAStatEnhancer
	{
		public RAMaxHealthEnhancer(DBAbility dba, int level) : base(dba, level, eProperty.MaxHealth) { }

		public override bool CheckRequirement(GamePlayer player)
		{
			return player.Level >= 40;
		}
	}
}