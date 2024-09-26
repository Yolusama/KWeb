using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjection
{
    public enum InjectionType
    {
        Single = 1,Scoped = 2
    }
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Class)]
    public class ServiceInjection : Attribute
    {
        public string Name { get; set; }
        public InjectionType Type { get; set; }
        public ServiceInjection(string name = "", InjectionType type = InjectionType.Scoped)
        {
            Name = name;
            Type = type;
        }
    }

    public class ServiceExistedException : Exception
    {
        public ServiceExistedException() : base("该服务已被注册了！") { }
    }

    public class NotImplementionException : Exception
    {
        public NotImplementionException() : base("注入的实际类型不是注入接口/抽象类类型的实现类") { }
    }
}
