using System;

namespace OpenBlackboard.Hosting
{
    /// <summary>
    /// Interface implemented by any object with an globally unique identifier.
    /// </summary>
    public interface IIdentifiable
    {
        /// <summary>
        /// Gets the unique ID used to identify this object.
        /// </summary>
        /// <value>
        /// The immutable unique ID that uniquely and globally identifies this object within the platform. 
        /// </value>
        string Id { get; }
    }
}
