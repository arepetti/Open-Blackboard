using System;
using System.ComponentModel.DataAnnotations;

namespace OpenBlackboard.Model.Storage
{
    /// <summary>
    /// Represents a single submitted value of a <see cref="Submission"/>.
    /// </summary>
    public class Value
    {
        /// <summary>
        /// Gets/sets the unique ID of this record.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets/sets the reference that associate this value to
        /// a specific rule/field inside the <see cref="Protocol"/>.
        /// </summary>
        [Required]
        public string Reference { get; set; }

        /// <summary>
        /// Gets/sets the textual representation of this record.
        /// </summary>
        /// <value>
        /// The textual representation of this value may be an alternative
        /// optional to be shown to end-users instead of <see cref="Number"/>
        /// or the only content of this value. If it has to be used depends on the
        /// type of the field as described in the <see cref="Protocol"/>.
        /// </value>
        public string Text { get; set; }

        /// <summary>
        /// Gets/sets the numeric value of this record.
        /// </summary>
        /// <value>
        /// The numeric value carried by this record. It may be the only value
        /// carried or it may be ignored, according to the field type and the
        /// rules described in the <see cref="Protocol"/>.
        /// </value>
        public double? Number { get; set; }

    }
}
