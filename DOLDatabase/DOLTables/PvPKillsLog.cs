using System;
using DOL.Database.Attributes;

namespace DOL.Database
{
	[DataTable(TableName = "PvPKillsLog")]
	public class PvPKillsLog : DataObject
	{
		long m_ID;
		DateTime m_dateKilled = DateTime.Now;
		string m_killedName;
		string m_killerName;
		string m_killerIP;
		string m_killedIP;
		string m_killerRealm;
		string m_killedRealm;
		int m_rpReward;
		byte m_sameIP = 0;

		private bool m_autoSave;

		public PvPKillsLog()
			: base()
		{
		}

		public override bool AutoSave
		{
			get { return m_autoSave; }
			set { m_autoSave = value; }
		}


		[PrimaryKey(AutoIncrement = true)]
		public long ID
		{
			get { return m_ID; }
			set
			{
				Dirty = true;
				m_ID = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public DateTime DateKilled
		{
			get { return m_dateKilled; }
			set
			{
				Dirty = true;
				m_dateKilled = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public string KilledName
		{
			get { return m_killedName; }
			set
			{
				Dirty = true;
				m_killedName = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public string KillerName
		{
			get { return m_killerName; }
			set
			{
				Dirty = true;
				m_killerName = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public string KillerIP
		{
			get { return m_killerIP; }
			set
			{
				Dirty = true;
				m_killerIP = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public string KilledIP
		{
			get { return m_killedIP; }
			set
			{
				Dirty = true;
				m_killedIP = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public string KilledRealm
		{
			get { return m_killedRealm; }
			set
			{
				Dirty = true;
				m_killedRealm = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public string KillerRealm
		{
			get { return m_killerRealm; }
			set
			{
				Dirty = true;
				m_killerRealm = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public int RPReward
		{
			get { return m_rpReward; }
			set
			{
				Dirty = true;
				m_rpReward = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public byte SameIP
		{
			get { return m_sameIP; }
			set
			{
				Dirty = true;
				m_sameIP = value;
			}
		}

	}
}
