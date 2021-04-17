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
    public abstract class WhereExpression
    {
        public abstract string WhereClause { get; }
        public abstract QueryParameter[] QueryParameters { get; }

        public virtual WhereExpression And(WhereExpression rightExpression) => new ChainingExpression(this, "AND", rightExpression);
        public virtual WhereExpression Or(WhereExpression rightExpression) => new ChainingExpression(this, "OR", rightExpression);

        public static WhereExpression Empty => new EmptyWhereExpression();

        public override int GetHashCode() => base.GetHashCode();
    }

    internal class EmptyWhereExpression : WhereExpression
    {
        public override string WhereClause => "";
        public override QueryParameter[] QueryParameters => new QueryParameter[0];

        public override WhereExpression And(WhereExpression rightExpression) => rightExpression;
        public override WhereExpression Or(WhereExpression rightExpression) => rightExpression;

        public override bool Equals(object obj) => obj is EmptyWhereExpression;
        public override int GetHashCode() => base.GetHashCode();
    }

    internal class FilterExpression : WhereExpression
    {
        private static string alphabet = "abcdefghijklmnopqrstuvwxyz";
        private static uint placeHolderIndex = 0;

        private string columnName;
        private string op;
        private object val;
        private uint id;

        internal FilterExpression(string columnName, string op, object val)
        {
            this.columnName = columnName;
            this.op = op;
            this.val = val;
            id = GetID();
        }

        public override string WhereClause
        {
            get
            {
                if (op != "IN")
                    return $"{columnName} {op} {GetPlaceHolder(id)}";
                var values = (IEnumerable<object>)val;
                var prefix = GetPlaceHolder(id);
                var ids = string.Join(",", values.Select((_, i) => $"{prefix}{i}"));
                return $"{columnName} IN ({ids})";
            }
        }
        public override QueryParameter[] QueryParameters
        {
            get
            {
                if (op != "IN")
                    return new QueryParameter[] { new QueryParameter(GetPlaceHolder(id), val) };
                var values = (IEnumerable<object>)val;
                var prefix = GetPlaceHolder(id);
                return values.Select((v, i) => new QueryParameter($"{prefix}{i}", v)).ToArray();
            }
        }

        protected static string GetPlaceHolder(uint id)
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

        private uint GetID() => ++placeHolderIndex - 1;

        public override bool Equals(object obj)
        {
            if (obj is FilterExpression filterExpression)
            {
                return filterExpression.op.Equals(op) 
                    && filterExpression.columnName.Equals(columnName) 
                    && filterExpression.val.Equals(val);
            }
            return false;
        }

        public override int GetHashCode() => base.GetHashCode();
    }

    internal class PlainTextExpression : WhereExpression
    {
        private string columnName;
        private string op;

        internal PlainTextExpression(string columnName, string op)
        {
            this.columnName = columnName;
            this.op = op;
        }

        public override string WhereClause => $"{columnName} {op}";
        public override QueryParameter[] QueryParameters => new QueryParameter[] { };
    }

    internal class ChainingExpression : WhereExpression
    {
        private WhereExpression left;
        private WhereExpression right;
        private string chainingOperator;

        internal ChainingExpression(WhereExpression left, string chainingOperator, WhereExpression right)
        {
            this.left = left;
            this.right = right;
            this.chainingOperator = chainingOperator;
        }

        public override string WhereClause
        {
            get
            {
                if (right is EmptyWhereExpression) return left.WhereClause;
                return $"({left.WhereClause} {chainingOperator} {right.WhereClause})";
            }
        }

        public override QueryParameter[] QueryParameters
        {
            get
            {
                var list = new List<QueryParameter>();
                list.AddRange(left.QueryParameters);
                list.AddRange(right.QueryParameters);
                return list.ToArray();
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is ChainingExpression chainingExpression)
            {
                return chainingExpression.left.Equals(left)
                    && chainingExpression.chainingOperator.Equals(chainingOperator)
                    && chainingExpression.right.Equals(right);
            }
            return false;
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

        public WhereExpression IsEqualTo(object val) => new FilterExpression(Name, "=", val);
        public WhereExpression IsNotEqualTo(object val) => new FilterExpression(Name, "!=", val);
        public WhereExpression IsGreatherThan(object val) => new FilterExpression(Name, ">", val);
        public WhereExpression IsGreaterOrEqualTo(object val) => new FilterExpression(Name, ">=", val);
        public WhereExpression IsLessThan(object val) => new FilterExpression(Name, "<", val);
        public WhereExpression IsLessOrEqualTo(object val) => new FilterExpression(Name, "<=", val);
        public WhereExpression IsLike(object val) => new FilterExpression(Name, "LIKE", val);
        public WhereExpression IsNull() => new PlainTextExpression(Name, "IS NULL");
        public WhereExpression IsNotNull() => new PlainTextExpression(Name, "IS NOT NULL");
        public WhereExpression IsIn<T>(IEnumerable<T> values) => new FilterExpression(Name, "IN", values.Cast<object>());
    }
}