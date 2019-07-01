using Basic.Generic.Enum.Pager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic.Generic.Interface.Pager
{
    public interface IPagerQuery : IPaging, ISorting, IFilterable
    {

    }
}
