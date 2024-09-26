using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koober
{
    public class MultiIdException : Exception
    {
        public MultiIdException() : base("Id重复！") { }
    }

    public class NoneIdException : Exception
    {
        public NoneIdException() : base("未设置主键Id") { }
    }

    public class MultiContentException : Exception
    {
        public MultiContentException() : base("结果集的容量大于1！") { }
    }
        
}
