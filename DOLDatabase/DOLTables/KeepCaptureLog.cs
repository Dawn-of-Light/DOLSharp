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
		int m_combatTime;
		string m_capturedBy;
		private bool m_autoSave;

		public KeepCaptureLog()
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
		public string CapturedBy
		{
			get { return m_capturedBy; }
			set
			{
				Dirty = true;
				m_capturedBy = value;
			}
		}
	}
}
