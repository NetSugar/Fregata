using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Fregata
{
    internal partial class IocProvider
    {
        internal static IServiceProvider Provider { get; private set; }

        internal static void SetProvider(IServiceProvider serviceProvider)
        {
            Provider = serviceProvider;
        }

        public static IServiceProvider GetProvider()
        {
            return Provider;
        }

        /// <summary>
        /// Creates a new Microsoft.Extensions.DependencyInjection.IServiceScope that can be used to resolve scoped services.
        /// </summary>
        /// <returns>A Microsoft.Extensions.DependencyInjection.IServiceScope that can be used to resolve scoped services.</returns>
        public static IServiceScope CreateScope()
        {
            return GetProvider().CreateScope();
        }

        /// <summary>
        /// Get service of type serviceType from the System.IServiceProvider.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <exception cref="System.InvalidOperationException">There is no service of type serviceType.</exception>
        /// <returns>A service object of type serviceType.</returns>
        public static object GetRequiredService(Type serviceType)
        {
            return GetProvider().GetRequiredService(serviceType);
        }

        /// <summary>
        /// Get service of type T from the System.IServiceProvider.
        /// </summary>
        /// <typeparam name="T">The type of service object to get.</typeparam>
        /// <exception cref="System.InvalidOperationException">There is no service of type T.</exception>
        /// <returns>A service object of type T.</returns>
        public static T GetRequiredService<T>()
        {
            return GetProvider().GetRequiredService<T>();
        }

        /// <summary>
        ///  Gets the service object of the specified type.
        /// </summary>
        /// <param name="typeService">An object that specifies the type of service object to get.</param>
        /// <returns>A service object of type serviceType. -or- null if there is no service object of type serviceType.</returns>
        public static object GetService(Type typeService)
        {
            return GetProvider().GetService(typeService);
        }

        /// <summary>
        /// Get service of type T from the System.IServiceProvider.
        /// </summary>
        /// <typeparam name="T">The type of service object to get.</typeparam>
        /// <returns>A service object of type T or null if there is no such service.</returns>
        public static T GetService<T>()
        {
            return GetProvider().GetService<T>();
        }

        /// <summary>
        /// Get an enumeration of services of type T from the System.IServiceProvider.
        /// </summary>
        /// <typeparam name="T">The type of service object to get.</typeparam>
        /// <returns>An enumeration of services of type T.</returns>
        public static IEnumerable<T> GetServices<T>()
        {
            return GetProvider().GetServices<T>();
        }

        /// <summary>
        /// Get an enumeration of services of type serviceType from the System.IServiceProvider.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <returns>An enumeration of services of type serviceType.</returns>
        public static IEnumerable<object> GetServices(Type serviceType)
        {
            return GetProvider().GetServices(serviceType);
        }
    }
}