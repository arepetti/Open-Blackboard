using System;

namespace OpenBlackboard.Model
{
    /// <summary>
    /// Represents the severity of an issue described by a <see cref="ModelError"/> or <see cref="DataError"/>.
    /// </summary>
    public enum IssueSeverity
    {
        /// <summary>
        /// Issue is not critical or it's not an issue at all. It dosn't prevent the model/data to be used.
        /// </summary>
        Warning,

        /// <summary>
        /// Issue is an error which prevents this model to be used. Usually it denotes an error in the model
        /// but it might be caused by wrong or missing input data (for example when used for evaluating an expression
        /// of a calculated value).
        /// </summary>
        ModelError,

        /// <summary>
        /// Issue is a data validation error.
        /// </summary>
        ValidationError,
    }
}
