using DependencyInjection;
using KLogger;
using Koober.Attributes;
using KWeb;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Koober.MySql
{
    public abstract class KooberMySql<T, ID> : KooberBase<T, ID>
    {
        public KooberMySql() : base() 
        {
            SqlType = "MySql";
            connection = (DbConnection)ServiceProvider.Get(typeof(DbConnection), SqlType);
        }

        public override int NonQuery(string command, params object[] values)
        {
            int res = 0;
            connection.Open();
            DbTransaction transaction = connection.BeginTransaction();
            using (DbCommand cmd = connection.CreateCommand())
            {
                try
                {
                    cmd.CommandText = command;
                    int index = command.IndexOf('@');
                    if (index > 0)
                    {
                        string[] paramters = command.Split(',', ' ', '=');
                        for (int i = 0; i < paramters.Length; i++)
                        {
                            AddParamter(cmd, paramters[i], values[i]);
                        }
                    }
                    logger.Trace(SqlLogText(cmd));
                    res = cmd.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message + ":" + ex.StackTrace);
                    transaction.Rollback();
                }
            }
            transaction.Dispose();
            connection.Close(); 
            return res;
        }

        private object[] ObjectToArray<Q>(Q queryObj)
        {
            Type paramType = typeof(Q);
            PropertyInfo[] properties = paramType.GetProperties();
            object[] values = new object[properties.Length];
            for (int i = 0; i < properties.Length; i++)
                values[i] = properties[i].GetValue(queryObj);
            return values;
        }

        public override int NonQuery<Q>(string command, Q query)
        {
            return NonQuery(command,ObjectToArray(query));
        }

        public override List<T> Query(string command, params object[] values)
        {
            Type type = typeof(T);
            List<T> res = new List<T>();
            connection.Open();
            using (DbCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = command;
                int index = command.IndexOf('@');
                if (index > 0)
                {
                    string[] paramters = command.Split(',', ' ', '=').Where(s=>s.Contains('@')).ToArray();
                    for(int i = 0; i < paramters.Length; i++)
                    {
                        AddParamter(cmd, paramters[i], values[i]);
                    }
                }
                logger.Trace(SqlLogText(cmd));
                using(DbDataReader reader = cmd.ExecuteReader())
                {
                    if(reader.Depth > 1)
                    {
                        MultiContentException exception = new MultiContentException();
                        logger.Warn($"{exception.Message}: {exception.StackTrace}");
                        throw exception;
                    }
                    while (reader.Read())
                    {   
                        T obj = (T)Activator.CreateInstance(type);
                        ReadSingle(type.GetProperties(BindingFlags.Instance | BindingFlags.Public), reader, obj);
                        res.Add(obj);
                    }
                    
                }
            }
            connection.Close();
            return res;
        }

        public override List<T> Query<Q>(string command, Q query)
        {
            return Query(command,ObjectToArray(query));
        }

        public override List<TR> Select<TR>(string command, params object[] values)
        {
            return Select<TR>(command, null, values);
        }

        public override List<TR> Select<TR>(string command, Func<T, TR> result, params object[] values)
        {   
            List<T> res = Query(command, values);
            if(result == null)
            {
                List<TR> trRes = new List<TR>();
                foreach(T item in res)
                {
                  trRes.Add(ObjectUtil.CopyProperties(item,
                        (TR)Activator.CreateInstance(typeof(TR)),BindingFlags.Public | BindingFlags.Instance));
                }
                return trRes;
            }
            return res.Select(result).ToList();
        }
        public override List<TR> Select<TR, Q>(string command, Func<T, TR> result, Q query)
        {
            return Select(command, result, ObjectToArray(query));
        }

        public override T? SingleQuery(string command, params object[] values)
        {
            Type type = typeof(T);
            T? res = default;
            connection.Open();
            using (DbCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = command;
                int index = command.IndexOf('@');
                if (index > 0)
                {
                    string[] paramters = command.Split(',', ' ', '=').Where(s=>s.Contains('@')).ToArray();
                    for (int i = 0; i < paramters.Length; i++)
                    {
                        AddParamter(cmd, paramters[i].Substring(1), values[i]);
                    }
                }
                logger.Trace(SqlLogText(cmd));
                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Depth > 1)
                    {
                        MultiContentException exception = new MultiContentException();
                        logger.Warn($"{exception.Message}: {exception.StackTrace}");
                        throw exception;
                    }
                    if (reader.Read())
                    {
                        res = (T)Activator.CreateInstance(type);
                        ReadSingle(type.GetProperties(BindingFlags.Instance | BindingFlags.Public), reader, res);
                    }
                   
                }
            }
            connection.Close();
            return res;
        }

        public override T? SingleQuery<Q>(string command, Q query)
        {
            return SingleQuery(command,ObjectToArray(query));
        }
        public override TR? SelectOne<TR>(string command, params object[] values) where TR : default
        {
            return SelectOne<TR>(command, null, values);
        }
        public override TR? SelectOne<TR>(string command, Func<T, TR> result, params object[] values) where TR : default
        {
            T res = SingleQuery(command, values);
            if(result == null)
            {
                return ObjectUtil.CopyProperties(res,
                    (TR)Activator.CreateInstance(typeof(TR)),BindingFlags.Public| BindingFlags.Instance);
            }
            return result(res);
        }
        public override TR? SelectOne<TR, Q>(string command, Q query) where TR : default
        {
            return SelectOne<TR>(command,null,ObjectToArray(query));
        }
        public override TR? SelectOne<TR, Q>(string command, Func<T, TR> result, Q query) where TR : default
        {
            return SelectOne(command,result, ObjectToArray(query));
        }
        public override List<T> Query(PagedQuery<T> page, string command, params object[] values)
        {
            Type type = typeof(T);
            connection.Open();
            using (DbCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = $"{command} limit{(page.Page - 1)*page.Size},{page.Size}";
                int index = command.IndexOf('@');
                if (index > 0)
                {
                    string[] paramters = command.Split(',', ' ', '=').Where(s => s.Contains('@')).ToArray();
                    for (int i = 0; i < paramters.Length; i++)
                    {
                        AddParamter(cmd, paramters[i], values[i]);
                    }
                }
                logger.Trace(SqlLogText(cmd));
                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Depth > 1)
                    {
                        MultiContentException exception = new MultiContentException();
                        logger.Warn($"{exception.Message}: {exception.StackTrace}");
                        throw exception;
                    }
                    while (reader.Read())
                    { 
                        T obj = (T)Activator.CreateInstance(type);
                        ReadSingle(type.GetProperties(BindingFlags.Instance | BindingFlags.Public), reader, obj);
                        page.Data.Add(obj);
                    }
                   
                }
                Table? attribute = type.GetCustomAttribute<Table>();
                string tableName = attribute == null ? type.Name : attribute.Name;
                cmd.CommandText = $"select count(*) from {tableName} where{command.Substring(command.IndexOf("where")+5)}";
                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    reader.Read();
                    page.Total = reader.GetInt64(0);
                }
            }
            connection.Close();
            return page.Data; 
        }

        public override List<T> Query<Q>(PagedQuery<T> page, string command, Q query)
        {
            return Query(page,command,ObjectToArray(query));
        }
    }

    public static class DbExpansion
    {
        public static void AddMySqlConnection(this WebApplication app,Action<MySqlConnectionStringBuilder> builder)
        {
            MySqlConnectionStringBuilder strBuilder = new MySqlConnectionStringBuilder();
            builder(strBuilder);
            app.Services.AddService<DbConnection,MySqlConnection>
                (() => new MySqlConnection(strBuilder.ConnectionString),"MySql");
        }
    }
}
