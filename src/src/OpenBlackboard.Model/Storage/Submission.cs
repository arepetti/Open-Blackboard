using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OpenBlackboard.Model.Storage
{
    /// <summary>
    /// Represents a submission, that is a set of <see cref="Value"/>,
    /// according to the rules described in a <see cref="Protocol"/>, sent from a <see cref="Center"/>.
    /// </summary>
    public class Submission
    {
        /// <summary>
        /// Gets/sets the unique ID of this record.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets/sets the database ID for the protocol associated
        /// with this submission.
        /// </summary>
        /// <value>
        /// The database ID for the protocol associated with this submission.
        /// </value>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int ProtocolId { get; set; }

        /// <summary>
        /// Gets/sets the protocol associated with this submission.
        /// </summary>
        /// <value>
        /// The protocol associated with this submission.
        /// </value>
        [Required]
        public Protocol Protocol { get; set; }

        /// <summary>
        /// Gets/sets the timestamp of when this submission has been received.
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// Gets/sets the name of whom sent this submission.
        /// </summary>
        /// <value>
        /// Name of whom sent this submission. This field is an informative
        /// optional field and its meaning is defined by its users.
        /// </value>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Gets/sets the values associated with this submission.
        /// </summary>
        /// <value>
        /// The values associated with this submission, according to the
        /// rules specified in the <see cref="Protocol"/>.
        /// </value>
        public virtual ICollection<Value> Values { get; set; }
    }
}
