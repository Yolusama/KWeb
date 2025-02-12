using Koober.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
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
                PropertyInfo[] properties = idType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (PropertyInfo property in properties)
                {
                    idStrBuider.Append($"{property.Name} = @{property.Name} and ");
                    AddParamter(command,property.Name,property.GetValue(id));
                }
                idStrBuider.Remove(idStrBuider.Length - 5, 5);
            }
            return new string[] { idStrBuider.ToString(), tableName };
        }

        protected void ReadSingle<TI>(PropertyInfo[] properties,DbDataReader reader,TI target)
        {
            foreach (PropertyInfo property in properties)
            {
                string columnName = GetColumnName(property);
                if(reader.IsDBNull(columnName)) continue;
                object value;
                if (property.GetCustomAttribute<Combo>() == null)
                    value = reader.GetValue(columnName);
                else
                {
                    Type comboType = property.PropertyType;
                    object comboValue = Activator.CreateInstance(comboType);
                    foreach(PropertyInfo pro in comboType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        string columnNameCombo = GetColumnName(pro);
                        property.SetValue(comboType,
                    TransformValue(property.PropertyType, reader.GetValue(columnNameCombo)));
                    }
                    value = comboValue;
                }
                property.SetValue(target,
                    TransformValue(property.PropertyType, value));
            }
        }

        protected void AddParamter(DbCommand command,string name,object value)
        {
            DbParameter parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            if(!command.Parameters.Contains(parameter))
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
                object value = property.GetValue(entity); ;
                if (property.GetCustomAttribute<Combo>() != null)
                {
                    Type comboType = property.PropertyType;
                    foreach (PropertyInfo pro in comboType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    { 
                        string comboColumnName = GetColumnName(pro);
                        builder.Append($"{comboColumnName}=@{pro.Name},");
                        AddParamter(command, pro.Name, pro.GetValue(value));
                    }
                }
                else
                    builder.Append($"{columnName}=@{property.Name},");
                AddParamter(command, property.Name, value);
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
                logger.Fatal(exception.ToString());
                throw exception;
            }
            if (idPros.Count() > 1)
            {
                exception = new MultiIdException();
                logger.Fatal(exception.ToString());
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
                object value = property.GetValue(entity);;
                if (property.GetCustomAttribute<Combo>() != null)
                {
                    Type comboType = property.PropertyType;
                    foreach (PropertyInfo pro in comboType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        builder.Append($"@{pro.Name},");
                        AddParamter(command, pro.Name, pro.GetValue(value));
                    }
                }
                else
                {
                    builder.Append($"@{property.Name},");
                    AddParamter(command, property.Name, value);
                }
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
