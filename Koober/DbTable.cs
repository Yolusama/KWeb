using DependencyInjection;
using KLogger;
using Koober.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Koober
{
    public abstract class DbTable : IDisposable
    {
        protected DbConnection connection;

        public abstract void CreateTable(Type type);

        protected virtual string BaseTypeToDbType(Type type)
        {
            if (type == typeof(string))
                return "varchar(50)";
            else if (type == typeof(bool))
                return "tinyint(1)";
            else if (type == typeof(DateTime))
                return "datetime";
            else if (type == typeof(DateOnly))
                return "date";
            else if (type.IsEnum)
            {
                EnumConvertion? enumAttr = type.GetCustomAttribute<EnumConvertion>();
                return enumAttr == null ? "int" : "varchar(25)";
            }
            else if (type == typeof(long))
                return "bigint";
            else if (type == typeof(short))
                return "small int";
            else if(type == typeof(int))
                return "int";
            return "";
        }
        protected virtual string GetColumnString(PropertyInfo property)
        {
            Column? column = property.GetCustomAttribute<Column>();
            if (column == null)
                return $"{property.Name} {BaseTypeToDbType(property.PropertyType)}";
            else
            {
                if (column.Type == null)
                    return $"{column.Name} {BaseTypeToDbType(property.PropertyType)}";
                else
                    return $"{column.Name} {column.Type}";
            }
        }

        public void Dispose()
        {
           connection.Dispose();
        }
    }

   
    public interface IDbTableConfigurator
    {
        public DbTableBuilder<T> TableBuilder<T>();
    }
}