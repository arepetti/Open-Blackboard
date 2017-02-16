using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace OpenBlackboard.Model
{
    static class Helpers
    {
        public static ValueDescriptor GetDescriptorFromPropertyAccess(Expression<Func<string>> expressionSelector)
        {
            Debug.Assert(expressionSelector != null);
            Debug.Assert(!expressionSelector.CanReduce);
            Debug.Assert(expressionSelector.NodeType == ExpressionType.Lambda);
            Debug.Assert(expressionSelector.Body.NodeType == ExpressionType.MemberAccess);

            // It works only for expressions in the form
            // () => descriptorInstance.PropertyToGet

            var propertyGetExpression = (MemberExpression)expressionSelector.Body;
            var fieldAccessOnClosureExpression = (MemberExpression)propertyGetExpression.Expression;
            var closureExpression = (ConstantExpression)fieldAccessOnClosureExpression.Expression;
            var closuerInstance = closureExpression.Value;
            var closureFieldInfo = (FieldInfo)fieldAccessOnClosureExpression.Member;

            return (ValueDescriptor)closureFieldInfo.GetValue(closuerInstance);
        }
    }
}
