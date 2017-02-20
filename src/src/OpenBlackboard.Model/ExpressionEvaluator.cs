using System;
using System.Collections.Generic;
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
            _parameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public void AddConstant(string name, object value)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(name));
            Debug.Assert(_parameters != null);

            _parameters.Add(name, value);
        }

        public bool Evaluate(Expression<Func<string>> expressionSelector, out object result)
        {
            Debug.Assert(_dataset?.Issues != null);
            Debug.Assert(expressionSelector != null);

            _descriptor = new Lazy<ValueDescriptor>(() => Helpers.GetDescriptorFromPropertyAccess(expressionSelector));
            var expression = expressionSelector.Compile()();

            try
            {
                _currentExpression = CreateExpression(expression);
                result = _currentExpression.Evaluate();

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

        public object Evaluate(ValueDescriptor descriptor, string expression)
        {
            Debug.Assert(descriptor != null);

            _descriptor = new Lazy<ValueDescriptor>(() => descriptor);
            _currentExpression = CreateExpression(expression);
            return _currentExpression.Evaluate();
        }

        public bool Evaluate<T>(Expression<Func<string>> expressionSelector, out T result)
        {
            Debug.Assert(_dataset.Culture != null);

            object unconvertedResult;
            if (Evaluate(expressionSelector, out unconvertedResult))
            {
                result = (T)Convert.ChangeType(unconvertedResult, typeof(T), _dataset.Culture);
                return true;
            }

            result = default(T);
            return false;
        }

        public object Evaluate(Expression<Func<string>> expressionSelector)
        {
            object result;
            if (Evaluate(expressionSelector, out result))
                return result;

            return null;
        }

        internal const string IdentifierValue = "value";
        internal const string IdentifierValues = "values";
        private const string IdentifierThis = "this";
        private const string FunctionLastOf = "sequence";
        private const string FunctionLet = "let";
        private const string FunctionCount = "count";
        private const string FunctionSum = "sum";
        private const string FunctionAverage = "average";
        private const string FunctionIsNull = "isnull";
        private const string ConstantNull = "null";
        private const string IdentifierRequired = "required";
        private const char OperatorNullCoalesce = '?';

        private readonly DataSet _dataset;
        private readonly Dictionary<string, object> _parameters;
        private Lazy<ValueDescriptor> _descriptor; // Warning: this shared field makes this class not thread-safe (regardless DataSet behavior...)
        private NCalc.Expression _currentExpression;

        private NCalc.Expression CreateExpression(string expression)
        {
            var expr = new NCalc.Expression(expression.Replace("\n", " ").Replace("\r", ""), NCalc.EvaluateOptions.IgnoreCase);
            expr.EvaluateFunction += EvaluateFunction;
            expr.EvaluateParameter += EvaluateParameter;
            expr.Parameters.Add(ConstantNull, null);

            foreach (var parameter in _parameters)
                expr.Parameters.Add(parameter.Key, parameter.Value);

            return expr;
        }

        private void EvaluateFunction(string name, NCalc.FunctionArgs args)
        {
            if (String.Equals(name, FunctionIsNull, StringComparison.OrdinalIgnoreCase))
            {
                args.HasResult = true;
                args.Result = args.EvaluateParameters().Any(x => x == null);
            }
            else if (String.Equals(name, FunctionCount, StringComparison.OrdinalIgnoreCase))
            {
                args.HasResult = true;
                args.Result = AggregationFunctions.Count(_descriptor.Value, _dataset.Culture, AggregationFunctions.Unpack(args.EvaluateParameters()));
            }
            else if (String.Equals(name, FunctionAverage, StringComparison.OrdinalIgnoreCase))
            {
                args.HasResult = true;
                args.Result = AggregationFunctions.Average(_descriptor.Value, _dataset.Culture, AggregationFunctions.Unpack(args.EvaluateParameters()));
            }
            else if (String.Equals(name, FunctionLet, StringComparison.OrdinalIgnoreCase))
            {
                if (args.Parameters.Length != 2)
                    throw new ArgumentException($"Invalid number of arguments for {FunctionLet}().");

                var result = args.Parameters[1].Evaluate();
                _currentExpression.Parameters.Add(Convert.ToString(args.Parameters[0].Evaluate()), result);

                args.HasResult = true;
                args.Result = result;
            }
            else if (String.Equals(name, FunctionLastOf, StringComparison.OrdinalIgnoreCase))
            {
                args.HasResult = true;
                args.Result = args.EvaluateParameters().LastOrDefault();
            }
            else if (String.Equals(name, FunctionSum, StringComparison.OrdinalIgnoreCase))
            {
                if (args.Parameters.Length == 0 || args.Parameters.Length > 2)
                    throw new ArgumentException($"Invalid number of arguments for {FunctionSum}().");

                var set = (IEnumerable<object>)args.Parameters[0].Evaluate();
                if (args.Parameters.Length == 2)
                    set = AggregationFunctions.Project(_descriptor.Value, _dataset.Culture, set, args.Parameters[1]);

                args.HasResult = true;
                args.Result = AggregationFunctions.Sum(_descriptor.Value, _dataset.Culture, set);
            }
        }

        private ExpressionEvaluator CloneThisEvaluatorForNewExpression()
        {
            var evaluator = new ExpressionEvaluator(_dataset);

            foreach (var parameter in _parameters)
                evaluator._parameters.Add(parameter.Key, parameter.Value);

            return evaluator;
        }

        private void EvaluateParameter(string name, NCalc.ParameterArgs args)
        {
            if (_dataset == null)
                return;

            Debug.Assert(_dataset.Values != null);

            var referenceName = name;

            // "this" is an alias for the currently evaluated descriptor (if any)
            if (_descriptor != null && String.Equals(referenceName, IdentifierThis, StringComparison.OrdinalIgnoreCase))
                referenceName = _descriptor.Value.Reference;

            // "required" means that a value for current descriptor must be specified (or calculated with defaults)
            if (_descriptor != null && String.Equals(referenceName, IdentifierRequired, StringComparison.OrdinalIgnoreCase))
            {
                DataSetValue dummy;
                args.HasResult = true;
                args.Result = _dataset.Values.TryGetValue(_descriptor.Value.Reference, out dummy);

                return;
            }

            // Ending a parameter name with "?" returns null if it doesn't exist
            if (referenceName.EndsWith(OperatorNullCoalesce.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                referenceName.TrimEnd(OperatorNullCoalesce);
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
