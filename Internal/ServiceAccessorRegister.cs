using EPiServer.ServiceLocation.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPiServer.ServiceLocation.Autofac.Internal
{
    class ServiceAccessorRegister : IRegisteredService
    {
        private readonly IServiceConfigurationProvider _serviceConfigurationProvider;
        private readonly Type _serviceType;

        public ServiceAccessorRegister(IServiceConfigurationProvider serviceConfigurationProvider, Type serviceType)
        {
            _serviceConfigurationProvider = serviceConfigurationProvider;
            _serviceType = serviceType;
        }
        public IRegisteredService Add(Type serviceType, Type implementationType, ServiceInstanceScope lifetime)
        {
            return _serviceConfigurationProvider.Add(serviceType, implementationType, lifetime);
        }

        public IRegisteredService Add(Type serviceType, Func<IServiceLocator, object> implementationFactory, ServiceInstanceScope lifetime)
        {
            return _serviceConfigurationProvider.Add(serviceType, implementationFactory, lifetime);
        }

        public IRegisteredService Add(Type serviceType, object instance)
        {
            return _serviceConfigurationProvider.Add(serviceType, instance);
        }

        public IServiceConfigurationProvider AddServiceAccessor()
        {
            ReflectiveServiceConfigurationHelper.RegisterServiceAccessorDelegates(this, _serviceType);
            return _serviceConfigurationProvider;
        }

        public bool Contains(Type serviceType)
        {
            return _serviceConfigurationProvider.Contains(serviceType);
        }

        public IServiceConfigurationProvider RemoveAll(Type serviceType)
        {
            return _serviceConfigurationProvider.RemoveAll(serviceType);
        }
    }
}
