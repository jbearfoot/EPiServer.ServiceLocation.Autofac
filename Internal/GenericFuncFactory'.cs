using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace EPiServer.ServiceLocation.Autofac.Internal
{
    internal static class FuncFactory
    {
        [DebuggerStepThrough]
        public static Func<IServiceLocator, TService> Get<TService>(Func<IServiceLocator, object> untyped)
        {
            return (s) => (TService)untyped(s);
        }
    }
}
