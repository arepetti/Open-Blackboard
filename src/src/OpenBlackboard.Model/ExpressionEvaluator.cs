using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace OpenBlackboard.Model
{
    sealed class ExpressionEvaluator
    {
        public ExpressionEvaluator(DataSet dataset)
        {
            Debug.Assert(dataset != null);

            _dataset = dataset;
        }

        public bool Evaluate(Expression<Func<string>> expressionSelector, out object result)
        {
            Debug.Assert(_dataset?.Issues != null);
            Debug.Assert(expressionSelector != null);

            _descriptor = new Lazy<ValueDescriptor>(() => Helpers.GetDescriptorFromPropertyAccess(expressionSelector));
            var expression = expressionSelector.Compile()();

            try
            {
                result = CreateExpression(expression).Evaluate();

                return true;
            }
            catch (ArgumentException e) // Unknown parameter
            {
                // We report this error as IssueSeverity.ModelError but it might be both
                // a mistake in expression code or some missing input data. See CreateExpression()
                // for implementation (we may also check if descriptor exists and throw a different exception).
                _dataset.Issues.AddModelError(_descriptor.Value, e.Message);
            }
            catch (NCalc.EvaluationException e) // Syntax error
            {
                _dataset.Issues.AddModelError(Helpers.GetDescriptorFromPropertyAccess(expressionSelector), e.Message);
            }

            result = null;
            return false;
        }

        public bool Evaluate<T>(Expression<Func<string>> expressionSelector, out T result)
        {
            Debug.Assert(_dataset != null);

            object unconvertedResult;
            if (Evaluate(expressionSelector, out unconvertedResult))
            {
                result = (T)Convert.ChangeType(unconvertedResult, typeof(T), _dataset.Culture);
                return true;
            }

            result = default(T);
            return false;
        }

        private readonly DataSet _dataset;
        private Lazy<ValueDescriptor> _descriptor; // Warning: this shared field makes this class not thread-safe (regardless DataSet behavior...)

        private NCalc.Expression CreateExpression(string expression)
        {
            var expr = new NCalc.Expression(expression, NCalc.EvaluateOptions.IgnoreCase);
            expr.EvaluateFunction += EvaluateFunction;
            expr.EvaluateParameter += EvaluateParameter;

            return expr;
        }

        private static void EvaluateFunction(string name, NCalc.FunctionArgs args)
        {
            if (String.Equals(name, "isnull", StringComparison.OrdinalIgnoreCase))
            {
                args.HasResult = true;
                args.Result = args.EvaluateParameters().All(x => x == null);
            }
        }

        private void EvaluateParameter(string name, NCalc.ParameterArgs args)
        {
            Debug.Assert(_dataset?.Values != null);

            var referenceName = name;

            // "this" is an alias for the currently evaluated descriptor (if any)
            if (_descriptor != null && String.Equals(referenceName, "this", StringComparison.OrdinalIgnoreCase))
                referenceName = _descriptor.Value.Reference;

            // "required" means that a value for current descriptor must be specified (or calculated with defaults)
            if (_descriptor != null && String.Equals(referenceName, "required", StringComparison.OrdinalIgnoreCase))
            {
                DataSetValue dummy;
                args.HasResult = true;
                args.Result = _dataset.Values.TryGetValue(_descriptor.Value.Reference, out dummy);

                return;
            }

            // Ending a parameter name with "?" returns null if it doesn't exist
            if (referenceName.EndsWith("?", StringComparison.OrdinalIgnoreCase))
            {
                referenceName.TrimEnd('?');
                args.HasResult = true;
                args.Result = null;
            }

            DataSetValue item;
            if (_dataset.Values.TryGetValue(referenceName, out item))
            {
                args.HasResult = true;
                args.Result = item.Value;
            }
        }
    }
}
