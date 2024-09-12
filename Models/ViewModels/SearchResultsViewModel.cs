using Newtonsoft.Json.Linq;

namespace Store.Models.ViewModels
{
    public class SearchResultsViewModel
    {
        public QueryProduct QueryProduct { get; set; }
        public IEnumerable<JToken> Items { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

}
