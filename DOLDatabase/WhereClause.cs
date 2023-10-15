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
using System;
using System.Collections.Generic;
using System.Linq;

namespace DOL.Database
{
    public class WhereClause
    {
        internal virtual ParameterizedExpression expression { get; }

        private WhereClause(ParameterizedExpression expr)
        {
            expression = expr;
        }

        internal WhereClause(string formatString, params object[] values)
        {
            expression = new ParameterizedExpression(formatString, values);
        }

        internal WhereClause(string formatString, ParameterizedExpression expr)
        {
            var queryText = formatString.Replace("{}", expr.QueryText);
            expression = new ParameterizedExpression(queryText, expr.Parameters);
        }

        public virtual string ParameterizedText
        {
            get
            {
                if (expression.IsEmpty) return string.Empty;
                var clause = $"WHERE {expression.QueryText}";
                for (uint i = 0; i < Parameters.Length; i++)
                {
                    var searchString = "{}";
                    int index = clause.IndexOf(searchString);
                    clause = clause.Remove(index, searchString.Length);
                    clause = clause.Insert(index, GetPlaceHolder(i));
                }
                return clause.Trim();
            }
        }

        public virtual QueryParameter[] Parameters
        {
            get
            {
                var result = new List<QueryParameter>();
                uint parameterID = 0;
                foreach (var param in expression.Parameters)
                {
                    result.Add(new QueryParameter(GetPlaceHolder(parameterID), param));
                    parameterID++;
                }
                return result.ToArray();
            }
        }

        public virtual WhereClause And(WhereClause rightExpression)
            => ChainExpressions(this, "AND", rightExpression);
        public virtual WhereClause Or(WhereClause rightExpression)
            => ChainExpressions(this, "OR", rightExpression);

        private WhereClause ChainExpressions(WhereClause left, string chainingOperator, WhereClause right)
        {
            if (right.Equals(Empty)) return new WhereClause(left.expression);
            if (left.Equals(Empty)) return new WhereClause(right.expression);

            var leftText = left.expression.QueryText;
            var rightText = right.expression.QueryText;
            var leftParameters = left.expression.Parameters;
            var rightParameters = right.expression.Parameters;
            var queryText = $"( {leftText} {chainingOperator} {rightText} )";
            var parameters = leftParameters.Concat(rightParameters).ToArray();
            return new WhereClause(new ParameterizedExpression(queryText, parameters));
        }

        public static readonly WhereClause Empty = new WhereClause("", new object[] { });

        public override bool Equals(object obj)
        {
            if(obj is WhereClause clause)
            {
                return expression.Equals(clause.expression);
            }
            else return false;
        }

        public override int GetHashCode() => base.GetHashCode();

        private string GetPlaceHolder(uint id)
        {
            var prefix = "@";
            uint radix = (uint)alphabet.Length;
            var result = "";
            do
            {
                var letter = alphabet[(int)(id % radix)];
                result = letter + result;
                id /= radix;
            } while (id > 0);
            return prefix + result;
        }

        private static string alphabet = "abcdefghijklmnopqrstuvwxyz";
    }

    internal class ParameterizedExpression
    {
        public string QueryText { get; }
        public object[] Parameters { get; }

        public ParameterizedExpression(string queryText, params object[] parameters)
        {
            QueryText = queryText;
            Parameters = parameters;
        }

        public bool IsEmpty => string.IsNullOrEmpty(QueryText);

        public override bool Equals(object obj)
        {
            if(obj is ParameterizedExpression paramQuery)
            {
                var sameQueryText = paramQuery.QueryText.Equals(QueryText);
                return sameQueryText && Enumerable.SequenceEqual(Parameters, paramQuery.Parameters);
            }
            else return false;
        }

        public override int GetHashCode() => base.GetHashCode();
    }

    public class DB
    {
        public static DBColumn Column(string columnName) => new DBColumn(columnName);
    }

    public class DBColumn
    {
        public string Name { get; }

        internal DBColumn(string columnName)
        {
            if (string.IsNullOrEmpty(columnName)) throw new ArgumentException("ColumnName may not be null or empty.");
            Name = columnName;
        }

        public WhereClause IsEqualTo(object val) => new WhereClause($"{Name} = {{}}", val);
        public WhereClause IsNotEqualTo(object val) => new WhereClause($"{Name} != {{}}", val);
        public WhereClause IsGreatherThan(object val) => new WhereClause($"{Name} > {{}}", val);
        public WhereClause IsGreaterOrEqualTo(object val) => new WhereClause($"{Name} >= {{}}", val);
        public WhereClause IsLessThan(object val) => new WhereClause($"{Name} < {{}}", val);
        public WhereClause IsLessOrEqualTo(object val) => new WhereClause($"{Name} <= {{}}", val);
        public WhereClause IsLike(object val) => new WhereClause($"{Name} LIKE {{}}", val);
        public WhereClause IsNull() => new WhereClause($"{Name} IS NULL");
        public WhereClause IsNotNull() => new WhereClause($"{Name} IS NOT NULL");
        public WhereClause IsIn<T>(IEnumerable<T> values)
        {
            if (!values.Any()) return new WhereClause("false");
            var expr = $"( {string.Join(" , ", values.Select(s => "{}"))} )";
            var collectionParamText = new ParameterizedExpression(expr, values.Cast<object>().ToArray());
            return new WhereClause($"{Name} IN {{}}", collectionParamText);
        }
        public WhereClause IsNotIn<T>(IEnumerable<T> values)
        {
            if (!values.Any()) return new WhereClause("true");
            var expr = $"( {string.Join(" , ", values.Select(s => "{}"))} )";
            var collectionParamText = new ParameterizedExpression(expr, values.Cast<object>().ToArray());
            return new WhereClause($"{Name} NOT IN {{}}", collectionParamText);
        }
    }
}