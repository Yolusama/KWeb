using DependencyInjection;
using KLogger;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koober
{
    public abstract class DbTableBuilder<T> : IDisposable
    {
        protected DbConnection connection;
        protected readonly List<DbTableColumn<T>> columns;
        protected readonly List<DbIndex<T>> indexes;
        public DbTableBuilder()
        {
            columns = new List<DbTableColumn<T>>();
            indexes = new List<DbIndex<T>>();
        }
        public DbTableColumnBuilder<T> Column()
        {
            DbTableColumnBuilder<T> tableColumnBuilder = new DbTableColumnBuilder<T>();
            columns.Add(tableColumnBuilder.Build());
            return tableColumnBuilder;
        }
        public DbIndexBuilder<T> Index()
        {
            DbIndexBuilder<T> indexBuilder = new DbIndexBuilder<T>();
            indexes.Add(indexBuilder.Build());
            return indexBuilder;
        }
        public abstract void Build(string tableName = "");
        public void Dispose()
        {
            connection.Dispose();
        }
    }
}
