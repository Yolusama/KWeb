using Koober.Attributes;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using KLogger;
using DependencyInjection;

namespace Koober
{
    public abstract partial class KooberBase<T,ID> : IDisposable
    {
        protected DbConnection connection;
        protected IKLogger logger;
        
        protected KooberBase()
        {
            logger = (IKLogger)ServiceProvider.Get(typeof(IKLogger));
        }
        public string SqlType { get; set; }

        public T? SelectById(ID id)
        {
            T? res = default(T);
            Type type = typeof(T);
            
            connection.Open();
            using (DbCommand command = connection.CreateCommand())
            {
                string[] idTableName = GetIdTableNameStr(command,id);
                command.CommandText = $"select * from {idTableName[1]} where {idTableName[0]}";
                using (DbDataReader reader = command.ExecuteReader())
                {
                    PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance|BindingFlags.Public);
                    res =(T?)Activator.CreateInstance(type);
                    while (reader.Read())
                    {
                        ReadSingle(properties,reader,res);
                    }
                }
            }
            connection.Close();
            return res;
        }
        public List<T> SelectAll()
        {
            Type type = typeof(T);
            List<T> res = new List<T>();
            Table? tableAttr = type.GetCustomAttribute<Table>();
            string tableName = tableAttr == null ? type.Name : tableAttr.Name;
            connection.Open();
            using(DbCommand command = connection.CreateCommand())
            {
                command.CommandText = $"select * from {tableName}";
                logger.Trace(SqlLogText(command));
                using (DbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        T? obj = (T?)Activator.CreateInstance(type);
                        ReadSingle(type.GetProperties(BindingFlags.Instance|BindingFlags.Public), reader, obj);
                        res.Add(obj);
                    }
                }
            }
            connection.Close();
            return res;
        }
        public int Delete(ID id)
        {
            int res = 0;

            connection.Open();
            DbTransaction transaction = connection.BeginTransaction();
            using (DbCommand command = connection.CreateCommand())
            {
                try
                {
                    string[] idTableName = GetIdTableNameStr(command, id);
                    command.CommandText = $"delete from {idTableName[1]} where {idTableName[0]}";
                    logger.Trace(SqlLogText(command));
                    res = command.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    logger.Error(ex.ToString() + ":" + ex.StackTrace);
                    transaction.Rollback();
                }

            }
            transaction.Dispose();
            connection.Close();
            return res;
        }
        public int Update(T entity)
        {
            int res = 0;
            PropertyInfo idPro = GetIdProperty();
            connection.Open();
            DbTransaction transaction = connection.BeginTransaction();
            using(DbCommand command = connection.CreateCommand())
            {
                try
                {
                    string[] idTableStr = GetIdTableNameStr(command, (ID)idPro.GetValue(entity));
                    command.CommandText = 
                        $"update {idTableStr[1]} set {GetUpdateStr(command,entity)} where {idTableStr[0]}";
                    logger.Trace(SqlLogText(command));
                    res = command.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    logger.Error(ex.ToString() + ":" + ex.StackTrace);
                    transaction.Rollback();
                }
            }

            transaction.Dispose();
            connection.Close();

            return res;
        }

        public int Insert(T entity)
        {
            int res = 0;
            Type type = typeof(T);
            connection.Open();
            DbTransaction transaction = connection.BeginTransaction();
            using( DbCommand command = connection.CreateCommand())
            {
                try
                {
                    Table? tableAttr = type.GetCustomAttribute<Table>();
                    string tableName = tableAttr != null ? tableAttr.Name : type.Name;
                    command.CommandText = $"insert into {tableName} values {GetInsertStr(command,entity)}";
                    logger.Trace(SqlLogText(command));
                    res = command.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch(Exception ex)
                {
                    logger.Error(ex.Message + ":" + ex.StackTrace);
                    transaction.Rollback();
                }
            }
            transaction.Dispose();
            connection.Close();
            return res;
        }

        public int Delete(T entity)
        {
            int res = 0;

            connection.Open();
            DbTransaction transaction = connection.BeginTransaction();
            using (DbCommand command = connection.CreateCommand())
            {
                try
                {
                    PropertyInfo idPro = GetIdProperty();
                    string[] idTableName = GetIdTableNameStr(command, (ID)idPro.GetValue(entity));
                    command.CommandText = $"delete from {idTableName[1]} where {idTableName[0]}";
                    logger.Trace(SqlLogText(command));
                    res = command.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    logger.Error(ex.ToString() + ":" + ex.StackTrace);
                    transaction.Rollback();
                }

            }
            transaction.Dispose();
            connection.Close();
            return res;
        }
        public int BatchInsert(params T[] entities)
        {
            int res = 0;
            Type type = typeof(T);
            connection.Open();
            DbTransaction transaction = connection.BeginTransaction();
            using (DbCommand command = connection.CreateCommand())
            {
                try
                {
                    Table? tableAttr = type.GetCustomAttribute<Table>();
                    string tableName = tableAttr != null ? tableAttr.Name : type.Name;
                    StringBuilder builder = new StringBuilder();
                    foreach (T entity in entities)
                    {
                        builder.Append($"{GetInsertStr(command, entity)},");
                    }
                    builder.Remove(builder.Length - 1, 1);
                    command.CommandText = $"insert into {tableName} values {builder}";
                    logger.Trace(SqlLogText(command));
                    res = command.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    logger.Error(ex.ToString() + ":" + ex.StackTrace);
                    transaction.Rollback();
                }
            }
            transaction.Dispose();
            connection.Close();
            return res;
        }

        public abstract T? SingleQuery(string command, params object[] values);
        public abstract List<T> Query(string command, params object[] values);
        public abstract int NonQuery(string command, params object[] values);
        public abstract List<T> Query(PagedQuery<T> page,string command, params object[] values);

        public abstract T? SingleQuery<Q>(string command,Q query);
        public abstract List<T> Query<Q>(string command,Q query);
        public abstract int NonQuery<Q>(string command,Q query);
        public abstract List<T> Query<Q>(PagedQuery<T> page, string command, Q query);

        public void Dispose()
        {
            connection.Dispose();
        }
    }
}
