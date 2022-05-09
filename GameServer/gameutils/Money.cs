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
using DOL.GS.Finance;

namespace DOL.GS
{
	public class Money
	{
		public static int GetMithril(long money)
		{
			return (int)(money / 100L / 100L / 1000L / 1000L % 1000L);
		}

		public static int GetPlatinum(long money)
		{
			return (int)(money / 100L / 100L / 1000L % 1000L);
		}

		public static int GetGold(long money)
		{
			return (int)(money / 100L / 100L % 1000L);
		}

		public static int GetSilver(long money)
		{
			return (int)(money / 100L % 100L);
		}

		public static int GetCopper(long money)
		{
			return (int)(money % 100L);
		}

		public static long GetMoney(int mithril, int platinum, int gold, int silver, int copper)
		{
			return ((((long)mithril * 1000L + (long)platinum) * 1000L + (long)gold) * 100L + (long)silver) * 100L + (long)copper;
		}

		public static string GetString(long money)
		{
			return Currency.Copper.Mint(money).ToText();
		}

		public static string GetShortString(long money)
		{
			return Currency.Copper.Mint(money).ToAbbreviatedText();
		}

		[Obsolete("This is going to be removed without replacement.")]
		public static long SetAutoPrice(int level, int quality)
		{
			int levelmod = level * level * level;

			double dCopper = (levelmod / 0.6); // level 50, 100 quality; worth aprox 20 gold, sells for 10 gold
			double dQuality = quality / 100.0;

			dCopper = dCopper * dQuality * dQuality * dQuality * dQuality * dQuality * dQuality;

			if (dCopper < 2)
				dCopper = 2;

			return (long)dCopper;
		}
	}
}