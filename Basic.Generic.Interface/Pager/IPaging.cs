using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic.Generic.Interface.Pager
{
    public interface IPaging
    {
        /// <summary>
        /// Index courant.
        /// </summary>
        int CurrentIndex { get; set; }

        /// <summary>
        /// Nb d'items affiché par page.
        /// Default : 25
        /// </summary>
        int ItemsPerPage { get; set; }

        /// <summary>
        /// Nb total d'items de la liste.
        /// </summary>
        int TotalItemsCount { get; set; }

        /// <summary>
        /// Nombre d'enregistrements avant le début de la sélection.
        /// </summary>
        int SkipValue { get; }

        bool HasPagingCondition { get; }
    }
}
