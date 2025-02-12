using Koober.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Koober
{
    public class DbIndex<T>
    {
        private DbIndexType type = DbIndexType.Normal;
        private string name;
        private Type reflectionType;
        public string Name => name;
        public DbIndexType Type => type;
        public Type ReflectionType => reflectionType;
        public virtual DbIndex<T> WithProperty<TS>(Func<T, TS> selector)
        {
            Type type = typeof(TS);
            reflectionType = type;
            if (Name == null)
            {
                PropertyInfo[] properties = type.GetProperties();
                StringBuilder builder = new StringBuilder("index_");
                foreach (PropertyInfo property in properties)
                    builder.Append($"{property.Name}_");
                builder.Remove(builder.Length - 1, 1);
                return WithName(builder.ToString());
            }
            return this;
        }
        public DbIndex<T> IsUnique()
        {
            type = DbIndexType.Unique;
            return this;
        }
        public DbIndex<T> IsFullText()
        {
            type = DbIndexType.FullText;
            return this;
        }
        public DbIndex<T> IsSpatial()
        {
            type = DbIndexType.Spatial;
            return this;
        }

        public DbIndex<T> WithName(string name)
        {
            this.name = name;
            return this;
        }
    }

    public class DbIndexBuilder<T>
    {
        private readonly DbIndex<T> index = new DbIndex<T>();
        public virtual DbIndexBuilder<T> WithProperty<TS>(Func<T, TS> selector)
        {
            index.WithProperty(selector);
            return this;
        }
        public DbIndexBuilder<T> IsUnique()
        {
            index.IsUnique();
            return this;
        }
        public DbIndexBuilder<T> IsFullText()
        {
            index.IsFullText();
            return this;
        }
        public DbIndexBuilder<T> IsSpatial()
        {
            index.IsSpatial();
            return this;
        }

        public DbIndexBuilder<T> WithName(string name)
        {
            index.WithName(name);
            return this;
        }
        public DbIndex<T> Build()
        {
            return index;
        }
    }
}
