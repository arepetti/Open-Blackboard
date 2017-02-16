using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace OpenBlackboard.Hosting
{
    /// <summary>
    /// Represents application configuration.
    /// </summary>
    public sealed class Settings
    {
        /// <summary>
        /// Default instance of the application settings.
        /// </summary>
        public static readonly Settings Default = new Settings();

        /// <summary>
        /// Creates a new <see cref="Settings"/> reading configuration
        /// values from the specified JSON file.
        /// </summary>
        /// <param name="path">Full path for the JSON file containing the configuration settings.</param>
        public Settings(string path)
        {
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection()
                .AddJsonFile(path, true)
                .Build();
        }

        private Settings()
            : this(ResolveDefaultConfigurationFilePath())
        {
        }

        /// <summary>
        /// Gets the configuration value with the specified name.
        /// </summary>
        /// <param name="name">Name of the configuration value to read.</param>
        /// <returns>
        /// Value of the configuration value or <see langword="null"/> if it is not present and there
        /// is not another default value.
        /// </returns>
        public string this[string name]
        {
            get { return _configuration[name]; }
        }

        /// <summary>
        /// Gets a connection string with the specified name.
        /// </summary>
        /// <param name="name">Name of the connection string.</param>
        /// <returns>
        /// The connection string with the specified name, located in
        /// <c>ConnectionStrings:&lt;name&gt;</c>.
        /// </returns>
        public string GetConnectionString(string name)
        {
            return _configuration.GetConnectionString(name);
        }

        /// <summary>
        /// Read the specified object from configuration.
        /// </summary>
        /// <typeparam name="T">Type of the object stored in configuration.</typeparam>
        /// <returns>
        /// A newly created object of type <typeparamref name="T"/> where its properties
        /// are read from configuration file.
        /// </returns>
        public T Get<T>()
        {
            return _configuration.Get<T>();
        }

        private readonly IConfiguration _configuration;

        private static string ResolveDefaultConfigurationFilePath()
        {
            return Path.Combine(Host.GetBaseDirectory(), Host.GetModuleFileName() + ".json");
        }
    }
}
