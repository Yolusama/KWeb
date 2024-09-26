using Koober.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Koober
{
    public abstract partial class KooberBase<T,ID> : IDisposable
    {
        protected string[] GetIdTableNameStr(DbCommand command,ID id)
        {
            Type type = typeof(T);
            Table? tableAttribute = type.GetCustomAttribute<Table>();
            StringBuilder idStrBuider = new StringBuilder();
            string tableName = tableAttribute == null ? type.Name : tableAttribute.Name;
            Type idType = typeof(ID);
            ComboId? comboIDAttribute = idType.GetCustomAttribute<ComboId>();
            if (comboIDAttribute == null)
            {
                PropertyInfo idPro = GetIdProperty();
                string idName = GetColumnName(idPro);
                idStrBuider.Append($"{idName} = @id");
                AddParamter(command, idName, id);
            }
            else
            {
                FieldInfo[] fields = idType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (FieldInfo field in fields)
                {
                    idStrBuider.Append($"{field.Name} = @{field.Name} and ");
                    AddParamter(command,field.Name,field.GetValue(id));
                }
                idStrBuider.Remove(idStrBuider.Length - 5, 5);
            }
            return new string[] { idStrBuider.ToString(), tableName };
        }

        protected void ReadSingle(PropertyInfo[] properties,DbDataReader reader,T target)
        {
            foreach (PropertyInfo property in properties)
            {
                string columnName = GetColumnName(property);
                property.SetValue(target,
                    TransformValue(property.PropertyType, reader.GetValue(columnName)));
            }
        }

        protected void AddParamter(DbCommand command,string name,object value)
        {
            DbParameter parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }

        protected string GetUpdateStr(DbCommand command,T entity)
        {
            PropertyInfo[] properties = entity.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            StringBuilder builder = new StringBuilder();
            foreach (PropertyInfo property in properties)
            {
                string columnName = GetColumnName(property);
                Id? idAttr = property.GetCustomAttribute<Id>();
                if (idAttr != null) continue;
                builder.Append($"{columnName}=@{property.Name},");
                AddParamter(command, property.Name, property.GetValue(entity));
            }
            builder.Remove(builder.Length - 1, 1);
            return builder.ToString();
        }

        protected string SqlLogText(DbCommand command)
        {
            return $"{SqlType} command excutes,sql sentence:{command.CommandText}.";
        }

        protected PropertyInfo GetIdProperty()
        {
            Type type = typeof(T);
            IEnumerable<PropertyInfo> idPros = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.GetCustomAttribute<Id>() != null || p.GetCustomAttribute<ComboId>() != null);
            Exception? exception = null;
            if (idPros.Count() == 0)
            {
                exception = new NoneIdException();
                logger.Fatal(exception.Message + ':' + exception.StackTrace.ToString());
                throw exception;
            }
            if (idPros.Count() > 1)
            {
                exception = new MultiIdException();
                logger.Fatal(exception.Message + ':' + exception.StackTrace.ToString());
                throw exception;
            }
            return idPros.First();
        }

        protected string GetInsertStr(DbCommand command,T entity)
        {
            StringBuilder builder = new StringBuilder();
            PropertyInfo[] properties = entity.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            builder.Append('(');
            foreach (PropertyInfo property in properties)
            {
                Id? idAttr = property.GetCustomAttribute<Id>();
                if (idAttr != null && idAttr.Type == IdType.Auto)
                {
                    builder.Append("null,");
                    continue;
                }
                builder.Append($"@{property.Name},");
                AddParamter(command,property.Name,property.GetValue(entity));
            }
            builder.Remove(builder.Length - 1, 1);
            builder.Append(')');
            return builder.ToString();
        }

        protected string GetColumnName(PropertyInfo property)
        {
            Column? columnAttr = property.GetCustomAttribute<Column>();
            string columnName = columnAttr == null || columnAttr.Name == "" ?
                property.Name : columnAttr.Name;
            return columnName;
        }
        protected virtual object TransformValue(Type type,object value)
        {
            if (type == typeof(DateTime))
            {
                return (DateTime)value;
            }
            else if (type == typeof(DateOnly))
            {
                return (DateOnly)value;
            }
            else if (type == typeof(bool))
            {
                if (value.GetType() != typeof(bool))
                {
                    int val = (int)value;
                    if (val == 1)
                    {
                        return true;
                    }
                    else return false;
                }
                else return value;
            }
            return value;
        }
    }
}
