using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controller
{
    [Route("/Common")]
    public class CommonController : RestController
    {
        [HttpGet]
        public ActionResult<string> HeartBeat()
        {
            return OK<string>("芝士心跳请求...");
        }
    }
}
