using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    [ServiceInjection]
    public class BService
    {
        public AConfig config { get; }

        [ServiceInjection]
        public BService(AConfig config) 
        {
            this.config = config;   
        }

        public string Info()
        {
            return config.Config();
        }
    }
}
