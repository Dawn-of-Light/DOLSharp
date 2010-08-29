using System;
using System.Collections.Generic;

namespace DOL.GS.Quests
{
	public interface IDataQuestStep
	{
		bool Execute(GamePlayer player, int step, bool isFinish);
	}
}
