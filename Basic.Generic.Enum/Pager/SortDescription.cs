using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic.Generic.Enum.Pager
{
    public class SortDescription
    {
        public SortDescription(string name, SortDirection direction)
        {
            PropertyName = name;
            Direction = direction;
        }

        public SortDirection Direction { get; set; }
        public string PropertyName { get; set; }
    }
}
