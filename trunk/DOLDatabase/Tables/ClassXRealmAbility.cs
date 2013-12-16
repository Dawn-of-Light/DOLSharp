using System;
using DOL.Database.Attributes;

namespace DOL.Database
{
	/// <summary>
	/// Class => Realm abilities collection
	/// </summary>
	[DataTable(TableName = "ClassXRealmAbility")]
	public class ClassXRealmAbility : DataObject
	{
		protected string m_abilityKey;
		protected int m_charClass;

		public ClassXRealmAbility()
		{
			AllowAdd = false;
		}

		/// <summary>
		/// Char class that can get this ability
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public int CharClass
		{
			get { return m_charClass; }
			set
			{
				Dirty = true;
				m_charClass = value;
			}
		}

		/// <summary>
		/// The key of this ability
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public string AbilityKey
		{
			get { return m_abilityKey; }
			set
			{
				Dirty = true;
				m_abilityKey = value;
			}
		}
	}
}