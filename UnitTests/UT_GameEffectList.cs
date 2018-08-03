using DOL.AI;
using DOL.AI.Brain;
using DOL.GS;
using DOL.GS.Effects;
using NSubstitute;
using NUnit.Framework;

namespace DOL.UnitTests.Gameserver
{
    [TestFixture]
    class UT_GameEffectList
    {
        [Test]
        public void Add_OwnerIsNotAlive_ReturnFalse()
        {
            var owner = Substitute.For<GameLiving>();
            owner.IsAlive.Returns(false);
            var effectList = new GameEffectList(owner);
            var effect = Substitute.For<IGameEffect>();

            bool actual = effectList.Add(effect);

            Assert.AreEqual(false, actual);
        }

        [Test]
        public void Add_OwnerIsInactiveObject_ReturnFalse()
        {
            var owner = Substitute.For<GameLiving>();
            owner.IsAlive.Returns(true);
            owner.ObjectState = GameObject.eObjectState.Inactive;
            var effectList = new GameEffectList(owner);
            var effect = Substitute.For<IGameEffect>();

            bool actual = effectList.Add(effect);

            Assert.AreEqual(false, actual);
        }

        [Test]
        public void Add_OwnerIsActiveObjectAndAlive_ReturnTrue()
        {
            var owner = Substitute.For<GameLiving>();
            owner.IsAlive.Returns(true);
            owner.ObjectState = GameObject.eObjectState.Active;
            var effectList = new GameEffectList(owner);
            var effect = Substitute.For<IGameEffect>();

            bool actual = effectList.Add(effect);

            Assert.AreEqual(true, actual);
        }

        [Test]
        public void Add_ToFreshListAndOwnerIsAliveAndActiveObject_ListCountIsOne()
        {
            var owner = Substitute.For<GameLiving>();
            owner.IsAlive.Returns(true);
            owner.ObjectState = GameObject.eObjectState.Active;
            var effectList = new GameEffectList(owner);
            var effect = Substitute.For<IGameEffect>();

            effectList.Add(effect);

            int actual = effectList.Count;
            Assert.AreEqual(1, actual);
        }

        [Test]
        public void Add_ToFreshListAndOwnerIsNotAlive_ListCountRemainsZero()
        {
            var owner = Substitute.For<GameLiving>();
            owner.IsAlive.Returns(false);
            var effectList = new GameEffectList(owner);
            var effect = Substitute.For<IGameEffect>();

            effectList.Add(effect);

            int actual = effectList.Count;
            Assert.AreEqual(0, actual);
        }

        [Test]
        public void Remove_EffectFromFreshList_ReturnFalse()
        {
            var owner = Substitute.For<GameLiving>();
            var effectList = new GameEffectList(owner);
            var effect = Substitute.For<IGameEffect>();

            bool actual = effectList.Remove(effect);
            
            Assert.AreEqual(false, actual);
        }

        [Test]
        public void Remove_EffectFromListContainingSameEffect_ReturnTrue()
        {
            var owner = Substitute.For<GameLiving>();
            owner.IsAlive.Returns(true);
            owner.ObjectState = GameObject.eObjectState.Active;
            var effectList = new GameEffectList(owner);
            var effect = Substitute.For<IGameEffect>();
            effectList.Add(effect);

            bool actual = effectList.Remove(effect);

            Assert.AreEqual(true, actual);
        }

        [Test]
        public void Remove_EffectFromListContainingDifferentEffect_ReturnFalse()
        {
            GameEffectList effectList = createEffectListWithValidOwner();
            var effect = Substitute.For<IGameEffect>();
            var differentEffect = Substitute.For<IGameEffect>();
            effectList.Add(differentEffect);

            bool actual = effectList.Remove(effect);
            
            Assert.AreEqual(false, actual);
        }

        [Test]
        public void Remove_EffectFromListContainingSameEffect_ListCountIsZero()
        {
            GameEffectList effectList = createEffectListWithValidOwner();
            var effect = Substitute.For<IGameEffect>();
            effectList.Add(effect);

            effectList.Remove(effect);

            int actual = effectList.Count;
            Assert.AreEqual(0, actual);
        }

        [Test]
        public void CancelAll_EffectContainsOneEffect_EffectIsCancelled()
        {
            GameEffectList effectList = createEffectListWithValidOwner();
            var effect = Substitute.For<IGameEffect>();
            effectList.Add(effect);

            effectList.CancelAll();

            effect.Received().Cancel(false);
        }

        [Test]
        public void OnEffectsChanged_NoOpenChanges_NPCupdatePetWindowIsCalled()
        {
            var brain = Substitute.For<ABrain, IControlledBrain>();
            var owner = new GameNPC(brain);
            var effectList = new GameEffectList(owner);
            
            effectList.OnEffectsChanged(null);

            (owner.Brain as IControlledBrain).Received().UpdatePetWindow();
        }

        [Test]
        public void OnEffectsChanged_OpenChanges_NPCupdatePetWindowIsNotCalled()
        {
            var brain = Substitute.For<ABrain, IControlledBrain>();
            var owner = new GameNPC(brain);
            var effectList = new GameEffectList(owner);

            effectList.BeginChanges();
            effectList.OnEffectsChanged(null);

            (owner.Brain as IControlledBrain).DidNotReceive().UpdatePetWindow();
        }

        [Test]
        public void CommitChanges_NoOpenChanges_NPCupdatePetWindowIsCalled()
        {
            var brain = Substitute.For<ABrain, IControlledBrain>();
            var owner = new GameNPC(brain);
            var effectList = new GameEffectList(owner);
            
            effectList.OnEffectsChanged(null);

            (owner.Brain as IControlledBrain).Received().UpdatePetWindow();
        }

        [Test]
        public void CommitChanges_OpenChanges_NPCupdatePetWindowIsNotCalled()
        {
            var brain = Substitute.For<ABrain, IControlledBrain>();
            var owner = new GameNPC(brain);
            var effectList = new GameEffectList(owner);

            effectList.BeginChanges();
            effectList.OnEffectsChanged(null);

            (owner.Brain as IControlledBrain).DidNotReceive().UpdatePetWindow();
        }

        [Test]
        public void GetOfType_FreshList_ReturnNull()
        {
            var owner = Substitute.For<GameLiving>();
            var effectList = new GameEffectList(owner);

            IGameEffect actual = effectList.GetOfType<GameSpellEffect>();

            Assert.AreEqual(null, actual);
        }

        [Test]
        public void GetOfType_ListWithOneItemOfGivenType_ReturnListWithThatOneItem()
        {
            GameEffectList effectList = createEffectListWithValidOwner();
            var effect = new GameSpellEffect(null, 0, 0);
            effectList.Add(effect);

            IGameEffect actual = effectList.GetOfType<GameSpellEffect>();
            
            Assert.IsNotNull(actual);
        }

        //RestoreAllEffects calls Database

        //SaveAllEffects calls Database

        private static GameEffectList createEffectListWithValidOwner()
        {
            var owner = Substitute.For<GameLiving>();
            owner.IsAlive.Returns(true);
            owner.ObjectState = GameObject.eObjectState.Active;
            var effectList = new GameEffectList(owner);
            return effectList;
        }
    }
}
