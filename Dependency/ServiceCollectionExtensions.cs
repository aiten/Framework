/*
  This file is part of  https://github.com/aiten/Framework.

  Copyright (c) Herbert Aitenbichler

  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
  to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
  and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
  WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
*/

namespace Framework.Dependency
{
    using System;
    using System.Linq;
    using System.Reflection;

    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers public and internal types of the given assemblies with the unity services. This is necessary
        /// to workaround the internals visible to hacks in the code base.
        /// </summary>
        /// <param name="services">Dependency services.</param>
        /// <param name="liveTime"></param>
        /// <param name="assemblies">List of assemblies in which all types should be registered with their interfaces.
        /// This includes internal types. </param>
        /// <returns>This instance.</returns>
        public static IServiceCollection AddAssemblyIncludingInternals(this IServiceCollection services, ServiceLifetime liveTime, params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract))
                {
                    var interfaceName = "I" + type.Name;
                    var interfaceType = type.GetInterface(interfaceName);
                    if (interfaceType != null)
                    {
                        services.Add(new ServiceDescriptor(interfaceType, type, liveTime));
                    }
                }
            }

            return services;
        }

        public static IServiceCollection AddAssemblyByName(this IServiceCollection services, Func<string, bool> checkName, ServiceLifetime liveTime, params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract))
                {
                    if (checkName(type.Name))
                    {
                        services.Add(new ServiceDescriptor(type, type, liveTime));
                    }
                }
            }

            return services;
        }
    }
}