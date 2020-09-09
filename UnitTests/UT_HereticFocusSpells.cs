using NUnit.Framework;
using DOL.GS.Spells;
using DOL.GS;
using DOL.Database;
using DOL.GS.ServerRules;

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
            GameServer.LoadTestDouble(new FakeServer());
        }

        [Test]
        public void OnDirectEffect_100InitialDamage_NoTick_FirstTickDoes100Damage()
        {
            double initialDamage = 100;
            int growthPercent = 25;
            var spell = createHereticFocusDamageSpell(initialDamage, growthPercent);
            var source = createIdealL50Player();
            var target = createGenericNPC();
            var spellLine = createGenericSpellLine();
            var damageFocus = new RampingDamageFocus(source, spell, spellLine);
            var effectiveness = 1;

            Util.LoadTestDouble(new ChanceAlwaysHundredPercent());
            damageFocus.OnDirectEffect(target, effectiveness);

            var actual = source.LastDamageDealt;
            var expected = 100;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OnDirectEffect_100InitialDamageAnd25PercentGrowth_TickTwice_NextTickDoes150Damage()
        {
            double initialDamage = 100;
            int growthPercent = 25;
            var spell = createHereticFocusDamageSpell(initialDamage, growthPercent);
            var source = createIdealL50Player();
            var target = createGenericNPC();
            var spellLine = createGenericSpellLine();
            var damageFocus = new RampingDamageFocus(source, spell, spellLine);
            var effectiveness = 1;

            Util.LoadTestDouble(new ChanceAlwaysHundredPercent());
            damageFocus.OnSpellPulse(null);
            damageFocus.OnSpellPulse(null);
            damageFocus.OnDirectEffect(target, effectiveness);

            var actual = source.LastDamageDealt;
            var expected = 150;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OnDirectEffect_100InitialDamageAnd50PercentGrowth_OneTick_NextTickDoes150Damage()
        {
            double initialDamage = 100;
            int growthPercent = 50;
            var spell = createHereticFocusDamageSpell(initialDamage, growthPercent);
            var source = createIdealL50Player();
            var target = createGenericNPC();
            var spellLine = createGenericSpellLine();
            var damageFocus = new RampingDamageFocus(source, spell, spellLine);
            var effectiveness = 1;

            Util.LoadTestDouble(new ChanceAlwaysHundredPercent());
            damageFocus.OnSpellPulse(null);
            damageFocus.OnDirectEffect(target, effectiveness);

            var actual = source.LastDamageDealt;
            var expected = 150;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OnDirectEffect_100InitialDamageAnd50PercentGrowthAnd70PercentGrowthCap_TickTwice_NextTickDoes170Damage()
        {
            double initialDamage = 100;
            int growthPercent = 50;
            int growthCapPercent = 70;
            var spell = createHereticFocusDamageSpell(initialDamage, growthPercent, growthCapPercent);
            var source = createIdealL50Player();
            var target = createGenericNPC();
            var spellLine = createGenericSpellLine();
            var damageFocus = new RampingDamageFocus(source, spell, spellLine);
            var effectiveness = 1;

            Util.LoadTestDouble(new ChanceAlwaysHundredPercent());
            damageFocus.OnSpellPulse(null);
            damageFocus.OnSpellPulse(null);
            damageFocus.OnDirectEffect(target, effectiveness);

            var actual = source.LastDamageDealt;
            var expected = 170;
            Assert.AreEqual(expected, actual);
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
            dbspell.Target = "Enemy";
            var spell = new Spell(dbspell, 1);
            spell.Damage = initialDamage;
            spell.Level = 50;
            return spell;
        }

        private SpellLine createGenericSpellLine()
        {
            return new SpellLine("keyname", "lineName", "specName", true);
        }

        private FakePlayer createIdealL50Player()
        {
            var player = new FakePlayer();
            player.characterClass = new DefaultCharacterClass();
            player.Level = 50;
            player.modifiedEffectiveLevel = 50;
            player.modifiedIntelligence = 60;
            return player;
        }

        private class ChanceAlwaysHundredPercent : Util
        {
            protected override int RandomImpl(int min, int max)
            {
                return 100;
            }
        }

        private static FakeNPC createGenericNPC() { return FakeNPC.CreateGeneric(); }
    }
}
