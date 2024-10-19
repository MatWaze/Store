namespace Store.Models.ViewModels
{
    public class ProfileViewModel
    {
        public BasicInfoViewModel? BasicInfo { get; set; }
        public AddressViewModel? Address { get; set; }
        public List<Order>? Orders { get; set; }
    }
}
