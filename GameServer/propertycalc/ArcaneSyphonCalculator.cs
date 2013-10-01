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
    /// [Freya] Nidel. 
    /// The ArcaneSyphon calculator: 25% cap like live server: http://www.camelotherald.com/more/3202.shtml
    ///
    /// BuffBonusCategory1 unused
    /// BuffBonusCategory2 unused
    /// BuffBonusCategory3 unused
    /// BuffBonusCategory4 unused
    /// BuffBonusMultCategory1 unused
    /// </summary>
    [PropertyCalculator(eProperty.ArcaneSyphon)]
    public class ArcaneSyphonCalculator : PropertyCalculator
    {
    	public const int PROPERTY_ARCANESYPHON_HARDCAP = 25;
    	
        public override int CalcValue(GameLiving living, eProperty property)
        {
            GamePlayer player = living as GamePlayer;
            if(player == null)
            {
                return 0;
            }
            
            return Math.Min(living.ItemBonus[(int)property], PROPERTY_ARCANESYPHON_HARDCAP+GameMythirian.GetMythicalOverCapBonuses(living, property));
        }
    }
}