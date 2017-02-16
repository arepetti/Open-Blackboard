using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OpenBlackboard.Hosting
{
    /// <summary>
    /// Represents an abstract host that holds all the actors of the system
    /// and manage their interactions.
    /// </summary>
    public abstract class Host
    {
        /// <summary>
        /// Creates a new instance of <see cref="Host"/>.
        /// </summary>
        protected Host()
        {
            Context = new Context();
        }

        /// <summary>
        /// Gets the context associated with this host.
        /// </summary>
        /// <value>
        /// The context associated with this host.
        /// </value>
        public Context Context
        {
            get;
        }

        internal static string GetBaseDirectory()
        {
            string entryAssemblyPath = GetEntryAssemblyPath();
            if (entryAssemblyPath == null)
                return AppContext.BaseDirectory;

            return Path.GetDirectoryName(entryAssemblyPath);
        }

        internal static string GetModuleFileName()
        {
            string entryAssemblyPath = GetEntryAssemblyPath();
            if (entryAssemblyPath == null)
                return nameof(OpenBlackboard);

            return Path.GetFileName(entryAssemblyPath);
        }

        private static string GetEntryAssemblyPath()
        {
            var entryAssembly = Assembly.GetEntryAssembly();

            if ((entryAssembly?.IsDynamic ?? true) || String.IsNullOrEmpty(entryAssembly.Location))
                return null;

            return Uri.UnescapeDataString(new UriBuilder(entryAssembly.Location).Path);
        }
    }
}
