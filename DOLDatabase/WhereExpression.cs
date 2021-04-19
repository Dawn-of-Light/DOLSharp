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
        private static string alphabet = "abcdefghijklmnopqrstuvwxyz";

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

        public virtual string WhereClause
        {
            get
            {
                var clause = "";
                uint parameterID = 0;
                foreach (var atom in IntermediateRepresentation)
                {
                    if (atom.IsParameterized)
                    {
                        clause += GetPlaceHolder(parameterID);
                        parameterID++;
                    }
                    else
                    {
                        clause += atom.Val;
                    }
                    clause += " ";
                }
                return clause.Trim();
            }
        }

        public virtual QueryParameter[] QueryParameters
        {
            get
            {
                var result = new List<QueryParameter>();
                uint parameterID = 0;
                foreach (var atom in IntermediateRepresentation)
                {
                    if (atom.IsParameterized)
                    {
                        result.Add(new QueryParameter(GetPlaceHolder(parameterID), atom.Val));
                        parameterID++;
                    }
                }
                return result.ToArray();
            }
        }

        internal abstract List<TextAtom> IntermediateRepresentation { get; }

        public virtual WhereExpression And(WhereExpression rightExpression)
            => rightExpression.Equals(Empty) ? this : new ChainingExpression(this, "AND", rightExpression);
        public virtual WhereExpression Or(WhereExpression rightExpression) 
            => rightExpression.Equals(Empty) ? this : new ChainingExpression(this, "OR", rightExpression);

        public static WhereExpression Empty => new EmptyWhereExpression();

        public override int GetHashCode() => base.GetHashCode();
    }

    internal class TextAtom
    {
        public object Val { get; }

        public TextAtom(object val) { Val = val; }

        public virtual bool IsParameterized => false;
    }

    internal class ValueAtom : TextAtom
    {
        public ValueAtom(object val) : base(val) { }

        public override bool IsParameterized => true;
    }

    internal class EmptyWhereExpression : WhereExpression
    {
        internal override List<TextAtom> IntermediateRepresentation => new List<TextAtom>();

        public override WhereExpression And(WhereExpression rightExpression) => rightExpression;
        public override WhereExpression Or(WhereExpression rightExpression) => rightExpression;

        public override bool Equals(object obj) => obj is EmptyWhereExpression;
        public override int GetHashCode() => base.GetHashCode();
    }

    internal class FilterExpression : WhereExpression
    {
        private string columnName;
        private string op;
        private object val;

        internal FilterExpression(string columnName, string op, object val)
        {
            this.columnName = columnName;
            this.op = op;
            this.val = val;
        }

        internal override List<TextAtom> IntermediateRepresentation
        {
            get
            {
                if (val is IEnumerable<object> valueCollection && valueCollection.Any())
                {
                    var result = new List<TextAtom>() { new TextAtom(columnName), new TextAtom(op), new TextAtom("(") };
                    result.Add(new ValueAtom(valueCollection.ElementAt(0)));
                    foreach(var element in valueCollection.Skip(1))
                    {
                        result.Add(new TextAtom(","));
                        result.Add(new ValueAtom(element));
                    }
                    result.Add(new TextAtom(")"));
                    return result;
                }
                return new List<TextAtom>() { new TextAtom(columnName), new TextAtom(op), new ValueAtom(val) };
            }
        }

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

        internal override List<TextAtom> IntermediateRepresentation
            => new List<TextAtom>() { new TextAtom(columnName), new TextAtom(op) };

        public override bool Equals(object obj)
        {
            if (obj is PlainTextExpression expression)
            {
                return expression.op.Equals(op)
                    && expression.columnName.Equals(columnName);
            }
            return false;
        }

        public override int GetHashCode() => base.GetHashCode();
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

        internal override List<TextAtom> IntermediateRepresentation
        {
            get
            {
                if (right.Equals(Empty)) return left.IntermediateRepresentation;

                var result = new List<TextAtom>() { new TextAtom("(") };
                result.AddRange(left.IntermediateRepresentation);
                result.Add(new TextAtom(chainingOperator));
                result.AddRange(right.IntermediateRepresentation);
                result.Add(new TextAtom(")"));
                return result;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is ChainingExpression expr)
            {
                return expr.left.Equals(left)
                    && expr.chainingOperator.Equals(chainingOperator)
                    && expr.right.Equals(right);
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
