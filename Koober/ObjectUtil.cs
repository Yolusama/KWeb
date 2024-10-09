using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Koober
{
    public static class ObjectUtil
    {
        public static TR CopyProperties<T,TR>(T src,TR to,BindingFlags flags)
        {
            Type srcType = typeof(T),toType = typeof(TR);
            var properties = toType.GetProperties(flags);
            foreach (var property in properties)
            {
                if (srcType.Equals(toType))
                    property.SetValue(to, property.GetValue(src));
                else
                {
                    PropertyInfo? srcPro = srcType.GetProperty(property.Name, flags);
                    if (srcPro == null) continue;
                    property.SetValue(to,srcPro.GetValue(src));
                }
            }
            return to;
        }

        public static TR CopyFields<T,TR>(T src,TR to,BindingFlags flags)
        {
            Type srcType = typeof(T), toType = typeof(TR);
            var fileds = toType.GetFields(flags);
            foreach (var field in fileds)
            {
                if (srcType.Equals(toType))
                    field.SetValue(to,field.GetValue(src));
                else
                {
                    PropertyInfo? srcPro = srcType.GetProperty(field.Name, flags);
                    if (srcPro == null) continue;
                    field.SetValue(to, srcPro.GetValue(src));
                }
            }
            return to;
        }
    }
}
