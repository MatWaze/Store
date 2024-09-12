namespace Store.Models.ViewModels
{
    public class PagingInfo
    {
        public int ItemsCount { get; set; }
        public int ItemsPerPage { get; set; }
        public int CurrentPage { get; set; }

        public int TotalPages =>
            (int)Math.Ceiling((decimal) ItemsCount / ItemsPerPage);
        public long CurrentCategory { get; set; }
    }
}
