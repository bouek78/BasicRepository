using Basic.Generic.Enum.Pager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic.Generic.Interface.Pager
{
    public interface ISorting
    {
        /// <summary>
        /// Nom de la colonne à trié 
        /// </summary>
        string SortColumnName { get; set; }

        /// <summary>
        /// Direction de la colonne trié 
        /// </summary>
        SortDirection SortDirection { get; set; }

        bool HasSortingCondition { get; }

        IEnumerable<SortDescription> SortDescription { get; set; }

        /// <summary>
        /// Est ce le tri par défaut
        /// </summary>
        bool DefaultSort { get; }
    }
}
