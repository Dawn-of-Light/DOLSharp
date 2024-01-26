using NUnit.Framework;
using DOL.GS;
using DOL.GS.Spells;
using DOL.Events;
using System;
using DOL.Database;

namespace DOL.UnitTests.Gameserver
{
    [TestFixture]
    class UT_SpellHandler
    {
        [OneTimeSetUp]
        public void SetUpFakeServer()
        {
            FakeServer.Load();
        }

        #region CastSpell
        [Test]
        public void CastSpell_OnNPCTarget_True()
        {
            var caster = NewFakePlayer();
            var target = NewFakeNPC();
            var spell = NewFakeSpell();
            var spellHandler = new SpellHandler(caster, spell, null);

            bool isCastSpellSuccessful = spellHandler.CastSpell(target);

            Assert.That(isCastSpellSuccessful, Is.True);
        }

        [Test]
        public void CastSpell_OnNPCTarget_CastStartingEventFired()
        {
            var caster = NewFakePlayer();
            var target = NewFakeNPC();
            var spell = NewFakeSpell();
            var spellHandler = new SpellHandler(caster, spell, null);

            spellHandler.CastSpell(target);

            var actual = caster.lastNotifiedEvent;
            var expected = GameLivingEvent.CastStarting;
            Assert.That(actual, Is.EqualTo(expected));
            Assert.That(spellHandler, Is.EqualTo((caster.lastNotifiedEventArgs as CastingEventArgs).SpellHandler));
        }

        [Test]
        public void CastSpell_FocusSpell_FiveEventsOnCasterAndOneEventOnTarget()
        {
            var caster = NewFakePlayer();
            var target = NewFakePlayer();
            var spell = NewFakeSpell();
            spell.fakeIsFocus = true;
            spell.fakeTarget = "Realm";
            spell.Duration = 20;
            var spellHandler = new SpellHandler(caster, spell, NewSpellLine());
            var gameEventMgrSpy = GameEventMgrSpy.LoadAndReturn();
            UtilChanceIsHundredPercent.Enable();

            spellHandler.CastSpell(target);

            var eventNumberOnCaster = gameEventMgrSpy.GameObjectEventCollection[caster].Count;
            var eventNumberOnTarget = gameEventMgrSpy.GameObjectEventCollection[target].Count;
            Assert.That(eventNumberOnCaster, Is.EqualTo(5), "Caster has not the right amount of event subscriptions");
            Assert.That(eventNumberOnTarget, Is.EqualTo(1), "Target has not the right amount of event subscriptions");
        }

        [Test]
        public void CastSpell_FocusSpellAndCasterMoves_AllEventsRemoved()
        {
            var caster = NewFakePlayer();
            var target = NewFakeNPC();
            var spell = NewFakeSpell();
            spell.fakeIsFocus = true;
            spell.fakeTarget = "Enemy";
            spell.Duration = 20;
            var spellHandler = new SpellHandler(caster, spell, NewSpellLine());
            var gameEventMgrSpy = GameEventMgrSpy.LoadAndReturn();
            UtilChanceIsHundredPercent.Enable();

            spellHandler.CastSpell(target);
            caster.OnPlayerMove();

            var eventNumberOnCaster = gameEventMgrSpy.GameObjectEventCollection[caster].Count;
            var eventNumberOnTarget = gameEventMgrSpy.GameObjectEventCollection[target].Count;
            Assert.That(eventNumberOnCaster, Is.EqualTo(0), "Caster has not the right amount of event subscriptions");
            Assert.That(eventNumberOnTarget, Is.EqualTo(0), "Target has not the right amount of event subscriptions");
        }

        [Test]
        public void CastSpell_FocusSpellCastAndTicksOnce_AllEventsRemoved()
        {
            var caster = NewFakePlayer();
            var target = NewFakePlayer();
            var spell = NewFakeSpell();
            spell.fakeIsFocus = true;
            spell.fakeTarget = "Realm";
            spell.Duration = 20;
            spell.fakeFrequency = 20;
            spell.fakeSpellType = "DamageShield";
            spell.fakePulse = 1;
            var spellHandler = new SpellHandler(caster, spell, NewSpellLine());
            var gameEventMgrSpy = GameEventMgrSpy.LoadAndReturn();

            Assert.That(spellHandler.CastSpell(target), Is.True);
            target.fakeRegion.fakeElapsedTime = 2;
            spellHandler.StartSpell(target); //tick
            caster.OnPlayerMove();

            var eventNumberOnCaster = gameEventMgrSpy.GameObjectEventCollection[caster].Count;
            var eventNumberOnTarget = gameEventMgrSpy.GameObjectEventCollection[target].Count;
            Assert.That(eventNumberOnCaster, Is.EqualTo(0), "Caster has not the right amount of event subscriptions");
            Assert.That(eventNumberOnTarget, Is.EqualTo(0), "Target has not the right amount of event subscriptions");
        }

        [Test]
        public void CheckBeginCast_NPCTarget_True()
        {
            var caster = NewFakePlayer();
            var spell = NewFakeSpell();
            var spellHandler = new SpellHandler(caster, spell, null);

            bool isBeginCastSuccessful = spellHandler.CheckBeginCast(NewFakeNPC());
            Assert.That(isBeginCastSuccessful, Is.True);
        }
        #endregion CastSpell

        #region CalculateDamageVariance
        [Test]
        public void CalculateDamageVariance_TargetIsGameLiving_MinIs125Percent()
        {
            var target = NewFakeLiving();
            var spellLine = NewSpellLine();
            var spellHandler = new SpellHandler(null, null, spellLine);

            spellHandler.CalculateDamageVariance(target, out double actual, out double ignoredValue);

            Assert.That(actual, Is.EqualTo(1.25));
        }

        [Test]
        public void CalculateDamageVariance_TargetIsGameLiving_MaxIs125Percent()
        {
            var target = NewFakeLiving();
            var spellLine = NewSpellLine();
            var spellHandler = new SpellHandler(null, null, spellLine);

            spellHandler.CalculateDamageVariance(target, out double ignoredValue, out double actual);

            Assert.That(actual, Is.EqualTo(1.25));
        }

        [Test]
        public void CalculateDamageVariance_SpellLineIsItemEffects_MinIs100Percent()
        {
            var spellLine = new SpellLine("Item Effects", "", "", false);
            var spellHandler = new SpellHandler(null, null, spellLine);

            spellHandler.CalculateDamageVariance(null, out double actual, out double ignoredValue);

            Assert.That(actual, Is.EqualTo(1.00));
        }

        [Test]
        public void CalculateDamageVariance_SpellLineIsCombatStyleEffects_MinIs100Percent()
        {
            var spellLine = new SpellLine("Combat Style Effects", "", "", false);
            var spellHandler = new SpellHandler(null, null, spellLine);

            spellHandler.CalculateDamageVariance(null, out double actual, out double ignoredValue);

            Assert.That(actual, Is.EqualTo(1.00));
        }

        [Test]
        public void CalculateDamageVariance_SpellLineIsCombatStyleEffects_MaxIs150Percent()
        {
            var spellLine = new SpellLine("Combat Style Effects", "", "", false);
            var spellHandler = new SpellHandler(null, null, spellLine);

            spellHandler.CalculateDamageVariance(null, out double ignoredValue, out double actual);

            Assert.That(actual, Is.EqualTo(1.5));
        }

        [Test]
        public void CalculateDamageVariance_SpellLineIsReservedSpells_MinAndMaxIs100Percent()
        {
            var spellLine = new SpellLine("Reserved Spells", "", "", false);
            var spellHandler = new SpellHandler(null, null, spellLine);

            spellHandler.CalculateDamageVariance(null, out double actualMin, out double actualMax);

            Assert.That(actualMin, Is.EqualTo(1.0));
            Assert.That(actualMax, Is.EqualTo(1.0));
        }

        [Test]
        public void CalculateDamageVariance_SourceAndTargetLevel30AndSpecLevel16_MinIs75Percent()
        {
            var source = new FakePlayer();
            var target = NewFakeLiving();
            source.modifiedSpecLevel = 16;
            source.Level = 30;
            target.Level = 30;
            var spellLine = NewSpellLine();
            var spellHandler = new SpellHandler(source, null, spellLine);

            spellHandler.CalculateDamageVariance(target, out double actual, out double ignoredValue);

            Assert.That(actual, Is.EqualTo(0.75));
        }

        [Test]
        public void CalculateDamageVariance_SameLevelButNoSpec_MinIs25Percent()
        {
            var source = new FakePlayer();
            var target = NewFakeLiving();
            source.modifiedSpecLevel = 1;
            source.Level = 30;
            target.Level = 30;
            var spellLine = NewSpellLine();
            var spellHandler = new SpellHandler(source, null, spellLine);

            spellHandler.CalculateDamageVariance(target, out double actual, out double ignoredValue);

            Assert.That(actual, Is.EqualTo(0.25));
        }

        [Test]
        public void CalculateDamageVariance_SameLevelButFiveSpecLevelOverTargetLevel_MinIs127Percent()
        {
            var source = new FakePlayer();
            var target = NewFakeLiving();
            source.modifiedSpecLevel = 35;
            source.Level = 30;
            target.Level = 30;
            var spellLine = NewSpellLine();
            var spellHandler = new SpellHandler(source, null, spellLine);

            spellHandler.CalculateDamageVariance(target, out double actual, out double ignoredValue);

            Assert.That(actual, Is.EqualTo(1.27));
        }

        [Test]
        public void CalculateDamageVariance_NoSpecButSourceHasTwiceTheTargetLevel_MinIs55Percent()
        {
            var source = new FakePlayer();
            var target = NewFakeLiving();
            source.modifiedSpecLevel = 1;
            source.Level = 30;
            target.Level = 15;
            var spellLine = NewSpellLine();
            var spellHandler = new SpellHandler(source, null, spellLine);

            spellHandler.CalculateDamageVariance(target, out double actual, out double ignoredValue);

            Assert.That(actual, Is.EqualTo(0.55));
        }

        [Test]
        public void CalculateDamageVariance_NoSpecButSourceHasTwiceTheTargetLevel_MaxIs155Percente()
        {
            var source = new FakePlayer();
            var target = NewFakeLiving();
            source.modifiedSpecLevel = 1;
            source.Level = 30;
            target.Level = 15;
            var spellLine = NewSpellLine();
            var spellHandler = new SpellHandler(source, null, spellLine);

            spellHandler.CalculateDamageVariance(target, out double ignoredValue, out double actual);

            Assert.That(actual, Is.EqualTo(1.55));
        }
        #endregion CalculateDamageVariance

        #region CalculateDamageBase
        [Test]
        public void CalculateDamageBase_SpellDamageIs100AndCombatStyleEffect_ReturnAround72()
        {
            var spell = NewFakeSpell();
            spell.Damage = 100;
            var source = NewFakePlayer();
            var target = NewFakePlayer();
            var spellLine = new SpellLine(GlobalSpellsLines.Combat_Styles_Effect, "", "", false);
            var spellHandler = new SpellHandler(source, spell, spellLine);

            double actual = spellHandler.CalculateDamageBase(target);

            double expected = 100 * (0 + 200) / 275.0;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void CalculateDamageBase_SpellDamageIs100SourceIsCasterWith100ManaStat_ReturnCirca109()
        {
            var spell = NewFakeSpell();
            spell.Damage = 100;
            var source = NewFakePlayer();
            source.fakeCharacterClass = IntCaster;
            source.modifiedIntelligence = 100;
            var target = NewFakePlayer();
            var spellLine = NewSpellLine();
            var spellHandler = new SpellHandler(source, spell, spellLine);

            double actual = spellHandler.CalculateDamageBase(target);

            double expected = 100 * (100 + 200) / 275.0;
            Assert.That(actual, Is.EqualTo(expected).Within(0.001));
        }

        [Test]
        public void CalculateDamageBase_SpellDamageIs100SourceIsCasterPetWith100ManaStatAndOwnerWith100ManaStat_ReturnCirca119()
        {
            var spell = NewFakeSpell();
            spell.Damage = 100;
            var owner = NewFakePlayer();
            owner.fakeCharacterClass = IntCaster;
            owner.modifiedIntelligence = 100;
            owner.Level = 50;
            var brain = new FakeControlledBrain();
            brain.fakeOwner = owner;
            GamePet source = new GamePet(brain);
            source.Level = 50; //temporal coupling through AutoSetStat()
            source.Intelligence = 100;
            var target = NewFakePlayer();
            var spellLine = NewSpellLine();
            var spellHandler = new SpellHandler(source, spell, spellLine);

            double actual = spellHandler.CalculateDamageBase(target);

            double expected = 100 * (100 + 200) / 275.0 * (100 + 200) / 275.0;
            Assert.That(actual, Is.EqualTo(expected).Within(0.001));
        }

        [Test]
        public void CalculateDamageBase_SpellDamageIs100FromGameNPCWithoutOwner_ReturnAround119()
        {
            GameLiving.LoadCalculators(); //temporal coupling and global state
            var spell = NewFakeSpell();
            spell.Damage = 100;
            var source = NewFakeNPC();
            source.Level = 50;
            source.Intelligence = 100;
            var target = NewFakePlayer();
            var spellLine = NewSpellLine();
            var spellHandler = new SpellHandler(source, spell, spellLine);

            double actual = spellHandler.CalculateDamageBase(target);

            double expected = 100 * (100 + 200) / 275.0;
            Assert.That(actual, Is.EqualTo(expected).Within(0.001));
        }
        #endregion CalculateDamageBase

        #region CalculateToHitChance
        [Test]
        public void CalculateToHitChance_BaseChance_Return85()
        {
            var spell = NewFakeSpell();
            var source = NewFakePlayer();
            var target = NewFakePlayer();
            var spellLine = NewSpellLine();
            var spellHandler = new SpellHandler(source, spell, spellLine);

            int actual = spellHandler.CalculateToHitChance(target);

            Assert.That(actual, Is.EqualTo(85));
        }

        [Test]
        public void CalculateToHitChance_SpellLevelIs50TargetLevelIsZero_Return110()
        {
            var spell = NewFakeSpell();
            spell.Level = 50;
            var source = NewFakePlayer();
            var target = NewFakePlayer();
            target.Level = 0;
            var spellLine = NewSpellLine();
            var spellHandler = new SpellHandler(source, spell, spellLine);

            int actual = spellHandler.CalculateToHitChance(target);

            Assert.That(actual, Is.EqualTo(110));
        }

        [Test]
        public void CalculateToHitChance_SpellBonusIsTen_Return90()
        {
            var spell = NewFakeSpell();
            var source = NewFakePlayer();
            source.modifiedSpellLevel = 10; //spellBonus
            var target = NewFakePlayer();
            var spellLine = NewSpellLine();
            var spellHandler = new SpellHandler(source, spell, spellLine);

            int actual = spellHandler.CalculateToHitChance(target);

            Assert.That(actual, Is.EqualTo(90));
        }

        [Test]
        public void CalculateToHitChance_SpellBonusIsSeven_Return88()
        {
            var spell = NewFakeSpell();
            var source = NewFakePlayer();
            source.modifiedSpellLevel = 7; //spellBonus
            var target = NewFakePlayer();
            var spellLine = NewSpellLine();
            var spellHandler = new SpellHandler(source, spell, spellLine);

            int actual = spellHandler.CalculateToHitChance(target);

            Assert.That(actual, Is.EqualTo(88));
        }

        [Test]
        public void CalculateToHitChance_SourceSpellBonusIsTenSpellLevelAndTargetLevelAre50_Return85()
        {
            var spell = NewFakeSpell();
            spell.Level = 50;
            var source = NewFakePlayer();
            source.modifiedSpellLevel = 10; //spellBonus
            source.modifiedToHitBonus = 0;
            var target = NewFakePlayer();
            target.Level = 50;
            var spellLine = NewSpellLine();
            var spellHandler = new SpellHandler(source, spell, spellLine);

            int actual = spellHandler.CalculateToHitChance(target);

            Assert.That(actual, Is.EqualTo(85));
        }

        [Test]
        public void CalculateToHitChance_SameTargetAndSpellLevelWithFiveToHitBonus_Return90()
        {
            var spell = NewFakeSpell();
            spell.Level = 50;
            var source = NewFakePlayer();
            source.modifiedSpellLevel = 0; //spellBonus
            source.modifiedToHitBonus = 5;
            var target = NewFakePlayer();
            target.Level = 50;
            var spellLine = NewSpellLine();
            var spellHandler = new SpellHandler(source, spell, spellLine);

            int actual = spellHandler.CalculateToHitChance(target);

            Assert.That(actual, Is.EqualTo(90));
        }

        [Test]
        public void CalculateToHitChance_TargetIsNPCLevel50SourceIsLevel50PlayerAndSpellLevelIs40_Return80()
        {
            GS.ServerProperties.Properties.PVE_SPELL_CONHITPERCENT = 10;
            var spell = NewFakeSpell();
            spell.Level = 40;
            var source = NewFakePlayer();
            source.modifiedEffectiveLevel = 50;
            var target = NewFakeNPC();
            target.Level = 50;
            target.modifiedEffectiveLevel = 50;
            var spellLine = NewSpellLine();
            var spellHandler = new SpellHandler(source, spell, spellLine);

            int actual = spellHandler.CalculateToHitChance(target);

            Assert.That(actual, Is.EqualTo(80));
        }

        #endregion

        private static GameLiving NewFakeLiving() => new FakeLiving();
        private static FakePlayerSpy NewFakePlayer() => new FakePlayerSpy() { Realm = eRealm.Albion };
        private static FakeNPC NewFakeNPC() => new FakeNPC();
        private static FakeSpell NewFakeSpell() => new FakeSpell();
        private static SpellLine NewSpellLine() => new SpellLine("", "", "", false);
        private static CharacterClass IntCaster
            => CharacterClass.Create(new DBCharacterClass() { ManaStat = (byte)eStat.INT });

        private class FakeSpell : Spell
        {
            public bool fakeIsFocus = false;
            public string fakeTarget = "";
            public int fakeFrequency = 0;
            public string fakeSpellType = "";
            public int fakePulse = 0;
            public int fakeRange = 0;

            public FakeSpell() : base(new DBSpell(), 0) { }

            public override int Pulse => fakePulse;
            public override string SpellType => fakeSpellType;
            public override bool IsFocus => fakeIsFocus;
            public override string Target => fakeTarget;
            public override int Frequency => fakeFrequency;
            public override int Range => fakeRange;
        }

        private class FakePlayerSpy : FakePlayer
        {
            public DOLEvent lastNotifiedEvent;
            public EventArgs lastNotifiedEventArgs;

            public FakePlayerSpy() : base()
            {
                fakeCharacterClass = GS.CharacterClass.None;
                fakeRegion.fakeElapsedTime = 0;
            }

            public override void Notify(DOLEvent e, object sender, EventArgs args)
            {
                lastNotifiedEvent = e;
                lastNotifiedEventArgs = args;
                base.Notify(e, sender, args);
            }
        }

        private class GameEventMgrSpy : GameEventMgr
        {
            public System.Collections.Generic.Dictionary<object, DOLEventHandlerCollection> GameObjectEventCollection => m_gameObjectEventCollections;

            public static GameEventMgrSpy LoadAndReturn()
            {
                var spy = new GameEventMgrSpy();
                LoadTestDouble(spy);
                return spy;
            }
        }
    }
}
