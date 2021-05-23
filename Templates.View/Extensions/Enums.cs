using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Templates.View.Models;

namespace Templates.View.Extensions
{
    public static class Enums
    {
        public static IEnumerable<NameValue<T>> ToNameValues<T>() where T: Enum
        {
            return typeof(T)
                .GetFields(BindingFlags.Static | BindingFlags.Public)
                .Select(x => new NameValue<T>
                {
                    Name = x.Name,
                    Value = (T)x.GetValue(null)
                });
        }
    }
}
