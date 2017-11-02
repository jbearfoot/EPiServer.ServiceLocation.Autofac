using Autofac;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPiServer.ServiceLocation
{
    public static  class ServiceConfigurationExtensions
    {
        public static ContainerBuilder Builder(this IServiceConfigurationProvider provider)
        {
            return (provider as Autofac.AutofacServiceConfigurationProvider).Builder;
        }
    }
}
