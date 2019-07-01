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
        public List<T> List { get; set; } = new List<T>();

        public IPagerQuery PagerQuery { get; set; } = new PagerQuery();
    }
}
