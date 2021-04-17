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
using NUnit.Framework;

using DOL.Database;
using System;

namespace DOL.UnitTests.Database
{
    [TestFixture]
    class UT_WhereExpression
    {
        [Test]
        public void FilterExpressionWhereClause_KeyColumn_KeyEqualsPlaceHolder()
        {
            var expression = DB.Column("key").IsEqualTo(1);

            var firstQueryParameter = expression.QueryParameters[0];
            var placeHolder = firstQueryParameter.Item1;
            Assert.AreEqual(expression.WhereClause, "key = " + placeHolder);
        }

        [Test]
        public void FilterExpressionWhereQueryParameters_KeyColumnIsEqualToOne_FirstQueryParameterValueIsOne()
        {
            var expression = DB.Column("key").IsEqualTo(1);

            var firstQueryParameter = expression.QueryParameters[0];
            Assert.AreEqual(firstQueryParameter.Value, 1);
        }

        [Test]
        public void FilterExpressionQueryParameters_TwoCreated_PlaceHoldersAreNotEqual()
        {
            var expression1 = DB.Column("foo").IsEqualTo(1);
            var expression2 = DB.Column("foo").IsEqualTo(1);

            var placeHolder1 = expression1.QueryParameters[0];
            var placeHolder2 = expression2.QueryParameters[0];
            Assert.AreNotEqual(placeHolder1.Item1, placeHolder2.Item1);
        }

        [Test]
        public void AndExpressionWhereClause_TwoFilterExpressions_FilterExpressionWhereClausesConnectedWithAND()
        {
            var expression1 = DB.Column("foo").IsEqualTo(1);
            var expression2 = DB.Column("bar").IsEqualTo(2);

            var andExpression = expression1.And(expression2);

            var placeHolder1 = expression1.QueryParameters[0].Item1;
            var placeHolder2 = expression2.QueryParameters[0].Item1;
            var actual = andExpression.WhereClause;
            var expected = $"(foo = {placeHolder1} AND bar = {placeHolder2})";
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void AndExpressionWhereClause_TwoFilterExpressions_FilterExpressionParametersInArray()
        {
            var expression1 = DB.Column("foo").IsEqualTo(1);
            var expression2 = DB.Column("bar").IsEqualTo(2);

            var andExpression = expression1.And(expression2);

            var actual = andExpression.QueryParameters;
            var expected = new QueryParameter[] { expression1.QueryParameters[0], expression2.QueryParameters[0] };
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void DBColumn_Null_ThrowArgumentException()
        {
            Assert.Throws(typeof(ArgumentException), () => DB.Column(null));
        }

        [Test]
        public void DBColumn_EmptyString_ThrowArgumentException()
        {
            Assert.Throws(typeof(ArgumentException), () => DB.Column(string.Empty));
        }
    }
}
