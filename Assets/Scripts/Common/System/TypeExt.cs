using System.Linq;
using System.Collections.Generic;

namespace System
{
    public static class TypeExt
    {
        public static IEnumerable<Type> GetFilteredTypeList(this Type type)
        {
            return type.Assembly.GetTypes()
                .Where(x => !x.IsAbstract)
                .Where(x => !x.IsGenericTypeDefinition)
                .Where(x => type.IsAssignableFrom(x));
        }
    }
}