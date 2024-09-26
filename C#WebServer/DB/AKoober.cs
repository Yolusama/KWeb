using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB
{
    [ServiceInjection]
    public class AKoober : KooberMySql<A,int>
    {
       public List<A> GetAll()
        {
            return SelectAll();
        }
    }
}
