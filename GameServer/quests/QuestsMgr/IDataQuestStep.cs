namespace DOL.GS.Quests
{
    public enum eStepCheckType
    {
        Qualification,
        Offer,
        GiveItem,
        Step,
        Finish,
        RewardsChosen,
        PostFinish,
    }

    public interface IDataQuestStep
    {
        bool Execute(DataQuest dataQuest, GamePlayer player, int step, eStepCheckType stepCheckType);
    }
}
