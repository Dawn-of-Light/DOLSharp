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
			if (ServerProperties.Properties.USE_NEW_PASSIVES_RAS_SCALING)
			{
				switch (level)
				{
						case 1: return 3;
						case 2: return 6;
						case 3: return 9;
						case 4: return 13;
						case 5: return 17;
						case 6: return 22;
						case 7: return 27;
						case 8: return 33;
						case 9: return 39;
						default: return 39;
				}
			}
			else
			{
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
			if (ServerProperties.Properties.USE_NEW_PASSIVES_RAS_SCALING)
			{
				switch (level)
				{
						case 1: return 2;
						case 2: return 3;
						case 3: return 4;
						case 4: return 6;
						case 5: return 8;
						case 6: return 10;
						case 7: return 12;
						case 8: return 14;
						case 9: return 16;
						default: return 16;
				}
			}
			else
			{
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
	}


	/// <summary>
	/// Wild healing ability, critical heal chance bonus to heal spells (SpellHandler checks for it)
	/// </summary>
	public class WildHealingAbility : RAPropertyEnhancer
	{
		public WildHealingAbility(DBAbility dba, int level)
			: base(dba, level, eProperty.CriticalHealHitChance)
		{
		}

		protected override string ValueUnit { get { return "%"; } }

		public override int GetAmountForLevel(int level)
		{
			if (level < 1) return 0;
			if (ServerProperties.Properties.USE_NEW_PASSIVES_RAS_SCALING)
			{
				switch (level)
				{
						case 1: return 3;
						case 2: return 6;
						case 3: return 9;
						case 4: return 13;
						case 5: return 17;
						case 6: return 22;
						case 7: return 27;
						case 8: return 33;
						case 9: return 39;
						default: return 39;
				}
			}
			else
			{
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
			if (ServerProperties.Properties.USE_NEW_PASSIVES_RAS_SCALING)
			{
				switch (level)
				{
						case 1: return 2;
						case 2: return 4;
						case 3: return 6;
						case 4: return 9;
						case 5: return 12;
						case 6: return 16;
						case 7: return 20;
						case 8: return 25;
						case 9: return 30;
						default: return 30;
				}
			}
			else
			{
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
			if (ServerProperties.Properties.USE_NEW_PASSIVES_RAS_SCALING)
			{
				switch (level)
				{
						case 1: return 3;
						case 2: return 6;
						case 3: return 9;
						case 4: return 13;
						case 5: return 17;
						case 6: return 22;
						case 7: return 27;
						case 8: return 33;
						case 9: return 39;
						default: return 39;
				}
			}
			else
			{
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

		public override bool CheckRequirement(GamePlayer player)
		{
			return player.Level >= 40;
		}
	}
}
