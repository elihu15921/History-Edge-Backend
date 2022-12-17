using Lib.Common.Components.Agreements;
using System;
using System.Reflection;

namespace Edge.Zeus.Panels
{
    internal class SimpleFactory
    {
        internal static IConstruction BuildService(string service) => (IConstruction)Activator.CreateInstance(Assembly.LoadFrom(AppDomain.CurrentDomain.BaseDirectory + service.Split(',')[1]).GetType(service.Split(',')[0]));
    }
}
