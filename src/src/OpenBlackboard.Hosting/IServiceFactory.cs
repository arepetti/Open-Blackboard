using System;

namespace OpenBlackboard.Hosting
{
    /// <summary>
    /// Represents the factory used by <see cref="ServiceLocator"/> to determine
    /// services that may be created in a given context.
    /// </summary>
    public interface IServiceFactory
    {
        /// <summary>
        /// Determines if the service managed by this factory can be created in
        /// the specified context.
        /// </summary>
        /// <param name="context">Context into which the service will be created.</param>
        /// <returns></returns>
        bool IsAvailable(Context context);

        /// <summary>
        /// Creates the service managed by this factory in the specified context.
        /// </summary>
        /// <param name="context">Context into which the service will be created.</param>
        /// <returns>
        /// A new instance of the service managed by this factory, created into the
        /// specified context.
        /// </returns>
        /// <remarks>
        /// Caller will not register the service as available to be discovered by other services,
        /// if the newly created service must be published then it is responsibility of this factory
        /// to register it using <see cref="Context.Services"/> methods.
        /// </remarks>
        Service Create(Context context);
    }
}
