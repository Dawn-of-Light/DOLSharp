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
    class UT_WhereClause
    {
        [Test]
        public void ParameterizedText_KeyIsEqualToOne_KeyEqualAtA()
        {
            var expression = DB.Column("key").IsEqualTo(1);

            Assert.That(expression.ParameterizedText, Is.EqualTo("WHERE key = @a"));
        }

        [Test]
        public void Parameters_KeyColumnIsEqualToOne_FirstQueryParameterValueIsOne()
        {
            var expression = DB.Column("key").IsEqualTo(1);

            var firstQueryParameter = expression.Parameters[0];
            Assert.That(1, Is.EqualTo(firstQueryParameter.Value));
        }

        [Test]
        public void ParameterizedText_FooIsEqualToOneAndBarIsEqualToOne_WhereFooEqualAtAAndBarEqualAtB()
        {
            var expression1 = DB.Column("foo").IsEqualTo(1);
            var expression2 = DB.Column("bar").IsEqualTo(1);

            var andExpression = expression1.And(expression2);

            var actual = andExpression.ParameterizedText;
            var expected = $"WHERE ( foo = @a AND bar = @b )";
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ParameterizedText_FooIsInOneAndTwo_FooIsInAtACommaAtB()
        {
            var expr = DB.Column("foo").IsIn(new[] { 1, 2 });
            var placeHolder1 = expr.Parameters[0].Item1;
            var placeHolder2 = expr.Parameters[1].Item1;
            var actual = expr.ParameterizedText;
            var expected = $"WHERE foo IN ( {placeHolder1} , {placeHolder2} )";
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ParameterizedText_FooIsInAandB_FooIsInAtACommaAtB()
        {
            var expr = DB.Column("foo").IsIn(new[] { "a", "b" });
            var placeHolder1 = expr.Parameters[0].Item1;
            var placeHolder2 = expr.Parameters[1].Item1;
            var actual = expr.ParameterizedText;
            var expected = $"WHERE foo IN ( {placeHolder1} , {placeHolder2} )";
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ParameterizedText_FooIsInEmptyArray_WhereFalse()
        {
            var expr = DB.Column("foo").IsIn(Array.Empty<object>());
            var actual = expr.ParameterizedText;
            var expected = $"WHERE false";
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ParameterizedText_FooIsNotInEmptyArray_WhereTrue()
        {
            var expr = DB.Column("foo").IsNotIn(Array.Empty<object>());
            var actual = expr.ParameterizedText;
            var expected = $"WHERE true";
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Parameters_Expression1AndExpression2_ConcatenationOfBothParameters()
        {
            var expression1 = DB.Column("foo").IsEqualTo(1);
            var expression2 = DB.Column("bar").IsEqualTo(2);

            var andExpression = expression1.And(expression2);

            var actual = andExpression.Parameters;
            var expected = new[] { new QueryParameter("@a", 1), new QueryParameter("@b", 2) };
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ParameterizedText_EmptyWhereClause_EmptyString()
        {
            var expression = WhereClause.Empty;

            var actual = expression.ParameterizedText;
            var expected = string.Empty;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Parameters_EmptyWhereClause_Empty()
        {
            var expression = WhereClause.Empty;

            var actual = expression.Parameters;
            Assert.That(actual, Is.Empty);
        }

        [Test]
        public void And_EmptyAndFilterExpression_EqualsFilterExpression()
        {
            var emptyExpression = WhereClause.Empty;
            var filterExpression = DB.Column("foo").IsEqualTo(0);

            var andExpression = emptyExpression.And(filterExpression);

            var actual = andExpression;
            var expected = filterExpression;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void And_FilterExpressionAndEmpty_EqualsFilterExpression()
        {
            var emptyExpression = WhereClause.Empty;
            var filterExpression = DB.Column("foo").IsEqualTo(2);

            var andExpression = filterExpression.And(emptyExpression);

            var actual = andExpression;
            var expected = filterExpression;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void EmptyExpression_AndEmptyExpression_OnlyFilterExpression()
        {
            var andExpression = WhereClause.Empty.And(WhereClause.Empty);

            var actual = andExpression.ParameterizedText;
            var expected = string.Empty;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ParameterizedText_DBColumnFooIsNull_WhereFooIsNull()
        {
            var actual = DB.Column("Foo").IsNull().ParameterizedText;
            var expected = "WHERE Foo IS NULL";
            Assert.That(actual, Is.EqualTo(expected));
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
        public void Equals_TwoFreshEmptyWhereClauses_True()
        {
            Assert.That(WhereClause.Empty.Equals(WhereClause.Empty), Is.True);
        }

        [Test]
        public void Equals_TwoFilterExpressionsWithSameProperties_True()
        {
            var expression1 = DB.Column("foo").IsEqualTo(1);
            var expression2 = DB.Column("foo").IsEqualTo(1);

            Assert.That(expression1.Equals(expression2), Is.True);
        }

        [Test]
        public void Equals_TwoFilterExpressionsWithOnlyDifferentValue_False()
        {
            var expression1 = DB.Column("foo").IsEqualTo(0);
            var expression2 = DB.Column("foo").IsEqualTo(1);

            Assert.That(expression1.Equals(expression2), Is.False);
        }

        [Test]
        public void Equals_TwoFilterExpressionsWithOnlyDifferentColumnName_False()
        {
            var expression1 = DB.Column("foo").IsEqualTo(1);
            var expression2 = DB.Column("bar").IsEqualTo(1);

            Assert.That(expression1.Equals(expression2), Is.False);
        }

        [Test]
        public void Equals_TwoFilterExpressionsWithOnlyDifferentOperator_False()
        {
            var expression1 = DB.Column("foo").IsEqualTo(1);
            var expression2 = DB.Column("foo").IsNotEqualTo(1);

            Assert.That(expression1.Equals(expression2), Is.False);
        }

        [Test]
        public void Equals_TwoAndExpressionsWithSameExpressions_True()
        {
            var filterExpression = DB.Column("foo").IsEqualTo(1);
            var andExpression1 = filterExpression.And(filterExpression);
            var andExpression2 = filterExpression.And(filterExpression);

            Assert.That(andExpression1.Equals(andExpression2), Is.True);
        }

        [Test]
        public void Equals_AndExpressionsAndOrExpressionWithSameExpressions_False()
        {
            var filterExpression = DB.Column("foo").IsEqualTo(1);
            var andExpression = filterExpression.And(filterExpression);
            var orExpression = filterExpression.Or(filterExpression);

            Assert.That(andExpression.Equals(orExpression), Is.False);
        }
    }
}
