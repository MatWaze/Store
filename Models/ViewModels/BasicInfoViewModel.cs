using Store.Resources.Models;
using System.ComponentModel.DataAnnotations;

namespace Store.Models.ViewModels
{
    public class BasicInfoViewModel
    {
        [Required(ErrorMessageResourceName = "Required",
            ErrorMessageResourceType = typeof(ValidationMessages))]
        public string? UserName { get; set; }
        [Required(ErrorMessageResourceName = "Required",
            ErrorMessageResourceType = typeof(ValidationMessages))]
        public string? FullName { get; set; }
        [Required(ErrorMessageResourceName = "Required",
            ErrorMessageResourceType = typeof(ValidationMessages))]
        public string? Email { get; set; }
    }
}
