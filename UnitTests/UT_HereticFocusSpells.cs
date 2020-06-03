using NUnit.Framework;
using DOL.GS.Spells;
using DOL.GS;

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
        public void OnDirectEffect_100InitialDamage_Hit_LastDamageTickIsHundred()
        {
            var source = createIdealL50Player();
            var target = Create.FakeNPC();
            var spell = Create.Spell();
            spell.Damage = 100;
            spell.Level = 50;
            var spellLine = createGenericSpellLine();
            var damageFocus = new HereticDoTLostOnPulse(source, spell, spellLine);
            var effectiveness = 1;

            damageFocus.FinishSpellCast(target);
            Util.LoadTestDouble(new ChanceAlwaysHundredPercent());
            damageFocus.OnDirectEffect(target, effectiveness);

            Assert.AreEqual(100, source.lastDealtDamage);
        }

        [Test]
        public void OnDirectEffect_100InitialDamage_ResistHitHit_LastDamageTickIs125()
        {
            var source = createIdealL50Player();
            var target = Create.FakeNPC();
            var spell = Create.Spell();
            spell.Damage = 100;
            spell.Level = 50;
            var spellLine = createGenericSpellLine();
            var damageFocus = new HereticDoTLostOnPulse(source, spell, spellLine);
            var effectiveness = 1;

            damageFocus.FinishSpellCast(target);
            Util.LoadTestDouble(new ChanceAlwaysZero());
            damageFocus.OnDirectEffect(target, effectiveness);
            Util.LoadTestDouble(new ChanceAlwaysHundredPercent());
            damageFocus.OnDirectEffect(target, effectiveness);
            damageFocus.OnDirectEffect(target, effectiveness);

            Assert.AreEqual(125, source.lastDealtDamage);
        }

        [Test]
        public void OnDirectEffect_100InitialDamage_HitResistHit_LastDamageTickIs93()
        {
            var source = createIdealL50Player();
            var target = Create.FakeNPC();
            var spell = Create.Spell();
            spell.Damage = 100;
            spell.Level = 50;
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

            Assert.AreEqual(93, source.lastDealtDamage);
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
