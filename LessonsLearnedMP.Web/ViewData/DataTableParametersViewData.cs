using Suncor.LessonsLearnedMP.Framework;
using Suncor.LessonsLearnedMP.Web.Helpers;

namespace Suncor.LessonsLearnedMP.Web.ViewData
{
    public class DataTableParametersViewData
    {
        /// <summary>
        /// Request sequence number sent by DataTable,
        /// same value must be returned in response
        /// </summary>       
        public string sEcho { get; set; }

        /// <summary>
        /// Text used for filtering
        /// </summary>
        public string sSearch { get; set; }

        /// <summary>
        /// Number of records that should be shown in table
        /// </summary>
        public int iDisplayLength { get; set; }

        /// <summary>
        /// First record that should be shown(used for paging)
        /// </summary>
        public int iDisplayStart { get; set; }

        /// <summary>
        /// Number of columns in table
        /// </summary>
        public int iColumns { get; set; }

        /// <summary>
        /// Number of columns that are used in sorting
        /// </summary>
        public int iSortingCols { get; set; }

        /// <summary>
        /// Comma separated list of column names
        /// </summary>
        public string sColumns { get; set; }

        public int iSortCol_0 { get; set; }
        public int iSortCol_1 { get; set; }
        public int iSortCol_2 { get; set; }
        public int iSortCol_3 { get; set; }
        public int iSortCol_4 { get; set; }
        public int iSortCol_5 { get; set; }
        public string sSortDir_0 { get; set; }
        public string sSortDir_1 { get; set; }
        public string sSortDir_2 { get; set; }
        public string sSortDir_3 { get; set; }
        public string sSortDir_4 { get; set; }
        public string sSortDir_5 { get; set; }

        public string SearchModelJson { get; set; }

        public SearchViewModel SearchModel
        {
            get
            {
                if (string.IsNullOrEmpty(SearchModelJson))
                {
                    return new SearchViewModel();
                }

                return HtmlHelpers.JsonDeserialize<SearchViewModel>(SearchModelJson);
            }
            set
            {
                SearchModelJson = value.ToJson();
            }
        }

        public Enumerations.NavigationPage NavigationPage { get; set; }
    }
}