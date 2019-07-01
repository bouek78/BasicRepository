using Basic.Generic.Interface.Pager;
using Basic.Generic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic.Generic.Common.Pager
{
    public class PagerList<T> : IPagerList<T> where T : ModelWithId
    {
        public PagerList()
        {
            List = new List<T>();
            PagerQuery = new PagerQuery();
        }

        public PagerList(List<T> lst, IPagerQuery pagerQuery)
        {
            List = lst;
            PagerQuery = pagerQuery;
        }

        public List<T> List { get; set; }

        public IPagerQuery PagerQuery { get; set; } 
    }
}
