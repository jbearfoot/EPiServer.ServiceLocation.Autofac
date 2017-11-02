using Autofac;
using Autofac.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPiServer.ServiceLocation.Autofac
{
    public class AutofacServiceLocator : IServiceLocator
    {
        
        public IContainer Container { get; set; }

        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            Type enumerableOfType = typeof(IEnumerable<>).MakeGenericType(serviceType);
            return (object[])Container.ResolveService(new TypedService(enumerableOfType));
        }

        public object GetInstance(Type serviceType)
        {
            return Container.Resolve(serviceType);
        }

        public TService GetInstance<TService>()
        {
            return Container.Resolve<TService>();
        }

        public object GetService(Type serviceType)
        {
            return GetInstance(serviceType);
        }

        public bool TryGetExistingInstance(Type serviceType, out object instance)
        {
            return Container.TryResolve(serviceType, out instance);
        }
    }
}
