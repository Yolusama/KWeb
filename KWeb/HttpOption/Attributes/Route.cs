using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWeb.HttpOption.Attributes
{
    [AttributeUsage(AttributeTargets.Method|AttributeTargets.Class)]
    public class Route : Attribute
    {
        public Route(string value) { Value = value; }
        public string Value { get; set; }
    }
}
