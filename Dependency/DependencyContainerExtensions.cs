/*
  This file is part of CNCLib - A library for stepper motors.

  Copyright (c) Herbert Aitenbichler

  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
  to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
  and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
  WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
*/

using Microsoft.Extensions.DependencyInjection;

namespace Framework.Dependency
{
    using System;
    using System.Linq;
    using System.Reflection;

    public static class DependencyContainerExtensions
    {
        /// <summary>
        /// Registers public and internal types of the given assemblies with the unity container. This is necessary
        /// to workaround the internals visible to hacks in the code base.
        /// </summary>
        /// <param name="container">Dependency container.</param>
        /// <param name="liveTime"></param>
        /// <param name="assemblies">List of assemblies in which all types should be registered with their interfaces. 
        /// This includes internal types. </param>
        /// <returns>This instance.</returns>
        public static IServiceCollection RegisterTypesIncludingInternals(this IServiceCollection container, ServiceLifetime liveTime, params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract))
                {
                    string interfaceName = "I" + type.Name;
                    var    interfaceType = type.GetInterface(interfaceName);
                    if (interfaceType != null)
                    {
                        container.Add(new ServiceDescriptor(interfaceType, type, liveTime));
                    }
                }
            }

            return container;
        }

        public static IServiceCollection RegisterTypesByName(this IServiceCollection container, Func<string, bool> checkName, ServiceLifetime liveTime, params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract))
                {
                    if (checkName(type.Name))
                    {
                        container.Add(new ServiceDescriptor(type, type, liveTime));
                    }
                }
            }

            return container;
        }

/*
        /// <summary>
        /// Resolves the interface to a specific type that was registered earlier. 
        /// </summary>
        /// <param name="container">Dependency container.</param>
        /// <typeparam name="TInterface">Interface for which the registered type is looked up.</typeparam>
        /// <returns>An instance of the interface that was registered with the container earlier.</returns>
        public static TInterface Resolve<TInterface>(this IServiceCollection container)
        {
            object obj = container.GetResolver().Resolve(typeof(TInterface));
            return (TInterface)obj;
        }

        public static TInterface Resolve<TInterface>(this IServiceCollection resolver)
        {
            return (TInterface)resolver.Resolve(typeof(TInterface));
        }
*/
    }
}