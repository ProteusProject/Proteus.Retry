#region License

/*
 * Copyright © 2014-2016 the original author or authors.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

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