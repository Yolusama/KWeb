using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Koober
{
    public class DbQuery<T>
    {
        public string SqlFragment { get; }

        public DbQuery<T> Eq<E>(Func<T,E> func)
        {
            return this;
        }
    }
}
