using Autofac;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPiServer.ServiceLocation
{
    public static class ServiceLocatorExtensions
    {
        public static IContainer Container(this IServiceLocator locator)
        {
            return (locator as Autofac.AutofacServiceLocator).Container;
        }
    }
}
