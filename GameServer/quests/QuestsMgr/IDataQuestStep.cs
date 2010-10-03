using System;
using System.Collections.Generic;

namespace DOL.GS.Quests
{
	public enum eStepCheckType : int
	{
		Offer,
		Step,
		Finish,
	}

	public interface IDataQuestStep
	{
		bool Execute(DataQuest dataQuest, GamePlayer player, int step, eStepCheckType stepCheckType);
	}
}
