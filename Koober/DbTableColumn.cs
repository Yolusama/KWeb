using Koober.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Koober
{
   
    public class DbTableColumn<T>
    {
        private string name;
        private string comment;
        private string type;
        private bool notNull;
        private bool isKey;
        private bool autoIncresing;

        public string Name => name;
        public string Comment => comment;
        public string Type => type;
        public bool NotNull => notNull;
        public bool IsKey => isKey;
        public bool AutoIncresing 
        {
            get 
            { 
                if(isKey)
                  return autoIncresing;
                throw new NotIdException();
            }
        }
        public DbTableColumn<T> WithProperty<TS>(Func<T,TS> selector)
        {
            Type type = typeof(TS);
            PropertyInfo property = type.GetProperties()[0];
            if(name == null)
               name = property.Name;
            if(this.type == null)
            {
                this.type = BaseTypeToDbType(property.PropertyType);
            }
            return this;
        }
        public DbTableColumn<T> WithName(string name)
        {
            this.name = name;
            return this;
        }
        public DbTableColumn<T> HasComment(string comment)
        {
            this.comment = comment;
            return this;
        }
        public DbTableColumn<T> IsPrimaryKey()
        {
            isKey = true;
            return this;
        }
        public DbTableColumn<T> IsAutoIncresing()
        {
            if (isKey)
            {
                autoIncresing = true;
                return this;
            }
            throw new NotIdException();
        }
        public DbTableColumn<T> Required()
        {
            notNull = true;
            return this;
        }
        public DbTableColumn<T> DbType(string type)
        {
            this.type = type;
            return this;
        }
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
                return "smallint";
            else if (type == typeof(int))
                return "int";
            throw new UnTransformableException();
        }
    }

    public class DbTableColumnBuilder<T>
    {
        private readonly DbTableColumn<T> column = new DbTableColumn<T>();
        public virtual DbTableColumnBuilder<T> WithProperty<TS>(Func<T, TS> selector)
        {
            column.WithProperty(selector);
            return this;
        }
        
        public virtual DbTableColumnBuilder<T> WithName(string name)
        {
            column.WithName(name);
            return this;
        }
        public virtual DbTableColumnBuilder<T> HasComment(string comment)
        {
            column.HasComment(comment);
            return this;
        }
        public virtual DbTableColumnBuilder<T> IsPrimaryKey()
        {
            column.IsPrimaryKey();
            return this;
        }
        public virtual DbTableColumnBuilder<T> IsAutoIncresing()
        {
            if (column.IsKey)
            {
                column.IsAutoIncresing();
                return this;
            }
            throw new NotIdException();
        }
        public virtual DbTableColumnBuilder<T> Required()
        {
            column.Required();
            return this;
        }
        public virtual DbTableColumnBuilder<T> DbType(string type)
        {
            column.DbType(type);
            return this;
        }
        public virtual DbTableColumn<T> Build()
        {
            return column;
        }
    }
}
