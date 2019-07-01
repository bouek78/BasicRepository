using Basic.Generic.Enum.Pager;
using Basic.Generic.Interface.Pager;
using System;

namespace Basic.Generic.Common.Pager
{
    public class PagerQuery : IPagerQuery
    {
        private const int DEFAULT_ITEMS_PAGE = 25;
        private const int DEFAULT_CURRENT_INDEX = 0;

        public PagerQuery()
        {
            ItemsPerPage = DEFAULT_ITEMS_PAGE;
            CurrentIndex = DEFAULT_CURRENT_INDEX;
            TotalItemsCount = 0;
            SortColumnName = nameof(ISortable.Name);
            SortDirection = SortDirection.Descending;
            SearchKey = String.Empty;
            DefaultSort = true;
        }

        public PagerQuery(PagerQuery query)
        {
            ItemsPerPage = query.ItemsPerPage;
            CurrentIndex = query.CurrentIndex;
            TotalItemsCount = query.TotalItemsCount;
            SortColumnName = query.SortColumnName;
            SortDirection = query.SortDirection;
            DefaultSort = false;
        }

        public PagerQuery(string sortColumnName) : this()
        {
            SortColumnName = sortColumnName;
        }

        public int ItemsPerPage { get; set; }

        public int TotalItemsCount { get; set; }

        public int CurrentIndex { get; set; }

        public int SkipValue
        {
            get
            {
                int skp = CurrentIndex * ItemsPerPage;
                skp = skp < 0 ? 0 : skp; // EF ne supporte pas les offsets négatifs
                return skp;
            }
        }

        public string SortColumnName { get; set; }

        public SortDirection SortDirection { get; set; }

        public bool HasSortingCondition => !String.IsNullOrEmpty(SortColumnName) && !DefaultSort;

        public bool HasPagingCondition => CurrentIndex > DEFAULT_CURRENT_INDEX && ItemsPerPage > DEFAULT_ITEMS_PAGE;

        // Cherche selon une clé
        public string SearchKey { get; set; }

        // Id (FK) d'une recherche 
        public Guid RelativeId { get; set; }
        public bool DefaultSort { get; protected set; }
    }
}
