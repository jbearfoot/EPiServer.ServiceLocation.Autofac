using Autofac;
using Autofac.Core.Lifetime;
using EPiServer.ServiceLocation.Autofac.Internal;
using EPiServer.ServiceLocation.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EPiServer.ServiceLocation.Autofac
{
    public class AutofacServiceConfigurationProvider : IServiceConfigurationProvider, IInterceptorRegister
    {
        private ContainerBuilder _containerBuilder;
        private HashSet<Type> _registeredTypes = new HashSet<Type>();

        //Some registrations are delegated to generic extensions method in EPiServer.Framework
        private MethodInfo _httpContextTypeMethod = typeof(ServiceConfigurationProviderExtensions).GetMethods().
                        Single(m => m.Name.Equals("AddHttpContextScoped") && m.GetParameters().Length == 1);
        private MethodInfo _httpContextFactoryMethod = typeof(ServiceConfigurationProviderExtensions).GetMethods().
                        Single(m => m.Name.Equals("AddHttpContextScoped") && m.GetParameters().Length == 2);
        private MethodInfo _hybridContextTypeMethod = typeof(ServiceConfigurationProviderExtensions).GetMethods().
                        Single(m => m.Name.Equals("AddHttpContextOrThreadScoped") && m.GetParameters().Length == 1);
        private MethodInfo _hybridContextFactoryMethod = typeof(ServiceConfigurationProviderExtensions).GetMethods().
                Single(m => m.Name.Equals("AddHttpContextOrThreadScoped") && m.GetParameters().Length == 2);
        private MethodInfo _funcFactory = typeof(FuncFactory).GetMethod("Get");

        public AutofacServiceConfigurationProvider(ContainerBuilder containerBuilder)
        {
            _containerBuilder = containerBuilder;
        }

        public ContainerBuilder Builder => _containerBuilder;

        public IRegisteredService Add(Type serviceType, Type implementationType, ServiceInstanceScope lifetime)
        {
            //Open generics are defined differently in Autofac
            if (serviceType.IsGenericTypeDefinition)
                return RegisterOpenGeneric(serviceType, implementationType, lifetime);

            switch (lifetime)
            {
                case ServiceInstanceScope.HttpContext:
                    var genericMethod = _httpContextTypeMethod.MakeGenericMethod(serviceType, implementationType);
                    genericMethod.Invoke(null, new object[] { this });
                    break;
                case ServiceInstanceScope.Singleton:
                    _containerBuilder.RegisterType(implementationType).As(serviceType).SingleInstance();
                    break;
                case ServiceInstanceScope.Hybrid:
                    var genericHybridMethod = _hybridContextTypeMethod.MakeGenericMethod(serviceType, implementationType);
                    genericHybridMethod.Invoke(null, new object[] { this });
                    break;
                case ServiceInstanceScope.Transient:
#pragma warning disable 618
                case ServiceInstanceScope.PerRequest:
                case ServiceInstanceScope.Unique:
#pragma warning restore 618
                    _containerBuilder.RegisterType(implementationType).As(serviceType).InstancePerDependency();
                    break;
                default:
                    throw new NotSupportedException($"Lifetime '{lifetime}' is not supported");
            }

            _registeredTypes.Add(serviceType);
            return new ServiceAccessorRegister(this, serviceType);         
        }

        private IRegisteredService RegisterOpenGeneric(Type serviceType, Type implementationType, ServiceInstanceScope lifetime)
        {
            switch (lifetime)
            {
                case ServiceInstanceScope.HttpContext:
                    var genericMethod = _httpContextTypeMethod.MakeGenericMethod(serviceType, implementationType);
                    genericMethod.Invoke(null, new object[] { this });
                    break;
                case ServiceInstanceScope.Singleton:
                    _containerBuilder.RegisterGeneric(implementationType).As(serviceType).SingleInstance();
                    break;
                case ServiceInstanceScope.Hybrid:
                    var genericHybridMethod = _hybridContextTypeMethod.MakeGenericMethod(serviceType, implementationType);
                    genericHybridMethod.Invoke(null, new object[] { this });
                    break;
                case ServiceInstanceScope.Transient:
#pragma warning disable 618
                case ServiceInstanceScope.PerRequest:
                case ServiceInstanceScope.Unique:
#pragma warning restore 618
                    _containerBuilder.RegisterGeneric(implementationType).As(serviceType).InstancePerDependency();
                    break;
                default:
                    throw new NotSupportedException($"Lifetime '{lifetime}' is not supported");
            }

            _registeredTypes.Add(serviceType);
            return new ServiceAccessorRegister(this, serviceType);
        }

        public IRegisteredService Add(Type serviceType, Func<IServiceLocator, object> implementationFactory, ServiceInstanceScope lifetime)
        {
            switch (lifetime)
            {
                case ServiceInstanceScope.HttpContext:
                    var funcFactoryMethod = _funcFactory.MakeGenericMethod(serviceType);
                    var genericMethod = _httpContextFactoryMethod.MakeGenericMethod(serviceType);
                    genericMethod.Invoke(null, new object[] { this, funcFactoryMethod.Invoke(null, new object[] { implementationFactory }) });
                    break;
                case ServiceInstanceScope.Singleton:
                    _containerBuilder.Register(context => implementationFactory(context.Resolve<IServiceLocator>()))
                        .As(serviceType).SingleInstance();
                    break;
                case ServiceInstanceScope.Hybrid:
                    var funcFactoryHybridMethod = _funcFactory.MakeGenericMethod(serviceType);
                    var genericHybridMethod = _hybridContextFactoryMethod.MakeGenericMethod(serviceType);
                    genericHybridMethod.Invoke(null, new object[] { this, funcFactoryHybridMethod.Invoke(null, new object[] { implementationFactory }) });
                    break;
                case ServiceInstanceScope.Transient:
#pragma warning disable 618
                case ServiceInstanceScope.PerRequest:
                case ServiceInstanceScope.Unique:
#pragma warning restore 618
                    _containerBuilder.Register(context => implementationFactory(context.Resolve<IServiceLocator>()))
                        .As(serviceType).InstancePerDependency();
                    break;
                default:
                    throw new NotSupportedException($"Lifetime '{lifetime}' is not supported");
            }

            _registeredTypes.Add(serviceType);
            return new ServiceAccessorRegister(this, serviceType);
        }

        public IRegisteredService Add(Type serviceType, object instance)
        {
            _containerBuilder.RegisterInstance(instance).As(serviceType);
            _registeredTypes.Add(serviceType);
            return new ServiceAccessorRegister(this, serviceType);
        }

        public bool Contains(Type serviceType)
        {
            //Since Autofac have no way of checking previous registrations, we need to store all service types
            return _registeredTypes.Contains(serviceType);
        }

        public void Intercept<T>(Func<IServiceLocator, T, T> interceptorFactory) where T : class
        {
            _containerBuilder.RegisterDecorator<T>((context, existingService) => 
                interceptorFactory(context.Resolve<IServiceLocator>(), existingService), typeof(T));
        }

        public IServiceConfigurationProvider RemoveAll(Type serviceType)
        {
            //Not so elegant, perhaps exist a cleaner way??
            var container = _containerBuilder.Build();

            var newBuilder = new ContainerBuilder();
            var components = container.ComponentRegistry.Registrations
                    .Where(cr => cr.Activator.LimitType != typeof(LifetimeScope))
                    .Where(cr => cr.Activator.LimitType != serviceType);
            foreach (var c in components)
            {
                newBuilder.RegisterComponent(c);
            }
            _containerBuilder = newBuilder;

            _registeredTypes.Remove(serviceType);
            return this;
        }
    }
}
