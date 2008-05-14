using System;


namespace DOL.Database2
{
	/// <summary>
	/// Class => Realm abilities collection
	/// </summary>
	[Serializable]//TableName = "ClassXRealmAbility")]
	public class ClassXRealmAbility : DatabaseObject
	{
		static bool m_autoSave;
		protected string m_abilityKey;
		protected int m_charClass;

        public ClassXRealmAbility()
            : base()
		{
			m_autoSave = false;
		}

		
		/// <summary>
		/// Char class that can get this ability
		/// </summary>
		
		public int CharClass
		{
			get { return m_charClass; }
			set
			{
				m_Dirty = true;
				m_charClass = value;
			}
		}

		/// <summary>
		/// The key of this ability
		/// </summary>
		
		public string AbilityKey
		{
			get { return m_abilityKey; }
			set
			{
				m_Dirty = true;
				m_abilityKey = value;
			}
		}
	}
}