using NUnit.Framework;
using DOL.GS.Spells;
using DOL.GS;
using DOL.Database;

namespace DOL.UnitTests.Gameserver
{
    [TestFixture]
    class UT_HereticFocusSpells
    {
        [SetUp]
        public void SetUp()
        {
            GameServer.LoadTestDouble(new FakeServer());
        }

        [Test]
        public void OnDirectEffect_100InitialDamage_NoTick_FirstTickDoes100Damage()
        {
            double initialDamage = 100;
            int growthPercent = 25;
            var spell = NewHereticFocusDamageSpell(initialDamage, growthPercent);
            var source = NewL50Player();
            var target = NewFakeNPC();
            var spellLine = NewGenericSpellLine();
            var damageFocus = new RampingDamageFocus(source, spell, spellLine);
            var effectiveness = 1;

            Util.LoadTestDouble(new ChanceAlwaysHundredPercent());
            damageFocus.OnDirectEffect(target, effectiveness);

            var actual = source.LastDamageDealt;
            var expected = 100;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void OnDirectEffect_100InitialDamageAnd25PercentGrowth_TickTwice_NextTickDoes150Damage()
        {
            double initialDamage = 100;
            int growthPercent = 25;
            var spell = NewHereticFocusDamageSpell(initialDamage, growthPercent);
            var source = NewL50Player();
            var target = NewFakeNPC();
            var spellLine = NewGenericSpellLine();
            var damageFocus = new RampingDamageFocus(source, spell, spellLine);
            var effectiveness = 1;

            Util.LoadTestDouble(new ChanceAlwaysHundredPercent());
            damageFocus.OnSpellPulse(null);
            damageFocus.OnSpellPulse(null);
            damageFocus.OnDirectEffect(target, effectiveness);

            var actual = source.LastDamageDealt;
            var expected = 150;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void OnDirectEffect_100InitialDamageAnd50PercentGrowth_OneTick_NextTickDoes150Damage()
        {
            double initialDamage = 100;
            int growthPercent = 50;
            var spell = NewHereticFocusDamageSpell(initialDamage, growthPercent);
            var source = NewL50Player();
            var target = NewFakeNPC();
            var spellLine = NewGenericSpellLine();
            var damageFocus = new RampingDamageFocus(source, spell, spellLine);
            var effectiveness = 1;

            Util.LoadTestDouble(new ChanceAlwaysHundredPercent());
            damageFocus.OnSpellPulse(null);
            damageFocus.OnDirectEffect(target, effectiveness);

            var actual = source.LastDamageDealt;
            var expected = 150;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void OnDirectEffect_100InitialDamageAnd50PercentGrowthAnd70PercentGrowthCap_TickTwice_NextTickDoes170Damage()
        {
            double initialDamage = 100;
            int growthPercent = 50;
            int growthCapPercent = 70;
            var spell = NewHereticFocusDamageSpell(initialDamage, growthPercent, growthCapPercent);
            var source = NewL50Player();
            var target = NewFakeNPC();
            var spellLine = NewGenericSpellLine();
            var damageFocus = new RampingDamageFocus(source, spell, spellLine);
            var effectiveness = 1;

            Util.LoadTestDouble(new ChanceAlwaysHundredPercent());
            damageFocus.OnSpellPulse(null);
            damageFocus.OnSpellPulse(null);
            damageFocus.OnDirectEffect(target, effectiveness);

            var actual = source.LastDamageDealt;
            var expected = 170;
            Assert.That(actual, Is.EqualTo(expected));
        }

        private Spell NewHereticFocusDamageSpell(double initialDamage, int growthPercent)
        {
            int noCap = int.MaxValue;
            return NewHereticFocusDamageSpell(initialDamage, growthPercent, noCap);
        }

        private Spell NewHereticFocusDamageSpell(double initialDamage, int growthPercent, int growthCapPercent)
        {
            var dbspell = new DBSpell();
            dbspell.LifeDrainReturn = growthPercent;
            dbspell.AmnesiaChance = growthCapPercent;
            dbspell.Target = "Enemy";
            var spell = new Spell(dbspell, 1);
            spell.Damage = initialDamage;
            spell.Level = 50;
            return spell;
        }

        private SpellLine NewGenericSpellLine()
        {
            return new SpellLine("keyname", "lineName", "specName", true);
        }

        private FakePlayer NewL50Player()
        {
            var player = new FakePlayer();
            player.fakeCharacterClass = CharacterClass.None;
            player.Level = 50;
            return player;
        }

        private class ChanceAlwaysHundredPercent : Util
        {
            protected override int RandomImpl(int min, int max)
            {
                return 100;
            }
        }

        private static FakeNPC NewFakeNPC() => new FakeNPC();
    }
}
