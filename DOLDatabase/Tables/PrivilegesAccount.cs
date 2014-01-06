using System;
using DOL.Database;
using DOL.Database.Attributes;

namespace DOL.Database
{
	[DataTable(TableName = "PrivilegesAccount")]
	public class PrivilegesAccount : DataObject
	{
        private string m_account;
		private string m_serializedRanks;

		public PrivilegesAccount()
		{
		}

		[PrimaryKey]
		public string Account
		{
			get { return m_account; }
			set
			{
				Dirty = true;
				m_account = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public string SerializedRanks
		{
			get { return m_serializedRanks; }
			set
			{
				Dirty = true;
				m_serializedRanks = value;
			}
		}
	}
}