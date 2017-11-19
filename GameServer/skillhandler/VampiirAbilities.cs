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

using DOL.Database;

namespace DOL.GS.SkillHandler
{
    /// <summary>
    /// Abstract Vampiir Ability using Level Based Ability to enable stat changing with Ratio Preset.
    /// </summary>
    public abstract class VampiirAbility : LevelBasedStatChangingAbility
	{
		/// <summary>
		/// Multiplier for Ability Level to adjust Stats for given Ability 
		/// </summary>
		public abstract int RatioByLevel { get; }
				
		/// <summary>
		/// Return Amount for this Stat Changing Ability
		/// Based on Current Ability Level and Ratio Multiplier
		/// </summary>
		/// <param name="level">Targeted Ability Level</param>
		/// <returns>Stat Changing amount</returns>
		public override int GetAmountForLevel(int level)
		{
			//(+stats every level starting level 6),
			return level < 6 ? 0 : (level - 5) * RatioByLevel;
		}
		
		protected VampiirAbility(DBAbility dba, int level, eProperty property)
			: base(dba, level, property)
		{
		}
	}

	/// <summary>
	/// Vampiir Ability for Strength Stat
	/// </summary>
	public class VampiirStrength : VampiirAbility
	{
		/// <summary>
		/// Ratio Preset to *3
		/// </summary>
		public override int RatioByLevel { get { return 3; } }
		
		public VampiirStrength(DBAbility dba, int level)
			: base(dba, level, eProperty.Strength)
		{
		}
	}

	/// <summary>
	/// Vampiir Ability for Strength Stat
	/// </summary>
	public class VampiirDexterity : VampiirAbility
	{
		/// <summary>
		/// Ratio Preset to *3
		/// </summary>
		public override int RatioByLevel { get { return 3; } }

		public VampiirDexterity(DBAbility dba, int level)
			: base(dba, level, eProperty.Dexterity)
		{
		}
	}

	/// <summary>
	/// Vampiir Ability for Strength Stat
	/// </summary>
	public class VampiirConstitution : VampiirAbility
	{
		/// <summary>
		/// Ratio Preset to *3
		/// </summary>
		public override int RatioByLevel { get { return 3; } }

		public VampiirConstitution(DBAbility dba, int level)
			: base(dba, level, eProperty.Constitution)
		{
		}
	}

	/// <summary>
	/// Vampiir Ability for Strength Stat
	/// </summary>
	public class VampiirQuickness : VampiirAbility
	{
		/// <summary>
		/// Ratio Preset to *2
		/// </summary>
		public override int RatioByLevel { get { return 2; } }

		public VampiirQuickness(DBAbility dba, int level)
			: base(dba, level, eProperty.Quickness)
		{
		}
	}
}
