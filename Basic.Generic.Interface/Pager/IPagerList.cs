using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic.Generic.Interface.Pager
{
    public interface IPagerList<T>
    {
        List<T> List { get; set; } 

        IPagerQuery PagerQuery { get; set; }
    }
}
