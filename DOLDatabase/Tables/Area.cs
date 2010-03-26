using System;
using DOL.Database;
using DOL.Database.Attributes;

namespace DOL.Database
{
	[DataTable(TableName = "Area")]
	public class DBArea : DataObject
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
        private string m_points;

		public DBArea()
		{
			m_autoSave = false;
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

		[DataElement(AllowDbNull = false)]
		public string Description
		{
			get
			{
				return m_description;
			}
			set
			{
				Dirty = true;
				m_description = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int X
		{
			get
			{
				return m_x;
			}
			set
			{
				Dirty = true;
				m_x = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public int Y
		{
			get
			{
				return m_y;
			}
			set
			{
				Dirty = true;
				m_y = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public int Z
		{
			get
			{
				return m_z;
			}
			set
			{
				Dirty = true;
				m_z = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public int Radius
		{
			get
			{
				return m_radius;
			}
			set
			{
				Dirty = true;
				m_radius = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public ushort Region
		{
			get
			{
				return m_region;
			}
			set
			{
				Dirty = true;
				m_region = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public string ClassType
		{
			get
			{
				return m_classType;
			}
			set
			{
				Dirty = true;
				m_classType = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public bool CanBroadcast
		{
			get
			{
				return m_canBroadcast;
			}
			set
			{
				Dirty = true;
				m_canBroadcast = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public byte Sound
		{
			get
			{
				return m_sound;
			}
			set
			{
				Dirty = true;
				m_sound = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public bool CheckLOS
		{
			get
			{
				return m_checkLOS;
			}
			set
			{
				Dirty = true;
				m_checkLOS = value;
			}
		}

        [DataElement(AllowDbNull = true)]
        public string Points
        {
            get
            {
                return m_points;
            }
            set
            {
                Dirty = true;
                m_points = value;
            }
        }
	}
}