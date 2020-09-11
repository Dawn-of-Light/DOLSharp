using DOL.Database;
using DOL.GS;
using DOL.GS.Effects;
using NUnit.Framework;
using System.Collections.Generic;

namespace DOL.UnitTests.Gameserver
{
    [TestFixture]
    class UT_GameEffectList
    {
        [Test]
        public void Add_OwnerIsNotAlive_ReturnFalse()
        {
            var owner = NewFakeLiving();
            owner.mockIsAlive = false;
            var effectList = NewGameEffectList(owner);
            var effect = NewFakeEffect();

            bool actual = effectList.Add(effect);

            Assert.AreEqual(false, actual);
        }

        [Test]
        public void Add_OwnerIsInactiveObject_ReturnFalse()
        {
            var owner = NewFakeLiving();
            owner.mockIsAlive = true;
            owner.mockObjectState = GameObject.eObjectState.Inactive;
            var effectList = NewGameEffectList(owner);
            var effect = NewFakeEffect();

            bool actual = effectList.Add(effect);

            Assert.AreEqual(false, actual);
        }

        [Test]
        public void Add_OwnerIsActiveObjectAndAlive_ReturnTrue()
        {
            var owner = NewFakeLiving();
            owner.mockIsAlive = true;
            owner.mockObjectState = GameObject.eObjectState.Active;
            var effectList = NewGameEffectList(owner);
            var effect = NewFakeEffect();

            bool actual = effectList.Add(effect);

            Assert.AreEqual(true, actual);
        }

        [Test]
        public void Add_ToFreshListAndOwnerIsAliveAndActiveObject_ListCountIsOne()
        {
            var owner = NewFakeLiving();
            owner.mockIsAlive = true;
            owner.mockObjectState = GameObject.eObjectState.Active;
            var effectList = NewGameEffectList(owner);
            var effect = NewFakeEffect();

            effectList.Add(effect);

            int actual = effectList.Count;
            Assert.AreEqual(1, actual);
        }

        [Test]
        public void Add_ToFreshListAndOwnerIsNotAlive_ListCountRemainsZero()
        {
            var owner = NewFakeLiving();
            owner.mockIsAlive = false;
            var effectList = NewGameEffectList(owner);
            var effect = NewFakeEffect();

            effectList.Add(effect);

            int actual = effectList.Count;
            Assert.AreEqual(0, actual);
        }

        [Test]
        public void Remove_EffectFromFreshList_ReturnFalse()
        {
            var owner = NewFakeLiving();
            var effectList = NewGameEffectList(owner);
            var effect = NewFakeEffect();

            bool actual = effectList.Remove(effect);
            
            Assert.AreEqual(false, actual);
        }

        [Test]
        public void Remove_EffectFromListContainingSameEffect_ReturnTrue()
        {
            var owner = NewFakeLiving();
            var effectList = NewGameEffectList(owner);
            var effect = NewFakeEffect();
            effectList.Add(effect);

            bool actual = effectList.Remove(effect);

            Assert.AreEqual(true, actual);
        }

        [Test]
        public void Remove_EffectFromListContainingDifferentEffect_ReturnFalse()
        {
            GameEffectList effectList = createEffectListWithValidOwner();
            var effect = NewFakeEffect();
            var differentEffect = NewFakeEffect();
            effectList.Add(differentEffect);

            bool actual = effectList.Remove(effect);
            
            Assert.AreEqual(false, actual);
        }

        [Test]
        public void Remove_EffectFromListContainingSameEffect_ListCountIsZero()
        {
            GameEffectList effectList = createEffectListWithValidOwner();
            var effect = NewFakeEffect();
            effectList.Add(effect);

            effectList.Remove(effect);

            int actual = effectList.Count;
            Assert.AreEqual(0, actual);
        }

        [Test]
        public void CancelAll_EffectContainsOneEffect_EffectIsCancelled()
        {
            GameEffectList effectList = createEffectListWithValidOwner();
            var effect = NewFakeEffect();
            effectList.Add(effect);

            effectList.CancelAll();

            Assert.IsTrue(effect.receivedCancel);
        }

        [Test]
        public void OnEffectsChanged_NoOpenChanges_NPCupdatePetWindowIsCalled()
        {
            var brain = NewNullControlledBrain();
            var owner = new GameNPC(brain);
            var effectList = NewGameEffectList(owner);
            
            effectList.OnEffectsChanged(null);

            Assert.IsTrue(brain.receivedUpdatePetWindow);
        }

        [Test]
        public void OnEffectsChanged_OpenChanges_NPCupdatePetWindowIsNotCalled()
        {
            var brain = NewNullControlledBrain();
            var owner = new GameNPC(brain);
            var effectList = NewGameEffectList(owner);

            effectList.BeginChanges();
            effectList.OnEffectsChanged(null);

            Assert.IsFalse(brain.receivedUpdatePetWindow);
        }

        [Test]
        public void CommitChanges_NoOpenChanges_NPCupdatePetWindowIsCalled()
        {
            var brain = NewNullControlledBrain();
            var owner = new GameNPC(brain);
            var effectList = NewGameEffectList(owner);
            
            effectList.OnEffectsChanged(null);

            Assert.IsTrue(brain.receivedUpdatePetWindow);
        }

        [Test]
        public void CommitChanges_OpenChanges_NPCupdatePetWindowIsNotCalled()
        {
            var brain = NewNullControlledBrain();
            var owner = new GameNPC(brain);
            var effectList = NewGameEffectList(owner);

            effectList.BeginChanges();
            effectList.OnEffectsChanged(null);

            Assert.IsFalse(brain.receivedUpdatePetWindow);
        }

        [Test]
        public void GetOfType_FreshList_ReturnNull()
        {
            var owner = NewFakeLiving();
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
            var owner = Create.FakeNPC;
            var effectList = new GameEffectList(owner);
            return effectList;
        }

        private static GameEffectList NewGameEffectList(GameLiving owner) => new GameEffectList(owner);
        private static FakeGameLiving NewFakeLiving() => Create.FakeLiving;
        private static FakeGameEffect NewFakeEffect() => new FakeGameEffect();
        private static NullControlledBrain NewNullControlledBrain() => new NullControlledBrain();

        private class FakeGameEffect : IGameEffect
        {
            public bool receivedCancel = false;

            public string Name => throw new System.NotImplementedException();
            public int RemainingTime => throw new System.NotImplementedException();
            public ushort Icon => throw new System.NotImplementedException();
            public ushort InternalID { get => throw new System.NotImplementedException(); set { } }
            public IList<string> DelveInfo => throw new System.NotImplementedException();
            public void Cancel(bool playerCanceled) { receivedCancel = true; }
            public PlayerXEffect getSavedEffect() { throw new System.NotImplementedException(); }
        }
    }
}
