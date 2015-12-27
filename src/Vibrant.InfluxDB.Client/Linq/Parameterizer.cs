using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;

namespace Vibrant.InfluxDB.Client.Linq
{
    /// <summary>
    /// Converts constants into parameters
    /// </summary>
    public class Parameterizer : ExpressionVisitor
    {
        private readonly Func<Expression, bool> fnCanBeParameter;
        private readonly List<ParameterExpression> parameters;
        private readonly List<object> values;

        private Parameterizer(Func<Expression, bool> fnCanBeParameter)
        {
            this.fnCanBeParameter = fnCanBeParameter;
            this.parameters = new List<ParameterExpression>();
            this.values = new List<object>();
        }

        public static Expression Parameterize(Expression expression, Func<Expression, bool> fnCanBeParameter, out List<ParameterExpression> parameters, out List<object> values)
        {
            var p = new Parameterizer(fnCanBeParameter);
            var result = p.Visit(expression);
            parameters = p.parameters;
            values = p.values;
            return result;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            bool isQueryRoot = c.Value is IQueryable;

            if (!isQueryRoot && !this.fnCanBeParameter(c))
                return c;

            var p = Expression.Parameter(c.Type, "p" + parameters.Count);
            parameters.Add(p);
            values.Add(c.Value);

            // If query root then parameterize it so we pass the value to the compiled query,
            // but don't replace in the tree so it won't try to map this parameter to actual SQL.
            if (isQueryRoot)
                return c;

            return p;
        }
    }
}

