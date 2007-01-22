using System;

using DOL.Database;
using DOL.Database.Attributes;

namespace DOL
{
	namespace Database
	{
		/// <summary>
		/// Account table
		/// </summary>
		[DataTable(TableName = "PlayerXEffect")]
		public class PlayerXEffect : DataObject
		{
			private string m_charid;
			private string m_effecttype;
			private bool m_ishandler;
			private int m_duration;
			private int m_var1;
			private int m_var2;
			private int m_var3;
			private int m_var4;
			private int m_var5;
			private int m_var6;
			private static bool m_autoSave;

			public PlayerXEffect()
			{

				m_autoSave = false;
			}
			[DataElement(AllowDbNull = true)]
			public bool IsHandler
			{
				get
				{
					return m_ishandler;
				}
				set
				{
					Dirty = true;
					m_ishandler = value;
				}
			}
			[DataElement(AllowDbNull = true)]
			public int Var6
			{
				get
				{
					return m_var6;
				}
				set
				{
					Dirty = true;
					m_var6 = value;
				}
			}
			[DataElement(AllowDbNull = true)]
			public int Var5
			{
				get
				{
					return m_var5;
				}
				set
				{
					Dirty = true;
					m_var5 = value;
				}
			}
			[DataElement(AllowDbNull = true)]
			public int Var4
			{
				get
				{
					return m_var4;
				}
				set
				{
					Dirty = true;
					m_var4 = value;
				}
			}
			[DataElement(AllowDbNull = true)]
			public int Var3
			{
				get
				{
					return m_var3;
				}
				set
				{
					Dirty = true;
					m_var3 = value;
				}
			}
			[DataElement(AllowDbNull = true)]
			public int Var2
			{
				get
				{
					return m_var2;
				}
				set
				{
					Dirty = true;
					m_var2 = value;
				}
			}
			[DataElement(AllowDbNull = true)]
			public int Var1
			{
				get
				{
					return m_var1;
				}
				set
				{
					Dirty = true;
					m_var1 = value;
				}
			}
			[DataElement(AllowDbNull = true)]
			public int Duration
			{
				get
				{
					return m_duration;
				}
				set
				{
					Dirty = true;
					m_duration = value;
				}
			}


			[DataElement(AllowDbNull = true)]
			public string EffectType
			{
				get
				{
					return m_effecttype;
				}
				set
				{
					Dirty = true;
					m_effecttype = value;
				}
			}


			override public bool AutoSave
			{
				get
				{
					return m_autoSave;
				}
				set
				{
					m_autoSave = value;
				}
			}

			[DataElement(AllowDbNull = true)]
			public string ChardID
			{
				get
				{
					return m_charid;
				}
				set
				{
					Dirty = true;
					m_charid = value;
				}
			}

		}
	}
}