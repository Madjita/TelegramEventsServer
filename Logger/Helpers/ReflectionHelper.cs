using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Logger.Helpers
{
    public static class ReflectionHelper
    {
        public static IEnumerable<Type> GetTypesImplementingInterface(Type interfaceType)
        {
            return Assembly.GetAssembly(interfaceType)
                .GetTypes()
                .Where(t => interfaceType.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);
        }
    }
}
