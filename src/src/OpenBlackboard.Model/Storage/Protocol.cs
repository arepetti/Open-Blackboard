using System;
using System.ComponentModel.DataAnnotations;

namespace OpenBlackboard.Model.Storage
{
    /// <summary>
    /// Represents a protocol, that is a set of fields that must be collected
    /// for a submission, and all the rules used to describe and validate them.
    /// </summary>
    public class Protocol
    {
        /// <summary>
        /// Gets/sets the unique ID of this record.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets/sets the reference name of this protocol.
        /// </summary>
        /// <value>
        /// The reference name of the protocol uniquely identify the
        /// protocol with an immutable human-friendly ID (regardless its
        /// numeric database <see cref="Id"/>).
        /// </value>
        [Required]
        public string Reference { get; set; }

        /// <summary>
        /// Gets/sets the name of this protocol.
        /// </summary>
        /// <value>
        /// The friendly display name of this protocol. This name is informative
        /// only and can be changed as required.
        /// </value>
        [Required]
        public string Name { get; set; }
    }
}
