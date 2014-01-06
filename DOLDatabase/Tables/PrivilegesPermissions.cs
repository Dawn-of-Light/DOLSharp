using System;
using DOL.Database;
using DOL.Database.Attributes;

namespace DOL.Database
{
	[DataTable(TableName = "PrivilegesPermissions")]
	public class PrivilegesPermission : DataObject
	{
		private string m_id;
		private string m_command;
		private string m_subcommand;

		public PrivilegesPermission()
		{
		}

		[DataElement(AllowDbNull = false)]
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
		public string Command
		{
			get { return m_command; }
			set
			{
				Dirty = true;
				m_command = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public string SubCommand
		{
			get { return m_subcommand; }
			set
			{
				Dirty = true;
				m_subcommand = value;
			}
		}
	}
}