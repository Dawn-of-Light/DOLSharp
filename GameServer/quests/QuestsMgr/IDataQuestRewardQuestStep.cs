using System;

namespace DOL.GS.Quests
{
	public enum eGoalTypeCheck : int
	{
		//Qualification,
		Interact,
		InteractWhisper,
		InteractFinish,
		Kill,
		Step,
        RewardsChosen
        //PostFinish,
    }

	public interface IDQRewardQStep
	{
		bool Execute(DQRewardQ dqRewardQ, GamePlayer player, int step, eGoalTypeCheck goalCheckType);
	}
}
