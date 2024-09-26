using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koober.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class Table : Attribute
    {
        public string Name { get; set; }
        public Table(string name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class Column : Attribute
    {
        public string Name { get; set; }
        public string Type { get;set;}
        public Column(string name = "")
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class ComboId : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Property)]
    public class Id : Attribute
    {
        public IdType Type { get; set; }
        public Id() { }
    }
}
