namespace Store.Models.ViewModels
{
    public class ProfileViewModel
    {
        public ApplicationUser? User { get; set; }
        public List<Order>? Orders { get; set; }
    }
}
