using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CPP.Framework;

// ReSharper disable once CheckNamespace
namespace System.Linq.Expressions
{
    /// <summary>
    /// Provides extension methods for the system <see cref="Expression"/> and 
    /// <see cref="Expression{TDelegate}"/> classes.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ExpressionExtensions
    {
        /// <summary>
        /// Gets the <see cref="MemberInfo"/> object for the member referenced by a member-access
        /// expression.
        /// </summary>
        /// <param name="expression">The expression to evaluate.</param>
        /// <returns>A <see cref="MemberInfo"/> instance.</returns>
        /// <exception cref="ArgumentException"><paramref name="expression"/> is not a valid member-access expression.</exception>
        /// <exception cref="NullReferenceException"><paramref name="expression"/> is a null reference.</exception>
        public static MemberInfo GetMemberInfo(this Expression expression)
        {
            ArgumentValidator.ValidateThisObj(() => expression);
            var target = expression;

            MemberInfo memberInfo = null;
            do
            {
                switch (target.NodeType)
                {
                    case ExpressionType.Lambda:
                        {
                            target = ((LambdaExpression)target).Body;
                        }
                        break;
                    case ExpressionType.Call:
                        {
                            memberInfo = ((MethodCallExpression)target).Method;
                        }
                        break;
                    case ExpressionType.MemberAccess:
                        {
                            memberInfo = ((MemberExpression)target).Member;
                        }
                        break;
                    case ExpressionType.Convert:
                    case ExpressionType.ConvertChecked:
                        {
                            target = ((UnaryExpression)target).Operand;
                        }
                        break;
                    default:
                        {
                            target = null;
                        }
                        break;
                }
            }
            while ((memberInfo == null) && (target != null));

            if (memberInfo == null)
            {
                throw ArgumentValidator.CreateArgumentExceptionFor(
                    () => expression,
                    ErrorStrings.InvalidMemberAccessExpression);
            }
            return memberInfo;
        }

        /// <summary>
        /// Gets the name of member referenced by a lambda expression.
        /// </summary>
        /// <param name="expression">The expression to evaluate.</param>
        /// <returns>The name of the member referenced by <paramref name="expression"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="expression"/> is not a valid member-access expression.</exception>
        /// <exception cref="NullReferenceException"><paramref name="expression"/> is a null reference.</exception>
        public static string GetMemberName(this Expression expression)
        {
            return expression.GetMemberInfo().Name;
        }

        /// <summary>
        /// Gets the <see cref="MethodInfo"/> object for the member referenced by an expression.
        /// </summary>
        /// <param name="expression">The expression to evaluate.</param>
        /// <returns>A <see cref="MemberInfo"/> instance.</returns>
        /// <exception cref="ArgumentException"><paramref name="expression"/> is not a valid method call expression.</exception>
        /// <exception cref="NullReferenceException"><paramref name="expression"/> is a null reference.</exception>
        public static MethodInfo GetMethodInfo(this Expression expression)
        {
            var methodInfo = (expression.GetMemberInfo() as MethodInfo);
            if (methodInfo != null) return methodInfo;
            throw ArgumentValidator.CreateArgumentExceptionFor(() => expression, ErrorStrings.InvalidMethodCallExpression);
        }
    }
}
