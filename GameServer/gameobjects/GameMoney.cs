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
using System.Collections;
using DOL.GS;
using DOL.GS.Database;

namespace DOL.GS
{
	/// <summary>
	/// Zusammenfassung für GameLoot.
	/// </summary>
	public class GameMoney: GameStaticItemTimed
	{
		/// <summary>
		/// The value of copper in this coinbag
		/// </summary>
		protected long m_copperValue;

		/// <summary>
		/// The list of money loot names
		/// </summary>
		protected static readonly string[] NAMES = new string[]{"bag of coins", "small chest", "large chest", "some copper coins"};

		/// <summary>
		/// Constructs a new Money bag with a value that will disappear after 2 minutes
		/// </summary>
		/// <param name="copperValue">the coppervalue of this bag</param>
		public GameMoney(long copperValue) : base(120000)
		{
			Level = 0;
			Model = 488;
			Realm = 0;
			m_copperValue=copperValue;
			Name = NAMES[0];
		}

		/// <summary>
		/// Checks whether the name is money item name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static bool IsItemMoney(string name) {
			for (int i=0; i<NAMES.Length; i++) {
				if (name == NAMES[i]) return true;
			}			
			return false;
		}

		/// <summary>
		/// Constructs a new Money bag with a value and the position of the dropper
		/// It will disappear after 2 minutes
		/// </summary>
		/// <param name="copperValue">the coppervalue of this bag</param>
		/// <param name="dropper">the gameobject that dropped this bag</param>
		public GameMoney(long copperValue, GameObject dropper):this(copperValue)
		{
			Position = dropper.Position;
			Heading = dropper.Heading;
			Region = dropper.Region;
		}
		/// <summary>
		/// Returns the number of mithril pieces in this bag
		/// </summary>
		public int Mithril
		{
			get
			{
				return Money.GetMithril(m_copperValue);
			}
		}

		/// <summary>
		/// Returns the number of platinum pieces in this bag
		/// </summary>
		public int Platinum
		{
			get
			{
				return Money.GetPlatinum(m_copperValue);
			}
		}

		/// <summary>
		/// Returns the number of gold pieces in this bag
		/// </summary>
		public int Gold
		{
			get
			{
				return Money.GetGold(m_copperValue);
			}
		}

		/// <summary>
		/// Returns the number of silver pieces in this bag
		/// </summary>
		public int Silver
		{
			get
			{
				return Money.GetSilver(m_copperValue);
			}
		}

		/// <summary>
		/// Returns the number of copper pieces in this bag
		/// </summary>
		public int Copper
		{
			get
			{
				return Money.GetCopper(m_copperValue);
			}
		}

		/// <summary>
		/// Gets/Sets the total copper value of this bag
		/// </summary>
		public long TotalCopper
		{
			get
			{
				return m_copperValue;
			}
			set
			{
				m_copperValue = value;
			}
		}
	}
}
