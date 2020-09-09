using NUnit.Framework;
using NSubstitute;
using DOL.GS;
using DOL.GS.Spells;

namespace DOL.UnitTests.Gameserver
{
    [TestFixture]
    class UT_SpellHandler
    {
        [SetUp]
        public void SetUp()
        {
            GS.ServerProperties.Properties.DEBUG_LOAD_REGIONS = "";
            GS.ServerProperties.Properties.DISABLED_REGIONS = "";
            GS.ServerProperties.Properties.DISABLED_EXPANSIONS = "";
            GS.ServerProperties.Properties.PVE_SPELL_DAMAGE = 1;
            GameServer.LoadTestDouble(new FakeServer());
        }

        #region CastSpell
        [Test]
        public void CastSpell_GenericTarget_CastStartingEventFired()
        {
            var caster = new FakePlayerNotifySpy();
            var spell = GenericSpell;
            var spellHandler = new SpellHandler(caster, spell, null);

            spellHandler.CastSpell(GenericNPC);

            var actual = caster.lastNotifiedEvent;
            var expected = GameLivingEvent.CastStarting;
            Assert.AreEqual(expected, actual);
            Assert.AreEqual((caster.lastNotifiedEventArgs as CastingEventArgs).SpellHandler, spellHandler);
        }

        [Test]
        public void CheckBeginCast_NPCTarget_True()
        {
            var caster = new FakePlayerNotifySpy();
            var spell = GenericSpell;
            var spellHandler = new SpellHandler(caster, spell, null);

            var actual = spellHandler.CheckBeginCast(GenericNPC);

            Assert.IsTrue(actual);
        }
        #endregion CastSpell

        private class FakePlayerNotifySpy : FakePlayer
        {
            public DOLEvent lastNotifiedEvent;
            public EventArgs lastNotifiedEventArgs;

            public FakePlayerNotifySpy() : base()
            {
                characterClass = new DefaultCharacterClass();
            }

            public override void Notify(DOLEvent e, object sender, EventArgs args)
            {
                lastNotifiedEvent = e;
                lastNotifiedEventArgs = args;
                base.Notify(e, sender, args);
            }
        }

        #region CalculateDamageVariance
        [Test]
        public void CalculateDamageVariance_TargetIsGameLiving_MinIs125Percent()
        {
            var target = Substitute.For<GameLiving>();
            var spellLine = new SpellLine("", "", "", false);
            var spellHandler = new SpellHandler(null, null, spellLine);

            spellHandler.CalculateDamageVariance(target, out double actual, out double ignoredValue);

            Assert.AreEqual(1.25, actual);
        }

        [Test]
        public void CalculateDamageVariance_TargetIsGameLiving_MaxIs125Percent()
        {
            var target = Substitute.For<GameLiving>();
            var spellLine = new SpellLine("", "", "", false);
            var spellHandler = new SpellHandler(null, null, spellLine);

            spellHandler.CalculateDamageVariance(target, out double ignoredValue, out double actual);

            Assert.AreEqual(1.25, actual);
        }

        [Test]
        public void CalculateDamageVariance_SpellLineIsItemEffects_MinIs100Percent()
        {
            var spellLine = new SpellLine("Item Effects", "", "", false);
            var spellHandler = new SpellHandler(null, null, spellLine);

            spellHandler.CalculateDamageVariance(null, out double actual, out double ignoredValue);

            Assert.AreEqual(1.00, actual);
        }

        [Test]
        public void CalculateDamageVariance_SpellLineIsCombatStyleEffects_MinIs100Percent()
        {
            var spellLine = new SpellLine("Combat Style Effects", "", "", false);
            var spellHandler = new SpellHandler(null, null, spellLine);

            spellHandler.CalculateDamageVariance(null, out double actual, out double ignoredValue);

            Assert.AreEqual(1.00, actual);
        }

        [Test]
        public void CalculateDamageVariance_SpellLineIsCombatStyleEffects_MaxIs150Percent()
        {
            var spellLine = new SpellLine("Combat Style Effects", "", "", false);
            var spellHandler = new SpellHandler(null, null, spellLine);

            spellHandler.CalculateDamageVariance(null, out double ignoredValue, out double actual);

            Assert.AreEqual(1.5, actual);
        }

        [Test]
        public void CalculateDamageVariance_SpellLineIsReservedSpells_MinAndMaxIs100Percent()
        {
            var spellLine = new SpellLine("Reserved Spells", "", "", false);
            var spellHandler = new SpellHandler(null, null, spellLine);

            spellHandler.CalculateDamageVariance(null, out double actualMin, out double actualMax);

            Assert.AreEqual(1.0, actualMin);
            Assert.AreEqual(1.0, actualMax);
        }

        [Test]
        public void CalculateDamageVariance_SourceAndTargetLevel30AndSpecLevel16_MinIs75Percent()
        {
            var source = new FakePlayer();
            var target = Substitute.For<GameLiving>();
            source.modifiedSpecLevel = 16;
            source.Level = 30;
            target.Level = 30;
            var spellLine = new SpellLine("", "", "", false);
            var spellHandler = new SpellHandler(source, null, spellLine);

            spellHandler.CalculateDamageVariance(target, out double actual, out double ignoredValue);

            Assert.AreEqual(0.75, actual);
        }

        [Test]
        public void CalculateDamageVariance_SameLevelButNoSpec_MinIs25Percent()
        {
            var source = new FakePlayer();
            var target = Substitute.For<GameLiving>();
            source.modifiedSpecLevel = 1;
            source.Level = 30;
            target.Level = 30;
            var spellLine = new SpellLine("", "", "", false);
            var spellHandler = new SpellHandler(source, null, spellLine);

            spellHandler.CalculateDamageVariance(target, out double actual, out double ignoredValue);

            Assert.AreEqual(0.25, actual);
        }

        [Test]
        public void CalculateDamageVariance_SameLevelButFiveSpecLevelOverTargetLevel_MinIs127Percent()
        {
            var source = new FakePlayer();
            var target = Substitute.For<GameLiving>();
            source.modifiedSpecLevel = 35;
            source.Level = 30;
            target.Level = 30;
            var spellLine = new SpellLine("", "", "", false);
            var spellHandler = new SpellHandler(source, null, spellLine);

            spellHandler.CalculateDamageVariance(target, out double actual, out double ignoredValue);

            Assert.AreEqual(1.27, actual);
        }

        [Test]
        public void CalculateDamageVariance_NoSpecButSourceHasTwiceTheTargetLevel_MinIs55Percent()
        {
            var source = new FakePlayer();
            var target = Substitute.For<GameLiving>();
            source.modifiedSpecLevel = 1;
            source.Level = 30;
            target.Level = 15;
            var spellLine = new SpellLine("", "", "", false);
            var spellHandler = new SpellHandler(source, null, spellLine);

            spellHandler.CalculateDamageVariance(target, out double actual, out double ignoredValue);

            Assert.AreEqual(0.55, actual);
        }

        [Test]
        public void CalculateDamageVariance_NoSpecButSourceHasTwiceTheTargetLevel_MaxIs155Percente()
        {
            var source = new FakePlayer();
            var target = Substitute.For<GameLiving>();
            source.modifiedSpecLevel = 1;
            source.Level = 30;
            target.Level = 15;
            var spellLine = new SpellLine("", "", "", false);
            var spellHandler = new SpellHandler(source, null, spellLine);

            spellHandler.CalculateDamageVariance(target, out double ignoredValue, out double actual);

            Assert.AreEqual(1.55, actual);
        }
        #endregion CalculateDamageVariance

        #region CalculateDamageBase
        [Test]
        public void CalculateDamageBase_SpellDamageIs100AndCombatStyleEffect_ReturnAround72()
        {
            var spell = GenericSpell;
            spell.Damage = 100;
            var source = GenericFakePlayer;
            var target = GenericFakePlayer;
            var spellLine = new SpellLine(GlobalSpellsLines.Combat_Styles_Effect, "", "", false);
            var spellHandler = new SpellHandler(source, spell, spellLine);

            double actual = spellHandler.CalculateDamageBase(target);

            double expected = 100 * (0 + 200) / 275.0;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CalculateDamageBase_SpellDamageIs100SourceIsAnimistWith100Int_ReturnAround109()
        {
            var spell = GenericSpell;
            spell.Damage = 100;
            var source = FakePlayerAnimist;
            source.modifiedIntelligence = 100;
            var target = GenericFakePlayer;
            var spellLine = new SpellLine("", "", "", false);
            var spellHandler = new SpellHandler(source, spell, spellLine);

            double actual = spellHandler.CalculateDamageBase(target);

            double expected = 100 * (100 + 200) / 275.0;
            Assert.AreEqual(expected, actual, 0.001);
        }

        [Test]
        public void CalculateDamageBase_SpellDamageIs100SourceIsAnimistPetWith100IntAndOwnerWith100Int_ReturnAround119()
        {
            var spell = GenericSpell;
            spell.Damage = 100;
            var owner = FakePlayerAnimist;
            owner.modifiedIntelligence = 100;
            owner.Level = 50;
            GamePet source = Create.GenericRealPet(owner);
            source.Level = 50; //temporal coupling through AutoSetStat()
            source.Intelligence = 100;
            var target = GenericFakePlayer;
            var spellLine = new SpellLine("", "", "", false);
            var spellHandler = new SpellHandler(source, spell, spellLine);

            double actual = spellHandler.CalculateDamageBase(target);

            double expected = 100 * (100 + 200) / 275.0 * (100 + 200) / 275.0;
            Assert.AreEqual(expected, actual, 0.001);
        }

        [Test]
        public void CalculateDamageBase_SpellDamageIs100FromGameNPCWithoutOwner_ReturnAround119()
        {
            GameLiving.LoadCalculators(); //temporal coupling and global state
            var spell = GenericSpell;
            spell.Damage = 100;
            var source = GenericNPC;
            source.Level = 50;
            source.Intelligence = 100;
            var target = GenericFakePlayer;
            var spellLine = new SpellLine("", "", "", false);
            var spellHandler = new SpellHandler(source, spell, spellLine);

            double actual = spellHandler.CalculateDamageBase(target);

            double expected = 100 * (100 + 200) / 275.0;
            Assert.AreEqual(expected, actual, 0.001);
        }
        #endregion CalculateDamageBase

        #region CalculateToHitChance
        [Test]
        public void CalculateToHitChance_BaseChance_Return85()
        {
            var spell = GenericSpell;
            var source = GenericFakePlayer;
            var target = GenericFakePlayer;
            var spellLine = new SpellLine("", "", "", false);
            var spellHandler = new SpellHandler(source, spell, spellLine);

            int actual = spellHandler.CalculateToHitChance(target);

            Assert.AreEqual(85, actual);
        }

        [Test]
        public void CalculateToHitChance_SpellLevelIs50TargetLevelIsZero_Return110()
        {
            var spell = GenericSpell;
            spell.Level = 50;
            var source = GenericFakePlayer;
            var target = GenericFakePlayer;
            target.Level = 0;
            var spellLine = new SpellLine("", "", "", false);
            var spellHandler = new SpellHandler(source, spell, spellLine);

            int actual = spellHandler.CalculateToHitChance(target);

            Assert.AreEqual(110, actual);
        }

        [Test]
        public void CalculateToHitChance_SpellBonusIsTen_Return90()
        {
            var spell = GenericSpell;
            var source = GenericFakePlayer;
            source.modifiedSpellLevel = 10; //spellBonus
            var target = GenericFakePlayer;
            var spellLine = new SpellLine("", "", "", false);
            var spellHandler = new SpellHandler(source, spell, spellLine);

            int actual = spellHandler.CalculateToHitChance(target);

            Assert.AreEqual(90, actual);
        }

        [Test]
        public void CalculateToHitChance_SpellBonusIsSeven_Return88()
        {
            var spell = GenericSpell;
            var source = GenericFakePlayer;
            source.modifiedSpellLevel = 7; //spellBonus
            var target = GenericFakePlayer;
            var spellLine = new SpellLine("", "", "", false);
            var spellHandler = new SpellHandler(source, spell, spellLine);

            int actual = spellHandler.CalculateToHitChance(target);

            Assert.AreEqual(88, actual);
        }

        [Test]
        public void CalculateToHitChance_SourceSpellBonusIsTenSpellLevelAndTargetLevelAre50_Return85()
        {
            var spell = GenericSpell;
            spell.Level = 50;
            var source = GenericFakePlayer;
            source.modifiedSpellLevel = 10; //spellBonus
            source.modifiedToHitBonus = 0;
            var target = GenericFakePlayer;
            target.Level = 50;
            var spellLine = new SpellLine("", "", "", false);
            var spellHandler = new SpellHandler(source, spell, spellLine);

            int actual = spellHandler.CalculateToHitChance(target);

            Assert.AreEqual(85, actual);
        }

        [Test]
        public void CalculateToHitChance_SameTargetAndSpellLevelWithFiveToHitBonus_Return90()
        {
            var spell = GenericSpell;
            spell.Level = 50;
            var source = GenericFakePlayer;
            source.modifiedSpellLevel = 0; //spellBonus
            source.modifiedToHitBonus = 5;
            var target = GenericFakePlayer;
            target.Level = 50;
            var spellLine = new SpellLine("", "", "", false);
            var spellHandler = new SpellHandler(source, spell, spellLine);

            int actual = spellHandler.CalculateToHitChance(target);

            Assert.AreEqual(90, actual);
        }

        [Test]
        public void CalculateToHitChance_TargetIsNPCLevel50SourceIsLevel50PlayerAndSpellLevelIs40_Return80()
        {
            GS.ServerProperties.Properties.PVE_SPELL_CONHITPERCENT = 10;
            var spell = GenericSpell;
            spell.Level = 40;
            var source = GenericFakePlayer;
            source.modifiedEffectiveLevel = 50;
            var target = GenericNPC;
            target.Level = 50;
            target.modifiedEffectiveLevel = 50;
            var spellLine = new SpellLine("", "", "", false);
            var spellHandler = new SpellHandler(source, spell, spellLine);

            int actual = spellHandler.CalculateToHitChance(target);

            Assert.AreEqual(80, actual);
        }
        #endregion

        private static FakePlayer GenericFakePlayer => Create.GenericFakePlayer;
        private static FakeNPC GenericNPC => FakeNPC.CreateGeneric();
        private static FakePlayer FakePlayerAnimist => Create.FakePlayerAnimist;
        private static Spell GenericSpell => Create.Spell;
    }
}
