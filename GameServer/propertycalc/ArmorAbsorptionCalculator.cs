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
	/// The Armor Absorption calculator / bonus without armors...
	/// </summary>
	[PropertyCalculator(eProperty.ArmorAbsorption)]
	public class ArmorAbsorptionCalculator : GenericPercentCalculator
	{
		/// <summary>
		/// 90% Hard Cap
		/// </summary>
		/// <param name="living"></param>
		/// <returns></returns>
		public override int HardCap(GameLiving living)
		{
			return 90;
		}
		
		/// <summary>
		/// 50% Ability Cap
		/// </summary>
		/// <param name="living"></param>
		/// <returns></returns>
		public override int AbilityCap(GameLiving living)
		{
			return 50;
		}
		
		/// <summary>
		/// 35% Debuff Cap to prevent overdamage 
		/// </summary>
		/// <param name="living"></param>
		/// <returns></returns>
		public override int DebuffCap(GameLiving living)
		{
			return -35;
		}
		
		/// <summary>
		/// 35% for buff Cap (spec 25+base 10)
		/// </summary>
		/// <param name="living"></param>
		/// <returns></returns>
		public override int BuffCap(GameLiving living)
		{
			return 35;
		}
		
		/// <summary>
		/// 10% for Item cap (shouldn't be abs on items)
		/// </summary>
		/// <param name="living"></param>
		/// <returns></returns>
		public override int ItemCap(GameLiving living)
		{
			return 10;
		}
		
		/// <summary>
		/// Calc for GameNPC.
		/// </summary>
		/// <param name="living"></param>
		/// <param name="property"></param>
		/// <returns></returns>
		public override int CalcValueBase(GameLiving living, eProperty property)
		{
			if(living is GameNPC)
			{
				GameNPC npc = ((GameNPC)living);
				
				// For NPC Absorb is based on some stats buffs, debuff would also reduce absorb !! (modCon+modDex / con+dex - 1 ) buffed or debuffed, halved and raised to base 100
				return Math.Max(DebuffCap(living), Math.Min(BaseCap(living), base.CalcValueBase(living, property)+((int)((((double)(living.GetModified(eProperty.Constitution)+living.GetModified(eProperty.Dexterity))/(double)(npc.Constitution+npc.Dexterity))-1.0)*100) >> 1)));
			}
			
			return base.CalcValueBase(living, property);
		}
	}
}
