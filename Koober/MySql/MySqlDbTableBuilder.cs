using DependencyInjection;
using KLogger;
using Koober.Attributes;
using KWeb;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Koober.MySql
{ 
    public class MySqlDbTableBuilder<T> : DbTableBuilder<T>
    {
        public MySqlDbTableBuilder()
        { 
            connection = ServiceProvider.Get<DbConnection>("MySql"); 
        }
       
        public override void Build(string tableName="")
        {
            Type type = typeof(T);
            string _tableName = tableName == ""? type.Name : tableName;
            try
            {
                connection.Open();
                using(DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = $"SHOW TABLES LIKE '{_tableName}'";
                    using(DbDataReader reader = command.ExecuteReader())
                    {
                        if(reader.Read())
                        { 
                            if (reader.GetValue(0) != null) return;
                        }
                    }
                    StringBuilder commandStrBuilder = new StringBuilder($"create table {_tableName}(");
                    foreach(DbTableColumn<T> column in columns)
                    {
                        StringBuilder attachStr = new StringBuilder(" ");
                        if(column.Comment != null)
                            attachStr.Append($"comment '{column.Comment}' ");
                        if (column.IsKey)
                        {
                            if (column.AutoIncresing)
                                attachStr.Append($"primary key auto_increment ");
                            else
                                attachStr.Append("primary key ");
                        }
                        else
                        {
                            if (column.NotNull)
                                attachStr.Append("not null ");
                        }
                        commandStrBuilder.AppendLine($"{column.Name} {column.Type}{attachStr.ToString().TrimEnd()},");
                    }
                    commandStrBuilder.Remove(commandStrBuilder.ToString().LastIndexOf(','),1).AppendLine(");");
                    foreach(DbIndex<T> index in indexes)
                    {
                        StringBuilder columnStr = new StringBuilder();
                        foreach (PropertyInfo property in index.ReflectionType.GetProperties())
                        {
                            columnStr.Append($"{property.Name},");
                        }
                        if (index.Type == DbIndexType.Normal)
                            commandStrBuilder.AppendLine($"alter table {_tableName} add index {index.Name}" +
                                $"({columnStr.ToString().TrimEnd(',')});");
                        else
                            commandStrBuilder.AppendLine($"alter table {_tableName} add {index.Type.ToString().ToLower()} index " +
                                $"{index.Name}({columnStr.ToString().TrimEnd(',')});");
                    }
                    command.CommandText = commandStrBuilder.ToString();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
        }

    }
    public class MySqlDbTableConfigurator : IDbTableConfigurator
    {
        public DbTableBuilder<T> TableBuilder<T>()
        {
            return new MySqlDbTableBuilder<T>();
        }
    }

    public static partial class DbExpansion
    {
        public static void CreateTables(this WebApplication app,Action<MySqlDbTableConfigurator> configurator)
        {
            configurator(new MySqlDbTableConfigurator());
        }
    }

}
