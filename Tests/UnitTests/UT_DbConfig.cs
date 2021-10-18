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
using NUnit.Framework;

namespace DOL.UnitTests.Database
{
    [TestFixture]
    class UT_DbConfig
    {
        [Test]
        public void ConnectionString_NonDefaultOptionSetToValue_NonDefaultOptionEqualValue()
        {
            var dbConfig = new DbConfig("NonDefaultOption=Value");

            var actual = dbConfig.ConnectionString;
            var expected = "NonDefaultOption=Value";
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ConnectionString_AddDefaultOptionWithValue_DefaultOptionsEqualValue()
        {
            var dbConfig = new DbConfig("");

            dbConfig.AddDefaultOption("Default Option", "Value");

            var actual = dbConfig.ConnectionString;
            var expected = "Default Option=Value";
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ConnectionString_SetDefaultOptionToAnotherValue_DefaultOptionHasAnotherValue()
        {
            var dbConfig = new DbConfig("Default Option=Another Value");
            dbConfig.AddDefaultOption("Default Option", "Value");

            var actual = dbConfig.ConnectionString;
            var expected = "Default Option=Another Value";
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ConnectionString_DefaultOptionWithoutSpaces_DefaultOptionWithSpace()
        {
            var dbConfig = new DbConfig("DefaultOption=");
            dbConfig.AddDefaultOption("Default Option", "");

            var actual = dbConfig.ConnectionString;
            var expected = "Default Option=";
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ConnectionString_NonDefaultOption_DefaultOptionAtTheEnd()
        {
            var dbConfig = new DbConfig("NonDefaultOption=");
            dbConfig.AddDefaultOption("Default Option", "");

            var actual = dbConfig.ConnectionString;
            var expected = "NonDefaultOption=;Default Option=";
            Assert.AreEqual(expected, actual);
        }
    }
}
