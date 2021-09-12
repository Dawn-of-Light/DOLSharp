/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
using DOL.Database;
using DOL.GS;
using DOL.GS.Spells;
using NUnit.Framework;

namespace DOL.Integration.Gameserver
{
    [TestFixture]
    class Test_SpellDelve
    {
        [Test]
        public void GetClientDelve_DeflectPunctures_CompareContent()
        {
            var dbSpell = new DBSpell();
            dbSpell.Name = "Deflect Punctures";
            dbSpell.Target = "Self";
            dbSpell.Type = "SavageThrustResistanceBuff";
            dbSpell.Range = 0;
            dbSpell.Power = -3;
            dbSpell.CastTime = 0;
            dbSpell.Duration = 30;
            dbSpell.RecastDelay = 5;
            dbSpell.Value = 5;
            dbSpell.TooltipId = 3838;

            var actual = new SpellDelve(new Spell(dbSpell, 0)).GetClientDelve().ClientMessage;

            Assert.That(actual, Does.StartWith("(Spell "));
            Assert.That(actual, Does.Contain("(Index \"3838\")"));
            Assert.That(actual, Does.Contain("(Name \"Deflect Punctures\")"));
            Assert.That(actual, Does.Contain("(instant \"1\")"));
            Assert.That(actual, Does.Contain("(power_cost \"-3\")"));
            Assert.That(actual, Does.Contain("(duration \"30\")"));
            Assert.That(actual, Does.Contain("(dur_type \"2\")"));
            Assert.That(actual, Does.Contain("(timer_value \"5\")"));
            Assert.That(actual, Does.Contain("(cost_type \"2\")"));
            Assert.That(actual, Does.Contain("(delve_string \"Increases the target's resistance to thrust damage by 5%.\")"));
            Assert.That(actual, Does.Not.Contain("cast_timer"));
        }

        [Test]
        public void GetClientDelve_MajorSmite_CompareContent()
        {
            var dbSpell = new DBSpell();
            dbSpell.Name = "Major Smite";
            dbSpell.Description = "Enhance the targets resistance to the listed damage type.";
            dbSpell.Target = "Enemy";
            dbSpell.Range = 1500;
            dbSpell.Power = 9;
            dbSpell.CastTime = 2.6;
            dbSpell.Type = "DirectDamage";
            dbSpell.Damage = 49;
            dbSpell.DamageType = 15;
            dbSpell.TooltipId = 1422;

            var actual = new SpellDelve(new Spell(dbSpell, 0)).GetClientDelve().ClientMessage;

            Assert.That(actual, Does.StartWith("(Spell "));
            Assert.That(actual, Does.Contain("(Index \"1422\")"));
            Assert.That(actual, Does.Contain("(Name \"Major Smite\")"));
            Assert.That(actual, Does.Contain("(cast_timer \"600\")"));
            Assert.That(actual, Does.Contain("(power_cost \"9\")"));
            Assert.That(actual, Does.Contain("(damage_type \"11\")"));
            Assert.That(actual, Does.Contain("(delve_string \"Does 49 Spirit damage to the target.\")"));
        }

        [Test]
        public void GetClientDelve_GenericSpellWith2SecondsCastTime_CastTimerIsZeroAndNotInstant()
        {
            var dbSpell = new DBSpell();
            dbSpell.Name = "GenericSpell";
            dbSpell.Type = "Heal";
            dbSpell.CastTime = 2.0;

            var actual = new SpellDelve(new Spell(dbSpell, 0)).GetClientDelve().ClientMessage;

            Assert.That(actual, Does.Not.Contain("instant"));
        }
    }
}
