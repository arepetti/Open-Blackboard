using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBlackboard.Model
{
    static class AggregationFunctions
    {
        public static IEnumerable<object> Unpack(IEnumerable<object> values)
        {
            foreach (var value in values)
            {
                var enumerable = value as IEnumerable<object>;
                if (enumerable == null)
                    yield return value;
                else
                {
                    foreach (var innerValue in Unpack(enumerable))
                        yield return innerValue;
                }
            }
        }

        public static object Count(ValueDescriptor descriptor, CultureInfo culture, IEnumerable<object> values)
        {
            return values.Count();
        }

        public static object Sum(ValueDescriptor descriptor, CultureInfo culture, IEnumerable<object> values)
        {
            return ToNumbers(descriptor, culture, values).Sum();
        }

        public static object Average(ValueDescriptor descriptor, CultureInfo culture, IEnumerable<object> values)
        {
            var numbers = ToNumbers(descriptor, culture, values);
            if (numbers.Any())
                return numbers.Sum() / numbers.Count();

            return null;
        }

        public static IEnumerable<object> Project(ValueDescriptor descriptor, CultureInfo culture, IEnumerable<object> values, NCalc.Expression expression)
        {
            return values.Select(x =>
            {
                expression.Parameters[ExpressionEvaluator.IdentifierValue] = x;
                return expression.Evaluate();
            });
        }

        private static IEnumerable<double> ToNumbers(ValueDescriptor descriptor, CultureInfo culture, IEnumerable<object> values)
        {
            return values
                .Select(x => ValueConversions.ToNumber(descriptor, culture, x))
                .Where(x => x.HasValue)
                .Select(x => x.Value);
        }
    }
}
