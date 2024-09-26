using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koober
{
    public class PagedQuery<T>
    {
        private PagedQuery() { }
        public List<T> Data { get;private set; } = new List<T>();
        public long Total {  get; set; }

        public int Page {  get; set; }
        public int Size { get; set; }

        public static PagedQuery<T> Create(int Page,int size)
        {
            return new PagedQuery<T>
            {
                Page = Page,
                Size = size
            };
        }
    }
}
