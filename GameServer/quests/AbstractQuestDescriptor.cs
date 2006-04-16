using System;
using System.Text;
using DOL.Events;

namespace DOL.GS.Quests
{
	/// <summary>
	/// QuestDscriptor instance are stobe by npc and 
	/// used to know if they can give a quest to a player
	/// </summary>
	public abstract class AbstractQuestDescriptor
	{
		#region Declaration

		/// <summary>
		/// The unique id of this quest requirement instance
		/// </summary>
		ushort descriptorUniqueID;

		/// <summary>
		/// Retrive the unique id of this quest requirement instance
		/// </summary>
		public ushort DescriptorUniqueID
		{
			get { return descriptorUniqueID; }
			set { descriptorUniqueID = value; }
		}

		/// <summary>
		/// Get the type of the quest linked with this requirement
		/// </summary>
		public abstract Type LinkedQuestType
		{
			get;
		}

		/// <summary>
		/// Retrieves how many time player can do the quest
		/// </summary>
		public virtual int MaxQuestCount
		{
			get { return 1; }
		}

		/// <summary>
		/// Retrieves the min level to do this quest
		/// </summary>
		public virtual int MinLevel
		{
			get { return 1; }
		}

		/// <summary>
		/// Retrieves the max level to do this quest
		/// </summary>
		public virtual int MaxLevel
		{
			get { return 50; }
		}
		#endregion

		#region Function

		/// <summary>
		/// This method checks if a player qualifies for the linked quest
		/// </summary>
		/// <returns>true if qualified, false if not</returns>
		public virtual bool CheckQuestQualification(GamePlayer player)
		{
			// if the player is already doing the quest his level is no longer of relevance
			if (player.IsDoingQuest(LinkedQuestType) != null)
				return true;

			// This checks below are only performed is player isn't doing quest already
			if (player.Level < MinLevel || player.Level > MaxLevel)
				return false;

			// Does the player can do the quest again
			if(player.HasFinishedQuest(LinkedQuestType) >= MaxQuestCount)
				return false;

			return true;
		}

		#endregion
	}
}
