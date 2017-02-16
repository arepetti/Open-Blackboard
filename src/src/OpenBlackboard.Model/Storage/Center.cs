using System;
using System.Collections.Generic;

namespace OpenBlackboard.Model.Storage
{
    /// <summary>
    /// Represents a center which is an entity associated with
    /// a set of submissions <see cref="Submission"/>.
    /// </summary>
    public class Center
    {
        /// <summary>
        /// Gets/sets the unique ID of this record.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets/sets the display name.
        /// </summary>
        /// <value>
        /// The friendly display name used to identify this center when
        /// presented to other human being users.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets/sets when this center has been created and inserted in the database.
        /// </summary>
        /// <value>
        /// When this center has been created and inserted in the database. This value is fixed
        /// and it doesn't reflect changes.
        /// </value>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// Gets/sets the name of the creator of this center.
        /// </summary>
        /// <value>
        /// The ID or a friendly name used to identify who created this center in the database. This
        /// is a free-text informative field.
        /// </value>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Gets/sets the list of all submissions made by this center.
        /// </summary>
        /// <value>
        /// The list of all submissions that has been made by this center.
        /// </value>
        public virtual ICollection<Submission> Submissions { get; set; }
    }
}
