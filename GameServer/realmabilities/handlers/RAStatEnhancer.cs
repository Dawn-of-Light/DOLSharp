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
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public eProperty Property
		{
			get { return m_property.Length > 0 ? m_property[0] : eProperty.Undefined; }
		}

		public RAStatEnhancer(DBAbility dba, int level, eProperty property)
			: base(dba, level, property)
		{
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

		public override int GetAmountForLevel(int level)
		{
			if (level < 1) return 0;
			if (ServerProperties.Properties.USE_NEW_PASSIVES_RAS_SCALING)
			{
				switch (level)
				{
						case 1: return 4;
						case 2: return 8;
						case 3: return 12;
						case 4: return 17;
						case 5: return 22;
						case 6: return 28;
						case 7: return 34;
						case 8: return 41;
						case 9: return 48;
						default: return 48;
				}
			}
			else
			{
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
		}

		/// <summary>
		/// send updates about the changes
		/// </summary>
		/// <param name="target"></param>
		protected override void SendUpdates(GameLiving target)
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
			return base.CheckRequirement(player) && player.Level >= 40;
		}
	}
}