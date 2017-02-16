using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace OpenBlackboard.Hosting
{
    /// <summary>
    /// Exposes methods to locate services available in a system.
    /// </summary>
    public sealed class ServiceLocator
    {
        /// <summary>
        /// The shared and thread-safe instance of the service locator.
        /// </summary>
        public static readonly ServiceLocator Default = new ServiceLocator();

        /// <summary>
        /// Finds and creates an instance of all the services located in the default application directory.
        /// </summary>
        /// <param name="context">The context into which services will be created.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="context"/> is <see langword="null"/>.
        /// </exception>
        public async Task<IEnumerable<Service>> DiscoveryAsync(Context context)
        {
            return await DiscoveryAsync(context,
                Host.GetBaseDirectory(), "*.dll", SearchOption.TopDirectoryOnly);
        }

        /// <summary>
        /// Finds and creates and instance of all the services located in the specified search path,
        /// searching for assemblies with indicated search pattern and options.
        /// </summary>
        /// <param name="context">The context into which services will be created.</param>
        /// <param name="searchPath">Folder containing the candiate assemblies to export the services.</param>
        /// <param name="searchPatterns">List of search patterns (separated with <c>;</c>) to locate assemblies.</param>
        /// <param name="searchOption">Options for the search.</param>
        /// <returns>
        /// A new list of newly created services, available in the specified context, located in the folder and searched
        /// with the indicated search pattern(s).
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="context"/> is <see langword="null"/>.
        /// </exception>
        public async Task<IEnumerable<Service>> DiscoveryAsync(Context context, string searchPath, string searchPatterns, SearchOption searchOption)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var assemblies = LocateAssembliesAsync(searchPath, searchPatterns, searchOption);

            var conventions = new ConventionBuilder();
            conventions.ForTypesDerivedFrom<IServiceFactory>()
                .Export<IServiceFactory>()
                .Shared();

            var configuration = new ContainerConfiguration();
            configuration.WithAssemblies(await assemblies, conventions);

            using (var container = configuration.CreateContainer())
            {
                return container.GetExports<IServiceFactory>()
                    .Where(x => x.IsAvailable(context))
                    .Select(x => x.Create(context))
                    .ToArray();
            }
        }

        private async Task<IEnumerable<Assembly>> LocateAssembliesAsync(string searchPath, string searchPatterns, SearchOption searchOption)
        {
            return await searchPatterns.Split(';')
                .SelectManyAsync(async searchPattern => LoadAssemblies(await GetFilesAsync(searchPath, searchPattern, searchOption)));
        }

        private IEnumerable<Assembly> LoadAssemblies(IEnumerable<string> assemblies)
        {
            return assemblies
                .Select(AssemblyLoadContext.GetAssemblyName)
                .Select(AssemblyLoadContext.Default.LoadFromAssemblyName);
        }

        private async Task<IEnumerable<string>> GetFilesAsync(string searchPath, string searchPattern, SearchOption searchOption)
        {
            return await Task.Run(() => Directory.GetFiles(searchPath, searchPattern, searchOption));
        }

    }
}
