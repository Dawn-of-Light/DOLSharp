using DOL.GS;
using DOL.Database;

namespace DOL.GS.SkillHandler
{
	public class VampiirAbility : StatChangingAbility
	{
		public VampiirAbility(DBAbility dba, int level, eProperty property)
			: base(dba, level, property)
		{
		}
		
		public override string Name {
			get { return string.Format("{0} +{1}", base.Name, GetAmountForLevel(Level)); }
			set { base.Name = value; }
		}
	}

	public class VampiirStrength : VampiirAbility
	{
		public VampiirStrength(DBAbility dba, int level)
			: base(dba, level, eProperty.Strength)
		{
		}

		public override int GetAmountForLevel(int level)
		{
			//(+3 strength every level starting level 6),
			return level < 6 ? 0 : (level - 5) * 3;
		}
	}

	public class VampiirDexterity : VampiirAbility
	{
		public VampiirDexterity(DBAbility dba, int level)
			: base(dba, level, eProperty.Dexterity)
		{
		}

		public override int GetAmountForLevel(int level)
		{
			//(+3 dexterity every level starting level 6),
			return level < 6 ? 0 : (level - 5) * 3;
		}
	}

	public class VampiirConstitution : VampiirAbility
	{
		public VampiirConstitution(DBAbility dba, int level)
			: base(dba, level, eProperty.Constitution)
		{
		}

		public override int GetAmountForLevel(int level)
		{
			//(+3 constitution every level starting level 6),
			return level < 6 ? 0 : (level - 5) * 3;
		}
	}

	public class VampiirQuickness : VampiirAbility
	{
		public VampiirQuickness(DBAbility dba, int level)
			: base(dba, level, eProperty.Quickness)
		{
		}

		public override int GetAmountForLevel(int level)
		{
			//(+2 quickness every level starting level 6),
			return level < 6 ? 0 : (level - 5) * 2;
		}
	}
}
