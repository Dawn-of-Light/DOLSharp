using System;

using DOL.Database2;


namespace DOL
{
	namespace Database2
	{
		/// <summary>
		/// Account table
		/// </summary>
		[Serializable]//TableName = "PlayerXEffect")]
		public class PlayerXEffect : DatabaseObject
		{
			private UInt64 m_charid;
			private string m_effecttype;
			private bool m_ishandler;
			private int m_duration;
			private int m_var1;
			private int m_var2;
			private int m_var3;
			private int m_var4;
			private int m_var5;
			private int m_var6;
			private string m_spellLine;
			private static bool m_autoSave;

            public PlayerXEffect()
                : base()
			{

				m_autoSave = false;
			}
			//[DataElement(AllowDbNull=true)]
			public bool IsHandler
			{
				get
				{
					return m_ishandler;
				}
				set
				{
					m_Dirty = true;
					m_ishandler = value;
				}
			}
			//[DataElement(AllowDbNull=true)]
			public int Var6
			{
				get
				{
					return m_var6;
				}
				set
				{
					m_Dirty = true;
					m_var6 = value;
				}
			}
			//[DataElement(AllowDbNull=true)]
			public int Var5
			{
				get
				{
					return m_var5;
				}
				set
				{
					m_Dirty = true;
					m_var5 = value;
				}
			}
			//[DataElement(AllowDbNull=true)]
			public int Var4
			{
				get
				{
					return m_var4;
				}
				set
				{
					m_Dirty = true;
					m_var4 = value;
				}
			}
			//[DataElement(AllowDbNull=true)]
			public int Var3
			{
				get
				{
					return m_var3;
				}
				set
				{
					m_Dirty = true;
					m_var3 = value;
				}
			}
			//[DataElement(AllowDbNull=true)]
			public int Var2
			{
				get
				{
					return m_var2;
				}
				set
				{
					m_Dirty = true;
					m_var2 = value;
				}
			}
			//[DataElement(AllowDbNull=true)]
			public int Var1
			{
				get
				{
					return m_var1;
				}
				set
				{
					m_Dirty = true;
					m_var1 = value;
				}
			}
			//[DataElement(AllowDbNull=true)]
			public int Duration
			{
				get
				{
					return m_duration;
				}
				set
				{
					m_Dirty = true;
					m_duration = value;
				}
			}


			//[DataElement(AllowDbNull=true)]
			public string EffectType
			{
				get { return m_effecttype; }
				set
				{
					m_Dirty = true;
					m_effecttype = value;
				}
			}

			//[DataElement(AllowDbNull=true)]
			public string SpellLine
			{
				get { return m_spellLine; }
				set
				{
					m_Dirty = true;
					m_spellLine = value;
				}
			}



			//[DataElement(AllowDbNull = true, Index = true)]
			public UInt64 ChardID
			{
				get
				{
					return m_charid;
				}
				set
				{
					m_Dirty = true;
					m_charid = value;
				}
			}

		}
	}
}