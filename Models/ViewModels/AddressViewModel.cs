using Store.Resources.Models;
using System.ComponentModel.DataAnnotations;

namespace Store.Models.ViewModels
{
    public class AddressViewModel
    {
        [Required(ErrorMessageResourceName = "Required",
            ErrorMessageResourceType = typeof(ValidationMessages))]
        public string? Country { get; set; }
        [Required(ErrorMessageResourceName = "Required",
            ErrorMessageResourceType = typeof(ValidationMessages))]
        public string? Region { get; set; }
        [Required(ErrorMessageResourceName = "Required",
            ErrorMessageResourceType = typeof(ValidationMessages))]
        public string? City { get; set; }
        [Required(ErrorMessageResourceName = "Required",
            ErrorMessageResourceType = typeof(ValidationMessages))]
        public string? Street { get; set; }
        [Required(ErrorMessageResourceName = "Required",
            ErrorMessageResourceType = typeof(ValidationMessages))]
        public string? PostalCode { get; set; }
    }
}
