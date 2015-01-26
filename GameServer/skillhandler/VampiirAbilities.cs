using DOL.GS;
using DOL.Database;

namespace DOL.GS.SkillHandler
{
	public class LevelBasedStatChangingAbility : StatChangingAbility
	{
		public LevelBasedStatChangingAbility(DBAbility dba, int level, eProperty property)
			: base(dba, level, property)
		{
		}

		public override int Level
		{
			get
			{
				// Report Max Value if no living assigned to trigger the ability override
				if (m_activeLiving != null)
					return m_activeLiving.Level;
				
				return int.MaxValue;
			}
			set
			{
				// Override Setter to have Living Level Updated if available.
				if (m_activeLiving != null)
					base.Level = m_activeLiving.Level;
				
				base.Level = value;
			}
		}
		
		public override string Name {
			get { return string.Format("{0} +{1}", base.Name, GetAmountForLevel(Level)); }
			set { base.Name = value; }
		}
		
		public override void Activate(GameLiving living, bool sendUpdates)
		{
			// Set Base level Before Living is set.
			Level = living.Level;
			base.Activate(living, sendUpdates);
		}
	}
	
	public abstract class VampiirAbility : LevelBasedStatChangingAbility
	{
		public abstract int RatioByLevel { get; }
		
		protected VampiirAbility(DBAbility dba, int level, eProperty property)
			: base(dba, level, property)
		{
		}
		
		public override int GetAmountForLevel(int level)
		{
			//(+stats every level starting level 6),
			return level < 6 ? 0 : (level - 5) * RatioByLevel;
		}
	}

	public class VampiirStrength : VampiirAbility
	{
		public override int RatioByLevel { get { return 3; } }
		
		public VampiirStrength(DBAbility dba, int level)
			: base(dba, level, eProperty.Strength)
		{
		}
	}

	public class VampiirDexterity : VampiirAbility
	{
		public override int RatioByLevel { get { return 3; } }

		public VampiirDexterity(DBAbility dba, int level)
			: base(dba, level, eProperty.Dexterity)
		{
		}
	}

	public class VampiirConstitution : VampiirAbility
	{
		public override int RatioByLevel { get { return 3; } }

		public VampiirConstitution(DBAbility dba, int level)
			: base(dba, level, eProperty.Constitution)
		{
		}
	}

	public class VampiirQuickness : VampiirAbility
	{
		public override int RatioByLevel { get { return 2; } }

		public VampiirQuickness(DBAbility dba, int level)
			: base(dba, level, eProperty.Quickness)
		{
		}
	}
}
