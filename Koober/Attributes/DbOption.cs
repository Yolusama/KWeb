using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koober.Attributes
{
    public enum DbIndexType
    {
        Normal = 1,Unique = 3,FullText=7,Spatial = 27
    }

    public enum IdType
    {
        Auto = 3, Manual = 4
    }

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
    [AttributeUsage(AttributeTargets.Property)]
    public class Combo : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ComboId : Combo
    {

    }


    [AttributeUsage(AttributeTargets.Property)]
    public class Id : Attribute
    {
        public IdType Type { get; set; }
        public Id() { }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
    public class DbIndex : Attribute
    {
        public DbIndexType Type { get; set; }
        public string Name { get; set; }
        public DbIndex(string name="",DbIndexType type=DbIndexType.Normal) 
        { 
           Name = name;
           Type = type;
        }
    }
    [AttributeUsage(AttributeTargets.Enum)]
    public class EnumConvertion : Attribute
    {
        public EnumConvertion() { }
    }
}
