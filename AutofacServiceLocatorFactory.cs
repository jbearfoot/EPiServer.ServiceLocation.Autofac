using Autofac;
using EPiServer.ServiceLocation.AutoDiscovery;
using EPiServer.ServiceLocation.Autofac;
using System;

[assembly: ServiceLocatorFactory(typeof(AutofacServiceLocatorFactory))]

namespace EPiServer.ServiceLocation.Autofac
{
    public class AutofacServiceLocatorFactory : IServiceLocatorFactory
    {
        private readonly ContainerBuilder _containerBuilder;

        public AutofacServiceLocatorFactory() :this(null)
        { }
        public AutofacServiceLocatorFactory(ContainerBuilder containerBuilder)
        {
            _containerBuilder = containerBuilder ?? new ContainerBuilder();
        }
        public IServiceLocator CreateLocator()
        {
            var locator = new AutofacServiceLocator();
            _containerBuilder.RegisterInstance<IServiceLocator>(locator).SingleInstance();
            locator.Container = _containerBuilder.Build();
            return locator;
        }

        public IServiceConfigurationProvider CreateProvider()
        {
            return new AutofacServiceConfigurationProvider(_containerBuilder);
        }
    }
}
