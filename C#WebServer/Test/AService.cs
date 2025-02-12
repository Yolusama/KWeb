using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    [ServiceInjection]
    public class AConfig
    {
        public string Name { get; set; } = "RandomString:";
        public string Config()
        {
            return "ATestConfig";
        }
    }
    public class AService
    {
        private string[] table = new string[]
        {
            "0123456789","abcdefghijklmnopqrstuvxyz","ABCDEFGHIJKLMNOPQRSTUVXYZ"
        };
        [ServiceInjection]
        private readonly AConfig config;
        public string RandomString()
        {
            StringBuilder builder = new StringBuilder(config.Name);
            int count = Random.Shared.Next(5, 26);
            for(int i = 0; i < count; i++)
            {
                int tableIndex = Random.Shared.Next(0,table.Length);
                builder.Append(table[tableIndex][Random.Shared.Next(0, table[tableIndex].Length)]);
            }
            return builder.ToString();
        }
        
    }
}
