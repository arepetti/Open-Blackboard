using System;

namespace OpenBlackboard.Hosting
{
    /// <summary>
    /// Represents the execution context for an actor in the system.
    /// </summary>
    public sealed class Context
    {
        /// <summary>
        /// Creates a new instance of <see cref="Context"/>.
        /// </summary>
        public Context()
        {
            Services = new ServiceRepository();
        }

        /// <summary>
        /// Gets the service repository.
        /// </summary>
        /// <value>
        /// The service repository where you can register services you expose or ask for services exposed
        /// by other actors.
        /// </value>
        public ServiceRepository Services
        {
            get;
        }
    }
}
