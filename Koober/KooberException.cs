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
    public class NotIdException : Exception
    {
        public NotIdException() : base("非主键列") { }
    }

    public class MultiContentException : Exception
    {
        public MultiContentException() : base("结果集的容量大于1！") { }
    }
        
    public class UnTransformableException : Exception
    {
        public UnTransformableException() : base("该C#类型无法直接转换为数据库表列的对应类型") { }
    }
}
