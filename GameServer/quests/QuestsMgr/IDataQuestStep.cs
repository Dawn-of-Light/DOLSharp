using System;
using System.Collections.Generic;

namespace DOL.GS.Quests
{
	public enum eStepCheckType : int
	{
		Qualification,
		Offer,
		Step,
		Finish,
		RewardsChosen,
	}

	public interface IDataQuestStep
	{
		bool Execute(DataQuest dataQuest, GamePlayer player, int step, eStepCheckType stepCheckType);
	}
}
