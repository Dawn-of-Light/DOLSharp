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
        public void WhereClause_KeyIsEqualToOne_KeyEqualAtA()
        {
            var expression = DB.Column("key").IsEqualTo(1);

            Assert.AreEqual("WHERE key = @a", expression.WhereClause);
        }

        [Test]
        public void QueryParameters_KeyColumnIsEqualToOne_FirstQueryParameterValueIsOne()
        {
            var expression = DB.Column("key").IsEqualTo(1);

            var firstQueryParameter = expression.QueryParameters[0];
            Assert.AreEqual(firstQueryParameter.Value, 1);
        }

        [Test]
        public void WhereClause_FooIsEqualToOneAndBarIsEqualToOne_WhereFooEqualAtAAndBarEqualAtB()
        {
            var expression1 = DB.Column("foo").IsEqualTo(1);
            var expression2 = DB.Column("bar").IsEqualTo(1);

            var andExpression = expression1.And(expression2);

            var actual = andExpression.WhereClause;
            var expected = $"WHERE ( foo = @a AND bar = @b )";
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void InExpressionWhereClause_WithIntValues()
        {
            var expr = DB.Column("foo").IsIn(new [] { 1, 2 });
            var placeHolder1 = expr.QueryParameters[0].Item1;
            var placeHolder2 = expr.QueryParameters[1].Item1;
            var actual = expr.WhereClause;
            var expected = $"foo IN ( {placeHolder1} , {placeHolder2} )";
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void InExpressionWhereClause_WithStringValues()
        {
            var expr = DB.Column("foo").IsIn(new [] { "a", "b" });
            var placeHolder1 = expr.QueryParameters[0].Item1;
            var placeHolder2 = expr.QueryParameters[1].Item1;
            var actual = expr.WhereClause;
            var expected = $"foo IN ( {placeHolder1} , {placeHolder2} )";
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void QueryParameters_Expression1AndExpression2_ConcatenationOfBothQueryParameters()
        {
            var expression1 = DB.Column("foo").IsEqualTo(1);
            var expression2 = DB.Column("bar").IsEqualTo(2);

            var andExpression = expression1.And(expression2);

            var actual = andExpression.QueryParameters;
            var expected = new[] { new QueryParameter("@a", 1), new QueryParameter("@b", 2) };
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void WhereClause_EmptyWhereExpression_EmptyString()
        {
            var expression = WhereExpression.Empty;

            var actual = expression.QueryParameters;
            Assert.IsEmpty(actual);
        }

        [Test]
        public void QueryParameters_EmptyWhereExpression_Empty()
        {
            var expression = WhereExpression.Empty;

            var actual = expression.WhereClause;
            var expected = string.Empty;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void And_EmptyAndFilterExpression_EqualsFilterExpression()
        {
            var emptyExpression = WhereExpression.Empty;
            var filterExpression = DB.Column("foo").IsEqualTo(0);

            var andExpression = emptyExpression.And(filterExpression);

            var actual = andExpression;
            var expected = filterExpression;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void And_FilterExpressionAndEmpty_EqualsFilterExpression()
        {
            var emptyExpression = WhereExpression.Empty;
            var filterExpression = DB.Column("foo").IsEqualTo(2);

            var andExpression = filterExpression.And(emptyExpression);

            var actual = andExpression;
            var expected = filterExpression;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void EmptyExpression_AndEmptyExpression_OnlyFilterExpression()
        {
            var andExpression = WhereExpression.Empty.And(WhereExpression.Empty);

            var actual = andExpression.WhereClause;
            var expected = string.Empty;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void WhereClause_DBColumnFooIsNull_WhereFooIsNull()
        {
            var actual = DB.Column("Foo").IsNull().WhereClause;
            var expected = "WHERE Foo IS NULL";
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

        [Test]
        public void Equals_TwoFreshEmptyWhereExpressions_True()
        {
            Assert.IsTrue(WhereExpression.Empty.Equals(WhereExpression.Empty));
        }

        [Test]
        public void Equals_TwoFilterExpressionsWithSameProperties_True()
        {
            var expression1 = DB.Column("foo").IsEqualTo(1);
            var expression2 = DB.Column("foo").IsEqualTo(1);

            Assert.IsTrue(expression1.Equals(expression2));
        }

        [Test]
        public void Equals_TwoFilterExpressionsWithOnlyDifferentValue_False()
        {
            var expression1 = DB.Column("foo").IsEqualTo(0);
            var expression2 = DB.Column("foo").IsEqualTo(1);

            Assert.IsFalse(expression1.Equals(expression2));
        }

        [Test]
        public void Equals_TwoFilterExpressionsWithOnlyDifferentColumnName_False()
        {
            var expression1 = DB.Column("foo").IsEqualTo(1);
            var expression2 = DB.Column("bar").IsEqualTo(1);

            Assert.IsFalse(expression1.Equals(expression2));
        }

        [Test]
        public void Equals_TwoFilterExpressionsWithOnlyDifferentOperator_False()
        {
            var expression1 = DB.Column("foo").IsEqualTo(1);
            var expression2 = DB.Column("foo").IsNotEqualTo(1);

            Assert.IsFalse(expression1.Equals(expression2));
        }

        [Test]
        public void Equals_TwoAndExpressionsWithSameExpressions_True()
        {
            var filterExpression = DB.Column("foo").IsEqualTo(1);
            var andExpression1 = filterExpression.And(filterExpression);
            var andExpression2 = filterExpression.And(filterExpression);

            Assert.IsTrue(andExpression1.Equals(andExpression2));
        }

        [Test]
        public void Equals_AndExpressionsAndOrExpressionWithSameExpressions_False()
        {
            var filterExpression = DB.Column("foo").IsEqualTo(1);
            var andExpression = filterExpression.And(filterExpression);
            var orExpression = filterExpression.Or(filterExpression);

            Assert.IsFalse(andExpression.Equals(orExpression));
        }
    }
}
