using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace OpenBlackboard.Model
{
    /// <summary>
    /// Represents a set submitted values (i.e. a <see cref="ProtocolDescriptor"/> with all its
    /// <see cref="ValueDescriptor"/> and all related values).
    /// </summary>
    public sealed class DataSet : IEnumerable<DataSetValue>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DataSet"/> for the specified
        /// protocol.
        /// </summary>
        /// <param name="protocol">
        /// The protocol for which data are submitted. Structure and content of this protocol must not be
        /// changed after the creation of this <see cref="DataSet"/> object.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="protocol"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// Values with a default are calculated immediately after this object has been constructed
        /// and they're available for inspection. For this reason default values always use invariant culture
        /// if a conversion is required (see <see cref="Culture"/>).
        /// </remarks>
        public DataSet(ProtocolDescriptor protocol)
            : this(protocol, true)
        {
        }

        internal DataSet(ProtocolDescriptor protocol, bool populateWithInitialValues)
        {
            if (protocol == null)
                throw new ArgumentNullException(nameof(protocol));

            Issues = new DataErrorCollection();
            Protocol = protocol;
            _culture = CultureInfo.InvariantCulture;
            _evaluator = new ExpressionEvaluator(this);

            // At this stage model error aren't allowed and this dataset is not usable
            Issues.AddModelErrors(protocol.ValidateModel());
            if (Issues.Any())
            {
                _cachedDescriptorsLookUp = new Dictionary<string, ValueDescriptor>();
                Values = new Dictionary<string, DataSetValue>();
            }
            else
            {
                _cachedDescriptorsLookUp = protocol.Sections.VisitAllValues()
                    .Where(x => !String.IsNullOrWhiteSpace(x.Reference))
                    .ToDictionary(x => x.Reference, StringComparer.OrdinalIgnoreCase);

                Values = new Dictionary<string, DataSetValue>(_cachedDescriptorsLookUp.Count, StringComparer.OrdinalIgnoreCase);

                // It's done here instead of in Calculate() because external values (included with Add()) may
                // overwrite them but we don't want to overwrite external values with their defaults.
                if (populateWithInitialValues)
                    PopulateInitialValues();
            }
        }
        
        /// <summary>
        /// Gets the protocol that contains the definition of submitted values.
        /// </summary>
        public ProtocolDescriptor Protocol { get; }

        /// <summary>
        /// Gets the value for field with specified reference ID.
        /// </summary>
        /// <param name="reference">Reference ID for the field to read.</param>
        /// <returns>The actual value for the specified field.</returns>
        /// <exception cref="KeyNotFoundException">
        /// If there is not any value for the field with specified reference ID.
        /// </exception>
        public DataSetValue this[string reference]
        {
            get
            {
                Debug.Assert(Values != null);
                Debug.Assert(!String.IsNullOrWhiteSpace(reference));

                return Values[reference];
            }
        }

        /// <summary>
        /// Gets/sets the culture used to format <see cref="DataSetValue.Text"/>.
        /// </summary>
        /// <value>
        /// When a conversion is required for <see cref="DataSetValue.Value"/> it is performed
        /// using this culture. Default value is <see cref="CultureInfo.InvariantCulture"/>.
        /// </value>
        /// <remarks>
        /// Usually it's not necessary to change this property, input values are already in the required format
        /// and conversions are necessary only for expressions (which are usually written using invariant culture).
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// If the specified value is <see langword="null"/>.
        /// </exception>
        public CultureInfo Culture
        {
            get { return _culture; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();

                _culture = value;
            }
        }

        /// <summary>
        /// Gets the list of issues found with values in this dataset.
        /// </summary>
        /// <value>
        /// The list of issues found with values in this dataset. Before you call
        /// <see cref="Calculate"/> this collection contains only model errors (which must not
        /// be ignored); after all the values have been calculated then it contains all the issues
        /// including warnings and validation errors. Note that calling <c>Calculate()</c> does not clear
        /// the previous list of errors, if you're supposed to call <c>Calculate()</c> multiple times you should
        /// also call <c>Issues.Clear()</c>.
        /// </value>
        public DataErrorCollection Issues { get; }

        /// <overload>
        /// Adds a new value to this dataset.
        /// </overload>
        /// <summary>
        /// Adds a new value to this dataset associated with a specified <see cref="ValueDescriptor"/> by reference ID.
        /// </summary>
        /// <param name="valueDescriptorReference">Reference ID for the descriptor of this value.</param>
        /// <param name="value">
        /// Value to add to the dataset, when required it will be converted to the proper data type mandated
        /// by its descriptor (eventually using <see cref="Culture"/> for the conversion).
        /// </param>
        /// <returns>The newly added value.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="valueDescriptorReference"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="valueDescriptorReference"/> is an empty or blank string.
        /// <br/>-or-<br/>
        /// If <paramref name="valueDescriptorReference"/> is not a known reference ID for the
        /// protocol <see cref="Protocol"/>.
        /// </exception>
        public DataSetValue AddValue(string valueDescriptorReference, object value)
        {
            Debug.Assert(_cachedDescriptorsLookUp != null);

            if (valueDescriptorReference == null)
                throw new ArgumentNullException(nameof(valueDescriptorReference));

            if (String.IsNullOrWhiteSpace(valueDescriptorReference))
                throw new ArgumentException("Cannot add a value for an unnamed descriptor.", nameof(valueDescriptorReference));

            if (!_cachedDescriptorsLookUp.ContainsKey(valueDescriptorReference))
                throw new ArgumentException("Unknown reference ID.", nameof(valueDescriptorReference));

            return AddValueCore(_cachedDescriptorsLookUp[valueDescriptorReference], value);
        }

        /// <summary>
        /// Adds a new value to this dataset associated with the specified <see cref="ValueDescriptor"/>.
        /// </summary>
        /// <param name="descriptor">Reference ID for the descriptor of this value.</param>
        /// <param name="value">
        /// Value to add to the dataset, when required it will be converted to the proper data type mandated
        /// by its descriptor (eventually using <see cref="Culture"/> for the conversion).
        /// </param>
        /// <returns>The newly added value.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="descriptor"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="descriptor"/> does not belong to <see cref="Protocol"/>.
        /// </exception>
        public DataSetValue AddValue(ValueDescriptor descriptor, object value)
        {
            Debug.Assert(_cachedDescriptorsLookUp != null);

            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            if (!_cachedDescriptorsLookUp.ContainsKey(descriptor.Reference))
                throw new ArgumentException("Specified descriptor does not belong to the protocol associated with this object.", nameof(descriptor));

            return AddValueCore(descriptor, value);
        }

        /// <summary>
        /// Validate all accumulated values and evaluate calculated values.
        /// </summary>
        /// <remarks>
        /// After calling this method all errors (usually validation errors but eventually also model errors)
        /// and warnings are stored in the <see cref="Issues"/> collection. If you call this method multiple times
        /// you should clear the collection before each call.
        /// </remarks>
        public void Calculate()
        {
            // We perform a two steps validation, first using only external values and values with default; if validation
            // fails here then we do not need to calculate anything else (because missing or invalid values will cause
            // also calculated values to fail).
            var notCalculatedValues = FindDescriptorsWithoutExpression(x => x.CalculatedValueExpression);
            CheckRules(checkForWarnings: true, descriptors: notCalculatedValues);

            if (!CheckRules(checkForWarnings: false, descriptors: notCalculatedValues))
                return;

            // Then, if we passed basic validation we can validate calculated values.
            var calculatedValues = PopulateCalculatedValues();
            CheckRules(checkForWarnings: true, descriptors: calculatedValues);
            CheckRules(checkForWarnings: false, descriptors: calculatedValues);

            IsChangedAfterLastCalculation = false;
        }

        /// <summary>
        /// Enumerates all the stored values.
        /// </summary>
        /// <returns>An enumeratore to iterate through all stored values.</returns>
        public IEnumerator<DataSetValue> GetEnumerator()
        {
            Debug.Assert(Values != null);

            return Values.Values.GetEnumerator();
        }

        /// <summary>
        /// Enumerates all the stored values.
        /// </summary>
        /// <returns>An enumeratore to iterate through all stored values.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            Debug.Assert(Values != null);

            return ((System.Collections.IEnumerable)Values.Values).GetEnumerator();
        }

        internal Dictionary<string, DataSetValue> Values
        {
            get;
        }

        internal bool IsChangedAfterLastCalculation
        {
            get;
            private set;
        }

        private CultureInfo _culture;
        private readonly ExpressionEvaluator _evaluator;
        private readonly Dictionary<string, ValueDescriptor> _cachedDescriptorsLookUp;

        private DataSetValue AddValueCore(ValueDescriptor descriptor, object value)
        {
            Debug.Assert(Values != null);
            Debug.Assert(descriptor != null && !String.IsNullOrWhiteSpace(descriptor.Reference));

            // TryGetValue() approach is faster but it's easier to keep DataSetValue immutable
            // because we do not need to track external changes and we just use IsChangedAfterLastCalculation.
            Values.Remove(descriptor.Reference);

            var item = new DataSetValue(descriptor, value);
            Values.Add(descriptor.Reference, item);

            IsChangedAfterLastCalculation = true;

            return item;
        }

        private void PopulateInitialValues()
        {
            foreach (var descriptor in FindDescriptorsWithExpression(x => x.DefaultValueExpression))
            {
                Debug.Assert(descriptor != null);

                object value;
                if (_evaluator.Evaluate(() => descriptor.DefaultValueExpression, out value))
                    AddValueCore(descriptor, value);
            }
        }

        private IEnumerable<ValueDescriptor> PopulateCalculatedValues()
        {
            var descriptors = FindDescriptorsWithExpression(x => x.CalculatedValueExpression).Where(IsEnabled);
            foreach (var descriptor in descriptors)
            {
                Debug.Assert(descriptor != null);

                object calculatedValue;
                if (!_evaluator.Evaluate(() => descriptor.CalculatedValueExpression, out calculatedValue))
                    continue;

                AddValueCore(descriptor, calculatedValue);
            }

            return descriptors;
        }

        private bool CheckRules(bool checkForWarnings, IEnumerable<ValueDescriptor> descriptors)
        {
            Debug.Assert(Values != null);
            Debug.Assert(Issues != null);
            Debug.Assert(descriptors != null);

            // TODO: refactor this, it's not clear at all and there is too
            // much almost duplicated code. Separate class?
            int issueCount = 0;
            foreach (var descriptor in descriptors)
            {
                if (checkForWarnings)
                {
                    if (String.IsNullOrWhiteSpace(descriptor.WarningIfExpression))
                        continue;

                    bool hasWarning;
                    if (!_evaluator.Evaluate(() => descriptor.WarningIfExpression, out hasWarning))
                        continue;

                    if (!hasWarning)
                        continue;
                }
                else
                {
                    if (String.IsNullOrWhiteSpace(descriptor.ValidIfExpression))
                        continue;

                    bool isValid;
                    if (!_evaluator.Evaluate(() => descriptor.ValidIfExpression, out isValid))
                        continue;

                    if (isValid)
                        continue;
                }

                DataSetValue item;
                object itemValue = null;

                if (Values.TryGetValue(descriptor.Reference, out item))
                    itemValue = item.Value;

                // CHECK: message is not localized assuming that for every validation rule there will be
                // a clear ad-hoc description (and this is just for debugging purposes). If it will be not the case
                // then move this text to resources.
                string message = checkForWarnings ? descriptor.WarningMessage : descriptor.ValidationMessage;
                if (String.IsNullOrWhiteSpace(message))
                    message = $"Value '{itemValue}' for '{descriptor.Reference}' is not valid.";

                if (checkForWarnings)
                    Issues.AddWarning(descriptor, message);
                else
                    Issues.AddValidationError(descriptor, message);

                ++issueCount;
            }

            return issueCount == 0;
        }

        private IEnumerable<ValueDescriptor> FindDescriptorsWithExpression(Func<ValueDescriptor, string> selector)
        {
            Debug.Assert(_cachedDescriptorsLookUp != null);
            Debug.Assert(selector != null);

            return _cachedDescriptorsLookUp.Values.Where(x => !String.IsNullOrWhiteSpace(selector(x)));
        }

        private IEnumerable<ValueDescriptor> FindDescriptorsWithoutExpression(Func<ValueDescriptor, string> selector)
        {
            Debug.Assert(_cachedDescriptorsLookUp != null);
            Debug.Assert(selector != null);

            return _cachedDescriptorsLookUp.Values.Where(x => String.IsNullOrWhiteSpace(selector(x)));
        }

        private bool IsEnabled(ValueDescriptor descriptor)
        {
            Debug.Assert(descriptor != null);

            if (String.IsNullOrWhiteSpace(descriptor.EnabledIfExpression))
                return true;

            bool enabled;
            if (_evaluator.Evaluate(() => descriptor.EnabledIfExpression, out enabled))
                return enabled;

            return false;
        }
    }
}
