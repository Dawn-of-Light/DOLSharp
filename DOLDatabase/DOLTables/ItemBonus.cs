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

using DOL.Database.Attributes;

namespace DOL.Database
{
	/// <summary>
	/// The bonus set for an item
	/// </summary>
	[DataTable(TableName = "BonusXItem")]
	public class ItemBonus : DataObject
	{
		public const byte MAX_BONUS = 10;
		public const byte CHARGE1 = 15;
		public const byte CHARGE2 = 16;
		public const byte PROC1 = 17;
		public const byte PROC2 = 18;

		private string m_itemID;
		private byte m_bonusType;
		private int m_bonusAmount;
		private byte m_bonusLevel;

		public ItemBonus()
			: base()
		{ }

		public ItemBonus(string itemID, byte bonusType, int bonusAmount)
			: base()
		{
			m_itemID = itemID;
			m_bonusType = bonusType;
			m_bonusAmount = bonusAmount;
		}

		public override bool AutoSave
		{
			get { return true; }
			set { }
		}

		[DataElement(AllowDbNull = false)]
		public string ItemID
		{
			get { return m_itemID; }
			set
			{
				Dirty = true;
				m_itemID = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public byte BonusType
		{
			get { return m_bonusType; }
			set
			{
				Dirty = true;
				m_bonusType = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public int BonusAmount
		{
			get { return m_bonusAmount; }
			set
			{
				Dirty = true;
				m_bonusAmount = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public byte BonusLevel
		{
			get { return m_bonusLevel; }
			set
			{
				Dirty = true;
				m_bonusLevel = value;
			}
		}
	}
}
