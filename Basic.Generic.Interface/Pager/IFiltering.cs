﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic.Generic.Interface.Pager
{
    public interface IFilterable
    {
        // Cherche selon une clé
        string SearchKey { get; set; }
    }
}
