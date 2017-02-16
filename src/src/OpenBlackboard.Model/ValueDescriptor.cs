using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;

namespace OpenBlackboard.Model
{
    /// <summary>
    /// Represents the description of a single acquired field which may
    /// submitted as <see cref="DataSetValue"/>.
    /// </summary>
    [DebuggerDisplay("{Reference} : {Type}")]
    public sealed class ValueDescriptor : NamedItemDescriptor, IReferenceable
    {
        /// <summary>
        /// Gets/sets the unique ID used to identify this value within all the other
        /// values of the same protocol.
        /// </summary>
        /// <value>
        /// The unique ID used to identify this value within all the other
        /// values of the same protocol. It is also used in expressions as <em>reference</em>
        /// to the value of this field. Default value is an empty string <see cref="String.Empty"/>
        /// and should be used only for calculated fields (<see cref="CalculatedValueExpression"/>).
        /// </value>
        public string Reference { get; set; } = "";

        /// <summary>
        /// Gets/sets the type of content stored in <see cref="DataSetValue"/>. 
        /// </summary>
        /// <value>
        /// The type of content stored in <see cref="DataSetValue"/> described by this
        /// <see cref="ValueDescriptor"/>. Default value is <see cref="TypeOfValue.Double"/>.
        /// Note that <see cref="TypeOfValue"/> is shomehow a superset of possible types
        /// supported by <see cref="DataSetValue"/> then a conversion should be performed when saving/loading
        /// values for display purposes.
        /// </value>
        [DefaultValue(typeof(TypeOfValue), nameof(TypeOfValue.Double))]
        public TypeOfValue Type { get; set; } = TypeOfValue.Double;

        /// <summary>
        /// Gets the list of possible values for this field.
        /// </summary>
        /// <value>
        /// The list of possible values for this field. If the list is not empty then, for editable fields, only
        /// values from this list can be used. This collection must be empty for calculated values.
        /// </value>
        public ListItemCollection AvailableValues { get; set; } = new ListItemCollection();

        /// <summary>
        /// Gets/sets the default value expression for this field.
        /// </summary>
        /// <value>
        /// An expression used to calculated the default value for this field. Default value
        /// of this property is <see cref="String.Empty"/>. This property cannot be used if
        /// the field is not editable (i.e. it's a calculated field with <see cref="CalculatedValueExpression"/>).
        /// </value>
        [DefaultValue(""), JsonProperty("DefaultValue")]
        public string DefaultValueExpression { get; set; } = "";

        /// <summary>
        /// Gets/sets the expression to calculate the value of this field.
        /// </summary>
        /// <value>
        /// The expression used to calculate the value of this field. If an expression is provided
        /// then value isn't editable (user or external sources may store a value) but calculated according
        /// to this expression. Note that calculated value are not save in underlying storage: there will never
        /// be a <see cref="DataSetValue"/> with the content calculated using this expression.
        /// Default value is <see cref="String.Empty"/>.
        /// </value>
        [DefaultValue(""), JsonProperty("CalculatedValue")]
        public string CalculatedValueExpression { get; set; } = "";

        /// <summary>
        /// Gets/sets the ex[ression used to validate the value of this field.
        /// </summary>
        /// <value>
        /// An expression used to validate the value of this field. This expression can
        /// be also applied to calculated fields. It is not evaluated if value is empty and
        /// <see cref="RequiredIfExpression"/> is specified.
        /// Default value is <see cref="String.Empty"/>.
        /// </value>
        [DefaultValue(""), JsonProperty("ValidIf")]
        public string ValidIfExpression { get; set; } = "";

        /// <summary>
        /// Gets/sets the localized message to report if a given value isn't valid according
        /// to <see cref="ValidIfExpression"/>. If omitted then default message is used.
        /// Default value is <see cref="String.Empty"/>.
        /// </summary>
        [DefaultValue("")]
        public string ValidationMessage { get; set; } = "";

        /// <summary>
        /// Gets/sets the ex[ression used to determine if value of this field is possibly wrong.
        /// </summary>
        /// <value>
        /// An expression used to determine if value of this field is possibly wrong. Warnings are
        /// not errors then data can be submitted, they may be useful to warn users about possible
        /// issues with data. Default value is <see cref="String.Empty"/>.
        /// </value>
        [DefaultValue(""), JsonProperty("WarningIf")]
        public string WarningIfExpression { get; set; } = "";

        /// <summary>
        /// Gets/sets the localized message to report if a given value is possibly wrong according
        /// to <see cref="WarningIfExpression"/>. If omitted then default message is used.
        /// Default value is <see cref="String.Empty"/>.
        /// </summary>
        [DefaultValue("")]
        public string WarningMessage { get; set; } = "";

        /// <summary>
        /// Gets/sets the expression used to determine if this field is enabled.
        /// </summary>
        /// <value>
        /// An expression used to determine if this field is enabled. Disabled fields are not
        /// calculated and user cannot edit them. Disabled fields are not saved into the underlying
        /// storage. Default value is <see cref="String.Empty"/>.
        /// </value>
        [DefaultValue(""), JsonProperty("EnabledIf")]
        public string EnabledIfExpression { get; set; } = "";

        /// <summary>
        /// Gets/sets the expression used to determine if this field is visible.
        /// </summary>
        /// <value>
        /// An expression used to determine if this field is visible. Invisible fields
        /// are not presented to end-user. Default value is <see cref="String.Empty"/>.
        /// </value>
        /// <remarks>
        /// Regardless presentation purposes this expression may be a literal <em>false</em> constant,
        /// used for calculated fields to split very complex validation expressions.
        /// </remarks>
        [DefaultValue(""), JsonProperty("VisibleIf")]
        public string VisibleIfExpression { get; set; } = "";

        /// <summary>
        /// Gets/sets the preferred aggregation mode to summarize multiple values of this field.
        /// </summary>
        /// <value>
        /// The preferred aggregation mode used to summarize multiple values of this field
        /// when presenting a summary. Default value is <see cref="AggregationMode.None"/>.
        /// </value>
        [DefaultValue(typeof(AggregationMode), nameof(AggregationMode.None))]
        public AggregationMode PreferredAggregation { get; set; } = AggregationMode.None;

        /// <summary>
        /// Gets/sets the expression applied to the value before perfroming the aggregation
        /// specified with <see cref="PreferredAggregation"/>.
        /// </summary>
        /// <value>
        /// An expression used to pre-compute/transform the value of the field before aggregation.
        /// </value>
        [DefaultValue(""), JsonProperty("TransformationForAggregation")]
        public string TransformationForAggregation { get; set; } = "";

        /// <summary>
        /// Gets the list of all children values related to this field.
        /// </summary>
        /// <value>
        /// The list of children values related to this field.
        /// </value>
        public ValueDescriptorCollection Children { get; } = new ValueDescriptorCollection();

        /// <summary>
        /// Infrastructure. This method is to support JSON serialization and should not be called directly.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("This method should not be used directly", true)]
        public bool ShouldSerializeChildren()
        {
            return Children.Count > 0;
        }

        /// <summary>
        /// Infrastructure. This method is to support JSON serialization and should not be called directly.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("This method should not be used directly", true)]
        public bool ShouldSerializeAvailableValues()
        {
            return AvailableValues.Count > 0;
        }

        public override int GetHashCode()
        {
            return Reference?.GetHashCode() ?? 0;
        }

        internal IEnumerable<ModelError> ValidateModel()
        {
            bool isCalculated = !String.IsNullOrWhiteSpace(CalculatedValueExpression);

            if (!isCalculated && String.IsNullOrWhiteSpace(Reference))
                yield return Error("Editable field must have a reference ID.");

            if (isCalculated && AvailableValues.Count > 0)
                yield return Error($"{nameof(AvailableValues)} cannot be used for calculated fields.");

            if (isCalculated && !String.IsNullOrWhiteSpace(DefaultValueExpression))
                yield return Error($"{nameof(DefaultValueExpression)} cannot be specified for calculated fields.");

            if (!String.IsNullOrWhiteSpace(ValidationMessage) && String.IsNullOrWhiteSpace(ValidIfExpression))
                yield return Warning($"{nameof(ValidationMessage)} should not be specified without {nameof(ValidIfExpression)}.");

            if (!String.IsNullOrWhiteSpace(WarningMessage) && String.IsNullOrWhiteSpace(WarningIfExpression))
                yield return Warning($"{nameof(WarningMessage)} should not be specified without {nameof(WarningIfExpression)}.");

            if (String.IsNullOrWhiteSpace(TransformationForAggregation))
            {
                bool aggregationRequiresNumericValue = PreferredAggregation == AggregationMode.Average
                    || PreferredAggregation == AggregationMode.Sum;

                if (Type == TypeOfValue.String && aggregationRequiresNumericValue)
                    yield return Error($"Aggregation mode {PreferredAggregation} cannot be used for type {Type} without a transformation expression.");
            }
            else if (PreferredAggregation == AggregationMode.None)
            {
                yield return Error($"{nameof(TransformationForAggregation)} cannot be specified with aggregation mode {nameof(AggregationMode.None)}.");
            }
        }

        internal IEnumerable<ValueDescriptor> VisitAllValues()
        {
            Debug.Assert(Children != null);

            return Children.VisitAllValues().Prepend(this);
        }

        private ModelError Error(string message)
        {
            string name = String.IsNullOrWhiteSpace(Reference) ? Name : Reference;
            return new ModelError(IssueSeverity.ModelError, this, $"Value '{name}': {message}");
        }

        private ModelError Warning(string message)
        {
            string name = String.IsNullOrWhiteSpace(Reference) ? Name : Reference;
            return new ModelError(IssueSeverity.Warning, this, $"Value '{name}': {message}");
        }
    }
}
