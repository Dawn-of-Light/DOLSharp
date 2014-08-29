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

namespace DOL.GS.PropertyCalc
{
	/// <summary>
	/// BladeturnReinforcementCalculator for Bladeturn level improvement against higher level attacker.
	/// Absolute value capped to Level (Level + this for chance computing at most)
	/// No debuff.
	/// </summary>
	[PropertyCalculator(eProperty.BladeturnReinforcement)]
	public class BladeturnReinforcementCalculator : GenericCappedPropertyCalculator
	{
		/// <summary>
		/// HardCap for Total Value (Ability+Other+Base-Debuff)
		/// </summary>
		public override int HardCap(GameLiving living)
		{
			return living.Level;
		}
		
		/// <summary>
		/// Cap For Item Bonuses
		/// </summary>
		public override int ItemCap(GameLiving living)
		{
			return living.Level / 2;
		}

		/// <summary>
		/// Cap For Buff Bonuses
		/// </summary>
		public override int BuffCap(GameLiving living)
		{
			return living.Level / 2;
		}
		
		/// <summary>
		/// Cap For Ability Bonuses
		/// </summary>
		public override int AbilityCap(GameLiving living)
		{
			return living.Level;
		}
		
		/// <summary>
		/// Cap For Base Item + Buff Bonuses
		/// </summary>
		public override int BaseCap(GameLiving living)
		{
			return living.Level;
		}
		
		/// <summary>
		/// Cap For Debuff (Minimal Total Value / Base Value)
		/// </summary>
		public override int DebuffCap(GameLiving living)
		{
			return 0;
		}
	}
}
