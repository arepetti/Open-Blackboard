using System;

namespace OpenBlackboard.Hosting
{
    /// <summary>
    /// Represents an actor (human being or service) that can produce or consume any <strong>public piece information</strong>,
    /// interacting with other actors in the system.
    /// to produce 
    /// </summary>
    public interface IActor : IIdentifiable
    {
        /// <summary>
        /// Gets the display name.
        /// </summary>
        /// <value>
        /// The display name, mostly used in User Interface, shown for
        /// this actor. Display name does not need to be unique and may change over time.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Gets the host on which this actor is operating.
        /// </summary>
        /// <value>
        /// The host which instantiated this actor and on which this actor
        /// is operating, this also determines all the other visible actors and
        /// available services.
        /// </value>
        Host Host { get; }
    }
}
