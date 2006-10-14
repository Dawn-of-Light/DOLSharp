using System;

using DOL.Events;

namespace DOL.GS.Quests
{
	public class KillMission : AbstractMission
	{
		private Type m_targetType = null;
		private int m_total = 0;
		private int m_current = 0;
		private string m_desc = "";

		public KillMission(Type targetType, int total, string desc, object owner)
			: base(owner)
		{
			m_targetType = targetType;
			m_total = total;
			m_desc = desc;
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			if (e != GameLivingEvent.EnemyKilled)
				return;

			EnemyKilledEventArgs eargs = args as EnemyKilledEventArgs;

			if (eargs.Target.GetType().Equals(m_targetType))
				return;

			m_current++;
			UpdateMission();
			if (m_current == m_total)
				FinishMission();

		}

		public override string Description
		{
			get
			{
				return "Kill " + m_total + " " + m_desc + ", you have killed " + m_current + ".";
			}
		}
	}
}