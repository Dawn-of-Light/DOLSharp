using System;
using DOL.Database;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Wild power ability, critical hit chance bonus to damage spells (SpellHandler checks for it)
	/// </summary>
	public class WildPowerAbility : RAPropertyEnhancer
	{

		public WildPowerAbility(DBAbility dba, int level)
			: base(dba, level, eProperty.CriticalSpellHitChance)
		{
		}

		protected override string ValueUnit { get { return "%"; } }

		public override int GetAmountForLevel(int level)
		{
			if (level < 1) return 0;
			switch (level)
			{
				case 1: return 3;
				case 2: return 9;
				case 3: return 17;
				case 4: return 27;
				case 5: return 39;
				default: return 39;
			}
		}
	}


	/// <summary>
	/// Mastery of Magery ability, adds to effectivenes of damage spells
	/// </summary>
	public class MasteryOfMageryAbility : RAPropertyEnhancer
	{
		public MasteryOfMageryAbility(DBAbility dba, int level)
			: base(dba, level, eProperty.SpellDamage)
		{
		}

		protected override string ValueUnit { get { return "%"; } }

		public override int GetAmountForLevel(int level)
		{
			if (level < 1) return 0;
			switch (level)
			{
				case 1: return 2;
				case 2: return 4;
				case 3: return 7;
				case 4: return 11;
				case 5: return 15;
				default: return 15;
			}
		}
	}


	/// <summary>
	/// Wild healing ability, critical heal chance bonus to heal spells (SpellHandler checks for it)
	/// </summary>
	public class WildHealingAbility : RAPropertyEnhancer
	{
		public WildHealingAbility(DBAbility dba, int level)
			: base(dba, level, eProperty.Undefined)
		{
		}

		protected override string ValueUnit { get { return "%"; } }

		public override int GetAmountForLevel(int level)
		{
			if (level < 1) return 0;
			switch (level)
			{
				case 1: return 3;
				case 2: return 9;
				case 3: return 17;
				case 4: return 27;
				case 5: return 39;
				default: return 39;
			}
		}
	}

	/// <summary>
	/// Mastery of healing ability, adds to heal spells (SpellHandler checks for it)
	/// </summary>
	public class MasteryOfHealingAbility : RAPropertyEnhancer
	{
		public MasteryOfHealingAbility(DBAbility dba, int level)
			: base(dba, level, eProperty.HealingEffectiveness)
		{
		}

		protected override string ValueUnit { get { return "%"; } }

		public override int GetAmountForLevel(int level)
		{
			if (level < 1) return 0;
			switch (level)
			{
				case 1: return 2;
				case 2: return 5;
				case 3: return 12;
				case 4: return 19;
				case 5: return 28;
				default: return 28;
			}
		}
	}

	/// <summary>
	/// Mastery of focus ability, adds to spell-level for resist bonus (SpellHandler checks for it)
	/// </summary>
	public class MasteryOfFocusAbility : RAPropertyEnhancer
	{
		public MasteryOfFocusAbility(DBAbility dba, int level) : base(dba, level, eProperty.SpellLevel) { }

		public override int GetAmountForLevel(int level)
		{
			if (level < 1) return 0;
			switch (level)
			{
				case 1: return 3;
				case 2: return 9;
				case 3: return 17;
				case 4: return 27;
				case 5: return 39;
				default: return 39;
			}
		}

		public override bool CheckRequirement(GamePlayer player)
		{
			return player.Level >= 40;
		}
	}
}