using Koober.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    [Table("d")]
    public class D
    {
        [Id(Type = IdType.Auto)]
        public int id { get; set; }
        public string info { get; set; }
        public string url {  get; set; }
        public HttpStatusCode statusCode { get; set; }
    }
}
