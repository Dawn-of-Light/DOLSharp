using NUnit.Framework;
using DOL.GS.Spells;
using DOL.GS;
using DOL.Database;
using System;

namespace DOL.UnitTests.Gameserver
{
    [TestFixture]
    class UT_HereticFocusSpells
    {
        [SetUp]
        public void SetUp()
        {
            GS.ServerProperties.Properties.DEBUG_LOAD_REGIONS = "";
            GS.ServerProperties.Properties.DISABLED_REGIONS = "";
            GS.ServerProperties.Properties.DISABLED_EXPANSIONS = "";
            GS.ServerProperties.Properties.PVE_SPELL_DAMAGE = 1;
        }

        [Test]
        public void OnDirectEffect_100InitialDamage_OneSuccessfulTick_Does100Damage()
        {
            double initialDamage = 100;
            var spell = createHereticFocusDamageSpell(initialDamage, 0);
            var source = createIdealL50Player();
            var target = Create.FakeNPC();
            var spellLine = createGenericSpellLine();
            var damageFocus = new HereticDoTLostOnPulse(source, spell, spellLine);
            var effectiveness = 1;

            damageFocus.FinishSpellCast(target);
            Util.LoadTestDouble(new ChanceAlwaysHundredPercent());
            damageFocus.OnDirectEffect(target, effectiveness);

            Assert.AreEqual(100, source.lastDealtDamage);
        }

        [Test]
        public void OnDirectEffect_100InitialDamageAnd25PercentGrowth_ResistTickTick_LastTickDoes150Damage()
        {
            double initialDamage = 100;
            int growthPercent = 25;
            var spell = createHereticFocusDamageSpell(initialDamage, growthPercent);
            var source = createIdealL50Player();
            var target = Create.FakeNPC();
            var spellLine = createGenericSpellLine();
            var damageFocus = new HereticDoTLostOnPulse(source, spell, spellLine);
            var effectiveness = 1;

            damageFocus.FinishSpellCast(target);
            Util.LoadTestDouble(new ChanceAlwaysZero()); //always resists
            damageFocus.OnDirectEffect(target, effectiveness);
            Util.LoadTestDouble(new ChanceAlwaysHundredPercent());
            damageFocus.OnDirectEffect(target, effectiveness);
            damageFocus.OnDirectEffect(target, effectiveness);

            Assert.AreEqual(150, source.lastDealtDamage);
        }

        [Test]
        public void OnDirectEffect_100InitialDamageAnd25PercentGrowth_TickResistTick_LastTickDoes150Damage()
        {
            double initialDamage = 100;
            int growthPercent = 25;
            var spell = createHereticFocusDamageSpell(initialDamage, growthPercent);
            var source = createIdealL50Player();
            var target = Create.FakeNPC();
            var spellLine = createGenericSpellLine();
            var damageFocus = new HereticDoTLostOnPulse(source, spell, spellLine);
            var effectiveness = 1;

            damageFocus.FinishSpellCast(target);
            Util.LoadTestDouble(new ChanceAlwaysHundredPercent());
            damageFocus.OnDirectEffect(target, effectiveness);
            Util.LoadTestDouble(new ChanceAlwaysZero());
            damageFocus.OnDirectEffect(target, effectiveness);
            Util.LoadTestDouble(new ChanceAlwaysHundredPercent());
            damageFocus.OnDirectEffect(target, effectiveness);

            Assert.AreEqual(150, source.lastDealtDamage);
        }

        [Test]
        public void OnDirectEffect_100InitialDamageAnd25PercentGrowth_TickThreeTimes_LastTickDoes150Damage()
        {
            double initialDamage = 100;
            int growthPercent = 25;
            var spell = createHereticFocusDamageSpell(initialDamage, growthPercent);
            var source = createIdealL50Player();
            var target = Create.FakeNPC();
            var spellLine = createGenericSpellLine();
            var damageFocus = new HereticDoTLostOnPulse(source, spell, spellLine);
            var effectiveness = 1;

            damageFocus.FinishSpellCast(target);
            Util.LoadTestDouble(new ChanceAlwaysHundredPercent());
            damageFocus.OnDirectEffect(target, effectiveness);
            damageFocus.OnDirectEffect(target, effectiveness);
            damageFocus.OnDirectEffect(target, effectiveness);

            Assert.AreEqual(150, source.lastDealtDamage);
        }

        [Test]
        public void OnDirectEffect_100InitialDamageAnd50PercentGrowth_TickTwice_LastTickDoes150Damage()
        {
            double initialDamage = 100;
            int growthPercent = 50;
            var spell = createHereticFocusDamageSpell(initialDamage, growthPercent);
            var source = createIdealL50Player();
            var target = Create.FakeNPC();
            var spellLine = createGenericSpellLine();
            var damageFocus = new HereticDoTLostOnPulse(source, spell, spellLine);
            var effectiveness = 1;

            damageFocus.FinishSpellCast(target);
            Util.LoadTestDouble(new ChanceAlwaysHundredPercent());
            damageFocus.OnDirectEffect(target, effectiveness);
            damageFocus.OnDirectEffect(target, effectiveness);

            Assert.AreEqual(150, source.lastDealtDamage);
        }

        [Test]
        public void OnDirectEffect_100InitialDamageAnd50PercentGrowthAnd70PercentGrowthCap_TickThreeTimes_LastTickDoes170Damage()
        {
            double initialDamage = 100;
            int growthPercent = 50;
            int growthCapPercent = 70;
            var spell = createHereticFocusDamageSpell(initialDamage, growthPercent, growthCapPercent);
            var source = createIdealL50Player();
            var target = Create.FakeNPC();
            var spellLine = createGenericSpellLine();
            var damageFocus = new HereticDoTLostOnPulse(source, spell, spellLine);
            var effectiveness = 1;

            damageFocus.FinishSpellCast(target);
            Util.LoadTestDouble(new ChanceAlwaysHundredPercent());
            damageFocus.OnDirectEffect(target, effectiveness);
            damageFocus.OnDirectEffect(target, effectiveness);
            damageFocus.OnDirectEffect(target, effectiveness);

            Assert.AreEqual(170, source.lastDealtDamage);
        }

        private Spell createHereticFocusDamageSpell(double initialDamage, int growthPercent)
        {
            int noCap = int.MaxValue;
            return createHereticFocusDamageSpell(initialDamage, growthPercent, noCap);
        }

        private Spell createHereticFocusDamageSpell(double initialDamage, int growthPercent, int growthCapPercent)
        {
            var dbspell = new DBSpell();
            dbspell.LifeDrainReturn = growthPercent;
            dbspell.AmnesiaChance = growthCapPercent;
            var spell = new Spell(dbspell, 1);
            spell.Damage = initialDamage;
            spell.Level = 50;
            return spell;
        }

        private SpellLine createGenericSpellLine()
        {
            return new SpellLine("keyname", "lineName", "specName", true);
        }

        private MockPlayer createIdealL50Player()
        {
            var player = new MockPlayer();
            player.Level = 50;
            player.modifiedEffectiveLevel = 50;
            player.modifiedIntelligence = 60;
            return player;
        }

        private class MockPlayer : FakePlayer
        {
            public int lastDealtDamage = -1;

            public MockPlayer()
            {
                this.characterClass = new DefaultCharacterClass();
            }

            public override void DealDamage(AttackData ad)
            {
                lastDealtDamage = ad.Damage;
            }

        }

        private class ChanceAlwaysHundredPercent : Util
        {
            protected override int RandomImpl(int min, int max)
            {
                return 100;
            }
        }

        private class ChanceAlwaysZero : Util
        {
            protected override int RandomImpl(int min, int max)
            {
                return -1;
            }
        }
	}
}
