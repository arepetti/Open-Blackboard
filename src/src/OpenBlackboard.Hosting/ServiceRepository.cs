using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenBlackboard.Hosting
{
    /// <summary>
    /// Represents a repository of available services in a specific context.
    /// </summary>
    public sealed class ServiceRepository
    {
        /// <summary>
        /// Registers specified provider for the service of indicated type.
        /// </summary>
        /// <param name="serviceType">
        /// Type of the service exposed by <paramref name="serviceInstance"/>.
        /// This type can be any interface or any class derived from <see cref="Service"/> (in this case
        /// it does not need to be a concrete class).
        /// </param>
        /// <param name="serviceInstance">
        /// An instance of a class that implements or derives from the type <paramref name="serviceType"/>.
        /// Every instance must be a class derived from <see cref="Service"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="serviceType"/> is <see langword="null"/>.
        /// <br/>-or-<br/>
        /// If <paramref name="serviceInstance"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="serviceInstance"/> does not implement the interface or does not derive from the type
        /// specified with <paramref name="serviceType"/>.
        /// </exception>
        public void Register(Type serviceType, Service serviceInstance)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            if (serviceInstance == null)
                throw new ArgumentNullException(nameof(serviceInstance));

            if (!InstanceImplementsService(serviceType, serviceInstance))
                throw new ArgumentException($"To be registered as service {serviceInstance.GetType().Name} must implement/derive from {serviceType.Name}", nameof(serviceInstance));

            _services.Add(serviceType, serviceInstance);
        }

        /// <summary>
        /// Registers specified provider for the service of indicated type.
        /// </summary>
        /// <typeparam name="TService">
        /// Type of the service exposed by <paramref name="serviceInstance"/>.
        /// This type can be any interface or any class derived from <see cref="Service"/> (in this case
        /// it does not need to be a concrete class).
        /// </typeparam>
        /// <param name="serviceInstance">
        /// An instance of a class that implements or derives from the type <typeparamref name="TService"/>.
        /// Every instance must be a class derived from <see cref="Service"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="serviceInstance"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="serviceInstance"/> does not implement the interface or does not derive from the type
        /// specified with <typeparamref name="TService"/>.
        /// </exception>
        public void Register<TService>(Service serviceInstance)
        {
            Register(typeof(TService), serviceInstance);
        }

        /// <summary>
        /// Register the specified service as a provider of its own type.
        /// </summary>
        /// <param name="service">Instance of the service to register, consumers will need to know its exact type to use it.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="serviceInstance"/> is <see langword="null"/>.
        /// </exception>
        public void Register(Service serviceInstance)
        {
            if (serviceInstance == null)
                throw new ArgumentNullException(nameof(serviceInstance));

            _services.Add(serviceInstance.GetType(), serviceInstance);
        }

        /// <summary>
        /// Deregisters specified provider for the service of indicated type.
        /// </summary>
        /// <param name="serviceType">
        /// Type of the service exposed by <paramref name="serviceInstance"/>.
        /// </param>
        /// <param name="serviceInstance">
        /// The instance of the service to deregister.
        /// </param>
        /// <returns>
        /// <see langword="false"/> if the specified service cannot be found or there is not <paramref name="serviceInstance"/>
        /// as a registered provider otherwise <see langword="true"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="serviceType"/> is <see langword="null"/>.
        /// <br/>-or-<br/>
        /// If <paramref name="serviceInstance"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="serviceInstance"/> does not implement the interface or does not derive from the type
        /// specified with <paramref name="serviceType"/>.
        /// </exception>
        public bool Deregister(Type serviceType, Service serviceInstance)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            if (serviceInstance == null)
                throw new ArgumentNullException(nameof(serviceInstance));

            if (!InstanceImplementsService(serviceType, serviceInstance))
                throw new ArgumentException($"To deregister service {serviceInstance.GetType().Name} must implement/derive from {serviceType.Name}", nameof(serviceInstance));

            return _services.Remove(serviceType, serviceInstance);
        }

        /// <summary>
        /// Deregisters specified provider for the service of indicated type.
        /// </summary>
        /// <typeparam name="TService">
        /// Type of the service exposed by <paramref name="serviceInstance"/>.
        /// </typeparam>
        /// <param name="serviceInstance">
        /// The instance of the service to deregister.
        /// </param>
        /// <returns>
        /// <see langword="false"/> if the specified service cannot be found or there is not <paramref name="serviceInstance"/>
        /// as a registered provider otherwise <see langword="true"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="serviceInstance"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="serviceInstance"/> does not implement the interface or does not derive from the type
        /// specified with <paramref name="serviceType"/>.
        /// </exception>
        public bool Deregister<TService>(Service serviceInstance)
        {
            return Deregister(typeof(TService), serviceInstance);
        }

        /// <summary>
        /// Deregister the specified service.
        /// </summary>
        /// <param name="serviceType">
        /// Instance of the service to deregister. Note that service type is the exact type of
        /// <paramref name="serviceType"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> is service has been deregistered.
        /// </returns>
        /// <remarks>
        /// Even if it is perfectly possible to register a service using <see cref="Register(Type, Service)"/> using, for example,
        /// <c>Register(obj.GetType(), obj)</c> and then deregistering using <c>Deregister(obj)</c> it's a discouraged practice and
        /// registering/deregistering should be performed the most appropriate pair.
        /// </remarks>
        public bool Deregister(Service serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            return _services.Remove(serviceType.GetType(), serviceType);
        }

        /// <summary>
        /// Finds all the providers for the specified service.
        /// </summary>
        /// <param name="serviceType">Service for which you want to find all the registered providers.</param>
        /// <returns>
        /// All the providers registered for the service of type <paramref name="serviceType"/>. List may be empty
        /// if no one registered a provider or to contain many elements if multiple providers have been registered for the
        /// same service. Use <c>First()</c>, <c>FirstOrDefault()</c> or their counterparts <c>Single()</c> and <c>SingleOrDefault()</c>
        /// as appropriate. Each instance must be casted to the required type.
        /// </returns>
        public IEnumerable<Service> Discovery(Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            return _services.GetAll(serviceType);
        }

        /// <summary>
        /// Finds all the providers for the specified service.
        /// </summary>
        /// <typeparam name="TService">Service for which you want to find all the registered providers.</typeparam>
        /// <returns>
        /// All the providers registered for the service of type <typeparamref name="serviceType"/>. List may be empty
        /// if no one registered a provider or to contain many elements if multiple providers have been registered for the
        /// same service. Use <c>First()</c>, <c>FirstOrDefault()</c> or their counterparts <c>Single()</c> and <c>SingleOrDefault()</c>
        /// as appropriate.
        /// </returns>
        public IEnumerable<TService> Discovery<TService>()
        {
            return _services.GetAll(typeof(TService)).Cast<TService>();
        }

        private readonly ServiceDictionary<Type, Service> _services = new ServiceDictionary<Type, Service>();

        private static bool InstanceImplementsService(Type serviceType, object serviceInstance)
        {
            return serviceType.GetTypeInfo().IsAssignableFrom(serviceInstance.GetType());
        }
    }
}
