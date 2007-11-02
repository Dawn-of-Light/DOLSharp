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
using System.Reflection;
using log4net;

namespace DOL.GS.PropertyCalc
{
	/// <summary>
	/// The Concentration point calculator
	/// 
	/// BuffBonusCategory1 unused
	/// BuffBonusCategory2 unused
	/// BuffBonusCategory3 unused
	/// BuffBonusCategory4 unused
	/// BuffBonusMultCategory1 unused
	/// </summary>
	[PropertyCalculator(eProperty.MaxConcentration)]
	public class MaxConcentrationCalculator : PropertyCalculator
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public MaxConcentrationCalculator() {}

		public override int CalcValue(GameLiving living, eProperty property) 
		{
			if (living is GamePlayer) 
			{
				GamePlayer player = living as GamePlayer;
				if (player.CharacterClass.ManaStat == eStat.UNDEFINED) 
                    return 1000000;

				int concBase = (int)((player.Level * 4) * 2.2);
				int stat = player.GetModified((eProperty)player.CharacterClass.ManaStat);
				int factor = (stat > 50) ? (stat - 50) / 2 : (stat - 50);
				int conc = (concBase + concBase * factor / 100) / 2;
				conc = (int)(player.Effectiveness * (double)conc);

				if (conc < 0)
				{
					if (log.IsWarnEnabled)
						log.WarnFormat(living.Name+": concentration is less than zerro (conc:{0} eff:{1:R} concBase:{2} stat:{3} factor:{4})", conc, player.Effectiveness, concBase, stat, factor);
					conc = 0;
				}

                if (player.GetSpellLine("Perfecter") != null
				   && player.MLLevel >= 4)
                    conc += (20 * conc / 100);

				return conc;
			} 
			else 
			{
				return 1000000;	// default
			}
		}
	}
}
