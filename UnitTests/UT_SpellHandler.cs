using NUnit.Framework;
using NSubstitute;
using DOL.GS;
using DOL.GS.Spells;

namespace DOL.UnitTests.Gameserver
{
    [TestFixture]
    class UT_SpellHandler
    {
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
            var spell = Create.DamageSpell(100);
            var source = Create.FakePlayer();
            var target = Create.FakePlayer();
            var spellLine = new SpellLine(GlobalSpellsLines.Combat_Styles_Effect, "", "", false);
            var spellHandler = new SpellHandler(source, spell, spellLine);

            double actual = spellHandler.CalculateDamageBase(target);

            double expected = 100 * (0 + 200) / 275.0;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CalculateDamageBase_SpellDamageIs100SourceIsAnimistWith100Int_ReturnAround109()
        {
            var spell = Create.DamageSpell(100);
            var source = Create.FakePlayer(new CharacterClassAnimist());
            source.modifiedIntelligence = 100;
            var target = Create.FakePlayer();
            var spellLine = new SpellLine("", "", "", false);
            var spellHandler = new SpellHandler(source, spell, spellLine);

            double actual = spellHandler.CalculateDamageBase(target);

            double expected = 100 * (100 + 200) / 275.0;
            Assert.AreEqual(expected, actual, 0.001);
        }

        [Test]
        public void CalculateDamageBase_SpellDamageIs100SourceIsAnimistPetWith100IntAndOwnerWith100Int_ReturnAround119()
        {
            var spell = Create.DamageSpell(100);
            var owner = Create.FakePlayer(new CharacterClassAnimist());
            owner.modifiedIntelligence = 100;
            owner.Level = 50;
            GamePet source = Create.Pet(owner);
            source.Level = 50; //temporal coupling through AutoSetStat()
            source.Intelligence = 100;
            var target = Create.FakePlayer();
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
            var spell = Create.DamageSpell(100);
            var source = Create.FakeNPC();
            source.Level = 50;
            source.Intelligence = 100;
            var target = Create.FakePlayer();
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
            var spell = Create.Spell();
            var source = Create.FakePlayer();
            var target = Create.FakePlayer();
            var spellLine = new SpellLine("", "", "", false);
            var spellHandler = new SpellHandler(source, spell, spellLine);

            int actual = spellHandler.CalculateToHitChance(target);

            Assert.AreEqual(85, actual);
        }

        [Test]
        public void CalculateToHitChance_SpellLevelIs50TargetLevelIsZero_Return110()
        {
            var spell = Create.Spell();
            spell.Level = 50;
            var source = Create.FakePlayer();
            var target = Create.FakePlayer();
            target.Level = 0;
            var spellLine = new SpellLine("", "", "", false);
            var spellHandler = new SpellHandler(source, spell, spellLine);

            int actual = spellHandler.CalculateToHitChance(target);

            Assert.AreEqual(110, actual);
        }

        [Test]
        public void CalculateToHitChance_SpellBonusIsTen_Return90()
        {
            var spell = Create.Spell();
            var source = Create.FakePlayer();
            source.modifiedSpellLevel = 10; //spellBonus
            var target = Create.FakePlayer();
            var spellLine = new SpellLine("", "", "", false);
            var spellHandler = new SpellHandler(source, spell, spellLine);

            int actual = spellHandler.CalculateToHitChance(target);

            Assert.AreEqual(90, actual);
        }

        [Test]
        public void CalculateToHitChance_SpellBonusIsSeven_Return88()
        {
            var spell = Create.Spell();
            var source = Create.FakePlayer();
            source.modifiedSpellLevel = 7; //spellBonus
            var target = Create.FakePlayer();
            var spellLine = new SpellLine("", "", "", false);
            var spellHandler = new SpellHandler(source, spell, spellLine);

            int actual = spellHandler.CalculateToHitChance(target);

            Assert.AreEqual(88, actual);
        }

        [Test]
        public void CalculateToHitChance_SourceSpellBonusIsTenSpellLevelAndTargetLevelAre50_Return85()
        {
            var spell = Create.Spell();
            spell.Level = 50;
            var source = Create.FakePlayer();
            source.modifiedSpellLevel = 10; //spellBonus
            source.modiefiedToHitBonus = 0;
            var target = Create.FakePlayer();
            target.Level = 50;
            var spellLine = new SpellLine("", "", "", false);
            var spellHandler = new SpellHandler(source, spell, spellLine);

            int actual = spellHandler.CalculateToHitChance(target);

            Assert.AreEqual(85, actual);
        }

        [Test]
        public void CalculateToHitChance_SameTargetAndSpellLevelWithFiveToHitBonus_Return90()
        {
            var spell = Create.Spell();
            spell.Level = 50;
            var source = Create.FakePlayer();
            source.modifiedSpellLevel = 0; //spellBonus
            source.modiefiedToHitBonus = 5;
            var target = Create.FakePlayer();
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
            var spell = Create.Spell();
            spell.Level = 40;
            var source = Create.FakePlayer();
            source.modifiedEffectiveLevel = 50;
            var target = Create.FakeNPC();
            target.Level = 50;
            target.modifiedEffectiveLevel = 50;
            var spellLine = new SpellLine("", "", "", false);
            var spellHandler = new SpellHandler(source, spell, spellLine);

            int actual = spellHandler.CalculateToHitChance(target);

            Assert.AreEqual(80, actual);
        }
        #endregion
    }
}
