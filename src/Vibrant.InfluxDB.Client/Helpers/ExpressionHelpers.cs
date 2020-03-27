using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Vibrant.InfluxDB.Client.Helpers
{
    public static class ExpressionHelpers
    {
        // courtesy of: Trent Raymond (https://stackoverflow.com/a/49695423)
        
        /// <summary>
        ///     Gets the corresponding <see cref="PropertyInfo" /> from an <see cref="Expression" />.
        /// </summary>
        /// <param name="property">The expression that selects the property to get info on.</param>
        /// <returns>The property info collected from the expression.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="property" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The expression doesn't indicate a valid property."</exception>
        public static PropertyInfo GetPropertyInfo<T>(Expression<Func<T, object>> property)
        {
            if (property == null) {
                throw new ArgumentNullException(nameof(property));
            }

            if (property.Body is UnaryExpression unaryExp) {
                if (unaryExp.Operand is MemberExpression memberExp) {
                    return (PropertyInfo)memberExp.Member;
                }
            }
            else if (property.Body is MemberExpression memberExp) {
                return (PropertyInfo)memberExp.Member;
            }

            throw new ArgumentException($"The expression doesn't indicate a valid property [{property}]");
        }
    }
}