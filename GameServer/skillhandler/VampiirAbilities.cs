using DOL.GS;
using DOL.Database2;

namespace DOL.GS.SkillHandler
{
	public class VampiirStrength : StatChangingAbility
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

	public class VampiirDexterity : StatChangingAbility
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

	public class VampiirConstitution : StatChangingAbility
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

	public class VampiirQuickness : StatChangingAbility
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
