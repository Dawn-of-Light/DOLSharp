using System;
using DOL.Database;
using DOL.Database.Attributes;

namespace DOL.Database
{
	[DataTable(TableName = "PrivilegesRank")]
	public class PrivilegesRank : DataObject
	{
		private string m_id;
		private string m_name;

		public PrivilegesRank()
		{
		}

		[PrimaryKey]
		public string ID
		{
			get { return m_id; }
			set
			{
				Dirty = true;
				m_id = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public string Name
		{
			get { return m_name; }
			set
			{
				Dirty = true;
				m_name = value;
			}
		}
	}
}