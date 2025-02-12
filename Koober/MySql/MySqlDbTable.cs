using DependencyInjection;
using KLogger;
using Koober.Attributes;
using KWeb;
using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Reflection;
using System.Text;
namespace Koober.MySql
{
    using DbIndex = Attributes.DbIndex;
    public class MySqlDbTable : DbTable
    {
        public MySqlDbTable()
        {
            connection = ServiceProvider.Get<MySqlConnection>("MySql");
        }
        public override void CreateTable(Type type)
        {
            Table? tableAttr = type.GetCustomAttribute<Table>();
            if (tableAttr == null)
            {
                throw new NoTableAttrSignedException();
            }
            connection.Open();
            using (DbCommand command = connection.CreateCommand())
            {
                try
                {
                    command.CommandText = $"SHOW TABLES LIKE '{tableAttr.Name}'";
                    using (DbDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (reader.GetValue(0) != null) return;
                        }
                    }
                    StringBuilder builder = new StringBuilder($"create table {tableAttr.Name}(");
                    PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetCustomAttribute<ComboId>() == null)
                            builder.Append($"{GetColumnString(property)},");
                        else
                        {
                            foreach (var pro in property.PropertyType.GetProperties())
                                builder.Append($"{GetColumnString(pro)},");
                        }
                    }
                    builder.Remove(builder.Length - 1, 1);
                    builder.Append(");");
                    IEnumerable<PropertyInfo> idPros = properties.Where(p => p.GetCustomAttribute<Id>() != null
                    || p.GetCustomAttribute<ComboId>() != null);
                    if(idPros.Count()==0)
                    {  
                        throw new NoneIdException();
                    }
                    if (idPros.Count() > 2)
                    {
                        throw new MultiIdException();
                    }
                    PropertyInfo idPro = idPros.First();
                    if (idPro.GetCustomAttribute<Id>() != null)
                    {
                        string columnStr = GetColumnString(idPro);
                        Id id = idPro.GetCustomAttribute<Id>();
                        string columnName = columnStr.Substring(0, columnStr.IndexOf(' '));
                        if (id.Type == IdType.Auto)
                            builder.Append(
                              $"alter table {tableAttr.Name} modify column {columnStr} auto_increment,add primary key({columnName});");
                        else
                            builder.Append($"alter table {tableAttr.Name} add primary key({columnName})");
                    }
                    else
                    {
                        StringBuilder idStrBuilder = new StringBuilder();
                        foreach (PropertyInfo property in idPro.
                            PropertyType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                        {
                            string columnStr = GetColumnString(property);
                            idStrBuilder.Append($"{columnStr.Substring(columnStr.IndexOf(' '))},");                          
                        }
                        idStrBuilder.Remove(idStrBuilder.Length - 1, 1);
                            
                        builder.Append($"alter table {tableAttr.Name} add primary key({idStrBuilder.ToString()});");
                    }
                    IEnumerable<PropertyInfo> indexPros = properties.Where(p => p.GetCustomAttribute<DbIndex>() != null);
                    foreach (PropertyInfo indexPro in indexPros)
                    {
                        DbIndex dbIndex = indexPro.GetCustomAttribute<DbIndex>();
                        if (indexPro.GetCustomAttribute<Combo>() == null)
                        {
                            string columnName = GetColumnString(indexPro);
                            string indexName = dbIndex.Name == "" ? $"index_{columnName}" : dbIndex.Name;
                            if (dbIndex.Type == DbIndexType.Normal)
                                builder.Append($"alter table {tableAttr.Name} add index {indexName}({columnName});");
                            else
                                builder.Append($"alter table {tableAttr.Name} add " +
                                    $"{dbIndex.Type.ToString().ToLower()} index {indexName}({columnName});");
                        }
                        else
                        {
                            string indexName;
                            StringBuilder indexStrBuilder = new StringBuilder("index_");
                            PropertyInfo[] colPros = indexPro.PropertyType.GetProperties(
                                    BindingFlags.Instance | BindingFlags.Public);
                            if (dbIndex.Name == "")
                            {
                                foreach (PropertyInfo colPro in colPros)
                                {
                                    string colName = GetColumnString(colPro);
                                    indexStrBuilder.Append($"{colName}_");
                                }
                                indexStrBuilder.Remove(indexStrBuilder.Length - 1, 1);
                                indexName = indexStrBuilder.ToString();
                            }
                            else
                            {
                                indexName = dbIndex.Name;
                            }
                            indexStrBuilder.Clear();
                            foreach (PropertyInfo colPro in colPros)
                            {
                                indexStrBuilder.Append($"{GetColumnString(colPro)},");
                            }
                            indexStrBuilder.Remove(indexStrBuilder.Length - 1, 1);
                            if (dbIndex.Type == DbIndexType.Normal)
                                builder.Append($"alter table {tableAttr.Name} add index {indexName}({indexStrBuilder});");
                            else
                                builder.Append($"alter table {tableAttr.Name} add {dbIndex.Type.ToString().ToLower()}" +
                                    $" index {indexName}({indexStrBuilder});");
                        }
                    }
                    command.CommandText = builder.ToString();
                    command.ExecuteNonQuery();
                }
                catch(Exception ex)
                {
                    Console.Error.WriteLine(ex.ToString());
                }
            }
            connection.Close();
            Dispose();  
        }

    }

        public class NoTableAttrSignedException : Exception
        {
            public NoTableAttrSignedException() : base("未使用Table特性标记无法加载此实体类！") { }
        }

    public static partial class DbExpansion
    {
        public static void CreateTables(this WebApplication app,params Type[] entityTypes)
        {
            using DbTable table = new MySqlDbTable();
            foreach(Type type in entityTypes)
            {
                table.CreateTable(type);
            }
        }
    }
}
