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
		static bool m_autoSave;
		protected string m_abilityKey;
		protected int m_charClass;

		public ClassXRealmAbility()
		{
			m_autoSave = false;
		}

		/// <summary>
		/// auto save Db or not
		/// </summary>
		override public bool AutoSave
		{
			get { return m_autoSave; }
			set { m_autoSave = value; }
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