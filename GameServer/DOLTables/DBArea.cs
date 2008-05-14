using System;
using DOL.Database2;


namespace DOL.Database2
{
	[Serializable]//TableName = "Area")]
	public class DBArea : DatabaseObject
	{
		private string m_description;
		private int m_x;
		private int m_y;
		private int m_z;
		private int m_radius;
		private ushort m_region;
		private string m_classType = "";
		private static bool m_autoSave;
		private bool m_canBroadcast;
		private byte m_sound;
		private bool m_checkLOS;

        public DBArea()
            : base()
		{
			m_autoSave = false;
		}
		
		public string Description
		{
			get
			{
				return m_description;
			}
			set
			{
				m_Dirty = true;
				m_description = value;
			}
		}

		//[DataElement(AllowDbNull=true)]
		public int X
		{
			get
			{
				return m_x;
			}
			set
			{
				m_Dirty = true;
				m_x = value;
			}
		}

		
		public int Y
		{
			get
			{
				return m_y;
			}
			set
			{
				m_Dirty = true;
				m_y = value;
			}
		}

		
		public int Z
		{
			get
			{
				return m_z;
			}
			set
			{
				m_Dirty = true;
				m_z = value;
			}
		}

		
		public int Radius
		{
			get
			{
				return m_radius;
			}
			set
			{
				m_Dirty = true;
				m_radius = value;
			}
		}

		
		public ushort Region
		{
			get
			{
				return m_region;
			}
			set
			{
				m_Dirty = true;
				m_region = value;
			}
		}

		//[DataElement(AllowDbNull=true)]
		public string ClassType
		{
			get
			{
				return m_classType;
			}
			set
			{
				m_Dirty = true;
				m_classType = value;
			}
		}

		//[DataElement(AllowDbNull=true)]
		public bool CanBroadcast
		{
			get
			{
				return m_canBroadcast;
			}
			set
			{
				m_Dirty = true;
				m_canBroadcast = value;
			}
		}

		//[DataElement(AllowDbNull=true)]
		public byte Sound
		{
			get
			{
				return m_sound;
			}
			set
			{
				m_Dirty = true;
				m_sound = value;
			}
		}

		//[DataElement(AllowDbNull=true)]
		public bool CheckLOS
		{
			get
			{
				return m_checkLOS;
			}
			set
			{
				m_Dirty = true;
				m_checkLOS = value;
			}
		}
	}
}