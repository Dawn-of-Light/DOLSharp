using System;
using DOL.Database.Attributes;

namespace DOL.Database
{
	[DataTable(TableName = "KeepCaptureLog")]
	public class KeepCaptureLog : DataObject
	{
		long m_ID;
		DateTime m_dateTaken = DateTime.Now;
		string m_keepName;
		string m_keepType;
		int m_numEnemies;
		int m_rpReward;
		int m_bpReward;
		long m_xpReward;
		long m_moneyReward;
		int m_combatTime;
		string m_capturedBy;
		string m_rpGainerList = "";

		public KeepCaptureLog()
			: base()
		{
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
		public DateTime DateTaken
		{
			get { return m_dateTaken; }
			set
			{
				Dirty = true;
				m_dateTaken = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public string KeepName
		{
			get { return m_keepName; }
			set
			{
				Dirty = true;
				m_keepName = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public string KeepType
		{
			get { return m_keepType; }
			set
			{
				Dirty = true;
				m_keepType = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public int NumEnemies
		{
			get { return m_numEnemies; }
			set
			{
				Dirty = true;
				m_numEnemies = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public int CombatTime
		{
			get { return m_combatTime; }
			set
			{
				Dirty = true;
				m_combatTime = value;
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
		public int BPReward
		{
			get { return m_bpReward; }
			set
			{
				Dirty = true;
				m_bpReward = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public long XPReward
		{
			get { return m_xpReward; }
			set
			{
				Dirty = true;
				m_xpReward = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public long MoneyReward
		{
			get { return m_moneyReward; }
			set
			{
				Dirty = true;
				m_moneyReward = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public string CapturedBy
		{
			get { return m_capturedBy; }
			set
			{
				Dirty = true;
				m_capturedBy = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public string RPGainerList
		{
			get { return m_rpGainerList; }
			set
			{
				Dirty = true;
				m_rpGainerList = value;
			}
		}
	}
}
