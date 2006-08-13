using DOL.Database;
using DOL.Database.Attributes;
using System;

namespace DOL.Database
{
	/// <summary>
	/// Table to hold positions of various Area Capture objects
	/// </summary>
	[DataTable(TableName = "KeepPosition")]
	public class DBKeepPosition : DataObject
	{
		private int m_componentSkin;
		private string m_templateID;
		private int m_height;
		private int m_x;
		private int m_y;
		private int m_z;
		private int m_h;
		private string m_classType;

		private bool m_autoSave;

		/// <summary>
		/// Constructor
		/// </summary>
		public DBKeepPosition()
		{
			m_autoSave = true;
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

		/// <summary>
		/// The Skin ID of the Keep Component the Position is assigned to
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int ComponentSkin
		{
			get
			{
				return m_componentSkin;
			}
			set
			{
				Dirty = true;
				m_componentSkin = value;
			}
		}

		/// <summary>
		/// The KeepObjectID, consider this a template ID
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string TemplateID
		{
			get
			{
				return m_templateID;
			}
			set
			{
				Dirty = true;
				m_templateID = value;
			}
		}

		/// <summary>
		/// The Height that this position is stored for, 0,1,2,3
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int Height
		{
			get
			{
				return m_height;
			}
			set
			{
				Dirty = true;
				m_height = value;
			}
		}

		/// <summary>
		/// X Offset Position of the object for the component
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int XOff
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

		/// <summary>
		/// Y Offset Position of the object for the component
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int YOff
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


		/// <summary>
		/// Z Offset Position of the object for the component
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int ZOff
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

		/// <summary>
		/// Heading Offset Position of the object for the component
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int HOff
		{
			get
			{
				return m_h;
			}
			set
			{
				Dirty = true;
				m_h = value;
			}
		}

		/// <summary>
		/// The type of object this position will hold
		/// </summary>
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
	}
}
