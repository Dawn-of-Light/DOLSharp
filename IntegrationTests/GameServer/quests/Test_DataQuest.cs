using DOL.Database;
using DOL.GS.Quests;
using DOL.UnitTests.Gameserver;
using NUnit.Framework;
using System.Collections.Generic;

namespace DOL.Integration.Gameserver
{
    [TestFixture]
    class Test_DataQuest
    {
        DBDataQuest dbDataQuest = new DBDataQuest();

        [SetUp]
        public void init()
        {
            dbDataQuest.ID = 23886;
            dbDataQuest.Name = "QuestName";
            dbDataQuest.StartType = 0;
            dbDataQuest.StartName = "StartNPC";
            dbDataQuest.StartRegionID = 27;
            dbDataQuest.AcceptText = "defense";
            dbDataQuest.Description = "description";
            dbDataQuest.SourceName = null;
            dbDataQuest.SourceText = "storyA||storyC|storyD|storyEnd";
            dbDataQuest.StepType = "0|4|6|6|5";
            dbDataQuest.StepText = "step1|step2|step3|step4|step5";
            dbDataQuest.StepItemTemplates = "item1|item2|item3|item4|item5";
            dbDataQuest.AdvanceText = "||important mission|warn|";
            dbDataQuest.TargetName = "tic;10|trick;9|truck;8|track;7|trock;6";
            dbDataQuest.TargetText = "foo|bar|baz|bork|fuu";
            dbDataQuest.CollectItemTemplate = null;
            dbDataQuest.MaxCount = 1;
            dbDataQuest.MinLevel = 1;
            dbDataQuest.MaxLevel = 11;
            dbDataQuest.RewardMoney = "1|2|3|4|5";
            dbDataQuest.RewardXP = "6|7|8|9|10";
            dbDataQuest.OptionalRewardItemTemplates = null;
            dbDataQuest.FinalRewardItemTemplates = null;
            dbDataQuest.FinishText = "Welcome, young <Class>. You look a bit out of breath! What business brings you to our village?\r\n\r\nYou tell the Veteran Guard that you were sent to warn him about the attack on the tower.\r\n\r\nYes - I can see theres something going on over there. I assumed it was a training exercise. I commend your bravery. You show great promise, here is some coin. Theres a merchant named Miraveth standing next to the well, and she has a cloak for sale that will suit you nicely. Well, my friend, I think it is safe to say you are no longer in need of training. It has been an honor, and I hope you will consider staying around to assist the town further in its time of need. Speak to Artigan, in the upper part of Fintain, if you have more questions. If you ever wish to leave this place, speak with the Channeler here in town. They can send you to the capital at anytime.";
            dbDataQuest.QuestDependency = "Basics of Combat (Hibernia)";
            dbDataQuest.ClassType = null;
            dbDataQuest.AllowedClasses = "1|2|3";
            dbDataQuest.RewardCLXP = null;
            dbDataQuest.RewardRP = null;
            dbDataQuest.RewardBP = null;
        }

        [Test]
        public void ParseQuest_GivenDBDataQuest_SetData()
        {
            var dataQuest = new DataQuestSpy(dbDataQuest);

            Assert.AreEqual(23886, dataQuest.ID);
            Assert.AreEqual("QuestName", dataQuest.Name);
            Assert.AreEqual(DataQuest.eStartType.Standard, dataQuest.StartType);
            Assert.AreEqual("description", dataQuest.Description);
            Assert.AreEqual(null, dataQuest.SpySourceName);
            Assert.AreEqual(1, dataQuest.MaxQuestCount);
            Assert.AreEqual(1, dataQuest.Level);
            Assert.AreEqual(11, dataQuest.MaxLevel);
            Assert.AreEqual("", dataQuest.OptionalRewards);
            Assert.AreEqual("", dataQuest.FinalRewards);
            Assert.AreEqual(new List<string>() { "Basics of Combat (Hibernia)" }, dataQuest.SpyQuestDependency);
            Assert.AreEqual(new List<byte> { 1, 2, 3 }, dataQuest.SpyAllowedClasses);
        }

        [Test]
        public void ParseQuest_GivenDBDataQuest_CompareStepData()
        {
            var dataQuest = new DataQuestSpy(dbDataQuest);
            FakeServer.LoadAndReturn();

            var stepCount = 5;
            for (int i = 1; i <= stepCount; i++)
            {
                dataQuest.Step = i;
                int index = i - 1;
                Assert.AreEqual(new List<byte>() { 0, 4, 6, 6, 5 }[index], (byte)dataQuest.StepType, "Failed at Step " + dataQuest.Step);
                Assert.AreEqual(new List<long>() { 1, 2, 3, 4, 5 }[index], dataQuest.SpyRewardMoney);
                Assert.AreEqual(new long[] { 6, 7, 8, 9, 10 }[index], dataQuest.SpyRewardXP);
                Assert.AreEqual(0, dataQuest.SpyRewardBP);
                Assert.AreEqual(new string[] { "tic", "trick", "truck", "track", "trock" }[index], dataQuest.TargetName);
                Assert.AreEqual(new long[] { 10, 9, 8, 7, 6 }[index], dataQuest.TargetRegion);
                Assert.AreEqual(new string[] { "foo", "bar", "baz", "bork", "fuu" }[index], dataQuest.SpyTargetText);
                Assert.AreEqual("storyA", dataQuest.Story);
                Assert.AreEqual(new string[] { "storyA", "", "storyC", "storyD", "storyEnd" }[index], dataQuest.SpySourceText);
                Assert.AreEqual(new string[] { "step1", "step2", "step3", "step4", "step5"}[index], dataQuest.StepTexts[index]);
                Assert.AreEqual(new string[] { "item1", "item2", "item3", "item4", "item5" }[index], dataQuest.SpyStepItemTemplate);
            }
            GS.GameServer.LoadTestDouble(null);
        }

        private class DataQuestSpy : DataQuest
        {
            public DataQuestSpy(DBDataQuest dbDataQuest) : base(dbDataQuest) { m_charQuest = new CharacterXDataQuest(); }

            public string SpySourceName => SourceName;
            public List<string> SpyQuestDependency => m_questDependencies;
            public List<byte> SpyAllowedClasses => m_allowedClasses;

            public long SpyRewardMoney => RewardMoney;
            public long SpyRewardXP => RewardXP;
            public long SpyRewardBP => RewardBP;
            public long SpyRewardRP => RewardRP;
            public long SpyRewardCLXP => RewardCLXP;
            public string SpySourceText => SourceText;
            public string SpyTargetText => TargetText;
            public string SpyStepItemTemplate => StepItemTemplate;
        }
    }
}
