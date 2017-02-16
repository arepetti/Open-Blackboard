using System;

namespace OpenBlackboard.Hosting
{
    /// <summary>
    /// Base class for all the objects that expose a service.
    /// </summary>
    public abstract class Service
    {
        /// <summary>
        /// Creates a new instance of <see cref="Service"/>.
        /// </summary>
        /// <param name="context">Context to which this service will belong to.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="context"/> is <see langword="null"/>.
        /// </exception>
        protected Service(Context context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            Context = context;
        }

        /// <summary>
        /// Gets the context associated with this service.
        /// </summary>
        /// <value>
        /// The context that has been associated with this service when it
        /// has been created.
        /// </value>
        public Context Context
        {
            get;
        }
    }
}
