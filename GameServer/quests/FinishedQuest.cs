using System;

namespace DOL.GS.Quests
{
	/// <summary>
	/// Description résumée de FinishedQuest.
	/// </summary>
	public class FinishedQuest
	{
		#region Declaration
		/// <summary>
		/// The unique id of this quest finished
		/// </summary>
		protected Type m_finishedQuestType;

		/// <summary>
		/// How much time the player has finished the quest
		/// </summary>
		protected byte m_count;

		/// <summary>
		/// Gets or Sets the type of this quest finished
		/// </summary>
		public Type FinishedQuestType
		{
			get { return m_finishedQuestType; }
			set { m_finishedQuestType = value; }
		}

		/// <summary>
		/// Gets or sets the player doing the quest
		/// </summary>
		public byte Count
		{
			get	{ return m_count; }
			set	{ m_count = value; }
		}
		#endregion
	}
}
