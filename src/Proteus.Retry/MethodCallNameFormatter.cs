using System;
using System.Linq;
using System.Linq.Expressions;

namespace Proteus.Retry
{
    /// <summary>
    /// Class MethodCallNameFormatter.
    /// </summary>
    public class MethodCallNameFormatter
    {
        /// <summary>
        /// Gets the formatted name of the expression.
        /// </summary>
        /// <typeparam name="TExpression">The type of the t expression.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns>System.String.</returns>
        public static string GetFormattedName<TExpression>(Expression<TExpression> expression)
        {
            var methodName = expression.ToString();

            var methodCall = expression.Body as MethodCallExpression;

            if (methodCall?.Method.DeclaringType != null)
            {
                methodName = GetFullTypeName(methodCall) + "." + GetMethodName(methodCall) + GetArguments(methodCall);
            }

            return methodName;
        }

        private static string GetMethodName(MethodCallExpression methodCall)
        {
            return methodCall.Method.Name;
        }

        private static string GetFullTypeName(MethodCallExpression methodCall)
        {
            return methodCall.Method.DeclaringType?.FullName ?? "DYNAMIC_TYPE";
        }

        private static string GetArguments(MethodCallExpression methodCall)
        {
            return "(" + string.Join(", ", methodCall.Arguments) + ")";
        }
    }
}