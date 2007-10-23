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
using DOL.AI;
using DOL.AI.Brain;

namespace DOL.GS.PropertyCalc
{
	/// <summary>
	/// The Living Effective Level calculator
	/// 
	/// BuffBonusCategory1 is used for buffs, uncapped
	/// BuffBonusCategory2 unused
	/// BuffBonusCategory3 unused
	/// BuffBonusCategory4 unused
	/// BuffBonusMultCategory1 unused
	/// </summary>
	[PropertyCalculator(eProperty.LivingEffectiveLevel)]
	public class LivingEffectiveLevelCalculator : PropertyCalculator
	{
		public override int CalcValue(GameLiving living, eProperty property) 
		{
			if (living is GamePlayer) 
			{
//				GamePlayer player = (GamePlayer)living;
				return living.Level + living.ItemBonus[(int)property] + living.BuffBonusCategory1[(int)property];
			} 		
			else if (living is GameNPC) 
			{
				IControlledBrain brain = ((GameNPC)living).Brain as IControlledBrain;
				if (brain != null)
					return brain.Owner.Level + living.ItemBonus[(int)property] + living.BuffBonusCategory1[(int)property];
				return living.Level + living.ItemBonus[(int)property] + living.BuffBonusCategory1[(int)property];
			}
			return 0;
		}
	}
}
