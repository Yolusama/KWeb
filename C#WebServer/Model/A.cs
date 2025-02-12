using Koober;
using Koober.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class A
    {
        [Id(Type=IdType.Auto)]
        public int Id {  get; set; }
        public string Name { get; set; }
    }
}
