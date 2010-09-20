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
using System.Text;
using DOL.GS.ServerProperties;
using DOL.Language;

namespace DOL.GS
{
	/// <summary>
	/// capsulate money operations
	/// currently there is no instance of Money
	/// use long instead
	/// </summary>
	public class Money
	{
		private Money()
		{
		}

		public static int GetMithril(long money)
		{
			return (int) (money/100L/100L/1000L/1000L);
		}

		public static int GetPlatinum(long money)
		{
			return (int) (money/100L/100L/1000L%1000L);
		}

		public static int GetGold(long money)
		{
			return (int) (money/100L/100L%1000L);
		}

		public static int GetSilver(long money)
		{
			return (int) (money/100L%100L);
		}

		public static int GetCopper(long money)
		{
			return (int) (money%100L);
		}

		public static long GetMoney(int mithril, int platinum, int gold, int silver, int copper)
		{
			return ((((long)mithril * 1000L + (long)platinum) * 1000L + (long)gold) * 100L + (long)silver) * 100L + (long)copper;
		}

		/// <summary>
		/// return different formatted strings for money
		/// </summary>
		/// <param name="money"></param>
		/// <returns></returns>
		public static string GetString(long money)
		{
			if (money == 0)
				return LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "Money.GetString.Text1");

			int copper = GetCopper(money);
			int silver = GetSilver(money);
			int gold = GetGold(money);
			int platin = GetPlatinum(money);
			int mithril = GetMithril(money);

			var res = new StringBuilder();
			if (mithril != 0)
			{
				res.Append(mithril);
				res.Append(" ");
				res.Append(LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "Money.GetString.Text2"));
				res.Append(" ");
			}
			if (platin != 0)
			{
				res.Append(platin);
				res.Append(" ");
				res.Append(LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "Money.GetString.Text3"));
				res.Append(" ");
			}
			if (gold != 0)
			{
				res.Append(gold);
				res.Append(" ");
				res.Append(LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "Money.GetString.Text4"));
				res.Append(" ");
			}
			if (silver != 0)
			{
				res.Append(silver);
				res.Append(" ");
				res.Append(LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "Money.GetString.Text5"));
				res.Append(" ");
			}
			if (copper != 0)
			{
				res.Append(copper);
				res.Append(" ");
				res.Append(LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "Money.GetString.Text6"));
				res.Append(" ");
			}

			// remove last comma
			if (res.Length > 1)
				res.Length -= 2;

			return res.ToString();
		}

		public static string GetShortString(long money)
		{
			if (money == 0)
				return LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "Money.GetString.Text1");

			int copper = GetCopper(money);
			int silver = GetSilver(money);
			int gold = GetGold(money);
			int platin = GetPlatinum(money);
			int mithril = GetMithril(money);

			var res = new StringBuilder();
			if (mithril != 0)
			{
				res.Append(mithril);
				res.Append("m, ");
			}
			if (platin != 0)
			{
				res.Append(platin);
				res.Append("p, ");
			}
			if (gold != 0)
			{
				res.Append(gold);
				res.Append("g, ");
			}
			if (silver != 0)
			{
				res.Append(silver);
				res.Append("s, ");
			}
			if (copper != 0)
			{
				res.Append(copper);
				res.Append("c, ");
			}

			// remove last comma
			if (res.Length > 1)
				res.Length -= 2;

			return res.ToString();
		}

		/// <summary>
		/// Calculate an approximate price for given level and quality
		/// </summary>
		/// <param name="level"></param>
		/// <param name="quality"></param>
		/// <returns></returns>
		public static long SetAutoPrice(int level, int quality)
		{
			int levelmod = level*level*level;

			double dCopper = (levelmod/0.6); // level 50, 100 quality; worth aprox 20 gold, sells for 10 gold
			double dQuality = quality/100.0;

			dCopper = dCopper*dQuality*dQuality*dQuality*dQuality*dQuality*dQuality;

			if (dCopper < 2)
				dCopper = 2;

			return (long) dCopper;
		}
	}
}